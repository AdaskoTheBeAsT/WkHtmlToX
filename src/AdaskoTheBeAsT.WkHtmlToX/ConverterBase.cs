using System;
using System.Diagnostics.CodeAnalysis;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.EventDefinitions;
using AdaskoTheBeAsT.WkHtmlToX.Modules;

namespace AdaskoTheBeAsT.WkHtmlToX
{
    public abstract class ConverterBase
        : IDisposable
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable SA1401 // Fields should be private
        internal IWkHtmlToXModule _module;
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore CA1051 // Do not declare visible instance fields

#pragma warning disable S3442 // "abstract" classes should not have "public" constructors
        internal ConverterBase(
            IWkHtmlToXModuleFactory moduleFactory,
            ModuleKind moduleKind)
        {
            if (moduleFactory is null)
            {
                throw new ArgumentNullException(nameof(moduleFactory));
            }

            _module = moduleFactory.GetModule(moduleKind);
        }
#pragma warning restore S3442 // "abstract" classes should not have "public" constructors

        [ExcludeFromCodeCoverage]
        protected ConverterBase(ModuleKind moduleKind)
            : this(new WkHtmlToXModuleFactory(), moduleKind)
        {
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

        internal void RegisterEvents(IntPtr converter)
        {
            if (converter == IntPtr.Zero)
            {
                throw new ArgumentException("converter pointer cannot be zero", nameof(converter));
            }

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

        internal void OnPhaseChanged(IntPtr converter)
        {
            if (PhaseChanged == null)
            {
                return;
            }

            var phaseCount = _module.GetPhaseCount(converter);
            var currentPhase = _module.GetCurrentPhase(converter);
            var phaseDescription = _module.GetPhaseDescription(converter, currentPhase);

            var eventArgs = new PhaseChangedEventArgs(
                ProcessingDocument,
                phaseCount,
                currentPhase,
                phaseDescription);

            PhaseChanged.Invoke(this, eventArgs);
        }

        internal void OnProgressChanged(IntPtr converter)
        {
            if (ProgressChanged == null)
            {
                return;
            }

            var progress = _module.GetProgressString(converter);
            var eventArgs = new ProgressChangedEventArgs(
                ProcessingDocument,
                progress);

            ProgressChanged?.Invoke(this, eventArgs);
        }

        internal void OnFinished(IntPtr converter, int success)
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

        internal void OnError(IntPtr converter, string message)
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

        internal void OnWarning(IntPtr converter, string message)
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

        protected virtual void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                _module?.Dispose();
            }
        }
    }
}
