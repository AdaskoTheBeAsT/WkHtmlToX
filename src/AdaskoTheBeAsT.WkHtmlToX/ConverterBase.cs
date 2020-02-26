using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.EventDefinitions;
using AdaskoTheBeAsT.WkHtmlToX.Modules;
using ErrorEventArgs = AdaskoTheBeAsT.WkHtmlToX.EventDefinitions.ErrorEventArgs;

namespace AdaskoTheBeAsT.WkHtmlToX
{
    public class ConverterBase
        : IDisposable
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable SA1401 // Fields should be private
        internal IWkHtmlToXModule _module;
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore CA1051 // Do not declare visible instance fields

        protected ConverterBase(ModuleKind moduleKind)
        {
            var moduleFactory = new WkHtmlToXModuleFactory();
            _module = moduleFactory.GetModule(moduleKind);
        }

        public event EventHandler<PhaseChangedEventArgs>? PhaseChanged;

        public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;

        public event EventHandler<FinishedEventArgs>? Finished;

        public event EventHandler<ErrorEventArgs>? Error;

        public event EventHandler<WarningEventArgs>? Warning;

        public IDocument? ProcessingDocument { get; internal set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                _module?.Dispose();
            }
        }

        protected void RegisterEvents(IntPtr converter)
        {
            if (PhaseChanged != null)
            {
                _module.SetPhaseChangedCallback(converter, OnPhaseChanged);
            }

            if (ProgressChanged != null)
            {
                _module.SetProgressChangedCallback(converter, OnProgressChanged);
            }

            if (Finished != null)
            {
                _module.SetFinishedCallback(converter, OnFinished);
            }

            if (Warning != null)
            {
                _module.SetWarningCallback(converter, OnWarning);
            }

            if (Error != null)
            {
                _module.SetErrorCallback(converter, OnError);
            }
        }

        private void OnPhaseChanged(IntPtr converter)
        {
            if (PhaseChanged == null)
            {
                return;
            }

            var currentPhase = _module.GetCurrentPhase(converter);

            var eventArgs = new PhaseChangedEventArgs(
                ProcessingDocument,
                _module.GetPhaseCount(converter),
                currentPhase,
                _module.GetPhaseDescription(converter, currentPhase));

            PhaseChanged.Invoke(this, eventArgs);
        }

        private void OnProgressChanged(IntPtr converter)
        {
            if (ProgressChanged == null)
            {
                return;
            }

            var eventArgs = new ProgressChangedEventArgs(
                ProcessingDocument,
                _module.GetProgressString(converter));

            ProgressChanged?.Invoke(this, eventArgs);
        }

        private void OnFinished(IntPtr converter, int success)
        {
            if (Finished == null)
            {
                return;
            }

            var eventArgs = new FinishedEventArgs(
                ProcessingDocument,
                success == 1);

            Finished?.Invoke(this, eventArgs);
        }

        private void OnError(IntPtr converter, string message)
        {
            if (Error == null)
            {
                return;
            }

            var eventArgs = new ErrorEventArgs(
                ProcessingDocument,
                message);

            Error?.Invoke(this, eventArgs);
        }

        private void OnWarning(IntPtr converter, string message)
        {
            if (Warning == null)
            {
                return;
            }

            var eventArgs = new WarningEventArgs(
                ProcessingDocument,
                message);

            Warning?.Invoke(this, eventArgs);
        }
    }
}
