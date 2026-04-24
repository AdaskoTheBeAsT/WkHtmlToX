# 0016 - Upgrade to `wkhtmltox` 0.12.6

- Status: Accepted
- Date: 2020-07-08
- Traceability: `700fa1e` "upgraded to wkhtmltox to version 0.12.6"

## Context

The 0.12.5 native binaries shipped with known crashes on modern Linux
distributions and did not have binaries for recent Windows builds.
0.12.6 is the last blessed upstream `wkhtmltox` release.

## Decision

Ship `runtimes/<rid>/native/wkhtmltox.*` from the 0.12.6 release for
all supported RIDs. Track that version as the baseline; do not try to
mix in newer community forks unless they are API-compatible.

## Consequences

- Consumers on existing 0.12.5 integrations must redeploy; entry-point
  names did not change so public API stays intact.
- `wkhtmltox` is effectively frozen upstream, which is part of why
  ADR 0030 values session recycling and fault signalling so highly.
