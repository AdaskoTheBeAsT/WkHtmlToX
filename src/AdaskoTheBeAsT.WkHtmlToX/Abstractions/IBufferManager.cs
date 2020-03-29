using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    public interface IBufferManager
    {
        void CopyBuffer(
            IntPtr data,
            Stream stream,
            int length);

        Task CopyBufferAsync(
            IntPtr data,
            Stream stream,
            int length,
            CancellationToken token);
    }
}
