# 0024 - Add .NET 9 target

- Status: Superseded by [ADR 0025](0025-target-net10.md)
- Date: 2025-01-05
- Traceability: `b954378` "update to .net 9", `5c2e1c1` "modified azure pipeline",
  `6161876` "downgrade resharper"

## Context

.NET 9 ships on schedule and brings further interop improvements
(`LibraryImportAttribute` codegen, `SearchValues<T>`).

## Decision

Add `net9.0` target. Update CI matrix. Downgrade the ReSharper
CLI version that regressed on `net9.0` inputs until upstream fixes
land.

## Consequences

- Build matrix accommodates the new TFM.
- Public API unchanged.
