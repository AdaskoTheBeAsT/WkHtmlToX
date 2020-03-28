using System;
using AdaskoTheBeAsT.WkHtmlToX.Native;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    internal class WkHtmlToPdfWindowsAdditionalModule
        : WkHtmlToPdfModule
    {
        public override IntPtr CreateObjectSettings() => NativeMethodsPdfWindows.CreateObjectSettings();

        public override int DestroyObjectSetting(
            IntPtr settings) =>
            NativeMethodsPdfWindows.DestroyObjectSettings(settings);

        public override int SetObjectSetting(
            IntPtr settings,
            string name,
            string? value) => NativeMethodsPdfWindows.SetObjectSettings(settings, name, value);

        public override void AddObject(
            IntPtr converter,
            IntPtr objectSettings,
            byte[] data) =>
            NativeMethodsPdfWindows.AddObject(converter, objectSettings, data);

        public override void AddObject(
            IntPtr converter,
            IntPtr objectSettings,
            string data) =>
            NativeMethodsPdfWindows.AddObject(converter, objectSettings, data);

        protected override int GetObjectSettingImpl(
            IntPtr settings,
            string name,
            byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return NativeMethodsPdfWindows.GetObjectSettings(
                settings,
                name,
                buffer,
                buffer.Length);
        }
    }
}
