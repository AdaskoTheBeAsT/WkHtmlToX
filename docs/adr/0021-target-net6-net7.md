# 0021 - Add .NET 6 and .NET 7 targets

- Status: Superseded by [ADR 0022](0022-target-net8-file-scoped-namespaces.md)
- Date: 2022-11-13
- Traceability: `68ccf40` ".net 6 in azure-pipelines", `7deeba3` "- .NET 7 release",
  `2bbe886` "fix azure-pipelines"

## Context

.NET 6 is LTS; .NET 7 brings native AOT groundwork and faster P/Invoke
stubs that benefit interop-heavy libraries.

## Decision

Multi-target `net6.0`, `net7.0` in addition to existing TFMs. Update
the Azure Pipelines matrix accordingly.

## Consequences

- Binary is available for modern hosts without a netstandard-bridge
  penalty.
- CI matrix grows; Azure Pipelines keeps up via parallel jobs until
  ADR 0027 moves to GitHub Actions.
