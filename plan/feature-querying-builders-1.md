---
goal: Add fluent query builders for filter/search/sort/page in the Admin frontend
version: 2.0
date_created: 2026-07-06
last_updated: 2026-07-06
owner: Platform Team
status: Planned
tags: feature, frontend, admin, query-builders, typescript
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Add `FilterBuilder`, `SortBuilder`, and `QueryBuilder` TypeScript classes to `app/Admin/src/shared/query/` that provide a fluent API for constructing backend-compatible query parameters (filter DSL, sort clauses, search, paging). These builders generate URL query params matching the backend's `QueryingParameters` binding model (`filter`, `search`, `searchFields`, `searchMode`, `sort`, `pageNumber`, `pageSize`), replacing the current ad-hoc `URLSearchParams` construction in every list composable.

## 1. Requirements & Constraints

- **REQ-001**: Builders must produce URL query parameter objects compatible with the backend `QueryingParameters` record (`.NET Minimal API [AsParameters]` binding)
- **REQ-002**: Backend parameter names: `filter` (DSL string), `search` (string), `searchFields` (string[]), `searchMode` (string: `"any"`|`"all"`), `sort` (string[]), `pageNumber` (int), `pageSize` (int)
- **REQ-003**: Filter DSL format matches `FilterDslParser` grammar: `"field:operator:value"` with supported operators `eq`, `neq`, `contains`, `starts`, `ends`, `gt`, `gte`, `lt`, `lte`, `isnull`, `isnotnull`
- **REQ-004**: Sort format matches `SortDslParser` grammar: `"field:asc"` or `"field:desc"`
- **REQ-005**: `FilterBuilder` must support fluent chaining: `.where('field').eq(value).and('field2').contains(value).or('field3').gt(5)`
- **REQ-006**: `SortBuilder` must support: `.orderBy('field').thenByDesc('field2')`
- **REQ-007**: `QueryBuilder` must combine filter + sort + search + page in one chain ending with `.build()` returning `{ params: Record<string, string | string[] | number>, url: string }`
- **REQ-008**: Builders must use string field names (TypeScript has no expression trees), but provide type-safe generic constraints via `QueryBuilder<T extends Record<string, unknown>>` for IDE autocomplete on field names when using literal union types
- **REQ-009**: A `useList` composable must wrap the builder + API call pattern so feature modules get a single `useList(builder, fetchFn)` hook
- **CON-001**: All builders go in `app/Admin/src/shared/query/` — Store app is out of scope
- **CON-002**: No changes to existing `PagedResult`, `PageRequest` types or the Axios client
- **PAT-001**: Follow existing `shared/` file conventions — barrel `index.ts` export
- **GUD-001**: `TreatWarningsAsErrors=true` — no lint or type-check warnings
- **GUD-002**: Existing `PageRequest` and `Sort` types in `shared/types/` remain unchanged

## 2. Implementation Steps

### Implementation Phase 1 — Shared type definitions

- GOAL-001: Define TypeScript types for the backend's query parameter model

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `app/Admin/src/shared/query/` directory | | |
| TASK-002 | Create `app/Admin/src/shared/query/types.ts` with: `FilterOperator` union type (`'eq' \| 'neq' \| 'contains' \| 'starts' \| 'ends' \| 'gt' \| 'gte' \| 'lt' \| 'lte' \| 'isnull' \| 'isnotnull'`), `FilterClause` interface (`{ field: string, operator: FilterOperator, value?: string }`), `SortClause` interface (`{ field: string, direction: 'asc' \| 'desc' }`), `QueryParams` interface (`{ filter?: string, search?: string, searchFields?: string[], searchMode?: 'any' \| 'all', sort?: string[], pageNumber?: number, pageSize?: number }`), `QueryBuilderResult` type (`{ params: Record<string, string \| string[] \| number>, url: string }`) | | |
| TASK-003 | Create `app/Admin/src/shared/query/index.ts` barrel export | | |

### Implementation Phase 2 — FilterBuilder

- GOAL-002: Create `FilterBuilder` with fluent conditional chaining

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Create `app/Admin/src/shared/query/filter-builder.ts` with `FilterBuilder` class: internal `clauses: FilterClause[]` array; `.where(field: string)` returning a `FilterOperatorBuilder` intermediate object exposing operator methods (`.eq(v)`, `.neq(v)`, `.contains(v)`, `.starts(v)`, `.ends(v)`, `.gt(v)`, `.gte(v)`, `.lt(v)`, `.lte(v)`, `.isnull()`, `.isnotnull()`) each returning `FilterBuilder` for chaining; `.and()` / `.or()` separator methods that set the logic for the next clause (stored as prefix `"and:"` / `"or:"` in DSL, default `"and"`); `.build()` returning `QueryParams` with `filter` as a DSL string like `"field1:eq:val,and,field2:contains:val2"` | | |
| TASK-005 | `.toDsl()` internal method that serializes accumulated clauses to the backend DSL string format: each clause becomes `"field:operator:value"`, separator logic is encoded per the `FilterDslParser` grammar (comma for AND, `"or,"` prefix for OR) | | |

