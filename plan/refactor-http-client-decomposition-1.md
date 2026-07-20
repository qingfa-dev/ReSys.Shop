---
goal: Decompose Shared HTTP Layer — Token Service, Error Handler Split, Barrel Exports, Unit Tests
version: 1.0
date_created: 2026-07-20
owner: Admin SPA Team
status: Planned
tags:
  - refactor
  - decomposition
  - testing
  - shared-layer
  - admin-spa
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Decompose the shared HTTP client layer under `src/shared/api/http/` into focused, testable modules. Extract localStorage access into a `token.service.ts`. Split the monolithic `error.handler.ts` into type/parser/normalizer files. Decompose `mapper.utils.ts` into string/object transforms. Add barrel index files at every directory level. Write vitest unit tests for all extracted pure functions and services.

## 1. Requirements & Constraints

- **REQ-001**: All `localStorage` access must go through `token.service.ts` — no direct `localStorage.getItem/setItem/removeItem` in interceptors, handlers, or client.
- **REQ-002**: `error.handler.ts` (currently 116 lines, 3 responsibilities) must be split into at least 3 files: type, normalizer, parser.
- **REQ-003**: `mapper.utils.ts` must be split into `string.transforms.ts` and `object.transforms.ts`.
- **REQ-004**: Every subdirectory under `src/shared/` must have a barrel `index.ts` exporting all public symbols from that directory.
- **REQ-005**: Every extracted module must have a corresponding `__tests__/` unit test file.
- **REQ-006**: The `auth.interceptor.ts` and `refresh.handler.ts` must use `token.service.ts` instead of raw `localStorage`.
- **CON-001**: Zero behavior changes — refactoring must not alter request/response flow, token handling, or error wrapping.
- **CON-002**: Existing imports in `api.client.ts`, `auth.store.ts`, `auth.api.ts` must remain valid or be updated with correct paths.
- **GUD-001**: Test files live alongside source: `src/shared/api/http/handlers/__tests__/error.parser.spec.ts` using vitest + jsdom.
- **PAT-001**: Pure functions exported as named exports; services as named `const` objects (matching `auth.service.ts` pattern).

## 2. Implementation Steps

### Implementation Phase 1 — Token Service

- GOAL-001: Extract all localStorage token access into a single service.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `src/shared/api/http/services/token.service.ts` — export `tokenService` with methods: `getAccessToken(): string \| null`, `getRefreshToken(): string \| null`, `setTokens(access: string, refresh: string): void`, `clearTokens(): void`, `hasTokens(): boolean`. All using keys `'accessToken'` and `'refreshToken'`. | | |
| TASK-002 | Update `src/shared/api/http/interceptors/auth.interceptor.ts` — replace `localStorage.getItem('accessToken')` with `tokenService.getAccessToken()`. | | |
| TASK-003 | Update `src/shared/api/http/handlers/refresh.handler.ts` — replace all `localStorage.getItem/setItem/removeItem` calls with `tokenService.getRefreshToken()`, `tokenService.setTokens()`, `tokenService.clearTokens()`. | | |
| TASK-004 | Update `src/shared/api/http/api.client.ts` — in error interceptor (line 43), replace `localStorage.getItem('accessToken')` with `tokenService.getAccessToken()`. | | |
| TASK-005 | Create `src/shared/api/http/services/__tests__/token.service.spec.ts` — test: `getAccessToken` returns null when not set, returns value after `setTokens`, `getRefreshToken` mirrors, `clearTokens` removes both, `hasTokens` returns true/false. | | |

### Implementation Phase 2 — Error Handler Decomposition

