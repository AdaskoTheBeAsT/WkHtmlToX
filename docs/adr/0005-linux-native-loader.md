# 0005 - Support Linux native loading via `libdl` P/Invoke

- Status: Superseded by [ADR 0029](0029-adopt-interop-unmanaged-loader.md)
- Date: 2019-12-15
- Traceability: `5956fb0` "adding tests, modifying loader for linux"

## Context

The original loader only handled Windows (`LoadLibraryEx`,
`GetProcAddress`, `FreeLibrary`). Linux and macOS consumers could not
use the library in-process.

## Decision

Split the loader into `LibraryLoaderBase`, `LibraryLoaderWindows`,
`LibraryLoaderLinux`, `LibraryLoaderOsx` and `LibraryLoaderPosix`.
Wrap `dlopen`, `dlsym`, `dlclose` through `SystemPosixNativeMethods`
and keep `SafeLibraryHandle` as the RAII wrapper. Pick the right
loader at runtime from `RuntimeInformation.IsOSPlatform(...)` and
discover the correct `runtimes/<rid>/native/libwkhtmltox.*` file.

## Consequences

- The library runs on Linux and macOS for the first time.
- We now own a cross-platform test matrix for the loader.
- Mono, glibc/musl, and `RTLD_NOW` quirks are our problem.
- ADR 0029 eventually delegates all of this to
  `AdaskoTheBeAsT.Interop.Unmanaged`.
