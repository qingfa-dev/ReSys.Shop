---
goal: Enable Vue Router navigation guards in the Admin SPA: re-activate the authentication guard that redirects unauthenticated users to the login page, add a guest-only guard that redirects authenticated users away from login/auth pages, and make the login flow honor the `redirect` query parameter so the user returns to their originally-intended page with open-redirect protection.
version: 1.0
date_created: 2026-08-04
last_updated: 2026-08-04
owner: ReSys.Shop Engineering
status: 'Completed'
tags: [`feature`, `admin-spa`, `vue`, `router`, `auth`, `guards`]
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

The Admin SPA (`app/Admin/`) scaffolds its router (`app/Admin/src/app/router/`) but the navigation guard that protects authenticated screens is currently disabled. In `app/Admin/src/app/router/guards.ts` the `beforeEach` hook calls `useAuthStore().init()` exactly once (guarded by a module-level `isInitialized` flag) and then performs no redirect: the actual `requiresAuth` check is commented out with a `// TODO: re-enable auth guard after route scaffold review` marker.

As a result, a user can reach the `/` AppLayout subtree (`/`, `/catalog/products`, `/identity/users`, etc.) without being authenticated; those views render and then fail on their `getSession`-backed API calls. The root route `routes.ts:20` already declares `meta: { requiresAuth: true }`, and the three auth routes (`login`, `forgot-password`, `reset-password`) already declare `meta: { requiresAuth: false }`, so the metadata is in place — only the guard logic is disabled.

This plan re-enables the authentication guard, introduces a `guestOnly` guard to keep logged-in users off the login/auth pages, and makes `LoginPage.vue` honor the `redirect` query (Append) with an open-redirect-safe resolver. The store already exposes `init()`, `isAuthenticated`, and `logout()` needed for these flows (`authStore.ts`), and no new dependencies are required.

## 1. Requirements & Constraints

- **REQ-001**: When a target route has `meta.requiresAuth === true` and the user is not authenticated, the `beforeEach` guard MUST return a `{ name: 'login', query: { redirect: to.fullPath } }` navigation redirect.
- **REQ-002**: When a target route has `meta.guestOnly === true` and the user IS authenticated, the `beforeEach` guard MUST return a redirect to `{ name: 'dashboard' }`.
- **REQ-003**: The `RouteMeta` interface MUST expose a `guestOnly?: boolean` flag alongside the existing `requiresAuth?: boolean`.
- **REQ-004**: The three guard/auth routes (`login`, `forgot-password`, `reset-password`) MUST carry `guestOnly: true` so authenticated users do not see them.
- **REQ-005**: `LoginPage.vue` MUST, after a successful login, navigate to the value of `route.query.redirect` when it is a safe internal path, otherwise navigate to `/` (the existing default).
- **REQ-006**: The `useAuthStore().init()` invocation in the guard MUST remain once-per-session via the existing `isInitialized` flag, and `router.afterEach` MUST continue setting `document.title`.
- **SEC-001**: Open-redirect prevention — the `redirect` query value MUST only be honored when it is an absolute-origin internal path: it must be a `string`, start with a single `/`, and NOT start with `//` or `/\`. Any other value fails back to `/`.
- **CON-001**: No new runtime dependency packages may be added; guard logic uses only `vue-router`, `pinia`, and existing shared utilities.
- **CON-002**: `cd app/Admin && pnpm run lint && pnpm run type-check && pnpm run test:unit` MUST complete with 0 errors before the plan is marked complete.
- **CON-003**: Comments must follow the Admin SPA Code Commenting Standard: single-line `// Label: Capitalised sentence.` with lowercase labels chosen from the standard (e.g. `Validate:`, `Redirect:`), 100-char limit (F3). See `app/Admin/AGENTS.md`.
- **CON-004**: Guards MUST return values (not the legacy `next()` callback) per `commit 39a42277`.
- **GUD-001**: Keep the guard logic minimal and deterministic; the auth store owns all auth state, the guard only reads `isAuthenticated` and redirects.
- **GUD-002**: Prefer a pure, exported helper for redirect resolution so it is directly unit-testable without mounting router or components.
- **PAT-001**: Route metadata is declared via the merged `RouteMeta` in `app/Admin/src/app/router/route-meta.ts` and read via `to.meta` in the guard.
- **PAT-002**: Parent route metadata (`requiresAuth: true` on `/`) merges into child routes and to `to.meta.requiresAuth` on every descendant; child routes DO NOT need to restate it.

## 2. Implementation Steps

### Implementation Phase 1 — Re-enable the auth guard and add the guest-only guard

