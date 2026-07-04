---
goal: Refactor InferenceClient config to use typed options pattern instead of raw GetSection calls
version: 1.0
date_created: 2026-07-03
owner: Platform Team
status: 'Completed'
tags: refactor, http, options, inference, embeddings
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Refactor the `AddInferenceClient` DI registration in the Catalog module to replace raw `IConfiguration.GetSection` / `GetValue` calls with a typed `InferenceClientOptions` class bound via the options pattern (`IOptions<T>`). This eliminates stringly-typed config access, enables FluentValidation support, and allows downstream consumers to inject the options directly.

## 1. Requirements & Constraints

- **REQ-001**: Replace `configuration.GetSection("Http:Clients:Inference")`, `section["BaseAddress"]`, `section.GetValue<int>("TimeoutSeconds")`, and `section.GetSection("DefaultHeaders")` with a typed options class
- **REQ-002**: The options class must be bound to the config section via `services.Configure<InferenceClientOptions>(configuration.GetSection("Http:Clients:Inference"))` so that `IOptions<InferenceClientOptions>`, `IOptionsSnapshot<T>`, or `IOptionsMonitor<T>` can be injected by any downstream service
- **REQ-003**: The `AddInferenceClient(this IServiceCollection, IConfiguration)` extension method signature must remain unchanged — no call-site changes in `Catalog.Extension.cs`
- **REQ-004**: The `InferenceClient` implementation and `IInferenceClient` interface must not change
- **REQ-005**: Config section path remains `Http:Clients:Inference` — no schema changes to `appsettings.json`
- **REQ-006**: `NamedClientOptions` from `Shared.Operational.Http.Options` must NOT be reused directly — a module-specific `InferenceClientOptions` prevents coupling to the shared dictionary-dispatch pattern and allows inference-specific validation/settings in the future
- **CON-001**: The `Configure` call happens at service collection time and `IConfiguration` is available — no need for `IPostConfigureOptions`
- **PAT-001**: Follow the existing options pattern established by `HttpOptions` / `NamedClientOptions` in the Shared project
- **PAT-002**: Use the same validation approach as `HttpOptionsValidator` if FluentValidation is added later

## 2. Implementation Steps

### Implementation Phase 1

- GOAL-001: Create the `InferenceClientOptions` class in the module client folder

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.Options.cs` with `InferenceClientOptions` sealed class containing `BaseAddress` (string, default `HttpConstant.HttpInferenceDefaultBaseAddress`), `TimeoutSeconds` (int, default `0`), and `DefaultHeaders` (Dictionary\<string,string\>, default `[]`) | |  |
| TASK-002 | Add a new public constant `HttpInferenceDefaultBaseAddress = "http://inference"` to `HttpConstant.cs` (or inline it if no other client needs it) — place in `HttpConstant.Defaults` | |  |

### Implementation Phase 2

- GOAL-002: Refactor `AddInferenceClient` to use the options pattern

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | In `ImageEmbedding.Inference.DependencyInjection.cs`, add `services.Configure<InferenceClientOptions>(configuration.GetSection("Http:Clients:Inference"))` before the typed client registration | |  |
| TASK-004 | Replace the three raw `configuration.GetSection` / `GetValue` calls with a single `configuration.GetSection("Http:Clients:Inference").Get<InferenceClientOptions>()` call and use the strongly-typed properties in the `configure` action | |  |
| TASK-005 | Remove the `using Microsoft.Extensions.Configuration;` import if no longer needed directly (the `IConfiguration` parameter is still used for `GetSection`) | |  |

### Implementation Phase 3

- GOAL-003: Build and run existing tests to verify no regressions

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Run `dotnet build service/Api/src/Api/Api.csproj` — must succeed | |  |
| TASK-007 | Run `dotnet test service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj` — all HTTP extension tests pass | |  |
| TASK-008 | Run `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj` — all inference client tests pass | |  |

## 3. Alternatives

- **ALT-001**: Reuse `NamedClientOptions` from Shared.Operational.Http.Options directly. Rejected because it creates a dependency on the dictionary-keyed `Clients` dispatch pattern and prevents adding inference-specific validation or behavior in the future without changing the shared class.
- **ALT-002**: Keep the raw `GetSection` / `GetValue` calls and only wrap them in a local helper method. Rejected because it does not enable DI injection of options (`IOptions<T>`) by other services that need configuration values.
- **ALT-003**: Use `BindConfiguration` with a config path prefix. Rejected because `BindConfiguration` expects the section name to match the options class name by convention, which would require either renaming the section or adding a custom `ConfigKey` attribute — the `Configure<T>(section)` overload is more explicit.

## 4. Dependencies

- **DEP-001**: `Microsoft.Extensions.Options` (transitively available via `Microsoft.Extensions.Configuration` already referenced in the Module project)
- **DEP-002**: `Microsoft.Extensions.Configuration.Binder` (transitively available — used by `Get<T>()`)

## 5. Files

| File | Change |
|------|--------|
| `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.Options.cs` | **NEW** — `InferenceClientOptions` sealed class |
| `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.DependencyInjection.cs` | **MODIFY** — replace raw `GetSection`/`GetValue` with `Configure<T>` + `Get<T>()` |
| `service/Api/src/Shared/Operational/Http/Options/HttpConstant.cs` | **MODIFY** — (optional) add `HttpInferenceDefaultBaseAddress` constant |

## 6. Testing

- **TEST-001**: Existing `Module.UnitTests` inference client tests (9 test methods in `ImageEmbedding.Inference.Tests.cs`) must pass unchanged — they create `InferenceClient` directly with a mocked `HttpClient` and exercise only the client behavior, not the DI registration
- **TEST-002**: Existing `Shared.UnitTests` HTTP extension tests (3 test methods in `HttpExtensions.Tests.cs`) must pass unchanged
- **TEST-003**: `dotnet build` on Api, Module, and Shared projects succeeds

## 7. Risks & Assumptions

- **RISK-001**: The `configure` callback in `AddTypedHttpClient` captures the options from config at registration time — if config changes at runtime (via `IOptionsMonitor<T>`), the typed client still uses the initial values. Mitigation: this matches current behavior and is acceptable; clients are typically not recreated per-request.
- **ASSUMPTION-001**: The `Http:Clients:Inference` config section is present in `appsettings.json` with the existing schema — no config changes required.
- **ASSUMPTION-002**: The `Get<T>()` call succeeds even when the section is missing (returns `null`), and the fallback to `new InferenceClientOptions()` with defaults works correctly.

## 8. Related Specifications / Further Reading

- [Existing refactoring plan](plan/refactor-inference-client-shared-http-1.md)
- [Shared HTTP options](service/Api/src/Shared/Operational/Http/Options/HttpOptions.cs)
- [Current DI registration](service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.DependencyInjection.cs)
