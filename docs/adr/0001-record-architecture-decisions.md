# 0001 - Record architecture decisions

- Status: Accepted
- Date: 2019-12-08
- Traceability: retroactive for `62ab285` / `d51d1ad` "Initial commit"

## Context

The project started as a native wrapper around `wkhtmltox` and has grown
non-trivial interop, threading, loader and packaging concerns over
several years. Decisions were implicit in commits with messages like
"fixes", "upgraded libs" or "style fixes", which makes it hard to
understand *why* the code looks the way it does.

## Decision

Adopt lightweight Markdown ADRs stored in `docs/adr/`. Each ADR gets a
monotonically increasing number, a short subject, a status and a date
that reflects the decision, not the file creation time. Historical
decisions are reconstructed from git history and recorded retroactively
so future refactors have a shared context.

## Consequences

- New work that changes architecture adds an ADR before or with the PR.
- Old commits are linked back to ADRs via the "Traceability" line so the
  narrative can be followed in either direction.
- Superseded ADRs are kept in the tree and cross-linked instead of
  being deleted.
