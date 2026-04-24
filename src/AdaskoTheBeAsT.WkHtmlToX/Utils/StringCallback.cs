using System;
using System.Runtime.InteropServices;
using System.Security;

namespace AdaskoTheBeAsT.WkHtmlToX.Utils;

[SuppressUnmanagedCodeSecurity]
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#if NET462
public delegate void StringCallback(IntPtr converter, IntPtr str);
#else
public delegate void StringCallback(IntPtr converter, [MarshalAs(UnmanagedType.LPUTF8Str)] string? str);
#endif
