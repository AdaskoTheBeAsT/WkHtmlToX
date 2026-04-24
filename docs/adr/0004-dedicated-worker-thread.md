# 0004 - Serialize native calls on a dedicated worker thread with a blocking queue

- Status: Superseded by [ADR 0030](0030-adopt-execution-worker.md)
- Date: 2019-12-08
- Traceability: `d51d1ad`, `e4630a9` "added CancellationTokenSource due to problems in .net core"

## Context

`wkhtmltox` is not re-entrant and, on Windows, has historically
been happiest when called from an STA thread that owns a message
pump. Calling it from arbitrary `ThreadPool` threads led to crashes
and hangs, especially under concurrency.

## Decision

Own a single background `Thread` in `WkHtmlToXEngine`:

- pin it STA on Windows via `SetApartmentState(ApartmentState.STA)`;
- feed it through a `BlockingCollection<ConvertWorkItemBase>`;
- synchronize startup with a `TaskCompletionSource<Exception?>`;
- on disposal, cancel in-flight and fail pending items.

All public `ConvertAsync` entry points enqueue work items and wait
on their per-item `TaskCompletionSource`.

## Consequences

- Deterministic single-threaded access to the native API.
- Queue depth / apartment state / disposal ordering are now our
  responsibility, which produced a non-trivial amount of code to
  maintain (several hundred lines in `WkHtmlToXEngine.cs`).
- Set the stage for ADR 0030, which replaces the hand-rolled worker
  with `ExecutionWorker<TSession>`.
