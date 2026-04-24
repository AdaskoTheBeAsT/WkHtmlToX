# 0002 - Wrap `wkhtmltox` natively via P/Invoke instead of shelling out to the CLI

- Status: Accepted
- Date: 2019-12-08
- Traceability: `d51d1ad` "Initial commit"

## Context

The most common .NET `wkhtmltopdf` integrations at the time spawned the
`wkhtmltopdf.exe` CLI per conversion. That path pays a process-start
cost per request, loses streaming of input/output, makes cancellation
unreliable, and is hostile to server environments that forbid arbitrary
child processes.

## Decision

Load `wkhtmltox.dll` / `libwkhtmltox.*` in-process and call its C API via
`DllImport` (`Native/*NativeMethods.cs`). Expose high-level
`PdfConverter` / `ImageConverter` facades that hide P/Invoke from
consumers. Own the native lifetime (init / terminate) inside the
library.

## Consequences

- No per-call process spawn; conversions run in-process, cancellable
  through managed `CancellationToken`.
- The library carries a platform matrix of native binaries under
  `runtimes/<rid>/native/`.
- Threading and STA concerns become our problem, which is addressed by
  ADR 0004 and later ADR 0030.
- Loader portability becomes our problem, which is addressed by ADR 0005
  and later ADR 0029.
