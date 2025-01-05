using System;
using System.Collections.Generic;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.EventDefinitions;
using AdaskoTheBeAsT.WkHtmlToX.Utils;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Engine;

public partial class PdfProcessorTest
{
    private readonly Fixture _fixture;
    private readonly Mock<IWkHtmlToPdfModule> _module;
    private readonly PdfProcessor _sut;

    public PdfProcessorTest()
    {
        _fixture = new Fixture();
        _module = new Mock<IWkHtmlToPdfModule>(MockBehavior.Strict);
        _module.Setup(
            m =>
                m.AddObject(It.IsAny<IntPtr>(), It.IsAny<IntPtr>(), It.IsAny<string>()));
        _module.Setup(
            m =>
                m.AddObject(It.IsAny<IntPtr>(), It.IsAny<IntPtr>(), It.IsAny<byte[]>()));
        _module.Setup(m =>
            m.CreateGlobalSettings())
            .Returns(IntPtr.Zero);
        _module.Setup(m =>
                m.CreateObjectSettings())
            .Returns(IntPtr.Zero);
        _module.Setup(m =>
                m.CreateConverter(It.IsAny<nint>()))
            .Returns(IntPtr.Zero);
        _module.Setup(m =>
                m.SetGlobalSetting(It.IsAny<nint>(), It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(0);
        _sut = new PdfProcessor(
            new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null),
            _module.Object);
    }

    [Fact]
    public void ShouldThrowExceptionWhenNullPassedInModuleConstructor()
    {
        // Arrange

        // ReSharper disable once AssignmentIsFullyDiscarded
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable IDISP004 // Don't ignore created IDisposable.
        Action action = () => _ = new PdfProcessor(
            new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null),
            pdfModule: null);
#pragma warning restore IDISP004 // Don't ignore created IDisposable.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act and Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ShouldAllowToObtainProcessingDocument()
    {
        // Arrange
        var doc = new Mock<ISettings>(MockBehavior.Strict).Object;

        // Act
        _sut.ProcessingDocument = doc;

        // Assert
        _sut.ProcessingDocument.Should().Be(doc);
    }

    [Fact]
    public void RegisterEventsShouldThrowExceptionWhenPointerZeroPassed()
    {
        // Arrange
        Action action = () => _sut.RegisterEvents(IntPtr.Zero);

        // Act and Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RegisterEventsShouldNotRegisterWhereEventsNotAttached()
    {
        // Arrange
        _module.Setup(m => m.SetErrorCallback(It.IsAny<IntPtr>(), It.IsAny<StringCallback>()));
        _module.Setup(m => m.SetWarningCallback(It.IsAny<IntPtr>(), It.IsAny<StringCallback>()))
            .Returns(0);
        _module.Setup(m => m.SetFinishedCallback(It.IsAny<IntPtr>(), It.IsAny<IntCallback>()));
        _module.Setup(m => m.SetPhaseChangedCallback(It.IsAny<IntPtr>(), It.IsAny<VoidCallback>()));
        _module.Setup(m => m.SetProgressChangedCallback(It.IsAny<IntPtr>(), It.IsAny<VoidCallback>()));

        // Act
        _sut.RegisterEvents(new IntPtr(12));

        // Assert
        using (new AssertionScope())
        {
            _module.Verify(
                m =>
                    m.SetErrorCallback(
                        It.IsAny<IntPtr>(),
                        It.IsAny<StringCallback>()),
                Times.Never);
            _module.Verify(
                m =>
                    m.SetWarningCallback(
                        It.IsAny<IntPtr>(),
                        It.IsAny<StringCallback>()),
                Times.Never);
            _module.Verify(
                m =>
                    m.SetFinishedCallback(
                        It.IsAny<IntPtr>(),
                        It.IsAny<IntCallback>()),
                Times.Never);
            _module.Verify(
                m =>
                    m.SetPhaseChangedCallback(
                        It.IsAny<IntPtr>(),
                        It.IsAny<VoidCallback>()),
                Times.Never);
            _module.Verify(
                m =>
                    m.SetProgressChangedCallback(
                        It.IsAny<IntPtr>(),
                        It.IsAny<VoidCallback>()),
                Times.Never);
        }
    }

    [Fact]
    public void RegisterEventsShouldRegisterErrorCallbackWhenSpecified()
    {
        // Arrange
        _module.Setup(m => m.SetErrorCallback(It.IsAny<IntPtr>(), It.IsAny<StringCallback>()))
            .Returns(0);

        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null)
        {
            ErrorAction = _ => { },
        };

        var sut = new PdfProcessor(configuration, _module.Object);

        // Act
        sut.RegisterEvents(new IntPtr(12));

        // Assert
        using (new AssertionScope())
        {
            _module.Verify(
                m =>
                    m.SetErrorCallback(
                        It.IsAny<IntPtr>(),
                        It.IsAny<StringCallback>()),
                Times.Once);
        }
    }

    [Fact]
    public void RegisterEventsShouldRegisterWarningCallbackWhenSpecified()
    {
        // Arrange
        _module.Setup(m => m.SetWarningCallback(It.IsAny<IntPtr>(), It.IsAny<StringCallback>()))
            .Returns(0);

        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null)
        {
            WarningAction = _ => { },
        };

        var sut = new PdfProcessor(configuration, _module.Object);

        // Act
        sut.RegisterEvents(new IntPtr(12));

        // Assert
        using (new AssertionScope())
        {
            _module.Verify(
                m =>
                    m.SetWarningCallback(
                        It.IsAny<IntPtr>(),
                        It.IsAny<StringCallback>()),
                Times.Once);
        }
    }

