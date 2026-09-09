# 0036 - Bounded request ownership and process-lifetime native binding

- Status: Accepted
- Date: 2026-09-07
- Traceability: `astra_plan.md`, WK-06 and Phase 4.
- Clarifies the current contracts of ADRs 0012, 0028, 0029, 0030, and 0031.

## Context

Queued mutable documents could change before execution. Borrowed streams and
slow output shared the only native thread. Native setting failures were ignored.
Explicit loading and cached P/Invoke addresses did not establish safe unloading
or replacement. Historical README examples used internal APIs and overstated
platform and cancellation guarantees.

## Decision

1. Capture built-in document settings and byte arrays at synchronous admission.
   Reject arbitrary settings subclasses, ambiguous inputs, stdin, native `Out`,
   null PDF objects, and invalid managed numeric values. Check actual native
   setter return codes before conversion, without including values in errors.
2. Bound the whole request lifetime, including managed input and delivery.
   Reject overload immediately. Use bounded in-memory buffering, not temporary
   files. Keep a shared detached-output budget as well as per-request limits.
3. Borrow readable, seekable input streams and writable destination streams until
   the public task finishes. Never dispose them. Read input outside native work;
   copy native output on its thread while the converter exists, then deliver
   asynchronously after destruction. Input readiness determines native queue order.
4. Snapshot configuration at engine construction/DI registration. Native callbacks
   see the request snapshot and stay synchronous, short, contained, and non-reentrant.
   Native finished notifications are not delivery-completion notifications.
5. Keep task-returning operations and wrapper-owned request options public.
   Retain obsolete work-item/visitor APIs for one migration cycle, without
   presenting them as a plugin interface. Bool converters remain compatible.
6. Bind exactly one trusted native module path for the process lifetime.
   Use a DllImport resolver on .NET 8+ and controlled Windows preloading on
   .NET Framework. Do not free a module whose P/Invoke addresses can be cached.
   Native session termination and process-wide native ownership remain separate.
7. Use `AppContext.BaseDirectory` or an explicit absolute library path, reject
   unsupported process architectures, and document unverified native platforms.
   Single-file applications deploy the native binary separately. Trimming/AOT
   and separate wrapper copies in multiple load contexts are not supported.
8. Commit restore lock files and require locked CI restore. Compile README C#
   examples against locally packed packages in a fresh Windows x64 consumer.

## Consequences

- Queued work no longer observes later document/buffer mutations.
- Slow managed I/O does not hold the native thread, but it does consume admission
  and memory budgets. Extra bounded output copying is an intentional safety cost.
- [ADR 0037](0037-aggregate-input-budget.md) adds input reservations;
  [ADR 0038](0038-pipeline-aware-shutdown.md) extends disposal and host stop to
  join the whole pipeline. Callers retain tasks and streams after a timed-out wait.
- Native output limits are applied after native rendering and cannot cap native
  memory, remote resources, or network access.
- There is no claim that cancellation interrupts native execution or that an
  in-process wrapper isolates crashes or untrusted content. Process isolation
  and benchmarks remain separate, optional work.
- Changing the native binary requires a process restart. Recycling does not mean
  unloading/replacing a DLL or eliminating every form of native memory growth.
