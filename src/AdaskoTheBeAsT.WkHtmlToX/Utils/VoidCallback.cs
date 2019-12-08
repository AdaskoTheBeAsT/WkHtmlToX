using System;
using System.Runtime.InteropServices;

namespace AdaskoTheBeAsT.WkHtmlToX.Utils
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void VoidCallback(IntPtr converter);
}
