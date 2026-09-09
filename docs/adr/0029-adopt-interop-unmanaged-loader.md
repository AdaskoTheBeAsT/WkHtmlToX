# 0029 - Adopt `AdaskoTheBeAsT.Interop.Unmanaged` for native loading (Phase 1)

- Status: Accepted. Supersedes [ADR 0005](0005-linux-native-loader.md)
- Date: 2026-04-21
- Traceability: `docs/plan.md` Section 2.3 / 3 Phase 1; uncommitted
  deletions of `Loaders/SafeLibraryHandle.cs`,
  `Native/LoadLibraryFlags.cs`, `Native/SystemPosixNativeMethods.cs`,
  `Native/SystemWindowsNativeMethods.cs`.

## Context

`Loaders/` and `Native/*NativeMethods.cs` reimplement exactly what
`AdaskoTheBeAsT.Interop.Unmanaged` already ships (SafeHandle-style
library handle, cross-platform `dlopen`/`LoadLibraryEx`,
`GetUnmanagedFunction<T>` / `TryGetExport`). Maintaining our own copy
duplicates effort and misses fixes landed upstream.

## Decision

Phase 1 of the Interop migration sketched in `docs/plan.md`:

1. Add a `PackageReference` to `AdaskoTheBeAsT.Interop.Unmanaged`.
2. Replace `SafeLibraryHandle`, `SystemWindowsNativeMethods`,
   `SystemPosixNativeMethods` and `LoadLibraryFlags` with the types
   shipped by the package.
3. Rewrite `LibraryLoaderBase` + per-OS loaders as thin wrappers that
   resolve the right `runtimes/<rid>/native/wkhtmltox.*` path and
   call `new UnmanagedLibrary(path, flags)`. Keep `ILibraryLoader` as
   a seam.
4. Delegate `Release()` / `Dispose()` to the `UnmanagedLibrary`.
5. Resolve every wkhtml export through
   `GetUnmanagedFunction<T>` (classic path) or `TryGetExport`
   (`net8+`) for `delegate* unmanaged[Cdecl]<...>` call sites.
6. Preserve the public exception shape by catching the underlying
   `Win32Exception` in the wrapper and rethrowing
   `DllNotLoadedException` with the original as `InnerException`.

## Consequences

- Large deletion of custom interop under `Loaders/` and `Native/`.
- Cross-platform loader quirks (`RTLD_NOW`, musl,
  `[SupportedOSPlatform]`, forward-compat for renamed exports) are
  owned by `AdaskoTheBeAsT.Interop.Unmanaged`, not us.
- Public behaviour preserved; consumers see no API break.
