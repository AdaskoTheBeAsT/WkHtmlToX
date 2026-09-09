# 0039 - Expose wrapper-owned worker configuration

- Status: Accepted
- Date: 2026-09-09
- Refines [0030](0030-adopt-execution-worker.md),
  [0031](0031-dependency-injection-and-hosting-packages.md), and
  [0038](0038-pipeline-aware-shutdown.md).

## Context

WK-05 calls for wrapper-owned options rather than making the generic execution
package the permanent configuration contract. Previously, only DI/hosting
consumers could configure recycling and synchronous disposal timeout, through an
`ExecutionWorkerOptions` delegate. Standalone engines always used fixed defaults.
Interop.Execution 2.1.0 updates dependencies without changing the worker APIs.

## Decision

- Add `WkHtmlToXConfiguration.WorkerOptions`, containing a
  `WkHtmlToXWorkerOptions` with `Name`, `MaxOperationsPerSession`, `DisposeTimeout`,
  and the wrapper-owned `WkHtmlToXShutdownMode`.
- Preserve defaults: the existing worker name, no periodic recycling, infinite
  synchronous disposal wait, drain shutdown, and STA on Windows.
- Validate and snapshot options at construction/registration, before reserving
  native ownership or modifying the service collection. Reject missing option
  objects, negative recycling intervals, unsupported shutdown modes, and timeouts
  outside zero through `int.MaxValue` milliseconds (except the infinite sentinel).
- Translate to Interop options internally. Do not replace the shared worker,
  change its outcome processing, or adopt pooled ValueTasks without benchmarks.
- Preserve existing registration signatures and their advanced Interop delegate,
  applied after the wrapper options. Keep existing explicit shutdown overloads;
  the parameterless-policy overload uses the configured wrapper policy.
- Keep whole-pipeline admission in `RequestOptions`. Native queue capacity,
  custom diagnostics, and advanced thread settings remain accessible through the
  existing delegate, rather than duplicating every generic worker option.

## Consequences

Common configuration and README examples no longer require an Interop namespace
and behave consistently for direct construction and hosting. Existing advanced
configuration retains precedence. This is an additive migration, not removal of
every Interop type from the public API.

Neither disposal timeouts nor cancel-pending shutdown interrupts native calls
or active stream operations. Asynchronous disposal still joins actual teardown.
Process isolation, Qt thread replacement warnings, and unvalidated native
platforms remain outside this change.
