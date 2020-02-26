using System;
using System.Buffers;
using System.Text;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    internal abstract class WkHtmlToPdfModule
        : IWkHtmlToPdfModule
    {
        protected const int MaxBufferSize = 2048;

        public abstract IntPtr CreateObjectSettings();

        public abstract int DestroyObjectSetting(
            IntPtr settings);

        public abstract int SetObjectSetting(
            IntPtr settings,
            string name,
            string? value);

        public string GetObjectSetting(
            IntPtr settings,
            string name)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(MaxBufferSize);
            try
            {
                _ = GetObjectSettingImpl(
                    settings,
                    name,
                    buffer);

                var nullPos = Array.IndexOf(buffer, byte.MinValue);

                return Encoding.UTF8.GetString(buffer, 0, nullPos);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public abstract void AddObject(
            IntPtr converter,
            IntPtr objectSettings,
            byte[] data);

        public abstract void AddObject(
            IntPtr converter,
            IntPtr objectSettings,
            string data);

        protected abstract int GetObjectSettingImpl(
            IntPtr settings,
            string name,
            byte[] buffer);
    }
}
