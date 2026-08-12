---
goal: Refactor Admin SPA API services to inline full-path endpoint URLs and route every composable/view API call through the {Entity}Api classes
version: 1.0
date_created: 2026-08-11
owner: Frontend Platform Team
status: 'Planned'
tags: refactor, admin, api-services, composables, views, typescript
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The Admin SPA (`app/Admin`) scatters API endpoint knowledge across three places:
`private static readonly BASE = 'api/admin/...'` class constants in the
`{Entity}Api` service classes, inline relative URL strings passed straight into
`usePagedQuery(...)` inside thin `use*List` composables, and inline `usePagedQuery(...)`
calls inside views (some with broken paths such as `api/locations/countries`).

This plan makes the `{Entity}Api` services the single source of truth for endpoint
paths and moves all callers onto them, in gradual per-module phases:

1. Remove every `BASE` constant and inline the full `/api/admin/...` path in each
   service method (rationale: easy to track and rename endpoints).
2. Switch service list methods to accept raw `QueryingParameters` (removing the
   `toXxxQueryParams` indirection so `usePagedQuery` state can flow through unchanged).
3. Delete the 26 thin composables that only wrap `usePagedQuery`/`useActiveList`
   around a single service method and call the Api services directly from the views.
4. Verify lint, type-check, and unit tests after every module phase.

Only the Admin SPA changes. The Store SPA and the .NET backend are untouched.

## 1. Requirements & Constraints

- **REQ-001**: Remove every `private static readonly BASE = '...'` and module-level `const BASE = '...'` from the 33 Admin service files and inline the full absolute-path URL literal (`/api/admin/...`, always starting with `/`) at every call site in that file.
- **REQ-002**: No inline API URL literal may remain in any Admin composable or view. Every request must route through a `{Entity}Api` method.
- **REQ-003**: Every list-style service method accepts `QueryingParameters` (raw params) instead of a typed `XxxQuery`, removing the `toXxxQueryParams(...)` mapping call and its import.
- **REQ-004**: Every composable that only wraps a single service call in `usePagedQuery` or `useActiveList` is deleted; the consuming views call the Api service directly through the fetcher form.
- **REQ-005**: Each module phase must pass `pnpm run lint`, `pnpm run type-check`, and `pnpm run test:unit` inside `app/Admin` before the next phase starts.
- **SEC-001**: No secrets, credentials, or environment values are introduced. All URL literals are static, publicly-known API paths.
- **CON-001**: Scope is the Admin SPA (`app/Admin/src`) only. Store SPA (`app/Store/src`) and backend C# routes are not modified.
- **CON-002**: Backend route prefixes (`/api/admin/...`) must not change; this is a frontend refactor only.
- **CON-003**: Gradual rollout — exactly one module (plus the foundation phase) per commit/change set; modules are never combined.
- **CON-004**: Every API URL string literal must start with `/` (absolute-path form), matching the Vite `/api` proxy rule and the Store SPA convention.
- **GUD-001**: Preserve each caller's existing option passthrough (the `...options` spread and current `default*` options) when inlining fetchers.
- **GUD-002**: Do not add or remove comments beyond what is required by the existing per-file convention; the Code Commenting Standard in `app/Admin/AGENTS.md` applies to views.
- **GUD-003**: The service method's hardcoded `allowedFilterFields` / `allowedSortFields` / `allowedSearchFields` become the single source of truth. When a view currently passes `allowed*` lists that the service does not set, those lists are promoted verbatim into the service method so runtime behavior is preserved exactly (e.g. `CountriesList` passes `allowedSearchFields: COUNTRY_FILTER_FIELDS` today; `CountryApi.getCountries` must set the same list).
- **PAT-001**: Plain list fetcher — `usePagedQuery<T>((params) => XxxApi.getXxx(params), { ...options })`.
- **PAT-002**: Active-options fetcher — `useActiveList<T>(() => XxxApi.getXxx({ ...fixedParams }))`.
- **PAT-003**: Scoped fetcher (entity-scope ref/arg) — `usePagedQuery<T>((params) => scope.value ? XxxApi.getList(scope.value, params) : XxxApi.getXxx(params), { ...options })`.

## 2. Implementation Steps

### Implementation Phase 1 — Foundation: `usePagedQuery` fetcher support

- GOAL-001: Extend `usePagedQuery` to accept a `PagedFetcher<T>` so callers can drive it with an `{Entity}Api` service method instead of a URL string, and add unit tests for the fetcher mode.

Canonical change — `app/Admin/src/shared/composables/usePagedQuery.ts`:

