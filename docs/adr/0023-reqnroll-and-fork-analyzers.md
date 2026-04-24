# 0023 - Replace SpecFlow with Reqnroll and fork analyzers

- Status: Accepted
- Date: 2024-02-16
- Traceability: `ac0d950` "added own version of analyzers and migrate to Reqnroll from SpecFlow"

## Context

SpecFlow stopped receiving timely updates for new .NET TFMs and had
licensing/maintenance uncertainty. Separately, several third-party
analyzers we depended on stopped honouring our ruleset on new
compiler versions.

## Decision

- Migrate all `.feature` suites and step bindings from SpecFlow to
  [Reqnroll](https://reqnroll.net/), a community fork compatible with
  modern .NET.
- Publish and depend on our own forks of the analyzers we care about
  (prefixed `AdaskoTheBeAsT.*`), so we can cut releases aligned with
  our supported TFMs.

## Consequences

- Integration tests run again on .NET 8+.
- Analyzer versions no longer block TFM upgrades.
- Slight increase in owned surface area, accepted as the cost of
  keeping the quality gate working.
