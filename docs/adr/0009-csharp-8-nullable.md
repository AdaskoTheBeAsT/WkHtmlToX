# 0009 - Switch to C# 8.0 and nullable reference types

- Status: Accepted
- Date: 2020-02-26
- Traceability: `fe18a7f` "switch to C# 8.0 and nullable"

## Context

Interop code is a natural home for null bugs: `GetProcAddress` can
return null, native entry points can return empty strings, user-supplied
`Stream`s can be null. Catching these at compile time is much cheaper
than failing inside an STA worker.

## Decision

Turn on `<Nullable>enable</Nullable>` project-wide and bump
`<LangVersion>` to 8.0 (raised further in later TFM bumps). Annotate
all interop surfaces, callbacks and configuration types.

## Consequences

- Null-related regressions caught by the compiler, not at runtime.
- Some call sites require `!` or explicit guards; documented in
  comments only when the guard reason is not obvious.
- Downstream consumers on older language versions still work through
  the non-annotated `netstandard2.0` surface.
