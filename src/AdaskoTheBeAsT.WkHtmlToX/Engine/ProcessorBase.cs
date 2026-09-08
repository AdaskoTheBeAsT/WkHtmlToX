using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.EventDefinitions;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Utils;
using ErrorEventArgs = AdaskoTheBeAsT.WkHtmlToX.EventDefinitions.ErrorEventArgs;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

internal abstract class ProcessorBase
{
    private readonly WkHtmlToXConfiguration _configuration;
    private readonly List<string> _warnings = [];
    private Exception? _callbackFailure;
    private StringCallback? _warningCallback;
    private StringCallback? _errorCallback;
    private VoidCallback? _phaseChangedCallback;
    private IntCallback? _progressChangedCallback;
    private IntCallback? _finishedCallback;

    protected ProcessorBase(WkHtmlToXConfiguration configuration)
    {
        _configuration = configuration.Snapshot();
    }

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

        if (_configuration.PhaseChangedAction != null)
        {
            _phaseChangedCallback = pointer => CaptureCallback(() => OnPhaseChanged(pointer));
            SetPhaseChangedCallback(converter, _phaseChangedCallback);
        }

        if (_configuration.ProgressChangedAction != null)
        {
            _progressChangedCallback = (pointer, progress) => CaptureCallback(() => OnProgressChanged(pointer, progress));
            SetProgressChangedCallback(converter, _progressChangedCallback);
        }

        if (_configuration.FinishedAction != null)
        {
            _finishedCallback = (_, success) => CaptureCallback(() => OnFinished(success));
            SetFinishedCallback(converter, _finishedCallback);
        }

        _warningCallback = (_, message) => CaptureCallback(() => OnWarning(message));
        SetWarningCallback(converter, _warningCallback);