```ts
// before (current first parameter)
export function usePagedQuery<T>(url: string | (() => string), options?: UsePagedQueryOptions): PagedQueryState<T>
```

```ts
// after
export type PagedFetcher<T> = (
  params: QueryingParameters,
  options?: PagedRequestOptions,
) => Promise<PagedResult<T>>

export function usePagedQuery<T>(
  source: string | (() => string) | PagedFetcher<T>,
  options?: UsePagedQueryOptions,
): PagedQueryState<T>
```

Inside `fetch()`: build `params` and `opts` exactly as today. When `typeof source === 'function'`, treat a function with arity `>= 1` as a fetcher and `await source(params, opts)`; otherwise resolve the URL (`source` or `source()`) and keep calling `getPaged<T>(resolvedUrl, params, opts)`. Import `QueryingParameters` from `@/shared/types/querying` and `PagedRequestOptions` from `@/shared/api`. Export `PagedFetcher` from `app/Admin/src/shared/composables/index.ts`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Edit `app/Admin/src/shared/composables/usePagedQuery.ts`: add `PagedFetcher<T>`, extend the first parameter to `string \| (() => string) \| PagedFetcher<T>`, branch in `fetch()` on function arity (`source.length >= 1` ⇒ fetcher; else URL-fn). URL-mode behavior must be unchanged. | Yes | 2026-08-11 |
| TASK-002 | Add fetcher-mode cases to `app/Admin/src/shared/composables/__tests__/usePagedQuery.spec.ts`: (a) fetcher is invoked with assembled `QueryingParameters`; (b) `result.isSuccess` updates `items`; (c) `setPage`/`setFilter` trigger a new fetcher call with updated params. Keep all existing URL-mode tests green. | Yes | 2026-08-11 |
| TASK-003 | Update `app/Admin/src/shared/composables/index.ts` to export the new `PagedFetcher` type. | Yes | 2026-08-11 |
| TASK-004 | Verify Phase 1: `cd app/Admin && pnpm run lint && pnpm run type-check && pnpm run test:unit`. | Yes | 2026-08-11 |

### Implementation Phase 2 — Location + Dashboard modules

- GOAL-002: Convert the Location and Dashboard service classes to inline full paths, switch list methods to `QueryingParameters`, delete the thin `useActiveCountries` composable, and migrate `CountriesList`, `StatesList`, and `StateDetail` views onto the Api services (fixing the broken `api/locations/...` paths).

Service conversion rule (applies to every service in this and later phases):

```ts
// before — app/Admin/src/features/location/services/countryApi.ts
export class CountryApi {
  private static readonly BASE = 'api/admin/location/countries'
  static getCountries(query: CountryQuery): Promise<PagedResult<CountryListItem>> {
    return getPaged<CountryListItem>(CountryApi.BASE, toCountryQueryParams(query), {
      allowedFilterFields: COUNTRY_FILTER_FIELDS,
      allowedSortFields: COUNTRY_SORT_FIELDS,
    })
  }
  static getCountry(id: string): Promise<Result<CountryDetail>> {
    return get<Result<CountryDetail>>(`${CountryApi.BASE}/${id}`)
  }
}
```

```ts
// after
export class CountryApi {
  static getCountries(params: QueryingParameters): Promise<PagedResult<CountryListItem>> {
    return getPaged<CountryListItem>('/api/admin/location/countries', params, {
      allowedFilterFields: COUNTRY_FILTER_FIELDS,
      allowedSortFields: COUNTRY_SORT_FIELDS,
      allowedSearchFields: COUNTRY_FILTER_FIELDS,
    })
  }
  static getCountry(id: string): Promise<Result<CountryDetail>> {
    return get<Result<CountryDetail>>(`/api/admin/location/countries/${id}`)
  }
}
```

Import `QueryingParameters` from `@/shared/types/querying` (or via `@/shared/types`, which re-exports it). Remove the now-unused `toXxxQueryParams` and `XxxQuery` imports. Template literals like `` `${CountryApi.BASE}/${id}` `` become `` `/api/admin/location/countries/${id}` ``.

Spec update rule (applies to every service spec in later phases): prepend `/` to every asserted URL string and convert typed-query invocations/assertions to the `QueryingParameters` form. The expected `getPaged` params equal the passed params (no transformation). Example:

