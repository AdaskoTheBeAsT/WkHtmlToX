using System;
using System.IO;
using System.Runtime.InteropServices;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders;

#pragma warning disable IDISP025 // Class with no virtual dispose method should be sealed.
internal abstract class LibraryLoaderBase
    : ILibraryLoader
{
    private const string NativeFolder = "native";
    private const string RuntimesFolder = "runtimes";

    internal string? ExplicitPath { get; set; }

    public abstract void Load();

    public abstract void Release();

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    internal static string GetProcessorArchitecture(Architecture architecture)
    {
        return architecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            _ => throw new PlatformNotSupportedException("Only x64 and x86 native renderer processes are supported."),
        };
    }

    protected static string GetCurrentDir() => AppContext.BaseDirectory;

    protected static string GetProcessorArchitecture() => GetProcessorArchitecture(RuntimeInformation.ProcessArchitecture);

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

    protected string[] GetPaths(string runtimeIdentifier, string libraryName)
    {
        if (ExplicitPath is not null)
        {
            return [ExplicitPath];
        }

        return
        [
            GetRuntimeLibraryPath(GetCurrentDir(), runtimeIdentifier, libraryName),
            GetCurrentDirectoryLibraryPath(GetCurrentDir(), libraryName),
        ];
    }

    protected abstract void Dispose(bool disposing);
}
#pragma warning restore IDISP025 // Class with no virtual dispose method should be sealed.
