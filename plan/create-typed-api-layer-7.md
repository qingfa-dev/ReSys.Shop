---
goal: Create typed API client factories, module-specific query builders, and standardized paged result models aligned to backend Shared/Application/Models
version: 1.0
date_created: 2026-07-07
status: 'In progress'
last_updated: 2026-07-07
tags: refactor, api, types, query-builder, models
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The Admin frontend API layer has a strong foundation (Axios client, CRUD factory, QueryBuilder, ApiResult types), but it lacks **typed module-specific API client contracts** and **standardized query parameter types** that exactly mirror the backend `Shared/Application/Models/Querying/` infrastructure. The backend defines a precise `QueryingParameters` record (with `filter`, `search`, `searchFields`, `searchMode`, `sort`, `page`, `pageSize`) and each module has domain-specific CQRS query types — the frontend should have matching TypeScript types and factory functions.

This plan closes the gap by:
- Creating a `ServerQueryingParameters` type that is a 1:1 TypeScript mirror of the backend `QueryingParameters` record
- Creating module-specific `*SearchParams` types that extend it with domain filter/sort helpers
- Creating typed endpoint factories per backend module (Catalog, Identity, Orders, Inventory, Profile, Location, Reports) that wrap `createCrudService` and add module-specific endpoint methods
- Standardizing paged result consumption across all stores with a shared `usePagedList` composable
- Removing ad-hoc `params: any` and hardcoded URL strings from feature services

## 1. Requirements & Constraints

- **REQ-001**: Create `ServerQueryingParameters` TypeScript type that mirrors backend `QueryingParameters` record field-by-field (camelCase JSON serialization)
- **REQ-002**: Every feature module must have a typed `*SearchParams` type extending `ServerQueryingParameters` with domain-specific filter condition helpers
- **REQ-003**: Create a `createModuleApi<TModule>` factory that returns a typed client with all module endpoints (not just CRUD), organized by sub-resource
- **REQ-004**: Replace all remaining `params: any` and raw `apiClient.get(...)` with properly typed calls through the module API client
- **REQ-005**: Create a `usePagedList` composable that standardizes store state management for paged data (items, totalRecords, loading, page, pageSize, fetch, sort, search)
- **CON-001**: Do NOT change the Axios interceptor, `parseApiError`, or `ApiResult` discriminated union — they are correct and stable
- **CON-002**: All new query parameter types must serialize to query strings that the backend `QueryingParameters` model binder understands
- **CON-003**: Every service that returns `ApiResult<T[]>` from a paged endpoint must also return `PaginationMeta` in `meta`
- **PAT-001**: Follow the existing `ProductSearchParams` / `OrderSearchParams` pattern for all new module param types
- **PAT-002**: Each module API client should be a plain object with methods (not a class), matching the existing service pattern
- **PAT-003**: Module API clients import `createCrudService` + `apiClient` from `@/shared/api` barrel

## 2. Implementation Steps

### Implementation Phase 1: Create `ServerQueryingParameters` root type

- GOAL-001: Create a TypeScript type that is the exact 1:1 mirror of the backend `Shared/Application/Models/Querying/Querying.Parameters.cs` record, ensuring all query string serialization matches.

**Backend `QueryingParameters` reference:**

| Property | Type | C# Source | JSON/Query name |
|----------|------|-----------|-----------------|
| `Filter` | `string?` | `IFilteringParameters` | `filter` |
| `Search` | `string?` | `ISearchingParameters` | `search` |
| `SearchFields` | `string[]?` | `ISearchingParameters` | `searchFields` |
| `SearchMode` | `SearchMode?` | `ISearchingParameters` | `searchMode` |
| `Sort` | `string[]?` | `ISortingParameters` | `sort` |
| `PageNumber` | `int?` | `IPagingParameters` | `page` (JsonPropertyName) |
| `PageSize` | `int?` | `IPagingParameters` | `pageSize` (JsonPropertyName) |

The existing frontend `QueryParams` type (in `query-params.types.ts`) already covers these fields but with slightly different naming: `page` already matches, `pageSize` matches, `sort` matches, `search` matches, `searchFields` matches, `searchMode` matches, `filter` matches. The only gap is that `QueryParams` may lack `SearchMode` type alignment — verify and harden.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Read `src/shared/api/types/query-params.types.ts` — rename `QueryParams` to `ServerQueryingParameters` and re-export as alias `QueryParams = ServerQueryingParameters` for backward compat. Ensure `SearchMode` type aligns to the backend `SearchMode` enum (`'any' \| 'all'`). | | |
| TASK-002 | Update all barrel exports (`types/index.ts`, `api/index.ts`) to export `ServerQueryingParameters`. | | |
| TASK-003 | Run `pnpm type-check` — verify zero errors after rename. | | |

