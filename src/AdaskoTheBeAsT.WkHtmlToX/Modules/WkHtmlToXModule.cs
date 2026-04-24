using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules;

[ExcludeFromCodeCoverage]
internal abstract class WkHtmlToXModule
    : IWkHtmlToXModule
{
    protected const int MaxBufferSize = 2048;
    private const int MaxCopyBufferSize = 81920;

    public abstract int Initialize(
        int useGraphics);

    public abstract int Terminate();

    public abstract int ExtendedQt();

    public string GetLibraryVersion()
    {
        var ptr = GetLibraryVersionImpl();
        return Utf8Interop.PtrToString(ptr);
    }

    public abstract IntPtr CreateGlobalSettings();

    public abstract void DestroyGlobalSetting(
        IntPtr settings);

    public abstract int SetGlobalSetting(
        IntPtr settings,
        string name,
        string? value);

    public string GetGlobalSetting(
        IntPtr settings,
        string name)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(MaxBufferSize);
        try
        {
            var retVal = GetGlobalSettingImpl(
                settings,
                name,
                buffer);

            if (retVal != 1)
            {
                throw new GetGlobalSettingsFailedException($"GetGlobalSettings failed for obtaining setting={name}");
            }

            var nullPos = Array.IndexOf(buffer, byte.MinValue);
            if (nullPos < 0)
            {
                throw new GetGlobalSettingsFailedException(
                    $"GetGlobalSettings failed for obtaining setting={name}. Returned value was not null-terminated.");
            }

            return Encoding.UTF8.GetString(buffer, 0, nullPos);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public abstract IntPtr CreateConverter(
        IntPtr globalSettings);

    public abstract void DestroyConverter(
        IntPtr converter);

    public abstract void SetWarningCallback(
        IntPtr converter,
        StringCallback callback);

    public abstract void SetErrorCallback(
        IntPtr converter,
        StringCallback callback);

    public abstract void SetPhaseChangedCallback(
        IntPtr converter,
        VoidCallback callback);

    public abstract void SetProgressChangedCallback(
        IntPtr converter,
        IntCallback callback);

    public abstract void SetFinishedCallback(
        IntPtr converter,
        IntCallback callback);

    public abstract bool Convert(
        IntPtr converter);

    public abstract int GetCurrentPhase(
        IntPtr converter);

    public string GetPhaseDescription(
        IntPtr converter,
        int phase)
    {
        var ptr = GetPhaseDescriptionImpl(converter, phase);
        return Utf8Interop.PtrToString(ptr);
    }

    public string GetProgressDescription(
        IntPtr converter)
    {
        var ptr = GetProgressStringImpl(converter);
        return Utf8Interop.PtrToString(ptr);
    }

    public abstract int GetPhaseCount(
        IntPtr converter);

    public abstract int GetHttpErrorCode(
        IntPtr converter);

    public void GetOutput(IntPtr converter, Stream stream)
    {
#if !NET8_0_OR_GREATER
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }
#endif
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(stream);
#endif
        GetOutput(converter, _ => stream);
    }

    public void GetOutput(
        IntPtr converter,
        Func<int, Stream> createStreamFunc)
    {
#if !NET8_0_OR_GREATER
        if (createStreamFunc is null)
        {
            throw new ArgumentNullException(nameof(createStreamFunc));
        }
#endif
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(createStreamFunc);
#endif
        var totalLength = GetOutputImpl(converter, out IntPtr data);
        if (totalLength == 0)
        {
            return;
        }

#pragma warning disable IDISP001 // Dispose created.
#pragma warning disable CC0031 // Check for null before calling a delegate
        var stream = createStreamFunc(totalLength) ?? throw new ArgumentException("Create stream returned null");
#pragma warning restore CC0031 // Check for null before calling a delegate
#pragma warning restore IDISP001 // Dispose created.

        (totalLength, var length) = CopyBuffer(data, stream, totalLength);

        while (totalLength > 0)
        {
            data = IntPtr.Add(data, length);
            (totalLength, length) = CopyBuffer(data, stream, totalLength);
        }

        stream.Flush();
    }

    protected abstract int GetGlobalSettingImpl(
        IntPtr settings,
        string name,
        byte[] buffer);

    protected abstract int GetOutputImpl(
        IntPtr converter,
        out IntPtr data);

    protected abstract IntPtr GetLibraryVersionImpl();

    protected abstract IntPtr GetPhaseDescriptionImpl(
        IntPtr converter,
        int phase);

    protected abstract IntPtr GetProgressStringImpl(
        IntPtr converter);

    private static (int totalLength, int length) CopyBuffer(
        IntPtr data,
        Stream stream,
        int totalLength)
    {
        var length = Math.Min(totalLength, MaxCopyBufferSize);
        var buffer = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            Marshal.Copy(data, buffer, 0, length);
            stream.Write(buffer, 0, length);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        totalLength -= length;
        return (totalLength, length);
    }
}
