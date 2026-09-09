using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.EventDefinitions;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Microsoft.IO;
using Reqnroll;

namespace AdaskoTheBeAsT.WkHtmlToX.IntegrationTest.Steps;

[Binding]
[Scope(Feature = nameof(PdfConverter))]
public sealed class PdfConverterSteps
    : IDisposable
{
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
    private readonly List<PhaseChangedEventArgs> _phaseChangedEvents;
    private readonly List<ProgressChangedEventArgs> _progressChangedEvents;
    private readonly List<FinishedEventArgs> _finishedEvents;
    private PdfConverter? _sut;
    private WkHtmlToXEngine? _ownedEngine;
    private string? _htmlContent;
    private HtmlToPdfDocument? _htmlToPdfDocument;

    public PdfConverterSteps()
    {
        _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        _phaseChangedEvents = [];
        _progressChangedEvents = [];
        _finishedEvents = [];
    }

    [Given("I have SynchronizedPdfConverter")]
    public void GivenIHaveSynchronizedPdfConverter()
    {
        CreateSut(new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null));
    }

    [Given("I have SynchronizedPdfConverter with callback tracking")]
    public void GivenIHaveSynchronizedPdfConverterWithCallbackTracking()
    {
        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null)
        {
            PhaseChangedAction = eventArgs => _phaseChangedEvents.Add(eventArgs),
            ProgressChangedAction = eventArgs => _progressChangedEvents.Add(eventArgs),
            FinishedAction = eventArgs => _finishedEvents.Add(eventArgs),
        };

        CreateSut(configuration);
    }

    [Given("I have sample html to convert '(.*)'")]
    public void GivenIHaveSampleHtmlToConvert(string fileName)
    {
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
#pragma warning disable SEC0116 // Path Tampering Unvalidated File Path
        _htmlContent = File.ReadAllText(Path.Combine("./HtmlSamples", fileName));
#pragma warning restore SEC0116 // Path Tampering Unvalidated File Path
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
    }

    [Given("I created HtmlToPdfDocument")]
    public void GivenICreatedHtmlToPdfDocument()
    {
        _htmlToPdfDocument = new HtmlToPdfDocument
        {
            GlobalSettings = new PdfGlobalSettings
            {
                DocumentTitle = "Sample",
            },
            ObjectSettings =
            {
                new PdfObjectSettings
                {
                    HtmlContent = _htmlContent,
                },
            },
        };
    }

    [When("I convert html to pdf (.*) times")]
    public async Task WhenIConvertHtmlToPdfTimesAsync(int count)
    {
        for (var i = 0; i < count; i++)
        {
#pragma warning disable RCS1212 // Remove redundant assignment.
            Stream? stream = null;
#pragma warning disable S8969 // Null-forgiving operators should not be redundant
            await _sut!.ConvertAsync(
                _htmlToPdfDocument!,
                length =>
                {
                    stream = _recyclableMemoryStreamManager.GetStream(
                        Guid.NewGuid(),
                        "wkhtmltox",
                        length);
                    return stream;
                },
                CancellationToken.None).ConfigureAwait(false);
#pragma warning restore S8969 // Null-forgiving operators should not be redundant
#pragma warning restore RCS1212 // Remove redundant assignment.

#if NET8_0_OR_GREATER
#pragma warning disable S2583 // Conditionally executed code should be reachable
            if (stream != null)
            {
                await stream.DisposeAsync().ConfigureAwait(false);
            }
#pragma warning restore S2583 // Conditionally executed code should be reachable
#else
            stream?.Dispose();
#endif
        }
    }

    [Then("pdf lifecycle callbacks should be raised")]
    public void ThenPdfLifecycleCallbacksShouldBeRaised()
    {
        using (new AssertionScope())
        {
            _phaseChangedEvents.Should().NotBeEmpty();
            _phaseChangedEvents.Should().OnlyContain(
                eventArgs => ReferenceEquals(eventArgs.Document, _finishedEvents[0].Document)
                    && eventArgs.PhaseCount > 0
                    && eventArgs.CurrentPhase >= 0);
            _phaseChangedEvents.Should().Contain(eventArgs => !string.IsNullOrWhiteSpace(eventArgs.Description));

            _progressChangedEvents.Should().NotBeEmpty();
            _progressChangedEvents.Should().OnlyContain(
                eventArgs => ReferenceEquals(eventArgs.Document, _finishedEvents[0].Document));
            _progressChangedEvents.Should().Contain(eventArgs => !string.IsNullOrWhiteSpace(eventArgs.Description));

            _finishedEvents.Should().ContainSingle();
            _finishedEvents[0].Document.Should().NotBeSameAs(_htmlToPdfDocument);
            _finishedEvents[0].Document.Should().BeEquivalentTo(_htmlToPdfDocument);
            _finishedEvents[0].Success.Should().BeTrue();
        }
    }

    [Then("proper pdf should be created")]
#pragma warning disable MA0038 // Make method static
    public void ThenProperPdfShouldBeCreated()
    {
        // noop
    }
#pragma warning restore MA0038 // Make method static

    [AfterScenario]
    public void AfterScenario()
    {
        DisposeOwnedEngine();
    }

    public void Dispose()
    {
        DisposeOwnedEngine();
    }

    private void DisposeOwnedEngine()
    {
        _ownedEngine?.Dispose();
        _ownedEngine = null;
    }

    private void CreateSut(WkHtmlToXConfiguration configuration)
    {
        DisposeOwnedEngine();
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
        _ownedEngine = new WkHtmlToXEngine(configuration);
#pragma warning restore IDISP003 // Dispose previous before re-assigning.
        _ownedEngine.Initialize();
        _sut = new PdfConverter(_ownedEngine);
    }
}
