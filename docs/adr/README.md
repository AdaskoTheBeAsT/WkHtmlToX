# Architecture Decision Records (ADR)

This folder contains retrospective and forward-looking ADRs for the
`AdaskoTheBeAsT.WkHtmlToX` project.

The log was reconstructed from the git history (`d51d1ad` .. `da8ee81`),
from the uncommitted work on branch `framework/new-worker`, and from
`docs/plan.md` which sketches the planned migration on top of the
`AdaskoTheBeAsT.Interop.*` libraries.

ADRs use a lightweight [MADR](https://adr.github.io/madr/) layout.

## Index

| #    | Date       | Subject                                                                        | Status      |
| ---- | ---------- | ------------------------------------------------------------------------------ | ----------- |
| 0001 | 2019-12-08 | Record architecture decisions                                                  | Accepted    |
| 0002 | 2019-12-08 | Wrap `wkhtmltox` natively via P/Invoke instead of shelling out to the CLI      | Accepted    |
| 0003 | 2019-12-08 | Use `RecyclableMemoryStream` and remove `unsafe` code                          | Superseded by 0013 |
| 0004 | 2019-12-08 | Serialize native calls on a dedicated worker thread with a blocking queue     | Superseded by 0030 |
| 0005 | 2019-12-15 | Support Linux native loading via `libdl` P/Invoke                              | Superseded by 0029 |
| 0006 | 2019-12-23 | Multi-target `netcoreapp3.1` and `net48`                                       | Superseded by 0018, 0021, 0022, 0024, 0025 |
| 0007 | 2019-12-23 | Reorganise `samples/` and add ASP.NET Core and OWIN samples                   | Accepted    |
| 0008 | 2020-02-25 | Introduce `CancellationTokenSource` to fix .NET Core hang on shutdown          | Accepted    |
| 0009 | 2020-02-26 | Switch to C# 8.0 and nullable reference types                                  | Accepted    |
| 0010 | 2020-03-03 | Split integration tests into a separate project                                | Accepted    |
| 0011 | 2020-03-27 | Adopt `ArrayPool<byte>.Shared` for HTML buffer allocation                      | Accepted    |
| 0012 | 2020-03-28 | Accept `byte[]` and `Stream` as HTML input                                     | Accepted    |
| 0013 | 2020-03-29 | Drop `RecyclableMemoryStream` dependency                                       | Accepted    |
| 0014 | 2020-03-29 | Adopt Azure DevOps pipeline for CI                                             | Superseded by 0027 |
| 0015 | 2020-04-19 | Deterministic builds with Source Link and embedded untracked sources           | Accepted    |
| 0016 | 2020-07-08 | Upgrade to `wkhtmltox` 0.12.6                                                  | Accepted    |
| 0017 | 2020-07-25 | SonarCloud + Roslynator + ReSharper as the quality gate                        | Accepted    |
| 0018 | 2020-11-12 | Target .NET 5                                                                  | Superseded by 0021 |
| 0019 | 2021-02-23 | Fix library initialisation race                                                | Accepted    |
| 0020 | 2021-07-31 | Enforce code style via ruleset, LGTM and project GUIDs                         | Accepted    |
| 0021 | 2022-11-13 | Add .NET 6 and .NET 7 targets                                                  | Superseded by 0022 |
| 0022 | 2023-12-05 | Add .NET 8 target and adopt file-scoped namespaces                             | Superseded by 0024 |
| 0023 | 2024-02-16 | Replace SpecFlow with Reqnroll and fork analyzers                              | Accepted    |
| 0024 | 2025-01-05 | Add .NET 9 target                                                              | Superseded by 0025 |
| 0025 | 2025-11-16 | Add .NET 10 target and adopt raw string literals                               | Accepted    |
| 0026 | 2026-04-04 | Tighten disposal contract across native owners                                 | Accepted    |
| 0027 | 2026-04-21 | Migrate solution to `.slnx` and CI from Azure Pipelines to GitHub Actions     | Accepted    |
| 0028 | 2026-04-21 | Extract `WkHtmlToXSession` and `WkHtmlToXSessionFactory` from the engine      | Accepted    |
| 0029 | 2026-04-21 | Adopt `AdaskoTheBeAsT.Interop.Unmanaged` for native loading (Phase 1)         | Accepted    |
| 0030 | 2026-04-21 | Adopt `AdaskoTheBeAsT.Interop.Execution.ExecutionWorker<TSession>` (Phase 2)  | Accepted    |
| 0031 | 2026-04-21 | Ship `DependencyInjection` and `Hosting` packages (Phase 3)                   | Accepted    |
| 0032 | 2026-04-21 | Opt-in use of `AdaskoTheBeAsT.Interop.Threading` utilities (Phase 4)          | Rejected    |
| 0033 | 2026-04-21 | Do not depend on `AdaskoTheBeAsT.Interop.COM` from the core engine            | Accepted    |
| 0034 | 2026-04-21 | Add native private-bytes memory probes                                        | Accepted    |
| 0035 | 2026-04-21 | Add `Utf8Interop` helper for zero-allocation native string marshaling         | Accepted    |
| 0036 | 2026-09-07 | [Bounded request ownership and native binding](0036-request-ownership-and-native-binding.md) | Accepted |
| 0037 | 2026-09-07 | [Aggregate input budget](0037-aggregate-input-budget.md) | Accepted |
| 0038 | 2026-09-07 | [Pipeline-aware shutdown](0038-pipeline-aware-shutdown.md) | Accepted |
