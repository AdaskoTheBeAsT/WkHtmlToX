using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Engine;

internal sealed class RequestTestStream(bool gateReads = false, bool gateWrites = false) : MemoryStream(new byte[4])
{
    internal TaskCompletionSource<bool> Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal TaskCompletionSource<bool> Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal bool FailRead { get; set; }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (gateReads)
        {
            await WaitForReleaseAsync();
        }

        if (FailRead)
        {
            throw new IOException("Test read failure.");
        }

        return await base.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (gateWrites)
        {
            await WaitForReleaseAsync();
        }

        await base.WriteAsync(buffer, offset, count, cancellationToken);
    }

    private Task WaitForReleaseAsync()
    {
        Entered.TrySetResult(true);
#pragma warning disable VSTHRD003 // The test releases the simulated I/O operation explicitly.
        return Release.Task;
#pragma warning restore VSTHRD003
    }
}
