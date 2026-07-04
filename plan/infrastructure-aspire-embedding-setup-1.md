---
goal: Fix Aspire service discovery and configuration for the Embedding service
version: 1.0
date_created: 2026-07-03
owner: Platform Team
status: 'Completed'
tags: infrastructure, aspire, embedding, service-discovery, http
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Correct the Aspire integration between the .NET API and the Python Embedding service. The current setup has a service discovery hostname mismatch (`BaseAddress: "http://inference"` vs Aspire resource name `"Embedding"`), a redundant dual-registration of HTTP clients, and incomplete ML runtime dependencies in the Python project.

## 1. Requirements & Constraints

- **REQ-001**: Aspire service discovery must correctly resolve the Embedding service when the API makes outbound requests via `InferenceClient` — the hostname in `BaseAddress` must match the Aspire resource name
- **REQ-002**: The redundancy of two separate HTTP client registrations for the same downstream service (named client `"Inference"` from `Http:Clients` config + typed client `IInferenceClient`/`InferenceClient` from module DI) must be eliminated — only one registration should exist
- **REQ-003**: The `AddInferenceClient(this IServiceCollection, IConfiguration)` extension method signature must remain unchanged — no call-site changes in `Catalog.Extension.cs`
- **REQ-004**: The `InferenceClient` implementation and `IInferenceClient` interface must not change
- **REQ-005**: The Embedding Python service must declare all its runtime ML dependencies (`torch`, `open-clip-torch`) in `pyproject.toml`
- **CON-001**: The Aspire resource constant `Services.Embedding = "Embedding"` in `ReSys.ServiceDefaults.Constants.Services` must not be renamed — it accurately describes the service's purpose
- **CON-002**: The typed client registration in `AddInferenceClient()` must retain `CorrelationIdPropagationHandler` and resilience pipeline — these are non-negotiable operational requirements
- **CON-003**: The `AddUvicornApp()` call in `AppHost.cs` must continue to use `WithUv()` and point to the correct Python module path
- **PAT-001**: Follow the existing `Shared.Operational.Http` typed-client pattern — `AddTypedHttpClient()` with correlation and resilience

## 2. Implementation Steps

### Implementation Phase 1

- GOAL-001: Fix service discovery hostname in `appsettings.json` and `InferenceClientOptions` default — align `BaseAddress` hostname with the Aspire resource name `"Embedding"`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | In `service/Api/src/Api/appsettings.json` line 92, change `"BaseAddress": "http://inference"` to `"BaseAddress": "http://embedding"` | |  |
| TASK-002 | In `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.Options.cs` line 7, change the `BaseAddress` default from `"http://inference"` to `"http://embedding"` | |  |

### Implementation Phase 2

- GOAL-002: Eliminate redundant dual registration — remove the `Inference` entry from `Http:Clients` in appsettings since the typed client registration via `AddInferenceClient` + `AddTypedHttpClient` handles everything (base address, timeout, headers, correlation, resilience)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | In `service/Api/src/Api/appsettings.json` lines 91-98, remove the `"Inference": { ... }` entry from `"Http":{"Clients":{...}}` — after removal, `"Clients"` will be an empty object `{}` | |  |
| TASK-004 | Run `dotnet build service/Api/src/Api/Api.csproj` — must succeed with zero warnings and errors | |  |
| TASK-005 | Run `dotnet test service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj` — all HTTP extension tests pass (the `AddHttpClients` method handles empty `Clients` dictionary gracefully — the `foreach` loop simply does nothing) | |  |
| TASK-006 | Run `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj` — all inference client tests pass | |  |

### Implementation Phase 3

- GOAL-003: Add missing ML runtime dependencies to the embedding service's `pyproject.toml`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | In `service/Embedding/pyproject.toml`, add `"torch>=2.0"` and `"open-clip-torch>=1.0"` to the `dependencies` list in `[project]` section | |  |
| TASK-008 | Verify the embedding service Python environment can resolve the new dependencies by running `cd service/Embedding && uv sync` — must complete without errors | |  |

## 3. Alternatives