- GOAL-002: Split `error.handler.ts` into type, normalizer, and parser modules.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Create `src/shared/api/http/handlers/error.type.ts` — move `ParsedApiError` interface here. No runtime code. | | |
| TASK-007 | Create `src/shared/api/http/handlers/error.normalizer.ts` — export `parseApiError(error: unknown): ParsedApiError` (the main parse function, 116 lines). Also export `normalizeServerErrors(errors: unknown): Record<string, string[]>` (renamed from `convertServerErrors`). Update internal imports to use `error.type.ts`. | | |
| TASK-008 | Create `src/shared/api/http/interceptors/error.interceptor.ts` — NEW: extract the error-wrapping logic from `api.client.ts` (lines 20–61) into a standalone `errorInterceptor(error: AxiosError): Promise<AxiosResponse>` function. Move the server error construction helper (`createServerErrorFromApiError`) as a private function inside this module. Import `parseApiError` from `error.normalizer` and `tokenService` from `token.service`. | | |
| TASK-009 | Update `src/shared/api/http/api.client.ts` — collapse from 63 lines to ~20 lines. Remove inline error interceptor lambda. Import `errorInterceptor` from `./interceptors/error.interceptor`. Wire: `apiClient.interceptors.response.use(camelCaseInterceptor, errorInterceptor)`. | | |
| TASK-010 | Delete old `src/shared/api/http/handlers/error.handler.ts` and `src/shared/api/http/handlers/refresh.handler.ts` — content already moved. | | |
| TASK-011 | Create `src/shared/api/http/handlers/__tests__/error.normalizer.spec.ts` — test: null/undefined input → 500 Connection Error, Axios 404 response with `isSuccess: false` body → ParsedApiError with statusCode 404, Axios network error (no response) → 500 Connection Error, raw object with status=422 → ParsedApiError with 422, non-object → 500. | | |
| TASK-012 | Create `src/shared/api/http/interceptors/__tests__/error.interceptor.spec.ts` — test: 401 non-auth-refresh request triggers token refresh attempt, 401 on auth/refresh endpoint returns ServerResult with 401 directly, non-401 error wraps to ServerResult with correct statusCode, success passthrough not affected. | | |

### Implementation Phase 3 — Mapper Decomposition

- GOAL-003: Split `mapper.utils.ts` into string and object transform files.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | Create `src/shared/mapper/string.transforms.ts` — export `toCamelCase(str: string): string`. Keep existing implementation. | | |
| TASK-014 | Create `src/shared/mapper/object.transforms.ts` — export `mapKeys<T>(obj, transform): Record<string, unknown>`, `toCamelCaseKeys<T>(obj): Record<string, unknown>`. Import `toCamelCase` from `string.transforms.ts`. | | |
| TASK-015 | Delete old `src/shared/mapper/mapper.utils.ts`. Update `src/shared/mapper/index.ts` barrel to export from new files. | | |
| TASK-016 | Update `src/shared/api/http/interceptors/camel-case.interceptor.ts` — change import from `@/shared/mapper/mapper.utils` to `@/shared/mapper/object.transforms`. | | |
| TASK-017 | Update `src/shared/index.ts` — change re-exports from `mapper/mapper.utils` to `mapper/string.transforms` and `mapper/object.transforms`. | | |
| TASK-018 | Create `src/shared/mapper/__tests__/string.transforms.spec.ts` — test: `toCamelCase('snake_case')` → `'snakeCase'`, `toCamelCase('foo_bar_baz')` → `'fooBarBaz'`, `toCamelCase('already')` → `'already'`, `toCamelCase('')` → `''`. | | |
| TASK-019 | Create `src/shared/mapper/__tests__/object.transforms.spec.ts` — test: `mapKeys({foo_bar: 1, baz_qux: 2}, toCamelCase)` → `{fooBar: 1, bazQux: 2}`, nested objects not traversed, preserves non-string keys. | | |

### Implementation Phase 4 — Barrel Index Files

- GOAL-004: Add missing `index.ts` barrel exports at every subdirectory level.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | Create `src/shared/api/http/index.ts` — re-export `apiClient` (default as named), `authInterceptor`, `camelCaseInterceptor`, `errorInterceptor`. | | |
| TASK-021 | Create `src/shared/api/http/handlers/index.ts` — re-export `parseApiError` from `error.normalizer`, `ParsedApiError` from `error.type`. | | |
| TASK-022 | Create `src/shared/api/http/interceptors/index.ts` — re-export `authInterceptor`, `camelCaseInterceptor`, `errorInterceptor`. | | |
| TASK-023 | Create `src/shared/api/http/services/index.ts` — re-export `tokenService`. | | |
| TASK-024 | Create `src/shared/api/types/index.ts` — re-export all from `result.type`. | | |
| TASK-025 | Create `src/shared/api/utils/index.ts` — re-export `parseApiError`, `ParsedApiError` from `api.utils`. | | |
| TASK-026 | Create `src/shared/mapper/index.ts` — re-export from `string.transforms` and `object.transforms`. | | |
| TASK-027 | Update `src/shared/api/utils/api.utils.ts` — change import from `../http/handlers/error.handler` to `../http/handlers/error.normalizer` and `../http/handlers/error.type`. | | |

