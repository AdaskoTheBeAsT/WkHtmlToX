# Plan: Integrating the `AdaskoTheBeAsT.Interop.*` toolbox into `WkHtmlToX`

This plan explains how `WkHtmlToX` can be refactored to sit on top of the four
sibling libraries:

- `AdaskoTheBeAsT.Interop.Execution` (+ DI + Hosting)
- `AdaskoTheBeAsT.Interop.Threading`
- `AdaskoTheBeAsT.Interop.Unmanaged`
- `AdaskoTheBeAsT.Interop.COM`

The current engine already solves the same problems each of those libraries
solves, but the code is hand-rolled, duplicated across projects, and harder to
test. Moving to the shared libraries deletes a lot of plumbing and gives the
project richer observability, deterministic disposal, cross-TFM parity and a
tested fault/recycle story for free.

The notes below refer to a companion migration note that already exists in the
Interop repository at `D:\GitHub\AdaskoTheBeAsT.Interop\wkhtml.md` - that
document is the authoritative sketch of the `ExecutionWorker<TSession>` move
and should be followed in lock-step with this plan.

---

## 1. Current state (baseline)

The WkHtmlToX project already has interop machinery in four places:

| Concern | Current location | What it does |
| --- | --- | --- |
| Dedicated worker thread + queue | `src/AdaskoTheBeAsT.WkHtmlToX/Engine/WkHtmlToXEngine.cs` | Owns a `BlockingCollection<ConvertWorkItemBase>`, a background `Thread` (STA on Windows), a `CancellationTokenSource`, startup handshake via `TaskCompletionSource`, cancel/fail of pending items on disposal. |
| Native library loading | `src/AdaskoTheBeAsT.WkHtmlToX/Loaders/` (`LibraryLoaderBase`, `LibraryLoaderWindows`, `LibraryLoaderLinux`, `LibraryLoaderOsx`, `LibraryLoaderPosix`, `SafeLibraryHandle`) | Loads `wkhtmltox.dll` / `libwkhtmltox.*` from `runtimes/<rid>/native`, implements `SafeHandle`-style cleanup, wraps `LoadLibraryEx` / `dlopen`. |
| Raw P/Invoke to loader APIs | `src/AdaskoTheBeAsT.WkHtmlToX/Native/SystemWindowsNativeMethods.cs`, `SystemPosixNativeMethods.cs`, `LoadLibraryFlags.cs` | Hand-rolled `DllImport` on `kernel32` / `libdl`. |
| Function resolution | `Modules/WkHtmlToPdfModule.cs`, `Modules/WkHtmlToImageModule.cs`, `Modules/WkHtmlToXModule.cs` | Resolves native symbols through the loader and invokes them. |

Everything above is WkHtml-specific today but is, at the pattern level, exactly
what the four `AdaskoTheBeAsT.Interop.*` packages generalize.

---

## 2. Which sibling library helps what

### 2.1 `AdaskoTheBeAsT.Interop.Execution` - replaces the worker thread + queue

The engine's single biggest block of code is generic worker machinery. That is
precisely the problem `ExecutionWorker<TSession>` already solves:

- `BlockingCollection<ConvertWorkItemBase>` -> internal `Channel<ExecutionWorkItem>`
- manual `Thread` with optional `SetApartmentState(ApartmentState.STA)` -> `ExecutionWorkerOptions.UseStaThread`
- startup handshake via `TaskCompletionSource<Exception?>` -> `InitializeAsync` / `Initialize`
- disposal cancel/fail of pending items -> built in
- "the worker is fatally broken" signalling -> `IsFaulted`, `WorkerFaulted`, terminal-once semantics
- session recycling on failure or after N operations -> `ExecutionRequestOptions.RecycleSessionOnFailure` + `MaxOperationsPerSession`
- observability -> `ActivitySource` + `Meter` + `GetSnapshot()`

What the WkHtml engine keeps is a tiny adapter that answers three questions
(see `wkhtml.md` in the Interop repo for the full sketch):

1. How do I create a session? -> load `wkhtmltox`, init PDF + Image modules.
2. How do I dispose a session? -> terminate both modules, release the loader.
3. What work runs on the session? -> `PdfProcessor.Convert(...)` /
   `ImageProcessor.Convert(...)`.

### 2.2 `AdaskoTheBeAsT.Interop.Threading` - replaces ad hoc STA/timeout code

