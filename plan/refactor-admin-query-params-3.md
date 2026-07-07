---
goal: Standardize frontend query parameter types and serialization to match backend Specifications module
version: 1.0
date_created: 2026-07-07
status: 'Planned'
tags: refactor, frontend, query-params, alignment
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The backend's `QueryingParameters` record (`service/Api/src/Shared/Operational/Persistence/Specifications/Querying/Querying.Parameters.cs`) is the canonical parameter type for **all 23+ paged list endpoints**, used via `[AsParameters]` model binding. It defines 7 query-string parameters:

| Backend property | Query parameter | Type | Example |
|---|---|---|---|
| `Filter` | `filter` | `string` | `name:=:foo` or JSON |
| `Search` | `search` | `string` | `iphone` |
| `SearchFields` | `searchFields` | `string[]` | `searchFields=name&searchFields=description` |
| `SearchMode` | `searchMode` | `string` | `any` or `all` |
| `Sort` | `sort` | `string[]` | `sort=-createdAt&sort=name` (DSL: `-field`=desc, `field`=asc) |
| `PageNumber` | `page` | `int` | `page=1` |
| `PageSize` | `pageSize` | `int` | `pageSize=20` |

The frontend currently has: no shared `QueryParams` type, inconsistent snake_case/camelCase across features, a `QueryBuilder` that emits `page_size`/`search_field` (wrong names), and sort sent as `sort_by`+`is_descending` (wrong structure). This plan aligns the frontend with the canonical backend model.

## 1. Requirements & Constraints

- **REQ-001**: Create a shared `QueryParams` TypeScript interface mirroring the backend `QueryingParameters` record
- **REQ-002**: Standardize all 10 feature-level search-param types to use/extend the shared `QueryParams`
- **REQ-003**: Update `QueryBuilder.build()` to emit `pageSize`/`searchFields`/`searchMode`/`sort`(as array) instead of `page_size`/`search_field`/`sort`(as comma-joined string)
- **REQ-004**: Update the CRUD service factory `list()` signature to accept `QueryParams` instead of `Record<string, unknown>`
- **REQ-005**: Update all stores to use the new standardized types when calling service.list()
- **REQ-006**: Ensure the Axios `paramsSerializer` (`indexes: null`) correctly serializes `string[]` params as `?key=a&key=b` (already correct — no change needed)
- **CON-001**: Do NOT change the response types (`ApiResult`, `PaginationMeta`, `ServerPagedResult`) — those were already aligned in Phase 1/2
- **CON-002**: Do NOT rename feature model types (Product, Order, User, etc.) — only query parameter types
- **CON-003**: `sort` field in `QueryBuilder` must output array entries, not a comma-joined string
- **GUD-001**: Backwards compatibility — existing endpoints still receiving `page_size` via snake_case model binding will continue to work (ASP.NET model binding is case-insensitive), but new code should use canonical casing

## 2. Implementation Steps

### Implementation Phase 1: Create shared `QueryParams` types

- GOAL-001: Create shared TypeScript interfaces mirroring the backend `QueryingParameters`, `FilterModel`, `SearchModel`, `SortModel`, `PageModel`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `src/shared/api/types/query-params.types.ts` with `PagingParams` interface (`page?`, `pageSize?`) | | |
| TASK-002 | Add `SortDirection` type (`'asc' \| 'desc'`) and `SortClause` interface (`field`, `direction?`, `nulls?`) | | |
| TASK-003 | Add `SortParams` interface (`sort?: string[]`) using backend DSL format per entry | | |
| TASK-004 | Add `SearchParams` interface (`search?`, `searchFields?`, `searchMode?: 'any' \| 'all'`) | | |
| TASK-005 | Add `FilterParams` interface (`filter?: string`) — raw DSL or JSON string | | |
| TASK-006 | Add `QueryParams` interface extending all of the above (mirrors `QueryingParameters`) | | |
| TASK-007 | Add `AllowedFields` helper type for backend field whitelist support | | |
| TASK-008 | Re-export all new types from `src/shared/api/types/index.ts` | | |

### Implementation Phase 2: Update `QueryBuilder.utils` to emit canonical names

- GOAL-002: Refactor the `QueryBuilder.build()` return type and output to match backend parameter names and structure.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Read `src/shared/utils/query-builder.utils.ts` fully | | |
| TASK-010 | Change `build()` return type from `{ page_size?, search_field? }` to `QueryParams` (`pageSize`, `searchFields`, `searchMode`) | | |
| TASK-011 | Change `page()` setter to store as `pageSize` instead of `page_size` in build output | | |
| TASK-012 | Change `search()` setter to store as `searchFields` instead of `search_field` in build output | | |
| TASK-013 | Add `searchMode()` method to `QueryBuilder` (enum: `'any'` / `'all'`) | | |
| TASK-014 | Change `sort` output from comma-joined string to `string[]` — each sort clause becomes a separate array entry DSL-formatted: `field` (asc) or `-field` (desc) | | |
| TASK-015 | Update JSDoc comments in `QueryBuilder` to reflect new parameter names | | |
| TASK-016 | Read `src/shared/utils/query-builder.utils.spec.ts` and update all test assertions to match the new output shape | | |

