---
goal: Eliminate duplicate API methods, dead response types, and the double-toast error bug in the Admin frontend
version: 1.0
date_created: 2026-07-07
status: 'Planned'
tags: refactor, api, duplication, services, error-handling
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The Admin API layer suffers from several structural issues discovered after the Phase 1–5 refactors:

0. **Frontend models not aligned to backend `Shared/Application/Models`**: `ServerResult<T>`, `ServerPagedResult<T>`, and `ServerError` were never audited against the canonical C# records. `PagedList<T>` is a dead type that doesn't correspond to any backend model. `ParsedApiError` conflates `status` and `statusCode` — the backend only has `StatusCode`. The backend `Error` record has a `Type` enum (`Validation`, `NotFound`, `Conflict`, `UnprocessableEntity`, `InternalServerError`) and optional `Metadata`, but the frontend uses a raw `number` for `type` and has `metadata?` as optional (not nullable). No `ErrorType` constants exist on the frontend.

1. **CRUD method duplication**: 13 of 19 feature services manually re-implement the same `list`/`getById`/`create`/`update`/`delete` pattern that `createCrudService()` already provides. 6 services that use the factory add redundant wrapper methods (`getList`, `getAll`) that just delegate to `this.list()`.

2. **`PagedList<T>` dead type**: This 2-field snake_case interface (`items`, `total_count`) is used as a type annotation on `ApiResult` generics (e.g., `ApiResult<PagedList<VariantSummary>>`), but the Axios interceptor unwraps paged responses into `ApiResult<T[]>` — so the `PagedList` envelope never matches runtime. `total_count` doesn't match `totalCount` either.

3. **Double-toast bug**: The Axios error interceptor in `api.client.ts` calls `showToast()` for every non-401 error (line 96). `useApiErrorHandler.handleFormErrors()` also calls `showToast()` for the same error. Every API error results in duplicate toasts.

The user also reports seeing `errors: undefined` in `ParsedApiError` objects, which should be an empty object `{}` for cleaner consumer code.

## 1. Requirements & Constraints

- **REQ-000**: Audit `ServerResult<T>`, `ServerPagedResult<T>`, and `ServerError` against backend `Shared/Application/Models` records; ensure field names, nullability, and types are exact mirrors
- **REQ-001**: Add `ErrorType` enum constants matching backend `Error.Type` (`Validation`, `NotFound`, `Conflict`, `UnprocessableEntity`, `InternalServerError`)
- **REQ-002**: Remove `status` field from `ParsedApiError` — backend only has `StatusCode`; consolidate to single source of truth
- **REQ-003**: Refactor all feature services to use `createCrudService()` factory and only add domain-specific methods
- **REQ-004**: Remove redundant wrapper methods (`getList`, `getAll`) — rename usages to `list` where safe
- **REQ-005**: Remove the `PagedList<T>` dead type from `result.types.ts` and replace its usages with the unwrapped array pattern `T[]`
- **REQ-006**: Fix the double-toast bug by removing the `showToast()` call from the Axios error interceptor in `api.client.ts` — error toasts should be the responsibility of the consumer composable
- **REQ-007**: Change `ParsedApiError.errors` type from `Record<string, string[]> | undefined` to `Record<string, string[]>` with a default of `{}` instead of `undefined`
- **CON-001**: Do NOT change any HTTP call behavior — only restructure method definitions and remove dead code
- **CON-002**: Every service that uses the factory must type its `list()` method with its specific `QueryParams` subclass (e.g., `ProductSearchParams`, `OrderSearchParams`)
- **PAT-001**: Follow the existing pattern established by `taxonomy.service.ts`, `option-type.service.ts`, `country.service.ts` — spread the factory then override `list()` with typed params

## 2. Implementation Steps

### Implementation Phase 0: Standardize frontend models against backend `Shared/Application/Models`

Align the frontend API model interfaces (`ServerResult`, `ServerPagedResult`, `ServerError`) with the canonical C# records in `service/Api/src/Shared/Application/Models/Results/` and `Errors/`. Remove or consolidate any fields that don't map to the backend contract.

