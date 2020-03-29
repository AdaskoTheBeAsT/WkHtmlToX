using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    [ExcludeFromCodeCoverage]
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

        private readonly IBufferManager _bufferManager;

        protected WkHtmlToXModule(
            IBufferManager bufferManager)
        {
            _bufferManager = bufferManager;
        }

        protected WkHtmlToXModule()
            : this(new BufferManager())
        {
        }

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
                var retVal = GetGlobalSettingImpl(
                    settings,
                    name,
                    buffer);

                if (retVal != 1)
                {
                    throw new GetGlobalSettingsFailedException($"GetGlobalSettings failed for obtaining setting={name}");
                }

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

        public string GetProgressDescription(
            IntPtr converter)
        {
            var ptr = GetProgressStringImpl(converter);
            return Marshal.PtrToStringAnsi(ptr);
        }

        public abstract int GetPhaseCount(
            IntPtr converter);

        public abstract int GetHttpErrorCode(
            IntPtr converter);

        public void GetOutput(IntPtr converter, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            GetOutput(converter, length => stream);
        }

        public void GetOutput(
            IntPtr converter,
            Func<int, Stream> createStreamFunc)
        {
            if (createStreamFunc == null)
            {
                throw new ArgumentNullException(nameof(createStreamFunc));
            }

            var totalLength = GetOutputImpl(converter, out IntPtr data);
            if (totalLength == 0)
            {
                return;
            }

            var stream = createStreamFunc(totalLength);
            if (stream is null)
            {
                throw new ArgumentException("Create stream returned null");
            }

            var length = Math.Min(totalLength, MaxCopyBufferSize);
            _bufferManager.CopyBuffer(data, stream, length);
            totalLength -= length;

            while (totalLength > 0)
            {
                data = IntPtr.Add(data, length);
                length = Math.Min(totalLength, MaxCopyBufferSize);
                _bufferManager.CopyBuffer(data, stream, length);
                totalLength -= length;
            }

            stream.Flush();
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
