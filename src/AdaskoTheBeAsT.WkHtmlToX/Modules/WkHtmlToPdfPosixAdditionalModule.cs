using System;
using System.Diagnostics.CodeAnalysis;
using AdaskoTheBeAsT.WkHtmlToX.Native;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    [ExcludeFromCodeCoverage]
    internal class WkHtmlToPdfPosixAdditionalModule
        : WkHtmlToPdfModule
    {
        public override IntPtr CreateObjectSettings() => NativeMethodsPdfPosix.CreateObjectSettings();

        public override int DestroyObjectSetting(
            IntPtr settings) =>
            NativeMethodsPdfPosix.DestroyObjectSettings(settings);

        public override int SetObjectSetting(
            IntPtr settings,
            string name,
            string? value) => NativeMethodsPdfPosix.SetObjectSettings(settings, name, value);

        public override void AddObject(
            IntPtr converter,
            IntPtr objectSettings,
            byte[] data) =>
            NativeMethodsPdfPosix.AddObject(converter, objectSettings, data);

        public override void AddObject(
            IntPtr converter,
            IntPtr objectSettings,
            string data) =>
            NativeMethodsPdfPosix.AddObject(converter, objectSettings, data);

        protected override int GetObjectSettingImpl(
            IntPtr settings,
            string name,
            byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return NativeMethodsPdfPosix.GetObjectSettings(
                settings,
                name,
                buffer,
                buffer.Length);
        }
    }
}
