# 0027 - Migrate solution to `.slnx` and CI from Azure Pipelines to GitHub Actions

- Status: Proposed
- Date: 2026-04-21
- Traceability: uncommitted on `framework/new-worker`:
  deletion of `AdaskoTheBeAsT.WkHtmlToX.sln`, `AdaskoTheBeAsT.WkHtmlToX.ndproj`,
  `.runsettings`, `azure-pipelines.yml`; addition of
  `AdaskoTheBeAsT.WkHtmlToX.slnx`, `.github/`.

## Context

The classic `.sln` format duplicates project metadata, produces
noisy diffs on trivial additions, and is overkill for a repo whose
projects are already GUID-stable (ADR 0020). Azure Pipelines served
well (ADR 0014) but GitHub Actions is where most contributors already
work, and the free tier covers the project's CI needs.

## Decision

- Adopt the new XML-based solution format `.slnx`
  (`AdaskoTheBeAsT.WkHtmlToX.slnx`) and delete the legacy `.sln`.
- Remove the NDepend project file (`.ndproj`) from source control -
  NDepend runs locally for audits only.
- Move CI into `.github/workflows/*.yml`:
  build / test / pack / sonar scan on every push and PR;
  release workflow gated on tag.
- Delete `azure-pipelines.yml` and `.runsettings`.

## Consequences

- Single-source CI configuration aligned with the host (GitHub).
- `.slnx` still has rough edges in some IDEs - contributors on older
  VS versions may need an update.
- Replaces ADR 0014.