### Implementation Phase 2: Create module-specific `*SearchParams` types

- GOAL-002: Every feature module that has a paged list endpoint must have its own `*SearchParams` type extending `ServerQueryingParameters` with domain-specific filter helpers using the `QueryBuilder<T>` fluent API.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Read `src/features/catalog/products/types/product.types.ts` — verify `ProductSearchParams` exists and extends `ServerQueryingParameters`. If it uses `QueryParams`, update to `ServerQueryingParameters`. | | |
| TASK-005 | Read `src/features/ordering/types/order.types.ts` — verify `OrderSearchParams` exists and extends `ServerQueryingParameters`. | | |
| TASK-006 | Read `src/features/users/types/user.types.ts` — verify `UserSearchParams` exists and extends `ServerQueryingParameters`. Add if missing. | | |
| TASK-007 | Read `src/features/inventories/types/inventory.types.ts` — verify `InventorySearchParams` exists and extends `ServerQueryingParameters`. Add if missing. | | |
| TASK-008 | Read `src/features/catalog/taxonomies/types/taxonomy.types.ts` — verify `TaxonomyQuery` exists and extends `ServerQueryingParameters`. Add `TaxonSearchParams` for taxon list endpoint. | | |
| TASK-009 | Read `src/features/catalog/option-types/types/option-type.types.ts` — add `OptionTypeSearchParams` extending `ServerQueryingParameters`. | | |
| TASK-010 | Read `src/features/catalog/property-types/types/property-type.types.ts` — add `PropertyTypeSearchParams` extending `ServerQueryingParameters`. | | |
| TASK-011 | Read `src/features/location/types/country.types.ts` — add `CountrySearchParams` extending `ServerQueryingParameters`. | | |
| TASK-012 | Read `src/features/location/types/state.types.ts` — add `StateSearchParams` extending `ServerQueryingParameters`. | | |
| TASK-013 | Read `src/features/reports/types/report.types.ts` — add `ReportSearchParams` extending `ServerQueryingParameters`. | | |

### Implementation Phase 3: Create module API client factories

- GOAL-003: Create typed module API clients that expose all endpoints for each backend module, wrapping `createCrudService` and adding typed sub-resource methods. Replace ad-hoc `apiClient.get(...)` calls in existing services.

**Module factory pattern:**
```typescript
// src/shared/api/services/module-api.factory.ts
import { createCrudService } from './crud.service'
import type { ApiResult } from '../types/api.types'

export function createModuleApi<TModule>(config: {
  basePath: string
  endpoints: Record<string, string>
}) {
  const crud = createCrudService<TModule>(config.basePath)
  return {
    ...crud,
    ...Object.fromEntries(
      Object.entries(config.endpoints).map(([name, path]) => [
        name,
        <T>(params?: Record<string, unknown>) =>
          apiClient.get<T>(`${config.basePath}${path}`, { params }) as Promise<ApiResult<T>>,
      ])
    ),
  }
}
```

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Create `src/shared/api/services/module-api.factory.ts` — implement `createModuleApi<TModule>(config)` that returns: factory CRUD methods + a `getSubResource<T>(path, params?)` method for sub-resource GET endpoints + a `postSubResource<T>(path, data)` method for sub-resource POST endpoints. | | |
| TASK-015 | Create `src/features/catalog/services/catalog.api.ts` — typed API client for all catalog endpoints using `createModuleApi` (products, variants, option-types, option-values, property-types, taxonomies, taxons). Export as `catalogApi`. | | |
| TASK-016 | Create `src/features/identity/services/identity.api.ts` — typed API client for identity endpoints (users, roles, permissions). Export as `identityApi`. | | |
| TASK-017 | Create `src/features/ordering/services/ordering.api.ts` — typed API client for ordering endpoints (orders, shipments, fulfillments). Export as `orderingApi`. | | |
| TASK-018 | Create `src/features/inventories/services/inventory.api.ts` — typed API client for inventory endpoints (stocks, locations, units, movements, transfers). Export as `inventoryApi`. | | |
| TASK-019 | Create `src/features/location/services/location.api.ts` — typed API client for location endpoints (countries, states). Export as `locationApi`. | | |
| TASK-020 | Create `src/features/profile/services/profile.api.ts` — typed API client for profile endpoints. Export as `profileApi`. | | |
| TASK-021 | Create `src/features/reports/services/reports.api.ts` — typed API client for reports endpoints. Export as `reportsApi`. | | |

### Implementation Phase 4: Refactor existing services to use module API clients

