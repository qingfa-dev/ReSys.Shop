---
goal: Replace @tanstack/vue-query with plain Axios-based API composables in Admin SPA
version: 1.0
date_created: 2026-07-06
owner: Platform Team
status: Completed
tags: refactor, frontend, admin, data-fetching, axios
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Remove the `@tanstack/vue-query` dependency from the Admin SPA (`app/Admin`) and replace all `useQuery`/`useMutation` composables with plain Axios-based async composables that use Vue `ref` for loading/error/data state. Also replace the underlying `fetch()`-based HTTP client with Axios. The Store SPA (`app/Store`) has no TanStack Query dependency and is **not** in scope.

## 1. Requirements & Constraints

- **REQ-001**: All existing API endpoints must continue to function identically
- **REQ-002**: `@tanstack/vue-query` must be fully removed from `package.json` and all imports
- **REQ-003**: The `Envelope<T>` unwrapping behavior must be preserved (`.NET Result<T>` pattern)
- **REQ-004**: Auth token injection via `Authorization: Bearer` header must be preserved
- **REQ-005**: Query composables must still expose `data`, `isLoading`, `error`, and `refetch`
- **REQ-006**: Mutation composables must still expose `mutateAsync` and `isPending`
- **REQ-007**: Cache invalidation via `useQueryClient` in `useAuthState` must be replaced with plain ref management
- **REQ-008**: All existing tests must pass after migration
- **PAT-001**: Follow existing feature-module file layout (`features/{module}/api/`, `features/{module}/composables/`, `features/{module}/ui/`)
- **GUD-001**: `TreatWarningsAsErrors=true` — no lint or type-check warnings
- **CON-001**: No changes to the Store SPA (`app/Store`)

## 2. Implementation Steps

### Implementation Phase 1 — Install Axios and rewrite shared HTTP client

- GOAL-001: Replace `fetch()`-based `shared/api/client.ts` with an Axios-based client

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Install `axios` package in `app/Admin`: `pnpm add axios` | | |
| TASK-002 | Rewrite `app/Admin/src/shared/api/client.ts` — replace `request()` function with an Axios instance using `axios.create()`; preserve all existing method signatures (`get`, `getPaged`, `post`, `put`, `delete`); keep `Envelope<T>` unwrapping in response interceptor; export `axiosInstance` for direct use if needed | | |
| TASK-003 | Update `app/Admin/src/shared/api/fetch-options.ts` — change `buildHeaders()` to provide a request interceptor function instead; keep `setAuthTokenAccessor()` with same API | | |
| TASK-004 | Update `app/Admin/src/shared/api/errors.ts` — ensure `ApiError` is thrown from Axios error interceptor (map Axios error responses to `ApiError`) | | |

### Implementation Phase 2 — Remove TanStack Query configuration

- GOAL-002: Eliminate all TanStack Query infrastructure including plugin registration, query keys, and dead files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Edit `app/Admin/src/app/main.ts` — remove `import { VueQueryPlugin, QueryClient }` and `app.use(VueQueryPlugin, ...)` lines (lines 3, 13-18) | | |
| TASK-006 | Delete `app/Admin/src/app/plugins/vue-query.ts` — dead code, not referenced from `main.ts` | | |
| TASK-007 | Delete `app/Admin/src/shared/api/query-keys.ts` — no longer needed | | |
| TASK-008 | Delete `app/Admin/src/features/auth/api/query-keys.ts` — no longer needed | | |
| TASK-009 | Delete `app/Admin/src/features/identity/users/api/query-keys.ts` — no longer needed | | |

### Implementation Phase 3 — Rewrite auth API composables

