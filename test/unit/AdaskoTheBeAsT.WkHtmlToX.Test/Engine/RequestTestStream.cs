using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Engine;

internal sealed class RequestTestStream(bool gateReads = false, bool gateWrites = false) : MemoryStream(new byte[4])
{
    internal TaskCompletionSource<bool> Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal TaskCompletionSource<bool> Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal bool FailRead { get; set; }

#if NET8_0_OR_GREATER
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
#else
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
#endif
    {
        if (gateReads)
        {
            await WaitForReleaseAsync();
        }

        if (FailRead)
        {
            throw new IOException("Test read failure.");
        }

#if NET8_0_OR_GREATER
        return await base.ReadAsync(buffer, cancellationToken);
#else
        return await base.ReadAsync(buffer, offset, count, cancellationToken);
#endif
    }

#if NET8_0_OR_GREATER
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
#else
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
#endif
    {
        if (gateWrites)
        {
            await WaitForReleaseAsync();
        }

#if NET8_0_OR_GREATER
        await base.WriteAsync(buffer, cancellationToken);
#else
        await base.WriteAsync(buffer, offset, count, cancellationToken);
#endif
    }

    private Task<bool> WaitForReleaseAsync()
    {
        Entered.TrySetResult(true);
#pragma warning disable VSTHRD003 // The test releases the simulated I/O operation explicitly.
        return Release.Task;
#pragma warning restore VSTHRD003
    }
}