### Implementation Phase 5 — Interceptor Unit Tests

- GOAL-005: Unit tests for auth and camel-case interceptors.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-028 | Create `src/shared/api/http/interceptors/__tests__/auth.interceptor.spec.ts` — mock `tokenService`, test: no token → header not set, token present → `Authorization: Bearer <token>` set, null headers → handled gracefully. | | |
| TASK-029 | Create `src/shared/api/http/interceptors/__tests__/camel-case.interceptor.spec.ts` — test: response with snake_case data → keys transformed to camelCase, null data → passthrough, non-object data → passthrough, nested object keys → not transformed (top-level only). | | |

### Implementation Phase 6 — Final Verification

- GOAL-006: Ensure all imports resolve, type-check, tests pass, and build succeeds.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-030 | Run `vue-tsc --build --noEmit` — zero errors. | | |
| TASK-031 | Run `pnpm run test:unit` — all new unit tests pass. | | |
| TASK-032 | Run `vite build` — production build succeeds. | | |

## 3. Alternatives

- **ALT-001**: Keep `error.handler.ts` as single file. Rejected — the file does 3 things (type, normalizer, parser), making testing of individual concerns impossible without mocking the entire module.
- **ALT-002**: Use a class-based `TokenService` with DI. Rejected — overengineering for a simple 5-method localStorage wrapper. A plain const object is sufficient and matches the existing codebase pattern (`authService`, `authRepository`).
- **ALT-003**: Use msw for interceptor tests. Rejected for this phase — vitest with manual mock of `tokenService` and `axios` is simpler and tests exactly what's in scope.

## 4. Dependencies

- **DEP-001**: `vitest` ^4.1.9, `@vue/test-utils` ^2.4.11, `jsdom` ^29.1.1 — already installed.
- **DEP-002**: `axios` ^1.18.1 — already installed; types needed for interceptor tests (`InternalAxiosRequestConfig`, `AxiosError`, `AxiosResponse`).
- **DEP-003**: No new packages required.

## 5. Files

### New Files

- **FILE-001**: `src/shared/api/http/services/token.service.ts` — NEW: token get/set/clear service
- **FILE-002**: `src/shared/api/http/services/__tests__/token.service.spec.ts` — NEW
- **FILE-003**: `src/shared/api/http/services/index.ts` — NEW
- **FILE-004**: `src/shared/api/http/handlers/error.type.ts` — NEW: ParsedApiError interface
- **FILE-005**: `src/shared/api/http/handlers/error.normalizer.ts` — NEW: parseApiError + normalizeServerErrors
- **FILE-006**: `src/shared/api/http/handlers/__tests__/error.normalizer.spec.ts` — NEW
- **FILE-007**: `src/shared/api/http/handlers/index.ts` — NEW
- **FILE-008**: `src/shared/api/http/interceptors/error.interceptor.ts` — NEW: error wrapping interceptor
- **FILE-009**: `src/shared/api/http/interceptors/__tests__/error.interceptor.spec.ts` — NEW
- **FILE-010**: `src/shared/api/http/interceptors/__tests__/auth.interceptor.spec.ts` — NEW
- **FILE-011**: `src/shared/api/http/interceptors/__tests__/camel-case.interceptor.spec.ts` — NEW
- **FILE-012**: `src/shared/api/http/interceptors/index.ts` — NEW
- **FILE-013**: `src/shared/api/http/index.ts` — NEW
- **FILE-014**: `src/shared/api/types/index.ts` — NEW
- **FILE-015**: `src/shared/api/utils/index.ts` — NEW
- **FILE-016**: `src/shared/mapper/string.transforms.ts` — NEW: toCamelCase
- **FILE-017**: `src/shared/mapper/object.transforms.ts` — NEW: mapKeys, toCamelCaseKeys
- **FILE-018**: `src/shared/mapper/__tests__/string.transforms.spec.ts` — NEW
- **FILE-019**: `src/shared/mapper/__tests__/object.transforms.spec.ts` — NEW
- **FILE-020**: `src/shared/mapper/index.ts` — NEW