- GOAL-003: Replace all 4 auth `useQuery`/`useMutation` wrappers with plain composables using `ref()` + `axiosInstance`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | Rewrite `app/Admin/src/features/auth/api/current-user.ts` — replace `useQuery({ queryKey, queryFn, ... })` with a composable that returns `{ data: Ref<AuthUser \| null>, isLoading: Ref<boolean>, error: Ref<Error \| null>, refetch: () => Promise<void> }`; fetch in a `watchEffect`-like pattern (or expose an explicit `load` + manual trigger) | | |
| TASK-011 | Rewrite `app/Admin/src/features/auth/api/login.ts` — replace `useMutation({ mutationFn })` with a composable returning `{ mutateAsync: (body: LoginRequest) => Promise<AuthTokens>, isPending: Ref<boolean>, error: Ref<Error \| null> }` | | |
| TASK-012 | Rewrite `app/Admin/src/features/auth/api/logout.ts` — same pattern as TASK-011, `mutateAsync: () => Promise<void>` | | |
| TASK-013 | Rewrite `app/Admin/src/features/auth/api/refresh.ts` — same pattern as TASK-011, `mutateAsync: (refreshToken: string) => Promise<AuthTokens>` | | |
| TASK-014 | Update `app/Admin/src/features/auth/api/index.ts` to export the new composables (no `query-keys` export) | | |

### Implementation Phase 4 — Rewrite identity/users API composables

- GOAL-004: Replace all 5 users module `useQuery`/`useMutation` wrappers with plain composables

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Rewrite `app/Admin/src/features/identity/users/api/get-list.ts` — replace `useQuery` with a composable returning `{ data: Ref<PagedResult<UserListItem> \| null>, isLoading: Ref<boolean>, error: Ref<Error \| null>, refetch: () => Promise<void> }`; accept `params: Ref<PageRequest>` and watch it to auto-refetch | | |
| TASK-016 | Rewrite `app/Admin/src/features/identity/users/api/get-by-id.ts` — replace `useQuery` with composable returning same shape; accept `id: Ref<string \| null>`; skip fetch when `id` is `null` | | |
| TASK-017 | Rewrite `app/Admin/src/features/identity/users/api/create.ts` — replace `useMutation` with composable returning `{ mutateAsync: (body: UserCreateRequest) => Promise<User>, isPending: Ref<boolean>, error: Ref<Error \| null> }`; no cache invalidation needed | | |
| TASK-018 | Rewrite `app/Admin/src/features/identity/users/api/update.ts` — replace `useMutation` with composable returning `{ mutateAsync: (body: UserUpdateRequest) => Promise<User>, isPending, error }` | | |
| TASK-019 | Rewrite `app/Admin/src/features/identity/users/api/delete.ts` — replace `useMutation` with composable returning `{ mutateAsync: (id: UserId) => Promise<void>, isPending, error }` | | |
| TASK-020 | Update `app/Admin/src/features/identity/users/api/index.ts` to remove `usersQueryKeys` export | | |

### Implementation Phase 5 — Rewrite `useAuthState.ts`

- GOAL-005: Remove all `useQueryClient` calls from the auth state composable; replace with plain ref-based state management

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-021 | Rewrite `app/Admin/src/features/auth/composables/useAuthState.ts` — remove `import { useQueryClient }` and `const qc = useQueryClient()` (line 2, line 16); in `setTokens()`, replace `qc.setQueryData(...)` with a direct `currentUser.data.value = user` assignment; in `clear()`, replace `qc.removeQueries(...)` with `currentUser.data.value = null`; keep `useCurrentUser()`, `useLogin()`, `useLogout()` calls but consume their `.data`/`.mutateAsync` refs directly | | |

### Implementation Phase 6 — Update UI components consuming TanStack Query hooks

- GOAL-006: Adapt all Vue components to the new composable API

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-022 | Update `app/Admin/src/features/auth/ui/LoginForm.vue` — `login.isPending` stays the same (ref); `login.mutateAsync()` stays the same; no changes needed if interface preserved | | |
| TASK-023 | Update `app/Admin/src/features/auth/ui/LogoutButton.vue` — `logout.mutateAsync()` stays the same; no changes needed if interface preserved | | |
| TASK-024 | Update `app/Admin/src/features/identity/users/ui/UserList.vue` — `query.isLoading.value` becomes `query.isLoading.value` (same); `query.data.value?.items` stays same; `query.refetch()` stays same; `remove.mutateAsync()` stays same; confirm `useUser(selectedId)` still works with new composable signature | | |
| TASK-025 | Update `app/Admin/src/features/identity/users/composables/useUserForm.ts` — `create.mutateAsync()`, `update.mutateAsync()`, and `create.isPending.value` / `update.isPending.value` stay the same if interface preserved | | |

