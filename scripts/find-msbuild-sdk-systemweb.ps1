<#
.SYNOPSIS
    Find which .csproj files reference MSBuild.Sdk.SystemWeb (or MSBuild.SDK.SystemWeb).

.DESCRIPTION
    Recursively searches one or more directories for .csproj files containing
    the MSBuild.Sdk/SystemWeb SDK identifier and prints filename + matching lines.

.PARAMETER Paths
    Array of paths to search. Defaults to '..' and '..\\..\\projects'.

.PARAMETER Recurse
    Switch to enable recursion (default: on).

.PARAMETER AsJson
    Output results as JSON.

.EXAMPLE
    .\find-msbuild-sdk-systemweb.ps1

.EXAMPLE
    .\find-msbuild-sdk-systemweb.ps1 -Paths '..','..\\..\\projects' -AsJson

#>

param(
    [Parameter(Position=0)]
    [string[]]
    $Paths = @('..','..\\..\\projects'),

    [switch]
    $Recurse = $true,

    [switch]
    $AsJson = $false
)

try {
    $pattern = '(?i)MSBuild\.(?:Sdk|SDK)\.SystemWeb'

    $foundFiles = @()
    foreach ($p in $Paths) {
        if (-not (Test-Path -Path $p)) {
            Write-Verbose "Path not found: $p"
            continue
        }

        if ($Recurse) {
            $items = Get-ChildItem -Path $p -Recurse -Filter '*.csproj' -File -ErrorAction SilentlyContinue
        }
        else {
            $items = Get-ChildItem -Path $p -Filter '*.csproj' -File -ErrorAction SilentlyContinue
        }

        if ($items) { $foundFiles += $items }
    }

    if (-not $foundFiles -or $foundFiles.Count -eq 0) {
        Write-Output "No .csproj files found in specified paths: $($Paths -join ', ')"
        exit 0
    }

    $results = @()
    foreach ($f in $foundFiles | Sort-Object -Property FullName -Unique) {
        $matches = Select-String -Path $f.FullName -Pattern $pattern -AllMatches -ErrorAction SilentlyContinue
        if ($matches) {
            $entry = [PSCustomObject]@{
                File = $f.FullName
                Matches = @()
            }
            foreach ($m in $matches) {
                $entry.Matches += [PSCustomObject]@{
                    LineNumber = $m.LineNumber
                    Line = $m.Line.Trim()
                }
            }
            $results += $entry
        }
    }

    if (-not $results -or $results.Count -eq 0) {
        Write-Output "No occurrences of MSBuild.Sdk.SystemWeb found in .csproj files under: $($Paths -join ', ')"
        exit 0
    }

    if ($AsJson) {
        $results | ConvertTo-Json -Depth 5
    }
    else {
        foreach ($r in $results) {
            Write-Output "File: $($r.File)"
            foreach ($m in $r.Matches) {
                Write-Output "  Line $($m.LineNumber): $($m.Line)"
            }
            Write-Output ""
        }
    }
}
catch {
    Write-Error "Error searching files: $_"
    exit 2
}