- GOAL-001: Restore the `requiresAuth` redirect and add a `guestOnly` redirect in the router guard, plus expose `guestOnly` in `RouteMeta` and mark the auth routes guest-only.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | In `app/Admin/src/app/router/guards.ts`, replace the commented-out branch (current lines 15-18) with live guard logic. After the `isInitialized`/`store.init()` block, add, in order: `if (to.meta.guestOnly && store.isAuthenticated) { return { name: 'dashboard' } }` then `if (to.meta.requiresAuth && !store.isAuthenticated) { return { name: 'login', query: { redirect: to.fullPath } } }`. Keep the existing `afterEach` title setter. Add a single-line `// Redirect:` comment above each new branch per CON-003. | ✓ | 2026-08-04 |
| TASK-002 | In `app/Admin/src/app/router/route-meta.ts`, add `guestOnly?: boolean` to the `RouteMeta` interface alongside `requiresAuth?: boolean` (add it as the line immediately after line 5). | ✓ | 2026-08-04 |
| TASK-003 | In `app/Admin/src/features/auth/routes/index.ts`, set `guestOnly: true` on each of the three routes (`login` at line 12, `forgot-password` at line 18, `reset-password` at line 24), keeping their existing `requiresAuth: false`, `title`, and `subtitle` meta. Result: `meta: { title: ..., subtitle: ..., requiresAuth: false, guestOnly: true }`. | ✓ | 2026-08-04 |

### Implementation Phase 2 — Honor the post-login redirect query

- GOAL-002: Create an open-redirect-safe redirect resolver and wire it into the login submit handler.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Create `app/Admin/src/shared/utils/postLoginRedirect.ts` exporting `resolvePostLoginRedirect(redirect: unknown, fallback = '/'): string` per SEC-001: if `typeof redirect === 'string' && redirect.startsWith('/') && !redirect.startsWith('//') && !redirect.startsWith('/\\')` return `redirect`, otherwise return `fallback`. Add `export * from './postLoginRedirect'` to `app/Admin/src/shared/utils/index.ts`. | ✓ | 2026-08-04 |
| TASK-005 | In `app/Admin/src/features/auth/views/LoginPage.vue`, change the success branch of `onSubmit` (currently `router.replace('/')` at line 31) to `router.replace(resolvePostLoginRedirect(route.query.redirect))`. Add `import { useRoute } from 'vue-router'`, `const route = useRoute()`, and `import { resolvePostLoginRedirect } from '@/shared/utils/postLoginRedirect'`. Keep the `// Redirect:` comment per CON-003. | ✓ | 2026-08-04 |

### Implementation Phase 3 — Tests and verification

- GOAL-003: Add a guard unit test suite and the redirect helper test, then run the admin quality gates and finalize the plan.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Create `app/Admin/src/shared/utils/__tests__/postLoginRedirect.spec.ts` covering: a safe internal path (`'/catalog/products'` returns it); `/` logic; an external URL (`'https://evil.com'`) returns `'/'`; `'//evil.com'` returns `'/'`; `'/\\evil.com'` returns `'/'`; a non-string (`undefined`, an array) returns `'/'`; a custom `fallback` is honored. | ✓ | 2026-08-04 |
| TASK-007 | Create `app/Admin/src/app/router/__tests__/guards.spec.ts`. Build an in-memory router with `createRouter({ history: createMemoryHistory(), routes })` (route components may be plain `{ template: '<div />' }` stubs). `vi.mock('@/features/auth/stores/authStore', () => ({ useAuthStore: () => mockStore }))`. Assert: (a) unauthenticated navigation to a `requiresAuth: true` route redirects to `{ name: 'login', query: { redirect: <fullPath> } }`; (b) authenticated navigation to a `requiresAuth: true` route proceeds; (c) authenticated navigation to a `guestOnly: true` route redirects to `{ name: 'dashboard' }`; (d) unauthenticated navigation to a `guestOnly: true` route proceeds. Reset `isInitialized`/store state between cases. | ✓ | 2026-08-04 |
| TASK-008 | Run `cd app/Admin && pnpm run lint` (0 errors), `pnpm run type-check` (0 errors), and `pnpm run test:unit` (all pass, including new specs and the existing 128-file suite). | ✓ | 2026-08-04 |
| TASK-009 | Run identifier-uniqueness checks from the plan template against `plan/feature-admin-router-guards-1.md`; renumber any duplicate declarations until checks (1) and (2) return zero rows. Then set `status: 'Completed'`, `last_updated: 2026-08-04`, and add completion dates to all completed tasks. | ✓ | 2026-08-04 |

## 3. Alternatives

- **ALT-001**: Enable auth by declaring `requiresAuth: true` on every child route individually. Rejected: vue-router merges parent `meta` into child `to.meta` (PAT-002), so redundant per-child flags add noise with no behavioral difference.
- **ALT-002**: Persist the intended destination in `sessionStorage` or a Pinia store instead of a `redirect` query param. Rejected: the existing commented-out guard already used the `redirect` query convention, and a query param is stateless, shareable, and directly testable.
- **ALT-003**: Handle the open-redirect check inline inside `LoginPage.vue` rather than a shared helper. Rejected: a standalone pure function (PAT-002/GUD-002) is trivially unit-tested and reusable by other auth views (e.g. a future login-by-links page) without duplicating the security rule.
- **ALT-004**: Also add a permission/role guard reading `meta.requiredPermission` now. Rejected as out of scope: no route currently declares `requiredPermission`, `hasPermission`/`hasRole` are not used by any route, and the backend enforces authorization as the authoritative boundary. Can be layered later without changing this plan.
- **ALT-005**: Redirect the guest-only guard to `'/'` instead of `{ name: 'dashboard' }`. Rejected: `name: 'dashboard'` is the canonical named home route (declared in `dashboard/routes/index.ts:8`) and avoids depending on a raw path.