```ts
// before
await CountryApi.getCountries({ isActive: true, page: 1, pageSize: 10 })
expect(mockGetPaged).toHaveBeenCalledWith(
  'api/admin/location/countries',
  { filter: 'isActive=true', search: null, sort: null, pageNumber: 1, pageSize: 10 },
  expect.objectContaining({ allowedFilterFields: expect.any(Array) }),
)
// after
await CountryApi.getCountries({ filter: 'isActive=true', pageNumber: 1, pageSize: 10 })
expect(mockGetPaged).toHaveBeenCalledWith(
  '/api/admin/location/countries',
  { filter: 'isActive=true', pageNumber: 1, pageSize: 10 },
  expect.objectContaining({ allowedFilterFields: expect.any(Array) }),
)
```

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Convert `app/Admin/src/features/location/services/countryApi.ts`, `stateApi.ts`, and `app/Admin/src/features/dashboard/services/dashboardApi.ts` per the conversion rule (`dashboardApi` has no list method; only inline the path). Update their specs: `location/services/__tests__/countryApi.spec.ts`, `location/services/__tests__/stateApi.spec.ts`, `dashboard/__tests__/services/dashboardApi.spec.ts`. | Yes | 2026-08-11 |
| TASK-006 | Delete `app/Admin/src/features/location/composables/useActiveCountries.ts` and `app/Admin/src/features/location/composables/__tests__/useActiveCountries.spec.ts`; remove its export from `app/Admin/src/features/location/composables/index.ts`. | Yes | 2026-08-11 |
| TASK-007 | Migrate views in `app/Admin/src/features/location/views/`: `CountriesList.vue` → `usePagedQuery<CountryListItem>((params) => CountryApi.getCountries(params), { ...current options })`; `StatesList.vue` → `usePagedQuery<StateListItem>((params) => StateApi.getStates(params), { ... })` and inline the active-countries select via `useActiveList<CountryListItem>(() => CountryApi.getCountries({ filter: 'isActive=true' }))`; `StateDetail.vue` → replace the `useActiveCountries` import with the same `useActiveList` fetcher. Import `useActiveList` from `@/shared/composables`. | Yes | 2026-08-11 |
| TASK-008 | Verify Phase 2: `cd app/Admin && pnpm run lint && pnpm run type-check && pnpm run test:unit`. Confirm no `api/locations` string remains in `app/Admin/src`. | Yes | 2026-08-11 |

### Implementation Phase 3 — Catalog module

- GOAL-003: Convert all 13 Catalog service classes, delete the 9 thin Catalog composables and the `variantListUrl` util, and migrate the 7 Catalog views (including the scoped `TaxonsList`/`VariantsList` fetchers) onto the Api services.

Special service notes (beyond the base conversion rule):

- `variantApi.ts`: `const BASE = 'api/admin/catalog/variants'` becomes inline `/api/admin/catalog/variants`; `getVariants(productId, query)` becomes `getVariants(productId, params: QueryingParameters)`; the four inline `variant-option-values` URL literals gain a leading `/` (`` `/api/admin/catalog/variant-option-values?...` `` etc.).
- `taxonRuleApi.ts`: `getRules(taxonId: string)` becomes `getRules(taxonId: string, params: QueryingParameters = {})`.
- `variantImageApi.ts`: `listImages(variantId: string)` becomes `listImages(variantId: string, params: QueryingParameters = {})`.
- `variantPriceApi.ts`: `listPrices(variantId: string)` becomes `listPrices(variantId: string, params: QueryingParameters = {})` (drop the hardcoded `{ pageNumber: 1, pageSize: 100 }`).
- `taxonApi.ts`: `getList(taxonomyId, query)` becomes `getList(taxonomyId, params)`.

View fetcher mapping for this phase (each view already imports the service; preserve `...options`/current option object):

