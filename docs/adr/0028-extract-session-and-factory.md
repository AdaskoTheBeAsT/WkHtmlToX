# 0028 - Extract `WkHtmlToXSession` and `WkHtmlToXSessionFactory` from the engine

- Status: Proposed
- Date: 2026-04-21
- Traceability: uncommitted on `framework/new-worker`:
  new `src/AdaskoTheBeAsT.WkHtmlToX/Engine/WkHtmlToXSession.cs`,
  new `src/AdaskoTheBeAsT.WkHtmlToX/Engine/WkHtmlToXSessionFactory.cs`,
  467-line reduction in `WkHtmlToXEngine.cs`.

## Context

`WkHtmlToXEngine` historically conflated three concerns:

1. owning the STA worker thread (ADR 0004);
2. initialising and terminating the native PDF / Image modules;
3. dispatching work items to the modules.

Concern 2 is a cohesive "native session" and is exactly what the
upcoming `ExecutionWorker<TSession>` integration (ADR 0030) needs as
its `TSession`.

## Decision

- Introduce `WkHtmlToXSession` that holds `ILibraryLoader`,
  `IPdfProcessor` and `IImageProcessor`.
- Introduce `WkHtmlToXSessionFactory : IExecutionSessionFactory<WkHtmlToXSession>`
  implementing `CreateSession` / `DisposeSession` using today's
  `InitializeInProcessingThread` / `CleanupProcessingThreadState`
  bodies.
- Keep `IWkHtmlToXEngine`, `PdfConverter` and `ImageConverter`
  untouched - this is a refactor below the public API.

## Consequences

- Prepares the ground for the `ExecutionWorker<TSession>` swap
  (ADR 0030) without a big-bang change.
- Unit tests can now construct a session factory with mocks and test
  the session lifecycle in isolation
  (`WkHtmlToXSessionFactoryTest.cs`).
- If Phase 2 is ever reverted, the engine can keep hosting a session
  internally with no public API change.
