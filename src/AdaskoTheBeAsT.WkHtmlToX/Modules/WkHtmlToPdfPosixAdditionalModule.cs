using System;
using AdaskoTheBeAsT.WkHtmlToX.Native;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    internal class WkHtmlToPdfPosixAdditionalModule
        : WkHtmlToPdfModule
    {
        public override IntPtr CreateObjectSettings() => NativeMethodsPdfPosix.wkhtmltopdf_create_object_settings();

        public override int DestroyObjectSetting(
            IntPtr settings) =>
            NativeMethodsPdfPosix.wkhtmltopdf_destroy_object_settings(settings);

        public override int SetObjectSetting(
            IntPtr settings,
            string name,
            string value) => NativeMethodsPdfPosix.wkhtmltopdf_set_object_setting(settings, name, value);

        public override void AddObject(
            IntPtr converter,
            IntPtr objectSettings,
            byte[] data) =>
            NativeMethodsPdfPosix.wkhtmltopdf_add_object(converter, objectSettings, data);

        public override void AddObject(
            IntPtr converter,
            IntPtr objectSettings,
            string data) =>
            NativeMethodsPdfPosix.wkhtmltopdf_add_object(converter, objectSettings, data);

        protected override int GetObjectSettingImpl(
            IntPtr settings,
            string name,
            byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return NativeMethodsPdfPosix.wkhtmltopdf_get_object_setting(
                settings,
                name,
                buffer,
                buffer.Length);
        }
    }
}