### Modified Files

- **FILE-021**: `src/shared/api/http/api.client.ts` — MODIFY: use tokenService, wire errorInterceptor
- **FILE-022**: `src/shared/api/http/interceptors/auth.interceptor.ts` — MODIFY: use tokenService
- **FILE-023**: `src/shared/api/http/interceptors/camel-case.interceptor.ts` — MODIFY: update import path
- **FILE-024**: `src/shared/api/utils/api.utils.ts` — MODIFY: update re-export paths
- **FILE-025**: `src/shared/index.ts` — MODIFY: update re-export paths
- **FILE-026**: `src/shared/api/index.ts` — MODIFY: add new exports

### Deleted Files

- **FILE-027**: `src/shared/api/http/handlers/error.handler.ts` — DELETE: split into error.type + error.normalizer
- **FILE-028**: `src/shared/api/http/handlers/refresh.handler.ts` — DELETE: replaced by token.service + inlined refresh in error.interceptor
- **FILE-029**: `src/shared/mapper/mapper.utils.ts` — DELETE: split into string/object transforms

## 6. Testing

- **TEST-001**: `token.service.spec.ts` — 5 test cases: getAccessToken returns null/returns after set, getRefreshToken mirrors, clearTokens removes both, hasTokens predicate.
- **TEST-002**: `error.normalizer.spec.ts` — 6 test cases: null input, non-object input, Axios 404 with server error shape, Axios network error (no response), raw error object with custom status, Axios 422 with `Errors` array.
- **TEST-003**: `error.interceptor.spec.ts` — 5 test cases: 401 on non-refresh URL triggers token refresh, 401 on refresh URL bypasses refresh, 500 error wraps to ServerResult, error without config handled, success response passthrough.
- **TEST-004**: `auth.interceptor.spec.ts` — 3 test cases: no token, token present, null headers.
- **TEST-005**: `camel-case.interceptor.spec.ts` — 4 test cases: snake_case data, null data, non-object data, data without underscore keys.
- **TEST-006**: `string.transforms.spec.ts` — 4 test cases: single underscore, multiple underscores, already camelCase, empty string.
- **TEST-007**: `object.transforms.spec.ts` — 4 test cases: flat object, empty object, null values preserved, non-recursive (nested keys not transformed).

## 7. Risks & Assumptions

- **RISK-001**: Moving the error interceptor out of `api.client.ts` may create a circular dependency if `token.service` imports `apiClient`. Mitigation: `token.service.ts` does not import anything from `shared/` — it's a pure localStorage wrapper.
- **RISK-002**: `refresh.handler.ts` uses `window.location.href = '/login'` on refresh failure. Moving this into `error.interceptor.ts` must preserve the same redirect behavior. Mitigation: copy the logic exactly; test with jsdom `window.location` mock.
- **ASSUMPTION-001**: No other files in `src/` import directly from `error.handler.ts`, `refresh.handler.ts`, or `mapper.utils.ts` beyond the ones listed in this plan. Verification: grep before deleting.
- **ASSUMPTION-002**: The `toCamelCaseKeys` function is only used in `camel-case.interceptor.ts` and does not recursively transform nested objects. Existing behavior preserved.

## 8. Related Specifications / Further Reading

- [Current `api.client.ts`](/home/qingfa/Repos/ReSys.Shop/app/Admin/src/shared/api/http/api.client.ts)
- [Current `error.handler.ts`](/home/qingfa/Repos/ReSys.Shop/app/Admin/src/shared/api/http/handlers/error.handler.ts)
- [Current `mapper.utils.ts`](/home/qingfa/Repos/ReSys.Shop/app/Admin/src/shared/mapper/mapper.utils.ts)
- [Legacy Admin shared layer — `app/lagacy/Admin/src/shared/`](/home/qingfa/Repos/ReSys.Shop/app/lagacy/Admin/src/shared/)
- [Testing docs — `docs/codebase/TESTING.md`](/home/qingfa/Repos/ReSys.Shop/docs/codebase/TESTING.md)
- [Conventions — `docs/codebase/CONVENTIONS.md`](/home/qingfa/Repos/ReSys.Shop/docs/codebase/CONVENTIONS.md)
