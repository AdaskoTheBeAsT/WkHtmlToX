using System;
using System.Threading;
using AdaskoTheBeAsT.Interop.Execution;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Modules;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

internal sealed class WkHtmlToXSessionFactory
    : IExecutionSessionFactory<WkHtmlToXSession>
{
#if NET9_0_OR_GREATER
    private static readonly Lock SyncRoot = new();
#else
    private static readonly object SyncRoot = new();
#endif

    private static int _activeSessionCount;

    private readonly WkHtmlToXConfiguration _configuration;
    private readonly ILibraryLoaderFactory _libraryLoaderFactory;
    private readonly Func<IPdfProcessor> _pdfProcessorFactory;
    private readonly Func<IImageProcessor> _imageProcessorFactory;

    public WkHtmlToXSessionFactory(
        WkHtmlToXConfiguration configuration,
        ILibraryLoaderFactory libraryLoaderFactory)
        : this(
            configuration,
            libraryLoaderFactory,
            () => new PdfProcessor(configuration, new WkHtmlToPdfModule()),
            () => new ImageProcessor(configuration, new WkHtmlToImageModule()))
    {
    }

    internal WkHtmlToXSessionFactory(
        WkHtmlToXConfiguration configuration,
        ILibraryLoaderFactory libraryLoaderFactory,
        Func<IPdfProcessor> pdfProcessorFactory,
        Func<IImageProcessor> imageProcessorFactory)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _libraryLoaderFactory = libraryLoaderFactory ?? throw new ArgumentNullException(nameof(libraryLoaderFactory));
        _pdfProcessorFactory = pdfProcessorFactory ?? throw new ArgumentNullException(nameof(pdfProcessorFactory));
        _imageProcessorFactory = imageProcessorFactory ?? throw new ArgumentNullException(nameof(imageProcessorFactory));
    }

    public WkHtmlToXSession CreateSession(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

#pragma warning disable IDISP003 // Dispose previous before re-assigning.
        var loader = _libraryLoaderFactory.Create(_configuration);
#pragma warning restore IDISP003 // Dispose previous before re-assigning.
        IPdfProcessor? pdfProcessor = null;
        IImageProcessor? imageProcessor = null;

        try
        {
            loader.Load();

            pdfProcessor = _pdfProcessorFactory();
            imageProcessor = _imageProcessorFactory();
            InitializeNativeRuntime(pdfProcessor, imageProcessor);

            return new WkHtmlToXSession(loader, pdfProcessor, imageProcessor);
        }
        catch
        {
            TryIgnore(loader.Dispose);
            throw;
        }
    }

    public void DisposeSession(WkHtmlToXSession session)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(session);
#else
        if (session is null)
        {
            throw new ArgumentNullException(nameof(session));
        }
#endif

        TryIgnore(() => ReleaseNativeRuntime(session));
        TryIgnore(session.Loader.Dispose);
    }

    private static void InitializeNativeRuntime(
        IPdfProcessor pdfProcessor,
        IImageProcessor imageProcessor)
    {
        lock (SyncRoot)
        {
            if (_activeSessionCount > 0)
            {
                _activeSessionCount++;
                return;
            }

            if (pdfProcessor.PdfModule.Initialize(0) != 1)
            {
                throw new PdfModuleInitializationException("Pdf module not loaded");
            }

            try
            {
                if (imageProcessor.ImageModule.Initialize(0) != 1)
                {
                    throw new ImageModuleInitializationException("Image module not loaded");
                }

                _activeSessionCount = 1;
            }
            catch
            {
                TryIgnore(() => pdfProcessor.PdfModule.Terminate());
                throw;
            }
        }
    }

    private static void ReleaseNativeRuntime(WkHtmlToXSession session)
    {
        lock (SyncRoot)
        {
            if (_activeSessionCount == 0)
            {
                return;
            }

            _activeSessionCount--;
            if (_activeSessionCount > 0)
            {
                return;
            }

            TryIgnore(() => session.ImageProcessor.ImageModule.Terminate());
            TryIgnore(() => session.PdfProcessor.PdfModule.Terminate());
        }
    }

    private static void TryIgnore(Action action)
    {
        try
        {
#pragma warning disable CC0031 // Check for null before calling a delegate
            action();
#pragma warning restore CC0031
        }
        catch (Exception)
        {
            GC.KeepAlive(action);
        }
    }
}
