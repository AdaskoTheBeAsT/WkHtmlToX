using System;
using System.IO;
using AdaskoTheBeAsT.Interop.Execution;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AdaskoTheBeAsT.WkHtmlToX.Utils;
using Moq;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Engine;

internal sealed class NativeRuntimeTestFixture
{
    internal NativeRuntimeTestFixture(
        WkHtmlToXConfiguration? configuration = null,
        NativeRuntimeOwnership? ownership = null)
    {
        configuration ??= new WkHtmlToXConfiguration(platformId: 0, runtimeIdentifier: null);
        ConfigureModule(Pdf);
        ConfigureModule(Image);
        Pdf.Setup(m => m.CreateObjectSettings()).Returns(new IntPtr(3));
        Pdf.Setup(m => m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(1);
        Pdf.Setup(m => m.AddObject(It.IsAny<IntPtr>(), It.IsAny<IntPtr>(), It.IsAny<byte[]>()));
        Pdf.Setup(m => m.DestroyObjectSetting(It.IsAny<IntPtr>()));
        Loader.Setup(l => l.Load());
        Loader.Setup(l => l.Dispose());
        var loaderFactory = new Mock<ILibraryLoaderFactory>(MockBehavior.Strict);
        loaderFactory.Setup(l => l.Create(It.IsAny<WkHtmlToXConfiguration>())).Returns(Loader.Object);
        Factory = new WkHtmlToXSessionFactory(
            configuration,
            loaderFactory.Object,
            () => new PdfProcessor(configuration, Pdf.Object),
            () => new ImageProcessor(configuration, Image.Object),
            ownership ?? new NativeRuntimeOwnership());
    }

    internal Mock<IWkHtmlToPdfModule> Pdf { get; } = new(MockBehavior.Strict);

    internal Mock<IWkHtmlToImageModule> Image { get; } = new(MockBehavior.Strict);

    internal Mock<ILibraryLoader> Loader { get; } = new(MockBehavior.Strict);

    internal WkHtmlToXSessionFactory Factory { get; }

    internal static HtmlToPdfDocument PdfDocument() =>
        new() { ObjectSettings = { new PdfObjectSettings { HtmlContent = "<html><body>test</body></html>" } } };

    internal static HtmlToImageDocument ImageDocument() =>
        new() { ImageSettings = new ImageSettings { In = "about:blank" } };

#pragma warning disable IDISP004, CA2000 // Ownership transfers to the returned engine.
    internal WkHtmlToXEngine CreateEngine(ExecutionWorkerOptions? options = null, WkHtmlToXRequestOptions? requestOptions = null) =>
        new(new WkHtmlToXWorker(Factory, options ?? new ExecutionWorkerOptions()), ownsWorker: true, requestOptions);
#pragma warning restore IDISP004, CA2000

    private static void ConfigureModule<T>(Mock<T> module)
        where T : class, IWkHtmlToXModule
    {
        module.Setup(m => m.Initialize(It.IsAny<int>())).Returns(1);
        module.Setup(m => m.Terminate()).Returns(1);
        module.Setup(m => m.CreateGlobalSettings()).Returns(new IntPtr(1));
        module.Setup(m => m.CreateConverter(It.IsAny<IntPtr>())).Returns(new IntPtr(2));
        module.Setup(m => m.DestroyConverter(It.IsAny<IntPtr>()));
        module.Setup(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()));
        module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(1);
        module.Setup(m => m.Convert(It.IsAny<IntPtr>())).Returns(value: true);
        module.Setup(m => m.GetHttpErrorCode(It.IsAny<IntPtr>())).Returns(0);
        module.Setup(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()));
        module.Setup(m => m.SetWarningCallback(It.IsAny<IntPtr>(), It.IsAny<StringCallback>()));
        module.Setup(m => m.SetErrorCallback(It.IsAny<IntPtr>(), It.IsAny<StringCallback>()));
        module.Setup(m => m.SetFinishedCallback(It.IsAny<IntPtr>(), It.IsAny<IntCallback>()));
        module.Setup(m => m.SetPhaseChangedCallback(It.IsAny<IntPtr>(), It.IsAny<VoidCallback>()));
        module.Setup(m => m.SetProgressChangedCallback(It.IsAny<IntPtr>(), It.IsAny<IntCallback>()));
        module.Setup(m => m.GetPhaseCount(It.IsAny<IntPtr>())).Returns(1);
        module.Setup(m => m.GetCurrentPhase(It.IsAny<IntPtr>())).Returns(0);
        module.Setup(m => m.GetPhaseDescription(It.IsAny<IntPtr>(), It.IsAny<int>())).Returns("phase");
        module.Setup(m => m.GetProgressDescription(It.IsAny<IntPtr>())).Returns("progress");
    }
}
