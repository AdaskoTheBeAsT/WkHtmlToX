using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Utils;
using Microsoft.IO;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    internal abstract class WkHtmlToXModule
        : IWkHtmlToXModule
    {
        protected const int MaxCopyBufferSize = 81920;
        protected const int MaxBufferSize = 2048;

        // used to maintain a reference to delegates to prevent them being garbage collected...
        // ReSharper disable once CollectionNeverQueried.Local
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable CA1051 // Do not declare visible instance fields
        protected readonly List<object> _delegates = new List<object>();
#pragma warning restore CA1051 // Do not declare visible instance fields
#pragma warning restore SA1401 // Fields should be private

        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager =
            new RecyclableMemoryStreamManager();

        public abstract int Initialize(
            int useGraphics);

        public abstract int Terminate();

        public abstract int ExtendedQt();

        public string GetLibraryVersion()
        {
            var ptr = GetLibraryVersionImpl();
            return Marshal.PtrToStringAnsi(ptr);
        }

        public abstract IntPtr CreateGlobalSettings();

        public abstract int DestroyGlobalSetting(
            IntPtr settings);

        public abstract int SetGlobalSetting(
            IntPtr settings,
            string name,
            string? value);

        public string GetGlobalSetting(
            IntPtr settings,
            string name)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(MaxBufferSize);
            try
            {
                _ = GetGlobalSettingImpl(
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

        public abstract IntPtr CreateConverter(
            IntPtr globalSettings);

        public abstract void DestroyConverter(
            IntPtr converter);

        public abstract int SetWarningCallback(
            IntPtr converter,
            StringCallback callback);

        public abstract int SetErrorCallback(
            IntPtr converter,
            StringCallback callback);

        public abstract int SetPhaseChangedCallback(
            IntPtr converter,
            VoidCallback callback);

        public abstract int SetProgressChangedCallback(
            IntPtr converter,
            VoidCallback callback);

        public abstract int SetFinishedCallback(
            IntPtr converter,
            IntCallback callback);

        public abstract bool Convert(
            IntPtr converter);

        public abstract int GetCurrentPhase(
            IntPtr converter);

        public string GetPhaseDescription(
            IntPtr converter,
            int phase)
        {
            var ptr = GetPhaseDescriptionImpl(converter, phase);
            return Marshal.PtrToStringAnsi(ptr);
        }

        public string GetProgressString(
            IntPtr converter)
        {
            var ptr = GetProgressStringImpl(converter);
            return Marshal.PtrToStringAnsi(ptr);
        }

        public abstract int GetPhaseCount(
            IntPtr converter);

        public abstract int GetHttpErrorCode(
            IntPtr converter);

        public Stream GetOutput(IntPtr converter)
        {
            var length = GetOutputImpl(converter, out IntPtr data);
            if (length == 0)
            {
                return Stream.Null;
            }

            var stream = _recyclableMemoryStreamManager.GetStream(
                Guid.NewGuid(),
                "wkhtmltox",
                length);

            var copyLength = Math.Min(length, MaxCopyBufferSize);

            var buffer = new byte[copyLength];
            Marshal.Copy(data, buffer, 0, copyLength);
            stream.Write(buffer, 0, copyLength);
            length -= copyLength;

            while (length > 0)
            {
                data = IntPtr.Add(data, copyLength);
                copyLength = Math.Min(length, MaxCopyBufferSize);
                buffer = new byte[copyLength];
                Marshal.Copy(data, buffer, 0, copyLength);
                stream.Write(buffer, 0, copyLength);
                length -= copyLength;
            }

            stream.Position = 0;
            return stream;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                _delegates.Clear();
            }
        }

        protected abstract int GetGlobalSettingImpl(
            IntPtr settings,
            string name,
            byte[] buffer);

        protected abstract int GetOutputImpl(
            IntPtr converter,
            out IntPtr data);

        protected abstract IntPtr GetLibraryVersionImpl();

        protected abstract IntPtr GetPhaseDescriptionImpl(
            IntPtr converter,
            int phase);

        protected abstract IntPtr GetProgressStringImpl(
            IntPtr converter);
    }
}
