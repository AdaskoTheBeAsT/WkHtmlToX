# 0033 - Do not depend on `AdaskoTheBeAsT.Interop.COM` from the core engine

- Status: Accepted
- Date: 2026-04-21
- Traceability: `docs/plan.md` Section 2.4.

## Context

`AdaskoTheBeAsT.Interop.COM` generalises COM activation, lifetime and
marshaling. `wkhtmltopdf` exposes a C export surface, not a COM
object, so the package has no role in the core engine.

## Decision

The core `AdaskoTheBeAsT.WkHtmlToX` engine and its session types do
not take a dependency on `AdaskoTheBeAsT.Interop.COM`. Consuming
applications that sit behind a COM host (e.g. Office add-ins) may
still pull it in at their level to bridge from COM into WkHtmlToX,
but that is out of scope for this repo.

## Consequences

- Smaller dependency graph for the core package.
- Keeps the library usable on non-Windows hosts where COM is not
  meaningful.
