# 0035 - Add `Utf8Interop` helper for zero-allocation native string marshaling

- Status: Proposed
- Date: 2026-04-21
- Traceability: uncommitted new file
  `src/AdaskoTheBeAsT.WkHtmlToX/Utils/Utf8Interop.cs`.

## Context

`wkhtmltox` expects `const char*` UTF-8 for every settings string.
Default `Marshal.StringToHGlobalAnsi` uses the ANSI code page, not
UTF-8, and both `StringToHGlobalAnsi` / `StringToCoTaskMemUTF8`
allocate unmanaged memory per call. In a pipeline that sets dozens
of settings per conversion that cost is noticeable.

## Decision

Introduce a small `Utf8Interop` helper that:

- encodes a managed `string` to UTF-8 into a stack/`ArrayPool<byte>`
  buffer when the encoded size is bounded;
- null-terminates in place;
- exposes a `ref byte` or `byte*` suitable for the wkhtml setters;
- avoids `Marshal.StringToCoTaskMemUTF8` on hot paths.

Use it from `WkHtmlToPdfModule.SetGlobalSetting`,
`SetObjectSetting`, `WkHtmlToImageModule.SetGlobalSetting` and the
`StringCallback` bridge.

## Consequences

- Per-setting allocations drop to zero (or near zero) on hot paths.
- The helper stays internal; public API is unaffected.
- Complements ADR 0011 (pooling) and makes the memory probes from
  ADR 0034 easier to keep in the green.
