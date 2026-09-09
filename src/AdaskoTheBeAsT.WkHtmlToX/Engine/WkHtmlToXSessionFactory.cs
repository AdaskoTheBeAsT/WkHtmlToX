using System;
using System.Collections.Generic;
using System.Threading;
using AdaskoTheBeAsT.Interop.Execution;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Modules;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

internal sealed class WkHtmlToXSessionFactory
    : IExecutionSessionFactory<WkHtmlToXSession>
{
    private readonly WkHtmlToXConfiguration _configuration;
    private readonly ILibraryLoaderFactory _libraryLoaderFactory;
    private readonly Func<IPdfProcessor> _pdfProcessorFactory;
    private readonly Func<IImageProcessor> _imageProcessorFactory;
    private readonly NativeRuntimeOwnership _ownership;
    private Thread? _thread;
    private WkHtmlToXSession? _session;

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
        Func<IImageProcessor> imageProcessorFactory,
        NativeRuntimeOwnership? ownership = null)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _libraryLoaderFactory = libraryLoaderFactory ?? throw new ArgumentNullException(nameof(libraryLoaderFactory));
        _pdfProcessorFactory = pdfProcessorFactory ?? throw new ArgumentNullException(nameof(pdfProcessorFactory));
        _imageProcessorFactory = imageProcessorFactory ?? throw new ArgumentNullException(nameof(imageProcessorFactory));
        _ownership = ownership ?? NativeRuntimeOwnership.Shared;
    }

    internal bool IsCurrentThread => ReferenceEquals(Volatile.Read(ref _thread), Thread.CurrentThread);

    public WkHtmlToXSession CreateSession(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ReserveOwnership();
        var thread = Interlocked.CompareExchange(ref _thread, Thread.CurrentThread, comparand: null);
        if ((thread is not null && !ReferenceEquals(thread, Thread.CurrentThread)) || _session is not null)
        {
            throw new InvalidOperationException("The native session must have one dedicated execution thread.");
        }

        var loader = _libraryLoaderFactory.Create(_configuration);
        IPdfProcessor? pdfProcessor = null;
        var pdfInitialized = false;
        try
        {
            loader.Load();
            pdfProcessor = _pdfProcessorFactory();
            var imageProcessor = _imageProcessorFactory();
            if (InitializeModule(pdfProcessor.PdfModule) != 1)
            {
                throw new PdfModuleInitializationException("Pdf module not loaded");
            }

            pdfInitialized = true;
            if (InitializeModule(imageProcessor.ImageModule) != 1)
            {
                throw new ImageModuleInitializationException("Image module not loaded");
            }

            _session = new WkHtmlToXSession(loader, pdfProcessor, imageProcessor);
            return _session;
        }
        catch (Exception exception)
        {
            var failures = new List<Exception>();
            if (pdfInitialized)
            {
                NativeCleanup.Attempt(() => Terminate(pdfProcessor!.PdfModule), failures);
            }

            NativeCleanup.Attempt(loader.Dispose, failures);
            if (failures.Count > 0)
            {
                _ownership.Poison();
                failures.Insert(0, exception);
                throw new AggregateException("Native initialization and rollback failed.", failures);
            }

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
        if (!IsCurrentThread || !ReferenceEquals(_session, session))
        {
            throw new InvalidOperationException("Only the owning thread can dispose the active native session.");
        }

        _session = null;
        var failures = new List<Exception>();
        NativeCleanup.Attempt(() => Terminate(session.ImageProcessor.ImageModule), failures);
        NativeCleanup.Attempt(() => Terminate(session.PdfProcessor.PdfModule), failures);
        NativeCleanup.Attempt(session.Loader.Dispose, failures);
        if (failures.Count > 0)
        {
            _ownership.Poison();
            throw new AggregateException("Native teardown failed. The process cannot safely initialize another native runtime.", failures);
        }
    }

    internal void ReserveOwnership() => _ownership.Acquire(this);

    // Called only after the worker's actual exit, never between recycled sessions.
    internal void ReleaseOwnership() => _ownership.Release(this);

    private static void Terminate(IWkHtmlToXModule module)
    {
        if (module.Terminate() != 1)
        {
            throw new InvalidOperationException("A native module rejected termination.");
        }
    }

    private int InitializeModule(IWkHtmlToXModule module)
    {
        try
        {
            return module.Initialize(0);
        }
        catch
        {
            // A thrown native initialization call may have changed process-global state.
            _ownership.Poison();
            throw;
        }
    }
}