- `ProductsList.vue` → `usePagedQuery<ProductListItem>((params) => ProductApi.getProducts(params), { ... })`
- `OptionTypesList.vue` → `usePagedQuery<OptionTypeListItem>((params) => OptionTypeApi.getOptionTypes(params), { ... })`
- `OptionTypeDetail.vue` → `usePagedQuery<OptionValueListItem>((params) => OptionValueApi.getOptionValues(params), { ... })`
- `TaxonomiesList.vue` → `usePagedQuery<TaxonomyListItem>((params) => TaxonomyApi.getTaxonomies(params), { ... })`
- `TaxonsList.vue` → PAT-003: `usePagedQuery<TaxonListItem>((params) => (taxonomyId.value ? TaxonApi.getList(taxonomyId.value, params) : TaxonApi.getTaxons(params)), { ... })` and `useActiveList<TaxonomyListItem>(() => TaxonomyApi.getTaxonomies({}))`
- `VariantsList.vue` → `usePagedQuery<VariantListItem>((params) => VariantApi.getVariants(productId.value ?? '', params), { ... })`; delete the `variantsListUrl` import/usage
- `TaxonDetail.vue` → replace `useActiveTaxonomies` with `useActiveList<TaxonomyListItem>(() => TaxonomyApi.getTaxonomies({}))`; `TaxonApi.getList(taxonomyId, {})` is unchanged (empty `{}` is a valid `QueryingParameters`)
- `useProductOptions.ts` (kept): `ProductApi.getProducts({ search: term, page: 1, pageSize: PAGE_SIZE, sortBy: 'name' })` becomes `ProductApi.getProducts({ search: term, pageNumber: 1, pageSize: PAGE_SIZE, sort: ['name'] })`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Convert the 13 Catalog services (`catalog/services/{productApi,taxonApi,taxonomyApi,taxonRuleApi,optionTypeApi,optionValueApi,productOptionTypeApi,productClassificationApi,catalogDashboardApi,imageEmbeddingApi,variantApi,variantImageApi,variantPriceApi}.ts`) per the conversion rule + special notes. Update the 13 Catalog service specs under `catalog/__tests__/services/*.spec.ts`. | | |
| TASK-010 | Delete the 9 thin Catalog composables: `composables/{useProductList,useOptionTypeList,useOptionValueList,useTaxonList,useTaxonRuleList,useVariantList,useVariantImageList,useVariantPriceList,useActiveTaxonomies}.ts`. Delete specs `composables/__tests__/{useTaxonList,useActiveTaxonomies}.spec.ts`. Delete the util `catalog/utils/variantListUrl.ts` and `catalog/__tests__/utils/variantListUrl.spec.ts`. Update `catalog/composables/index.ts` to keep only `useTaxonDetail`, `useProductOptions`, `useEmbeddingStatus` (+ their type exports). | | |
| TASK-011 | Migrate the 7 Catalog views (`ProductsList.vue`, `OptionTypesList.vue`, `OptionTypeDetail.vue`, `TaxonsList.vue`, `VariantsList.vue`, `TaxonomiesList.vue`, `TaxonDetail.vue`) per the fetcher mapping. Update `useProductOptions.ts` typed call. | | |
| TASK-012 | Verify Phase 3: `cd app/Admin && pnpm run lint && pnpm run type-check && pnpm run test:unit`. | | |

### Implementation Phase 4 — Identity module

- GOAL-004: Convert the 3 Identity service classes, delete the thin `useRoleList`/`useUserList`/`useActiveUsers` composables, extend `PermissionApi.getPermissions` to accept params, and migrate the `RolesList`, `UsersList`, `PermissionsList`, `DashboardPage`, and `UserDetail` call sites.

Service notes: `permissionApi.ts` `getPermissions()` becomes `getPermissions(params: QueryingParameters = {})` returning `getPaged<PermissionMetadata>('/api/admin/identity/permissions', params, { allowedSearchFields: ['name', 'category', 'description'] })` so the PermissionsList search keeps working.

View fetcher mapping:

- `RolesList.vue` → `usePagedQuery<RoleListItem>((params) => RoleApi.getRoles(params), { ... })`
- `UsersList.vue` → `usePagedQuery<UserListItem>((params) => UserApi.getUsers(params), { ... })`
- `PermissionsList.vue` → `usePagedQuery<PermissionMetadata>((params) => PermissionApi.getPermissions(params), { ... })` (keep `defaultPageSize: 100` and current search options)
- `DashboardPage.vue` → `useActiveList<UserListItem>(() => UserApi.getUsers({}))`
- `UserDetail.vue` → `RoleApi.getRoles({ pageSize: 100 })` is unchanged (valid `QueryingParameters`)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | Convert `identity/services/{userApi,roleApi,permissionApi}.ts` per the conversion rule + permission note. Update `identity/__tests__/services/{userApi,roleApi,permissionApi}.spec.ts`. | | |
| TASK-014 | Delete `identity/composables/{useRoleList,useUserList,useActiveUsers}.ts` and `identity/composables/__tests__/useActiveUsers.spec.ts`. Update `identity/composables/index.ts` to keep `useRoleDetail`, `useUserDetail`. | | |
| TASK-015 | Migrate `identity/views/{RolesList,UsersList,PermissionsList}.vue` and `dashboard/views/DashboardPage.vue` per the fetcher mapping. `UserDetail.vue` requires no edit. | | |
| TASK-016 | Verify Phase 4: `cd app/Admin && pnpm run lint && pnpm run type-check && pnpm run test:unit`. | | |

### Implementation Phase 5 — Inventory module

- GOAL-005: Convert the 6 Inventory service classes, delete the thin Inventory composables, and migrate the Inventory list views plus the `StockItemDetail`/`StockTransferDetail` inline typed calls.

Service notes:

