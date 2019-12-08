using System;
using AdaskoTheBeAsT.WkHtmlToX.Native;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    internal class WkHtmlToPdfWindowsAdditionalModule
        : WkHtmlToPdfModule
    {
        public override IntPtr CreateObjectSettings() => NativeMethodsPdfWindows.wkhtmltopdf_create_object_settings();

        public override int DestroyObjectSetting(
            IntPtr settings) =>
            NativeMethodsPdfWindows.wkhtmltopdf_destroy_object_settings(settings);

        public override int SetObjectSetting(
            IntPtr settings,
            string name,
            string value) => NativeMethodsPdfWindows.wkhtmltopdf_set_object_setting(settings, name, value);

        public override void AddObject(
            IntPtr converter,
            IntPtr objectSettings,
            byte[] data) =>
            NativeMethodsPdfWindows.wkhtmltopdf_add_object(converter, objectSettings, data);

        public override void AddObject(
            IntPtr converter,
            IntPtr objectSettings,
            string data) =>
            NativeMethodsPdfWindows.wkhtmltopdf_add_object(converter, objectSettings, data);

        protected override int GetObjectSettingImpl(
            IntPtr settings,
            string name,
            byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return NativeMethodsPdfWindows.wkhtmltopdf_get_object_setting(
                settings,
                name,
                buffer,
                buffer.Length);
        }
    }
}