### Implementation Phase 7 — Update tests

- GOAL-007: Remove `VueQueryPlugin` and `QueryClient` from test harnesses; update assertions

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-026 | Rewrite `app/Admin/src/features/auth/__tests__/api/login.spec.ts` — remove `VueQueryPlugin`/`QueryClient` imports and setup; mount test host component that directly calls the rewritten `useLogin` composable; verify `api.post` is called with correct endpoint | | |
| TASK-027 | Rewrite `app/Admin/src/features/auth/__tests__/composables/useAuthGuard.spec.ts` — remove `VueQueryPlugin`/`QueryClient`; `useAuthGuard` doesn't directly use TanStack Query so only the plugin setup needs removal | | |
| TASK-028 | Update `app/Admin/src/app/__tests__/router.spec.ts` — remove `VueQueryPlugin`/`QueryClient` imports and `app.use(VueQueryPlugin, ...)` calls in test harness | | |
| TASK-029 | Rewrite `app/Admin/src/features/identity/users/__tests__/api/get-list.spec.ts` — remove `VueQueryPlugin`/`QueryClient`; mount test host that calls `useUsersList(params)`; verify `api.getPaged` is called with correct URL fragment | | |

### Implementation Phase 8 — Final cleanup and verification

- GOAL-008: Remove the package dependency and verify everything compiles and passes

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-030 | Remove `@tanstack/vue-query` from `app/Admin/package.json` dependencies | | |
| TASK-031 | Run `pnpm install` in `app/Admin` to update lockfile | | |
| TASK-032 | Run `cd app/Admin && pnpm run type-check` to verify TypeScript compiles without errors | | |
| TASK-033 | Run `cd app/Admin && pnpm run lint` to verify no lint errors | | |
| TASK-034 | Run `cd app/Admin && pnpm run test:unit` to verify all tests pass | | |
| TASK-035 | Run `cd app/Admin && pnpm run build-only` to verify production build succeeds | | |

## 3. Alternatives

- **ALT-001 (Keep TanStack Query)**: TanStack Query provides caching, deduplication, and background refetching. Keeping it would avoid churn. **Rejected** because the current app has only 9 API endpoints and no complex cache invalidation patterns — the overhead of the library is not justified.
- **ALT-002 (Use Pinia stores like Store app)**: Could centralize all API state in Pinia stores. **Rejected** because it would require more restructuring; the composable-per-API pattern is lighter and maps directly to the existing file layout.
- **ALT-003 (Keep `fetch()` and only remove TanStack)**: Would be simpler but the user explicitly wants Axios, and Axios provides better interceptor support, request cancellation, and broader ecosystem compatibility.

## 4. Dependencies

- **DEP-001**: `axios` npm package — to be installed in Phase 1
- **DEP-002**: The existing `@/shared/api/client` module — will be rewritten but its public API must remain compatible so all consumers don't need simultaneous changes

## 5. Files

