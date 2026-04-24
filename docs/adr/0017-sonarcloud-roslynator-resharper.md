# 0017 - SonarCloud + Roslynator + ReSharper as the quality gate

- Status: Accepted
- Date: 2020-07-25
- Traceability: `a252b4e` "added new analyzers", `273655f` "new analyzers added",
  `ae992f8` "Add license scan report and status", `8c58205`, `20f5851`

## Context

Interop code is unusually dangerous: a missed `Marshal.FreeHGlobal`,
a missed `SafeHandle.DangerousRelease`, or a badly-shaped `DllImport`
is not visible in a code review but causes native crashes months
later.

## Decision

Run three layers of static analysis in CI and locally:

1. **SonarCloud** for hotspots, security and duplication.
2. **Roslynator** + a curated `.ruleset`
   (`AdaskoTheBeAsT.ruleset`) for Roslyn analyzers.
3. **ReSharper / JetBrains InspectCode** for style and
   resharper-specific inspections.

Warnings are errors by default. Explicit suppressions must carry an
inline reason or `.ruleset` entry.

## Consequences

- Stricter CI, but much higher confidence in interop correctness.
- Occasional friction with new Roslyn analyzer releases; see
  `6161876` "downgrade resharper" for a concrete example.
- LGTM, FOSSA and coverage badges are added/removed over time
  (`ae992f8`, `90aacdd`, `0f5f6d6`, `757c66d`), SonarCloud remains the
  constant.
