# WkHtmlToX

🚀 A high-performance, thread-safe C# wrapper for [wkhtmltopdf](https://wkhtmltopdf.org) that converts HTML to PDF and images with ease.

[![CodeFactor](https://www.codefactor.io/repository/github/adaskothebeast/wkhtmltox/badge/master)](https://www.codefactor.io/repository/github/adaskothebeast/wkhtmltox/overview/master)
[![Build Status](https://adaskothebeast.visualstudio.com/AdaskoTheBeAsT.WkHtmlToX/_apis/build/status/AdaskoTheBeAsT.WkHtmlToX?branchName=master)](https://adaskothebeast.visualstudio.com/AdaskoTheBeAsT.WkHtmlToX/_build/latest?definitionId=8&branchName=master)
![Azure DevOps tests](https://img.shields.io/azure-devops/tests/AdaskoTheBeAsT/AdaskoTheBeAsT.WkHtmlToX/18)
![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/AdaskoTheBeAsT/AdaskoTheBeAsT.WkHtmlToX/18?style=plastic)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=AdaskoTheBeAsT_AdaskoTheBeAsT.WkHtmlToX&metric=alert_status)](https://sonarcloud.io/dashboard?id=AdaskoTheBeAsT_AdaskoTheBeAsT.WkHtmlToX)
![Nuget](https://img.shields.io/nuget/dt/AdaskoTheBeAsT.WkHtmlToX)
![Sonar Coverage](https://img.shields.io/sonar/coverage/AdaskoTheBeAsT_AdaskoTheBeAsT.WkHtmlToX?server=https%3A%2F%2Fsonarcloud.io&style=plastic)

## ✨ Features

- **Cross-Platform Support** - Works on Windows (x64/x86), macOS, and Linux (multiple distributions)
- **Thread-Safe** - Built with concurrency in mind using modern .NET patterns
- **Memory Efficient** - Leverages `ArrayPool<byte>` and supports [RecyclableMemoryStream](https://github.com/microsoft/Microsoft.IO.RecyclableMemoryStream)
- **Async/Await** - Fully asynchronous API with `CancellationToken` support
- **Multi-Target** - Supports .NET Framework 4.6.2+, .NET 8.0, .NET 9.0, and .NET 10.0
- **Production Ready** - Comprehensive test coverage and extensive static analysis
- **Event Callbacks** - Track conversion progress, phases, warnings, and errors
- **HTML to PDF** - Convert HTML content, streams, or byte arrays to PDF
- **HTML to Image** - Convert HTML to various image formats

## 📦 Installation

```bash
# Core package
dotnet add package AdaskoTheBeAsT.WkHtmlToX

# Optional: plain Microsoft.Extensions.DependencyInjection integration
dotnet add package AdaskoTheBeAsT.WkHtmlToX.DependencyInjection

# Optional: generic-host (IHostedService) driven lifecycle
dotnet add package AdaskoTheBeAsT.WkHtmlToX.Hosting
```

## 🚀 Quick Start

### Console Application

```csharp
using AdaskoTheBeAsT.WkHtmlToX;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Settings;

var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, null);

using var engine = new WkHtmlToXEngine(configuration);
engine.Initialize();

var converter = new PdfConverter(engine);

// Create document with HTML content
var document = new HtmlToPdfDocument
{
    GlobalSettings = new PdfGlobalSettings
    {
        ColorMode = ColorMode.Color,
        Orientation = Orientation.Portrait,
        PaperSize = PaperKind.A4
    },
    ObjectSettings =
    {
        new PdfObjectSettings
        {
            HtmlContent = "<html><body><h1>Hello World!</h1></body></html>",
            WebSettings = { DefaultEncoding = "utf-8" }
        }
    }
};

// Convert to PDF
using var stream = new FileStream("output.pdf", FileMode.Create);
var result = await converter.ConvertAsync(
    document, 
    _ => stream, 
    CancellationToken.None);

Console.WriteLine(result ? "Success!" : "Failed!");
```

### ASP.NET Core Web API with Dependency Injection

Starting with v11.0.0, DI and Hosting integration is shipped as two small
companion packages built on top of
[`AdaskoTheBeAsT.Interop.Execution`](https://www.nuget.org/packages/AdaskoTheBeAsT.Interop.Execution/).
See the [Migration guide](#-migration-guide-v10x--v1100) below if you are
upgrading from v10.x.

#### Option A - `AdaskoTheBeAsT.WkHtmlToX.Hosting` (recommended)

The hosted service drives worker `InitializeAsync` / `DisposeAsync`
through the generic host, so your app never calls `engine.Initialize()`
manually.

```csharp
// Program.cs
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Hosting;

var builder = WebApplication.CreateBuilder(args);

var wkhtmlConfiguration = new WkHtmlToXConfiguration(
    (int)Environment.OSVersion.Platform,
    runtimeIdentifier: null);

builder.Services.AddWkHtmlToXHostedService(wkhtmlConfiguration, options =>
{
    options.MaxOperationsPerSession = 500;
    options.RecycleSessionOnFailure = true;
});

var app = builder.Build();
app.Run();
```

#### Option B - `AdaskoTheBeAsT.WkHtmlToX.DependencyInjection`

Use this when you are not running on the generic host (e.g. OWIN /
ASP.NET 4.8 on full framework). Call `InitializeAsync` on the resolved
engine during application startup and dispose it on shutdown.

```csharp
using AdaskoTheBeAsT.WkHtmlToX.DependencyInjection;
using AdaskoTheBeAsT.WkHtmlToX.Engine;

services.AddWkHtmlToX(new WkHtmlToXConfiguration(
    (int)Environment.OSVersion.Platform,
    runtimeIdentifier: null));
```

Both packages register `IWkHtmlToXEngine`, `IPdfConverter` and
`IImageConverter` for injection.

#### Controller Implementation

```csharp
[ApiController]
[Route("api/[controller]")]
public class PdfController : ControllerBase
{
    private readonly IPdfConverter _pdfConverter;
    private readonly RecyclableMemoryStreamManager _memoryManager;

    public PdfController(IPdfConverter pdfConverter)
    {
        _pdfConverter = pdfConverter;
        _memoryManager = new RecyclableMemoryStreamManager();
    }

    [HttpPost("convert")]
    public async Task<IActionResult> ConvertToPdf([FromBody] string htmlContent)
    {
        var document = new HtmlToPdfDocument
        {
            GlobalSettings = new PdfGlobalSettings(),
            ObjectSettings = 
            { 
                new PdfObjectSettings { HtmlContent = htmlContent } 
            }
        };

        Stream? stream = null;
        var converted = await _pdfConverter.ConvertAsync(
            document,
            length =>
            {
                stream = _memoryManager.GetStream(
                    Guid.NewGuid(), 
                    "wkhtmltox", 
                    length);
                return stream;
            },
            HttpContext.RequestAborted);

        if (converted && stream != null)
        {
            stream.Position = 0;
            return File(stream, "application/pdf", "output.pdf");
        }

        return BadRequest("Conversion failed");
    }
}
```

## 🔧 Advanced Configuration

### Linux Platform Configuration

For Linux environments, specify the runtime identifier:

```csharp
var configuration = new WkHtmlToXConfiguration(
    (int)PlatformID.Unix, 
    WkHtmlToXRuntimeIdentifier.Ubuntu2004X64);
```

### Supported Linux Distributions

- Ubuntu (14.04, 16.04, 18.04, 20.04) - x64/x86
- Debian (9, 10) - x64/x86
- CentOS (6, 7, 8)
- Amazon Linux 2
- OpenSUSE Leap 15

### Event Callbacks

Monitor conversion progress and handle warnings/errors:

```csharp
var configuration = new WkHtmlToXConfiguration(
    (int)Environment.OSVersion.Platform, 
    null)
{
    PhaseChangedAction = args => 
        Console.WriteLine($"Phase {args.CurrentPhase}/{args.PhaseCount}: {args.Description}"),
    
    ProgressChangedAction = args => 
        Console.WriteLine($"Progress: {args.Description}"),
    
    WarningAction = args => 
        Console.WriteLine($"Warning: {args.Message}"),
    
    ErrorAction = args => 
        Console.WriteLine($"Error: {args.Message}"),
    
    FinishedAction = args => 
        Console.WriteLine($"Finished: {(args.Success ? "Success" : "Failed")}")
};
```

### Custom PDF Settings

```csharp
var document = new HtmlToPdfDocument
{
    GlobalSettings = new PdfGlobalSettings
    {
        ColorMode = ColorMode.Color,
        Orientation = Orientation.Landscape,
        PaperSize = PaperKind.A4,
        Margins = new MarginSettings
        {
            Top = 10,
            Bottom = 10,
            Left = 10,
            Right = 10,
            Unit = Unit.Millimeters
        },
        DocumentTitle = "My Document",
        UseCompression = true
    },
    ObjectSettings =
    {
        new PdfObjectSettings
        {
            HtmlContent = htmlContent,
            WebSettings = 
            { 
                DefaultEncoding = "utf-8",
                EnableJavascript = true,
                LoadImages = true
            },
            HeaderSettings = new SectionSettings
            {
                HtmlContent = "<div>Header Content</div>"
            },
            FooterSettings = new SectionSettings
            {
                HtmlContent = "<div>Page [page] of [toPage]</div>"
            }
        }
    }
};
```

### Memory-Efficient Stream Handling

The library supports custom stream creation, allowing integration with `RecyclableMemoryStream` to minimize memory allocations:

```csharp
var memoryManager = new RecyclableMemoryStreamManager();

Stream? stream = null;
var result = await converter.ConvertAsync(
    document,
    length =>
    {
        // RecyclableMemoryStream reuses memory blocks
        stream = memoryManager.GetStream(Guid.NewGuid(), "wkhtmltox", length);
        return stream;
    },
    cancellationToken);
```

## 🔀 Migration guide (v10.x → v11.0.0)

v11.0.0 is a breaking release that moves infrastructure concerns onto
the shared `AdaskoTheBeAsT.Interop.*` toolbox and splits DI / Hosting
into dedicated packages. There is **no compatibility shim** - follow
this guide when upgrading.

### TL;DR

| Area | v10.x (before) | v11.0.0 (after) |
| --- | --- | --- |
| Target frameworks | `netstandard2.0`, `net8.0`, `net9.0`, `net10.0` | `net462`, `net472`, `net48`, `net481`, `net8.0`, `net9.0`, `net10.0` |
| DI registration | hand-written `AddSingleton<IWkHtmlToXEngine>(...)` in the core package | `AddWkHtmlToX(...)` / `AddWkHtmlToXHostedService(...)` in companion packages |
| Engine worker | custom `BlockingCollection` + STA `Thread` inside `WkHtmlToXEngine` | `ExecutionWorker<WkHtmlToXSession>` from `AdaskoTheBeAsT.Interop.Execution` |
| Native loader | `Loaders/` + `Native/*NativeMethods.cs` | `UnmanagedLibrary` from `AdaskoTheBeAsT.Interop.Unmanaged` |
| `ProgressChangedEventArgs` ctor | `(document, description)` | `(document, progress, description)` + new `Progress` property |
| CI | Azure Pipelines (`azure-pipelines.yml`) | GitHub Actions (`.github/workflows/ci.yml`) |
| Solution format | `AdaskoTheBeAsT.WkHtmlToX.sln` | `AdaskoTheBeAsT.WkHtmlToX.slnx` |

### 1. `netstandard2.0` is dropped

The core package no longer targets `netstandard2.0`. Supported TFMs are
`net462`, `net472`, `net48`, `net481`, `net8.0`, `net9.0`, `net10.0`.
If you still need `netstandard2.0`, stay on the v10.x line.

### 2. DI / Hosting moved to separate packages

In v10.x the recommended pattern was to new up the engine and call
`Initialize()` yourself:

```csharp
// v10.x
services.AddSingleton(sp =>
    new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, null));

services.AddSingleton<IWkHtmlToXEngine>(sp =>
{
    var engine = new WkHtmlToXEngine(
        sp.GetRequiredService<WkHtmlToXConfiguration>());
    engine.Initialize();
    return engine;
});

services.AddSingleton<IPdfConverter, PdfConverter>();
services.AddSingleton<IImageConverter, ImageConverter>();
```

In v11.0.0 pick one of the two companion packages:

```csharp
// v11.0.0 - generic host (recommended for ASP.NET Core / worker services)
services.AddWkHtmlToXHostedService(
    new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, null));

// v11.0.0 - plain DI (OWIN, non-hosted apps)
services.AddWkHtmlToX(
    new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, null));
```

Both extensions register `IWkHtmlToXEngine`, `IPdfConverter` and
`IImageConverter`. Remove any leftover `AddSingleton<IPdfConverter, ...>`
/ `AddSingleton<IImageConverter, ...>` lines - they are done for you.

### 3. `WkHtmlToXEngine` constructor changed

`WkHtmlToXEngine` no longer owns the worker thread itself. It takes an
`IExecutionWorker<WkHtmlToXSession>` and optionally a flag that says
whether it owns the worker lifetime.

```csharp
// v10.x
using var engine = new WkHtmlToXEngine(configuration);
engine.Initialize();

// v11.0.0 - via DI (preferred)
//   resolve IWkHtmlToXEngine from the container; Hosting package drives
//   InitializeAsync / DisposeAsync for you.

// v11.0.0 - manual wiring (console apps / tests)
var loaderFactory = new LibraryLoaderFactory();
var sessionFactory = new WkHtmlToXSessionFactory(configuration, loaderFactory);
await using var worker = new ExecutionWorker<WkHtmlToXSession>(
    sessionFactory,
    new ExecutionWorkerOptions { UseStaThread = true });
await worker.InitializeAsync();
using var engine = new WkHtmlToXEngine(worker, ownsWorker: true);
```

### 4. `ProgressChangedEventArgs` now carries the numeric progress

The progress integer reported by the native callback used to be
ignored. It is now surfaced on the event args.

```csharp
// v10.x
new ProgressChangedEventArgs(document, description);

// v11.0.0
new ProgressChangedEventArgs(document, progress, description);

configuration.ProgressChangedAction = args =>
    Console.WriteLine($"{args.Progress}% - {args.Description}");
```

If you subclassed `ProgressChangedEventArgs` or constructed it yourself
(mocks, custom tests), update the constructor call-sites.

### 5. Removed types

The following internals are gone - delete any references from consumer
code or CI filters:

- `SafeLibraryHandle`, `LoadLibraryFlags`,
  `SystemPosixNativeMethods`, `SystemWindowsNativeMethods` -
  replaced by `AdaskoTheBeAsT.Interop.Unmanaged.UnmanagedLibrary`.
- `AssemblyInfo.cs` - attributes are emitted by the SDK.
- `azure-pipelines.yml`, `.runsettings`, legacy `.sln` -
  replaced by GitHub Actions, `coverage.settings.xml` and `.slnx`.

### 6. Observability

The new execution worker emits `ActivitySource` / `Meter` telemetry
named `AdaskoTheBeAsT.Interop.Execution`. Hook it into OpenTelemetry
with two lines:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t.AddSource("AdaskoTheBeAsT.Interop.Execution"))
    .WithMetrics(m => m.AddMeter("AdaskoTheBeAsT.Interop.Execution"));
```

### 7. See also

- [`docs/plan.md`](docs/plan.md) - the migration plan this release implements.
- [`docs/adr/`](docs/adr) - architecture decision records for every step.

## 🏗️ Architecture

- **Clean Separation** - Abstractions, Engine, Loaders, Modules, and Settings
- **Resource Management** - Proper `IDisposable` implementation with thread-safe disposal
- **Cross-Platform Loading** - Platform-specific library loaders (Windows, Linux, macOS)
- **Worker Thread Pattern** - `ExecutionWorker<WkHtmlToXSession>` from
  `AdaskoTheBeAsT.Interop.Execution` owns the STA thread, queue,
  startup handshake, fault signalling and session recycling
- **Visitor Pattern** - Extensible work item processing

## 🧪 Code Quality

This project maintains high code quality standards with:

- 20+ static code analyzers (Roslyn, SonarAnalyzer, StyleCop, etc.)
- Comprehensive test coverage (unit, integration, memory tests)
- Strict null reference checks enabled
- Treats warnings as errors
- Continuous Integration with GitHub Actions
- SonarCloud / SonarQube quality gate

## 🤝 Contributing

Contributions are welcome! Please ensure:

1. Code follows existing patterns and conventions
2. All tests pass
3. Code analyzers produce no warnings
4. XML documentation is provided for public APIs

## 📄 License

This project is licensed under the terms specified in the [LICENSE](LICENSE) file.

## 🙏 Acknowledgments

This library is built upon [DinkToPdf](https://github.com/rdvojmoc/DinkToPdf) with a completely reworked interoperability layer to address memory management and thread-safety concerns.

## 📚 Additional Resources

- [wkhtmltopdf Documentation](https://wkhtmltopdf.org/usage/wkhtmltopdf.txt)
- [Sample Projects](./samples) - Console, Web API, and OWIN examples
- [Issue Tracker](https://github.com/AdaskoTheBeAsT/WkHtmlToX/issues)

## 💡 Tips & Best Practices

1. **Use the DI / Hosting packages** - prefer `AddWkHtmlToXHostedService`
   (or `AddWkHtmlToX` for non-hosted apps) over newing up the engine
2. **Singleton engine** - never construct more than one
   `WkHtmlToXEngine` in a process; the worker serializes native calls
3. **Session recycling** - set `MaxOperationsPerSession` for long-running
   processes to periodically reset native state
4. **Memory Management** - Use `RecyclableMemoryStream` for high-throughput scenarios
5. **Cancellation** - Always pass `CancellationToken` for responsive cancellation
6. **Linux Deployment** - Ensure correct runtime identifier for your Linux distribution