- **ALT-001**: Rename the Aspire resource from `Services.Embedding` to `Services.Inference` to match the existing `BaseAddress: "http://inference"`. Rejected because the service's purpose is embedding generation, not inference — the name `Embedding` accurately describes its function. Renaming would create confusion between the service name and the .NET client class name.
- **ALT-002**: Keep the `"Inference"` entry in `Http:Clients` as a named client and refactor `AddInferenceClient` to use `services.AddHttpClient<IInferenceClient, InferenceClient>("Inference")` referencing the named client. Rejected because it adds indirection without benefit — the typed client already registers itself correctly with `AddTypedHttpClient`.
- **ALT-003**: Use the `AddServiceDiscovery()` extension explicitly on the typed client instead of relying on `ConfigureHttpClientDefaults`. Rejected because `ConfigureHttpClientDefaults` already applies it to all clients; explicit duplication is unnecessary.

## 4. Dependencies

- **DEP-001**: `Aspire.Hosting.Python` NuGet package — already referenced by `ReSys.AppHost.csproj` (provides `AddUvicornApp`)
- **DEP-002**: `Microsoft.Extensions.ServiceDiscovery` — already added via `ReSys.ServiceDefaults/Extensions.cs` (`AddServiceDiscovery()`)
- **DEP-003**: No new NuGet packages required for the .NET side
- **DEP-004**: Python `torch` and `open-clip-torch` packages — added to `pyproject.toml` dependencies

## 5. Files

| File | Change |
|------|--------|
| `service/Api/src/Api/appsettings.json` | **MODIFY** — change `BaseAddress` from `"http://inference"` to `"http://embedding"` and remove redundant `"Inference"` entry from `Http:Clients` |
| `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.Options.cs` | **MODIFY** — change default `BaseAddress` from `"http://inference"` to `"http://embedding"` |
| `service/Embedding/pyproject.toml` | **MODIFY** — add `torch` and `open-clip-torch` to dependencies |

## 6. Testing

- **TEST-001**: `dotnet build` on Api, Module, and Shared projects succeeds
- **TEST-002**: `dotnet test` on `Shared.UnitTests` — all 2367 tests pass
- **TEST-003**: `dotnet test` on `Module.UnitTests` — all 1794 tests pass
- **TEST-004**: `uv sync` in `service/Embedding/` completes without errors

## 7. Risks & Assumptions

- **RISK-001**: Changing `BaseAddress` from `"http://inference"` to `"http://embedding"` may affect developers who have overridden the address via environment variables (`Http__Clients__Inference__BaseAddress`). Mitigation: document the change in commit message and update local overrides.
- **RISK-002**: Removing the `Inference` entry from `Http:Clients` means the `NamedClientOptions` validator (`HttpOptionsValidator`) will no longer validate the inference client config. Mitigation: the typed client registration still reads the same config section (`Http:Clients:Inference`) via `InferenceClientOptions` and applies timeout/headers — validation is preserved at the module level.
- **RISK-003**: `torch` and `open-clip-torch` are large packages (multiple GBs) — `uv sync` may take significant time and disk space. Mitigation: documented in the task as expected.
- **ASSUMPTION-001**: Aspire's `AddServiceDiscovery()` from `ConfigureHttpClientDefaults` correctly intercepts and resolves `http://embedding` to the actual Embedding service URL injected via `WithReference(embedding)` in AppHost.cs.
- **ASSUMPTION-002**: The `AddHttpClients()` `foreach` loop handles an empty `Clients` dictionary without error (the loop body simply never executes).

## 8. Related Specifications / Further Reading

- [Existing refactoring plan](plan/refactor-inference-client-shared-http-1.md)
- [Existing options refactoring plan](plan/refactor-inference-client-options-1.md)
- [Aspire AppHost](infra/Aspire/src/ReSys.AppHost/AppHost.cs)
- [ServiceDefaults Extensions](infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs)
- [HTTP client registration](service/Api/src/Shared/Operational/Http/Http.Extensions.cs)
- [Inference client DI](service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.DependencyInjection.cs)
