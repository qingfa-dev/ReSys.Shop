---
goal: Implement missing GET /api/storefront/catalog/products/images/inferences endpoint
version: 1.2
date_created: 2026-08-12
last_updated: 2026-08-12
owner: Backend
status: 'Completed'
tags: ['feature', 'api', 'catalog', 'visual-search']
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

The Storefront SPA calls `GET /api/storefront/catalog/products/images/inferences` to list available visual search ML models, but no backend endpoint existed. This plan adds the missing endpoint following the vertical slice pattern, reusing `IInferenceClient` from the Admin embeddings module. Returns `PagedResult<Response>` per codebase convention.

## 1. Requirements & Constraints

- **REQ-001**: Endpoint returns `PagedResult<Response>` where each item has `id`, `name`, `description`, `dimension`, `isOnnx`
- **REQ-002**: Handler uses `IPagedQuery<Response>` / `IPagedQueryHandler<Query, Response>` pattern
- **REQ-003**: `Response` record inherits from `VisualSearchModelResponse` (shared model in `Shared/Models/`)
- **REQ-004**: Handler calls `IInferenceClient.ListModelsAsync()` and maps `ModelMetadata` → `Response`
- **REQ-005**: Route is `GET api/storefront/catalog/products/images/inferences`
- **REQ-006**: No authentication required (public storefront endpoint)
- **CON-001**: Follow vertical slice pattern: Handler, Response, Endpoint in separate files
- **CON-002**: Shared model `VisualSearchModelResponse` in `Inferences/Shared/Models/` — properties mirror `ModelMetadata` but as a record
- **CON-003**: Mapping is explicit in handler (not implicit via inheritance) because `ModelMetadata` is a class, not a record
- **GUD-001**: Use `static partial class` for handler, matching `SearchByImage` and `GetStoreOptionTypes` patterns
- **PAT-001**: Follow `GetStoreOptionTypes` structure — `Query : IPagedQuery<Response>`, `PagedQueryHandler`, `Response` record, `Endpoint` with `.ToPagedResult()`

## 2. Implementation Steps

### Implementation Phase 1

- GOAL-001: Add backend endpoint for listing visual search ML models

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Add route constant `Inferences` in `CatalogFeature.Storefront.cs` under `Products.Images` | ✅ | 2026-08-12 |
| TASK-002 | Create `VisualSearchModelResponse` record in `Inferences/Shared/Models/VisualSearchModel.Response.cs` with `Id`, `Name`, `Dimension`, `Description`, `IsOnnx` | ✅ | 2026-08-12 |
| TASK-003 | Create `Response` record inheriting `VisualSearchModelResponse` in `Inferences/Get/GetVisualSearchModels.Response.cs` | ✅ | 2026-08-12 |
| TASK-004 | Create handler `GetVisualSearchModels` in `Inferences/Get/GetVisualSearchModels.cs` — `IPagedQuery<Response>`, inject `IInferenceClient`, map `ModelMetadata` → `Response` | ✅ | 2026-08-12 |
| TASK-005 | Create endpoint `GetVisualSearchModels.Endpoint.cs` — map GET route, `.ToPagedResult()`, produce `PagedResult<Response>` | ✅ | 2026-08-12 |

### Implementation Phase 2

- GOAL-002: Update frontend to match PagedResult response

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Update `searchByImageApi.ts` — return type `PagedResult<VisualSearchModel>`, access `.items` | ✅ | 2026-08-12 |
| TASK-007 | Update `useVisualSearch.ts` `loadModels()` — read `result.items` instead of `result.value` | ✅ | 2026-08-12 |
| TASK-008 | Add parameter descriptions to `VisualSearchView.vue` — Model, Results, Min Match %, Score Weight | ✅ | 2026-08-12 |

### Implementation Phase 3

- GOAL-003: Verify build

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | `dotnet build service/Api/src/Api/` — 0 errors | ✅ | 2026-08-12 |

## 3. Alternatives

- **ALT-001**: Return `Result<List<ModelMetadata>>` directly — rejected because it violates the `PagedResult` convention used by all other storefront endpoints
- **ALT-002**: Make `Response` inherit `ModelMetadata` via class inheritance — rejected because `Response` must be a record and records can't inherit from classes

## 4. Dependencies

- **DEP-001**: `IInferenceClient` registered via `InferenceClientDependencyInjection.AddInferenceClient()` (already done in `Catalog.Extension.cs:25`)
- **DEP-002**: Python Embedding sidecar running and reachable at configured `InferenceClientSetting.BaseAddress`

## 5. Files

- **FILE-001**: `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Storefront.cs` — added route constant
- **FILE-002**: `service/Api/src/Module/Catalog/Features/Storefront/Products/Images/Inferences/Shared/Models/VisualSearchModel.Response.cs` — new shared model
- **FILE-003**: `service/Api/src/Module/Catalog/Features/Storefront/Products/Images/Inferences/Get/GetVisualSearchModels.Response.cs` — new response record
- **FILE-004**: `service/Api/src/Module/Catalog/Features/Storefront/Products/Images/Inferences/Get/GetVisualSearchModels.cs` — new handler
- **FILE-005**: `service/Api/src/Module/Catalog/Features/Storefront/Products/Images/Inferences/Get/GetVisualSearchModels.Endpoint.cs` — new endpoint
- **FILE-006**: `app/Store/src/features/catalog/services/searchByImageApi.ts` — updated to PagedResult
- **FILE-007**: `app/Store/src/features/catalog/composables/useVisualSearch.ts` — updated loadModels
- **FILE-008**: `app/Store/src/features/catalog/views/VisualSearchView.vue` — added parameter descriptions

## 6. Testing

- **TEST-001**: `dotnet build service/Api/src/Api/` passes with no errors — ✅ verified
- **TEST-002**: Manual test: `GET https://localhost:5001/api/storefront/catalog/products/images/inferences` returns 200 with paged model list

## 7. Risks & Assumptions

- **RISK-001**: If Python Embedding sidecar is not running, endpoint returns 502 from `IInferenceClient` error — acceptable degradation
- **ASSUMPTION-001**: Frontend `VisualSearchModel` interface fields (`id`, `name`, `description`, `dimension`, `isOnnx`) match `VisualSearchModelResponse` properties via camelCase serialization

## 8. Related Specifications / Further Reading

- `service/Api/src/Module/Catalog/Features/Storefront/Products/Images/Search/SearchByImage.cs` — existing storefront image feature pattern
- `service/Api/src/Module/Catalog/Features/Admin/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.Interface.cs` — `IInferenceClient` interface
- `service/Api/src/Module/Catalog/Features/Admin/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.Models.cs` — `ModelMetadata` class
- `app/Store/src/features/catalog/services/searchByImageApi.ts` — frontend consumer