### Implementation Phase 3: Standardize feature query-param interfaces

- GOAL-003: Make every feature's search-param type either extend `QueryParams` or be replaced by it, normalizing to camelCase backend names.

**Catalog module — ProductSearchParams**
| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-017 | Read `src/features/catalog/products/types/product.types.ts` | | |
| TASK-018 | Replace `ProductSearchParams` body with `type ProductSearchParams = QueryParams & { status?: ProductStatus; taxonId?: string; season?: string }` (matching the backend `GetProductsPaged.Parameters` subclass fields) | | |

**Catalog module — TaxonomyQuery**
| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-019 | Read `src/features/catalog/taxonomies/types/taxonomy.types.ts` | | |
| TASK-020 | Replace `TaxonomyQuery` body with `type TaxonomyQuery = QueryParams` (backend `GetTaxonomiesPaged.Parameters` has no extra fields) | | |

**Catalog module — OptionTypeQuery**
| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-021 | Read `src/features/catalog/option-types/types/option-type.types.ts` | | |
| TASK-022 | Replace `OptionTypeQuery` body with `type OptionTypeQuery = QueryParams` (backend `GetOptionTypesPaged.Parameters` has no extra fields) | | |

**Catalog module — OptionValueQuery**
| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-023 | Read `src/features/catalog/option-types/option-values/types/option-value.types.ts` | | |
| TASK-024 | Replace `OptionValueQuery` body with `type OptionValueQuery = QueryParams & { optionTypeId?: string }` (backend `GetOptionValuesPaged.Parameters` may have extra fields — check the C# file) | | |

**Catalog module — PropertyTypeQuery**
| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-025 | Read `src/features/catalog/property-types/types/property-type.types.ts` | | |
| TASK-026 | Replace `PropertyTypeQuery` body with `type PropertyTypeQuery = QueryParams` | | |

**Catalog module — TaxonQuery**
| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-027 | Read `src/features/catalog/taxonomies/taxa/types/taxon.types.ts` | | |
| TASK-028 | Replace `TaxonQuery` body with `type TaxonQuery = QueryParams & { taxonomyId?: string[]; focusedTaxonId?: string; includeLeavesOnly?: boolean; includeHidden?: boolean; maxDepth?: number }` | | |

**Ordering module — OrderSearchParams**
| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-029 | Read `src/features/ordering/types/order.types.ts` | | |
| TASK-030 | Update `OrderSearchParams` to extend `QueryParams` and keep/extend with any extra fields like `state?`, `storeId?`, `warehouseId?`, `fromDate?`, `toDate?` | | |

**Users module — UserSearchParams**
| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-031 | Read `src/features/users/types/user.types.ts` | | |
| TASK-032 | Update `UserSearchParams` to extend `QueryParams` and keep `isActive?`, `role?` | | |

**Inventory module — InventorySearchParams**
| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-033 | Read `src/features/inventories/types/inventory.types.ts` | | |
| TASK-034 | Update `InventorySearchParams` to extend `QueryParams` and keep `lowStock?` | | |
| TASK-035 | Update `InventoryUnitSearchParams` and `StockMovementSearchParams` to extend `InventorySearchParams` | | |

**Reports module — DashboardQuery**
| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-036 | Read `src/features/reports/types/report.types.ts` | | |
| TASK-037 | Update `DashboardQuery` to extend `QueryParams` | | |

### Implementation Phase 4: Update CRUD service factory

- GOAL-004: Add a `QueryParams`-based `list()` overload to the CRUD factory while maintaining backwards compatibility.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-038 | Read `src/shared/api/services/crud.service.ts` | | |
| TASK-039 | Add `list(params?: QueryParams): Promise<ApiResult<T[]>>` — typed overload alongside the existing `Record<string, unknown>` version, OR replace the existing signature | | |

### Implementation Phase 5: Update stores to use new types

- GOAL-005: Update all stores to use the new standardized types in their query refs and fetch methods.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-040 | Read all store files: update `query` ref type from old types to new standardized types — 12 files total (product, taxonomy, taxon, option-type, option-value, property-type, order, user, inventory, fulfillment, country, state) | | |

### Implementation Phase 6: Update template usages

- GOAL-006: Fix `.vue` template files that directly construct query param objects with deprecated names.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-041 | Search for all occurrences of `page_size`, `sort_by`, `is_descending`, `search_field` in `.vue` files — update to new naming | | |
| TASK-042 | Update `pnpm type-check` to verify all changes compile without type errors | | |

### Implementation Phase 7: Verify

- GOAL-007: Run all verification steps.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-043 | Run `pnpm type-check` and confirm zero new errors (only pre-existing `@primevue/core/api`, `metadata-manager`, `GlobalSearch.$t`) | | |
| TASK-044 | Run `pnpm build-only` — confirm Vite build passes | | |
| TASK-045 | Run `pnpm test:unit` — confirm all 107 tests pass | | |

## 3. Alternatives

- **ALT-001**: Keep the existing snake_case/wild-west approach — rejected because the backend's `QueryingParameters` is the single source of truth, and inconsistent naming makes maintenance harder
- **ALT-002**: Remove `QueryBuilder` entirely — rejected because it's a useful abstraction for building filter DSL strings; it just needs to emit the correct parameter names
- **ALT-003**: Use case-insensitive model binding forever — rejected because it masks mismatches and won't work for features where the backend subclasses add extra properties with specific names

## 4. Dependencies

- **DEP-001**: Phases 1–5 of `plan/refactor-admin-api-layer-1.md` must be complete (they are — response envelope, model alignment, service consolidation all done)
- **DEP-002**: The backend `QueryingParameters` record at `Shared/Operational/Persistence/Specifications/Querying/Querying.Parameters.cs` is the canonical reference
- **DEP-003**: The Axios `paramsSerializer: { indexes: null }` configuration must remain as-is (already correct for array serialization)

## 5. Files

### Shared types (new)
- **FILE-001**: `src/shared/api/types/query-params.types.ts` — new file with all shared query param interfaces
- **FILE-002**: `src/shared/api/types/index.ts` — re-export the new types

### Shared utilities (modified)
- **FILE-003**: `src/shared/utils/query-builder.utils.ts` — update `build()` return type and output mapping
- **FILE-004**: `src/shared/utils/query-builder.utils.spec.ts` — update test assertions
- **FILE-005**: `src/shared/api/services/crud.service.ts` — update `list()` signature

### Feature types (all 10, modified)
- **FILE-006**: `src/features/catalog/products/types/product.types.ts`
- **FILE-007**: `src/features/catalog/taxonomies/types/taxonomy.types.ts`
- **FILE-008**: `src/features/catalog/option-types/types/option-type.types.ts`
- **FILE-009**: `src/features/catalog/option-types/option-values/types/option-value.types.ts`
- **FILE-010**: `src/features/catalog/property-types/types/property-type.types.ts`
- **FILE-011**: `src/features/catalog/taxonomies/taxa/types/taxon.types.ts`
- **FILE-012**: `src/features/ordering/types/order.types.ts`
- **FILE-013**: `src/features/users/types/user.types.ts`
- **FILE-014**: `src/features/inventories/types/inventory.types.ts`
- **FILE-015**: `src/features/reports/types/report.types.ts`

### Store files (12+, modified)
- **FILE-016 to FILE-027**: All `.store.ts` files in `src/features/*/stores/` and `src/features/*/subfeatures/stores/`

### Template files (.vue)
- **FILE-028 to FILE-035**: Various `.vue` files with inline query param construction

## 6. Testing

- **TEST-001**: `pnpm type-check` — zero actionable errors
- **TEST-002**: `pnpm build-only` — passes
- **TEST-003**: `pnpm test:unit` — all 107 tests pass
- **TEST-004**: `QueryBuilder` unit tests pass with new output format (`pageSize` not `page_size`, `sort` as `string[]` not comma-joined string)
- **TEST-005**: Feature store specs pass with new standardized types

## 7. Risks & Assumptions

- **RISK-001**: Some backend endpoints may subclass `QueryingParameters` with additional custom query params (e.g., `GetProductsPaged.Parameters` adds `Status`, `TaxonId`, `Season`). The `QueryParams` type defines only the 7 canonical params; feature-level types should extend it with the extras. Check each endpoint's `Parameters` subclass before finalizing the feature type.
- **RISK-002**: If any backend controller uses `[FromQuery]` with individual snake_case parameter names instead of `[AsParameters] QueryingParameters`, those endpoints would stop working if we stop sending the old names. However, the analysis found **zero** endpoints using individual `[FromQuery]` params — all 23+ paged endpoints use `[AsParameters]` with `QueryingParameters`. Low risk.
- **ASSUMPTION-001**: The existing `page_size` → `pageSize` rename won't break anything because ASP.NET Core model binding is case-insensitive for `[AsParameters]` properties (no `Name` override defaults to the property name).
- **ASSUMPTION-002**: The `sort` array (DSL format) will be used going forward. Old `sort_by`/`is_descending` params will be kept for backwards compatibility as optional fallbacks in the feature types.

## 8. Related Specifications / Further Reading

- `plan/refactor-admin-api-layer-1.md` — prior phase (response envelope, model aligment, service consolidation)
- `service/Api/src/Shared/Operational/Persistence/Specifications/Querying/Querying.Parameters.cs` — canonical backend record
- `service/Api/src/Shared/Operational/Persistence/Specifications/Sorting/Sort.Model.cs` — sort DSL documentation
- `service/Api/src/Shared/Operational/Persistence/Specifications/Filtering/` — filter DSL documentation
