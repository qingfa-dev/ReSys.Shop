---
goal: Keep Admin list services on raw QueryingParameters while retrofitting structured direct call sites through the existing typed query converters (toXxxQueryParams)
version: 1.0
date_created: 2026-08-12
owner: Frontend Platform Team
status: 'Planned'
tags: refactor, admin, querying, converters, typescript
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The Admin SPA (`app/Admin`) moved every list service onto a single canonical
request model: `usePagedQuery` holds state in `QueryingParameters` (filter DSL,
sort array, search, page, page size) and drives fetch through a
`PagedFetcher<T> = (params: QueryingParameters, options?) => Promise<PagedResult<T>>`.
Consequently the `{Entity}Api` list methods now accept raw
`params: QueryingParameters` and pass them straight through to `getPaged`
(see `plan/refactor-admin-api-services-1.md`, steps 2 & 5-7 of that plan).

As a side effect, the typed-convenience layer — the per-feature `XxxQuery`
interfaces and `toXxxQueryParams(...)` builders that convert a structured query
into a `QueryingParameters` object — is no longer called from any service. It
still exists, is exported from `features/<module>/types/*.ts` and the feature
barrels, and is covered by `__tests__/types/*.spec.ts`, but it is dormant.

This plan adopts the **hybrid** approach:

1. Keep every list service on `params: QueryingParameters` — no service edits
   and no changes to the `usePagedQuery` fetcher integration.
2. Keep the `XxxQuery` interfaces and `toXxxQueryParams` converters as a
   supported, tested typed-input layer.
3. Retrofit the two direct (non-composable) call sites that pass a *structured*
   filter field (`orderId`, `isActive`) so they build parameters through the
   matching converter, putting the converter back into active use where it adds
   value.
4. Leave pagination-only call sites (e.g. `getRoles({ pageSize: 100 })`) inline;
   wrapping them in a converter would only add noise.

Only the Admin SPA changes. The Store SPA, the `XxxQuery`/converter
implementations, their unit specs, and the .NET backend are untouched.

## 1. Requirements & Constraints

- **REQ-001**: Keep every list service method signature `(params: QueryingParameters)` unchanged. No `{Entity}Api` service `.ts` file is edited by this plan.
- **REQ-002**: Keep every `XxxQuery` interface, `toXxxQueryParams(...)` converter, and its export from `features/<module>/types/*.ts` and the feature `index.ts` barrels intact.
- **REQ-003**: Retrofit exactly the direct call sites that pass a structured filter field to their matching converter (see GOAL-001 and GOAL-002) — `OrderDetail.vue` payment fetch and the `StatesList.vue`/`StateDetail.vue` active-country select.
- **REQ-004**: Do not wrap pagination-only call sites (`ShippingRateDetail.vue:66`, `UserDetail.vue:75`, `useProfileDetail.ts:22`, `useActiveStockLocations.ts:8`) in converters; leave them as inline `QueryingParameters` literals.
- **REQ-005**: After each phase, `cd app/Admin && pnpm run lint && pnpm run type-check && pnpm run test:unit` must pass with zero changes (warnings-as-errors not applied to the SPA, but no new lint errors).
- **SEC-001**: No secrets, credentials, or environment values are introduced. Converter output is static query DSL over already-known filter fields.
- **CON-001**: Scope is the Admin SPA (`app/Admin/src`) only. Store SPA, converter implementations, converter specs, services, and backend C# are not modified.
- **CON-002**: The runtime DSL emitted by a converter must match the current inline `QueryingParameters` exactly for the fields present (`filter: 'orderId=<id>'` and `filter: 'isActive=true'`).
- **GUD-001**: Preserve all existing view comments; the retrofit only changes the affected `{Entity}Api.…` call expression and its imports, never surrounding comments (`app/Admin/AGENTS.md` Code Commenting Standard applies throughout).
- **GUD-002**: Import converters from the same module a view already imports types/services from — either the feature barrel (`@/features/<module>`) or the relative `../types/<file>` path — and keep import order/style consistent with the file.
- **PAT-001**: Structured direct call — `XxxApi.getXxx(toXxxQueryParams({ ... }))` — used only where the query carries at least one structured filter field (`orderId`, `isActive`).

## 2. Implementation Steps

### Implementation Phase 1 — Ordering: OrderDetail payment fetch via toPaymentQueryParams

- GOAL-001: Retrofit `OrderDetail.vue`'s lazy payments fetch to build its `QueryingParameters` through `toPaymentQueryParams`, exercising the payment feature's typed converter in real call-site use.

Canonical change — `app/Admin/src/features/ordering/views/OrderDetail.vue`:

```ts
// before (line 187)
const result = await PaymentApi.getPayments({ filter: 'orderId=' + orderId.value, pageSize: 100 })

// after
const result = await PaymentApi.getPayments(toPaymentQueryParams({ orderId: orderId.value, pageSize: 100 }))
```

