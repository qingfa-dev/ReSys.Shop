---
goal: Full Unit Tests for shared/ and common/ in Admin SPA
version: 1.0
date_created: 2026-07-20
last_updated: 2026-07-20
status: Planned
tags: [testing, shared, common, admin-spa]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Add comprehensive unit tests for all testable logic in `src/shared/` and `src/common/` of the Admin SPA (`app/Admin/`). Currently 6 source files have tests; 6 more contain untested functions, factory helpers, builder logic, and async HTTP flows. Each new test file follows existing Vitest + `@vue/test-utils` conventions.

## 1. Requirements & Constraints

- **REQ-001**: Every exported function in `shared/types/`, `shared/utils/`, `common/api/handlers/`, and `common/api/interceptors/` must have a corresponding Vitest test file
- **REQ-002**: Tests must cover happy path, edge cases (null/undefined/zero/negative inputs, empty collections), and error paths
- **REQ-003**: Test files placed alongside their source in a `__tests__/` sibling directory
- **REQ-004**: Follow existing naming convention: `<module>.spec.ts`
- **REQ-005**: Use `describe`/`it`/`expect` from Vitest; `vi.fn()`/`vi.mock()` for dependencies
- **REQ-006**: Test files must be committed independently after passing
- **CON-001**: Do NOT test barrel files, empty stubs, type-only files, or component Vue files (already tested)
- **CON-002**: Do NOT test `features/` files (auth store, auth API, login schema) — those are feature-specific, not shared/common
- **CON-003**: Do NOT test `app/layout/` or `app/router/` — those are app-level concerns
- **PAT-001**: Follow existing patterns from `object.transforms.spec.ts` and `error.normalizer.spec.ts`
- **PAT-002**: Use `vi.mock('@/common/auth/token.service', ...)` for tokenService mocking
- **PAT-003**: Use `vi.mock('axios', ...)` for axios mocking in HTTP-dependent tests

## 2. Implementation Steps

### Implementation Phase 1: shared/types/ — Pure Utility Tests

- GOAL-001: Add unit tests for pagination.model.ts, result.type.ts, and filtering.model.ts factory/helper functions

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `__tests__/pagination.model.spec.ts` — tests for `normalizePage`, `normalizePageSize`, `skip`, `totalPages`, `createPageModel` (5 functions, ~20 cases) | | |
| TASK-002 | Create `__tests__/result.type.spec.ts` — tests for `createServerResult`, `createServerErrorResult`, `createServerPagedResult` (3 functions, ~15 cases) | | |
| TASK-003 | Create `__tests__/filtering.model.spec.ts` — tests for `createFilterGroup` (1 function, ~5 cases) | | |

### Implementation Phase 2: shared/utils/ — Builder Logic Tests

- GOAL-002: Add unit tests for query-string.builder.ts serialization functions

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Create `__tests__/query-string.builder.spec.ts` — tests for `buildFilterParam`, `buildSearchParams`, `buildSortParams`, `buildPageParams` (4 functions, ~25 cases) | | |

### Implementation Phase 3: common/api/ — Async HTTP Flow Tests

- GOAL-003: Add unit tests for refresh.handler.ts and error.interceptor.ts

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Create `__tests__/refresh.handler.spec.ts` — tests for `refreshTokens` (1 function, ~8 cases covering all branches) | | |
| TASK-006 | Create `__tests__/error.interceptor.spec.ts` — tests for `errorInterceptor` (1 function, ~10 cases covering 401 retry, refresh guard, error normalization) | | |

## 3. Alternatives

- **ALT-001**: Test all 11 files in one pass — rejected because mixing pure-utility tests with heavily-mocked async tests makes review harder
- **ALT-002**: Skip Phase 3 (HTTP tests) as "integration-level" — rejected because `refreshTokens` and `errorInterceptor` have complex branching logic that can be unit-tested with proper mocking, and these are critical security flows (token refresh, 401 handling)

## 4. Dependencies