- `stockItemApi.ts`: `getStockItems(params)` per base rule. `getLowStockItems(params: LowStockQuery)` becomes `getLowStockItems(params: QueryingParameters = {})` returning `getPaged<LowStockItem>('/api/admin/inventory/stock-items/low-stock', params)` (drop the `locationId`/`threshold` URL-part logic — no caller uses it). `getStockSummary(query)` becomes `getStockSummary(params: QueryingParameters = {})` returning `getPaged<StockSummaryDetailResponse>('/api/admin/inventory/stock-items/summary', params)`.
- `stockMovementApi.ts`: `getStockMovements(params)` per base rule; drop the `fromUtc`/`toUtc`/`variantId`/`stockLocationId` destructuring and URL-part building (no caller uses them); URL is the plain base path.
- `stockReservationApi.ts`, `stockTransferApi.ts`, `stockLocationApi.ts`: base rule only.

View fetcher mapping (all views already import the service or the deleted composable):

- `StockItemsList.vue` → `usePagedQuery<StockItemListItem>((params) => StockItemApi.getStockItems(params), { ... })` + `useActiveList<StockLocationListItem>(() => StockLocationApi.getStockLocations({ pageSize: 100, sort: ['name'] }))`
- `StockLocationsList.vue` → `usePagedQuery<StockLocationListItem>((params) => StockLocationApi.getStockLocations(params), { ... })`
- `StockMovementsList.vue` → `usePagedQuery<StockMovementListItem>((params) => StockMovementApi.getStockMovements(params), { ... })`
- `StockReservationsList.vue` → `usePagedQuery<StockReservationListItem>((params) => StockReservationApi.getStockReservations(params), { ... })`
- `StockTransfersList.vue` → `usePagedQuery<StockTransferListItem>((params) => StockTransferApi.getStockTransfers(params), { ... })` + the active-stock-locations `useActiveList` fetcher
- `StockItemDetail.vue` → inline the active-stock-locations fetcher; `VariantApi.getVariants('', { pageSize: 100 })` is unchanged
- `StockTransferDetail.vue` → inline the active-stock-locations fetcher; `VariantApi.getVariants('', { pageSize: 100 })` unchanged

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-017 | Convert `inventory/services/{stockItemApi,stockLocationApi,stockMovementApi,stockReservationApi,stockTransferApi,inventoryDashboardApi}.ts` per the conversion rule + service notes. Update the 6 specs under `inventory/__tests__/services/*.spec.ts`. | | |
| TASK-018 | Delete `inventory/composables/{useStockItemList,useStockLocationList,useStockMovementList,useStockReservationList,useStockTransferList,useActiveStockLocations}.ts` and `inventory/composables/__tests__/useActiveStockLocations.spec.ts`. Update `inventory/composables/index.ts` to export nothing (or delete the file if empty). | | |
| TASK-019 | Migrate the 5 Inventory list views + `StockItemDetail.vue` + `StockTransferDetail.vue` per the fetcher mapping. | | |
| TASK-020 | Verify Phase 5: `cd app/Admin && pnpm run lint && pnpm run type-check && pnpm run test:unit`. | | |

### Implementation Phase 6 — Profile + Payment modules

- GOAL-006: Convert the Profile and Payment service classes, delete the thin Profile/Payment list composables, and migrate the Profile/Payment views and the `ProfileDetail`/`useProfileDetail` call sites.

View fetcher mapping:

- `ProfilesList.vue` → `usePagedQuery<ProfileListItem>((params) => ProfileApi.getProfiles(params), { ... })`
- `AddressesList.vue` → `usePagedQuery<AddressResponse>((params) => AddressApi.getAddresses(initialUserId, params), { ... })` (keep the current `initialUserId` argument)
- `PaymentsList.vue` → `usePagedQuery<PaymentListItem>((params) => PaymentApi.getPayments(params), { ... })`
- `PaymentMethodsList.vue` → `usePagedQuery<PaymentMethodListItem>((params) => PaymentMethodApi.getPaymentMethods(params), { ... })`
- `ProfileDetail.vue` → `AddressApi.getAddresses(userId, { pageSize: 100 })` (drop the redundant `userId` key from the params object)
- `useProfileDetail.ts` (kept) → `ProfileApi.getProfiles({ pageSize: 100 })` unchanged (valid `QueryingParameters`)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-021 | Convert `profile/services/{profileApi,addressApi}.ts` and `payment/services/{paymentApi,paymentMethodApi}.ts` per the conversion rule. Update specs `profile/__tests__/services/{profileApi,addressApi}.spec.ts` and `payment/__tests__/services/{paymentApi,paymentMethodApi}.spec.ts`. | | |
| TASK-022 | Delete `profile/composables/{useProfileList,useAddressList}.ts` and `payment/composables/{usePaymentList,usePaymentMethodList}.ts`. Update `profile/composables/index.ts` (keep `useProfileDetail`, `useAddressDetail`) and `payment/composables/index.ts` (keep `usePaymentMethodDetail`). | | |
| TASK-023 | Migrate `profile/views/{ProfilesList,AddressesList,ProfileDetail}.vue`, `payment/views/{PaymentsList,PaymentMethodsList}.vue` per the fetcher mapping. | | |
| TASK-024 | Verify Phase 6: `cd app/Admin && pnpm run lint && pnpm run type-check && pnpm run test:unit`. | | |