### Implementation Phase 3 — SortBuilder

- GOAL-003: Create `SortBuilder` with fluent sort clause chaining

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Create `app/Admin/src/shared/query/sort-builder.ts` with `SortBuilder` class: internal `clauses: SortClause[]` array; `.orderBy(field: string)` and `.orderByDesc(field: string)` as first/primary sort; `.thenBy(field: string)` and `.thenByDesc(field: string)` as secondary sorts; `.build()` returning `QueryParams` with `sort` as `string[]` like `["field1:asc", "field2:desc"]` | | |

### Implementation Phase 4 — QueryBuilder

- GOAL-004: Create `QueryBuilder` that combines filter, sort, search, and paging

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | Create `app/Admin/src/shared/query/query-builder.ts` with `QueryBuilder<TFields extends string = string>` generic class: internal `FilterBuilder`, `SortBuilder`, search term/fields/mode, page number/size; constructor optionally accepts a union of allowed field names for type safety (used as `TFields` generic); `.filterBy(action: (f: FilterBuilder) => void)` — creates internal `FilterBuilder`, calls action, stores result; `.sortBy(action: (s: SortBuilder) => void)`; `.search(term: string, fields?: string[], mode?: 'any' \| 'all')`; `.page(page: number, pageSize: number)`; `.build()` returning `QueryBuilderResult` with: `params` as a flat `Record<string, string | string[] | number>` (keys: `filter`, `search`, `searchFields`, `searchMode`, `sort`, `pageNumber`, `pageSize` — only includes non-empty values), and `url` as a helper: given a base path, returns the path with query string | | |
| TASK-008 | `.toUrl(basePath: string)` method that combines base path with serialized params via `URLSearchParams` | | |
| TASK-009 | Add overload `.build()` returning just `QueryParams` for callers who want the raw object | | |

### Implementation Phase 5 — useList composable

- GOAL-005: Create a `useList` composable that wraps QueryBuilder + API call into the standard reactive pattern used by all list views

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | Create `app/Admin/src/shared/composables/useList.ts` with `useList<TItem, TFields>(config: { baseUrl: string, builder: QueryBuilder<TFields>, fetch?: (url: string) => Promise<PagedResult<TItem>> })` — returns `{ data: Ref<TItem[]>, total: Ref<number>, isLoading: Ref<boolean>, error: Ref<Error \| null>, refetch: () => Promise<void> }`; internally calls `builder.toUrl(baseUrl) → api.getPaged<TItem>(url)`; supports reactive re-fetch when builder changes | | |
| TASK-011 | `useList` composable should accept `initial: { filter?, sort?, search?, page?, pageSize? }` optional initial values | | |

### Implementation Phase 6 — Integration into existing feature

- GOAL-006: Refactor one existing list composable to use QueryBuilder (proving the pattern works)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-012 | Update `app/Admin/src/features/identity/users/api/get-list.ts` — replace manual `URLSearchParams` construction with `QueryBuilder<PageRequest['search']>` (or similar); keep same public API signature `useUsersList(params: Ref<PageRequest>)` but internally use the builder; add `filter` and `sort` passthrough if present on the params | | |
| TASK-013 | Update `UserFilters.vue` to emit `status` as part of the query filter (using `FilterBuilder` via the parent) — proving the builder composition pattern | | |

### Implementation Phase 7 — Unit tests

- GOAL-007: Add comprehensive unit tests for all builders

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Create `app/Admin/src/shared/query/__tests__/filter-builder.spec.ts` — test `.where().eq()`, `.and().contains()`, `.or().gt()`, `.where().isnull()`, chaining 3+ conditions, `.build()` produces correct DSL string | | |
| TASK-015 | Create `app/Admin/src/shared/query/__tests__/sort-builder.spec.ts` — test single sort, multi-sort chain, asc/desc, empty builder produces no sort param | | |
| TASK-016 | Create `app/Admin/src/shared/query/__tests__/query-builder.spec.ts` — test full composition (filter + sort + search + page), partial composition (only page), `.toUrl()` produces correct query string | | |
| TASK-017 | Create `app/Admin/src/shared/composables/__tests__/useList.spec.ts` — test with mocked API call, verify data/loading/error refs | | |

### Implementation Phase 8 — Verification

- GOAL-008: Run lint, type-check, and unit tests

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-018 | Run `pnpm run type-check` — no TS errors | | |
| TASK-019 | Run `pnpm run lint` — no lint warnings | | |
| TASK-020 | Run `pnpm run test:unit` — all tests pass (existing + new) | | |

