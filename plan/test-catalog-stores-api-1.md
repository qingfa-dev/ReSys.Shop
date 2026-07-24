---
goal: Add unit tests for catalog stores, API classes, and getPagedList helper
version: 1.0
date_created: 2025-07-25
owner: Agent
status: Planned
tags: test, catalog, pinia, api-coverage
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Add unit tests for the 3 untested catalog Pinia stores (`product.store`, `taxonomy.store`, `option-type.store`), 2 untested catalog API classes (`TaxonApi`, `OptionValueApi`), and the `getPagedList` / `defaultListQuery` helpers in the shared layer. Follows the same patterns as existing auth store tests (`setActivePinia`, `createPinia`, API class mocking with `vi.mock`).

## 1. Requirements & Constraints

- **REQ-001**: Every store action (fetchMany, setPage, setSearch, setSort, setFilter, resetQuery) must be tested for the happy path
- **REQ-002**: Store error paths must be tested: API returns `isSuccess: false`, and network exceptions
- **REQ-003**: Store initial state must be verified (loading=false, error=null, items=[], totalRecords=0)
- **REQ-004**: `getPagedList` must be tested directly with `apiClient.get` mocked, verifying URL and serialized params match
- **REQ-005**: `defaultListQuery` must be tested for default values, custom pageSize, and immutability
- **REQ-006**: All API classes (TaxonApi, OptionValueApi) must have standalone test files with CRUD coverage
- **SEC-001**: No secrets or tokens in test code
- **CON-001**: Tests must use `setActivePinia(createPinia())` — NOT `createTestingPinia`
- **CON-002**: API classes must be mocked with `vi.mock` at module level, not `vi.spyOn`
- **CON-003**: Store tests must import API class from the barrel (`'../api'`) to verify barrel exports work
- **CON-004**: Store tests must use `vi.hoisted` for mock factory functions
- **GUD-001**: Follow existing auth store test pattern (auth.store.spec.ts:52-60 for setup)
- **PAT-001**: Three stores are structurally identical — test `product.store.ts` exhaustively, then verify `taxonomy.store` and `option-type.store` with a lighter smoke test (initial state + fetchMany success)
- **PAT-002**: `PagedResult` factory helper extracted to avoid repetition

## 2. Implementation Steps

### Implementation Phase 1: Shared-layer helpers

- GOAL-001: Add unit tests for `defaultListQuery` and `getPagedList`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Add `defaultListQuery` unit test to existing `query-serializer.spec.ts`: verify returns correct defaults, custom pageSize, and returns a new object each call | | |
| TASK-002 | Add `getPagedList` direct test file at `shared/api/utils/__tests__/getPagedList.spec.ts`: mock `apiClient.get`, verify URL, params, and response passthrough | | |

### Implementation Phase 2: Product store exhaustive tests

- GOAL-002: Exhaustive test for `useProductStore` — covers all actions, both success and error paths

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Create `features/catalog/store/__tests__/product.store.spec.ts` with `setActivePinia(createPinia())` setup, mock `ProductApi` at module level | | |
| TASK-004 | Test initial state: loading=false, error=null, items=[], totalRecords=0, query has defaults | | |
| TASK-005 | Test `fetchMany` success: items and totalRecords populated, loading toggled | | |
| TASK-006 | Test `fetchMany` failure: API returns `isSuccess: false` — error set, items cleared | | |
| TASK-007 | Test `fetchMany` network error: exception caught — error set to "Failed to load", items cleared | | |
| TASK-008 | Test `setPage`: query.page updated, fetchMany called, state reflects new page | | |
| TASK-009 | Test `setSearch`: query.search set with mode 'Any', page reset to 1, fetchMany called | | |
| TASK-010 | Test `setSort`: query.sort updated with single clause, fetchMany called | | |
| TASK-011 | Test `setFilter`: query.filters set, page reset to 1, fetchMany called | | |
| TASK-012 | Test `resetQuery`: query restored to defaults, fetchMany called | | |