- **DEP-001**: `vitest` — already configured in `app/Admin/vitest.config.ts` with jsdom environment
- **DEP-002**: `@vue/test-utils` — already installed (used by existing component tests)
- **DEP-003**: `vi.mock()` for `@/common/auth/token.service` — existing pattern from `auth.interceptor.spec.ts`
- **DEP-004**: `vi.mock('axios')` — for `refresh.handler.spec.ts` HTTP mocking

## 5. Files

- **FILE-001**: `app/Admin/src/shared/types/__tests__/pagination.model.spec.ts` — new file
- **FILE-002**: `app/Admin/src/shared/types/__tests__/result.type.spec.ts` — new file
- **FILE-003**: `app/Admin/src/shared/types/__tests__/filtering.model.spec.ts` — new file
- **FILE-004**: `app/Admin/src/shared/utils/__tests__/query-string.builder.spec.ts` — new file
- **FILE-005**: `app/Admin/src/common/api/handlers/__tests__/refresh.handler.spec.ts` — new file
- **FILE-006**: `app/Admin/src/common/api/interceptors/__tests__/error.interceptor.spec.ts` — new file
- **FILE-007**: `app/Admin/src/shared/types/pagination.model.ts` — existing source (read-only reference)
- **FILE-008**: `app/Admin/src/shared/types/result.type.ts` — existing source (read-only reference)
- **FILE-009**: `app/Admin/src/shared/types/filtering.model.ts` — existing source (read-only reference)
- **FILE-010**: `app/Admin/src/shared/utils/query-string.builder.ts` — existing source (read-only reference)
- **FILE-011**: `app/Admin/src/common/api/handlers/refresh.handler.ts` — existing source (read-only reference)
- **FILE-012**: `app/Admin/src/common/api/interceptors/error.interceptor.ts` — existing source (read-only reference)

## 6. Testing

### TASK-001: pagination.model.spec.ts

**Test cases:**

| # | Function | Case | Input | Expected |
|---|----------|------|-------|----------|
| 1 | `normalizePage` | uses default when undefined | `(undefined, bounds)` | `1` |
| 2 | `normalizePage` | clamps to >= 1 | `(0, bounds)` | `1` |
| 3 | `normalizePage` | clamps negative | `(-5, bounds)` | `1` |
| 4 | `normalizePage` | passes valid value | `(5, bounds)` | `5` |
| 5 | `normalizePage` | clamps to MAX_SAFE_INTEGER | `(Infinity, bounds)` | `Number.MAX_SAFE_INTEGER` |
| 6 | `normalizePageSize` | uses default when undefined | `(undefined, bounds)` | `10` |
| 7 | `normalizePageSize` | clamps to >= 1 | `(0, bounds)` | `1` |
| 8 | `normalizePageSize` | clamps to maxPageSize | `(999, bounds)` | `100` |
| 9 | `normalizePageSize` | passes valid value | `(25, bounds)` | `25` |
| 10 | `skip` | page 1, size 10 | `(1, 10)` | `0` |
| 11 | `skip` | page 3, size 20 | `(3, 20)` | `40` |
| 12 | `skip` | page 0, size 10 | `(0, 10)` | `-10` |
| 13 | `totalPages` | exact division | `(100, 10)` | `10` |
| 14 | `totalPages` | partial page | `(25, 10)` | `3` |
| 15 | `totalPages` | zero pageSize | `(100, 0)` | `0` |
| 16 | `totalPages` | zero totalCount | `(0, 10)` | `0` |
| 17 | `createPageModel` | all defaults | `()` | `{ page: 1, pageSize: 10, bounds: defaultPageBounds }` |
| 18 | `createPageModel` | custom page/size | `(3, 20)` | `{ page: 3, pageSize: 20, bounds: defaultPageBounds }` |
| 19 | `createPageModel` | custom bounds | `(undefined, undefined, customBounds)` | `page: customBounds.defaultPage, pageSize: customBounds.defaultPageSize` |
| 20 | `createPageModel` | clamps oversized inputs | `(9999, 9999)` | `page: MAX_SAFE_INTEGER, pageSize: 100` |

