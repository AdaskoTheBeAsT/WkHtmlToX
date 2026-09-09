# 0030 - Adopt `AdaskoTheBeAsT.Interop.Execution.ExecutionWorker<TSession>` (Phase 2)

- Status: Accepted. Supersedes [ADR 0004](0004-dedicated-worker-thread.md),
  [ADR 0008](0008-cancellation-token-source.md) and [ADR 0019](0019-fix-library-initialization-race.md).
- Date: 2026-04-21
- Traceability: `docs/plan.md` Section 2.1 / 3 Phase 2; see also
  `D:\GitHub\AdaskoTheBeAsT.Interop\wkhtml.md`.

## Context

The engine's biggest block of code is generic worker machinery:
`BlockingCollection`, STA thread, startup handshake, disposal cancel
and fail, and bespoke fault signalling. That is exactly what
`ExecutionWorker<TSession>` already solves, with added session
recycling, `MaxOperationsPerSession`, `WorkerFaulted` terminal-once
semantics and `ActivitySource` + `Meter` observability.

## Decision

The original extraction below is implemented. For the current major-release
contracts, see [ADR 0036](0036-request-ownership-and-native-binding.md): use
Interop.Execution 2.x, task-returning conversions, classified request recycling,
and one native owner. `RecycleSessionOnFailure` belongs to request options, not
worker options. Pooled workers with copied native DLLs are not a supported
parallel-rendering architecture.

[ADR 0038](0038-pipeline-aware-shutdown.md) extends shutdown beyond the worker:
the engine drains or skips pending requests across input/native/output stages
before allowing worker teardown and release of native ownership.

### Original extraction decision

Combined with ADR 0028 (session / factory extraction):

1. Add a `PackageReference` to `AdaskoTheBeAsT.Interop.Execution`.
2. Rewrite `WkHtmlToXEngine` as a thin wrapper around
   `ExecutionWorker<WkHtmlToXSession>` configured with
   `useStaThread: true` and `recycleSessionOnFailure: true`.
3. Route `PdfConvertWorkItem` / `ImageConvertWorkItem` to
   `_worker.ExecuteAsync((session, ct) => ...)`.
4. Delete queue / thread / startup / disposal / STA-selection code
   from the engine.
5. Keep `IWkHtmlToXEngine`, `PdfConverter`, `ImageConverter`
   unchanged - this is a non-breaking internal swap.

## Consequences

- Drops several hundred lines of custom infrastructure.
- Adds OpenTelemetry-friendly telemetry named
  `AdaskoTheBeAsT.Interop.Execution`.
- `MaxOperationsPerSession` provides a tested mitigation for native
  memory growth over long-lived processes.
- `WorkerFaulted` gives health checks a deterministic "wkhtml is
  permanently broken in this process" signal.
- Moving from `BlockingCollection` to `Channel` slightly changes the
  timing of queued-but-not-started items; tests that asserted on
  queue depth must migrate to `GetSnapshot().QueueDepth`.
- Optional future: introduce `ExecutionWorkerPool<WkHtmlToXSession>`
  over isolated native copies for real parallel conversions.
