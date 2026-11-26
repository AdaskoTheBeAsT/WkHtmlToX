# 0040 - Add .NET 11 and drop .NET 8 and .NET 9

- Status: Accepted
- Date: 2026-11-25
- Updates the target list from [0025](0025-target-net10.md).

## Context

The Interop dependency updates and the new container samples use .NET 11.
The preceding 13.0.0 release included .NET 8 and .NET 9 assets, so dropping
these targets is a breaking package change.

## Decision

- Ship .NET 11, .NET 10, and .NET Framework 4.7.2, 4.8, and 4.8.1 assets in
  release 14.0.0.
- Build with the .NET 11 SDK. Keep .NET 10 in the test and package validation
  matrix alongside .NET 11 and the .NET Framework targets.
- Keep Windows Nano Server and Ubuntu containers as samples; the libraries
  remain independent of the container images and do not bundle a renderer.

## Consequences

Applications on .NET 8 or .NET 9 must retarget to .NET 10 or later before
upgrading. The 13.0.0 package assets and migration guide remain historically
accurate.