### TASK-002: result.type.spec.ts

**Test cases:**

| # | Function | Case | Input | Expected |
|---|----------|------|-------|----------|
| 1 | `createServerResult` | 200 success | `(200, 'data')` | `{ isSuccess: true, statusCode: 200, value: 'data', errors: [], message: null, metadata: null }` |
| 2 | `createServerResult` | 201 created | `(201, { id: 1 })` | `{ isSuccess: true, statusCode: 201 }` |
| 3 | `createServerResult` | 400 failure | `(400, null)` | `{ isSuccess: false, statusCode: 400, value: null }` |
| 4 | `createServerResult` | 500 failure | `(500, 'error')` | `{ isSuccess: false, statusCode: 500 }` |
| 5 | `createServerResult` | with message | `(200, 'data', 'OK')` | `{ message: 'OK' }` |
| 6 | `createServerResult` | with metadata | `(200, 'data', undefined, { key: 'val' })` | `{ metadata: { key: 'val' } }` |
| 7 | `createServerErrorResult` | validation error | `(422, [{ code: 'V', message: 'bad', type: 0, metadata: null }])` | `{ isSuccess: false, statusCode: 422, errors: [...], value: undefined, message: null }` |
| 8 | `createServerErrorResult` | with message | `(409, [], 'Conflict')` | `{ message: 'Conflict', isSuccess: false }` |
| 9 | `createServerErrorResult` | with metadata | `(500, [], undefined, { trace: 'x' })` | `{ metadata: { trace: 'x' } }` |
| 10 | `createServerErrorResult` | has null value | `(400, [])` | `value is undefined (type assertion)` |
| 11 | `createServerPagedResult` | success page | `(200, ['a','b'], 1, 2, 100)` | `{ isSuccess: true, items: ['a','b'], page: 1, pageSize: 2, totalCount: 100 }` |
| 12 | `createServerPagedResult` | failure page | `(500, [], 1, 10, 0)` | `{ isSuccess: false, statusCode: 500 }` |
| 13 | `createServerPagedResult` | with message | `(200, ['x'], 1, 10, 1, 'OK')` | `{ message: 'OK' }` |
| 14 | `createServerPagedResult` | with metadata | `(200, [], 1, 10, 0, undefined, { total: 50 })` | `{ metadata: { total: 50 } }` |
| 15 | `ServerResultConstants` | values are frozen | — | `{ Ok: 200, Created: 201, Accepted: 202, NoContent: 204 }` |

### TASK-003: filtering.model.spec.ts

**Test cases:**

| # | Function | Case | Input | Expected |
|---|----------|------|-------|----------|
| 1 | `createFilterGroup` | defaults | `()` | `{ logic: 'and', conditions: [], groups: [] }` |
| 2 | `createFilterGroup` | or logic | `('or')` | `{ logic: 'or', conditions: [], groups: [] }` |
| 3 | `createFilterGroup` | with conditions | `('and', [{ field: 'name', op: '=', value: 'test' }])` | `conditions: [{ field: 'name', op: '=', value: 'test' }]` |
| 4 | `createFilterGroup` | with nested groups | `('and', [], [{ logic: 'and', conditions: [], groups: [] }])` | `groups.length === 1` |
| 5 | `emptyFilterGroup` | frozen singleton | — | `Object.freeze` assertion, default values |

### TASK-004: query-string.builder.spec.ts

**Test cases:**