**Backend model reference:**
| Backend record | Key fields | Serialized names (camelCase via System.Text.Json) |
|---|---|---|
| `Error` | `Code`, `Message`, `Type: ErrorType` enum, `Metadata` | `code`, `message`, `type`, `metadata` |
| `Result<T>` | `IsSuccess`, `StatusCode`, `Errors: List<Error>`, `Message`, `Metadata`, `Value` | `isSuccess`, `statusCode`, `errors`, `message`, `metadata`, `value` |
| `PagedResult<T>` | `IsSuccess`, `StatusCode`, `Errors`, `Message`, `Metadata`, `Items`, `Page`, `PageSize`, `TotalCount` | `isSuccess`, `statusCode`, `errors`, `message`, `metadata`, `items`, `page`, `pageSize`, `totalCount` |

**Backend `ErrorType` enum values:** `Validation = 0`, `NotFound = 1`, `Conflict = 2`, `UnprocessableEntity = 3`, `InternalServerError = 4`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-000 | Audit `ServerResult<T>` (result.types.ts:8-15) against backend `Result<T>` — all fields already match. No changes needed. | | |
| TASK-001 | Audit `ServerPagedResult<T>` (result.types.ts:17-27) against backend `PagedResult<T>` — all fields match. No changes needed. | | |
| TASK-002 | Audit `ServerError` (result.types.ts:1-6) against backend `Error` record — `metadata` is `optional` on frontend vs `nullable` on backend. Change `metadata?: Record<string, unknown> \| null` to `metadata: Record<string, unknown> \| null` (required) to match backend. | | |
| TASK-003 | Add `ErrorType` constants enum in `result.types.ts` matching backend `Error.Type` values: `ErrorType.Validation = 0`, `NotFound = 1`, `Conflict = 2`, `UnprocessableEntity = 3`, `InternalServerError = 4`. | | |
| TASK-004 | Audit `ParsedApiError` (api.utils.ts:3-11) — it has both `status` and `statusCode`. Backend only has `StatusCode`. Remove `status` field from interface and all construction sites; keep `statusCode` as the single source of truth. | | |
| TASK-005 | In `convertServerErrors` (api.utils.ts:14), change return type from `Record<string, string[]> \| undefined` to `Record<string, string[]>` with a default of `{}` — this is a prerequisite for TASK-009/010. | | |
| TASK-006 | Add `errorCode` field to `ParsedApiError` typed as `ErrorType \| undefined` instead of `string \| undefined`, and map backend `Error.Type` numbers to the enum in the parsing logic. | | |
| TASK-007 | Update all `ServerError` construction sites and test files to use the new `metadata: Record<string, unknown> \| null` shape (i.e., pass `null` explicitly instead of omitting). | | |

### Implementation Phase 1: Remove `PagedList<T>` dead type

- GOAL-001: Delete the unused `PagedList<T>` interface and fix all usages.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Read `src/shared/api/types/result.types.ts` — remove the `PagedList<T>` interface | | |
| TASK-009 | Read `src/features/catalog/products/services/product.service.ts` — replace `ApiResult<PagedList<ProductSummary>>` with `ApiResult<ProductSummary[]>` | | |
| TASK-010 | Read `src/features/catalog/products/variants/services/variant.service.ts` — replace `ApiResult<PagedList<VariantSummary>>` with `ApiResult<VariantSummary[]>` | | |
| TASK-011 | Read `src/features/users/services/user.service.ts` — replace `ApiResult<PagedList<UserSummary>>` with `ApiResult<UserSummary[]>` | | |
| TASK-012 | Read `src/features/users/services/role.service.ts` — replace `ApiResult<PagedList<RoleSummary>>` with `ApiResult<RoleSummary[]>` | | |
| TASK-013 | Read `src/features/users/services/permission.service.ts` — replace `ApiResult<PagedList<PermissionSummary>>` with `ApiResult<PermissionSummary[]>` | | |
| TASK-014 | Update `src/shared/api/types/index.ts` if it re-exports `PagedList` — remove the re-export | | |

### Implementation Phase 2: Fix `ParsedApiError.errors` default