- GOAL-004: Replace the 19 hand-written feature services with calls to the new typed module API clients. Each existing service file becomes a thin re-export or delegate.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-022 | Refactor `product.service.ts` — replace all `apiClient.get/post/put/delete` with calls to `catalogApi.products.*` and `catalogApi.products.images.*`, `catalogApi.products.properties.*`, `catalogApi.products.optionTypes.*`. | | |
| TASK-023 | Refactor `variant.service.ts` — replace with `catalogApi.variants.*`. | | |
| TASK-024 | Refactor `option-type.service.ts` — replace with `catalogApi.optionTypes.*`. | | |
| TASK-025 | Refactor `option-value.service.ts` — replace with `catalogApi.optionValues.*`. | | |
| TASK-026 | Refactor `property-type.service.ts` — replace with `catalogApi.propertyTypes.*`. | | |
| TASK-027 | Refactor `taxonomy.service.ts` — replace with `catalogApi.taxonomies.*`. | | |
| TASK-028 | Refactor `taxon.service.ts` — replace with `catalogApi.taxons.*`. | | |
| TASK-029 | Refactor `user.service.ts` — replace with `identityApi.users.*`. | | |
| TASK-030 | Refactor `role.service.ts` — replace with `identityApi.roles.*`. | | |
| TASK-031 | Refactor `permission.service.ts` — replace with `identityApi.permissions.*`. | | |
| TASK-032 | Refactor `order.service.ts` — replace with `orderingApi.orders.*`. | | |
| TASK-033 | Refactor `fulfillment.service.ts` — replace with `orderingApi.fulfillments.*`. | | |
| TASK-034 | Refactor `inventory.service.ts` — replace with `inventoryApi.*` (stocks, locations, units, movements, transfers). | | |
| TASK-035 | Refactor `country.service.ts` — replace with `locationApi.countries.*`. | | |
| TASK-036 | Refactor `state.service.ts` — replace with `locationApi.states.*`. | | |
| TASK-037 | Refactor `profile.service.ts` — replace with `profileApi.*`. | | |
| TASK-038 | Refactor `report.service.ts` — replace with `reportsApi.*`. | | |
| TASK-039 | Refactor `auth.service.ts` — keep as-is (it uses non-standard endpoints not matching CRUD pattern). | | |
| TASK-040 | Refactor `catalog-dashboard.service.ts` — replace with `catalogApi.dashboard.*`. | | |

### Implementation Phase 5: Create `usePagedList` composable

- GOAL-005: Create a shared Pinia-friendly composable that standardizes how stores manage paged list state (items array, pagination meta, loading state, fetch/refresh/sort/search/page actions).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-041 | Create `src/shared/composables/paged-list.use.ts` — implement `usePagedList<TItem, TParams extends ServerQueryingParameters>(fetchFn, defaultParams?)` that returns: `items`, `totalRecords`, `loading`, `page`, `pageSize`, `fetch(params?)`, `setPage(p)`, `setSort(field, dir)`, `setSearch(text, fields?)`, `refresh()`. | | |
| TASK-042 | Write tests for `paged-list.use.spec.ts` — verify: fetch populates items + meta, setPage calls fetch with correct page, setSort updates sort param, setSearch updates search param, error returns empty items. | | |

### Implementation Phase 6: Migrate stores to `usePagedList`

- GOAL-006: Refactor existing Pinia stores that manage paged lists to use the `usePagedList` composable, eliminating repetitive boilerplate.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-043 | Read `product.store.ts` — refactor `fetchProducts` to use `usePagedList` composable; keep domain-specific actions (create, update, delete). | | |
| TASK-044 | Read `order.store.ts` — refactor `fetchOrders` to use `usePagedList`. | | |
| TASK-045 | Read `user.store.ts` — refactor `fetchAdmins` to use `usePagedList`. | | |
| TASK-046 | Read `option-type.store.ts` — refactor `fetchList` to use `usePagedList`. | | |
| TASK-047 | Read `property-type.store.ts` — refactor `fetchList` to use `usePagedList`. | | |
| TASK-048 | Read `inventory.store.ts` — refactor stock/list location listing to use `usePagedList`. | | |
| TASK-049 | Read `fulfillment.store.ts` — refactor to use `usePagedList`. | | |
| TASK-050 | Read `report.store.ts` — refactor to use `usePagedList`. | | |

### Implementation Phase 7: Verify

- GOAL-007: Run full verification suite.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-051 | Run `pnpm type-check` — zero errors. | | |
| TASK-052 | Run `pnpm build-only` — passes. | | |
| TASK-053 | Run `pnpm test:unit` — all 107+ tests pass. | | |

## 3. Alternatives

