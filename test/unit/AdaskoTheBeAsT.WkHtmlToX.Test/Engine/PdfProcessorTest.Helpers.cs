using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Engine;

public partial class PdfProcessorTest
{
    private static IntPtr StringToUtf8Pointer(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value + "\0");
        var pointer = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, pointer, bytes.Length);
        return pointer;
    }
}