- GOAL-002: Change `errors` default from `undefined` to `{}` for cleaner consumer code.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Read `src/shared/api/utils/api.utils.ts` — change `ParsedApiError.errors` type from `Record<string, string[]> \| undefined` to `Record<string, string[]>` | | |
| TASK-016 | In `api.utils.ts:parseApiError` — change ALL return sites where `errors: undefined` to `errors: {}` (lines 43, 91, 107, 119) | | |
| TASK-017 | In `convertServerErrors` function — change return type from `Record<string, string[]> \| undefined` to `Record<string, string[]>`, return `{}` instead of `undefined` for null/empty cases | | |
| TASK-018 | In `api.utils.ts:88,98` — change `errors: undefined` to `errors: {}` in the fallback and non-axios error paths | | |
| TASK-019 | Update `src/features/auth/stores/auth.store.ts` and any test files that construct `ParsedApiError` objects — ensure they pass `errors: {}` not `errors: undefined` | | |

### Implementation Phase 3: Fix double-toast bug

- GOAL-003: Remove the `showToast()` call from the Axios interceptor to stop duplicate error toasts.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | Read `src/shared/api/http/api.client.ts` — remove the `showToast()` call and the surrounding toast logic (lines 86-97, the entire `if (apiError.status !== 401)` block) — keep the `return Promise.resolve(...)` | | |

### Implementation Phase 4: Consolidate services onto CRUD factory

- GOAL-004: Refactor all 13 hand-written services to spread `createCrudService()` and only keep their custom methods.

#### Sub-phase 4a: Simple services (few or no extra methods)
| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-021 | Read `src/features/users/services/permission.service.ts` — refactor to use `createCrudService(PERMISSIONS)` spread, keep only `getPermissionSelect` | | |
| TASK-022 | Read `src/features/users/services/role.service.ts` — refactor to use `createCrudService(ROLES)` spread, keep only custom permission methods and `getUsersInRole` | | |
| TASK-023 | Read `src/features/users/services/user.service.ts` — refactor to use `createCrudService(USERS)` spread, keep only status/password/roles/permissions/customers methods | | |
| TASK-024 | Read `src/features/ordering/services/order.service.ts` — refactor to use `createCrudService(ORDERS)` spread, keep only shipments/items/addresses/state/cancel/refund methods | | |

#### Sub-phase 4b: Complex services (many extra methods, needs careful handling)
| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-025 | Read `src/features/catalog/products/services/product.service.ts` — refactor to use `createCrudService(CATALOG + '/products')` spread, keep option-types, properties, images sub-resource methods (14 total custom methods) | | |
| TASK-026 | Read `src/features/catalog/products/variants/services/variant.service.ts` — refactor to use `createCrudService(...)` spread, keep `listByProductId`, `setMaster`, `updateOptionValues` | | |
| TASK-027 | Read `src/features/catalog/taxonomies/taxa/services/taxon.service.ts` — refactor to use `createCrudService(...)` spread, keep taxon rules CRUD, `regenerateProducts`, `getProductPreview` | | |
| TASK-028 | Read `src/features/inventories/services/inventory.service.ts` — refactor to use `createCrudService(...)` for both stock-items and stock-locations sub-resources; keep units, movements, transfers | | |

#### Sub-phase 4c: Remove redundant wrapper methods
| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-029 | Read `src/features/catalog/taxonomies/services/taxonomy.service.ts` — remove the redundant `getList()` wrapper, rename all callers to use `list()` | | |
| TASK-030 | Read `src/features/catalog/option-types/services/option-type.service.ts` — remove the redundant `getList()` wrapper, rename all callers to use `list()` | | |
| TASK-031 | Read `src/features/catalog/property-types/services/property-type.service.ts` — remove the redundant `getList()` wrapper, rename all callers to use `list()` | | |
| TASK-032 | Read `src/features/catalog/option-types/option-values/services/option-value.service.ts` — keep `reorder()`, ensure factory spread is correct | | |
| TASK-033 | Read `src/features/location/services/country.service.ts` — remove the redundant `getAll()` wrapper, rename all callers to use `list()` | | |
| TASK-034 | Read `src/features/location/services/state.service.ts` — remove the redundant `getAll()` wrapper, rename all callers to use `list()` | | |

### Implementation Phase 5: Update store callers

- GOAL-005: Find all store files that call the old redundant method names (`getList`, `getAll`) and update them to use `list`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-035 | Search `src/features/**/*.store.ts` for `.getList(` — rename each occurrence to `.list(` | | |
| TASK-036 | Search `src/features/**/*.store.ts` for `.getAll(` — rename each occurrence to `.list(` | | |
| TASK-037 | Search `src/features/**/*.vue` for `.getList(` and `.getAll(` — rename occurrences in template script sections | | |