`SingleThreadedApartmentTaskScheduler` gives a reusable STA thread with a real
message pump, proper cancellation composition, per-item timeouts, and a
disposable, DI-registerable instance.

Concrete uses for WkHtmlToX:

- `TaskExtension.TimeoutAfterAsync` on every `ConvertAsync` call so a stuck
  conversion cannot hang a web request forever.
- `MutexHelper.RunInMutex` to serialize access to a single native
  `wkhtmltox` install across processes on a build server or multi-instance
  deployment (`wkhtmltox` is not safe to run from two processes that share
  the same working files).
- `SingleThreadedApartmentTaskScheduler` as an alternative to the
  `ExecutionWorker`'s own STA thread when the host already owns one.
- `StaYield.Occasionally` / `Sleep` if long PDF jobs should keep a message
  pump alive (rarely needed for wkhtmltopdf but free when present).

The Execution worker itself can already flip to STA on Windows, so the
Threading package is complementary rather than mandatory. Pick it up when
the host needs the broader toolset (mutex, timeout, STA scheduler),
otherwise the Execution worker alone is enough.

### 2.3 `AdaskoTheBeAsT.Interop.Unmanaged` - replaces `Loaders/` + `Native/*NativeMethods.cs`

The entire `Loaders/` folder is a re-implementation of what `UnmanagedLibrary`
already ships:

| Current WkHtmlToX type | Replacement in `AdaskoTheBeAsT.Interop.Unmanaged` |
| --- | --- |
| `SafeLibraryHandle` | `SafeLibraryHandle` (same pattern, cross-platform) |
| `LibraryLoaderWindows` (`LoadLibraryEx` + flags) | `new UnmanagedLibrary(path, LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR | LOAD_LIBRARY_SEARCH_SYSTEM32)` |
| `LibraryLoaderLinux`, `LibraryLoaderOsx`, `LibraryLoaderPosix` | `UnmanagedLibrary` (delegates to `NativeLibrary` on `net8+`, raw `dlopen` on Mono) |
| `SystemWindowsNativeMethods` (`LoadLibrary`, `FreeLibrary`, `GetProcAddress`) | internal to `UnmanagedLibrary` - no longer public surface of this repo |
| `SystemPosixNativeMethods` | same |
| `LoadLibraryFlags` | `LoadLibraryFlags` enum |

All wkhtml exports can then be resolved via
`library.GetUnmanagedFunction<TDelegate>("wkhtmltopdf_convert")` (classic
path) or `library.TryGetExport(name, out var addr)` on `net8+` for
`delegate* unmanaged[Cdecl]<...>` call sites.

### 2.4 `AdaskoTheBeAsT.Interop.COM` - not applicable today

`wkhtmltopdf` is a C export surface, not a COM object. The COM package stays
out of scope for the core engine.

It can still be useful at the edges of a consuming application - e.g. if a
legacy Office automation path wants to call a WkHtmlToX-powered rendering
pipeline from inside a COM-hosted add-in - but it does not belong in this
library's dependency graph.

---

## 3. Proposed migration phases

Each phase is independently shippable and reversible. None of them force a
breaking change on consumers at the NuGet level.

### Phase 1 - swap the native loader for `AdaskoTheBeAsT.Interop.Unmanaged`

Scope: only files under `Loaders/` and `Native/`.

Steps:

1. Add a `PackageReference` to `AdaskoTheBeAsT.Interop.Unmanaged`.
2. Replace `SafeLibraryHandle`, `SystemWindowsNativeMethods`,
   `SystemPosixNativeMethods` and `LoadLibraryFlags` with the types shipped
   by the package.
3. Rewrite `LibraryLoaderBase` + per-OS loaders as thin wrappers that
   resolve the right `runtimes/<rid>/native/wkhtmltox.*` path and call
   `new UnmanagedLibrary(path, flags)`. Keep `ILibraryLoader` as the
   seam so nothing else in the engine has to change.
4. Delegate `Release()` / `Dispose()` to the `UnmanagedLibrary` instance.
5. Resolve every wkhtml export through `GetUnmanagedFunction<T>` or
   `TryGetExport`, depending on TFM.

Benefits:

- Deletes all custom `DllImport`s for `kernel32` / `libdl.so.2`.
- Gets `[SupportedOSPlatform("windows")]`-style annotations on the
  Windows-only code paths for free.