        if (_configuration.ErrorAction != null)
        {
            _errorCallback = (_, message) => CaptureCallback(() => OnError(message));
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

    protected internal void OnProgressChanged(IntPtr converter, int progress)
    {
        if (_configuration.ProgressChangedAction == null)
        {
            return;
        }

        var progressDescription = GetProgressDescription(converter);
        var eventArgs = new ProgressChangedEventArgs(
            ProcessingDocument,
            progress,
            progressDescription);

        _configuration.ProgressChangedAction?.Invoke(eventArgs);
    }

    protected internal void OnFinished(int success)
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

#if NET462
    protected internal void OnError(IntPtr messagePointer)
    {
        if (_configuration.ErrorAction == null)
        {
            return;
        }

        var message = Utf8Interop.PtrToString(messagePointer);

        var eventArgs = new ErrorEventArgs(
            ProcessingDocument,
            message);

        _configuration.ErrorAction?.Invoke(eventArgs);
    }

    protected internal void OnWarning(IntPtr messagePointer)
    {
        var message = Utf8Interop.PtrToString(messagePointer);
        RecordWarning(message);

        var eventArgs = new WarningEventArgs(
            ProcessingDocument,
            message);

        _configuration.WarningAction?.Invoke(eventArgs);
    }
#else
    protected internal void OnError(string? message)
    {
        if (_configuration.ErrorAction == null)
        {
            return;
        }

        var eventArgs = new ErrorEventArgs(
            ProcessingDocument,
            message ?? string.Empty);

        _configuration.ErrorAction?.Invoke(eventArgs);
    }

    protected internal void OnWarning(string? message)
    {
        RecordWarning(message ?? string.Empty);

        var eventArgs = new WarningEventArgs(
            ProcessingDocument,
            message ?? string.Empty);

        _configuration.WarningAction?.Invoke(eventArgs);
    }
#endif

    protected internal ConversionResult ExecuteConversion(
        ISettings document,
        Func<IntPtr> createConverter,
        IWkHtmlToXModule module,
        Func<int, Stream> createStreamFunc,
        CancellationToken cancellationToken)
    {
        ProcessingDocument = document;
        _callbackFailure = null;
        _warnings.Clear();
        var converter = IntPtr.Zero;
        var failureKind = ConversionFailureKind.NativeRuntimeError;
        Exception? failure = null;
        var httpErrorCode = 0;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            converter = createConverter.Invoke();
            RegisterEvents(converter);
            cancellationToken.ThrowIfCancellationRequested();
            var converted = module.Convert(converter);
            httpErrorCode = module.GetHttpErrorCode(converter);
            if (converted)
            {
                cancellationToken.ThrowIfCancellationRequested();
                (failureKind, failure) = WriteOutput(module, converter, createStreamFunc, cancellationToken);
            }
            else
            {
                failureKind = ConversionFailureKind.ConversionError;
            }
        }
        catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
        {
            failureKind = ConversionFailureKind.Cancellation;
            failure = exception;
        }
        catch (Exception exception)
        {
            if (converter == IntPtr.Zero)
            {
                failureKind = exception switch
                {
                    ArgumentException or HtmlContentEmptyException or HtmlContentStreamTooLargeException => ConversionFailureKind.InvalidInput,
                    IOException => ConversionFailureKind.InputReadError,
                    _ => ConversionFailureKind.NativeRuntimeError,
                };
            }

            failure = exception;
        }
        finally
        {
            (failureKind, failure) = DestroyConverter(module, converter, failureKind, failure);
        }

        failureKind = _callbackFailure is not null && failureKind == ConversionFailureKind.None
            ? ConversionFailureKind.CallbackError
            : failureKind;

        return new ConversionResult(
            failureKind,
            httpErrorCode,
            failure,
            _callbackFailure,
            Array.AsReadOnly(_warnings.ToArray()));
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

        var setter = GetApplySettingFunc(useGlobal);
        var localName = string.IsNullOrEmpty(prefix) ? name : $"{prefix}.{name}";

        if (typeof(bool) == type)
        {
            ApplySetting(setter, config, localName, (bool)value ? "true" : "false");
        }
        else if (typeof(double) == type)
        {
            ApplySetting(setter, config, localName, ((double)value).ToString("0.##", CultureInfo.InvariantCulture));
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
                ApplySetting(setter, config, $"{localName}.append", text: null);
                ApplySetting(setter, config, $"{localName}[{index.ToString(CultureInfo.InvariantCulture)}]", $"{pair.Key}\n{pair.Value}");

                index++;
            }
        }
        else
        {
            ApplySetting(setter, config, localName, value.ToString());
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

    private static void ApplySetting(
        Func<IntPtr, string, string?, int> setter,
        IntPtr pointer,
        string key,
        string? text)
    {
        if (setter.Invoke(pointer, key, text) != 1)
        {
            // Never include setting values (which can contain credentials or HTML).
            throw new ArgumentException($"The native renderer rejected setting '{key}'.");
        }
    }

    private static (ConversionFailureKind kind, Exception? failure) WriteOutput(
        IWkHtmlToXModule module,
        IntPtr converter,
        Func<int, Stream> createStreamFunc,
        CancellationToken cancellationToken)
    {
        try
        {
            module.GetOutput(converter, createStreamFunc);
            return (kind: ConversionFailureKind.None, failure: null);
        }
        catch (OutputWriteException exception)
        {
            var kind = exception.InnerException is OperationCanceledException && cancellationToken.IsCancellationRequested
                ? ConversionFailureKind.Cancellation
                : ConversionFailureKind.OutputWriteError;
            return (kind, failure: exception.InnerException);
        }
    }

    private (ConversionFailureKind kind, Exception? failure) DestroyConverter(
        IWkHtmlToXModule module,
        IntPtr converter,
        ConversionFailureKind kind,
        Exception? failure)
    {
        try
        {
            if (converter != IntPtr.Zero)
            {
                module.DestroyConverter(converter);
            }

            return (kind, failure);
        }
        catch (Exception exception)
        {
            return (kind: ConversionFailureKind.NativeRuntimeError, failure: failure is null ? exception : new AggregateException(failure, exception));
        }
        finally
        {
            // Native code can still invoke callbacks during destruction.
            ReleaseRegisteredCallbacks();
            ProcessingDocument = null;
        }
    }

    private void CaptureCallback(Action callback)
    {
        try
        {
            callback.Invoke();
        }
        catch (Exception exception)
        {
            // Never unwind an application exception through a reverse-P/Invoke frame.
            _callbackFailure ??= exception;
        }
    }

    private void RecordWarning(string message)
    {
        if (_warnings.Count < 32)
        {
#if NET8_0_OR_GREATER
            _warnings.Add(message.Length <= 1024 ? message : message[..1024]);
#else
            _warnings.Add(message.Length <= 1024 ? message : message.Substring(0, 1024));
#endif
        }
    }
}