### Implementation Phase 6: Verify

- GOAL-006: Run full verification.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-038 | Run `pnpm type-check` — zero errors | | |
| TASK-039 | Run `pnpm build-only` — passes | | |
| TASK-040 | Run `pnpm test:unit` — all 107 tests pass | | |

## 3. Alternatives

- **ALT-001**: Keep the duplicate services as-is — rejected because 80+ lines of identical boilerplate make the codebase harder to maintain and more error-prone
- **ALT-002**: Keep `PagedList<T>` and add a special interceptor branch for it — rejected because the type is structurally incorrect (wrong field names) and adds confusion; the existing unwrapping pattern is correct
- **ALT-003**: Add a flag to the Axios interceptor to suppress toasts — rejected because the interceptor should not be concerned with UI; toast display is a consumer responsibility

## 4. Dependencies

- **DEP-001**: The `createCrudService` factory in `src/shared/api/services/crud.service.ts` already accepts `QueryParams` typed `list()` from Phase 3 of prior work
- **DEP-002**: All feature types already extend `QueryParams` from Phase 3 — service `list()` overrides can use the correct feature-specific type

## 5. Files

- **FILE-000**: `src/shared/api/types/result.types.ts` — add `ErrorType` enum, change `ServerError.metadata` to required
- **FILE-001**: `src/shared/api/utils/api.utils.ts` — remove `status` from `ParsedApiError`, add `ErrorType` mapping, change `errors` default to `{}`
- **FILE-002**: `src/shared/api/types/result.types.ts` — remove `PagedList`
- **FILE-003**: `src/shared/api/utils/api.utils.ts` — change `errors` default, remove `| undefined` from type
- **FILE-004**: `src/shared/api/http/api.client.ts` — remove `showToast()` from error interceptor
- **FILE-005 to FILE-017**: 13 feature service files — refactor to use factory
- **FILE-018 to FILE-023**: 6 store files — rename `getList`/`getAll` to `list`
- **FILE-024**: `src/shared/api/types/index.ts` — remove `PagedList` re-export
- **FILE-025 to FILE-N**: Test fixtures and `ServerError` construction sites — add `metadata: null`

## 6. Testing

- **TEST-000**: After Phase 0 — `vue-tsc` passes with no `ServerError.metadata` or `ParsedApiError.status` errors
- **TEST-001**: After Phase 6 — `pnpm type-check` passes with zero errors
- **TEST-002**: `pnpm build-only` — passes
- **TEST-003**: `pnpm test:unit` — all 107 tests pass

## 7. Risks & Assumptions

- **RISK-001**: Service refactoring changes method references — stores that call `serviceXxx.getList()` will break until renamed to `serviceXxx.list()`. Tasks TASK-035–037 must be executed in the same pass as tasks TASK-029–034.
- **RISK-003**: Standardizing `ServerError.metadata` from `optional` to `required (nullable)` will break any code that omits `metadata` when constructing `ServerError` objects or test fixtures. Run `vue-tsc` after TASK-007 to catch all affected sites.
- **RISK-002**: Removing the interceptor toast may cause some errors to go unnoticed if the consumer code doesn't handle errors properly. However, the interceptor already returns the error in `ApiResult` format, and store/service code checks `result.success` — any path that doesn't will now silently fail instead of showing a toast. Audit `handleFormErrors` usage in stores to ensure coverage.
- **ASSUMPTION-001**: All feature services that use `PagedList<T>` as `ApiResult<PagedList<T>>` should actually return `ApiResult<T[]>` — the interceptor unwraps to `T[]` already, so these type annotations are incorrect and happen to "work" because TypeScript structural typing doesn't catch the mismatch.
- **ASSUMPTION-002**: Removing the interceptor toast won't break any existing behavior because the `useApiErrorHandler` composable is used consistently across all stores that call the API.

## 8. Related Specifications / Further Reading

- `plan/refactor-admin-api-layer-1.md` — established the CRUD factory and service structure
- `plan/refactor-admin-query-params-3.md` — standardized query param types used by service list() methods
