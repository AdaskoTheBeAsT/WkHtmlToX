using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.EventDefinitions;
using AdaskoTheBeAsT.WkHtmlToX.Modules;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

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

        protected internal void RegisterEvents(IntPtr converter)
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

        protected internal void OnPhaseChanged(IntPtr converter)
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

        protected internal void OnProgressChanged(IntPtr converter)
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

        protected internal void OnFinished(IntPtr converter, int success)
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

        protected internal void OnError(IntPtr converter, string message)
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

        protected internal void OnWarning(IntPtr converter, string message)
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

        protected internal void ApplyConfig(IntPtr config, ISettings settings, bool isGlobal)
        {
            if (settings == null)
            {
                return;
            }

            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var prop in settings.GetType().GetProperties(bindingFlags))
            {
                var propValue = prop.GetValue(settings);
                if (propValue == null)
                {
                    continue;
                }

                var wkHtmlAttribute = prop.GetCustomAttributes().OfType<WkHtmlAttribute>().FirstOrDefault();

                if (wkHtmlAttribute != null)
                {
                    Apply(config, wkHtmlAttribute.Name, propValue, isGlobal);
                }
                else if (propValue is ISettings propSettings)
                {
                    ApplyConfig(config, propSettings, isGlobal);
                }
            }
        }

        protected internal void Apply(IntPtr config, string name, object value, bool isGlobal)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var type = value.GetType();

            var applySetting = GetApplySettingFunc(isGlobal);

            if (typeof(bool) == type)
            {
                applySetting(config, name, (bool)value ? "true" : "false");
            }
            else if (typeof(double) == type)
            {
                applySetting(config, name, ((double)value).ToString("0.##", CultureInfo.InvariantCulture));
            }
            else if (typeof(Dictionary<string, string>).IsAssignableFrom(type))
            {
                var dictionary = (Dictionary<string, string>)value;
                var index = 0;

                foreach (var pair in dictionary)
                {
                    if (pair.Key == null || pair.Value == null)
                    {
                        continue;
                    }

                    // https://github.com/wkhtmltopdf/wkhtmltopdf/blob/c754e38b074a75a51327df36c4a53f8962020510/src/lib/reflect.hh#L192
                    applySetting(config, $"{name}.append", null);
                    applySetting(config, $"{name}[{index}]", $"{pair.Key}\n{pair.Value}");

                    index++;
                }
            }
            else
            {
                applySetting(config, name, value.ToString());
            }
        }

        protected internal virtual Func<IntPtr, string, string?, int> GetApplySettingFunc(bool isGlobal)
        {
            return _module.SetGlobalSetting;
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
