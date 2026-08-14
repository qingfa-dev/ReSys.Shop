---
goal: Fix the 24 failing unit tests in the Store SPA by correcting mock setup (composable-vs-Pinia store confusion), API mocks that resolve undefined, stale assertions, and cross-test singleton interference — restoring a green test suite.
version: 1.0
date_created: 2026-08-13
last_updated: 2026-08-13
owner: Store SPA team
status: 'Planned'
tags: [test, bug, store, vitest, pinia]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

`npx vitest run` in `app/Store` currently reports **24 failed / 333 passed**. The
failures fall into four root-cause classes discovered by investigation:

1. **Composable-vs-Pinia-store confusion (13 tests).** View/component specs
   treat plain `reactive` module-singleton composables (`useProfile`,
   `useAddresses`, `useWishlists`, `useOrders`) as if they were Pinia stores:
   they call `vi.mocked(store.action).mockResolvedValue(...)` / 
   `.toHaveBeenCalledWith(...)` on methods that are **not spies** because
   `createTestingPinia({ stubActions: true })` only stubs Pinia actions. Result:
   `TypeError: [AsyncFunction x] is not a spy or a call to a spy!` and
   `vi.mocked(...).mockResolvedValue is not a function`.
2. **API mocks resolve `undefined` (5 tests).** Ordering specs mock
   `OrderApi.getOrders`/`getOrder`/`getOrderTracking` as `vi.fn()` with no
   resolved value; the composable reads `result.isSuccess` on `undefined` and
   throws. Recent try/catch hardening now surfaces this as an error state
   instead of a silent unhandled rejection.
3. **Stale assertions (5 tests).** Component/contract changed but the test was
   not updated: `AppHeader` label `Shop` vs test `Catalog`, `AuthLayout` slot
   vs `RouterView`, `CartDrawer` asserts a ProgressBar that never existed,
   `refresh.spec` URL slash mismatch, `ShopView` layout-toggle keeps a stale DOM
   reference across a `v-if` swap.
4. **Cross-file singleton interference (1 observed in full runs).** Module
   singleton state leaks across test files, causing intermittent timeouts and a
   spurious `Catalog button` failure. Tests that pass in isolation but fail in
   the full run need state reset.

This plan fixes all four classes so `npx vitest run` is green (or green except
clearly documented pre-existing infrastructure issues).

## 1. Requirements & Constraints

