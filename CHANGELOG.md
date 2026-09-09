# Changelog

## 13.0.0 (unreleased)

Major-release hardening of the in-process renderer. See the
[migration guide](README.md#migration-to-1300) before upgrading.

### Added

- `IWkHtmlToXAsyncEngine`: task-returning PDF/image conversions, asynchronous
  initialization/disposal, native queue depth, and terminal health/fault access.
- Structured `ConversionResult` and failure categories, native HTTP errors,
  bounded warnings, and separately retained callback errors.
- `WkHtmlToXRequestOptions`: bounded request admission, inline PDF input size,
  per-request output size, and shared input/output memory budgets.
- `WkHtmlToXWorkerOptions` and `WkHtmlToXShutdownMode`: wrapper-owned worker
  naming, recycling, disposal-wait, and shutdown policy, available through
  configuration for standalone, DI, and hosted engines. Advanced Interop
  configuration delegates remain compatible and take precedence.
- `ShutdownAsync`: whole-pipeline drain/cancel-pending policies with independently
  cancellable waits, used by generic-host stop and engine/provider disposal.
- Parameterless OS-detecting configuration and an explicit trusted native path.
- Request mutation, stream ownership, input/output limits, callback/lifecycle,
  native setting rejection, and package-consumer regression checks.
- Locked CI restores and compilation of every README C# example against freshly
  packed libraries, including a Windows x64 single-file consumer check.

### Fixed

- Independent engines could concurrently enter process-global native state.
  One native owner now spans initialization, conversions, recycling, and actual
  worker exit. A second engine fails before loading native code.
- Application callback exceptions could unwind through reverse P/Invoke.
  They are now contained and reported after converter destruction; native-thread
  engine/lifecycle reentry is rejected.
- Native conversion returning false was indistinguishable from worker success.
  Recovery now uses explicit failure categories instead of incidental exceptions.
- Cleanup suppressed termination failures. All stages are attempted and failures
  remain observable; unsafe partial teardown poisons native ownership.
- Queued documents could observe later mutations. Built-in settings, nested
  dictionaries, encodings, paper sizes, object lists, and byte arrays are copied.
- Input reads and destination writes could block the sole native thread.
  Managed input buffering precedes native execution; bounded output is copied
  before converter destruction, then delivered with managed async I/O.
- Native setting return codes were ignored. Rejected settings now fail without
  exposing their values in the generated error message.
- PDF Page/Xsl input can use a null native data pointer instead of being rejected
  for lacking inline HTML.
- Native loading and static P/Invoke could select different libraries. Modern
  runtimes now resolve imports against the selected, process-lifetime handle.
- Updated to corrected Interop.Execution 2.x contracts for completion ownership,
  disposal joining, terminal faults, non-blocking startup, and diagnostics.
- Aggregate input capacity is reserved before payload copying and held through
  delivery. Input, output, and native worker teardown now share one shutdown;
  container disposal cannot bypass input still draining after a wait timeout.

### Changed

- Supported managed assets are net472/net48/net481/net8.0/net9.0/net10.0.
- Ambiguous input forms, null PDF objects, unsupported settings subclasses,
  stdin, native file/stdout output, invalid enums, and non-finite numeric settings
  are rejected before native work.
- Input streams must be readable and seekable. All streams remain caller-owned
  until actual conversion-task completion, including cancellation/failure.
- Destination factories run outside the native worker and after converter
  destruction. Native `FinishedAction` no longer implies output delivery ended.
  Callback documents are request snapshots, not the original object.
- Configuration is snapshotted at construction/DI registration. The engine/worker
  are singletons; converter facades are transient.
- Default admission is 32 requests; inline PDF input is limited to 32 MiB,
  aggregate inline input to 128 MiB/engine, output to 64 MiB/request, and detached
  output to 128 MiB/engine. Overload and shared-budget failures return structured results.
- The native module stays loaded for the process lifetime. Changing its path
  requires a restart; session teardown/recycling does not unload it.
- Default native search uses the application base directory and its native RID
  layout, not assembly location, current working directory, or system directories.
  ARM/ARM64 processes are rejected; unverified OS/architecture claims are narrowed.
- Dependency ranges exclude future incompatible major versions.
- Interop.Execution minimum is now 2.1.0. Package-consumer checks use matching
  Microsoft.Extensions.Hosting 10.0.12/9.0.20 dependencies.

### Deprecated

- Public work-item/visitor/completion-source APIs and `AddConvertWorkItem`.
  Compatibility shims remain for migration, with removal planned for the next
  major release. Use task-returning engine operations instead.

### Limitations

- Cancellation and host deadlines cannot interrupt a synchronous native call.
- Managed limits do not cap native rendering memory, remote input, or network
  access. No process isolation, hard deadline, or temporary-file spool is added.
- Windows x64 is the native validation environment. Linux/macOS/x86 native
  execution is not established by these release checks.
- Trimming, Native AOT, and multiple wrapper copies in separate load contexts
  are not supported.
