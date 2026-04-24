# 0025 - Add .NET 10 target and adopt raw string literals

- Status: Accepted
- Date: 2025-11-16
- Traceability: `a50370d` "update to .net10", `9663ff0` "fixed some code smells",
  `eb39099` "raw string"

## Context

.NET 10 LTS cycle starts. Raw string literals (C# 11+) are now widely
available in every supported TFM and allow embedded HTML test
fixtures to drop `\"` escapes, which reduces cognitive load when
reading SonarCloud findings.

## Decision

- Add `net10.0` target.
- Adopt raw string literals in tests and sample HTML fixtures.
- Clean up the remaining SonarCloud code smells flagged by the newer
  analyzers.

## Consequences

- Latest TFM consumers get a dedicated build.
- Test files are much easier to review.
