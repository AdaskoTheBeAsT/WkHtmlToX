# 0034 - Add native private-bytes memory probes

- Status: Proposed
- Date: 2026-04-21
- Traceability: uncommitted new files
  `test/memory/AdaskoTheBeAsT.WkHtmlToX.MemoryTest/NativePrivateBytesMemoryTest.cs`,
  `test/memory/AdaskoTheBeAsT.WkHtmlToX.MemoryTest/ImageConverterMemoryTest.cs`,
  and new projects
  `test/memory/AdaskoTheBeAsT.WkHtmlToX.NativeMemoryProbe/` and
  `test/memory/AdaskoTheBeAsT.WkHtmlToX.RuntimeMemoryTest/`.

## Context

Managed allocation tests caught managed leaks but missed native
growth inside `wkhtmltox`. The historical mitigation for that is
`MaxOperationsPerSession` (ADR 0030), but we had no automated way to
see when it was actually needed.

## Decision

Add memory probes that observe the process's private bytes rather
than managed GC counters:

- `NativePrivateBytesMemoryTest` asserts that a long loop of
  conversions does not grow the native working set past a budget.
- `AdaskoTheBeAsT.WkHtmlToX.NativeMemoryProbe` is a minimal host that
  exercises the engine to produce reproducible private-bytes traces
  for profilers.
- `AdaskoTheBeAsT.WkHtmlToX.RuntimeMemoryTest` hosts runtime-side
  managed-vs-native comparisons.

Image-side probes (`ImageConverterMemoryTest`) mirror the PDF tests
so both converters are covered.

## Consequences

- Regressions in the native session lifetime are caught in CI instead
  of by end users.
- The probes are excluded from the normal unit-test run to keep PR
  feedback fast, mirroring the split from ADR 0010.
