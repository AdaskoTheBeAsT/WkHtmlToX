using System;
using System.Runtime.InteropServices;

namespace AdaskoTheBeAsT.WkHtmlToX.Utils
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void StringCallback(IntPtr converter, [MarshalAs(UnmanagedType.LPStr)] string str);
}
