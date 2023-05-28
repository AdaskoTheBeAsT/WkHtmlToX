using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Engine
{
    public partial class PdfProcessorTest
    {
        public static IEnumerable<object?[]> GetTestData()
        {
            const string htmlContent = "<html><head><title>title</title></head><body></body></html>";
            yield return new object?[]
            {
                htmlContent,
                null,
                null,
            };

            var htmlContentByteArray = Encoding.UTF8.GetBytes(htmlContent);
            yield return new object?[]
            {
                null,
                htmlContentByteArray,
                null,
            };

#pragma warning disable IDISP001 // Dispose created.
            var stream = new MemoryStream(htmlContentByteArray);
#pragma warning restore IDISP001 // Dispose created.
            yield return new object?[]
            {
                null,
                null,
                stream,
            };
        }

        [Fact]
        public void GetApplySettingFuncShouldReturnGlobalApplySettingWhenIsGlobalTruePassed()
        {
            // Arrange
            var intVal1 = _fixture.Create<int>();
#pragma warning disable S1854 // Unused assignments should be removed
            var intVal2 = _fixture.Create<int>();
#pragma warning restore S1854 // Unused assignments should be removed
            var name = _fixture.Create<string>();
            var value = _fixture.Create<string>();

            // ReSharper disable once ConvertToLocalFunction
            Func<IntPtr, string, string?, int> setGlobalSetting = (_, _, _) => intVal1;

            // ReSharper disable once ConvertToLocalFunction
            Func<IntPtr, string, string?, int> setObjectSetting = (_, _, _) => intVal2;

            _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                    .Returns(setGlobalSetting);
            _module.Setup(m => m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(setObjectSetting);

            // Act
            var resultFunc = _sut.GetApplySettingFunc(useGlobal: true);
            var result = resultFunc(new IntPtr(1), name, value);

            // Assert
            using (new AssertionScope())
            {
                resultFunc.Should().NotBeNull();
                result.Should().Be(intVal1);
            }
        }

        [Fact]
        public void GetApplySettingFuncShouldReturnObjectApplySettingWhenIsGlobalFalsePassed()
        {
            // Arrange
#pragma warning disable S1854 // Unused assignments should be removed
            var intVal1 = _fixture.Create<int>();
#pragma warning restore S1854 // Unused assignments should be removed
            var intVal2 = _fixture.Create<int>();
            var name = _fixture.Create<string>();
            var value = _fixture.Create<string>();

            // ReSharper disable once ConvertToLocalFunction
            Func<IntPtr, string, string?, int> setGlobalSetting = (_, _, _) => intVal1;

            // ReSharper disable once ConvertToLocalFunction
            Func<IntPtr, string, string?, int> setObjectSetting = (_, _, _) => intVal2;

            _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(setGlobalSetting);
            _module.Setup(m => m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(setObjectSetting);

            // Act
            var resultFunc = _sut.GetApplySettingFunc(useGlobal: false);
            var result = resultFunc(new IntPtr(1), name, value);

            // Assert
            using (new AssertionScope())
            {
                resultFunc.Should().NotBeNull();
                result.Should().Be(intVal2);
            }
        }

        [Fact]
        public void CreateConverterShouldThrowArgumentNullExceptionWhenNullPassed()
        {
            // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

            // ReSharper disable once AssignmentIsFullyDiscarded
            Action action = () => _ = _sut.CreateConverter(document: null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act and Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateConverterShouldInvokeCreateGlobalSettings()
        {
            // Arrange
            var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
            var converterPtr = new IntPtr(_fixture.Create<int>());
            _module.Setup(m =>
                    m.CreateGlobalSettings())
                .Returns(globalSettingsPtr);
            _module.Setup(m =>
                    m.CreateConverter(It.IsAny<IntPtr>()))
                .Returns(converterPtr);
            _module.Setup(
                m =>
                    m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _module.Setup(
                m =>
                    m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            var document = new HtmlToPdfDocument();

            // Act
            var result = _sut.CreateConverter(document);

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(m => m.CreateGlobalSettings(), Times.Once);
                _module.Verify(
                    m =>
                        m.SetGlobalSetting(
                            It.IsAny<IntPtr>(),
                            It.IsAny<string>(),
                            It.IsAny<string?>()),
                    Times.Never);
                _module.Verify(
                    m =>
                        m.SetObjectSetting(
                            It.IsAny<IntPtr>(),
                            It.IsAny<string>(),
                            It.IsAny<string?>()),
                    Times.Never);
                result.converterPtr.Should().Be(converterPtr);
                result.globalSettingsPtr.Should().Be(globalSettingsPtr);
                result.objectSettingsPtrs.Should().NotBeNull();
                result.objectSettingsPtrs.Should().BeEmpty();
            }
        }

        [Fact]
        public void CreateConverterShouldSetGlobalSettings()
        {
            // Arrange
            var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
            var converterPtr = new IntPtr(_fixture.Create<int>());
            _module.Setup(m =>
                    m.CreateGlobalSettings())
                .Returns(globalSettingsPtr);
            _module.Setup(m =>
                    m.CreateConverter(It.IsAny<IntPtr>()))
                .Returns(converterPtr);
            _module.Setup(
                m =>
                    m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _module.Setup(
                m =>
                    m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            var document = new HtmlToPdfDocument();
            var documentTitle = _fixture.Create<string>();
            document.GlobalSettings.DocumentTitle = documentTitle;

            // Act
            var result = _sut.CreateConverter(document);

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(m => m.CreateGlobalSettings(), Times.Once);
                _module.Verify(
                    m =>
                        m.SetGlobalSetting(
                            It.Is<IntPtr>(v => v == globalSettingsPtr),
                            It.Is<string>(v => v == "documentTitle"),
                            It.Is<string?>(v => v == documentTitle)),
                    Times.Once);
                _module.Verify(
                    m =>
                        m.SetObjectSetting(
                            It.IsAny<IntPtr>(),
                            It.IsAny<string>(),
                            It.IsAny<string?>()),
                    Times.Never);
                result.converterPtr.Should().Be(converterPtr);
                result.globalSettingsPtr.Should().Be(globalSettingsPtr);
                result.objectSettingsPtrs.Should().NotBeNull();
                result.objectSettingsPtrs.Should().BeEmpty();
            }
        }

#pragma warning disable MA0051 // Method is too long
        [Fact]
        public void CreateConverterShouldSetObjectSettings()
        {
            // Arrange
            var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
            var objectSettingsPtr = new IntPtr(_fixture.Create<int>());
            var converterPtr = new IntPtr(_fixture.Create<int>());
            _module.Setup(m =>
                    m.CreateGlobalSettings())
                .Returns(globalSettingsPtr);
            _module.Setup(m =>
                    m.CreateConverter(It.IsAny<IntPtr>()))
                .Returns(converterPtr);
            _module.Setup(
                m =>
                    m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _module.Setup(m =>
                    m.CreateObjectSettings())
                .Returns(objectSettingsPtr);
            _module.Setup(
                m =>
                    m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            var document = new HtmlToPdfDocument();
            var documentTitle = _fixture.Create<string>();
            var captionText = _fixture.Create<string>();
            document.GlobalSettings.DocumentTitle = documentTitle;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            document.ObjectSettings.Add(item: null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            document.ObjectSettings.Add(
                new PdfObjectSettings
                {
                    CaptionText = captionText,
                    HtmlContent = "<html><head><title>title</title></head><body></body></html>",
                });

            // Act
            var result = _sut.CreateConverter(document);

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(m => m.CreateGlobalSettings(), Times.Once);
                _module.Verify(
                    m =>
                        m.SetGlobalSetting(
                            It.Is<IntPtr>(v => v == globalSettingsPtr),
                            It.Is<string>(v => v == "documentTitle"),
                            It.Is<string?>(v => v == documentTitle)),
                    Times.Once);
                _module.Verify(m => m.CreateObjectSettings(), Times.Once);
                _module.Verify(
                    m =>
                        m.SetObjectSetting(
                            It.Is<IntPtr>(v => v == objectSettingsPtr),
                            It.Is<string>(v => v == "toc.captionText"),
                            It.Is<string?>(v => v == captionText)),
                    Times.Once);
                result.converterPtr.Should().Be(converterPtr);
                result.globalSettingsPtr.Should().Be(globalSettingsPtr);
                result.objectSettingsPtrs.Should().NotBeNull();
                result.objectSettingsPtrs.Should().HaveCount(1);
                result.objectSettingsPtrs[0].Should().Be(objectSettingsPtr);
            }
        }
#pragma warning restore MA0051 // Method is too long

        [Fact]
        public void ConvertImplShouldThrowExceptionWhenObjectSettingsListEmpty()
        {
            // Arrange
            var document = new HtmlToPdfDocument();
            Action action = () => _sut.Convert(document, _ => Stream.Null);

            // Act and Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ConvertImplShouldThrowExceptionWhenModuleInitializeNotEqualOne()
        {
            // Arrange
            var document = new HtmlToPdfDocument();
            document.ObjectSettings.Add(new PdfObjectSettings());
            _module.Setup(m => m.Initialize(It.IsAny<int>()))
                .Returns(0);

            Action action = () => _sut.Convert(document, _ => Stream.Null);

            // Act and Assert
            action.Should().Throw<HtmlContentEmptyException>();
        }

#pragma warning disable MA0051 // Method is too long
        [Theory]
        [MemberData(nameof(GetTestData))]
        public void ConvertImplShouldReturnStreamWhenConverted(
            string? htmlContent,
            byte[]? htmlContentByteArray,
            Stream? htmlContentStream)
        {
            // Arrange
            using var memoryStream = new MemoryStream();
            var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
            var objectSettingsPtr = new IntPtr(_fixture.Create<int>());
            var converterPtr = new IntPtr(_fixture.Create<int>());
            _module.Setup(m => m.Initialize(It.IsAny<int>()))
                .Returns(1);
            _module.Setup(m =>
                    m.CreateGlobalSettings())
                .Returns(globalSettingsPtr);
            _module.Setup(m =>
                    m.CreateConverter(It.IsAny<IntPtr>()))
                .Returns(converterPtr);
            _module.Setup(
                m =>
                    m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _module.Setup(m => m.Convert(It.IsAny<IntPtr>()))
                .Returns(value: true);
            _module.Setup(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()));
            _module.Setup(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()));
            _module.Setup(m => m.DestroyConverter(It.IsAny<IntPtr>()));
            _module.Setup(m =>
                    m.CreateObjectSettings())
                .Returns(objectSettingsPtr);
            _module.Setup(
                m =>
                    m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _module.Setup(m => m.DestroyObjectSetting(It.IsAny<IntPtr>()));
            var document = new HtmlToPdfDocument();
            var documentTitle = _fixture.Create<string>();
            var captionText = _fixture.Create<string>();
            document.GlobalSettings.DocumentTitle = documentTitle;
            document.ObjectSettings.Add(
                new PdfObjectSettings
                {
                    CaptionText = captionText,
                    HtmlContent = htmlContent,
                    HtmlContentByteArray = htmlContentByteArray,
                    HtmlContentStream = htmlContentStream,
                });

            // Act
            // ReSharper disable once AccessToDisposedClosure
#pragma warning disable IDISP011
            var result = _sut.Convert(document, _ => memoryStream);
#pragma warning restore IDISP011

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(m => m.CreateGlobalSettings(), Times.Once);
                _module.Verify(
                    m =>
                        m.SetGlobalSetting(
                            It.Is<IntPtr>(v => v == globalSettingsPtr),
                            It.Is<string>(v => v == "documentTitle"),
                            It.Is<string?>(v => v == documentTitle)),
                    Times.Once);
                _module.Verify(m => m.CreateObjectSettings(), Times.Once);
                _module.Verify(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()), Times.Once);
                _module.Verify(
                    m =>
                        m.SetObjectSetting(
                            It.Is<IntPtr>(v => v == objectSettingsPtr),
                            It.Is<string>(v => v == "toc.captionText"),
                            It.Is<string?>(v => v == captionText)),
                    Times.Once);
                _module.Verify(m => m.DestroyObjectSetting(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.DestroyConverter(It.IsAny<IntPtr>()), Times.Once);
                result.Should().BeTrue();
            }
        }
#pragma warning restore MA0051 // Method is too long

        [Fact]
        public void AddContentShouldThrowExceptionWhenNullPdfObjectSettingsPassed()
        {
            // Arrange
            var converterPtr = new IntPtr(_fixture.Create<int>());
            var objectSettingsPtr = new IntPtr(_fixture.Create<int>());
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => _sut.AddContent(converterPtr, objectSettingsPtr, pdfObjectSettings: null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act and Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddContentShouldThrowExceptionWhenAllHtmlContentNullPassed()
        {
            // Arrange
            var converterPtr = new IntPtr(_fixture.Create<int>());
            var objectSettingsPtr = new IntPtr(_fixture.Create<int>());
            var pdfObjectSettings = _fixture.Build<PdfObjectSettings>()
                .Without(s => s.HtmlContent)
                .Without(s => s.HtmlContentByteArray)
                .Without(s => s.HtmlContentStream)
                .Create();

            Action action = () => _sut.AddContent(converterPtr, objectSettingsPtr, pdfObjectSettings);

            // Act and Assert
            action.Should().Throw<HtmlContentEmptyException>();
        }

        [Fact]
        public void AddContentStringShouldThrowExceptionWhenNullPassed()
        {
            // Arrange
            var converterPtr = new IntPtr(_fixture.Create<int>());
            var objectSettingsPtr = new IntPtr(_fixture.Create<int>());

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => _sut.AddContentString(converterPtr, objectSettingsPtr, pdfObjectSettings: null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act and Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddContentStringShouldThrowExceptionWhenHtmlContentNullPassed()
        {
            // Arrange
            var converterPtr = new IntPtr(_fixture.Create<int>());
            var objectSettingsPtr = new IntPtr(_fixture.Create<int>());
            var pdfObjectSettings = _fixture.Build<PdfObjectSettings>()
                .Without(s => s.HtmlContent)
                .Without(s => s.HtmlContentByteArray)
                .Without(s => s.HtmlContentStream)
                .Create();

            Action action = () => _sut.AddContentString(converterPtr, objectSettingsPtr, pdfObjectSettings);

            // Act and Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void AddContentStreamShouldThrowExceptionWhenNullPassed()
        {
            // Arrange
            var converterPtr = new IntPtr(_fixture.Create<int>());
            var objectSettingsPtr = new IntPtr(_fixture.Create<int>());

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => _sut.AddContentStream(converterPtr, objectSettingsPtr, htmlContentStream: null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act and Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddContentStreamShouldNotThrowExceptionWhenProperStreamPassed()
        {
            // Arrange
            var converterPtr = new IntPtr(_fixture.Create<int>());
            var objectSettingsPtr = new IntPtr(_fixture.Create<int>());
            const string html = @"<!DOCTYPE html>
                <html lang=""en"">
                <head>
                <meta charset=""utf-8"">
                <title>title</title>
                <link rel=""stylesheet"" href=""style.css"">
                <script src=""script.js""></script>
                </head>
                <body>
                <!-- page content -->
                </body>
                </html>";
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(html));

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

            // ReSharper disable once AccessToDisposedClosure
            Action action = () => _sut.AddContentStream(converterPtr, objectSettingsPtr, memoryStream);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act and Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void AddContentStreamShouldThrowExceptionWhenTooLargeStreamPassed()
        {
            // Arrange
            var converterPtr = new IntPtr(_fixture.Create<int>());
            var objectSettingsPtr = new IntPtr(_fixture.Create<int>());
            var streamMock = new Mock<Stream>();
            streamMock.SetupGet(s => s.Length)
                .Returns(int.MaxValue + 1L);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => _sut.AddContentStream(converterPtr, objectSettingsPtr, streamMock.Object);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act and Assert
            action.Should().Throw<HtmlContentStreamTooLargeException>();
        }

        [Fact]
        public void ConvertShouldThrowExceptionWhenNullDocumentPassed()
        {
            // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => _sut.Convert(document: null, _ => Stream.Null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act and Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ConvertShouldThrowExceptionWhenNullCreateStreamPassed()
        {
            // Arrange
            var document = new HtmlToPdfDocument();
            document.ObjectSettings.Add(new PdfObjectSettings());
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => _sut.Convert(document, createStreamFunc: null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act and Assert
            action.Should().Throw<ArgumentNullException>();
        }

#pragma warning disable MA0051 // Method is too long
        [Fact]
        public void ConvertShouldReturnNullStreamWhenNotConverted()
        {
            // Arrange
            var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
            var objectSettingsPtr = new IntPtr(_fixture.Create<int>());
            var converterPtr = new IntPtr(_fixture.Create<int>());
            _module.Setup(m => m.Initialize(It.IsAny<int>()))
                .Returns(1);
            _module.Setup(m =>
                    m.CreateGlobalSettings())
                .Returns(globalSettingsPtr);
            _module.Setup(m =>
                    m.CreateConverter(It.IsAny<IntPtr>()))
                .Returns(converterPtr);
            _module.Setup(
                m =>
                    m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _module.Setup(m => m.Convert(It.IsAny<IntPtr>()))
                .Returns(value: false);
            _module.Setup(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()));
            _module.Setup(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()));
            _module.Setup(m => m.DestroyConverter(It.IsAny<IntPtr>()));
            _module.Setup(m =>
                    m.CreateObjectSettings())
                .Returns(objectSettingsPtr);
            _module.Setup(
                m =>
                    m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _module.Setup(m => m.DestroyObjectSetting(It.IsAny<IntPtr>()));
            var document = new HtmlToPdfDocument();
            var documentTitle = _fixture.Create<string>();
            var captionText = _fixture.Create<string>();
            document.GlobalSettings.DocumentTitle = documentTitle;
            document.ObjectSettings.Add(
                new PdfObjectSettings
                {
                    CaptionText = captionText,
                    HtmlContent = "<html><head><title>title</title></head><body></body></html>",
                });

            // Act
            var result = _sut.Convert(document, _ => Stream.Null);

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(m => m.CreateGlobalSettings(), Times.Once);
                _module.Verify(
                    m =>
                        m.SetGlobalSetting(
                            It.Is<IntPtr>(v => v == globalSettingsPtr),
                            It.Is<string>(v => v == "documentTitle"),
                            It.Is<string?>(v => v == documentTitle)),
                    Times.Once);
                _module.Verify(m => m.CreateObjectSettings(), Times.Once);
                _module.Verify(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()), Times.Never);
                _module.Verify(
                    m =>
                        m.SetObjectSetting(
                            It.Is<IntPtr>(v => v == objectSettingsPtr),
                            It.Is<string>(v => v == "toc.captionText"),
                            It.Is<string?>(v => v == captionText)),
                    Times.Once);
                _module.Verify(m => m.DestroyObjectSetting(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.DestroyConverter(It.IsAny<IntPtr>()), Times.Once);
                result.Should().BeFalse();
            }
        }
#pragma warning restore MA0051 // Method is too long

#pragma warning disable MA0051 // Method is too long
        [Fact]
        public void ConvertShouldReturnStreamWhenConverted()
        {
            // Arrange
            using var memoryStream = new MemoryStream();
            var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
            var objectSettingsPtr = new IntPtr(_fixture.Create<int>());
            var converterPtr = new IntPtr(_fixture.Create<int>());
            _module.Setup(m => m.Initialize(It.IsAny<int>()))
                .Returns(1);
            _module.Setup(m =>
                    m.CreateGlobalSettings())
                .Returns(globalSettingsPtr);
            _module.Setup(m =>
                    m.CreateConverter(It.IsAny<IntPtr>()))
                .Returns(converterPtr);
            _module.Setup(
                m =>
                    m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _module.Setup(m => m.Convert(It.IsAny<IntPtr>()))
                .Returns(value: true);
            _module.Setup(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()));
            _module.Setup(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()));
            _module.Setup(m => m.DestroyConverter(It.IsAny<IntPtr>()));
            _module.Setup(m =>
                    m.CreateObjectSettings())
                .Returns(objectSettingsPtr);
            _module.Setup(
                m =>
                    m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _module.Setup(m => m.DestroyObjectSetting(It.IsAny<IntPtr>()));
            var document = new HtmlToPdfDocument();
            var documentTitle = _fixture.Create<string>();
            var captionText = _fixture.Create<string>();
            document.GlobalSettings.DocumentTitle = documentTitle;
            document.ObjectSettings.Add(
                new PdfObjectSettings
                {
                    CaptionText = captionText,
                    HtmlContent = "<html><head><title>title</title></head><body></body></html>",
                });

            // Act
            // ReSharper disable once AccessToDisposedClosure
#pragma warning disable IDISP011
            var result = _sut.Convert(document, _ => memoryStream);
#pragma warning restore IDISP011

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(m => m.CreateGlobalSettings(), Times.Once);
                _module.Verify(
                    m =>
                        m.SetGlobalSetting(
                            It.Is<IntPtr>(v => v == globalSettingsPtr),
                            It.Is<string>(v => v == "documentTitle"),
                            It.Is<string?>(v => v == documentTitle)),
                    Times.Once);
                _module.Verify(m => m.CreateObjectSettings(), Times.Once);
                _module.Verify(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()), Times.Once);
                _module.Verify(
                    m =>
                        m.SetObjectSetting(
                            It.Is<IntPtr>(v => v == objectSettingsPtr),
                            It.Is<string>(v => v == "toc.captionText"),
                            It.Is<string?>(v => v == captionText)),
                    Times.Once);
                _module.Verify(m => m.DestroyObjectSetting(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.DestroyConverter(It.IsAny<IntPtr>()), Times.Once);
                result.Should().BeTrue();
            }
        }
#pragma warning restore MA0051 // Method is too long
    }
}
