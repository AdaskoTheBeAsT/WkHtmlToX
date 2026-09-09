# WkHtmlToX

A C# wrapper for [wkhtmltopdf](https://wkhtmltopdf.org), with one dedicated
native execution thread for HTML-to-PDF and HTML-to-image conversion.

[![NuGet](https://img.shields.io/nuget/v/AdaskoTheBeAsT.WkHtmlToX)](https://www.nuget.org/packages/AdaskoTheBeAsT.WkHtmlToX)
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=AdaskoTheBeAsT_AdaskoTheBeAsT.WkHtmlToX&metric=alert_status)](https://sonarcloud.io/dashboard?id=AdaskoTheBeAsT_AdaskoTheBeAsT.WkHtmlToX)

## Why this major release is better

Version **13.0.0 (unreleased)** makes ownership and failure handling explicit:

- A second native engine fails fast instead of allowing concurrent access to
  process-global Qt state.
- Application callback exceptions cannot escape into native code. Results
  distinguish invalid input, rendering, stream I/O, callback, and native failures.
- Submitted settings, dictionaries, paper sizes, and byte arrays are snapshotted.
  Later changes cannot silently alter a queued conversion.
- Slow input reads and destination writes no longer occupy the native thread.
  Request admission and aggregate managed input/output buffering have explicit limits.
- Shutdown coordinates the entire pipeline, not just the native queue. Host
  deadlines stop waiting without prematurely releasing streams or native ownership.
- Rejected native settings are reported rather than silently ignored.
- Async lifecycle and health APIs use the corrected Interop.Execution 2.x
  worker. Cleanup failures are observable; incomplete native teardown requires
  a process restart.
- Native loading uses one pinned library binding, not an unload/reload promise
  that conflicts with cached P/Invoke addresses.

See the [migration guide](#migration-to-1300) and [CHANGELOG](CHANGELOG.md).
No throughput improvement is claimed without benchmarks.

## Installation

```shell
dotnet add package AdaskoTheBeAsT.WkHtmlToX
# Choose the native binary for the actual process architecture:
dotnet add package AdaskoTheBeAsT.WkHtmlToX.native.win.x64 --version 0.12.6
# Optional integrations:
dotnet add package AdaskoTheBeAsT.WkHtmlToX.DependencyInjection
dotnet add package AdaskoTheBeAsT.WkHtmlToX.Hosting
```

Keep the three managed packages on the same release. The core package does not
include a renderer binary. Never render untrusted HTML in your application
process: [upstream explicitly warns against it](https://wkhtmltopdf.org/status.html).
A child process is useful for crash containment but is not itself a security sandbox.

## Console example

This complete example is compiled and run by `scripts/verify-packages.ps1`.
Call `ConsoleExample.RunAsync(cancellationToken)` from your application.

```csharp
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

public static class ConsoleExample
{
    public static async Task RunAsync(CancellationToken cancellationToken)
    {
        var configuration = new WkHtmlToXConfiguration
        {
            WorkerOptions = new WkHtmlToXWorkerOptions
            {
                MaxOperationsPerSession = 500,
                DisposeTimeout = TimeSpan.FromSeconds(30),
                ShutdownMode = WkHtmlToXShutdownMode.Drain,
            },
            RequestOptions = new WkHtmlToXRequestOptions
            {
                MaxConcurrentRequests = 16,
                MaxInputBytes = 8 * 1024 * 1024,
                MaxBufferedInputBytes = 32 * 1024 * 1024,
                MaxOutputBytes = 16 * 1024 * 1024,
                MaxBufferedOutputBytes = 64 * 1024 * 1024,
            },
        };
        await using var engine = new WkHtmlToXEngine(configuration);
        await engine.InitializeAsync(cancellationToken);
        var document = new HtmlToPdfDocument
        {
            GlobalSettings = new PdfGlobalSettings { PaperSize = PaperKind.A4 },
            ObjectSettings =
            {
                new PdfObjectSettings { HtmlContent = "<html><body>Hello!</body></html>" },
            },
        };
        using var output = new MemoryStream();
        var result = await engine.ConvertPdfAsync(document, _ => output, cancellationToken);
        if (!result.Success || output.Length == 0)
        {
            throw new InvalidOperationException("PDF conversion failed: " + result.FailureKind);
        }

        // The stream remains yours. Rewind before reading or sending it.
        output.Position = 0;
    }
}
```

`ConvertImageAsync` uses `HtmlToImageDocument.ImageSettings.In` (URL or file path).
For headers and footers, use `SectionSettings.Left`, `Center`, `Right`, or
`HtmlUrl`, not a nonexistent `HtmlContent` setting.

## Generic host

The hosting package initializes the engine and shuts down its whole conversion
pipeline before stopping the native worker. The engine and
worker are singletons; `IPdfConverter` and `IImageConverter` are transient
facades sharing that engine. `IWkHtmlToXEngine` and `IWkHtmlToXAsyncEngine`
resolve to the same singleton.

```csharp
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Hosting;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class HostedExample
{
    public static async Task RunAsync(CancellationToken cancellationToken)
    {
        using var host = new HostBuilder()
            .ConfigureServices(services => services.AddWkHtmlToXHostedService(
                new WkHtmlToXConfiguration
                {
                    WorkerOptions = new WkHtmlToXWorkerOptions { MaxOperationsPerSession = 500 },
                }))
            .Build();
        await host.StartAsync(cancellationToken);
        var engine = host.Services.GetRequiredService<IWkHtmlToXAsyncEngine>();
        using var output = new MemoryStream();
        var result = await engine.ConvertImageAsync(
            new HtmlToImageDocument
            {
                ImageSettings = new ImageSettings { In = "about:blank", Format = "png" },
            },
            _ => output,
            cancellationToken);
        if (!result.Success || output.Length == 0)
        {
            throw new InvalidOperationException("Image conversion failed: " + result.FailureKind);
        }

        await host.StopAsync(cancellationToken);
    }
}
```

For ASP.NET Core, call the same registration extension on `builder.Services`.
Return a completed, rewound stream to the HTTP response, rather than assuming
the native renderer can be interrupted by `HttpContext.RequestAborted`.

## Plain dependency injection

Use this without a generic host. Initialize the async engine at startup.
Dispose the service provider at application shutdown; the shared engine coordinates
pipeline shutdown and worker disposal, regardless of container disposal order.
Do not dispose an injected engine per request.

```csharp
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.DependencyInjection;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using Microsoft.Extensions.DependencyInjection;

public static class DiExample
{
    public static async Task RunAsync(CancellationToken cancellationToken)
    {
        var services = new ServiceCollection();
        services.AddWkHtmlToX(new WkHtmlToXConfiguration());
        await using var provider = services.BuildServiceProvider();
        var engine = provider.GetRequiredService<IWkHtmlToXAsyncEngine>();
        await engine.InitializeAsync(cancellationToken);
        // At application shutdown. Drain is the default; CancelPending skips
        // requests not yet in native execution, without aborting active work.
        await engine.ShutdownAsync(cancellationToken);
    }
}
```

## Request ownership and resource limits

### Admission and input

- Do not mutate a document while calling the conversion method. Once it returns
  its task, built-in settings, object collections, byte arrays, encodings, and
  dictionaries have been copied. Settings subclasses are rejected because the
  library cannot guarantee ownership of arbitrary additional state.
- PDF objects require **exactly one** input: nonempty `HtmlContent`,
  `HtmlContentByteArray`, `HtmlContentStream`, `Page`, or `Xsl`.
  A PDF requires at least one non-null object. Image requests require `In`.
  Standard input (`"-"`) and native file/stdout output (`Out`) are rejected.
- HTML streams are **borrowed**, must be readable and seekable, and are read from
  their current position. Do not read, seek, mutate, or dispose them until the
  conversion task is terminal. Non-seekable input is rejected before native
  initialization; buffer it yourself with an application-defined limit.
- Stream reads use managed async I/O before native admission. Requests reach the
  native queue when input is ready; concurrent requests need not retain submission
  order. The native worker itself still executes one conversion at a time.
- Configuration and callback delegates are snapshotted at engine construction,
  or at DI registration. Changing the original configuration does not reconfigure
  a running engine. Captured state inside application delegates is still yours.

### Output and callbacks

- Native output-pointer copying stays on the owning thread, before converter
  destruction. Only a detached managed buffer crosses the native boundary.
- The destination factory runs on managed thread-pool execution after native
  conversion and destruction. Writes and flushes are awaited outside the worker.
  The library **never disposes** the destination stream.
- Task completion includes actual input processing and output delivery. After
  success, cancellation, or failure, the library no longer uses the borrowed
  streams. A failed or canceled delivery can leave partial output.
- Native progress/phase/finished/warning/error callbacks run on the native thread.
  Keep them short. Do not reenter the engine, initialize/dispose it, or wait for
  another conversion. Reentry is rejected and callback exceptions are contained.
  The callback document is the request snapshot, not the caller's original object.
  Do not block waiting for engine disposal from a destination factory either.
- `FinishedAction` describes native completion, **not** completion of output
  delivery. Await the conversion task for that guarantee.

Default limits (`WkHtmlToXConfiguration.RequestOptions`):

| Option | Default | Scope |
| --- | ---: | --- |
| `MaxConcurrentRequests` | 32 | All admitted requests, including input and slow output |
| `MaxInputBytes` | 32 MiB | Sum of inline HTML bytes across one PDF request |
| `MaxBufferedInputBytes` | 128 MiB | Inline input reservations across the engine |
| `MaxOutputBytes` | 64 MiB | One detached managed output |
| `MaxBufferedOutputBytes` | 128 MiB | Detached output across the engine |

Admission rejects immediately with `Overloaded`; there is no extra unbounded
backlog of requests waiting for a slot. Per-request input limit violations return
`InvalidInput`; aggregate input or output budget violations return `ResourceLimit`.
These are non-recycling failures. All limits must be positive; each shared budget
must cover its corresponding per-request maximum.

The engine reserves the complete PDF input before cloning byte arrays or
allocating stream buffers. It counts encoded `HtmlContent` bytes, byte-array
lengths, and remaining stream lengths. Page/Xsl and image paths reserve no inline
payload. Reservations remain held through output delivery and are released on
success, failure, or cancellation. Input streams that grow beyond the reservation
fail during preparation rather than exceeding the budget.

No temporary-file spooling is used, so spool usage is zero. These limits do
**not** measure total managed heap usage or cap memory already allocated inside
wkhtmltox, caller-held objects, UTF-16/settings strings, temporary marshalling
copies/pools, remote resources, images, files, or network traffic. Native output is
size-checked after rendering, before managed allocation. Budget native memory
separately and use supervised processes for hostile or memory-intensive input.
The optional advanced Interop `QueueCapacity` only bounds the native queue; the
wrapper's request limit also covers input and delivery.

### Worker configuration

Use `WkHtmlToXConfiguration.WorkerOptions` for standalone, DI, and hosted engines.
It uses WkHtmlToX-owned types, without requiring an Interop namespace:

| Option | Default | Contract |
| --- | --- | --- |
| `Name` | `WkHtmlToX Engine Worker` | Diagnostics name, never document data or credentials |
| `MaxOperationsPerSession` | 0 | Nonnegative periodic recycling interval; 0 disables it |
| `DisposeTimeout` | Infinite | Whole-pipeline synchronous disposal wait, from zero through `int.MaxValue` milliseconds, or `Timeout.InfiniteTimeSpan` |
| `ShutdownMode` | `WkHtmlToXShutdownMode.Drain` | Default pipeline policy; `CancelPending` skips requests before native start |

These values are validated and copied before acquiring native ownership or
registering services. Later mutations do not reconfigure the worker. The native
worker uses STA on Windows; this does not supply a Qt message pump.

The existing DI/hosting `configureWorker` delegate remains available for advanced
Interop settings such as diagnostics or native queue capacity. It runs **after**
the wrapper settings and may override them. Prefer `RequestOptions` for admission
limits because those cover the whole pipeline and return structured overload
results. The public constructor and existing shutdown overloads remain compatible.

## Cancellation, shutdown, and health

- A pre-canceled request never starts native work. Managed reads/writes and queued
  execution observe the request token. An active synchronous native conversion
  **cannot** be interrupted; its task remains pending until the native call exits
  and borrowed-resource use ends.
- Canceling `InitializeAsync` cancels that caller's wait, not shared startup.
- `ShutdownAsync` closes admission immediately. `Drain` finishes all admitted
  input preparation, native conversions, and output delivery, then tears down
  the worker. A successfully completed host stop has the same guarantee.
- `CancelPending` skips requests that have not crossed the native-start gate.
  Active stream reads finish before their requests become canceled. Queued tasks
  may wait for the active native delegate to exit before becoming canceled.
  Already-started conversions and their output delivery finish normally unless
  their own request token is canceled.
- Configure the default policy with `WorkerOptions.ShutdownMode`, or use
  `ShutdownAsync(ExecutionShutdownMode.CancelPending, cancellationToken)` explicitly.
  This explicit overload uses the Interop enum and requires
  `using AdaskoTheBeAsT.Interop.Execution;`.
  The first shutdown/disposal call selects the policy; later calls only join it.
- Shutdown and host-stop tokens limit **waiting**, not cleanup ownership. Even
  an already-canceled token starts shutdown. After a canceled/timed-out wait,
  retain conversion tasks and borrowed streams, then join shutdown again.
- `DisposeAsync` joins actual pipeline completion. Synchronous disposal uses
  `WorkerOptions.DisposeTimeout` (infinite by default) for the whole pipeline and can
  return early. Later container/worker disposal cannot bypass that pipeline.
  Do not unload native code or block on shutdown from a stream method/factory
  belonging to an active request.
- Inspect `IsFaulted` and `Fault` for terminal worker/session failure.
  `QueueDepth` is native queue depth, not total managed requests.
  A normal failed conversion does not necessarily fault the engine.

Prefer one engine for the **entire process lifetime**, not one per conversion.
On Windows x64/wkhtmltox 0.12.6, replacing engines on new threads reproduces
Qt `QObject::startTimer` and `QApplication` warnings in both the previous and
current implementations, even though every worker is STA. STA configures COM,
not Qt's thread/event dispatcher. Controlled probes using one engine, including
session recycling on that same thread, did not emit the warnings.
Successful output does not establish that skipped timers are harmless.
See the Qt comparison in [astra_plan.md](astra_plan.md).

`ConversionResult` includes `FailureKind`, `HttpErrorCode`, bounded `Warnings`,
`Exception`, and `CallbackException`. Only `NativeRuntimeError` triggers
failure-based session recycling. A rejected setting, native conversion returning
false, or application stream/callback exception does not automatically recycle.
Warnings and exceptions can contain application data: redact before logging.

Hard deadlines, safe parallel rendering, crash recovery, and untrusted content
require separately supervised renderer processes with filesystem/network/privilege
restrictions. They are not implemented by this in-process package.

## Native deployment and supported targets

Managed targets: `net472`, `net48`, `net481`, `net8.0`, `net9.0`, `net10.0`.
There is no `netstandard2.0` or `net462` asset in this release.

| Native environment | Status |
| --- | --- |
| Windows x64 | Real-native unit/integration and packaged-consumer validation target |
| Windows x86 | Loader retained; not validated in the current release checks |
| Linux x64/x86 | Loader retained; distribution/system dependencies require deployment validation |
| macOS x64 | Loader retained; not validated in the current release checks |
| ARM/ARM64 process | Rejected; bitness alone does not identify a compatible binary |
| .NET Framework outside Windows | Not supported |

Use the parameterless configuration for OS detection. Linux needs either a
legacy `WkHtmlToXRuntimeIdentifier` matching its native package or an explicit
`NativeLibraryPath`. The legacy distribution names are package layout identifiers,
not a promise of support for obsolete distributions.

`NativeLibraryPath` must be an absolute path to a **trusted** binary compatible
with the process architecture. Without it, loaders look under
`AppContext.BaseDirectory/runtimes/<rid>/native/` and then the application base
directory. They do not search the current working directory or `/usr/lib`.
Missing explicit paths do not fall back silently.

On .NET 8+, the assembly's DllImport resolver routes all wkhtmltox imports to the
selected handle. On .NET Framework, Windows preloading requires `wkhtmltox.dll`
and rejects a module already loaded outside this wrapper. The selected library
remains loaded for the process lifetime; changing the path requires a restart.
Session initialization/termination and recycling remain thread-owned.

For single-file deployment, keep the native binary and its dependencies beside
the application or supply `NativeLibraryPath`; automatic extraction paths are
not inferred. Trimming and Native AOT are not supported (settings use reflection).
Do not load multiple copies of this wrapper or wkhtmltox through different
assembly load contexts/AppDomains to circumvent ownership checks.

## Migration to 13.0.0

This is a **major release**. Upgrade the core, DI, and Hosting packages together,
then follow these steps:

1. **Use supported TFMs and dependencies.** Retarget older applications to at
   least .NET Framework 4.7.2 or .NET 8. Interop.Execution dependencies are
   constrained to `[2.1.0,3.0.0)` and Interop.Unmanaged to `[3.0.0,4.0.0)`.
2. **Keep one engine per process.** Reuse the DI singleton across PDF and image
   facades. A second engine now throws, including while the first is recycling
   or still shutting down. Failed native teardown requires a process restart.
3. **Move to `IWkHtmlToXAsyncEngine`.** Use `InitializeAsync`, `ConvertPdfAsync`,
   `ConvertImageAsync`, and `DisposeAsync`, as in the compiled examples above.
   The public `new WkHtmlToXEngine(configuration)` constructor still exists.
   Native sessions, loaders, and worker injection constructors are internal.
4. **Stop constructing work items.** `AddConvertWorkItem`, public work-item
   classes, visitor interfaces, and caller-writable completion sources are
   obsolete migration shims, planned for removal in the next major release.
   They still work with deprecation warnings. The visitor API is not a supported
   plugin mechanism. `PdfConverter`/`ImageConverter` and their `Task<bool>` APIs
   remain available and use the direct async engine when possible.
5. **Handle structured results.** `false` from native conversion becomes
   `ConversionError`, with HTTP status and warnings. Use `Overloaded` and
   `ResourceLimit` for explicit overload handling; do not blindly retry forever.
   Legacy bool converters return false for these outcomes and rethrow retained
   exceptions where applicable. Cancellation still produces a canceled task.
6. **Choose one input form and use built-in settings.** Remove ambiguous inputs,
   null PDF objects, stdin, native `Out`, and custom settings subclasses.
   Invalid combinations are rejected before native startup. Check return-code
   failures for setting names that your native build does not support.
7. **Respect stream lifetimes and execution changes.** Settings and buffers
   become snapshots, but streams remain borrowed until task completion. Factories
   now run off the native thread; `FinishedAction` precedes output delivery.
   Replace reference-equality comparisons against callback documents. Do not
   dispose streams merely because a separate timeout stopped waiting.
8. **Set explicit capacity budgets.** Previously unbounded workloads can now
   receive overload/size failures. Tune `RequestOptions` before construction or
   registration, including the new `MaxBufferedInputBytes` aggregate budget.
   Slow output retains input reservations as well as admission/output capacity,
   not the native worker. No temporary files are created.
9. **Remove `RecycleSessionOnFailure` from worker-option examples.** It is not an
   `ExecutionWorkerOptions` property. This wrapper classifies recovery internally;
   `WorkerOptions.MaxOperationsPerSession` is the periodic-recycling option.
   Configure `WorkerOptions` for the same policy in standalone and hosted engines;
   existing advanced Interop configuration delegates still work and take precedence.
10. **Review native deployment.** Configure a trusted absolute path when needed,
    deploy the correct architecture, and stop relying on ambient OS search paths,
    `Assembly.Location`, or unloading/replacing the native DLL during runtime.
11. **Update lifecycle expectations.** Cleanup waits and fault reporting use the
    corrected Interop 2.x contracts. Host stop now coordinates input and delivery
    before native teardown. Choose drain or cancel-pending deliberately; host
    deadlines bound waiting, not running work. Resolve the async engine interface
    for `InitializeAsync` and `ShutdownAsync`. Prefer asynchronous provider disposal.

For older v10.x applications, DI and Hosting are now separate packages; remove
duplicate hand-written converter registrations. Converters are transient, not
singleton registrations. `ProgressChangedEventArgs` includes numeric `Progress`
and its constructor takes `(document, progress, description)`.

## Development and release checks

- The repository enables analyzers and warnings-as-errors.
- `packages.lock.json` records exact restore graphs; CI uses locked restore.
  Update lock files deliberately with dependency changes, then verify with
  `dotnet restore AdaskoTheBeAsT.WkHtmlToX.slnx --locked-mode`.
- The SDK and test runner are selected by `global.json`. Run a targeted suite
  with `dotnet run --project <test.csproj> --framework net10.0 -- --progress off`.
  This invokes the xUnit v3 Microsoft.Testing.Platform runner directly.
  After dependency upgrades, a .NET Framework `FileLoadException` can indicate
  stale generated binding redirects. Rebuild the affected test target with
  `dotnet build <test.csproj> --framework net481 --no-restore -t:Rebuild`
  (substitute the affected framework), then rerun it.
- `scripts/verify-packages.ps1` packs all three libraries, extracts **every C# code
  block in this README**, compiles them against the local packages in a fresh
  consumer, and runs the examples on Windows x64. It also checks a single-file
  .NET 10 consumer. It never publishes packages.
  If local executable scanning prevents that check, `-SkipSingleFile` explicitly
  skips it while retaining all six normal and explicit-native-path consumer checks.
  The skip is reported and does not establish single-file runtime support.
- [Architecture decisions](docs/adr/README.md) record implementation status.
  [astra_plan.md](astra_plan.md) records validation results and remaining limits.

## License and acknowledgments

See [LICENSE](LICENSE). Built on [DinkToPdf](https://github.com/rdvojmoc/DinkToPdf)
and [wkhtmltopdf](https://wkhtmltopdf.org).
