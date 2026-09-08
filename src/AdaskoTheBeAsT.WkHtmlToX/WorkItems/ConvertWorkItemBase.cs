using System;
using System.IO;
using System.Threading.Tasks;

namespace AdaskoTheBeAsT.WkHtmlToX.WorkItems;

#pragma warning disable S1133 // Planned removal in the next major release; retained for migration.
[Obsolete("Use IWkHtmlToXAsyncEngine task-returning conversion methods. Work items will be removed in the next major release.")]
#pragma warning restore S1133
public abstract class ConvertWorkItemBase : IWorkItemVisitable
{
    protected ConvertWorkItemBase(Func<int, Stream> streamFunc)
    {
        StreamFunc = streamFunc ?? throw new ArgumentNullException(nameof(streamFunc));
        TaskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public TaskCompletionSource<bool> TaskCompletionSource { get; }

    public Func<int, Stream> StreamFunc { get; }

    public abstract void Accept(IWorkItemVisitor visitor);
}
