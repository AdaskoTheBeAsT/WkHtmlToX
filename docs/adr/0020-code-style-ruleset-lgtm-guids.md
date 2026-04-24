# 0020 - Enforce code style via ruleset, LGTM and project GUIDs

- Status: Accepted
- Date: 2021-07-31
- Traceability: `aaaa4f1` "added project guids", `0f5f6d6` "lgtm.yml",
  `757c66d` "added lgtm to badges", `16223d2` "fix casing of ruleset",
  `d26d997`, `75ff149`, `ffbd665`, `a491edf`, `c5eb429`, `81609f1`,
  `f6dce04`, `7f62169`, `adf4898`

## Context

As contributors grew, stylistic drift and per-IDE inspection
differences produced noisy diffs. Some tools also needed stable
project identifiers (GUIDs) to correlate reports.

## Decision

- Commit `AdaskoTheBeAsT.ruleset` at the repo root and reference it
  from `Directory.Build.props`.
- Commit `.editorconfig` with the agreed formatting.
- Add project GUIDs to every csproj.
- Enable LGTM for community code scanning.
- Standardize casing of file names and ruleset (no per-OS casing
  footguns).

## Consequences

- Style failures are deterministic across machines and CI.
- New contributors get analyser complaints early.
- LGTM was later removed (superseded by GitHub code scanning) but the
  ruleset and editorconfig pattern remains.
