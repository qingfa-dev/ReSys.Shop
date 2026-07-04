---
goal: Refactor InferenceClient to use Shared.Operational.Http Infrastructure
version: 1.0
date_created: 2026-07-03
last_updated: 2026-07-03
status: 'Completed'
tags: refactor, infrastructure, http, inference, embeddings
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Refactored the module-scoped `InferenceClient` registration in the Catalog module to use the shared `Shared.Operational.Http` infrastructure, gaining correlation ID propagation, config-driven base addresses and timeouts, and resilience pipeline support. The `InferenceClient` implementation and interface remain unchanged; only the DI registration layer was modified.

## 1. Requirements & Constraints

- **REQ-001**: `InferenceClient` must be registered as a typed HttpClient via `Shared.Operational.Http.HttpClientExtensions.AddTypedHttpClient<T>()`
- **REQ-002**: The custom `InferenceAuthHandler` (X-API-Key header) must remain in the handler pipeline
- **REQ-003**: Correlation ID propagation must be enabled for inference requests
- **REQ-004**: Resilience pipeline (retry, circuit breaker) must be attached
- **REQ-005**: The `AddInferenceClient(this IServiceCollection, IConfiguration)` extension method signature must remain unchanged — no call-site changes in `Catalog.Extension.cs`
- **REQ-006**: Base address and timeout must be configurable via `appsettings.json` under the `Http:Clients:Inference` section
- **REQ-007**: The `InferenceAuthOptions` config section (`Services:Inference`) stays separate — auth config is independent of HTTP transport config
- **CON-001**: `Microsoft.Extensions.Http` (providing `IHttpClientBuilder`) is not transitively resolvable in `Module.csproj` via the `Shared` project reference — return-type-based chaining does not work for module consumers
- **CON-002**: The `CorrelationIdPropagationHandler` is `internal sealed` in `Shared.Operational.Http` — the inference DI is in `Module.Catalog.*` namespace and cannot reference it directly; `AddTypedHttpClient` already registers it internally
- **PAT-001**: Follow the existing typed-client registration pattern established by `AddTypedHttpClient<T>()`

## 2. Implementation Steps

### Implementation Phase 1

- GOAL-001: Add generic overloads of `AddTypedHttpClient` that accept custom `DelegatingHandler` types as generic parameters, avoiding the need for the caller to resolve `IHttpClientBuilder`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Add `AddTypedHttpClient<TClient, THandler>(...)` overload (2 type params) where `THandler : DelegatingHandler` — registers a typed client with a custom message handler alongside `CorrelationIdPropagationHandler` | ✅ | 2026-07-03 |
| TASK-002 | Add `AddTypedHttpClient<TService, TImplementation, THandler>(...)` overload (3 type params) where `TImplementation : class, TService` and `THandler : DelegatingHandler` — supports the interface-to-implementation registration pattern needed by `IInferenceClient`/`InferenceClient` | ✅ | 2026-07-03 |

### Implementation Phase 2

- GOAL-002: Refactor `AddInferenceClient` DI to use shared `AddTypedHttpClient` with the `InferenceAuthHandler` as a generic type parameter

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | In `ImageEmbedding.Inference.DependencyInjection.cs`, replace `services.AddHttpClient<IInferenceClient, InferenceClient>(client => { client.BaseAddress = ... }).AddHttpMessageHandler<InferenceAuthHandler>()` with `services.AddTypedHttpClient<IInferenceClient, InferenceClient, InferenceAuthHandler>("http://inference")` | ✅ | 2026-07-03 |
| TASK-004 | Remove the hardcoded `client.BaseAddress = new Uri("http://inference")` — the shared method handles base address as a string parameter | ✅ | 2026-07-03 |
| TASK-005 | Keep `services.Configure<InferenceAuthOptions>(...)` and `services.AddTransient<InferenceAuthHandler>()` unchanged — auth setup is orthogonal to transport | ✅ | 2026-07-03 |
| TASK-006 | Add `using Shared.Operational.Http;` import to the DI file | ✅ | 2026-07-03 |

### Implementation Phase 3

- GOAL-003: Update `appsettings.json` to add both the inference HTTP transport config and the auth config

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | In `appsettings.json`, add an `"Inference"` entry under `"Http":{"Clients":{...}}` with `baseAddress: "http://inference"`, `timeoutSeconds: 30`, `attachResiliencePipeline: true` | ✅ | 2026-07-03 |
| TASK-008 | Add `"Services:Inference:ApiKey": ""` to `appsettings.json` under a new `"Services"` section | ✅ | 2026-07-03 |