| File | Action | Phase |
|------|--------|-------|
| `app/Admin/package.json` | Edit — remove `@tanstack/vue-query`, add `axios` | 1, 8 |
| `app/Admin/src/shared/api/client.ts` | Rewrite — Axios-based HTTP client | 1 |
| `app/Admin/src/shared/api/fetch-options.ts` | Update — interceptor-compatible auth | 1 |
| `app/Admin/src/shared/api/errors.ts` | Update — Axios error mapping | 1 |
| `app/Admin/src/app/main.ts` | Edit — remove VueQueryPlugin registration | 2 |
| `app/Admin/src/app/plugins/vue-query.ts` | Delete | 2 |
| `app/Admin/src/shared/api/query-keys.ts` | Delete | 2 |
| `app/Admin/src/features/auth/api/query-keys.ts` | Delete | 2 |
| `app/Admin/src/features/identity/users/api/query-keys.ts` | Delete | 2 |
| `app/Admin/src/features/auth/api/current-user.ts` | Rewrite | 3 |
| `app/Admin/src/features/auth/api/login.ts` | Rewrite | 3 |
| `app/Admin/src/features/auth/api/logout.ts` | Rewrite | 3 |
| `app/Admin/src/features/auth/api/refresh.ts` | Rewrite | 3 |
| `app/Admin/src/features/auth/api/index.ts` | Edit | 3 |
| `app/Admin/src/features/identity/users/api/get-list.ts` | Rewrite | 4 |
| `app/Admin/src/features/identity/users/api/get-by-id.ts` | Rewrite | 4 |
| `app/Admin/src/features/identity/users/api/create.ts` | Rewrite | 4 |
| `app/Admin/src/features/identity/users/api/update.ts` | Rewrite | 4 |
| `app/Admin/src/features/identity/users/api/delete.ts` | Rewrite | 4 |
| `app/Admin/src/features/identity/users/api/index.ts` | Edit | 4 |
| `app/Admin/src/features/auth/composables/useAuthState.ts` | Rewrite | 5 |
| `app/Admin/src/features/auth/ui/LoginForm.vue` | Verify (likely no changes) | 6 |
| `app/Admin/src/features/auth/ui/LogoutButton.vue` | Verify (likely no changes) | 6 |
| `app/Admin/src/features/identity/users/ui/UserList.vue` | Verify/update | 6 |
| `app/Admin/src/features/identity/users/composables/useUserForm.ts` | Verify/update | 6 |
| `app/Admin/src/features/auth/__tests__/api/login.spec.ts` | Rewrite | 7 |
| `app/Admin/src/features/auth/__tests__/composables/useAuthGuard.spec.ts` | Update | 7 |
| `app/Admin/src/app/__tests__/router.spec.ts` | Update | 7 |
| `app/Admin/src/features/identity/users/__tests__/api/get-list.spec.ts` | Rewrite | 7 |

## 6. Testing

- **TEST-001**: All existing unit tests pass (`cd app/Admin && pnpm run test:unit`)
- **TEST-002**: TypeScript compiles without errors (`cd app/Admin && pnpm run type-check`)
- **TEST-003**: Linter produces no warnings (`cd app/Admin && pnpm run lint`)
- **TEST-004**: Production build succeeds (`cd app/Admin && pnpm run build-only`)
- **TEST-005**: Manual smoke test — login flow works end-to-end
- **TEST-006**: Manual smoke test — user list, create, update, delete flows work

## 7. Risks & Assumptions

- **RISK-001**: Axios interceptor for auth tokens may behave differently from the current `buildHeaders()` approach — must ensure token injection fires on every request, including after token refresh
- **RISK-002**: The `Envelope<T>` unwrapping logic in `client.ts` is complex (checks `isSuccess` dynamically) — must preserve exact behavior in Axios response interceptor
- **RISK-003**: Axios error handling differs from `fetch` — the `ApiError` throwing must guarantee the same status/message shape
- **ASSUMPTION-001**: All current consumers of the API composables only use `.data`, `.isLoading`, `.error`, `.refetch` (queries) and `.mutateAsync`, `.isPending` (mutations) — no consumer accesses `.isSuccess`, `.isError`, or other TanStack Query-specific properties that have no equivalent in the replacement
- **ASSUMPTION-002**: No runtime behavior changed — the `useAuthState` composable's `setTokens`/`clear` functions work correctly after removing cache invalidation
- **ASSUMPTION-003**: The `pnpm` lockfile updates cleanly when swapping `@tanstack/vue-query` for `axios`

## 8. Related Specifications / Further Reading

- [Axios documentation](https://axios-http.com/docs/intro)
- `app/Store/src/api.ts` — reference implementation of a simple Axios-like pattern (currently uses `fetch`)
- `app/Admin/src/shared/api/client.ts` — current client to be replaced
