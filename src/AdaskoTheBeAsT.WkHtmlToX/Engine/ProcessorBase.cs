using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.EventDefinitions;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

internal abstract class ProcessorBase(WkHtmlToXConfiguration configuration)
{
    private StringCallback? _warningCallback;
    private StringCallback? _errorCallback;
    private VoidCallback? _phaseChangedCallback;
    private IntCallback? _progressChangedCallback;
    private IntCallback? _finishedCallback;

    public ISettings? ProcessingDocument { get; internal set; }

    protected internal bool HasRegisteredCallbacks =>
        _warningCallback is not null
        || _errorCallback is not null
        || _phaseChangedCallback is not null
        || _progressChangedCallback is not null
        || _finishedCallback is not null;

    protected internal void RegisterEvents(IntPtr converter)
    {
        if (converter == IntPtr.Zero)
        {
            throw new ArgumentException("converter pointer cannot be zero", nameof(converter));
        }

        if (configuration.PhaseChangedAction != null)
        {
            _phaseChangedCallback = OnPhaseChanged;
            SetPhaseChangedCallback(converter, _phaseChangedCallback);
        }

        if (configuration.ProgressChangedAction != null)
        {
            _progressChangedCallback = OnProgressChanged;
            SetProgressChangedCallback(converter, _progressChangedCallback);
        }

        if (configuration.FinishedAction != null)
        {
            _finishedCallback = OnFinished;
            SetFinishedCallback(converter, _finishedCallback);
        }

        if (configuration.WarningAction != null)
        {
            _warningCallback = OnWarning;
            SetWarningCallback(converter, _warningCallback);
        }

        if (configuration.ErrorAction != null)
        {
            _errorCallback = OnError;
            SetErrorCallback(converter, _errorCallback);
        }
    }

    protected internal void ReleaseRegisteredCallbacks()
    {
        _warningCallback = null;
        _errorCallback = null;
        _phaseChangedCallback = null;
        _progressChangedCallback = null;
        _finishedCallback = null;
    }

    protected internal void OnPhaseChanged(IntPtr converter)
    {
        if (configuration.PhaseChangedAction == null)
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

        configuration.PhaseChangedAction?.Invoke(eventArgs);
    }

    protected internal void OnProgressChanged(IntPtr converter, int progress)
    {
        if (configuration.ProgressChangedAction == null)
        {
            return;
        }

        var progressDescription = GetProgressDescription(converter);
        var eventArgs = new ProgressChangedEventArgs(
            ProcessingDocument,
            progress,
            progressDescription);

        configuration.ProgressChangedAction?.Invoke(eventArgs);
    }

#pragma warning disable CC0057 // Unused parameters
    protected internal void OnFinished(IntPtr converter, int success)
#pragma warning restore CC0057 // Unused parameters
    {
        if (configuration.FinishedAction == null)
        {
            return;
        }

        var eventArgs = new FinishedEventArgs(
            ProcessingDocument,
            success == 1);

        configuration.FinishedAction?.Invoke(eventArgs);
    }

#pragma warning disable CC0057 // Unused parameters
    protected internal void OnError(IntPtr converter, IntPtr messagePointer)
#pragma warning restore CC0057 // Unused parameters
    {
        if (configuration.ErrorAction == null)
        {
            return;
        }

        var message = Utf8Interop.PtrToString(messagePointer);

        var eventArgs = new ErrorEventArgs(
            ProcessingDocument,
            message);

        configuration.ErrorAction?.Invoke(eventArgs);
    }

#pragma warning disable CC0057 // Unused parameters
    protected internal void OnWarning(IntPtr converter, IntPtr messagePointer)
#pragma warning restore CC0057 // Unused parameters
    {
        if (configuration.WarningAction == null)
        {
            return;
        }

        var message = Utf8Interop.PtrToString(messagePointer);

        var eventArgs = new WarningEventArgs(
            ProcessingDocument,
            message);

        configuration.WarningAction?.Invoke(eventArgs);
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

            var attribute = prop.GetCustomAttribute<WkHtmlAttribute>();
            if (attribute != null
                && propValue is ISettings propSettings)
            {
                ApplyConfig(config, propSettings, useGlobal, attribute.Name);
            }
            else if (attribute != null)
            {
                Apply(config, prefix, attribute.Name, propValue, useGlobal);
            }
            else if (propValue is ISettings propSettings2)
            {
                ApplyConfig(config, propSettings2, useGlobal);
            }
        }
    }

    protected internal void Apply(IntPtr config, string? prefix, string name, object value, bool useGlobal)
    {
#if !NET8_0_OR_GREATER
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }
#endif
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(value);
#endif
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
#pragma warning disable REFL040
        else if (typeof(Dictionary<string, string>).IsAssignableFrom(type))
#pragma warning restore REFL040
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

    protected internal abstract void SetWarningCallback(
        IntPtr converter,
        StringCallback callback);

    protected internal abstract void SetErrorCallback(
        IntPtr converter,
        StringCallback callback);

    protected internal abstract void SetPhaseChangedCallback(
        IntPtr converter,
        VoidCallback callback);

    protected internal abstract void SetProgressChangedCallback(
        IntPtr converter,
        IntCallback callback);

    protected internal abstract void SetFinishedCallback(
        IntPtr converter,
        IntCallback callback);
}