`toPaymentQueryParams({ orderId, pageSize: 100 })` emits
`{ filter: 'orderId=<id>', search: null, searchFields: PAYMENT_SEARCH_FIELDS, sort: null, pageNumber: null, pageSize: 100 }`,
which is behaviorally identical for `getPaged` to the current literal (extra
`null`s and the identical `PAYMENT_SEARCH_FIELDS` array are already handled).

Add the value import next to the existing type import (line 25):
`import { toPaymentQueryParams } from '@/features/payment/types/payment'`
(minimise import churn; the feature barrel also exports it if preferred).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Edit `app/Admin/src/features/ordering/views/OrderDetail.vue`: add `toPaymentQueryParams` to the existing `@/features/payment/types/payment` import (line 25) and replace the `getPayments({ filter: 'orderId=' + orderId.value, ... })` call (line 187) with `getPayments(toPaymentQueryParams({ orderId: orderId.value, pageSize: 100 }))`. | | |
| TASK-002 | Verify Phase 1: `cd app/Admin && pnpm run lint && pnpm run type-check && pnpm run test:unit`. | | |

### Implementation Phase 2 — Location: active-country selects via toCountryQueryParams

- GOAL-002: Retrofit the shared active-countries fetcher used by `StatesList.vue` and `StateDetail.vue` to build its `QueryingParameters` through `toCountryQueryParams`, exercising the location feature's typed converter at two real call sites.

Canonical change — `StatesList.vue` (line 24) and `StateDetail.vue` (line 22):

```ts
// before
useActiveList<CountryListItem>(() => CountryApi.getCountries({ filter: 'isActive=true' }))

// after
useActiveList<CountryListItem>(() => CountryApi.getCountries(toCountryQueryParams({ isActive: true })))
```

`toCountryQueryParams({ isActive: true })` emits
`{ filter: 'isActive=true', search: null, sort: null, pageNumber: null, pageSize: null }`,
matching the current literal for the fields present.

Add the value import to both files (near the existing `../types/country` type import, StatesList.vue line 18 / StateDetail.vue line 14):
`import { toCountryQueryParams } from '../types/country'`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Edit `app/Admin/src/features/location/views/StatesList.vue`: add `toCountryQueryParams` to the `../types/country` import (line 18) and replace `getCountries({ filter: 'isActive=true' })` (line 24) with `getCountries(toCountryQueryParams({ isActive: true }))`. | | |
| TASK-004 | Edit `app/Admin/src/features/location/views/StateDetail.vue`: add `toCountryQueryParams` to the `../types/country` import (line 14) and replace `getCountries({ filter: 'isActive=true' })` (line 22) with `getCountries(toCountryQueryParams({ isActive: true }))`. | | |
| TASK-005 | Verify Phase 2: `cd app/Admin && pnpm run lint && pnpm run type-check && pnpm run test:unit`. | | |

### Implementation Phase 3 — Final verification & drift gates

- GOAL-003: Confirm the retrofit is isolated, the pagination-only call sites are untouched, and the converters remain covered by their existing specs with a green full Admin verification.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Confirm no service `.ts` file changed and only `OrderDetail.vue`, `StatesList.vue`, `StateDetail.vue` are modified: `git status --short app/Admin/src` and `git diff --stat` show exactly these three files. | | |
| TASK-007 | Confirm `toXxxQueryParams` is still exported and unused converters remain intact: `grep -rn "toPaymentQueryParams\|toCountryQueryParams" app/Admin/src/features --include=*.ts --include=*.vue` still resolves; converter specs `features/payment/__tests__/types/`, `features/location/__tests__/types/` (if present) and `shared/types/querying/__tests__/casing-regression.spec.ts` are untouched. | | |
| TASK-008 | Run the full Admin gate: `cd app/Admin && pnpm run lint && pnpm run type-check && pnpm run test:unit && pnpm run build`. | | |

## 3. Alternatives

- **ALT-001**: Revert every list service to accept the typed `XxxQuery` and convert internally via `toXxxQueryParams`. Rejected: the `usePagedQuery` composable holds `QueryingParameters` state, so a typed-signature service would force either a lossy reverse mapper (`QueryingParameters` → `XxxQuery`) or a return to URL-string `usePagedQuery` — both undo the fetcher integration the prior migration established.
- **ALT-002**: Delete the dormant converters, `XxxQuery` interfaces, and their `__tests__/types/*.spec.ts` as dead code. Rejected for this plan: the typed layer is a useful, tested input API; this plan instead puts it back into active use at structured call sites. A separate cleanup plan can remove any converter left truly unused afterwards.
- **ALT-003**: Retrofit every direct list call site through a converter, including pagination-only ones (`{ pageSize: 100 }` in `ShippingRateDetail.vue`, `UserDetail.vue`, `useProfileDetail.ts`, `useActiveStockLocations.ts`). Rejected: a converter emits the full `QueryingParameters` shape (null fields, search fields), adding noise without structural benefit when the only input is `pageSize`.
- **ALT-004**: Route the same-page list fetch in `OrderDetail.vue` through a `usePagedQuery` fetcher instead of a one-off converter call. Rejected: the function already inlines a fixed `{ orderId, pageSize: 100 }` read, has its own `paymentsLoaded`/`paymentsLoading` guards, and is not part of a paged interactive UI; introducing `usePagedQuery` would over-engineer the fetch.

