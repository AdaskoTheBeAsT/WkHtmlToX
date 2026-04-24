using System;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions;

internal interface IWkHtmlToPdfModule
    : IWkHtmlToXModule
{
    IntPtr CreateObjectSettings();

    void DestroyObjectSetting(IntPtr settings);

    int SetObjectSetting(
        IntPtr settings,
        string name,
        string? value);

    string GetObjectSetting(
        IntPtr settings,
        string name);

    void AddObject(
        IntPtr converter,
        IntPtr objectSettings,
        byte[] data);

    void AddObject(
        IntPtr converter,
        IntPtr objectSettings,
        string data);
}
