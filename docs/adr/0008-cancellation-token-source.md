# 0008 - Introduce `CancellationTokenSource` to fix .NET Core hang on shutdown

- Status: Accepted
- Date: 2020-02-25
- Traceability: `e4630a9` "added CancellationTokenSource due to problems in .net core"

## Context

Under .NET Core 3.1 the background worker thread occasionally held
the process open past `IHost` shutdown because `BlockingCollection.Take`
does not observe managed cancellation signals by itself.

## Decision

Own a dedicated `CancellationTokenSource` inside the engine. Cancel
it on `Dispose` / shutdown and pass the token into
`BlockingCollection.Take(out item, token)` so the worker loop exits
deterministically. Fail pending work items with an
`OperationCanceledException` during disposal.

## Consequences

- Shutdown is deterministic on both .NET Core and .NET Framework.
- Pending conversions surface cancellation to callers instead of
  hanging.
- Later subsumed by `ExecutionWorker<TSession>`'s built-in disposal
  semantics (ADR 0030).
