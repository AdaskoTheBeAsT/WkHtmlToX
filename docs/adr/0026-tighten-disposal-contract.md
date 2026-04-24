# 0026 - Tighten disposal contract across native owners

- Status: Accepted
- Date: 2026-04-04
- Traceability: `da8ee81` "fixed findings about disposal"

## Context

Static analysis and memory probes flagged several native-owning types
as having non-idempotent or racy `Dispose` implementations: the
loader could be released twice if the engine and a test harness both
disposed it, and some exception paths could return a rented
`ArrayPool` buffer twice.

## Decision

For every type that owns unmanaged resources
(`SafeLibraryHandle`, `LibraryLoader*`, `WkHtmlToPdfModule`,
`WkHtmlToImageModule`, `ProcessorBase`, `WkHtmlToXEngine`):

- Implement the standard `Dispose(bool)` pattern.
- Make `Dispose` idempotent via a `_disposed` flag.
- Protect finalizers with `GC.SuppressFinalize(this)` after managed
  disposal.
- Always return `ArrayPool` buffers in `finally` blocks.
- Surface double-dispose in tests so regressions are caught.

## Consequences

- `AV_*` crashes on process exit stop reproducing.
- Sets up the ground rules that `ExecutionWorker<TSession>` expects
  from its session type (ADR 0030).
