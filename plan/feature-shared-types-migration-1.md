---
goal: Port Backend Shared Domain Types into Frontend Shared Layer
version: 1.0
date_created: 2026-07-20
last_updated: 2026-07-20
owner: Admin SPA Team
status: Completed
tags:
  - feature
  - migration
  - shared-types
  - admin-spa
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Port backend `Shared` domain types from `service/Api/src/Shared/` into the Vue Admin SPA's `src/shared/` directory. Types are adapted to idiomatic TypeScript — string unions replace enums, files are consolidated (no single-class files), C#-only concepts (Descriptors, partial classes) are dropped. The result is a practical, frontend-friendly shared layer aligned with backend API contracts.

## 1. Requirements & Constraints

- **REQ-001**: Types must align with backend API contract shapes (field names, nullability) but use idiomatic TypeScript — string unions instead of enums where practical, consolidated files instead of one-class-per-file.
- **REQ-002**: No runtime parsing logic for `Specifications/` types; filter/search/sort/paging are type contracts + serialization helpers (`QueryStringBuilder`).
- **REQ-003**: `shared/` must remain a forward-only dependency — it imports from no other `src/` directories; only external packages and sibling `shared/` subdirectories.
- **REQ-004**: Exported all types via barrel (`index.ts`) files at each directory level.
- **REQ-005**: Align `ErrorType` enum with backend HTTP status code convention (replace current numeric 0–4 with 400-series integer constants matching `ErrorType.BadRequest` etc.).
- **CON-001**: Must not break the existing `api.client.ts`, `result.types.ts`, or auth flow — any type changes must be backward-compatible or coordinated with consumer updates.
- **CON-002**: Keep the `ServerResult<T>` / `ServerPagedResult<T>` naming convention already established in the frontend (not rename to `Result<T>`).
- **GUD-001**: Follow the existing code conventions from neighboring `shared/api/` files (no default exports, explicit `type` imports, JSDoc for public types).
- **PAT-001**: Each backend subdirectory maps to an equivalent frontend subdirectory under `src/shared/` (e.g., `Application/Models/Errors/` → `src/shared/models/errors/`).

## 2. Implementation Steps (Completed) & Post-Implementation Refactoring

All 7 phases executed. After initial 1:1 C#-to-TypeScript port, the models were refactored for frontend ergonomics:

| Change | Before (1:1 mirror) | After (frontend-friendly) |
|--------|---------------------|--------------------------|
| Drop `descriptors/` | `Descriptor`, `OptionDescriptor<T>` — C# attribute/reflection concepts | Removed — no frontend use case |
| Consolidate files | 8 subdirectories, 22 files | 6 flat files under `models/` |
| `FilterOperator` | 16-member `enum` + `FilterOperatorMap` with DSL token lookup | `type FilterOperator = '=' \| '!=' \| '*' \| ...` — string union, operator IS the DSL token |
| `FilterLogic` | `enum FilterLogic { And, Or }` | `type FilterLogic = 'and' \| 'or'` |
| `SortDirection` | `enum SortDirection { Ascending, Descending }` | `type SortDirection = 'asc' \| 'desc'` |
| `SortNulls` | `enum SortNulls { First, Last }` | `type SortNulls = 'first' \| 'last'` |
| `SearchMode` | `enum SearchMode { Any, All }` | `type SearchMode = 'any' \| 'all'` |
| Sentinels | `PageModel.Empty`, `FilterModel.Empty`, etc. (C# static class pattern) | `emptyPageModel`, `emptyFilterModel`, etc. (plain `const` objects) |
| Parameters naming | `INamedParameters`, `ISeoParameters` — C# interface prefix convention | `NamedParams`, `SeoParams` — idiomatic TypeScript |

Result: `src/shared/models/` is 6 files (`errors.ts`, `pagination.ts`, `filtering.ts`, `sorting.ts`, `searching.ts`, `parameters.ts`, `responses.ts`) served by a single barrel `index.ts`.

<details>
<summary>Original task table (all completed)</summary>

### Implementation Phase 1 — Result & Error Type Alignment

- GOAL-001: Align existing `result.types.ts` and error types with the backend `Error`, `Result`, `PagedResult` contracts. Add factory functions and operator-like helpers.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `src/shared/models/errors/ErrorType.ts` — export `ErrorType` constants matching backend `ErrorType` class: `BadRequest=400`, `Unauthorized=401`, `Forbidden=403`, `NotFound=404`, `Conflict=409`, `Validation=422`, `Unexpected=500`. Remove old `ErrorType` enum from `result.types.ts`. | | |
| TASK-002 | Create `src/shared/models/errors/Error.ts` — export `Error` interface matching backend `Error` struct: `code: string`, `message: string`, `type: number` (HTTP status code), `metadata: Record<string, unknown> \| null`. Add `ErrorConstant` namespace with constraints (`MaxCodeLength=256`, `MaxMessageLength=2048`). | | |
| TASK-003 | Update `result.types.ts` — import `Error` from `models/errors/`, replace existing `ServerError` with backend-aligned `Error` type. Ensure backward-compatible aliasing (`export type ServerError = Error`). Drop old `ErrorType` enum. | | |
| TASK-004 | Add `SharedResultFactories` to `result.types.ts` — export `createServerResult<T>(statusCode, value)` for success and `createServerErrorResult<T>(statusCode, errors)` for failure. Add `ServerResultConstants` namespace: `Ok=200`, `Created=201`, `Accepted=202`, `NoContent=204`. | | |
| TASK-005 | Create `src/shared/models/errors/index.ts`, `src/shared/models/index.ts`, `src/shared/index.ts` barrel exports. | | |

### Implementation Phase 2 — Descriptors & Parameters

- GOAL-002: Port `Descriptors/` (named metadata descriptions for operators/fields/enums) and `Parameters/` (trait interfaces for domain entity shapes).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Create `src/shared/models/descriptors/Descriptor.ts` — export `IDescriptor` interface (`name: string`, `description?: string`, `example?: unknown`) and `Descriptor` type implementing it. Export `OptionDescriptor<T>` interface extending `IDescriptor` with `value: T`. | | |
| TASK-007 | Create `src/shared/models/descriptors/DescriptorExtensions.ts` — `formatDescriptor(d)` returns `"name: description"`, `withName(d, n)`, `withDescription(d, d)`, `withExample(d, e)` — all returning new `Descriptor`. | | |
| TASK-008 | Create `src/shared/models/parameters/NamedParameters.ts` — export `INamedParameters` interface (`name: string`, `presentation?: string`). | | |
| TASK-009 | Create `src/shared/models/parameters/SeoParameters.ts` — export `ISeoParameters` interface (`metaTitle?: string`, `metaDescription?: string`, `metaKeywords?: string`). | | |
| TASK-010 | Create `src/shared/models/parameters/ActivatableParameters.ts` — export `IActivatableParameters` interface (`isActive: boolean`). | | |
| TASK-011 | Create `src/shared/models/parameters/SortableParameters.ts` — export `ISortableParameters` interface (`position: number`). | | |
| TASK-012 | Create barrel files: `descriptors/index.ts`, `parameters/index.ts`. Re-export from `models/index.ts`. | | |

### Implementation Phase 3 — Response Base Types

- GOAL-003: Port `Response` and `AuditableResponse` base types so all feature modules have a consistent response contract.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | Create `src/shared/models/responses/Response.ts` — export `IResponse` interface (`id: string` — Guid as string on frontend). Export `Response` interface extending `IResponse`. | | |
| TASK-014 | Create `src/shared/models/responses/AuditableResponse.ts` — export `AuditableResponse` interface extending `Response` with `createdAtUtc: string` (ISO 8601), `modifiedAtUtc?: string`, `createdBy?: string`, `modifiedBy?: string`. | | |
| TASK-015 | Create barrel file `responses/index.ts`. Re-export from `models/index.ts`. | | |

### Implementation Phase 4 — Specifications: Paging

- GOAL-004: Port `Paging/` types — the simplest specification, immediately needed for list endpoints in the feature migrations.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-016 | Create `src/shared/models/paging/PageBounds.ts` — export `PageBounds` interface (`defaultPage: number` (≥1, default 1), `defaultPageSize: number` (≥1, default 10), `maxPageSize: number` (≥1, default 100)). Export `PageBounds.Default` sentinel. | | |
| TASK-017 | Create `src/shared/models/paging/PageModel.ts` — export `PageModel` interface (`page: number` (≥1), `pageSize: number` (1..maxPageSize), `bounds: PageBounds`). Add `PageModelHelpers` namespace with `skip(page, pageSize)` and `totalPages(totalCount, pageSize)`. Export `PageModel.Empty` sentinel. | | |
| TASK-018 | Update `ServerPagedResult<T>` in `result.types.ts` to use backend-aligned field names: keep existing field names (they already match — backend serializes `PageNumber` as `page`). Add computed `totalPages` field. | | |
| TASK-019 | Create barrel `paging/index.ts`. Re-export from `models/index.ts`. | | |

### Implementation Phase 5 — Specifications: Filtering & Sorting & Searching (Type-Only)

- GOAL-005: Port `Filtering/`, `Sorting/`, `Searching/` models as TypeScript interfaces/constants. No runtime parsers yet — type contracts only.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | Create `src/shared/models/filtering/FilterLogic.ts` — export `FilterLogic` enum (`And`, `Or`). | | |
| TASK-021 | Create `src/shared/models/filtering/FilterOperator.ts` — export `FilterOperator` enum (16 values: `Equal`, `EqualCaseSensitive`, `NotEqual`, `GreaterThan`, `GreaterThanOrEqual`, `LessThan`, `LessThanOrEqual`, `Contains`, `ContainsCaseSensitive`, `NotContains`, `StartsWith`, `StartsWithCaseSensitive`, `NotStartsWith`, `EndsWith`, `EndsWithCaseSensitive`, `NotEndsWith`). Export `FilterOperatorMap` with `toDslToken(op)` and `caseSensitive`/`negation`/`stringOnly` lookup tables. | | |
| TASK-022 | Create `src/shared/models/filtering/FilterCondition.ts` — export `FilterCondition` interface (`field: string`, `operator: FilterOperator`, `value: string`). | | |
| TASK-023 | Create `src/shared/models/filtering/FilterGroup.ts` — export `FilterGroup` interface (`logic: FilterLogic`, `conditions: FilterCondition[]`, `groups: FilterGroup[]`). Export `FilterGroup.Empty` sentinel. | | |
| TASK-024 | Create `src/shared/models/filtering/FilterModel.ts` — export `FilterModel` interface (`root: FilterGroup`, `conditions: FilterCondition[]` (flattened), `allowedFields?: string[]`, `rawInput?: string`, `isValid: boolean`, `violations: string[]`, `isEmpty: boolean`). Export `FilterModel.Empty` sentinel. | | |
| TASK-025 | Create `src/shared/models/sorting/SortDirection.ts` — export `SortDirection` enum (`Ascending`, `Descending`). | | |
| TASK-026 | Create `src/shared/models/sorting/SortNulls.ts` — export `SortNulls` enum (`First`, `Last`). | | |
| TASK-027 | Create `src/shared/models/sorting/SortModel.ts` — export `SortClause` interface (`field: string`, `direction: SortDirection`, `nulls?: SortNulls`) and `SortModel` interface (`clauses: SortClause[]`, `allowedFields?: string[]`, `isValid: boolean`, `violations: string[]`, `isEmpty: boolean`). Export `SortModel.Empty` sentinel. | | |
| TASK-028 | Create `src/shared/models/searching/SearchMode.ts` — export `SearchMode` enum (`Any`, `All`). | | |
| TASK-029 | Create `src/shared/models/searching/SearchModel.ts` — export `SearchTerm` interface (`value: string`, `caseSensitive: boolean`) and `SearchModel` interface (`term: SearchTerm`, `fields: string[]`, `mode: SearchMode`, `allowedFields?: string[]`, `isValid: boolean`, `violations: string[]`, `isEmpty: boolean`). Export `SearchModel.Empty` sentinel. | | |
| TASK-030 | Create barrel files: `filtering/index.ts`, `sorting/index.ts`, `searching/index.ts`. Re-export from `models/index.ts`. | | |

### Implementation Phase 6 — Build Query Parameter Helpers

- GOAL-006: Create helper functions to serialize `FilterModel`/`SearchModel`/`SortModel`/`PageModel` into query parameters consumable by the backend's `QueryingParameters` endpoint binding.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-031 | Create `src/shared/api/query/QueryStringBuilder.ts` — export `buildFilterParam(model: FilterModel): string` (serializes to backend DSL string), `buildSearchParams(model: SearchModel): Record<string, string>` (search/searchFields/searchMode), `buildSortParams(model: SortModel): Record<string, string>` (sort=field:dir,field:dir), `buildPageParams(model: PageModel): Record<string, string>` (page/pageSize). | | |
| TASK-032 | Create barrel `query/index.ts`. Re-export from `shared/api/index.ts`. | | |

### Implementation Phase 7 — Wiring & Cleanup

- GOAL-007: Connect new types into existing consumers, remove deprecated types, add top-level barrel exports.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-033 | Create `src/shared/index.ts` — re-export all public types from `models/`, `api/`, `mapper/`. | | |
| TASK-034 | Update `api.client.ts` — import `Error` from new path, update response interceptor to construct errors using new `Error` shape. | | |
| TASK-035 | Update `error-handler.ts` — import `Error` from new path, return typed `Error` objects instead of raw record literals. | | |
| TASK-036 | Update `auth.store.ts` and `auth.api.ts` — verify types still compile, use new `Error` type in error handling paths. | | |
| TASK-037 | Verify `vue-tsc --noEmit` and `vite build` pass with no errors. | ✅ | 2026-07-20 |

</details>

## 3. Alternatives

- **ALT-001**: Auto-generate TypeScript types from C# models (e.g., via Typewriter or Swagger/OpenAPI codegen). Rejected for this phase — manual porting gives explicit control over naming conventions and avoids coupling to codegen tooling. Can revisit when API has stable Swagger output.
- **ALT-002**: Skip `Specifications/` types entirely and rely on raw `Record<string, string>` query params. Rejected — typed query-building catches field name typos at compile time and reduces backend validation failures.
- **ALT-003**: Use the existing `ServerResult<T>` naming but rename internally to `Result<T>`. Rejected — the `Server` prefix clarifies that this is a wire-format type, distinct from domain-layer results.

## 4. Dependencies

- **DEP-001**: Backend API must remain stable — types are ported from commit snapshot of `service/Api/src/Shared/` as of 2026-07-20.
- **DEP-002**: Existing `axios` (^1.18.1) — already installed; no additional packages needed.
- **DEP-003**: Existing `pinia` (^3.0.4) — `auth.store.ts` is a consumer that must stay functional through the type migration.

## 5. Files

- **FILE-001**: `src/shared/api/types/result.types.ts` — MODIFIED: replace `ServerError`/`ErrorType`, add factory functions
- **FILE-002**: `src/shared/models/errors/ErrorType.ts` — NEW
- **FILE-003**: `src/shared/models/errors/Error.ts` — NEW
- **FILE-004**: `src/shared/models/errors/index.ts` — NEW
- **FILE-005**: `src/shared/models/descriptors/Descriptor.ts` — NEW
- **FILE-006**: `src/shared/models/descriptors/DescriptorExtensions.ts` — NEW
- **FILE-007**: `src/shared/models/descriptors/index.ts` — NEW
- **FILE-008**: `src/shared/models/parameters/NamedParameters.ts` — NEW
- **FILE-009**: `src/shared/models/parameters/SeoParameters.ts` — NEW
- **FILE-010**: `src/shared/models/parameters/ActivatableParameters.ts` — NEW
- **FILE-011**: `src/shared/models/parameters/SortableParameters.ts` — NEW
- **FILE-012**: `src/shared/models/parameters/index.ts` — NEW
- **FILE-013**: `src/shared/models/responses/Response.ts` — NEW
- **FILE-014**: `src/shared/models/responses/AuditableResponse.ts` — NEW
- **FILE-015**: `src/shared/models/responses/index.ts` — NEW
- **FILE-016**: `src/shared/models/paging/PageBounds.ts` — NEW
- **FILE-017**: `src/shared/models/paging/PageModel.ts` — NEW
- **FILE-018**: `src/shared/models/paging/index.ts` — NEW
- **FILE-019**: `src/shared/models/filtering/FilterLogic.ts` — NEW
- **FILE-020**: `src/shared/models/filtering/FilterOperator.ts` — NEW
- **FILE-021**: `src/shared/models/filtering/FilterCondition.ts` — NEW
- **FILE-022**: `src/shared/models/filtering/FilterGroup.ts` — NEW
- **FILE-023**: `src/shared/models/filtering/FilterModel.ts` — NEW
- **FILE-024**: `src/shared/models/filtering/index.ts` — NEW
- **FILE-025**: `src/shared/models/sorting/SortDirection.ts` — NEW
- **FILE-026**: `src/shared/models/sorting/SortNulls.ts` — NEW
- **FILE-027**: `src/shared/models/sorting/SortModel.ts` — NEW
- **FILE-028**: `src/shared/models/sorting/index.ts` — NEW
- **FILE-029**: `src/shared/models/searching/SearchMode.ts` — NEW
- **FILE-030**: `src/shared/models/searching/SearchModel.ts` — NEW
- **FILE-031**: `src/shared/models/searching/index.ts` — NEW
- **FILE-032**: `src/shared/models/index.ts` — NEW
- **FILE-033**: `src/shared/api/query/QueryStringBuilder.ts` — NEW
- **FILE-034**: `src/shared/api/query/index.ts` — NEW
- **FILE-035**: `src/shared/api/index.ts` — NEW
- **FILE-036**: `src/shared/index.ts` — NEW
- **FILE-037**: `src/shared/api/http/handlers/error-handler.ts` — MODIFIED: use new Error type
- **FILE-038**: `src/shared/api/http/api.client.ts` — MODIFIED: use new Error type
- **FILE-039**: `src/features/auth/stores/auth.store.ts` — INSPECT: verify compilation
- **FILE-040**: `src/features/auth/api/auth.api.ts` — INSPECT: verify compilation

## 6. Testing

- **TEST-001**: TypeScript compilation — `vue-tsc --build --noEmit` must pass with zero errors after each phase.
- **TEST-002**: Production build — `vite build` must succeed with zero warnings.
- **TEST-003**: Auth flow smoke test — existing login API call must still return correctly typed `ServerResult<AuthSession>`.
- **TEST-004**: `QueryStringBuilder` unit tests — `buildFilterParam`, `buildSearchParams`, `buildSortParams`, `buildPageParams` produce expected query strings matching backend `QueryingParameters` expectations.
- **TEST-005**: Barrel export test — `import { Error, Result, PageModel, FilterModel, SortModel, SearchModel } from '@/shared'` must resolve all symbols.

## 7. Risks & Assumptions

- **RISK-001**: Changing `ServerError` to `Error` may break import paths in files outside `src/shared/` that directly reference `ServerError`. Mitigation: add a type alias (`export type ServerError = Error`) in `result.types.ts` during transition; search for direct imports before removing.
- **RISK-002**: The `Error.type` field changes from arbitrary integer to HTTP status code. The current `parseApiError` function already returns HTTP status codes in `statusCode` — risk is low but any code comparing `error.type === 0` (the old `Validation` value) must be updated to `error.type === 422`.
- **RISK-003**: `Specifications/` types are large and may bloat build if eagerly imported. Mitigation: type-only files with no runtime cost; tree-shaking handles unused exports.
- **ASSUMPTION-001**: Backend API response shapes won't change during this migration. The plan is based on the backend models as of 2026-07-20.
- **ASSUMPTION-002**: The `ServerPagedResult` field naming (`page`/`pageSize`/`totalCount`) already matches what the backend serializes — no wire-format changes needed.
- **ASSUMPTION-003**: No existing code depends on the old numeric `ErrorType` enum values (0–4). Audit confirms the enum is defined but never referenced in any consuming code.

## 8. Related Specifications / Further Reading

- [Backend Shared source — `service/Api/src/Shared/`](/home/qingfa/Repos/ReSys.Shop/service/Api/src/Shared/)
- [Backend Application Models — `service/Api/src/Shared/Application/Models/`](/home/qingfa/Repos/ReSys.Shop/service/Api/src/Shared/Application/Models/)
- [Backend Specifications — `service/Api/src/Shared/Operational/Persistence/Specifications/`](/home/qingfa/Repos/ReSys.Shop/service/Api/src/Shared/Operational/Persistence/Specifications/)
- [Frontend Shared — `app/Admin/src/shared/`](/home/qingfa/Repos/ReSys.Shop/app/Admin/src/shared/)
- [Legacy Admin shared layer — `app/lagacy/Admin/src/shared/`](/home/qingfa/Repos/ReSys.Shop/app/lagacy/Admin/src/shared/)
- [Architecture docs — `docs/codebase/ARCHITECTURE.md`](/home/qingfa/Repos/ReSys.Shop/docs/codebase/ARCHITECTURE.md)
- [Conventions — `docs/codebase/CONVENTIONS.md`](/home/qingfa/Repos/ReSys.Shop/docs/codebase/CONVENTIONS.md)
- [Harness domains — `.harness/domains.yml`](/home/qingfa/Repos/ReSys.Shop/.harness/domains.yml)