### Implementation Phase 3: Taxonomy and OptionType store smoke tests

- GOAL-003: Verify taxonomy and option-type stores work identically to product store with lighter coverage

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | Create `features/catalog/store/__tests__/taxonomy.store.spec.ts`: test initial state + fetchMany success (smoke test) | | |
| TASK-014 | Create `features/catalog/store/__tests__/option-type.store.spec.ts`: test initial state + fetchMany success (smoke test) | | |

### Implementation Phase 4: Taxon and OptionValue API standalone tests

- GOAL-004: Add standalone contract tests for the 2 API classes currently covered only as secondary in other test files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Create `features/catalog/api/__tests__/taxons.spec.ts`: mock `apiClient`, test getMany/create/update/delete for `TaxonApi` | | |
| TASK-016 | Create `features/catalog/api/__tests__/optionValues.spec.ts`: mock `apiClient`, test getMany/create/update/delete for `OptionValueApi` | | |

## 3. Alternatives

- **ALT-001**: Use `createTestingPinia` — rejected per CON-001. `createTestingPinia` adds spy requirements and does not work with the existing test patterns.
- **ALT-002**: Parameterized store tests (single spec file testing all 3 stores with a loop) — rejected because vitest module mocking is per-file. Three spec files with distinct `vi.mock` calls avoids mock pollution.

## 4. Dependencies

- **DEP-001**: `vitest` and `pinia` already in `package.json` devDependencies
- **DEP-002**: `@/shared/api/client` mock already established in existing API test files
- **DEP-003**: `PagedResult<T>`, `Result<T>` types already defined in `shared/models/result.ts`

## 5. Files

- **FILE-001**: `app/Admin/src/shared/api/utils/__tests__/query-serializer.spec.ts` — add `defaultListQuery` tests
- **FILE-002**: `app/Admin/src/shared/api/utils/__tests__/getPagedList.spec.ts` — NEW: `getPagedList` direct tests
- **FILE-003**: `app/Admin/src/features/catalog/store/__tests__/product.store.spec.ts` — NEW: exhaustive store tests
- **FILE-004**: `app/Admin/src/features/catalog/store/__tests__/taxonomy.store.spec.ts` — NEW: smoke test
- **FILE-005**: `app/Admin/src/features/catalog/store/__tests__/option-type.store.spec.ts` — NEW: smoke test
- **FILE-006**: `app/Admin/src/features/catalog/api/__tests__/taxons.spec.ts` — NEW: standalone TaxinApi tests
- **FILE-007**: `app/Admin/src/features/catalog/api/__tests__/optionValues.spec.ts` — NEW: standalone OptionValueApi tests

## 6. Testing

- **TEST-001**: All test files must pass via `npx vitest run` — zero failures
- **TEST-002**: TypeScript must pass via `npx vue-tsc --noEmit` — zero errors
- **TEST-003**: Coverage must be verified locally via `npx vitest run --coverage` (coverage-v8 already installed)

## 7. Risks & Assumptions

- **RISK-001**: Store tests start with `setActivePinia(createPinia())` but may require `localStorage` or `navigate` mocking if stores expand — not needed for current catalog stores
- **RISK-002**: `getPagedList` test mocks `apiClient.get` via `vi.mock('@/shared/api/client')` — if the mock path alias resolution fails, use relative path `../../client` instead
- **ASSUMPTION-001**: The 3 catalog stores are structurally identical enough that smoke testing taxonomy and option-type after exhaustive product store tests provides adequate coverage
- **ASSUMPTION-002**: `vi.hoisted` can be used for mock factories as in auth store tests

## 8. Related Specifications / Further Reading

[FEATURE_PATTERN.md](../app/Admin/docs/FEATURE_PATTERN.md)
[2025-07-25-universal-query-converter-design.md](../app/Admin/docs/specs/2025-07-25-universal-query-converter-design.md)
