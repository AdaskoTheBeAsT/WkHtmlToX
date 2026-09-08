using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

public sealed partial class WkHtmlToXEngine
{
    private readonly WkHtmlToXRequestOptions _requestOptions;
#if NET9_0_OR_GREATER
    private readonly Lock _admissionLock = new();
#else
    private readonly object _admissionLock = new();
#endif
    private readonly TaskCompletionSource<bool> _requestsExited = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _activeRequests;
    private bool _stopped;
    private long _bufferedInputBytes;
    private long _bufferedOutputBytes;

    private static async Task<ConversionResult> DeliverOutputAsync(
        byte[] output,
        Func<int, Stream> destination,
        ConversionResult result,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
#pragma warning disable IDISP001 // The destination is borrowed; ownership stays with the caller.
            var stream = destination.Invoke(output.Length);
#pragma warning restore IDISP001
            if (stream is null || !stream.CanWrite)
            {
                throw new ArgumentException("The destination factory must return a writable stream.");
            }

            await stream.WriteAsync(output, 0, output.Length, cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return new ConversionResult(
                ConversionFailureKind.OutputWriteError,
                result.HttpErrorCode,
                exception,
                result.CallbackException,
                result.Warnings);
        }
    }

    private Task<ConversionResult> AdmitAsync<T>(
        Func<Action<long>, T> snapshot,
        Func<T, Func<int, Stream>, CancellationToken, Task<ConversionResult>> convert,
        Func<int, Stream> destination,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<ConversionResult>(cancellationToken);
        }

        if (_worker is WkHtmlToXWorker nativeWorker)
        {
            nativeWorker.ThrowIfReentrant();
        }

        lock (_admissionLock)
        {
            if (_stopped)
            {
                return Task.FromException<ConversionResult>(new ObjectDisposedException(nameof(WkHtmlToXEngine)));
            }

            if (_activeRequests >= _requestOptions.MaxConcurrentRequests)
            {
                return Task.FromResult(new ConversionResult(ConversionFailureKind.Overloaded));
            }

            _activeRequests++;
        }

        long reservedInputBytes = 0;
        try
        {
            if (destination is null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            // Capture before returning to the caller, not when the native queue is read.
            var request = snapshot.Invoke(bytes => reservedInputBytes = ReserveInput(bytes));
            return RunRequestAsync(request, reservedInputBytes, convert, destination, cancellationToken);
        }
        catch (InputLimitException)
        {
            ReleaseRequest(reservedInputBytes);
            return Task.FromResult(new ConversionResult(ConversionFailureKind.ResourceLimit));
        }
        catch (ArgumentException exception)
        {
            ReleaseRequest(reservedInputBytes);
            return Task.FromResult(new ConversionResult(ConversionFailureKind.InvalidInput, exception: exception));
        }
        catch
        {
            ReleaseRequest(reservedInputBytes);
            throw;
        }
    }

    private async Task<ConversionResult> RunRequestAsync<T>(
        T request,
        long reservedInputBytes,
        Func<T, Func<int, Stream>, CancellationToken, Task<ConversionResult>> convert,
        Func<int, Stream> destination,
        CancellationToken cancellationToken)
    {
        try
        {
            // Even synchronous stream implementations and factories run off the native thread.
            return await Task.Run(
                () => ProcessRequestAsync(request, reservedInputBytes, convert, destination, cancellationToken),
                CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            ReleaseRequest(reservedInputBytes);
        }
    }

    private async Task<ConversionResult> ProcessRequestAsync<T>(
        T request,
        long reservedInputBytes,
        Func<T, Func<int, Stream>, CancellationToken, Task<ConversionResult>> convert,
        Func<int, Stream> destination,
        CancellationToken cancellationToken)
    {
        ThrowIfPendingCanceled();
        if (request is HtmlToPdfDocument pdf)
        {
            try
            {
                await RequestSnapshot.ReadStreamsAsync(pdf, reservedInputBytes, ThrowIfPendingCanceled, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested || IsPendingCanceled())
            {
                throw;
            }
            catch (Exception exception)
            {
                return new ConversionResult(ConversionFailureKind.InputReadError, exception: exception);
            }
        }

        ThrowIfPendingCanceled();
        using var buffer = new BufferedOutput(this);
        var result = await convert.Invoke(request, buffer.Create, cancellationToken).ConfigureAwait(false);
        if (result.Exception is OutputLimitException)
        {
            return new ConversionResult(ConversionFailureKind.ResourceLimit, result.HttpErrorCode, warnings: result.Warnings);
        }

        if (buffer.Bytes is null || (result.FailureKind != ConversionFailureKind.None && result.FailureKind != ConversionFailureKind.CallbackError))
        {
            return result;
        }

        return await DeliverOutputAsync(buffer.Bytes, destination, result, cancellationToken).ConfigureAwait(false);
    }

    private long ReserveInput(long bytes)
    {
        lock (_admissionLock)
        {
            if (bytes > _requestOptions.MaxBufferedInputBytes - _bufferedInputBytes)
            {
                throw new InputLimitException();
            }

            _bufferedInputBytes += bytes;
            return bytes;
        }
    }

    private void ReleaseRequest(long reservedInputBytes)
    {
        lock (_admissionLock)
        {
            _activeRequests--;
            _bufferedInputBytes -= reservedInputBytes;
            if (_stopped && _activeRequests == 0)
            {
                _requestsExited.TrySetResult(true);
            }
        }
    }

#pragma warning disable S3871 // Private signal translated to a public structured resource-limit result.
    private sealed class InputLimitException() : Exception("The managed input budget was exceeded.");

    private sealed class OutputLimitException() : Exception("The managed output budget was exceeded.");
#pragma warning restore S3871

    private sealed class BufferedOutput(WkHtmlToXEngine engine) : IDisposable
    {
        private MemoryStream? _stream;
        private int _reserved;

        internal byte[]? Bytes { get; private set; }

        public void Dispose()
        {
            lock (engine._admissionLock)
            {
                _stream?.Dispose();
                _stream = null;
                Bytes = null;
                engine._bufferedOutputBytes -= _reserved;
                _reserved = 0;
            }
        }

        internal Stream Create(int length)
        {
            if (length < 0 || length > engine._requestOptions.MaxOutputBytes)
            {
                throw new OutputLimitException();
            }

            lock (engine._admissionLock)
            {
                if (_stream is not null || length > engine._requestOptions.MaxBufferedOutputBytes - engine._bufferedOutputBytes)
                {
                    throw new OutputLimitException();
                }

                engine._bufferedOutputBytes += length;
                _reserved = length;

                Bytes = new byte[length];
#pragma warning disable IDISP003 // A repeated factory invocation is rejected above.
                _stream = new MemoryStream(Bytes, writable: true);
#pragma warning restore IDISP003
                return _stream;
            }
        }
    }
}
