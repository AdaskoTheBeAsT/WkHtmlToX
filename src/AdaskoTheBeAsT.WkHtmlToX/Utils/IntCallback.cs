using System;
using System.Runtime.InteropServices;

namespace AdaskoTheBeAsT.WkHtmlToX.Utils
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void IntCallback(IntPtr converter, int integer);
}
