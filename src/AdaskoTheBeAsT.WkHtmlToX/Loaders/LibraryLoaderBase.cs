using System;
using System.IO;
using System.Reflection;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders
{
#pragma warning disable IDISP025 // Class with no virtual dispose method should be sealed.
    public abstract class LibraryLoaderBase
        : ILibraryLoader
    {
        private const string NativeFolder = "native";
        private const string RuntimesFolder = "runtimes";

        public abstract void Load();

        public abstract void Release();

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected static string GetCurrentDir()
        {
            var uri = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase!);

            return Path.GetDirectoryName(uri.LocalPath) ?? "./";
        }

        protected static string GetProcessorArchitecture()
        {
            return Environment.Is64BitProcess ? "x64" : "x86";
        }

        protected static string GetRuntimeLibraryPath(
            string rootDirectory,
            string runtimeIdentifier,
            string libraryName)
        {
            return Path.Combine(rootDirectory, RuntimesFolder, runtimeIdentifier, NativeFolder, libraryName);
        }

        protected static string GetCurrentDirectoryLibraryPath(
            string rootDirectory,
            string libraryName)
        {
            return Path.Combine(rootDirectory, libraryName);
        }

        protected virtual void Dispose(bool disposing)
        {
            // no op
        }
    }
#pragma warning restore IDISP025 // Class with no virtual dispose method should be sealed.
}
