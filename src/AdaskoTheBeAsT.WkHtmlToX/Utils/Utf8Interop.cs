using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AdaskoTheBeAsT.WkHtmlToX.Utils;

internal static class Utf8Interop
{
    internal static string PtrToString(IntPtr pointer)
    {
        if (pointer == IntPtr.Zero)
        {
            return string.Empty;
        }

        var length = 0;
        while (Marshal.ReadByte(pointer, length) != 0)
        {
            length++;
        }

        if (length == 0)
        {
            return string.Empty;
        }

        var buffer = new byte[length];
        Marshal.Copy(pointer, buffer, 0, length);
        return Encoding.UTF8.GetString(buffer, 0, length);
    }
}
