# 0019 - Fix library initialisation race

- Status: Accepted
- Date: 2021-02-23
- Traceability: `76dd4f2` "fix for initializing library"

## Context

On some Windows hosts the PDF and Image modules raced during
`wkhtmltopdf_init` / `wkhtmlimage_init` when the engine was created
and immediately used from multiple threads. Symptoms were sporadic
`GetGlobalSettingsFailedException` on the first conversion.

## Decision

Gate module initialisation through the worker thread introduced in
ADR 0004 and a local startup handshake (`TaskCompletionSource`). No
caller observes a half-initialised engine; a failed init surfaces the
original exception on every queued conversion.

## Consequences

- Fewer flaky first-conversion failures.
- Init cost is paid once, on the worker thread, deterministically.
- Becomes part of the contract later implemented by
  `ExecutionWorker<TSession>` in ADR 0030.
