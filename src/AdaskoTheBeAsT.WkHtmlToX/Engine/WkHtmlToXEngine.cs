using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;
using AdaskoTheBeAsT.WkHtmlToX.Modules;
using AdaskoTheBeAsT.WkHtmlToX.WorkItems;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

public sealed class WkHtmlToXEngine
    : IWorkItemVisitor,
        IWkHtmlToXEngine
{
    private static readonly object SyncLock = new();
    private readonly BlockingCollection<ConvertWorkItemBase> _blockingCollection = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ILibraryLoaderFactory _libraryLoaderFactory;
    private readonly IPdfProcessor _pdfProcessor;
    private readonly IImageProcessor _imageProcessor;
    private readonly WkHtmlToXConfiguration? _configuration;

    private bool _initialized;
    private ILibraryLoader? _libraryLoader;
    private bool _disposed;

    public WkHtmlToXEngine(WkHtmlToXConfiguration configuration)
        : this(
            configuration,
            new LibraryLoaderFactory(),
            new PdfProcessor(configuration, new WkHtmlToPdfModule()),
            new ImageProcessor(configuration, new WkHtmlToImageModule()))
    {
    }

    internal WkHtmlToXEngine(
        WkHtmlToXConfiguration configuration,
        ILibraryLoaderFactory libraryLoaderFactory,
        IPdfProcessor pdfProcessor,
        IImageProcessor imageProcessor)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _libraryLoaderFactory = libraryLoaderFactory ?? throw new ArgumentNullException(nameof(libraryLoaderFactory));
        _pdfProcessor = pdfProcessor ?? throw new ArgumentNullException(nameof(pdfProcessor));
        _imageProcessor = imageProcessor ?? throw new ArgumentNullException(nameof(imageProcessor));
    }

#pragma warning disable MA0055 // Do not use destructor
    ~WkHtmlToXEngine()
#pragma warning restore MA0055 // Do not use destructor
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Initialize()
    {
        lock (SyncLock)
        {
            if (_initialized)
            {
                return;
            }

            var thread = new Thread(Process)
            {
                IsBackground = true,
            };

#if NETSTANDARD2_0
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                thread.SetApartmentState(ApartmentState.STA);
            }
#endif

#if NET6_0_OR_GREATER
            if (OperatingSystem.IsWindows())
            {
                thread.SetApartmentState(ApartmentState.STA);
            }
#endif

            thread.Start(_cancellationTokenSource.Token);
            _initialized = true;
        }
    }

    public void AddConvertWorkItem(
        ConvertWorkItemBase item,
        CancellationToken cancellationToken)
    {
        _blockingCollection.Add(item, cancellationToken);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    void IWorkItemVisitor.Visit(PdfConvertWorkItem item)
    {
        try
        {
            var converted = _pdfProcessor.Convert(item.Document, item.StreamFunc);
            item.TaskCompletionSource.SetResult(converted);
        }
        catch (Exception e)
        {
            item.TaskCompletionSource.SetException(e);
        }
    }

    void IWorkItemVisitor.Visit(ImageConvertWorkItem item)
    {
        try
        {
            var converted = _imageProcessor.Convert(item.Document, item.StreamFunc);
            item.TaskCompletionSource.SetResult(converted);
        }
        catch (Exception e)
        {
            item.TaskCompletionSource.SetException(e);
        }
    }

#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable S108 // Nested blocks of code should not be left empty
    internal void Process(object? obj)
    {
#if NETSTANDARD2_0
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }
#endif

#if NET6_0_OR_GREATER
#pragma warning disable RCS1256 // Invalid argument null check.
        ArgumentNullException.ThrowIfNull(obj);
#pragma warning restore RCS1256 // Invalid argument null check.
#endif

        var token = (CancellationToken)obj;

        InitializeInProcessingThread();

        try
        {
            foreach (var convertWorkItem in _blockingCollection.GetConsumingEnumerable(token))
            {
                convertWorkItem.Accept(this);
            }
        }
#pragma warning disable CC0004 // Catch block cannot be empty
        catch (OperationCanceledException)
        {
            // no op
        }
#pragma warning restore CC0004 // Catch block cannot be empty
    }
#pragma warning restore S108 // Nested blocks of code should not be left empty
#pragma warning restore CA1031 // Do not catch general exception types

    internal void InitializeInProcessingThread()
    {
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
        _libraryLoader = _libraryLoaderFactory.Create(_configuration!);
#pragma warning restore IDISP003 // Dispose previous before re-assigning.
        _libraryLoader.Load();

        var pdfModuleLoaded = _pdfProcessor.PdfModule.Initialize(0) == 1;
        if (!pdfModuleLoaded)
        {
            throw new PdfModuleInitializationException("Pdf module not loaded");
        }

        var imageModuleLoaded = _imageProcessor.ImageModule.Initialize(0) == 1;
        if (!imageModuleLoaded)
        {
            throw new ImageModuleInitializationException("Image module not loaded");
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _blockingCollection.CompleteAdding();
                _cancellationTokenSource.Cancel();
                _blockingCollection.Dispose();
                _cancellationTokenSource.Dispose();
                _libraryLoader?.Dispose();
            }

            _disposed = true;
        }
    }
}
