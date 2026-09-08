# 0032 - Opt-in use of `AdaskoTheBeAsT.Interop.Threading` utilities (Phase 4)

- Status: Rejected (2026-09-07)
- Date: 2026-04-21
- Traceability: `docs/plan.md` Section 2.2 / 3 Phase 4.

## Context

`AdaskoTheBeAsT.Interop.Threading` ships a
`SingleThreadedApartmentTaskScheduler`, `TaskExtension.TimeoutAfterAsync`,
`MutexHelper.RunInMutex` and `StaYield` helpers. The core
`ExecutionWorker<TSession>` already flips to STA on Windows
(ADR 0030), so Threading is complementary, not mandatory.

## Decision

The proposed extra dependency is not adopted. The execution worker already owns
the dedicated thread. A timeout helper only stops waiting and cannot interrupt
wkhtmltox; a cross-process mutex does not isolate process-global Qt state or
prove safe parallel rendering. Add such a dependency only for a demonstrated,
separate requirement. See [ADR 0036](0036-request-ownership-and-native-binding.md).

### Historical proposal (not implemented)

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

- No additional Threading dependency or misleading hard-timeout API.
- Isolation, hard deadlines, and process supervision remain separate work.
