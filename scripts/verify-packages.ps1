[CmdletBinding()]
param(
    [string[]] $Frameworks = @('net10.0', 'net9.0', 'net8.0', 'net481', 'net48', 'net472'),
    [switch] $SkipSingleFile
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (-not $IsWindows -or [System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -ne 'X64') {
    throw 'Native package validation currently requires Windows x64.'
}

$repo = Split-Path $PSScriptRoot -Parent
$work = Join-Path $repo "artifacts/package-validation/$([Guid]::NewGuid().ToString('N'))"
$packages = Join-Path $work 'feed'
$consumer = Join-Path $work 'consumer'
New-Item -ItemType Directory -Path $packages, $consumer -Force | Out-Null

function Invoke-DotNet {
    param([string[]] $Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet failed (exit $LASTEXITCODE): $($Arguments -join ' ')"
    }
}

# Never publish these validation artifacts or change the repository release version.
$version = '13.0.0-validation'
$names = @(
    'AdaskoTheBeAsT.WkHtmlToX',
    'AdaskoTheBeAsT.WkHtmlToX.DependencyInjection',
    'AdaskoTheBeAsT.WkHtmlToX.Hosting'
)
Invoke-DotNet @('restore', (Join-Path $repo 'AdaskoTheBeAsT.WkHtmlToX.slnx'), '--locked-mode', '-p:HUSKY=0')
foreach ($name in $names) {
    Invoke-DotNet @(
        'pack', (Join-Path $repo "src/$name/$name.csproj"),
        '--no-restore', '-c', 'Release', "-p:Version=$version",
        '-p:GeneratePackageOnBuild=false', '-p:ContinuousIntegrationBuild=true',
        '-p:HUSKY=0', '--output', $packages
    )
    $package = Join-Path $packages "$name.$version.nupkg"
    $archive = [System.IO.Compression.ZipFile]::OpenRead($package)
    try {
        foreach ($framework in $Frameworks) {
            if ($null -eq $archive.GetEntry("lib/$framework/$name.dll") -or
                $null -eq $archive.GetEntry("lib/$framework/$name.xml")) {
                throw "$package is missing the assembly or XML documentation for $framework."
            }
        }
        foreach ($entry in @('README.md', 'LICENSE')) {
            if ($null -eq $archive.GetEntry($entry)) { throw "$package is missing $entry." }
        }
    }
    finally {
        $archive.Dispose()
    }
}

# A fresh consumer with no repository project references or inherited build imports.
'<Project />' | Set-Content (Join-Path $consumer 'Directory.Build.props')
'<Project />' | Set-Content (Join-Path $consumer 'Directory.Build.targets')
$escapedFeed = [System.Security.SecurityElement]::Escape($packages)
@"
<configuration>
  <packageSources>
    <clear />
    <add key="validation" value="$escapedFeed" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="validation">
      <package pattern="AdaskoTheBeAsT.WkHtmlToX" />
      <package pattern="AdaskoTheBeAsT.WkHtmlToX.DependencyInjection" />
      <package pattern="AdaskoTheBeAsT.WkHtmlToX.Hosting" />
    </packageSource>
    <packageSource key="nuget.org"><package pattern="*" /></packageSource>
  </packageSourceMapping>
</configuration>
"@ | Set-Content (Join-Path $consumer 'NuGet.Config')
$tfms = $Frameworks -join ';'
@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFrameworks>$tfms</TargetFrameworks>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <PlatformTarget>x64</PlatformTarget>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
    <RestorePackagesPath>../packages</RestorePackagesPath>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="AdaskoTheBeAsT.WkHtmlToX" Version="[$version]" />
    <PackageReference Include="AdaskoTheBeAsT.WkHtmlToX.DependencyInjection" Version="[$version]" />
    <PackageReference Include="AdaskoTheBeAsT.WkHtmlToX.Hosting" Version="[$version]" />
    <PackageReference Include="AdaskoTheBeAsT.WkHtmlToX.native.win.x64" Version="[0.12.6]" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="[10.0.12]" Condition="'`$(TargetFramework)' != 'net8.0' and '`$(TargetFramework)' != 'net9.0'" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="[9.0.20]" Condition="'`$(TargetFramework)' == 'net9.0'" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="[8.0.1]" Condition="'`$(TargetFramework)' == 'net8.0'" />
  </ItemGroup>
</Project>
"@ | Set-Content (Join-Path $consumer 'Consumer.csproj')

$readme = Get-Content -LiteralPath (Join-Path $repo 'README.md') -Raw
$blocks = [regex]::Matches($readme, '(?ms)^```csharp\r?\n(.*?)^```')
if ($blocks.Count -ne 3) { throw 'Update the smoke entry point when adding or removing README C# examples.' }
for ($i = 0; $i -lt $blocks.Count; $i++) {
    $blocks[$i].Groups[1].Value | Set-Content (Join-Path $consumer "Readme$i.cs")
}
@'
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Settings;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        if (args.Length == 1)
        {
            await using var engine = new WkHtmlToXEngine(new WkHtmlToXConfiguration
            {
                NativeLibraryPath = Path.GetFullPath(args[0]),
            });
            using var output = new MemoryStream();
            var result = await engine.ConvertPdfAsync(
                new HtmlToPdfDocument { ObjectSettings = { new PdfObjectSettings { HtmlContent = "<p>explicit native path</p>" } } },
                _ => output,
                CancellationToken.None);
            if (!result.Success || output.Length == 0) { throw new InvalidOperationException("Explicit-path conversion failed."); }
            await engine.DisposeAsync();
            await using var replacement = new WkHtmlToXEngine(new WkHtmlToXConfiguration());
            try
            {
                await replacement.InitializeAsync(CancellationToken.None);
                throw new InvalidDataException("Changing the bound native path should require a restart.");
            }
            catch (InvalidOperationException)
            {
                if (!replacement.IsFaulted) { throw; }
            }
        }
        else
        {
            await ConsoleExample.RunAsync(CancellationToken.None);
            await HostedExample.RunAsync(CancellationToken.None);
            await DiExample.RunAsync(CancellationToken.None);
        }
        Console.WriteLine("Packaged README consumer passed.");
    }
}
'@ | Set-Content (Join-Path $consumer 'Program.cs')

