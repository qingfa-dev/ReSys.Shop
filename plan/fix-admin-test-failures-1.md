---
goal: Fix all 38 pre-existing test failures in the Admin SPA test suite
version: 1.0
date_created: 2026-07-17
status: 'Planned'
tags: test, bug, admin-spa
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

38 unit tests fail in `app/Admin` across 5 test files. All failures fall into 3 categories with distinct root causes: (1) repository mock mocks return `undefined` instead of a promise, (2) api.client interceptor tests expect response unwrapping that does not exist, (3) taxonomy store test mock uses `items` instead of `value`. This plan fixes all 38 failures with targeted changes to 5 test files and zero implementation changes.

## 1. Requirements & Constraints

- **REQ-001**: All 38 failing tests must pass after fixes
- **REQ-002**: Zero changes to implementation files (repository, service, api.client, store) — only test files may be modified
- **REQ-003**: Zero new test failures introduced
- **REQ-004**: Each fix must be isolated to its category — no cross-category changes in the same file
- **CON-001**: The api client interceptor is intentionally a pass-through (repositories unwrap `res.data` themselves) — tests must match this design
- **CON-002**: Repository methods chain `.then()` on `apiClient` calls — mock methods must return Promises
- **PAT-001**: Use `.mockResolvedValue(...)` for mock return values, not `.mockReturnValue(Promise.resolve(...))`

## 2. Implementation Steps

### Phase 1 — Fix mock setup in 3 repository spec files (34 failures)

- GOAL-001: Make all `vi.fn()` mock methods in the 3 repository test files return a Promise so `.then()` chains in repository implementations do not throw

**Root cause**: `vi.mock('@/shared/api/http/api.client', () => ({ default: { get: vi.fn(), ... } }))` — `vi.fn()` returns `undefined`, and `apiClient.get(url).then(...)` throws `Cannot read properties of undefined (reading 'then')`.

**Fix**: Change each `vi.fn()` to `vi.fn().mockResolvedValue({ data: {} })`. The `{ data: {} }` shape satisfies the `.then(res => res.data as ...)` pattern in every repository. Tests only assert the URL called, never the return value, so the mock payload is irrelevant.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | In `catalog/_tests/catalog.api.spec.ts` lines 11-18 (the `vi.mock` block): change `get: vi.fn(), post: vi.fn(), put: vi.fn(), patch: vi.fn(), delete: vi.fn()` to `get: vi.fn().mockResolvedValue({ data: {} }), post: vi.fn().mockResolvedValue({ data: {} }), put: vi.fn().mockResolvedValue({ data: {} }), patch: vi.fn().mockResolvedValue({ data: {} }), delete: vi.fn().mockResolvedValue({ data: {} })` | | |
| TASK-002 | In `inventories/_tests/inventory.api.spec.ts` lines 7-9 (the `vi.mock` block): change `get: vi.fn(), post: vi.fn(), put: vi.fn(), patch: vi.fn(), delete: vi.fn()` to `get: vi.fn().mockResolvedValue({ data: {} }), post: vi.fn().mockResolvedValue({ data: {} }), put: vi.fn().mockResolvedValue({ data: {} }), patch: vi.fn().mockResolvedValue({ data: {} }), delete: vi.fn().mockResolvedValue({ data: {} })` | | |
| TASK-003 | In `ordering/_tests/ordering.api.spec.ts` lines 6-8 (the `vi.mock` block): change `get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn()` to `get: vi.fn().mockResolvedValue({ data: {} }), post: vi.fn().mockResolvedValue({ data: {} }), put: vi.fn().mockResolvedValue({ data: {} }), delete: vi.fn().mockResolvedValue({ data: {} })` (note: ordering mock does not include `patch`) | | |
| TASK-004 | Run `pnpm run test:unit` in `app/Admin` and confirm all 34 previously-failing tests in catalog.api.spec.ts (11), inventory.api.spec.ts (5), ordering.api.spec.ts (18) now pass | | |

### Phase 2 — Fix api.client interceptor test expectations (3 failures)

- GOAL-002: Rewrite 3 tests in `api.client.spec.ts` to match the actual interceptor behavior instead of stale response unwrapping expectations

**Root cause**: Three tests extract interceptor functions from `apiClient.interceptors.response.handlers[0]` and assert they transform the response shape — but the success interceptor is intentionally a pass-through (`return response`), and the error interceptor returns `Promise.resolve({ data: ServerResult<null> })`. The tests expect `MappedResult<T>` / `FailureResult` shapes from a `result.mapper.ts` implementation that no longer exists.

**Fix**: Rewrite each of the 3 tests to assert the actual interceptor output shape:

- **Test 1** — success interceptor returns the raw `AxiosResponse` unchanged (identity)
- **Test 2** — success interceptor returns raw `AxiosResponse` for paged payloads (same pass-through)
- **Test 3** — error interceptor returns `Promise.resolve({ data: ServerResult<null> })` with `isSuccess: false`, `errors: [code/type/metadata]`, `value: null`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Read `src/shared/api/http/api.client.spec.ts` to capture the current test content for `should unwrap successful Result<T> response via value` (lines ~35-57), then rewrite it to assert the interceptor returns the raw `AxiosResponse` unchanged. The test should: (1) create a mock `AxiosResponse`, (2) call the success interceptor with it, (3) `expect(result).toEqual(mockResponse)` — verifying the pass-through behavior | | |
| TASK-006 | Rewrite the test `should unwrap successful PagedResult<T> response via items` (lines ~59-90) identically — assert the raw `AxiosResponse` is returned unchanged. The mock response should include `data.items`, `data.page`, `data.pageSize`, `data.totalCount`, and the assertion should be `expect(result).toEqual(mockResponse)` | | |
| TASK-007 | Rewrite the test `should parse and format error response` (lines ~92-117) to expect the actual error interceptor output: `Promise<AxiosResponse>` with `data` being `ServerResult<null>` containing `isSuccess: false`, `statusCode`, `errors: [{ code: 'ERROR', message, type: 0, metadata: null }]`, `message`, `metadata: null`, `value: null`. Use `async/await` since the interceptor returns a Promise, and verify the shape with `toMatchObject` | | |
| TASK-008 | Run `pnpm run test:unit` in `app/Admin` and confirm all 3 api.client.spec.ts tests now pass | | |

