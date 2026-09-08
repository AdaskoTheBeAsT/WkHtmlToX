using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Settings;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

internal static class RequestSnapshot
{
    internal static HtmlToPdfDocument Create(IHtmlToPdfDocument document, int maxInputBytes, Action<long> reserveInput)
    {
        if (document?.GlobalSettings?.Margins is null || document.ObjectSettings is null || document.ObjectSettings.Count == 0)
        {
            throw new ArgumentException("A PDF request requires global settings and at least one object.", nameof(document));
        }

        ValidateOutput(document.GlobalSettings.Out);
        var snapshot = new HtmlToPdfDocument { GlobalSettings = Copy(document.GlobalSettings) };
        long inputBytes = 0;
        foreach (var item in document.ObjectSettings)
        {
            // Capture metadata and buffer references first; do not clone payloads before reservation.
            var copy = Copy(item);
            var inputLength = ValidateInput(copy);
            if (inputLength > maxInputBytes - inputBytes)
            {
                throw new ArgumentException("The PDF request exceeds MaxInputBytes.", nameof(document));
            }

            inputBytes += inputLength;
            snapshot.ObjectSettings.Add(copy);
        }

        reserveInput.Invoke(inputBytes);
        foreach (var item in snapshot.ObjectSettings)
        {
            item.HtmlContentByteArray = (byte[]?)item.HtmlContentByteArray?.Clone();
        }

        return snapshot;
    }

    internal static HtmlToImageDocument Create(IHtmlToImageDocument document)
    {
        if (document?.ImageSettings is null || string.IsNullOrWhiteSpace(document.ImageSettings.In))
        {
            throw new ArgumentException("An image request requires an input URL or file path.", nameof(document));
        }

        ValidateOutput(document.ImageSettings.Out);
        if (string.Equals(document.ImageSettings.In, "-", StringComparison.Ordinal))
        {
            throw new ArgumentException("Standard input is not supported.", nameof(document));
        }

        return new HtmlToImageDocument { ImageSettings = Copy(document.ImageSettings) };
    }

    internal static async Task ReadStreamsAsync(
        HtmlToPdfDocument document,
        long reservedInputBytes,
        Action checkPending,
        CancellationToken cancellationToken)
    {
        // All byte-array snapshots already exist, including those after the next stream.
        // Do not let stream growth consume their capacity before a later loop iteration.
        var available = GetStreamCapacity(document, reservedInputBytes);
        foreach (var item in document.ObjectSettings)
        {
            checkPending.Invoke();
            var stream = item.HtmlContentStream;
            if (stream is null)
            {
                continue;
            }

            var inputLength = ValidateInput(item);
            if (inputLength > available)
            {
                throw new ArgumentException("The PDF input grew beyond its admission reservation.");
            }

            available -= inputLength;

            // The stream remains borrowed until the public conversion task finishes.
            var length = checked((int)inputLength);
            var buffer = new byte[length];
            var offset = 0;
            while (offset < length)
            {
                cancellationToken.ThrowIfCancellationRequested();
                checkPending.Invoke();
                var read = await stream.ReadAsync(buffer, offset, length - offset, cancellationToken).ConfigureAwait(false);
                if (read == 0)
                {
                    throw new EndOfStreamException("The HTML input stream ended before its declared length.");
                }

                offset += read;
            }

            item.HtmlContentStream = null;
            item.HtmlContentByteArray = buffer;
        }
    }

    private static long GetStreamCapacity(HtmlToPdfDocument document, long reservedInputBytes)
    {
        foreach (var item in document.ObjectSettings)
        {
            if (item.HtmlContentStream is not null)
            {
                continue;
            }

            var length = ValidateInput(item);
            if (length > reservedInputBytes)
            {
                throw new ArgumentException("The PDF input grew beyond its admission reservation.");
            }

            reservedInputBytes -= length;
        }

        return reservedInputBytes;
    }

    private static long ValidateInput(PdfObjectSettings? item)
    {
        if (item is null)
        {
            throw new ArgumentException("PDF objects cannot be null.");
        }

        var forms = (string.IsNullOrEmpty(item.HtmlContent) ? 0 : 1)
            + (item.HtmlContentByteArray is null ? 0 : 1)
            + (item.HtmlContentStream is null ? 0 : 1)
            + (string.IsNullOrEmpty(item.Page) ? 0 : 1)
            + (string.IsNullOrEmpty(item.Xsl) ? 0 : 1);
        if (forms != 1)
        {
            throw new ArgumentException("Specify exactly one PDF input: HTML, bytes, stream, Page, or Xsl.");
        }

        if (string.Equals(item.Page, "-", StringComparison.Ordinal))
        {
            throw new ArgumentException("Standard input is not supported.");
        }

        if (item.HtmlContent is not null && item.HtmlContent.Length > 0)
        {
            return (item.Encoding ?? Encoding.UTF8).GetByteCount(item.HtmlContent);
        }

        if (item.HtmlContentByteArray is not null)
        {
            return item.HtmlContentByteArray.Length;
        }

        var stream = item.HtmlContentStream;
        if (stream is null)
        {
            return 0;
        }

        if (!stream.CanRead || !stream.CanSeek)
        {
            throw new ArgumentException("HTML input streams must be readable, seekable, and positioned within their length.");
        }

        var position = stream.Position;
        var length = stream.Length;
        if (position < 0 || position > length)
        {
            throw new ArgumentException("HTML input streams must be positioned within their length.");
        }

        return length - position;
    }

    private static void ValidateOutput(string? path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Native file/stdout output is not supported. Use the destination stream factory.");
        }
    }

    private static T Copy<T>(T source)
        where T : class => (T)CopyValue(source)!;

    private static object? CopyValue(object? value)
    {
        if (value is null || value is string || value is Stream || value is byte[])
        {
            return value;
        }

        var type = value.GetType();
        if (type.IsValueType)
        {
            if ((type.IsEnum && !Enum.IsDefined(type, value))
                || (value is double number && (double.IsNaN(number) || double.IsInfinity(number))))
            {
                throw new ArgumentException("A setting contains an unsupported numeric value.");
            }

            return value;
        }

        if (value is Encoding encoding)
        {
            return encoding.Clone();
        }

        if (value is Dictionary<string, string> dictionary)
        {
            return new Dictionary<string, string>(dictionary, dictionary.Comparer);
        }

        if (value is PechkinPaperSize paperSize && type == typeof(PechkinPaperSize))
        {
            return new PechkinPaperSize(paperSize.Width, paperSize.Height);
        }

        var copy = CreateSettings(type);
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.CanRead && property.CanWrite)
            {
                property.SetValue(copy, CopyValue(property.GetValue(value)));
            }
        }

        return copy;
    }

    private static object CreateSettings(Type type) => type switch
    {
        _ when type == typeof(PdfGlobalSettings) => new PdfGlobalSettings(),
        _ when type == typeof(PdfObjectSettings) => new PdfObjectSettings(),
        _ when type == typeof(ImageSettings) => new ImageSettings(),
        _ when type == typeof(WebSettings) => new WebSettings(),
        _ when type == typeof(LoadSettings) => new LoadSettings(),
        _ when type == typeof(SectionSettings) => new SectionSettings(),
        _ when type == typeof(MarginSettings) => new MarginSettings(),
        _ => throw new ArgumentException("Only built-in settings types can be snapshotted."),
    };
}