## 3. Alternatives

- **ALT-001 (Keep ad-hoc URLSearchParams)**: Current pattern works but forces every list composable to reimplement the same parameter-building logic, leading to inconsistencies. **Rejected** — builders centralize the parameter construction and ensure backend compatibility.
- **ALT-002 (Use a generic `query-string` library)**: Libraries like `qs` or `query-string` handle serialization but provide no type safety or fluent filter DSL construction. **Rejected** — the value is in the DSL-aware builder, not just serialization.
- **ALT-003 (Generate builders from OpenAPI spec)**: Could auto-generate query builders from the .NET API's OpenAPI spec. **Rejected** — over-engineering for the current scale; manual builders are sufficient.

## 4. Dependencies

- **DEP-001**: Backend's `FilterDslParser` / `SortDslParser` grammar — builders must emit strings that parse correctly on the server side
- **DEP-002**: Existing `api.getPaged<T>()` method — the builder's `.toUrl()` output feeds into this
- **DEP-003**: Existing `PagedResult<T>` type — used by `useList` composable

## 5. Files

| File | Action |
|------|--------|
| `app/Admin/src/shared/query/types.ts` | Create |
| `app/Admin/src/shared/query/filter-builder.ts` | Create |
| `app/Admin/src/shared/query/sort-builder.ts` | Create |
| `app/Admin/src/shared/query/query-builder.ts` | Create |
| `app/Admin/src/shared/query/index.ts` | Create |
| `app/Admin/src/shared/composables/useList.ts` | Create |
| `app/Admin/src/features/identity/users/api/get-list.ts` | Edit — refactor to use QueryBuilder |
| `app/Admin/src/features/identity/users/ui/UserFilters.vue` | Edit — wire `status` through FilterBuilder |
| `app/Admin/src/shared/query/__tests__/filter-builder.spec.ts` | Create |
| `app/Admin/src/shared/query/__tests__/sort-builder.spec.ts` | Create |
| `app/Admin/src/shared/query/__tests__/query-builder.spec.ts` | Create |
| `app/Admin/src/shared/composables/__tests__/useList.spec.ts` | Create |

## 6. Testing

- **TEST-001**: FilterBuilder produces correct DSL `"name:eq:test,and,price:gt:10"` for chained `.where('name').eq('test').and('price').gt(10)`
- **TEST-002**: FilterBuilder handles `.or()` producing `"name:eq:test,or,price:gt:10"` with `or,` prefix
- **TEST-003**: FilterBuilder produces no `filter` param when no clauses added
- **TEST-004**: SortBuilder produces `["name:asc"]` for `.orderBy('name')`
- **TEST-005**: SortBuilder produces `["name:asc","price:desc"]` for `.orderBy('name').thenByDesc('price')`
- **TEST-006**: SortBuilder produces no `sort` param when no clauses added
- **TEST-007**: QueryBuilder full composition produces all expected params in `build().params`
- **TEST-008**: QueryBuilder partial (only page) produces only `pageNumber` and `pageSize`
- **TEST-009**: QueryBuilder `.toUrl('/api/products')` produces `"/api/products?pageNumber=1&pageSize=20&filter=..."`
- **TEST-010**: `useList` composable makes API call on mount and returns reactive data/loading/error
- **TEST-011**: `useList` re-fetches when builder changes (watching)
- **TEST-012**: All existing tests continue to pass

## 7. Risks & Assumptions

- **RISK-001**: Filter DSL grammar may differ between frontend builder and backend parser — must verify DSL format matches `FilterDslParser` exactly. Mitigate with integration tests that round-trip through a test API call.
- **RISK-002**: The `useList` composable may not fit all list views — some have custom query logic (like the products handler with `Status`/`TaxonId`/`Season` filters). Mitigate by keeping `useList` optional — feature composables can still use raw `QueryBuilder`.
- **ASSUMPTION-001**: The backend `FilterDslParser` accepts comma-separated conditions with `"or,"` prefix for OR logic, and `"and,"` prefix (or no prefix) for AND logic.
- **ASSUMPTION-002**: The Store app is not in scope — builders are Admin-only.

## 8. Related Specifications / Further Reading

- Backend specification parsers: `service/Api/src/Shared/Operational/Persistence/Specifications/Filtering/Parsing/FilterDslParser.cs`
- Backend spec grammar reference: `service/Api/src/Shared/Operational/Persistence/Specifications/Sorting/Parsing/SortDslParser.cs`
- Existing `Sort` type: `app/Admin/src/shared/types/sort.ts`
- Existing `PageRequest` type: `app/Admin/src/shared/types/page.ts`