### Implementation Phase 7 — Shipping + Ordering modules

- GOAL-007: Convert the Shipping and Ordering service classes, delete the thin Shipping/Ordering list composables, and migrate the Shipping/Ordering views including the `OrderDetail` inline typed calls.

View fetcher mapping:

- `ShippingMethodsList.vue` → `usePagedQuery<ShippingMethodListItem>((params) => ShippingMethodApi.getShippingMethods(params), { ... })`
- `ShippingRatesList.vue` → `usePagedQuery<ShippingRateListItem>((params) => ShippingRateApi.getShippingRates(params), { ... })`
- `OrdersList.vue` → `usePagedQuery<OrderListItem>((params) => OrderApi.getOrders(params), { ... })`
- `ShippingRateDetail.vue` → `ShippingMethodApi.getShippingMethods({ pageSize: 100 })` unchanged
- `OrderDetail.vue` → `OrderApi.getLineItems(orderId.value, { pageSize: 100 })` unchanged; `PaymentApi.getPayments({ orderId: orderId.value, pageSize: 100 })` becomes `PaymentApi.getPayments({ filter: \`orderId=${orderId.value}\`, pageSize: 100 })`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-025 | Convert `shipping/services/{shippingMethodApi,shippingRateApi}.ts` and `ordering/services/{orderApi,orderingDashboardApi}.ts` per the conversion rule. Update specs `shipping/__tests__/services/{shippingMethodApi,shippingRateApi}.spec.ts` and `ordering/__tests__/services/{orderApi,orderingDashboardApi}.spec.ts`. | | |
| TASK-026 | Delete `shipping/composables/{useShippingMethodList,useShippingRateList}.ts` and `ordering/composables/useOrderList.ts`. Update `shipping/composables/index.ts` (keep the two detail composables) and `ordering/composables/index.ts` (keep `useOrderDetail`). | | |
| TASK-027 | Migrate `shipping/views/{ShippingMethodsList,ShippingRatesList,ShippingRateDetail}.vue` and `ordering/views/{OrdersList,OrderDetail}.vue` per the fetcher mapping. | | |
| TASK-028 | Verify Phase 7: `cd app/Admin && pnpm run lint && pnpm run type-check && pnpm run test:unit`. | | |

### Implementation Phase 8 — Final verification & drift gates

- GOAL-008: Confirm zero relative/inline API URLs, zero `BASE` constants, zero remaining thin composables, and a green full Admin verification.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-029 | Run the drift gates and confirm all four print no matches under `app/Admin/src`: `grep -rn -E "readonly BASE =|^const BASE = " --include=*.ts --include=*.vue .`; `grep -rn -E "['\`]api/(admin|locations)" --include=*.ts --include=*.vue .`; `grep -rn -E "usePagedQuery<[^>]+>\(['\`/]|usePagedQuery<[^>]+>\(\(\) =>" --include=*.vue .`; `grep -rn "useActiveList<[^>]+>\(\(\) => .*Api\." --include=*.vue .` (last one is informational; fetchers should reference Api methods). | | |
| TASK-030 | Run the full Admin gate: `cd app/Admin && pnpm run lint && pnpm run type-check && pnpm run test:unit && pnpm run build`. | | |

## 3. Alternatives

- **ALT-001**: Keep the 26 thin composables and only swap their internals to call the Api services. Rejected: a composable whose body is solely `usePagedQuery((params) => XxxApi.getXxx(params))` adds an indirection layer with no behavior or type value; deleting it makes the view-API-service wiring explicit and reduces file count.
- **ALT-002**: Keep the typed `XxxQuery` parameters on service list methods and reconstruct the typed query from the `QueryingParameters` state inside each composable. Rejected: the composable state is raw DSL (`filter: 'status=Placed'`, `sort: ['-createdAtUtc']`, `pageNumber`, …) that the typed queries cannot represent without lossy parsing; passing `QueryingParameters` through unchanged is exact and removes the `toXxxQueryParams` indirection.
- **ALT-003**: Keep the `BASE` constant but only add a leading slash (`'/api/admin/...'`). Rejected: the user's rationale is that the endpoint must be visible and greppable inline at each call site so it is easy to track and rename; a shared constant re-hides the endpoint.
- **ALT-004**: A single codemod that rewrites all modules at once. Rejected: the user requires a gradual per-module rollout with per-phase verification so each change set is small and reviewable.

