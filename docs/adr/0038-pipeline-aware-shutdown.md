# 0038 - Coordinate shutdown across the whole conversion pipeline

- Status: Accepted
- Date: 2026-09-07
- Refines shutdown in [0030](0030-adopt-execution-worker.md),
  [0031](0031-dependency-injection-and-hosting-packages.md), and
  [0036](0036-request-ownership-and-native-binding.md).

## Context

Stopping only the native worker rejects admitted requests still reading input
and can report host stop before output delivery finishes. Container disposal can
also stop the worker after an engine's synchronous disposal wait times out.
Neither behavior represents shutdown of the user-visible conversion pipeline.

## Decision

- Expose `ShutdownAsync` on the async engine. Close admission synchronously and
  create one shared shutdown task. The first call selects the policy; subsequent
  shutdown/disposal calls join it and cannot escalate or change the policy.
- Default to the snapshotted worker policy (`Drain` by default). An overload
  selects `ExecutionShutdownMode` explicitly, including for standalone engines.
- Drain finishes every admitted request, including snapshots in progress, input
  reads, native execution, and destination delivery, before worker teardown.
- Cancel-pending skips requests that have not crossed the native-start gate.
  Check between managed reads and again at the start of the worker delegate.
  Queued requests can remain pending until the active delegate exits; they do
  not render or cause failure-based recycling. Never interrupt an active stream
  operation or cancel an already-started native request/delivery on shutdown's
  behalf. Ordinary per-request cancellation remains independent.
- Only after every request releases its resources may the owned worker exit
  and release native ownership. Host stop calls the engine, not a separate
  execution-worker hosted service.
- DI makes the engine the worker lifecycle coordinator. Worker disposal routes
  back through that coordinator, so container order and repeated disposal cannot
  bypass a pipeline still draining after a bounded synchronous wait.
- A shutdown cancellation token, including an expired host deadline, cancels
  only that caller's wait. It still starts shutdown. Sync disposal honors the
  worker's `DisposeTimeout` across the entire pipeline; async disposal joins
  actual completion. Native-thread lifecycle reentry remains rejected.

## Consequences

A successfully completed shutdown now means no borrowed stream operations or
native worker cleanup remain. A canceled/timed-out wait does not mean this:
retain conversion tasks and streams, and later join shutdown/disposal.
Hung native code or non-cooperative stream I/O can still prevent completion.
Callers must not synchronously wait for shutdown from their own stream methods
or destination factories. Hard deadlines and crash containment still require
supervised processes, not forced thread termination or native unloading.