- Makes missing exports detectable by `null` / `false` return instead of
  `EntryPointNotFoundException` - useful for forward compatibility with
  future wkhtml builds that rename or remove symbols.
- Cross-platform parity is inherited, not maintained: Linux/macOS loader
  quirks (`RTLD_NOW`, musl, etc.) are now someone else's test matrix.

### Phase 2 - replace the engine worker with `ExecutionWorker<TSession>`

Scope: `Engine/WkHtmlToXEngine.cs`, new `WkHtmlToXSession.cs`, new
`WkHtmlToXSessionFactory.cs`. `IWkHtmlToXEngine` stays as-is so that
`PdfConverter` and `ImageConverter` are unchanged.

Steps (mirroring the `wkhtml.md` sketch in the Interop repo):

1. Add `PackageReference` to `AdaskoTheBeAsT.Interop.Execution`.
2. Introduce `WkHtmlToXSession` holding `ILibraryLoader`,
   `IPdfProcessor` and `IImageProcessor`.
3. Introduce `WkHtmlToXSessionFactory : IExecutionSessionFactory<WkHtmlToXSession>`:
   - `CreateSession` runs today's `InitializeInProcessingThread` body.
   - `DisposeSession` runs today's `CleanupProcessingThreadState` body.
4. Rewrite `WkHtmlToXEngine` as a thin wrapper around
   `ExecutionWorker<WkHtmlToXSession>` configured with
   `useStaThread: true` and `recycleSessionOnFailure: true`.
5. Route `PdfConvertWorkItem` / `ImageConvertWorkItem` to
   `_worker.ExecuteAsync((session, ct) => ...)` and wire the result back
   to each item's `TaskCompletionSource`.
6. Delete everything that is now owned by the worker:
   - `_blockingCollection`, `_cancellationTokenSource`
   - `_workerThread`, `WorkerContext`
   - `Process`, `InitializeInProcessingThread`,
     `CleanupProcessingThreadState`, `CancelPendingWorkItems`,
     `FailPendingWorkItems`
   - the STA-selection code in `Initialize()`
   - finalizer + `_disposeState` bookkeeping (the worker owns this).

Benefits:

- Drops several hundred lines of generic worker code, `Thread` + queue +
  startup handshake + disposal + recycle.
- Adds `ActivitySource` + `Meter` telemetry named
  `AdaskoTheBeAsT.Interop.Execution` that OpenTelemetry can pick up with
  two lines of DI wiring.
- Gains `maxOperationsPerSession` to periodically reset wkhtml state (a
  common mitigation for long-running process memory growth).
- Gains `recycleSessionOnFailure` so one bad conversion does not poison
  all subsequent ones.
- Gains a terminal-once `WorkerFaulted` event - the engine can surface
  "wkhtml is permanently broken in this process" to health checks.
- Optional next step: also expose `ExecutionWorkerPool<WkHtmlToXSession>`
  when each pool worker points at its own `runtimes\<rid>\native` copy,
  for real parallel conversions. See `wkhtml.md` for the trade-offs.

### Phase 3 - optional, DI + Hosting helpers

Scope: the existing `AddSingleton<IWkHtmlToXEngine>` pattern in the README.

Steps:

1. Add `PackageReference`s to
   `AdaskoTheBeAsT.Interop.Execution.DependencyInjection` and
   `AdaskoTheBeAsT.Interop.Execution.Hosting`.
2. Ship a `AddWkHtmlToX(this IServiceCollection services, ...)` extension
   that internally calls
   `services.AddExecutionWorkerHostedService<WkHtmlToXSession>(opts => ...)`
   so `InitializeAsync` / `DisposeAsync` are driven by the generic host.
3. Optional: bind `WkHtmlToXConfiguration` through `IOptions<T>` so
   consumers can configure it from `appsettings.json`.

Benefits:

- Consumers stop writing the "new up the engine and call `Initialize()`
  in DI" boilerplate currently shown in the README.
- Clean shutdown of the native session on application stop becomes a
  framework concern, not the user's.
- Configuration binds naturally to `IOptions`, which plays well with
  validation / `IOptionsMonitor` scenarios.

### Phase 4 - optional, `AdaskoTheBeAsT.Interop.Threading` utilities

Only pull this in when the host app needs one of the below; it does not
improve the core engine on its own.

Candidate uses:

- Expose a `ConvertAsync(..., TimeSpan timeout, CancellationToken)` overload
  on `PdfConverter` / `ImageConverter` that wraps the inner task in
  `TaskExtension.TimeoutAfterAsync`. Today users have to do that manually.
