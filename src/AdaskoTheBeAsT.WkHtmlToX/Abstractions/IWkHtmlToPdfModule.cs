using System;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    internal interface IWkHtmlToPdfModule
    {
        IntPtr CreateObjectSettings();

        int DestroyObjectSetting(IntPtr settings);

        int SetObjectSetting(
            IntPtr settings,
            string name,
            string value);

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
}
