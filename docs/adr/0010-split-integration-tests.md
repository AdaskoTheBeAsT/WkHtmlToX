# 0010 - Split integration tests into a separate project

- Status: Accepted
- Date: 2020-03-03
- Traceability: `1abde7a` "moved integration tests to integ project"

## Context

Running real conversions against `wkhtmltox` is slow, platform-sensitive
and unsuitable for fast unit-test feedback. Mixing them with unit tests
forced every PR to pay the full integration cost.

## Decision

Split tests into three projects:

- `test/unit/AdaskoTheBeAsT.WkHtmlToX.Test` - mockable logic, runs on
  every build.
- `test/integ/AdaskoTheBeAsT.WkHtmlToX.IntegrationTest` - real native
  calls, gated behind a separate CI stage, uses SpecFlow/Reqnroll
  features.
- `test/memory/AdaskoTheBeAsT.WkHtmlToX.MemoryTest` - long-running
  allocation / leak probes (see ADR 0034).

## Consequences

- Fast PR feedback on the unit stage.
- Native-platform-specific failures isolated to the integration stage.
- Memory regressions get a dedicated home instead of polluting other
  suites.
