using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.Utils
{
    public class BufferManager
        : IBufferManager
    {
        public void CopyBuffer(
            IntPtr data,
            Stream stream,
            int length)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

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
        }

        public Task CopyBufferAsync(
            IntPtr data,
            Stream stream,
            int length,
            CancellationToken token)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return CopyBufferInternalAsync(data, stream, length, token);
        }

        private async Task CopyBufferInternalAsync(
            IntPtr data,
            Stream stream,
            int length,
            CancellationToken token)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                Marshal.Copy(data, buffer, 0, length);
                await stream.WriteAsync(buffer, 0, length, token);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
