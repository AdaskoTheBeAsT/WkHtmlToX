# 0018 - Target .NET 5

- Status: Superseded by [ADR 0021](0021-target-net6-net7.md)
- Date: 2020-11-12
- Traceability: `55df420` "upgraded to .net 5"

## Context

.NET 5 unified `netcoreapp` and `netstandard` and provided
`NativeLibrary` and improved `SafeHandle` support, which simplify
native interop.

## Decision

Add `net5.0` to the TFM list alongside `netcoreapp3.1`, `net48` and
`netstandard2.0`. Start preferring `NativeLibrary.Load` where it
simplifies loader code.

## Consequences

- Consumers on .NET 5 get a TFM-native build.
- Older TFMs keep the old loader until a later ADR (0029) replaces the
  entire loader stack.
