using System;
using System.IO;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.EventDefinitions;

namespace AdaskoTheBeAsT.WkHtmlToX.WorkItems
{
    public abstract class ConvertWorkItemBase : IWorkItemVisitable
    {
        protected ConvertWorkItemBase(Func<int, Stream> streamFunc)
        {
            StreamFunc = streamFunc ?? throw new ArgumentNullException(nameof(streamFunc));
            TaskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public Action<PhaseChangedEventArgs>? PhaseChanged { get; set; }

        public TaskCompletionSource<bool> TaskCompletionSource { get; }

        public Func<int, Stream> StreamFunc { get; }

        public abstract void Accept(IWorkItemVisitor visitor);
    }
}
