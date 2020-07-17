#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using AdaskoTheBeAsT.WkHtmlToX.Native;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    [ExcludeFromCodeCoverage]
    internal class WkHtmlToPdfAdditionalModule
        : WkHtmlToPdfModule
    {
        public override IntPtr CreateObjectSettings() => NativeMethodsPdf.wkhtmltopdf_create_object_settings();

        public override int DestroyObjectSetting(
            IntPtr settings) =>
            NativeMethodsPdf.wkhtmltopdf_destroy_object_settings(settings);

        public override int SetObjectSetting(
            IntPtr settings,
            string name,
            string? value) => NativeMethodsPdf.wkhtmltopdf_set_object_setting(settings, name, value);

        public override void AddObject(
            IntPtr converter,
            IntPtr objectSettings,
            byte[] data) =>
            NativeMethodsPdf.wkhtmltopdf_add_object(converter, objectSettings, data);

        public override void AddObject(
            IntPtr converter,
            IntPtr objectSettings,
            string data) =>
            NativeMethodsPdf.wkhtmltopdf_add_object(converter, objectSettings, data);

        protected override int GetObjectSettingImpl(
            IntPtr settings,
            string name,
            byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return NativeMethodsPdf.wkhtmltopdf_get_object_setting(
                settings,
                name,
                buffer,
                buffer.Length);
        }
    }
}
