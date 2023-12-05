using System;
using System.Runtime.InteropServices;
using System.Security;

namespace AdaskoTheBeAsT.WkHtmlToX.Utils;

[SuppressUnmanagedCodeSecurity]
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void IntCallback(IntPtr converter, int integer);