### Phase 3 — Fix taxonomy store test mock shape (1 failure)

- GOAL-003: Change the mock return value in `taxonomy.store.spec.ts` to use `value` instead of `items` to match the `ServerResult<T>` contract

**Root cause**: The test mocks `taxonomyService.list` to return `{ isSuccess: true, items: mockData, ... }` but the store's `fetchTaxonomies` reads `result.value` (from `ServerResult<T>`). Returning `items` causes `result.value` to be `undefined`, so `taxonomies` state is never updated.

**Fix**: Change `items: mockData` to `value: mockData` in the mockResolvedValue payload.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | In `catalog/taxonomies/tests/taxonomy.store.spec.ts`, find the `vi.mocked(taxonomyService.list).mockResolvedValue({...})` call and change the property `items: mockData` to `value: mockData`. Remove the `page`, `pageSize`, and `totalCount` properties from the mock payload since `ServerResult<T>` does not have them (they belong to `ServerPagedResult<T>` which is a different type) | | |
| TASK-010 | Run `pnpm run test:unit` in `app/Admin` and confirm the taxonomy.store.spec.ts test now passes | | |

### Phase 4 — Final verification

- GOAL-004: Verify all 38 failures are resolved and zero new failures introduced

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Run `pnpm run test:unit` in `app/Admin` and confirm 161 tests pass with 0 failures (the exact count depends on other pre-existing status — confirm no red ❌ in output) | | |
| TASK-012 | Run `pnpm run type-check` in `app/Admin` and confirm zero type errors (test files are type-checked by `vue-tsc --build`) | | |

## 3. Alternatives

- **ALT-001**: Convert all `.then()` repository implementations to `async/await` — rejected because it would change implementation files, violate REQ-002 (zero impl changes), and touch 20+ repository files with risk of introducing behavioral bugs
- **ALT-002**: Remove the failing tests entirely — rejected because the tests have valid coverage intent (verify correct API routes are called); only the mock setup is broken
- **ALT-003**: Implement the response unwrapping interceptor in `api.client.ts` — rejected because the current architecture correctly leaves unwrapping to repositories; adding an interceptor would double-unwrap and break the data flow

## 4. Dependencies

- **DEP-001**: The 3 repository spec files (catalog, inventory, ordering) were moved to match the new entity folder structure in a previous refactor (repos now live in `products/repositories/`, `orders/repositories/`, etc.) — the test file import paths were already updated and verified by type-check

## 5. Files

- **FILE-001**: `app/Admin/src/features/catalog/_tests/catalog.api.spec.ts` — mock setup (lines 11-18), change vi.fn() to mockResolvedValue
- **FILE-002**: `app/Admin/src/features/inventories/_tests/inventory.api.spec.ts` — mock setup (lines 7-9), change vi.fn() to mockResolvedValue
- **FILE-003**: `app/Admin/src/features/ordering/_tests/ordering.api.spec.ts` — mock setup (lines 6-8), change vi.fn() to mockResolvedValue
- **FILE-004**: `app/Admin/src/shared/api/http/api.client.spec.ts` — 3 interceptor tests (lines 35-117), rewrite assertions
- **FILE-005**: `app/Admin/src/features/catalog/taxonomies/tests/taxonomy.store.spec.ts` — mock payload shape (line ~44), change `items` to `value`

## 6. Testing

- **TEST-001**: `pnpm run test:unit` — confirms all 38 previously-failing tests now pass (run after each phase for incremental verification)
- **TEST-002**: `pnpm run type-check` — confirms no type regressions in test files

## 7. Risks & Assumptions

- **RISK-001**: The `api.client.spec.ts` success interceptor extraction via `apiClient.interceptors.response.handlers[0]` may expose internal interceptor properties differently across axios versions — the mock AxiosResponse must match the fields the test actually accesses (minimally `data`, `status`, `statusText`, `headers`, `config`); the test should use `toMatchObject` for partial matching rather than `toEqual` for exact matching
- **ASSUMPTION-001**: All 34 `.then()`-related failures have the identical root cause — every repository method calls `.then()` on the `apiClient` return value, and `vi.fn()` returns `undefined` in all cases
- **ASSUMPTION-002**: The `page`, `pageSize`, and `totalCount` fields in the taxonomy store test mock were included because the developer assumed `ServerPagedResult<T>` shape, but the actual `taxonomyService.list()` returns `ServerResult<TaxonomyListItem[]>` (from `taxonomyRepository.list()`), which has a `value: T` property, not paging fields

## 8. Related Specifications / Further Reading

- `plan/refactor-move-repos-mappers-to-entity-folders-1.md` — previous refactor that moved repository files (and changed the import paths in these test files)
- `src/shared/api/http/api.client.ts` — shows the actual interceptor implementation (success is pass-through, error wraps as ServerResult)
- `src/shared/api/utils/result.mapper.ts` — shows the `MappedResult` / `FailureResult` types that the outdated tests expected
