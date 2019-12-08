using System.Security.Permissions;
using AdaskoTheBeAsT.WkHtmlToX.Native;
using Microsoft.Win32.SafeHandles;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders
{
    /// <summary>
    /// See https://docs.microsoft.com/en-us/archive/msdn-magazine/2005/october/using-the-reliability-features-of-the-net-framework
    /// for more about safe handles.
    /// </summary>
    // ReSharper disable ClassNeverInstantiated.Global
#pragma warning disable S3453
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    internal sealed class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
#pragma warning restore S3453
    {
        private SafeLibraryHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return NativeMethodsSystemWindows.FreeLibrary(handle);
        }
    }

    // ReSharper restore ClassNeverInstantiated.Global
}
