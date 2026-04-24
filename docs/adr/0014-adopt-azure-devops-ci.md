# 0014 - Adopt Azure DevOps pipeline for CI

- Status: Superseded by [ADR 0027](0027-slnx-and-github-actions.md)
- Date: 2020-03-29
- Traceability: `dc0f3f2` "added azure devops configuration", `9b983b2` "Set up CI with Azure Pipelines"

## Context

Manual local builds made release cadence unpredictable and NuGet
packages were not reproducible. The project needed CI with SonarCloud
integration and multi-TFM build.

## Decision

Adopt Azure Pipelines (`azure-pipelines.yml`) as the canonical CI:

- restore / build / test per TFM;
- run SonarCloud analysis on pull request;
- pack and push NuGet artifacts on tagged builds;
- publish code coverage.

## Consequences

- Every merge produces the same binaries bit-for-bit (complemented by
  ADR 0015).
- SonarCloud becomes the quality gate (ADR 0017).
- Eventually retired in favour of GitHub Actions; see ADR 0027.
