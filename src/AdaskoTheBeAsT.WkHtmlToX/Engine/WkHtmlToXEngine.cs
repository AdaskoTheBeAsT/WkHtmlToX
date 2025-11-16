using System;
using System.Collections.Concurrent;
#if NETSTANDARD2_0
using System.Runtime.InteropServices;
#endif
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
#if NET9_0_OR_GREATER
    private static readonly Lock SyncLock = new();
#else
    private static readonly object SyncLock = new();
#endif
    private readonly BlockingCollection<ConvertWorkItemBase> _blockingCollection = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ILibraryLoaderFactory _libraryLoaderFactory;
    private readonly IPdfProcessor _pdfProcessor;
    private readonly IImageProcessor _imageProcessor;
    private readonly WkHtmlToXConfiguration? _configuration;

    private bool _initialized;
    private ILibraryLoader? _libraryLoader;

    // 0 = not disposed, 1 = disposed
    private int _disposeState;
    private Thread? _workerThread;

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

            thread.Name = "WkHtmlToXEngine Worker";
            thread.Start(_cancellationTokenSource.Token);
            _workerThread = thread;
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
#pragma warning disable MA0051 // Method is too long
    internal void Process(object? obj)
    {
#if NETSTANDARD2_0
#pragma warning disable RCS1256 // Invalid argument null check
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }
#pragma warning restore RCS1256 // Invalid argument null check
#endif

#if NET6_0_OR_GREATER
#pragma warning disable RCS1256 // Invalid argument null check.
        ArgumentNullException.ThrowIfNull(obj);
#pragma warning restore RCS1256 // Invalid argument null check.
#endif

        var token = (CancellationToken)obj;
        var initSucceeded = false;

        try
        {
            InitializeInProcessingThread();
            initSucceeded = true;

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
        finally
        {
            if (initSucceeded)
            {
                try
                {
                    _pdfProcessor.PdfModule.Terminate();
                }
                catch
                {
                    /* no op */
                }

                try
                {
                    _imageProcessor.ImageModule.Terminate();
                }
                catch
                {
                    /* no op */
                }

                try
                {
                    var loader = Interlocked.Exchange(ref _libraryLoader, value: null);
                    loader?.Release();
                }
                catch
                {
                    /* no op */
                }
            }

            lock (SyncLock)
            {
                _initialized = false;
            }
        }
#pragma warning restore CC0004 // Catch block cannot be empty
    }
#pragma warning restore MA0051 // Method is too long
#pragma warning restore S108 // Nested blocks of code should not be left empty
#pragma warning restore CA1031 // Do not catch general exception types

    internal void InitializeInProcessingThread()
    {
        // If already disposed, do not proceed.
        if (Volatile.Read(ref _disposeState) != 0)
        {
            throw new ObjectDisposedException(nameof(WkHtmlToXEngine));
        }

#pragma warning disable IDISP003 // Dispose previous before re-assigning.
        var newLoader = _libraryLoaderFactory.Create(_configuration!);
#pragma warning restore IDISP003 // Dispose previous before re-assigning.
        newLoader.Load();

        // If disposal happened while loading, dispose the created loader immediately.
        if (Volatile.Read(ref _disposeState) != 0)
        {
            newLoader.Dispose();
            throw new ObjectDisposedException(nameof(WkHtmlToXEngine));
        }

        // Publish atomically; dispose any previous (defensive).
        var previous = Interlocked.Exchange(ref _libraryLoader, newLoader);
        previous?.Dispose();

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
        // Make Dispose one-shot across all threads (including finalizer)
        if (Interlocked.Exchange(ref _disposeState, 1) != 0)
        {
            return;
        }

        if (disposing)
        {
            _blockingCollection.CompleteAdding();
            _cancellationTokenSource.Cancel();

            try
            {
                _workerThread?.Join();
            }
#pragma warning disable CC0004
            catch
            {
                // ignored
            }
#pragma warning restore CC0004

            _blockingCollection.Dispose();
            _cancellationTokenSource.Dispose();

            // Win the race for loader ownership if the worker didn't already release it
            var loader = Interlocked.Exchange(ref _libraryLoader, value: null);
            loader?.Dispose();
        }
    }
}
