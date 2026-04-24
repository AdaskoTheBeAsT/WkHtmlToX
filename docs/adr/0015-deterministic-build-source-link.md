# 0015 - Deterministic builds with Source Link and embedded untracked sources

- Status: Accepted
- Date: 2020-04-19
- Traceability: `30a918e`, `c24da74`, `cf49ab6`, `02a1f58`, `f3013f7`, `0620448`, `e45d9ef`

## Context

Downstream users wanted `Go To Definition` from NuGet binaries and
binary reproducibility for supply-chain audits. Default `dotnet pack`
embeds machine paths and ships PDBs separately, which both hurt
reproducibility and debuggability.

## Decision

- Enable `<Deterministic>true</Deterministic>` and
  `<ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>`.
- Add `Microsoft.SourceLink.GitHub` and `EmbedUntrackedSources`.
- Extend `AllowedOutputExtensionsInPackageBuildOutputFolder` so
  symbol files ship in the main nupkg (later adjusted to stop shipping
  `.pdb` files directly, per NuGet recommendation).
- Factor common MSBuild settings into `Directory.Build.props` and
  `DeterministicBuild.targets`.

## Consequences

- `nupkg`s are reproducible across build machines.
- Consumers can step through the library with Source Link.
- Build settings live in one place, not duplicated across csprojs.