| # | Function | Case | Input | Expected |
|---|----------|------|-------|----------|
| 1 | `buildFilterParam` | empty model | `emptyFilterModel` | `''` |
| 2 | `buildFilterParam` | single condition 'and' | `FilterModel with root: { logic: 'and', conditions: [{ field: 'name', op: '=', value: 'test' }], groups: [] }` | `'name=test'` |
| 3 | `buildFilterParam` | multiple conditions 'or' | `logic: 'or', conditions: [{ field: 'a', op: '=', value: '1' }, { field: 'b', op: '=', value: '2' }]` | `'a=1|b=2'` |
| 4 | `buildFilterParam` | multiple conditions 'and' | `logic: 'and', conditions: [{ field: 'a', op: '=', value: '1' }, { field: 'b', op: '=', value: '2' }]` | `'a=1,b=2'` |
| 5 | `buildFilterParam` | varied operators | conditions with `!=`, `>`, `*` | `'field!=val,count>10,name*test'` |
| 6 | `buildFilterParam` | nested groups | root with sub-group `{ logic: 'or', conditions: [{ field: 'x', op: '=', value: '1' }, { field: 'y', op: '=', value: '2' }] }` | `'(x=1|y=2)'` |
| 7 | `buildFilterParam` | deep nesting | root('and') → sub('or') → conditions | `'(a=1|b=2),(c=3|d=4)'` |
| 8 | `buildFilterParam` | empty sub-group yields empty | root with empty sub-group | `''` |
| 9 | `buildSearchParams` | empty model | `emptySearchModel` | `{}` |
| 10 | `buildSearchParams` | term only | `term: { value: 'shirt', caseSensitive: false }, fields: [], mode: undefined` | `{ search: 'shirt' }` |
| 11 | `buildSearchParams` | term + fields | `fields: ['name', 'description']` | `{ search: 'shirt', searchFields: 'name,description' }` |
| 12 | `buildSearchParams` | term + fields + mode | `mode: 'exact'` | `{ ..., searchMode: 'exact' }` |
| 13 | `buildSearchParams` | case sensitive | `term: { value: 'Shirt', caseSensitive: true }` | `{ search: 'Shirt', caseSensitive: 'true' }` |
| 14 | `buildSearchParams` | no term value, only fields | `term: { value: '', caseSensitive: false }, fields: ['name']` | `{ searchFields: 'name,description' }? — wait, empty term should still not add search key. Param: term.value is '' so not added. Only searchFields.` → `{ searchFields: 'name' }` |
| 15 | `buildSortParams` | empty model | `emptySortModel` | `{}` |
| 16 | `buildSortParams` | single asc | `clauses: [{ field: 'name', direction: 'asc' }]` | `{ sort: '+name' }` |
| 17 | `buildSortParams` | single desc | `clauses: [{ field: 'price', direction: 'desc' }]` | `{ sort: '-price' }` |
| 18 | `buildSortParams` | multiple clauses | `[{ field: 'name', direction: 'asc' }, { field: 'date', direction: 'desc' }]` | `{ sort: '+name,-date' }` |
| 19 | `buildPageParams` | page model | `page: 3, pageSize: 20` | `{ page: '3', pageSize: '20' }` |
| 20 | `buildPageParams` | page 1, size 10 | `page: 1, pageSize: 10` | `{ page: '1', pageSize: '10' }` |
| 21 | `buildFilterParam` | all 16 filter operators serialize correctly | one condition per op | verify each `serializeCondition` produces correct `field<op>value` |

### TASK-005: refresh.handler.spec.ts

**Test cases (mocking `tokenService` and `axios`):**

| # | Case | Setup | Expected |
|---|------|-------|----------|
| 1 | no refresh token → redirect | `getRefreshToken()` returns `null` | `window.location.href === '/login'`, returns `false` |
| 2 | successful refresh | `getRefreshToken()` returns `'rt'`, `axios.post` resolves with `{ data: { value: { accessToken: 'at', refreshToken: 'rt2' } } }` | `tokenService.setTokens('at', 'rt2')` called, returns `true` |
| 3 | response without value | `axios.post` resolves with `{ data: {} }` (no `value` key) | returns `false` |
| 4 | response with null value | `axios.post` resolves with `{ data: { value: null } }` | returns `false` (no tokens set) |
| 5 | network error → redirect | `axios.post` rejects | `tokenService.clearTokens()` called, `window.location.href === '/login'`, returns `false` |
| 6 | server error response | `axios.post` rejects with 500 | `clearTokens` called, redirect, returns `false` |
| 7 | token persists after success | verify `setTokens` called with correct args | args match response data |
| 8 | redirect suppression in test | Use `Object.defineProperty(window, 'location', { value: { href: '' }, writable: true })` | verify href assignment |