## 4. Dependencies

- **DEP-001**: The prior plan `plan/refactor-admin-api-services-1.md` is complete — services now accept `QueryingParameters` — which is the precondition making these converter-based call sites both valid and self-contained.
- **DEP-002**: Phase 3 runs after Phases 1 and 2 and consumes their edited files.
- **DEP-003**: `getPaged`, `PagedRequestOptions`, and the `QueryingParameters` machinery in `app/Admin/src/shared/api` and `app/Admin/src/shared/types/querying` are unchanged and are the runtime foundation of both the fetcher form and the converter output.
- **DEP-004**: The converter unit specs (`features/payment/__tests__/types/*.spec.ts`, existing `features/*/__tests__/types/*.spec.ts`, and `shared/types/querying/__tests__/casing-regression.spec.ts`) remain green; they are the contract guaranteeing the emitted DSL.

## 5. Files

- **FILE-001**: `app/Admin/src/features/ordering/views/OrderDetail.vue` — add `toPaymentQueryParams` import; retrofit the payments fetch (line 187).
- **FILE-002**: `app/Admin/src/features/location/views/StatesList.vue` — add `toCountryQueryParams` import; retrofit the active-countries fetcher (line 24).
- **FILE-003**: `app/Admin/src/features/location/views/StateDetail.vue` — add `toCountryQueryParams` import; retrofit the active-countries fetcher (line 22).
- **FILE-004**: Reference only (unchanged): `features/payment/types/payment.ts` (`toPaymentQueryParams`/`PAYMENT_SEARCH_FIELDS`), `features/location/types/country.ts` (`toCountryQueryParams`), and their feature barrels.

## 6. Testing

- **TEST-001**: `pnpm run lint` in `app/Admin` (per Phase 1, 2, 3) — catches unused imports after retrofit and comment-standard violations.
- **TEST-002**: `pnpm run type-check` in `app/Admin` (vue-tsc, per Phase 1, 2, 3) — verifies the converter signatures satisfy the `useActiveList` fetcher and `getPaged` call shapes.
- **TEST-003**: `pnpm run test:unit` in `app/Admin` (per Phase 1, 2, 3) — existing converter specs (payment, country/casing-regression) assert the DSL the retrofit now emits; no new specs required since behavior is unchanged.
- **TEST-004**: Drift checks in Phase 3 (TASK-006/007) — `git status --short app/Admin/src` shows exactly the three views; targeted greps confirm the converters and their specs are intact.

## 7. Risks & Assumptions

- **RISK-001**: A converter's full output (null `search`/`sort`/`pageNumber`, populated `searchFields`) could theoretically interact with `getPaged` defaults differently than the current minimal literal. Mitigation: `getPaged` already merges `QueryingParameters` with defaults (identical to every `usePagedQuery` path); `TEST-003` plus the existing converter specs lock the output; Phase 1/2 gates catch behavioral drift.
- **RISK-002**: `toCountryQueryParams({ isActive: true })` relies on the `isActive` branch (country.ts line 65) existing; if that branch were deleted the retrofit would silently drop the filter. Mitigation: converter specs assert the `isActive` output; TASK-005 verifies.
- **ASSUMPTION-001**: The three retrofitted call sites are the only direct (non-composable) ones passing a structured filter field today, matching the `OrderDetail.vue`/`StatesList.vue`/`StateDetail.vue` grep results at plan time.
- **ASSUMPTION-002**: Converter runtime DSL output equals the current inline literals for the fields present, so behavior, pagination, and search semantics are unchanged.

## 8. Related Specifications / Further Reading

- `plan/refactor-admin-api-services-1.md` — the completed migration that put list services on `QueryingParameters`
- `app/Admin/AGENTS.md` — Admin SPA conventions and Code Commenting Standard for view edits
- `app/Admin/src/shared/types/querying/querying.ts` — `QueryingParameters` shape
- `app/Admin/src/shared/composables/usePagedQuery.ts` — `PagedFetcher`/`QueryingParameters` fetcher mode
- `app/Admin/src/features/payment/types/payment.ts` and `app/Admin/src/features/location/types/country.ts` — converter implementations referenced by the retrofit