- Use `MutexHelper.RunInMutex("Global\\WkHtmlToX", ...)` around
  `WkHtmlToXSessionFactory.CreateSession` on multi-process build agents
  where concurrent native inits can fight over the same `wkhtmltox.dll`
  working set.
- Replace the worker's own STA thread with an externally-owned
  `ISingleThreadedApartmentTaskScheduler` when the host already runs an
  STA lane. In that mode the Execution worker flips to an MTA thread and
  dispatches each work item through the scheduler.

---

## 4. Concrete benefits summary

| Area | Before (today) | After (migration) |
| --- | --- | --- |
| Lines of custom interop code | `Loaders/`, `Native/`, worker/thread/queue in `WkHtmlToXEngine.cs` | mostly gone - replaced by `UnmanagedLibrary` and `ExecutionWorker<TSession>` |
| Cross-platform loader maintenance | owned in this repo | owned in `AdaskoTheBeAsT.Interop.Unmanaged` with its own test matrix |
| STA on Windows | manual `SetApartmentState` + `#if` guards | single boolean `UseStaThread` |
| Session recycling after failure | not supported | `ExecutionRequestOptions.RecycleSessionOnFailure = true` |
| Periodic native reset | not supported | `ExecutionWorkerOptions.MaxOperationsPerSession` |
| Fault signalling | `Exception` bubbling only | `IsFaulted`, `WorkerFaulted`, terminal-once semantics |
| Telemetry | none | `ActivitySource` + `Meter` named `AdaskoTheBeAsT.Interop.Execution` |
| Timeouts | hand-rolled per caller | `TaskExtension.TimeoutAfterAsync` (with the Threading package) |
| Cross-process mutex | none | `MutexHelper` (with the Threading package) |
| DI / Hosting integration | README snippet | `AddExecutionWorkerHostedService<WkHtmlToXSession>` |
| Testability | must spin up a real engine | inject `IExecutionSessionFactory<WkHtmlToXSession>` or mock `IComExecutor`-style seams per layer |

---

## 5. Risks and things to watch

- `AdaskoTheBeAsT.Interop.Threading` is Windows-only (`net*-windows` TFMs).
  Only add a dependency on it behind the same TFM gate that already applies
  to Windows-only code paths, otherwise the Linux/macOS build will lose
  the Linux loader support.
- `AdaskoTheBeAsT.Interop.Execution` is cross-platform, but
  `UseStaThread: true` is a no-op on non-Windows. Double-check that
  every call site that relies on STA today also relies on it being a
  Windows-only guarantee.
- Moving from `BlockingCollection<ConvertWorkItemBase>` to a
  `Channel<ExecutionWorkItem>` changes observable timing of "queued but
  not yet started" work items. Review any test that asserts on queue
  depth or on exception ordering. The worker's `GetSnapshot()` exposes
  `QueueDepth` for the new world.
- Replacing per-OS loaders with `UnmanagedLibrary` changes the *exception
  type* thrown on load failure (`Win32Exception` wrapping native message
  vs. today's `DllNotLoadedException`). Preserve the public behavior by
  catching inside the new loader wrapper and rethrowing
  `DllNotLoadedException` with the original exception as `InnerException`.
- `AdaskoTheBeAsT.Interop.Execution` targets `net462`..`net10.0`. WkHtmlToX
  targets `netstandard2.0` + `net8.0`/`net9.0`/`net10.0` today. Pick
  compatible TFMs on both sides, or drop `netstandard2.0` in a major
  version bump if it is no longer pulling its weight.

---

## 6. Suggested order of work

1. Phase 1 (Unmanaged loader swap) - low risk, isolated, easy to revert.
2. Phase 2 (ExecutionWorker refactor) - the big win; paired with
   `wkhtml.md` from the Interop repo.
3. Phase 3 (DI + Hosting helpers) - optional polish, strictly additive.
4. Phase 4 (Threading utilities) - opt-in per-feature (timeout overload,
   cross-process mutex, external STA scheduler).
5. Reassess whether an `ExecutionWorkerPool<WkHtmlToXSession>` makes sense
   for high-throughput workloads with isolated native copies.

Each phase can ship as its own minor release with no API break for
existing consumers of `IWkHtmlToXEngine`, `PdfConverter` and
`ImageConverter`.
