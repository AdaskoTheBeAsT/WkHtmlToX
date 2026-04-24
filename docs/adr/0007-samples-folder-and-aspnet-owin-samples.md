# 0007 - Reorganise `samples/` and add ASP.NET Core and OWIN samples

- Status: Accepted
- Date: 2019-12-23
- Traceability: `6d4c25e` "move sample folder to samples", `6ab9057`
  "added web api core project", `9bc0a5e` "added owin and web api 2"

## Context

Consumers repeatedly asked how to wire the engine into long-running
ASP.NET hosts without leaking native handles between requests.

## Decision

Move all examples under `samples/` and ship two reference hosts:

- **ASP.NET Core Web API** - registers `IWkHtmlToXEngine` as a
  singleton, initialized on startup, disposed on application stop.
- **OWIN / ASP.NET Web API 2** - shows the same pattern on full
  framework hosts that cannot use `IHostedService`.

## Consequences

- Documentation defaults to "engine is a singleton, initialize once".
- The samples become the reference for later DI/Hosting work
  (see ADR 0031).