## 4. Dependencies

- **DEP-001**: Phase 1 (`usePagedQuery` fetcher support) must be complete and green before any composable/view migration phase.
- **DEP-002**: Within a module phase, the service conversion (and spec update) precedes the composable deletion and view migration tasks, because the views call the new method signatures.
- **DEP-003**: The Vite dev proxy `/api` rule (`app/Admin/vite.config.ts`) and `VITE_API_URL` (`.env.development`) remain unchanged; leading-slash URLs resolve correctly whether or not `VITE_API_URL` is set.
- **DEP-004**: `getPaged`, `PagedRequestOptions`, and the `QueryingParameters` machinery in `app/Admin/src/shared/api` and `app/Admin/src/shared/types/querying` are unchanged and are the runtime foundation of the new fetcher form.

## 5. Files

- **FILE-001**: `app/Admin/src/shared/composables/usePagedQuery.ts` (extended), `app/Admin/src/shared/composables/index.ts` (export `PagedFetcher`), `app/Admin/src/shared/composables/__tests__/usePagedQuery.spec.ts` (new fetcher tests).
- **FILE-002**: 33 Admin service files converted (BASE removed, full path inlined, list methods take `QueryingParameters`): `features/dashboard/services/dashboardApi.ts`; `features/location/services/{countryApi,stateApi}.ts`; `features/ordering/services/{orderApi,orderingDashboardApi}.ts`; `features/catalog/services/{catalogDashboardApi,imageEmbeddingApi,optionTypeApi,optionValueApi,productApi,productClassificationApi,productOptionTypeApi,taxonApi,taxonRuleApi,taxonomyApi,variantApi,variantImageApi,variantPriceApi}.ts`; `features/identity/services/{permissionApi,roleApi,userApi}.ts`; `features/inventory/services/{inventoryDashboardApi,stockItemApi,stockLocationApi,stockMovementApi,stockReservationApi,stockTransferApi}.ts`; `features/payment/services/{paymentApi,paymentMethodApi}.ts`; `features/profile/services/{addressApi,profileApi}.ts`; `features/shipping/services/{shippingMethodApi,shippingRateApi}.ts`.
- **FILE-003**: 33 Admin service spec files updated (leading-slash URLs, `QueryingParameters` invocations): the `*Api.spec.ts` under `features/*/__tests__/services/` for the 33 services above (and `features/location/services/__tests__/`).
- **FILE-004**: 26 deleted thin composables: `features/catalog/composables/{useProductList,useOptionTypeList,useOptionValueList,useTaxonList,useTaxonRuleList,useVariantList,useVariantImageList,useVariantPriceList,useActiveTaxonomies}.ts`; `features/identity/composables/{useRoleList,useUserList,useActiveUsers}.ts`; `features/inventory/composables/{useStockItemList,useStockLocationList,useStockMovementList,useStockReservationList,useStockTransferList,useActiveStockLocations}.ts`; `features/location/composables/useActiveCountries.ts`; `features/ordering/composables/useOrderList.ts`; `features/payment/composables/{usePaymentList,usePaymentMethodList}.ts`; `features/profile/composables/{useProfileList,useAddressList}.ts`; `features/shipping/composables/{useShippingMethodList,useShippingRateList}.ts`.
- **FILE-005**: Deleted tests/util: `features/catalog/composables/__tests__/{useTaxonList,useActiveTaxonomies}.spec.ts`; `features/identity/composables/__tests__/useActiveUsers.spec.ts`; `features/inventory/composables/__tests__/useActiveStockLocations.spec.ts`; `features/location/composables/__tests__/useActiveCountries.spec.ts`; `features/catalog/utils/variantListUrl.ts`; `features/catalog/__tests__/utils/variantListUrl.spec.ts`.
- **FILE-006**: Feature composable barrels updated (deleted exports removed): `features/{catalog,identity,inventory,location,ordering,payment,profile,shipping}/composables/index.ts`.
- **FILE-007**: Views migrated to inline `usePagedQuery`/`useActiveList` fetchers calling Api services: `features/location/views/{CountriesList,StatesList,StateDetail}.vue`; `features/dashboard/views/DashboardPage.vue`; `features/catalog/views/{ProductsList,OptionTypesList,OptionTypeDetail,TaxonsList,VariantsList,TaxonomiesList,TaxonDetail}.vue`; `features/identity/views/{RolesList,UsersList,PermissionsList}.vue`; `features/inventory/views/{StockItemsList,StockLocationsList,StockMovementsList,StockReservationsList,StockTransfersList,StockItemDetail,StockTransferDetail}.vue`; `features/ordering/views/{OrdersList,OrderDetail}.vue`; `features/payment/views/{PaymentsList,PaymentMethodsList}.vue`; `features/profile/views/{ProfilesList,AddressesList,ProfileDetail}.vue`; `features/shipping/views/{ShippingMethodsList,ShippingRatesList,ShippingRateDetail}.vue`.
- **FILE-008**: `features/catalog/composables/useProductOptions.ts` (kept; typed call migrated to `QueryingParameters`). Kept unchanged: `useDashboard`, `useProductOptions`, all `*Detail` composables, `useEmbeddingStatus`, and the shared composables.

