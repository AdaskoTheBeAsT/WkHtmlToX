using System;
using System.Collections.Concurrent;
#if NETSTANDARD2_0
using System.Runtime.InteropServices;
#endif
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly BlockingCollection<ConvertWorkItemBase> _blockingCollection = [];
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ILibraryLoaderFactory _libraryLoaderFactory;
    private readonly IPdfProcessor _pdfProcessor;
    private readonly IImageProcessor _imageProcessor;
    private readonly WkHtmlToXConfiguration? _configuration;
#if NET9_0_OR_GREATER
    private readonly Lock _lifecycleLock = new();
#else
    private readonly object _lifecycleLock = new();
#endif

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
        TaskCompletionSource<Exception?>? startupCompletionSource = null;
        Thread? workerThread = null;

        lock (SyncLock)
        {
            ThrowIfDisposed();

            if (_initialized)
            {
                return;
            }

            startupCompletionSource = new TaskCompletionSource<Exception?>(TaskCreationOptions.RunContinuationsAsynchronously);
            var workerContext = new WorkerContext(_cancellationTokenSource.Token, startupCompletionSource);

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

#if NET8_0_OR_GREATER
            if (OperatingSystem.IsWindows())
            {
                thread.SetApartmentState(ApartmentState.STA);
            }
#endif

            thread.Name = "WkHtmlToXEngine Worker";
            thread.Start(workerContext);
            _workerThread = thread;
            workerThread = thread;
            _initialized = true;
        }

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        // Safe synchronous wait: the worker signals startup from its dedicated thread without
        // capturing a synchronization context, so this acts as a readiness latch for Initialize.
        var startupException = startupCompletionSource.Task.GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        if (startupException is null)
        {
            return;
        }

        workerThread?.Join();
        ExceptionDispatchInfo.Capture(startupException).Throw();
    }

    public void AddConvertWorkItem(
        ConvertWorkItemBase item,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
#if NETSTANDARD2_0
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }
#endif
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(item);
#endif

        lock (_lifecycleLock)
        {
            ThrowIfDisposed();

            if (_blockingCollection.IsAddingCompleted)
            {
                throw new ObjectDisposedException(nameof(WkHtmlToXEngine));
            }

            try
            {
                _blockingCollection.Add(item, cancellationToken);
            }
            catch (InvalidOperationException) when (_blockingCollection.IsAddingCompleted)
            {
                throw new ObjectDisposedException(nameof(WkHtmlToXEngine));
            }
        }
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
            item.TaskCompletionSource.TrySetResult(converted);
        }
        catch (Exception e)
        {
            item.TaskCompletionSource.TrySetException(e);
        }
    }

    void IWorkItemVisitor.Visit(ImageConvertWorkItem item)
    {
        try
        {
            var converted = _imageProcessor.Convert(item.Document, item.StreamFunc);
            item.TaskCompletionSource.TrySetResult(converted);
        }
        catch (Exception e)
        {
            item.TaskCompletionSource.TrySetException(e);
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

#if NET8_0_OR_GREATER
#pragma warning disable RCS1256 // Invalid argument null check.
        ArgumentNullException.ThrowIfNull(obj);
#pragma warning restore RCS1256 // Invalid argument null check.
#endif

        CancellationToken token;
        TaskCompletionSource<Exception?>? startupCompletionSource;
        if (obj is WorkerContext workerContext)
        {
            token = workerContext.CancellationToken;
            startupCompletionSource = workerContext.StartupCompletionSource;
        }
        else
        {
            token = (CancellationToken)obj;
            startupCompletionSource = null;
        }

        Exception? fatalException = null;

        try
        {
            InitializeInProcessingThread();
            startupCompletionSource?.TrySetResult(null);

            foreach (var convertWorkItem in _blockingCollection.GetConsumingEnumerable(token))
            {
                convertWorkItem.Accept(this);
            }
        }
#pragma warning disable CC0004 // Catch block cannot be empty
        catch (OperationCanceledException exception)
        {
            fatalException = exception;
            startupCompletionSource?.TrySetResult(exception);
        }
        catch (Exception exception)
        {
            fatalException = exception;
            startupCompletionSource?.TrySetResult(exception);
        }
        finally
        {
            if (fatalException is OperationCanceledException)
            {
                CancelPendingWorkItems();
            }
            else if (fatalException != null)
            {
                FailPendingWorkItems(fatalException);
            }

            CleanupProcessingThreadState();

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

#pragma warning disable MA0051 // Method is too long
    internal void InitializeInProcessingThread()
    {
        ThrowIfDisposed();

#pragma warning disable IDISP003 // Dispose previous before re-assigning.
        var newLoader = _libraryLoaderFactory.Create(_configuration!);
#pragma warning restore IDISP003 // Dispose previous before re-assigning.
        var loaderPublished = false;
        var pdfModuleLoaded = false;
        var imageModuleLoaded = false;

        try
        {
            newLoader.Load();

            // If disposal happened while loading, dispose the created loader immediately.
            ThrowIfDisposed();

            // Publish atomically; release any previous (defensive).
            var previous = Interlocked.Exchange(ref _libraryLoader, newLoader);
            loaderPublished = true;
            SafeReleaseLoader(previous);

            var pdfModuleInitialized = _pdfProcessor.PdfModule.Initialize(0) == 1;
            if (!pdfModuleInitialized)
            {
                throw new PdfModuleInitializationException("Pdf module not loaded");
            }

            pdfModuleLoaded = true;

            var imageModuleInitialized = _imageProcessor.ImageModule.Initialize(0) == 1;
            if (!imageModuleInitialized)
            {
                throw new ImageModuleInitializationException("Image module not loaded");
            }

            imageModuleLoaded = true;
        }
        catch
        {
            if (imageModuleLoaded)
            {
                TryIgnore(() => _imageProcessor.ImageModule.Terminate());
            }

            if (pdfModuleLoaded)
            {
                TryIgnore(() => _pdfProcessor.PdfModule.Terminate());
            }

            if (loaderPublished)
            {
                var loader = Interlocked.Exchange(ref _libraryLoader, value: null);
                SafeReleaseLoader(loader);
            }
            else
            {
                SafeReleaseLoader(newLoader);
            }

            throw;
        }
    }
#pragma warning restore MA0051 // Method is too long

    private static void SafeReleaseLoader(ILibraryLoader? loader)
    {
        if (loader is null)
        {
            return;
        }

        TryIgnore(loader.Release);
    }

    private static void TryIgnore(Action action)
    {
#if NETSTANDARD2_0
        if (action == null)
        {
            return;
        }
#endif
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(action);
#endif
        try
        {
            action();
        }
        catch (Exception)
        {
            GC.KeepAlive(action);
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
            lock (_lifecycleLock)
            {
                if (!_blockingCollection.IsAddingCompleted)
                {
                    _blockingCollection.CompleteAdding();
                }
            }

            _cancellationTokenSource.Cancel();

            TryIgnore(() =>
            {
                _workerThread?.Join();
            });

            CancelPendingWorkItems();

            _blockingCollection.Dispose();
            _cancellationTokenSource.Dispose();

            // Win the race for loader ownership if the worker didn't already release it
            var loader = Interlocked.Exchange(ref _libraryLoader, value: null);
            SafeReleaseLoader(loader);
        }
    }

    private void CancelPendingWorkItems()
    {
        while (_blockingCollection.TryTake(out var pendingItem))
        {
            pendingItem.TaskCompletionSource.TrySetCanceled();
        }
    }

    private void FailPendingWorkItems(Exception exception)
    {
        while (_blockingCollection.TryTake(out var pendingItem))
        {
            pendingItem.TaskCompletionSource.TrySetException(exception);
        }
    }

    private void CleanupProcessingThreadState()
    {
        TryIgnore(() => _pdfProcessor.PdfModule.Terminate());
        TryIgnore(() => _imageProcessor.ImageModule.Terminate());
        TryIgnore(() =>
        {
            var loader = Interlocked.Exchange(ref _libraryLoader, value: null);
            SafeReleaseLoader(loader);
        });
    }

    private void ThrowIfDisposed()
    {
#if NET8_0_OR_GREATER
        var disposed = Volatile.Read(ref _disposeState) != 0;
        ObjectDisposedException.ThrowIf(disposed, this);
#endif
#if NETSTANDARD2_0
        if (Volatile.Read(ref _disposeState) != 0)
        {
            throw new ObjectDisposedException(nameof(WkHtmlToXEngine));
        }
#endif
    }

    private sealed class WorkerContext
    {
        public WorkerContext(
            CancellationToken cancellationToken,
            TaskCompletionSource<Exception?> startupCompletionSource)
        {
            CancellationToken = cancellationToken;
            StartupCompletionSource = startupCompletionSource;
        }

        public CancellationToken CancellationToken { get; }

        public TaskCompletionSource<Exception?> StartupCompletionSource { get; }
    }
}
