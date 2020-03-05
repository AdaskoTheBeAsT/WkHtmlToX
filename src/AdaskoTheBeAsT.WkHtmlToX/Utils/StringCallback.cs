using System;
using System.Runtime.InteropServices;
using System.Security;

namespace AdaskoTheBeAsT.WkHtmlToX.Utils
{
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void StringCallback(IntPtr converter, [MarshalAs(UnmanagedType.LPStr)] string str);
}