## 4. Dependencies

- **DEP-001**: `vue-router` (installed) — `createRouter`, `createMemoryHistory`, `useRoute`, `RouteMeta`, `beforeEach`/`afterEach`.
- **DEP-002**: `pinia` + `useAuthStore` (`app/Admin/src/features/auth/stores/authStore.ts`) exposing `init()`, `isAuthenticated`, `logout()`.
- **DEP-003**: Existing route tree and metadata in `app/Admin/src/app/router/routes.ts` and the per-feature `routes/index.ts` files (root `/` carries `requiresAuth: true`).
- **DEP-004**: `vitest` + `jsdom` (installed) for the new unit tests; `createMemoryHistory` avoids needing a real browser history.
- **DEP-005**: The Admin Code Commenting Standard (`app/Admin/AGENTS.md`) governing the label format and 100-char limit for the comments added in TASK-001 and TASK-005.

## 5. Files

- **FILE-001**: `app/Admin/src/app/router/guards.ts` — re-enable `requiresAuth` redirect; add `guestOnly` redirect.
- **FILE-002**: `app/Admin/src/app/router/route-meta.ts` — add `guestOnly?: boolean` to `RouteMeta`.
- **FILE-003**: `app/Admin/src/features/auth/routes/index.ts` — set `guestOnly: true` on `login`, `forgot-password`, `reset-password`.
- **FILE-004**: `app/Admin/src/shared/utils/postLoginRedirect.ts` — new `resolvePostLoginRedirect` helper; `app/Admin/src/shared/utils/index.ts` — add barrel export.
- **FILE-005**: `app/Admin/src/features/auth/views/LoginPage.vue` — honor `route.query.redirect` via the helper on success.
- **FILE-006**: `app/Admin/src/shared/utils/__tests__/postLoginRedirect.spec.ts` — new helper test.
- **FILE-007**: `app/Admin/src/app/router/__tests__/guards.spec.ts` — new guard test suite.

## 6. Testing

- **TEST-001**: `postLoginRedirect.spec.ts` — safe internal path returns unchanged; `'/'` fallback; `https://…`, `//…`, `/\…` rejected; non-string rejected; custom fallback honored.
- **TEST-002**: `guards.spec.ts` unauthenticated→`requiresAuth` redirect to login with `redirect` query.
- **TEST-003**: `guards.spec.ts` authenticated→`requiresAuth` proceeds.
- **TEST-004**: `guards.spec.ts` authenticated→`guestOnly` redirects to `{ name: 'dashboard' }`.
- **TEST-005**: `guards.spec.ts` unauthenticated→`guestOnly` proceeds.
- **TEST-006**: Verification gate — `cd app/Admin && pnpm run lint && pnpm run type-check && pnpm run test:unit` all green (CON-002).

## 7. Risks & Assumptions

- **RISK-001**: `to.meta.requiresAuth` relies on parent→child `meta` merge (PAT-002, assumed by DEP-003). If a future route declares `requiresAuth: false` under the `/` subtree, children would inherit `false`, but current routes do not do this; verified by TASK-009 review of `routes.ts`.
- **RISK-002**: The `guestOnly` guard redirects any authenticated user off `login`/auth pages; if the login API returns a user without a fully populated `isAuthenticated` computed state, the redirect may not fire. The guard reads only `store.isAuthenticated`, which requires `status === 'authenticated' && user !== null` (authStore.ts :L14).
- **ASSUMPTION-001**: The backend accepts an authenticated user returned by `/auth/login` without redirect to static flows. If the reset-password flow is reached while still authenticated, the guest-only guard legitimately redirects away — acceptable for an admin tool.
- **ASSUMPTION-002**: `store.init()` resolves the session synchronously enough for the `beforeEach` hook (it is `await`ed); `isAuthenticated` is accurate immediately after `init()` completes for a valid stored access token.
- **ASSUMPTION-003**: No existing route currently uses `guestOnly` or an `authGuard` metadata name that would collide with the added flag.

## 8. Related Specifications / Further Reading

- [Admin router guards (current, disabled)](app/Admin/src/app/router/guards.ts)
- [Admin router metadata interface](app/Admin/src/app/router/route-meta.ts)
- [Admin route tree](app/Admin/src/app/router/routes.ts)
- [Auth store (init / isAuthenticated / logout)](app/Admin/src/features/auth/stores/authStore.ts)
- [Auth feature routes](app/Admin/src/features/auth/routes/index.ts)
- [Login view](app/Admin/src/features/auth/views/LoginPage.vue)
- [Vue Router Navigation Guards](https://router.vuejs.org/guide/advanced/navigation-guards.html)
- [Vue Router meta fields](https://router.vuejs.org/guide/advanced/meta.html)