- **ALT-001**: Keep the current per-service pattern (hand-written methods + `createCrudService` spread). Rejected because services still have inconsistent typing (`params: any` in some), hardcoded URL strings, and no centralized endpoint registry. The module API factory eliminates these issues.
- **ALT-002**: Use OpenAPI codegen (e.g., `openapi-typescript`) to auto-generate the API clients from the backend Swagger doc. Rejected because the backend Swagger doc may not cover all edge cases, and codegen output is harder to customize for the interceptor's unwrapping pattern.
- **ALT-003**: Create a single monolithic API client with all endpoints. Rejected because it violates the modular monolith principle and creates unnecessary coupling between feature modules.

## 4. Dependencies

- **DEP-001**: Phases 0-6 of prior work (Plan 6) — the frontend types (`ServerResult<T>`, `ServerPagedResult<T>`, `ServerError`, `ErrorType`, `ParsedApiError`) must be stable and aligned with backend models before creating typed API clients.
- **DEP-002**: The `createCrudService` factory and `QueryBuilder` must continue to work unchanged — module API clients wrap rather than replace them.
- **DEP-003**: The Axios response interceptor's unwrapping behavior (items→data, value→data) must remain stable since all typed return values depend on it.

## 5. Files

- **FILE-001**: `src/shared/api/types/query-params.types.ts` — rename `QueryParams` to `ServerQueryingParameters`
- **FILE-002**: `src/shared/api/types/index.ts` — export `ServerQueryingParameters`
- **FILE-003 to FILE-012**: Module type files — add `*SearchParams` types
- **FILE-013**: `src/shared/api/services/module-api.factory.ts` — NEW, typed module API factory
- **FILE-014 to FILE-020**: `*.api.ts` files — NEW, one per backend module
- **FILE-021 to FILE-039**: Feature service files — refactored to use module API clients
- **FILE-040**: `src/shared/composables/paged-list.use.ts` — NEW, paged list composable
- **FILE-041**: `src/shared/composables/paged-list.use.spec.ts` — NEW, tests
- **FILE-042 to FILE-049**: Store files — refactored to use `usePagedList`

## 6. Testing

- **TEST-001**: After Phase 1 — `pnpm type-check` passes with `ServerQueryingParameters` rename
- **TEST-002**: After Phase 3 — module API factory unit tests verify correct method generation
- **TEST-003**: After Phase 5 — `usePagedList` composable tests verify fetch/pagination/sort/search behavior
- **TEST-004**: After Phase 7 — `pnpm type-check` zero errors, `pnpm build-only` passes, `pnpm test:unit` all pass

## 7. Risks & Assumptions

- **RISK-001**: Creating module API clients is a large refactor (19 services → 8 module clients + thin re-exports). Store references to service methods will need updating. Execute Phase 4 and Phase 6 in the same pass to avoid intermediate breakage.
- **RISK-002**: The `usePagedList` composable may not cover all store patterns (some stores have unique state like `currentItem`, `selectedItems`, etc.). Phase 6 should handle exceptions on a case-by-case basis.
- **RISK-003**: `ServerQueryingParameters` rename from `QueryParams` may break imports in feature stores and services that reference the old name. TASK-003 must verify with `pnpm type-check`.
- **ASSUMPTION-001**: The backend `QueryingParameters` JSON serialization uses camelCase (System.Text.Json default), matching the existing frontend `QueryParams` field names.
- **ASSUMPTION-002**: All paged endpoints return data in the `{ items, totalCount, page, pageSize }` shape that the interceptor unwraps to `{ data: T[], meta: PaginationMeta }`. Any endpoint that deviates from this pattern will need special handling.
- **ASSUMPTION-003**: The `createModuleApi` factory pattern is flexible enough to cover all endpoint patterns (simple CRUD, sub-resource CRUD, custom actions). Complex endpoints (like `uploadImage` with `multipart/form-data`) may need manual methods.

## 8. Related Specifications / Further Reading

- `plan/fix-api-duplication-6.md` — prior plan: removed `PagedList`, standardized models, consolidated services
- `service/Api/src/Shared/Application/Models/Querying/Querying.Parameters.cs` — backend `QueryingParameters` record definition
- `service/Api/src/Shared/Application/Models/Querying/Paging.Parameters.cs` — backend `IPagingParameters` interface
- `service/Api/src/Shared/Application/Models/Querying/Sorting.Parameters.cs` — backend `ISortingParameters` interface
- `service/Api/src/Shared/Application/Models/Querying/Searching.Parameters.cs` — backend `ISearchingParameters` interface
- `service/Api/src/Shared/Application/Models/Querying/Filtering.Parameters.cs` — backend `IFilteringParameters` interface
- `service/Api/src/Shared/Application/Models/Results/PagedResult.cs` — backend `PagedResult<T>` record