$project = Join-Path $consumer 'Consumer.csproj'
Invoke-DotNet @('restore', $project)
Invoke-DotNet @('restore', $project, '--locked-mode')
$explicitDirectory = Join-Path $work 'explicit-native'
New-Item -ItemType Directory -Path $explicitDirectory | Out-Null
$explicitPath = Join-Path $explicitDirectory 'wkhtmltox.dll'
foreach ($framework in $Frameworks) {
    Invoke-DotNet @('run', '--project', $project, '--no-restore', '-c', 'Release', '-f', $framework)
    if (-not (Test-Path -LiteralPath $explicitPath)) {
        $output = Join-Path $consumer "bin/Release/$framework/win-x64"
        $native = Get-ChildItem -LiteralPath $output -Recurse -Filter wkhtmltox.dll | Select-Object -First 1
        if ($null -eq $native) { throw 'The consumer has no native binary.' }
        Copy-Item -LiteralPath $native.FullName -Destination $explicitPath
    }
    Invoke-DotNet @('run', '--project', $project, '--no-build', '--no-restore', '-c', 'Release', '-f', $framework, '--', $explicitPath)
}
if ($Frameworks -contains 'net10.0' -and -not $SkipSingleFile) {
    $published = Join-Path $work 'single-file'
    Invoke-DotNet @(
        'publish', $project, '-c', 'Release', '-f', 'net10.0', '-r', 'win-x64',
        '--self-contained', 'false', '-p:PublishSingleFile=true',
        '-p:PublishTrimmed=false', '-o', $published
    )
    $executable = Join-Path $published 'Consumer.exe'
    & $executable
    if ($LASTEXITCODE -ne 0) { throw 'Single-file consumer failed.' }
    & $executable $explicitPath
    if ($LASTEXITCODE -ne 0) { throw 'Explicit native-path consumer failed.' }
}
if ($SkipSingleFile) { Write-Warning 'Single-file execution was explicitly skipped; it is not validated by this run.' }
Write-Host "Validation succeeded. Non-release artifacts: $work"
