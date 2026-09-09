namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

public enum ConversionFailureKind
{
    None,
    InvalidInput,
    ConversionError,
    InputReadError,
    OutputWriteError,
    NativeRuntimeError,
    CallbackError,
    Cancellation,
    ResourceLimit,
    Overloaded,
}