- **REQ-001**: `npx vitest run` in `app/Store` must pass with zero failures after this plan (or a documented, minimal residual list with reasons).
- **REQ-002**: Fix the 13 composable-not-spy tests by mocking the composable **module** with `vi.mock('<composable-path>', () => ({ useX: vi.fn(() => ({ ...spy stubs... })) }))` — do NOT rely on `createTestingPinia` for non-Pinia composables.
- **REQ-003**: Fix API mocks that resolve `undefined` by making them return real `Result` envelopes (`ok(...)`, `pagedOk(...)` from `@/shared/types/result`) so composables branch on `isSuccess` correctly.
- **REQ-004**: Fix stale assertions to match current component/contract behavior (`Shop`, `RouterView`, no-ProgressBar, `/api/...` URL, re-queried grid element).
- **REQ-005**: Add per-file `beforeEach` singleton resets where composable state leaks across tests (call the composable's `reset()` if present; else re-seed all refs).
- **REQ-006**: Do NOT change production code to satisfy tests; fix the tests to match real behavior.
- **CON-001**: Tests must keep asserting real behavior (black-box), not implementation details of mocks.
- **CON-002**: `vi.clearAllMocks()` must remain in each `beforeEach`; do not disable mock clearing.
- **CON-003**: Comments follow the Store AGENTS.md standard.
- **CON-004**: Warnings-as-errors; `pnpm run build-only` and `pnpm run lint` must still pass.
- **GUD-001**: Prefer mocking the composable module (single `vi.mock` at top of spec) over per-test `vi.mocked(...)` on real singletons.
- **GUD-002**: Where a composable already exposes a `reset()` (e.g. `useCart.reset`, `useOrders` has none, `useWishlists.reset`, `useProfile.reset`), call it in `beforeEach` to isolate tests.
- **GUD-003**: Do not touch `useCheckout`, `useVisualSearch`, or `useProductDetail` tests — they are already green.
- **PAT-001**: Follow the working mock pattern already used by `OrderListView.spec.ts`'s API-mock (`vi.mock('../../services/orderApi', ...)`) but correct it to return real Results.
- **PAT-002**: For composable-module mocks, mirror the shape the composable returns (`reactive` object with refs unwrapped for template, but tests access `.value` via the returned reactive).

## 2. Implementation Steps

### Implementation Phase 1: Fix composable-not-spy specs (13 tests)

- GOAL-001: Make the profile/wishlist/orders/checkout view specs mock their composables as modules so actions are real spies.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | `app/Store/src/features/profile/views/__tests__/ProfileView.spec.ts` — add `vi.mock('../../composables/useProfile', ...)` returning `useProfile: vi.fn(() => ({ profile: ref(...), updateProfile: vi.fn().mockResolvedValue(true), ... }))`. Update `seedProfile` and the `updateProfile` assertion to use the mocked spy. Keep the two render tests passing. | |  |
| TASK-002 | `app/Store/src/features/profile/views/__tests__/AddressBookView.spec.ts` — add `vi.mock('../../composables/useAddresses', ...)` returning a reactive with spy actions (`createAddress`, `updateAddress`, `deleteAddress`, `fetchAddresses`) seeded from the existing fixtures. Fix the 6 failing tests (`renders address rows`, `empty message`, `opens add dialog`, `opens edit dialog`, `deletes via confirm`, `sets default`). | |  |
| TASK-003 | `app/Store/src/features/profile/views/__tests__/WishlistsView.spec.ts` — add `vi.mock('../../composables/useWishlists', ...)` with spy actions (`createWishlist`, `removeItem`, `addItem`, `fetchWishlists`) and seeded `lists`/`details`. Fix the 3 failing tests (`empty state`, `renders active list items and removes`, `creates new list`). | |  |
| TASK-004 | `app/Store/src/features/catalog/components/__tests__/ProductGridCard.spec.ts` — add `vi.mock('@/features/profile/composables/useWishlists', ...)` with `wishlistedVariantIds` seeded to include the fixture variant and `addItem`/`removeItem` as spies. Fix the 2 failing wishlist tests. | |  |
| TASK-005 | `app/Store/src/features/ordering/views/__tests__/CheckoutView.spec.ts` — add `vi.mock('@/features/profile/composables/useAddresses', ...)` so `addresses.createAddress` is a spy; fix the `maps a cascade country/state selection` test. | |  |

### Implementation Phase 2: Fix undefined-API-mock ordering specs (5 tests)

- GOAL-002: Make ordering API mocks return real `Result` envelopes so composables branch correctly.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | `app/Store/src/features/ordering/views/__tests__/OrderListView.spec.ts` — change `vi.mock('../../services/orderApi', ...)` stubs so `getOrders` resolves `pagedOk(items, 1, 20, totalCount)` (or the mocked `OrderApi.getOrders.mockResolvedValue` returns a proper `PagedResult`). Fix the 3 failing tests (`renders order rows`, `navigates to detail`, `empty state`) while keeping the `Retry`/error test working (that one seeds `store.error` directly and expects `getOrders` called). | |  |
| TASK-007 | `app/Store/src/features/ordering/views/__tests__/OrderDetailView.spec.ts` — make `OrderApi.getOrder`/`getOrderTracking` mocks resolve real `ok(...)` results. Fix the 2 failing tests (`renders with Estimated delivery`, `shows error state with retry`). Verify the `Estimated delivery` text is produced by `OrderDetailView.vue:188` when tracking data is present. | |  |

### Implementation Phase 3: Fix stale assertions (5 tests)

- GOAL-003: Align stale tests with current component behavior.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | `app/Store/src/app/components/layout/__tests__/AppHeader.spec.ts` — the `Catalog` test expects label `Catalog`; `AppHeader.vue:28` uses label `Shop`. Update the assertion to `toContain('Shop')` (and the test name to `renders the Shop button linking to /shop`). | |  |
| TASK-009 | `app/Store/src/app/layouts/__tests__/layouts.spec.ts` — the `AuthLayout` `renders the slotted form content inside the Card` test mounts with a `default` slot, but `AuthLayout` now renders `<RouterView />`. Rewrite the test to push a real route (`/login`) and assert the routed content renders (or delete the slot-based assertion if it duplicates the existing `links to register` tests). Keep the `AccountLayout` tests untouched. | |  |
| TASK-010 | `app/Store/src/features/ordering/components/__tests__/CartDrawer.spec.ts` — the `renders line items with subtotal and free-shipping progress` test asserts `[data-pc-name="progressbar"]` exists, but the component renders no ProgressBar. Remove that assertion (or replace with the actual subtotal assertion) and keep the rest of the test. | |  |
| TASK-011 | `app/Store/src/shared/api/interceptors/__tests__/refresh.spec.ts` — the `rotates both tokens` test asserts the refresh URL as `api/storefront/identity/auth/sessions/refresh` (no leading slash); the interceptor sends `/api/storefront/...`. Update the expected argument to include the leading slash. | |  |
| TASK-012 | `app/Store/src/features/catalog/views/__tests__/ShopView.spec.ts` — the `switches the grid to single-column list mode` test captures `grid` once and asserts it changed, but the template swaps the grid DOM via `v-if`/`v-else`. Re-query `grid` with `wrapper.find('a.group.block').element.parentElement` AFTER clicking the layout button, then assert the new node has `grid-cols-1` and not `grid-cols-2`. | |  |

### Implementation Phase 4: Isolate cross-file singleton state

- GOAL-004: Prevent module-singleton composable state from leaking between test files and causing full-run timeouts.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | Audit specs that seed singletons (`ShopView`, `HomeView`, `ProductGridCard`, `AppHeader`, `CartDrawer`, ordering/profile views). In each `beforeEach`, call the composable's `reset()` where it exists (`useCart.reset`, `useWishlists.reset`, `useProfile.reset`) or re-seed every ref back to a clean initial value (e.g. `useProducts`: `items.value = []`, `loading.value = false`, `isInitialLoad.value = true`, `page.value = 1`). | |  |
| TASK-014 | Add a shared test helper (e.g. `app/Store/src/test/resetComposables.ts`) exporting a `resetStores()` that resets `useCart`, `useWishlists`, `useProfile`, `useProducts`, `useOrders`, `useSearch`, `useTaxonomy` to pristine state, and call it in the `beforeEach` of the specs that mount those consumers. | |  |

### Implementation Phase 5: Verification

- GOAL-005: Prove the suite is green with the full command set.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Run `npx vitest run` in `app/Store` — report the full pass/fail count. All 24 known failures must be resolved (residual failures must each be documented with a reason). | |  |
| TASK-016 | Run `pnpm run build-only` in `app/Store` — passes with zero warnings. | |  |
| TASK-017 | Run `pnpm run lint` in `app/Store` — zero errors in the changed spec files. | |  |
| TASK-018 | Run `npx vitest run` three times consecutively to confirm no flaky full-run interference remains. | |  |

## 3. Alternatives

- **ALT-001**: Convert `useProfile`/`useAddresses`/`useWishlists`/`useOrders` from plain composables to real Pinia stores so `createTestingPinia` stubs them. Rejected: large production refactor to satisfy tests (violates REQ-006 "don't change production for tests"), and it changes app architecture beyond the bug.
- **ALT-002**: In the specs, `vi.mocked(store.action)` after manually assigning `store.action = vi.fn()` on the singleton. Rejected: mutates the shared singleton and leaks state across files; the module-level `vi.mock` is cleaner and matches Vitest's intended pattern.
- **ALT-003**: Delete the failing tests. Rejected: they assert real behavior and are worth fixing; the composable-not-spy ones catch genuine regression risk.
- **ALT-004**: Only fix the assertion text and ignore the undefined-mock ordering tests. Rejected: those tests reveal a real composable bug class (reading `.isSuccess` on `undefined`) that the try/catch hardening exposed; making mocks return real Results is the correct fix.

## 4. Dependencies

- **DEP-001**: `vitest` + `@vue/test-utils` — `vi.mock`, `mount`, `flushPromises`.
- **DEP-002**: `@pinia/testing` `createTestingPinia` — used for the actual Pinia stores (auth, and the router/global wiring); NOT for composables.
- **DEP-003**: `@/shared/types/result` — `ok`, `pagedOk`, `pagedFailure` for realistic API mocks.
- **DEP-004**: The module-singleton composables (`useProfile`, `useAddresses`, `useWishlists`, `useOrders`, `useCart`, `useProducts`, `useSearch`, `useTaxonomy`) — their `reset()` methods and exported state.
- **DEP-005**: PrimeVue test deps (`primevue/config`, `primevue/toastservice`) already used in the specs.

## 5. Files

- **FILE-001**: `app/Store/src/features/profile/views/__tests__/ProfileView.spec.ts` — composable module mock.
- **FILE-002**: `app/Store/src/features/profile/views/__tests__/AddressBookView.spec.ts` — composable module mock.
- **FILE-003**: `app/Store/src/features/profile/views/__tests__/WishlistsView.spec.ts` — composable module mock.
- **FILE-004**: `app/Store/src/features/catalog/components/__tests__/ProductGridCard.spec.ts` — wishlist composable module mock.
- **FILE-005**: `app/Store/src/features/ordering/views/__tests__/CheckoutView.spec.ts` — addresses composable module mock.
- **FILE-006**: `app/Store/src/features/ordering/views/__tests__/OrderListView.spec.ts` — real `Result` API mocks.
- **FILE-007**: `app/Store/src/features/ordering/views/__tests__/OrderDetailView.spec.ts` — real `Result` API mocks.
- **FILE-008**: `app/Store/src/app/components/layout/__tests__/AppHeader.spec.ts` — `Catalog`→`Shop` assertion.
- **FILE-009**: `app/Store/src/app/layouts/__tests__/layouts.spec.ts` — AuthLayout RouterView-based test.
- **FILE-010**: `app/Store/src/features/ordering/components/__tests__/CartDrawer.spec.ts` — remove phantom ProgressBar assertion.
- **FILE-011**: `app/Store/src/shared/api/interceptors/__tests__/refresh.spec.ts` — leading-slash URL.
- **FILE-012**: `app/Store/src/features/catalog/views/__tests__/ShopView.spec.ts` — re-query grid after toggle.
- **FILE-013**: `app/Store/src/test/resetComposables.ts` (new) — shared composable-state reset helper.
- **FILE-014**: The affected view/component specs wired to call the shared reset helper in `beforeEach` (ShopView, HomeView, ProductGridCard, AppHeader, CartDrawer, OrderListView, OrderDetailView, ProfileView, AddressBookView, WishlistsView).

## 6. Testing

- **TEST-001**: `npx vitest run` full suite green (or documented residual).
- **TEST-002**: Per-file runs green for each edited spec.
- **TEST-003**: Three consecutive full-suite runs with no flaky interference.
- **TEST-004**: `pnpm run build-only` zero warnings.
- **TEST-005**: `pnpm run lint` zero errors in changed spec files.

## 7. Risks & Assumptions

- **RISK-001**: Composable-module mocks must mirror the `reactive` return shape exactly (refs vs unwrapped values in template, `.value` access in tests). Mitigated by reading each composable's return object before mocking.
- **RISK-002**: The undefined-API-mock ordering tests may mask a real composable contract gap (reading `.isSuccess` on a malformed result). The plan fixes the mocks to real Results; if a composable still throws on malformed results, that is a separate production hardening task (documented, not in scope).
- **RISK-003**: Resetting singletons in `beforeEach` could hide legitimate shared-state behavior; each reset mirrors what `reset()` already does on logout, so it matches production semantics.
- **RISK-004**: `ShopView` grid re-query fix depends on the exact template structure; verify the selector `a.group.block` still matches after the toggle.
- **ASSUMPTION-001**: The 24 failures are fully accounted for by the four classes above (verified by investigation: 13 composable-spy + 5 undefined-mock + 5 stale-assertion + 1 cross-file interference = 24).
- **ASSUMPTION-002**: No production code change is required; all fixes are test-side.

## 8. Related Specifications / Further Reading

- [Store SPA AGENTS.md — comment standard](app/Store/AGENTS.md)
- [Vitest module mocking](https://vitest.dev/guide/mocking)
- [Pinia testing utilities](https://pinia.vuejs.org/cookbook/testing.html)
- [Result envelope helpers](app/Store/src/shared/types/result.ts)
