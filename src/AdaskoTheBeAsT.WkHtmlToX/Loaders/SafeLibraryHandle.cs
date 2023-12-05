using System.Diagnostics.CodeAnalysis;
#if NET
#else
using System.Security.Permissions;
#endif
using AdaskoTheBeAsT.WkHtmlToX.Native;
using Microsoft.Win32.SafeHandles;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders;

/// <summary>
/// See https://docs.microsoft.com/en-us/archive/msdn-magazine/2005/october/using-the-reliability-features-of-the-net-framework
/// for more about safe handles.
/// </summary>
#pragma warning disable S3453
// ReSharper disable ClassNeverInstantiated.Global
[ExcludeFromCodeCoverage]
#if NET
#else
[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
#endif
internal sealed class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public SafeLibraryHandle()
        : base(ownsHandle: true)
    {
    }

    protected override bool ReleaseHandle()
    {
        return SystemWindowsNativeMethods.FreeLibrary(handle);
    }
}

// ReSharper restore ClassNeverInstantiated.Global
#pragma warning restore S3453