### TASK-006: error.interceptor.spec.ts

**Test cases (mocking `parseApiError`, `tokenService`, `refreshTokens`, dynamic import):**

| # | Case | Setup | Expected |
|---|------|-------|----------|
| 1 | non-401 error → error result | `parseApiError` returns `{ statusCode: 404, title: 'Not Found', detail: 'Missing', errorCode: 'NOT_FOUND' }` | Returns response with `data.statusCode === 404`, `data.isSuccess === false` |
| 2 | 401 on refresh endpoint → immediate error | `statusCode: 401`, `url: '/auth/session/refresh'` | Returns `createErrorResult(401, ..., 'UNAUTHORIZED')`, does NOT call `refreshTokens` |
| 3 | 401 on other endpoint with retry=false → refresh + retry | `statusCode: 401`, `url: '/catalog/products'`, `refreshTokens` returns `true`, `getAccessToken` returns `'new-token'` | Calls `refreshTokens`, sets `Authorization: Bearer new-token`, calls `apiClient(originalRequest)` |
| 4 | 401 refresh fails → fallback error | `statusCode: 401`, `refreshTokens` returns `false` | Returns error result with 401, does NOT retry the request |
| 5 | 401 already retried → fallback error | `statusCode: 401`, `_retry: true` | Returns error result, does NOT call `refreshTokens` again |
| 6 | 500 internal server error | `statusCode: 500, title: 'Server Error'` | Returns errorResult with 500 status |
| 7 | 422 validation error | `statusCode: 422, errorCode: 'VALIDATION'` | Returns errorResult with errorCode 'VALIDATION' |
| 8 | retry with null headers | `statusCode: 401`, `originalRequest.headers: null`, `refreshTokens: true` | Does not crash; graceful fallback |
| 9 | retry with null token after refresh | `statusCode: 401`, `refreshTokens: true`, `getAccessToken: null` | Sets Authorization header? No — skips if `!newToken`. Calls `apiClient(originalRequest)` anyway |
| 10 | createErrorResult fallback | `parseApiError` returns object without `detail` | `createErrorResult` uses `message || 'Request failed'` as fallback |

## 7. Risks & Assumptions

- **RISK-001**: `refresh.handler.ts` modifies `window.location.href` — requires mock setup via `Object.defineProperty`
- **RISK-002**: `error.interceptor.ts` uses dynamic `import('@/common/api')` — requires `vi.mock('@/common/api', ...)` for the retry path
- **RISK-003**: Test files in `shared/types/__tests__/` directory may not exist yet — must create the directory first
- **ASSUMPTION-001**: `vi.mock()` for axios works identically to existing `auth.interceptor.spec.ts` pattern
- **ASSUMPTION-002**: `vitest` auto-mocking via `vi.mock('module')` hoists mocks correctly (default Vitest behavior)
- **ASSUMPTION-003**: All type imports resolve correctly via `@/` alias (already verified by existing test files)

## 8. Related Specifications / Further Reading

- [Shared Component Library P0 Design Spec](../specs/2026-07-20-shared-component-library-design.md)
- [Shared Component Library P0 Implementation Plan](2026-07-20-shared-component-library-p0.md)
- `app/Admin/vitest.config.ts` — existing Vitest configuration (jsdom environment)
- `app/Admin/src/common/api/interceptors/__tests__/auth.interceptor.spec.ts` — reference for axios + tokenService mocking pattern
- `app/Admin/src/common/api/handlers/__tests__/error.normalizer.spec.ts` — reference for handler test pattern