### Implementation Phase 4

- GOAL-004: Verify the refactored registration compiles and the handler pipeline order is correct

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Run `dotnet build` on the Api project — builds successfully | ✅ | 2026-07-03 |
| TASK-010 | Run existing `Shared.UnitTests` HTTP tests — all 15 tests pass | ✅ | 2026-07-03 |
| TASK-011 | Verify the handler pipeline order: CorrelationIdPropagationHandler (outermost) → Resilience (middle) → InferenceAuthHandler (innermost, closest to HTTP call) | ✅ | 2026-07-03 |

## 3. Alternatives

- **ALT-001**: Keep the current approach where `InferenceClient` is registered entirely within the Catalog module with no shared infrastructure. Rejected because it duplicates correlation propagation and resilience setup.
- **ALT-002**: Change `AddTypedHttpClient` return type from `IServiceCollection` to `IHttpClientBuilder` so callers can chain `.AddHttpMessageHandler<T>()`. Attempted but reverted — `Module.csproj` cannot resolve `IHttpClientBuilder` transitively from the Shared project (the `Microsoft.Extensions.Http` assembly). Generic-type-parameter overloads avoid this by keeping all `IHttpClientBuilder` usage inside the Shared project.
- **ALT-003**: Add `Microsoft.Extensions.Http` as a direct `PackageReference` in `Module.csproj`. Rejected because it would couple the Module project to a low-level infrastructure package and is unnecessary when the generic overload approach works cleanly.
- **ALT-004**: Move the entire inference client registration into `AddHttpClients()` via config-driven named clients. Rejected because named clients don't support custom `DelegatingHandler` injection, making the auth handler impossible to wire up without additional infrastructure changes.

## 4. Dependencies

- **DEP-001**: `Shared.Operational.Http` namespace — the `AddTypedHttpClient<T>()` extension method
- **DEP-002**: No NuGet package changes required

## 5. Files

| File | Change |
|------|--------|
| `service/Api/src/Shared/Operational/Http/Http.Extensions.cs` | Added 2 new overloads: `AddTypedHttpClient<TClient, THandler>` and `AddTypedHttpClient<TService, TImplementation, THandler>` |
| `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.DependencyInjection.cs` | Replaced manual `AddHttpClient` with `AddTypedHttpClient<IInferenceClient, InferenceClient, InferenceAuthHandler>("http://inference")` |
| `service/Api/src/Api/appsettings.json` | Added `Inference` entry under `Http:Clients` and `Services:Inference:ApiKey` |
| `service/Api/src/Module/Catalog/Catalog.Extension.cs` | No changes needed — `AddInferenceClient` signature is stable |

## 6. Testing

- **TEST-001**: Existing `Shared.UnitTests` HTTP tests (15 tests) pass — covers `CorrelationIdPropagationHandler` and basic `AddTypedHttpClient` functionality
- **TEST-002**: `dotnet build` on Api, Module, and Shared projects succeeds
- **TEST-003**: Handler pipeline order verified: `CorrelationIdPropagationHandler` → Resilience → `InferenceAuthHandler` (in order of registration in `AddTypedHttpClient`)

## 7. Risks & Assumptions

- **RISK-001**: New overloads create additional public API surface. Mitigated because they follow the exact same pattern as the existing `AddTypedHttpClient<T>()`.
- **ASSUMPTION-001**: The handler registration order (`AddHttpMessageHandler`) is additive; the shared handler (CorrelationId) is registered first, then the custom handler. The outer-to-inner order is: CorrelationId → Resilience → InferenceAuthHandler, which is correct (auth is per-request specific).
- **ASSUMPTION-002**: The inference service base address `http://inference` is appropriate for the container/development environment. It can be overridden via `Http:Clients:Inference:BaseAddress` in environment-specific `appsettings.*.json` or environment variables.
- **ASSUMPTION-003**: The `AddInferenceClient` method is called after `AddOperational()` in the startup pipeline, ensuring `CorrelationIdPropagationHandler` is registered as a service.

## 8. Related Specifications / Further Reading

- [Shared.Operational.Http current implementation](service/Api/src/Shared/Operational/Http/)
- [InferenceClient current implementation](service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/)
- [Catalog module entry point](service/Api/src/Module/Catalog/Catalog.Extension.cs)
