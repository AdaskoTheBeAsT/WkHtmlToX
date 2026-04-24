# 0031 - Ship `DependencyInjection` and `Hosting` packages (Phase 3)

- Status: Proposed
- Date: 2026-04-21
- Traceability: `docs/plan.md` Section 2 / 3 Phase 3; uncommitted
  new projects `src/AdaskoTheBeAsT.WkHtmlToX.DependencyInjection/` and
  `src/AdaskoTheBeAsT.WkHtmlToX.Hosting/` plus matching test projects
  `test/unit/AdaskoTheBeAsT.WkHtmlToX.DependencyInjection.Test/`
  and `test/unit/AdaskoTheBeAsT.WkHtmlToX.Hosting.Test/`.

## Context

Today's README tells consumers to
`services.AddSingleton<IWkHtmlToXEngine>(...)` and call
`Initialize()` themselves. That boilerplate is duplicated per
consumer and easy to get wrong around disposal.

## Decision

- Ship `AdaskoTheBeAsT.WkHtmlToX.DependencyInjection` with a single
  `AddWkHtmlToX(this IServiceCollection services, Action<WkHtmlToXConfiguration>? configure = null)`
  extension.
- Ship `AdaskoTheBeAsT.WkHtmlToX.Hosting` that internally calls
  `services.AddExecutionWorkerHostedService<WkHtmlToXSession>(...)`
  so `InitializeAsync` / `DisposeAsync` are driven by the generic
  host.
- Bind `WkHtmlToXConfiguration` through `IOptions<T>` so consumers
  can configure from `appsettings.json`.

## Consequences

- The README shrinks to `services.AddWkHtmlToX();`.
- Native shutdown is deterministic on application stop.
- Two new NuGet ids to maintain; versioned together with the core.