## 6. Testing

- **TEST-001**: `usePagedQuery.spec.ts` — new fetcher-mode cases: fetcher receives assembled `QueryingParameters`, updates `items` on success, and is re-invoked on `setPage`/`setFilter`/`setSearch`. Existing URL-mode cases must stay green.
- **TEST-002**: Per-phase `pnpm run test:unit` in `app/Admin` — the updated service specs assert leading-slash URLs and params passthrough; view/composable changes are covered by the remaining unit suite.
- **TEST-003**: Per-phase `pnpm run type-check` in `app/Admin` (vue-tsc) — catches any missed call-site signature mismatch after the `QueryingParameters` switch and composable deletions.
- **TEST-004**: Per-phase `pnpm run lint` in `app/Admin` — catches unused imports (removed `toXxxQueryParams`/`XxxQuery` imports, deleted composable exports) and dead references.
- **TEST-005**: Drift gate greps in Phase 8 (see TASK-029) — zero `BASE` constants, zero relative/`api/locations` URL literals, zero string/URL-fn `usePagedQuery` sources under `app/Admin/src`.
- **TEST-006**: Final `pnpm run build` in `app/Admin` — full production type-check + build after the last phase.

## 7. Risks & Assumptions

- **RISK-001**: View-custom `allowedFilterFields`/`allowedSearchFields` lists could be dropped instead of promoted into the service (GUD-003), changing server-side search/filter field validation. Mitigation: GUD-003 mandates promoting the view's current lists verbatim; per-phase unit tests and manual smoke of list search catch regressions.
- **RISK-002**: `usePagedQuery` distinguishes URL-functions from fetchers by function arity (`source.length >= 1`). A 0-arity fetcher would be misrouted. Mitigation: all plan fetchers are `(params) => …` (arity 1); the type union + tests lock the contract; URL-fn mode is retained only for backward compatibility and the shared spec.
- **RISK-003**: The `api/locations/countries` / `api/locations/states` paths in `CountriesList`/`StatesList` are wrong today; migrating them to `CountryApi`/`StateApi` changes the runtime endpoint. Mitigation: the correct `/api/admin/location/...` routes are confirmed in `CountryApi`/`StateApi`; verify the two views load in dev after Phase 2.
- **RISK-004**: Removing `toXxxQueryParams` from services leaves the builder functions and `XxxQuery` interfaces exported from `features/*/types/*.ts` and the feature `index.ts` barrels as unused exports. Mitigation: harmless (no build/lint error for exported dead code); follow-up cleanup of dead builders is explicitly out of scope.
- **ASSUMPTION-001**: Backend `/api/admin/...` route prefixes are stable for the duration of this refactor.
- **ASSUMPTION-002**: `VITE_API_URL` and the Vite `/api` proxy are unchanged; leading-slash absolute paths resolve in dev and production.
- **ASSUMPTION-003**: The 26 composables to delete have no consumers outside `app/Admin/src` (verified by grep); the thin composables wrapping `useActiveList` are used only by the views enumerated in TASK-007/011/015/019/023/027.

## 8. Related Specifications / Further Reading

- `app/Admin/AGENTS.md` — Admin SPA conventions and Code Commenting Standard for views
- `app/Admin/src/shared/api/paged.ts` — `getPaged` + `PagedRequestOptions` (unchanged runtime)
- `app/Admin/src/shared/types/querying/querying.ts` — `QueryingParameters` shape
- `app/Admin/src/shared/composables/useActiveList.ts` — active-list fetcher pattern (PAT-002)
- `docs/codebase/CONVENTIONS.md` — repository coding conventions
