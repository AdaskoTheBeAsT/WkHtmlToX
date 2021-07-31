# WkHtmlToX

C# wrapper for wkhtmltopdf.org Html to Pdf and Image library.

In web api (in combination with SimpleInjector) registration should be as follows:

```cs
   // for simplicity only version for win here
    var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, null);
    _container.RegisterInstance(configuration);
    _container.RegisterSingleton<IWkHtmlToXEngine, WkHtmlToXEngine>();
    _container.RegisterSingleton<IPdfConverter, PdfConverter>();
    _container.RegisterInitializer<IWkHtmlToXEngine>(e => e.Initialize());
```

In command line application:

```cs
    // for simplicity only version for win here
    var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, null);
    using (var engine = new WkHtmlToXEngine(configuration))
    {
        engine.Initialize();

        var converter = new PdfConverter(engine);
    }
```

Method for conversion to pdf takes 3 parameters.
First is settings object in which there is possibility to pass html content to be converted.
In second parameter you need to pass func which based on length will create stream.
In third you need to pass CancellationToken.

Second parameter is tricky but such construction allows to use for example [Microsoft.IO.RecyclableMemoryStream](https://github.com/microsoft/Microsoft.IO.RecyclableMemoryStream) to reuse block of memories.
Then whole conversion can look like this

```cs
    // doc settings object created earlier
    Stream? stream = null;
    var converted = await _pdfConverter.ConvertAsync(
        doc,
        length =>
        {
            stream = _recyclableMemoryStreamManager.GetStream(
                Guid.NewGuid(),
                "wkhtmltox",
                length);
            return stream;
        },
        _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None);
    stream!.Position = 0;
    if (converted)
    {
        var result = new FileStreamResult(stream, "application/pdf")
        {
            FileDownloadName = "sample.pdf",
        };

        return result;
    }
```

WkHtmlToX native lib comes with support for following operation system flavours:

- WinX64,
- WinX86,
- OsxX64,
- AmazonLinux2,
- Centos6,
- Centos7,
- Centos8,
- Debian9X64,
- Debian9X86,
- Debian10X64,
- Debian10X86,
- OpenSuseLeap15,
- Ubuntu1404X64,
- Ubuntu1404X86,
- Ubuntu1604X64,
- Ubuntu1604X86,
- Ubuntu1804X64,
- Ubuntu1804X86,
- Ubuntu2004X64,

For linux you can set it using second parameter in config.

```cs
    var configuration = new WkHtmlToXConfiguration((int)PlatformID.Unix, WkHtmlToXRuntimeIdentifier.Ubuntu2004X64);
```

## Badges

[![CodeFactor](https://www.codefactor.io/repository/github/adaskothebeast/wkhtmltox/badge/master)](https://www.codefactor.io/repository/github/adaskothebeast/wkhtmltox/overview/master)
[![Build Status](https://adaskothebeast.visualstudio.com/AdaskoTheBeAsT.WkHtmlToX/_apis/build/status/AdaskoTheBeAsT.WkHtmlToX?branchName=master)](https://adaskothebeast.visualstudio.com/AdaskoTheBeAsT.WkHtmlToX/_build/latest?definitionId=8&branchName=master)
![Azure DevOps tests](https://img.shields.io/azure-devops/tests/AdaskoTheBeAsT/AdaskoTheBeAsT.WkHtmlToX/18)
![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/AdaskoTheBeAsT/AdaskoTheBeAsT.WkHtmlToX/18?style=plastic)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=AdaskoTheBeAsT_AdaskoTheBeAsT.WkHtmlToX&metric=alert_status)](https://sonarcloud.io/dashboard?id=AdaskoTheBeAsT_AdaskoTheBeAsT.WkHtmlToX)
![Sonar Tests](https://img.shields.io/sonar/tests/AdaskoTheBeAsT_AdaskoTheBeAsT.WkHtmlToX?server=https%3A%2F%2Fsonarcloud.io)
![Sonar Test Count](https://img.shields.io/sonar/total_tests/AdaskoTheBeAsT_AdaskoTheBeAsT.WkHtmlToX?server=https%3A%2F%2Fsonarcloud.io)
![Sonar Test Execution Time](https://img.shields.io/sonar/test_execution_time/AdaskoTheBeAsT_AdaskoTheBeAsT.WkHtmlToX?server=https%3A%2F%2Fsonarcloud.io)
![Sonar Coverage](https://img.shields.io/sonar/coverage/AdaskoTheBeAsT_AdaskoTheBeAsT.WkHtmlToX?server=https%3A%2F%2Fsonarcloud.io&style=plastic)
![Nuget](https://img.shields.io/nuget/dt/AdaskoTheBeAsT.WkHtmlToX)
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FAdaskoTheBeAsT%2FWkHtmlToX.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FAdaskoTheBeAsT%2FWkHtmlToX?ref=badge_shield)

## Influencers

Library is based on wrapper (DinkToPdf)[https://github.com/rdvojmoc/DinkToPdf].
Interoperability was totally reworked and now it is under tests to see if leaking memory can be avoided.


## License
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FAdaskoTheBeAsT%2FWkHtmlToX.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2FAdaskoTheBeAsT%2FWkHtmlToX?ref=badge_large)
