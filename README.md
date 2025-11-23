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
- **Multi-Target** - Supports .NET Standard 2.0, .NET 8.0, .NET 9.0, and .NET 10.0
- **Production Ready** - Comprehensive test coverage and extensive static analysis
- **Event Callbacks** - Track conversion progress, phases, warnings, and errors
- **HTML to PDF** - Convert HTML content, streams, or byte arrays to PDF
- **HTML to Image** - Convert HTML to various image formats

## 📦 Installation

```bash
dotnet add package AdaskoTheBeAsT.WkHtmlToX
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

#### Startup Configuration

```csharp
// Program.cs or Startup.cs
services.AddSingleton(sp =>
{
    var configuration = new WkHtmlToXConfiguration(
        (int)Environment.OSVersion.Platform, 
        null);
    return configuration;
});

services.AddSingleton<IWkHtmlToXEngine>(sp =>
{
    var config = sp.GetRequiredService<WkHtmlToXConfiguration>();
    var engine = new WkHtmlToXEngine(config);
    engine.Initialize();
    return engine;
});

services.AddSingleton<IPdfConverter, PdfConverter>();
services.AddSingleton<IImageConverter, ImageConverter>();
```

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

## 🏗️ Architecture

- **Clean Separation** - Abstractions, Engine, Loaders, Modules, and Settings
- **Resource Management** - Proper `IDisposable` implementation with thread-safe disposal
- **Cross-Platform Loading** - Platform-specific library loaders (Windows, Linux, macOS)
- **Worker Thread Pattern** - Background thread processes conversion queue
- **Visitor Pattern** - Extensible work item processing

## 🧪 Code Quality

This project maintains high code quality standards with:

- 20+ static code analyzers (Roslyn, SonarAnalyzer, StyleCop, etc.)
- Comprehensive test coverage (unit, integration, memory tests)
- Strict null reference checks enabled
- Treats warnings as errors
- Continuous Integration with Azure DevOps
- SonarCloud quality gate

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

1. **Singleton Pattern** - Register `WkHtmlToXEngine` as a singleton in DI containers
2. **Memory Management** - Use `RecyclableMemoryStream` for high-throughput scenarios
3. **Cancellation** - Always pass `CancellationToken` for responsive cancellation
4. **Linux Deployment** - Ensure correct runtime identifier for your Linux distribution
5. **Thread Safety** - The engine handles concurrency internally; don't create multiple engines
