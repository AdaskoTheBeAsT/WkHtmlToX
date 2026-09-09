using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.EventDefinitions;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Microsoft.IO;
using Reqnroll;

namespace AdaskoTheBeAsT.WkHtmlToX.IntegrationTest.Steps;

[Binding]
[Scope(Feature = nameof(ImageConverter))]
public sealed class ImageConverterStepDefinitions
    : IDisposable
{
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
    private readonly List<PhaseChangedEventArgs> _phaseChangedEvents;
    private readonly List<ProgressChangedEventArgs> _progressChangedEvents;
    private readonly List<FinishedEventArgs> _finishedEvents;
    private ImageConverter? _sut;
    private WkHtmlToXEngine? _ownedEngine;
    private string? _filePath;
    private long _outputLength;
    private HtmlToImageDocument? _htmlToImageDocument;

    public ImageConverterStepDefinitions()
    {
        _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        _phaseChangedEvents = [];
        _progressChangedEvents = [];
        _finishedEvents = [];
    }

    [Given("I have SynchronizedImageConverter")]
    public void GivenIHaveSynchronizedImageConverter()
    {
        CreateSut(new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null));
    }

    [Given("I have SynchronizedImageConverter with callback tracking")]
    public void GivenIHaveSynchronizedImageConverterWithCallbackTracking()
    {
        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null)
        {
            PhaseChangedAction = eventArgs => _phaseChangedEvents.Add(eventArgs),
            ProgressChangedAction = eventArgs => _progressChangedEvents.Add(eventArgs),
            FinishedAction = eventArgs => _finishedEvents.Add(eventArgs),
        };

        CreateSut(configuration);
    }

    [Given("I have sample html to convert '([^']*)'")]
    public void GivenIHaveSampleHtmlToConvert(string fileName)
    {
        _filePath = Path.Combine("./HtmlSamples", fileName);
    }

    [Given("I created HtmlToImageDocument")]
    public void GivenICreatedHtmlToImageDocument()
    {
        _htmlToImageDocument = new HtmlToImageDocument
        {
            ImageSettings =
            {
                Format = "png",
                Quality = "94",
                In = _filePath,
            },
        };
    }

    [When("I convert html to image (.*) times")]
    public async Task WhenIConvertHtmlToImageTimesAsync(int count)
    {
        var converter = _sut ?? throw new InvalidOperationException("The converter has not been initialized.");
        var document = _htmlToImageDocument ?? throw new InvalidOperationException("The image document has not been created.");
        for (var i = 0; i < count; i++)
        {
#if NET8_0_OR_GREATER
            await using var stream = _recyclableMemoryStreamManager.GetStream();
#else
            using var stream = _recyclableMemoryStreamManager.GetStream();
#endif
#pragma warning disable IDISP011 // The engine borrows the destination until the awaited task completes.
            await converter.ConvertAsync(
                    document,
                    _ => stream,
                    CancellationToken.None)
                .ConfigureAwait(false);
#pragma warning restore IDISP011
            _outputLength = stream.Length;
        }
    }

    [Then("image lifecycle callbacks should be raised")]
    public void ThenImageLifecycleCallbacksShouldBeRaised()
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
            _finishedEvents[0].Document.Should().NotBeSameAs(_htmlToImageDocument);
            _finishedEvents[0].Document.Should().BeEquivalentTo(_htmlToImageDocument);
            _finishedEvents[0].Success.Should().BeTrue();
        }
    }

    [Then("proper image should be created")]
    public void ThenProperImageShouldBeCreated()
    {
        _outputLength.Should().BePositive();
    }

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
        _sut = new ImageConverter(_ownedEngine);
    }
}
