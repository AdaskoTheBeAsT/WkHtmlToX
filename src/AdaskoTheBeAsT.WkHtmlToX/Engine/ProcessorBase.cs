using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.EventDefinitions;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine
{
    internal abstract class ProcessorBase
    {
        private readonly WkHtmlToXConfiguration _configuration;

        protected ProcessorBase(WkHtmlToXConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ISettings? ProcessingDocument { get; internal set; }

        protected internal void RegisterEvents(IntPtr converter)
        {
            if (converter == IntPtr.Zero)
            {
                throw new ArgumentException("converter pointer cannot be zero", nameof(converter));
            }

            if (_configuration.PhaseChangedAction != null)
            {
                SetPhaseChangedCallback(converter, OnPhaseChanged);
            }

            if (_configuration.ProgressChangedAction != null)
            {
                SetProgressChangedCallback(converter, OnProgressChanged);
            }

            if (_configuration.FinishedAction != null)
            {
                SetFinishedCallback(converter, OnFinished);
            }

            if (_configuration.WarningAction != null)
            {
                SetWarningCallback(converter, OnWarning);
            }

            if (_configuration.ErrorAction != null)
            {
                SetErrorCallback(converter, OnError);
            }
        }

        protected internal void OnPhaseChanged(IntPtr converter)
        {
            if (_configuration.PhaseChangedAction == null)
            {
                return;
            }

            var phaseCount = GetPhaseCount(converter);
            var currentPhase = GetCurrentPhase(converter);
            var phaseDescription = GetPhaseDescription(converter, currentPhase);

            var eventArgs = new PhaseChangedEventArgs(
                ProcessingDocument,
                phaseCount,
                currentPhase,
                phaseDescription);

            _configuration.PhaseChangedAction?.Invoke(eventArgs);
        }

        protected internal void OnProgressChanged(IntPtr converter)
        {
            if (_configuration.ProgressChangedAction == null)
            {
                return;
            }

            var progress = GetProgressDescription(converter);
            var eventArgs = new ProgressChangedEventArgs(
                ProcessingDocument,
                progress);

            _configuration.ProgressChangedAction?.Invoke(eventArgs);
        }

#pragma warning disable CC0057 // Unused parameters
        protected internal void OnFinished(IntPtr converter, int success)
#pragma warning restore CC0057 // Unused parameters
        {
            if (_configuration.FinishedAction == null)
            {
                return;
            }

            var eventArgs = new FinishedEventArgs(
                ProcessingDocument,
                success == 1);

            _configuration.FinishedAction?.Invoke(eventArgs);
        }

#pragma warning disable CC0057 // Unused parameters
        protected internal void OnError(IntPtr converter, string message)
#pragma warning restore CC0057 // Unused parameters
        {
            if (_configuration.ErrorAction == null)
            {
                return;
            }

            var eventArgs = new ErrorEventArgs(
                ProcessingDocument,
                message);

            _configuration.ErrorAction?.Invoke(eventArgs);
        }

#pragma warning disable CC0057 // Unused parameters
        protected internal void OnWarning(IntPtr converter, string message)
#pragma warning restore CC0057 // Unused parameters
        {
            if (_configuration.WarningAction == null)
            {
                return;
            }

            var eventArgs = new WarningEventArgs(
                ProcessingDocument,
                message);

            _configuration.WarningAction?.Invoke(eventArgs);
        }

        protected internal void ApplyConfig(IntPtr config, ISettings? settings, bool useGlobal, string? prefix = null)
        {
            if (settings is null)
            {
                return;
            }

#pragma warning disable S3011 // Make sure that this accessibility bypass is safe here.
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
#pragma warning restore S3011 // Make sure that this accessibility bypass is safe here.

            foreach (var prop in settings.GetType().GetProperties(bindingFlags))
            {
                var propValue = prop.GetValue(settings);
                if (propValue == null)
                {
                    continue;
                }

                var wkHtmlAttribute = prop.GetCustomAttribute<WkHtmlAttribute>();
                if (wkHtmlAttribute != null
                    && propValue is ISettings propSettings)
                {
                    ApplyConfig(config, propSettings, useGlobal, wkHtmlAttribute.Name);
                }
                else if (wkHtmlAttribute != null)
                {
                    Apply(config, prefix, wkHtmlAttribute.Name, propValue, useGlobal);
                }
                else if (propValue is ISettings propSettings2)
                {
                    ApplyConfig(config, propSettings2, useGlobal);
                }
            }
        }

        protected internal void Apply(IntPtr config, string? prefix, string name, object value, bool useGlobal)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var type = value.GetType();

            var applySetting = GetApplySettingFunc(useGlobal);
            var localName = string.IsNullOrEmpty(prefix) ? name : $"{prefix}.{name}";

            if (typeof(bool) == type)
            {
                applySetting(config, localName, (bool)value ? "true" : "false");
            }
            else if (typeof(double) == type)
            {
                applySetting(config, localName, ((double)value).ToString("0.##", CultureInfo.InvariantCulture));
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
                    applySetting(config, $"{localName}.append", arg3: null);
                    applySetting(config, $"{localName}[{index.ToString(CultureInfo.InvariantCulture)}]", $"{pair.Key}\n{pair.Value}");

                    index++;
                }
            }
            else
            {
                applySetting(config, localName, value.ToString());
            }
        }

        protected internal abstract Func<IntPtr, string, string?, int> GetApplySettingFunc(bool useGlobal);

        protected internal abstract int GetCurrentPhase(IntPtr converter);

        protected internal abstract int GetPhaseCount(IntPtr converter);

        protected internal abstract string GetPhaseDescription(
            IntPtr converter,
            int phase);

        protected internal abstract string GetProgressDescription(
            IntPtr converter);

        protected internal abstract int SetWarningCallback(
            IntPtr converter,
            StringCallback callback);

        protected internal abstract int SetErrorCallback(
            IntPtr converter,
            StringCallback callback);

        protected internal abstract int SetPhaseChangedCallback(
            IntPtr converter,
            VoidCallback callback);

        protected internal abstract int SetProgressChangedCallback(
            IntPtr converter,
            VoidCallback callback);

        protected internal abstract int SetFinishedCallback(
            IntPtr converter,
            IntCallback callback);
    }
}