    [Fact]
    public void RegisterEventsShouldRegisterFinishedCallbackWhenSpecified()
    {
        // Arrange
        _module.Setup(m => m.SetFinishedCallback(It.IsAny<IntPtr>(), It.IsAny<IntCallback>()))
            .Returns(0);

        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null)
        {
            FinishedAction = _ => { },
        };

        var sut = new PdfProcessor(configuration, _module.Object);

        // Act
        sut.RegisterEvents(new IntPtr(12));

        // Assert
        using (new AssertionScope())
        {
            _module.Verify(
                m =>
                    m.SetFinishedCallback(
                        It.IsAny<IntPtr>(),
                        It.IsAny<IntCallback>()),
                Times.Once);
        }
    }

    [Fact]
    public void RegisterEventsShouldRegisterPhaseChangedCallbackWhenSpecified()
    {
        // Arrange
        _module.Setup(m => m.SetPhaseChangedCallback(It.IsAny<IntPtr>(), It.IsAny<VoidCallback>()))
            .Returns(0);

        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null)
        {
            PhaseChangedAction = _ => { },
        };

        var sut = new PdfProcessor(configuration, _module.Object);

        // Act
        sut.RegisterEvents(new IntPtr(12));

        // Assert
        using (new AssertionScope())
        {
            _module.Verify(
                m =>
                    m.SetPhaseChangedCallback(
                        It.IsAny<IntPtr>(),
                        It.IsAny<VoidCallback>()),
                Times.Once);
        }
    }

    [Fact]
    public void RegisterEventsShouldRegisterProgressChangedCallbackWhenSpecified()
    {
        // Arrange
        _module.Setup(m => m.SetProgressChangedCallback(It.IsAny<IntPtr>(), It.IsAny<VoidCallback>()))
            .Returns(0);

        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null)
        {
            ProgressChangedAction = _ => { },
        };

        var sut = new PdfProcessor(configuration, _module.Object);

        // Act
        sut.RegisterEvents(new IntPtr(12));

        // Assert
        using (new AssertionScope())
        {
            _module.Verify(
                m =>
                    m.SetProgressChangedCallback(
                        It.IsAny<IntPtr>(),
                        It.IsAny<VoidCallback>()),
                Times.Once);
        }
    }

    [Fact]
    public void OnErrorShouldNotThrowWhenNoEvent()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();
        Action action = () => _sut.OnError(new IntPtr(1), errorMessage);

        // Act and Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void OnErrorShouldPassDetailsAboutError()
    {
        // Arrange
        var result = default(ErrorEventArgs?);
        var errorMessage = _fixture.Create<string>();
        var doc = new Mock<ISettings>(MockBehavior.Strict).Object;

        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null)
        {
            ErrorAction = eventArgs => result = eventArgs,
        };

        var sut = new PdfProcessor(configuration, _module.Object)
        {
            ProcessingDocument = doc,
        };

        // Act
        sut.OnError(new IntPtr(1), errorMessage);

        // Assert
        using (new AssertionScope())
        {
            result!.Document.Should().Be(doc);
            result!.Message.Should().Be(errorMessage);
        }
    }

    [Fact]
    public void OnWarningShouldNotThrowWhenNoEvent()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();
        Action action = () => _sut.OnWarning(new IntPtr(1), errorMessage);

        // Act and Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void OnWarningShouldPassDetailsAboutError()
    {
        // Arrange
        var result = default(WarningEventArgs?);
        var errorMessage = _fixture.Create<string>();
        var doc = new Mock<ISettings>(MockBehavior.Strict).Object;

        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null)
        {
            WarningAction = eventArgs => result = eventArgs,
        };

        var sut = new PdfProcessor(configuration, _module.Object)
        {
            ProcessingDocument = doc,
        };

        // Act
        sut.OnWarning(new IntPtr(1), errorMessage);

        // Assert
        using (new AssertionScope())
        {
            result!.Document.Should().Be(doc);
            result!.Message.Should().Be(errorMessage);
        }
    }

    [Fact]
    public void OnFinishedShouldNotThrowWhenNoEvent()
    {
        // Arrange
        var code = _fixture.Create<int>();
        Action action = () => _sut.OnFinished(new IntPtr(1), code);

        // Act and Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void OnFinishedShouldPassDetailsAboutError()
    {
        // Arrange
        var result = default(FinishedEventArgs?);
        var code = _fixture.Create<int>();
        var doc = new Mock<ISettings>(MockBehavior.Strict).Object;

        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null)
        {
            FinishedAction = eventArgs => result = eventArgs,
        };

        var sut = new PdfProcessor(configuration, _module.Object)
        {
            ProcessingDocument = doc,
        };

        // Act
        sut.OnFinished(new IntPtr(1), code);

        // Assert
        using (new AssertionScope())
        {
            result!.Document.Should().Be(doc);
            result!.Success.Should().Be(code == 1);
        }
    }

    [Fact]
    public void OnPhaseChangedShouldNotThrowWhenNoEvent()
    {
        // Arrange
        Action action = () => _sut.OnPhaseChanged(new IntPtr(1));

        // Act and Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void OnPhaseChangedShouldPassDetailsAboutError()
    {
        // Arrange
        var phaseCount = _fixture.Create<int>();
        var currentPhase = _fixture.Create<int>();
        var phaseDescription = _fixture.Create<string>();
        _module.Setup(m => m.GetPhaseCount(It.IsAny<IntPtr>()))
            .Returns(phaseCount);
        _module.Setup(m => m.GetCurrentPhase(It.IsAny<IntPtr>()))
            .Returns(currentPhase);
        _module.Setup(m => m.GetPhaseDescription(It.IsAny<IntPtr>(), It.IsAny<int>()))
            .Returns(phaseDescription);

        var result = default(PhaseChangedEventArgs?);
        var doc = new Mock<ISettings>(MockBehavior.Strict).Object;

        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null)
        {
            PhaseChangedAction = eventArgs => result = eventArgs,
        };

        var sut = new PdfProcessor(configuration, _module.Object)
        {
            ProcessingDocument = doc,
        };

        // Act
        sut.OnPhaseChanged(new IntPtr(1));

        // Assert
        using (new AssertionScope())
        {
            result!.Document.Should().Be(doc);
            result!.PhaseCount.Should().Be(phaseCount);
            result!.CurrentPhase.Should().Be(currentPhase);
            result!.Description.Should().Be(phaseDescription);
        }
    }

    [Fact]
    public void OnProgressChangedShouldNotThrowWhenNoEvent()
    {
        // Arrange
        Action action = () => _sut.OnProgressChanged(new IntPtr(1));

        // Act and Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void OnProgressChangedShouldPassDetailsAboutError()
    {
        // Arrange
        var progressDescription = _fixture.Create<string>();
        _module.Setup(m => m.GetProgressDescription(It.IsAny<IntPtr>()))
            .Returns(progressDescription);

        var result = default(ProgressChangedEventArgs?);
        var doc = new Mock<ISettings>(MockBehavior.Strict).Object;

        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null)
        {
            ProgressChangedAction = eventArgs => result = eventArgs,
        };

        var sut = new PdfProcessor(configuration, _module.Object)
        {
            ProcessingDocument = doc,
        };

        // Act
        sut.OnProgressChanged(new IntPtr(1));

        // Assert
        using (new AssertionScope())
        {
            result!.Document.Should().Be(doc);
            result!.Description.Should().Be(progressDescription);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ApplyConfigShouldNotThrowExceptionWhenNullPassedAsSettings(bool useGlobal)
    {
        // Arrange
        var intPtr = new IntPtr(_fixture.Create<int>());
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Action action = () => _sut.ApplyConfig(intPtr, settings: null, useGlobal);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act and Assert
        action.Should().NotThrow();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ApplyConfigShouldSetValuesBasedOnSettings(bool useGlobal)
    {
        // Arrange
        if (useGlobal)
        {
            _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(0);
        }
        else
        {
            _module.Setup(m => m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(0);
        }

        var intPtr = new IntPtr(_fixture.Create<int>());
        const string name = "first";
        var value = _fixture.Create<string>();
        var settings = new TestSettings
        {
            First = value,
        };

        // Act
        _sut.ApplyConfig(intPtr, settings, useGlobal);

        // Assert
        if (useGlobal)
        {
            _module.Verify(
                m =>
                    m.SetGlobalSetting(
                        It.Is<IntPtr>(v => v == intPtr),
                        It.Is<string>(v => v == name),
                        It.Is<string?>(v => v == value)));
        }
        else
        {
            _module.Verify(
                m =>
                    m.SetObjectSetting(
                        It.Is<IntPtr>(v => v == intPtr),
                        It.Is<string>(v => v == name),
                        It.Is<string?>(v => v == value)));
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ApplyConfigShouldNotSetValueIfNullInSettings(bool useGlobal)
    {
        // Arrange
        if (useGlobal)
        {
            _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
        }
        else
        {
            _module.Setup(m => m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
        }

        var intPtr = new IntPtr(_fixture.Create<int>());
        const string name = "first";
        var settings = new TestSettings
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            First = null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        };

        // Act
        _sut.ApplyConfig(intPtr, settings, useGlobal);

        // Assert
        if (useGlobal)
        {
            _module.Verify(
                m =>
                    m.SetGlobalSetting(
                        It.Is<IntPtr>(v => v == intPtr),
                        It.Is<string>(v => v == name),
                        It.Is<string?>(v => v == null)),
                Times.Never);
        }
        else
        {
            _module.Verify(
                m =>
                    m.SetObjectSetting(
                        It.Is<IntPtr>(v => v == intPtr),
                        It.Is<string>(v => v == name),
                        It.Is<string?>(v => v == null)),
                Times.Never);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ApplyConfigShouldSetValuesBasedOnHierarchicalSettings(bool useGlobal)
    {
        // Arrange
        if (useGlobal)
        {
            _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(0);
        }
        else
        {
            _module.Setup(m => m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(0);
        }

        var intPtr = new IntPtr(_fixture.Create<int>());
        const string name = "first";
        var value = _fixture.Create<string>();

        var globalSettings = new TestGlobalSettings
        {
            TestSettings = new TestSettings
            {
                First = value,
            },
        };

        // Act
        _sut.ApplyConfig(intPtr, globalSettings, useGlobal);

        // Assert
        if (useGlobal)
        {
            _module.Verify(
                m =>
                    m.SetGlobalSetting(
                        It.Is<IntPtr>(v => v == intPtr),
                        It.Is<string>(v => v == name),
                        It.Is<string?>(v => v == value)));
        }
        else
        {
            _module.Verify(
                m =>
                    m.SetObjectSetting(
                        It.Is<IntPtr>(v => v == intPtr),
                        It.Is<string>(v => v == name),
                        It.Is<string?>(v => v == value)));
        }
    }

    [Fact]
    public void ApplyShouldThrowArgumentNullExceptionWhenNullAsValuePassed()
    {
        // Arrange
        var intPtr = new IntPtr(_fixture.Create<int>());
        var name = _fixture.Create<string>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Action action = () => _sut.Apply(intPtr, string.Empty, name, value: null, useGlobal: true);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act and Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(false, true, "true")]
    [InlineData(false, false, "false")]
    [InlineData(true, true, "true")]
    [InlineData(true, false, "false")]
    public void ApplyShouldSetProperBooleanValueInConfig(
        bool useGlobal,
        bool value,
        string expected)
    {
        // Arrange
        if (useGlobal)
        {
            _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(0);
        }
        else
        {
            _module.Setup(m => m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(0);
        }

        var intPtr = new IntPtr(_fixture.Create<int>());
        var name = _fixture.Create<string>();

        // Act
        _sut.Apply(intPtr, string.Empty, name, value, useGlobal);

        // Assert
        if (useGlobal)
        {
            _module.Verify(
                m =>
                    m.SetGlobalSetting(
                        It.Is<IntPtr>(v => v == intPtr),
                        It.Is<string>(v => v == name),
                        It.Is<string?>(v => v == expected)));
        }
        else
        {
            _module.Verify(
                m =>
                    m.SetObjectSetting(
                        It.Is<IntPtr>(v => v == intPtr),
                        It.Is<string>(v => v == name),
                        It.Is<string?>(v => v == expected)));
        }
    }

    [Theory]
    [InlineData(false, true, "true")]
    [InlineData(false, false, "false")]
    [InlineData(true, true, "true")]
    [InlineData(true, false, "false")]
    public void ApplyShouldSetProperBooleanValueInConfigWithPrefix(
        bool useGlobal,
        bool value,
        string expected)
    {
        // Arrange
        if (useGlobal)
        {
            _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(0);
        }
        else
        {
            _module.Setup(m => m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(0);
        }

        var intPtr = new IntPtr(_fixture.Create<int>());
        var name = _fixture.Create<string>();
        var prefix = _fixture.Create<string>();
        var nameWithPrefix = $"{prefix}.{name}";

        // Act
        _sut.Apply(intPtr, prefix, name, value, useGlobal);

        // Assert
        if (useGlobal)
        {
            _module.Verify(
                m =>
                    m.SetGlobalSetting(
                        It.Is<IntPtr>(v => v == intPtr),
                        It.Is<string>(v => v == nameWithPrefix),
                        It.Is<string?>(v => v == expected)));
        }
        else
        {
            _module.Verify(
                m =>
                    m.SetObjectSetting(
                        It.Is<IntPtr>(v => v == intPtr),
                        It.Is<string>(v => v == nameWithPrefix),
                        It.Is<string?>(v => v == expected)));
        }
    }

    [Theory]
    [InlineData(false, 2, "2")]
    [InlineData(false, 1.11, "1.11")]
    [InlineData(false, 1.2345, "1.23")]
    [InlineData(false, 1.2378, "1.24")]
    [InlineData(true, 2, "2")]
    [InlineData(true, 1.11, "1.11")]
    [InlineData(true, 1.2345, "1.23")]
    [InlineData(true, 1.2378, "1.24")]
    public void ApplyShouldSetProperDoubleValueInConfig(
        bool useGlobal,
        double value,
        string expected)
    {
        // Arrange
        if (useGlobal)
        {
            _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(0);
        }
        else
        {
            _module.Setup(m => m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(0);
        }

        var intPtr = new IntPtr(_fixture.Create<int>());
        var name = _fixture.Create<string>();

        // Act
        _sut.Apply(intPtr, string.Empty, name, value, useGlobal);

        // Assert
        if (useGlobal)
        {
            _module.Verify(
                m =>
                    m.SetGlobalSetting(
                        It.Is<IntPtr>(v => v == intPtr),
                        It.Is<string>(v => v == name),
                        It.Is<string?>(v => v == expected)));
        }
        else
        {
            _module.Verify(
                m =>
                    m.SetObjectSetting(
                        It.Is<IntPtr>(v => v == intPtr),
                        It.Is<string>(v => v == name),
                        It.Is<string?>(v => v == expected)));
        }
    }

    [Theory]
    [InlineData(false, 2, "2")]
    [InlineData(false, 7, "7")]
    [InlineData(true, 2, "2")]
    [InlineData(true, 7, "7")]
    public void ApplyShouldSetProperIntValueInConfig(
        bool useGlobal,
        int value,
        string expected)
    {
        // Arrange
        if (useGlobal)
        {
            _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(0);
        }
        else
        {
            _module.Setup(m => m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(0);
        }

        var intPtr = new IntPtr(_fixture.Create<int>());
        var name = _fixture.Create<string>();

        // Act
        _sut.Apply(intPtr, string.Empty, name, value, useGlobal);

        // Assert
        if (useGlobal)
        {
            _module.Verify(
                m =>
                    m.SetGlobalSetting(
                        It.Is<IntPtr>(v => v == intPtr),
                        It.Is<string>(v => v == name),
                        It.Is<string?>(v => v == expected)));
        }
        else
        {
            _module.Verify(
                m =>
                    m.SetObjectSetting(
                        It.Is<IntPtr>(v => v == intPtr),
                        It.Is<string>(v => v == name),
                        It.Is<string?>(v => v == expected)));
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ApplyShouldSetProperDictionaryValueInConfig(bool useGlobal)
    {
        // Arrange
        if (useGlobal)
        {
            _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(0);
        }
        else
        {
            _module.Setup(m => m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(0);
        }

        var intPtr = new IntPtr(_fixture.Create<int>());
        var name = _fixture.Create<string>();
        var keyName1 = _fixture.Create<string>();
        var value1 = _fixture.Create<string>();
        var keyName2 = _fixture.Create<string>();
        var dictionary = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            [keyName1] = value1, [keyName2] = null,
        };

        // Act
        _sut.Apply(intPtr, string.Empty, name, dictionary, useGlobal);

        // Assert
        if (useGlobal)
        {
            _module.Verify(
                m =>
                    m.SetGlobalSetting(
                        It.Is<IntPtr>(v => v == intPtr),
                        It.Is<string>(v => v == $"{name}.append"),
                        It.Is<string?>(v => v == null)));
            _module.Verify(
                m =>
                    m.SetGlobalSetting(
                        It.Is<IntPtr>(v => v == intPtr),
                        It.Is<string>(v => v == $"{name}[0]"),
                        It.Is<string?>(v => v == $"{keyName1}\n{value1}")));
            _module.VerifyNoOtherCalls();
        }
        else
        {
            _module.Verify(
                m =>
                    m.SetObjectSetting(
                        It.Is<IntPtr>(v => v == intPtr),
                        It.Is<string>(v => v == $"{name}.append"),
                        It.Is<string?>(v => v == null)));
            _module.Verify(
                m =>
                    m.SetObjectSetting(
                        It.Is<IntPtr>(v => v == intPtr),
                        It.Is<string>(v => v == $"{name}[0]"),
                        It.Is<string?>(v => v == $"{keyName1}\n{value1}")));
            _module.VerifyNoOtherCalls();
        }
    }

#pragma warning disable CA1034 // Nested types should not be visible
    public class TestSettings
        : ISettings
    {
        [WkHtml("first")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string First { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }

    public class TestGlobalSettings
        : ISettings
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public TestSettings TestSettings { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
#pragma warning restore CA1034 // Nested types should not be visible
}
