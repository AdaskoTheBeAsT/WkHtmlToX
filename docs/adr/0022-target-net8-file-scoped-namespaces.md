# 0022 - Add .NET 8 target and adopt file-scoped namespaces

- Status: Superseded by [ADR 0024](0024-target-net9.md)
- Date: 2023-12-05
- Traceability: `cdccf63` "upgrade to .net 8", `5fed681` "added version .net 8.0",
  `066de9f` "file based namespaces", `b0a282c` "removed additional #nullable",
  `1c23139` "comment non used libs"

## Context

.NET 8 is LTS and gains AOT-friendly interop helpers
(`[LibraryImport]`, `NativeLibrary.TryLoad` improvements). Also a good
time to modernize source style to C# 10 file-scoped namespaces.

## Decision

- Add `net8.0` target.
- Convert sources to file-scoped namespaces.
- Remove redundant per-file `#nullable enable` pragmas now that
  project-level nullable is enforced.

## Consequences

- Less indentation noise, smaller files.
- Public behaviour unchanged.
- Starting here, `LibraryImport` becomes an option for the loader but
  is deferred to ADR 0029.
