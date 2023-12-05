namespace AdaskoTheBeAsT.WkHtmlToX.Utils;

/// <summary>
/// This enum "extends" UnmanagedType enum from System.Runtime.InteropServices v4.1.0
/// which doesn't have LPUTF8Str (enum value is 48) enumeration defined.
/// </summary>
internal enum CustomUnmanagedType
{
    // ReSharper disable once InconsistentNaming
    LPUTF8Str = 48,
}
