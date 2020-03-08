using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AutoFixture;
using Moq;

namespace AdaskoTheBeAsT.WkHtmlToX.Test
{
    public sealed partial class BasicPdfConverterTest
        : IDisposable
    {
        private readonly Fixture _fixture;
        private readonly MockRepository _mockRepository;
        private readonly Mock<IWkHtmlToXModule> _module;
        private readonly Mock<IWkHtmlToPdfModule> _pdfModule;
        private readonly BasicPdfConverter _sut;

        public BasicPdfConverterTest()
        {
            _fixture = new Fixture();
            _mockRepository = new MockRepository(MockBehavior.Loose);

            var moduleFactoryMock = _mockRepository.Create<IWkHtmlToXModuleFactory>();
            _module = _mockRepository.Create<IWkHtmlToXModule>();
            _module.Setup(m => m.Dispose());
            moduleFactoryMock.Setup(mf => mf.GetModule(It.IsAny<ModuleKind>()))
                .Returns(_module.Object);

            var pdfModuleFactoryMock = _mockRepository.Create<IWkHtmlToPdfModuleFactory>();
            _pdfModule = _mockRepository.Create<IWkHtmlToPdfModule>();
            pdfModuleFactoryMock.Setup(mf => mf.GetModule())
                .Returns(_pdfModule.Object);

            _sut = new BasicPdfConverter(moduleFactoryMock.Object, pdfModuleFactoryMock.Object);
        }

        public void Dispose()
        {
            _sut.Dispose();
        }
    }
}
