# 0032 - Opt-in use of `AdaskoTheBeAsT.Interop.Threading` utilities (Phase 4)

- Status: Proposed
- Date: 2026-04-21
- Traceability: `docs/plan.md` Section 2.2 / 3 Phase 4.

## Context

`AdaskoTheBeAsT.Interop.Threading` ships a
`SingleThreadedApartmentTaskScheduler`, `TaskExtension.TimeoutAfterAsync`,
`MutexHelper.RunInMutex` and `StaYield` helpers. The core
`ExecutionWorker<TSession>` already flips to STA on Windows
(ADR 0030), so Threading is complementary, not mandatory.

## Decision

Take on `AdaskoTheBeAsT.Interop.Threading` only behind Windows TFMs
(`net*-windows`) and only to enable specific features:

- a `ConvertAsync(..., TimeSpan timeout, CancellationToken)` overload
  on `PdfConverter` / `ImageConverter` that wraps the inner task in
  `TaskExtension.TimeoutAfterAsync`;
- `MutexHelper.RunInMutex("Global\\WkHtmlToX", ...)` around
  `WkHtmlToXSessionFactory.CreateSession` on multi-process build
  agents where concurrent native inits fight over the same
  `wkhtmltox.dll` working set;
- `SingleThreadedApartmentTaskScheduler` as an alternative to the
  Execution worker's own STA thread when the host already owns one.

Do not unconditionally force the dependency on non-Windows TFMs.

## Consequences

- Optional timeout / mutex / STA scheduler primitives for hosts that
  need them.
- Linux / macOS builds are not forced to pull a Windows-only package.
