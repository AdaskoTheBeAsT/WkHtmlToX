# 0006 - Multi-target `netcoreapp3.1` and `net48`

- Status: Superseded by ADRs 0018, 0021, 0022, 0024, 0025
- Date: 2019-12-23
- Traceability: `7cddc7b` "multitarget netcoreapp3.1 net48"

## Context

Consumers were split between full .NET Framework 4.8 apps (IIS, OWIN,
WinForms/WPF) and the then-new .NET Core 3.1 long-term support
release. Shipping only `netstandard2.0` meant no TFM-specific API
access on .NET Core, and shipping only `net48` excluded cross-platform
hosts.

## Decision

Multi-target `netcoreapp3.1` + `net48` (with `netstandard2.0` as the
common denominator where appropriate). Use TFM-specific conditionals
for Windows-only code paths (STA, `LoadLibraryEx` flags).

## Consequences

- Larger CI matrix.
- Later TFM upgrades become additive: .NET 5 (ADR 0018), .NET 6/7
  (ADR 0021), .NET 8 (ADR 0022), .NET 9 (ADR 0024), .NET 10 (ADR 0025).
- Windows-only features stay behind `NET5_0_OR_GREATER` / platform
  guards.
