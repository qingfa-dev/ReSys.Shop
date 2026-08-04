# Remove Forced Pinia Stores from the Admin Frontend

> Status: Approved for implementation planning
> Date: 2026-08-03
> Branch: `feature/implement-admin-panel`
> Applies to: `app/Admin` (Vue 3 + TypeScript, PrimeVue 5, Pinia)

## Context

The Admin SPA has 20 Pinia stores, but most no longer serve a purpose:

- **12 stores are dead code** — zero usage in any view, composable, router, or guard after the composable-based refactor (`usePagedQuery`, `useOptionTypeList`, etc.). They survive only alongside their spec files.
- **7 stores are used but trivial** — mostly the same ~20-line "fetch-active list" cache pattern (a `ref`, a `loaded` flag, and a `fetchActive()` dedupe) or thin passthroughs to api services. They duplicate each other and the composable pattern that is now the house style.
- **1 store is genuinely complex** — `authStore` (session state, tokens, permissions, login/logout/revoke), consumed by login, menus, and router guards. It stays.

The composable pattern (`usePagedQuery` in `shared/composables/`, feature-level wrappers in each `features/*/composables/`) is the established house style for list and fetch state. `useTaxonList` already exists but has zero consumers.

## Goal

- Delete all 12 dead stores and the 7 used-but-trivial stores, plus their spec files and emptied barrel exports.
- Replace the 7 used stores with composables + api services.
- Keep `authStore` as the single remaining Pinia store. Pinia dependency and plugin stay.
- Fully rewrite the 9 affected views with Code Commenting Standard v3.0 labels applied to new/rewritten logic.
- End state: `features/auth/stores/` is the only stores directory left in the app.

## Non-Goals

- No change to `authStore` behavior or its consumers.
- No Pinia removal — `createPinia` and the `pinia` dependency remain.
- No changes to api services, backend, or other features' views.
- No commenting retrofit of untouched views or existing code (the guide targets new code).

## Deletions

### Dead stores (12) + spec files

| Feature | Stores |
|---------|--------|
| catalog | `productStore`, `optionTypeStore` |
| identity | `roleStore` |
| inventory | `stockTransferStore`, `stockItemStore` |
| location | `stateStore` |
| ordering | `orderStore` |
| payment | `paymentMethodStore` |
| profile | `profileStore`, `addressStore` |
| shipping | `shippingRateStore`, `shippingMethodStore` |

Spec files deleted: `roleStore.spec`, `stockTransferStore.spec`, `stockItemStore.spec`, `orderStore.spec`, `paymentMethodStore.spec`, `profileStore.spec`, `addressStore.spec`, `shippingRateStore.spec`, `shippingMethodStore.spec` (specs for `productStore`, `optionTypeStore`, `stateStore` do not exist — verify during implementation).

### Replaced stores (7) + spec files

| Feature | Store | Replacement |
|---------|-------|-------------|
| catalog | `taxonomyStore` | `useActiveTaxonomies` |
| catalog | `taxonStore` | `useTaxonList(taxonomyId)` (extended) |
| catalog | `taxonDetailStore` | `useTaxonDetail` (new) |
| dashboard | `dashboardStore` | view-local state + `DashboardApi` |
| identity | `userStore` | `useActiveUsers` |
| location | `countryStore` | `useActiveCountries` |
| inventory | `stockLocationStore` | `useActiveStockLocations` |

Spec files deleted: `taxonStore.spec`, `taxonDetailStore.spec`, `dashboardStore.spec`, `userStore.spec`, `stockLocationStore.spec`.

### Empty barrels

`stores/index.ts` is deleted for every feature whose stores are all gone: catalog, identity, inventory, location, ordering, payment, profile, shipping, dashboard. `features/auth/stores/index.ts` and `authStore` remain. Verify during implementation that nothing imports from the deleted barrels.

## New composables

### Shared: `useActiveList<T>` — `app/Admin/src/shared/composables/useActiveList.ts`

One composable for the fetch-active Select-option pattern, replacing 4 identical store bodies.

- Signature: `useActiveList<T>(fetcher: () => Promise<PagedResult<T>>, options?: { immediate?: boolean })`
- Returns: `{ items: Ref<T[]>, loaded: Ref<boolean>, loading: Ref<boolean>, error: Ref<string | null>, load(): Promise<void>, reset(): void }`
- `load()` dedupes via the `loaded` flag (identical semantics to current `fetchActive`); `reset()` clears state so `load()` can refetch.
- Exported from `shared/composables/index.ts`.
- Labels: `Contract:` on entry, `Cache:` on the loaded dedupe, `Await:` on the fetch, `Guard:` on failure state.

### Feature wrappers (each ~10 lines)

| Wrapper | File | Fetcher |
|---------|------|---------|
| `useActiveCountries` | `features/location/composables/useActiveCountries.ts` | `CountryApi.getCountries({ isActive: true })` |
| `useActiveStockLocations` | `features/inventory/composables/useActiveStockLocations.ts` | `StockLocationApi.getStockLocations({ pageSize: 100, sortBy: 'name', sortDirection: 'asc' })` |
| `useActiveTaxonomies` | `features/catalog/composables/useActiveTaxonomies.ts` | `TaxonomyApi.getTaxonomies({})` |
| `useActiveUsers` | `features/identity/composables/useActiveUsers.ts` | `UserApi.getUsers({})` |

Each wrapper is exported from its feature's `composables/index.ts` and documented with `Contract:`/`Call:` labels.

### Catalog: `useTaxonList` extension

`features/catalog/composables/useTaxonList.ts` currently wraps `usePagedQuery('${CATALOG}/taxons', ...)` with no consumers. Extend it to accept a taxonomy scope and preserve current `taxonStore` behavior:

- Signature: `useTaxonList(taxonomyId?: Ref<string | null>, options?: UsePagedQueryOptions)`
- URL built as a function: `() => id ? `${CATALOG}/taxons/list?taxonomyId=${id}` : `${CATALOG}/taxons`` (matching `TaxonApi.getList` vs `getTaxons`).
- Default sort `['position']` to match the current store.

### Catalog: `useTaxonDetail` (new)

`features/catalog/composables/useTaxonDetail.ts` — same shape as `taxonDetailStore` so TaxonDetail migrates cleanly:

- Returns `{ currentTaxon, detailLoading, fetchDetail(id), rules, rulesLoading, fetchRules(taxonId) }`
- `fetchDetail` calls `TaxonApi.getTaxon(id)`; `fetchRules` calls `TaxonRuleApi.getRules(taxonId)`.
- Exported from `features/catalog/composables/index.ts`.

## View rewrites (9)

Each view is rewritten completely (script + template) with store refs replaced by composable refs and the Code Commenting Standard v3.0 (`guide/code-commenting/`) applied to all new/rewritten logic. Behavior stays identical — only the state source changes.

| View | Removed | Replaced by |
|------|---------|-------------|
| `catalog/views/TaxonsList.vue` | `useTaxonStore`, `useTaxonomyStore` | `useTaxonList(taxonomyId)`, `useActiveTaxonomies` |
| `catalog/views/TaxonDetail.vue` | `useTaxonDetailStore`, `useTaxonomyStore` | `useTaxonDetail`, `useActiveTaxonomies` |
| `dashboard/views/DashboardPage.vue` | `useDashboardStore`, `useUserStore` | local `summary` ref + `DashboardApi.getDashboard()` in `onMounted`, `useActiveUsers` |
| `location/views/StatesList.vue` | `useCountryStore` | `useActiveCountries` |
| `location/views/StateDetail.vue` | `useCountryStore` | `useActiveCountries` |
| `inventory/views/StockItemsList.vue` | `useStockLocationStore` | `useActiveStockLocations` |
| `inventory/views/StockItemDetail.vue` | `useStockLocationStore` | `useActiveStockLocations` |
| `inventory/views/StockTransfersList.vue` | `useStockLocationStore` | `useActiveStockLocations` |
| `inventory/views/StockTransferDetail.vue` | `useStockLocationStore` | `useActiveStockLocations` |

### Commenting application (per `guide/code-commenting/SKILL.md`)

- `Call:` on each api-service boundary — name the service and purpose.
- `Await:` on async loads (with mount semantics where relevant).
- `Validate:` / `Guard:` on form submissions and empty/loading state handling.
- `Filter:` / `Compute:` on derived values (e.g., the location-name lookup in StockTransferDetail).
- `Contract:` on composable entry functions; `Reset:` on form resets.
- One label per comment, imperative mood, capitalised body, max 100 chars (F1–F10). No commenting of trivial lines (AP-3), no redundancy (AP-1).

## Tests

- New: `shared/composables/__tests__/useActiveList.spec.ts` (load, dedupe, reset, failure).
- New: specs for the 4 wrappers + `useTaxonDetail`, mirroring existing store-spec patterns (mock api modules; no Pinia needed).
- `authStore.spec` untouched.
- Deleted: all spec files listed under Deletions.
- Update: any test that imported from a deleted `stores/index.ts` barrel.

## Verification

```bash
cd app/Admin
pnpm exec vue-tsc --noEmit
pnpm run lint
pnpm run test:unit
```

- Grep gate: `use\w+Store\(` must only match `authStore` (views, `router/guards.ts`) and `authStore.spec.ts`.
- No remaining imports from deleted `stores/` barrels.
- No remaining references to the 19 deleted/replaced store files.

## Risks

- **Missed barrel consumers:** any import of a deleted store via a barrel breaks typecheck — caught by `vue-tsc` gate.
- **Behavior drift in rewrites:** mitigated by keeping template structure and naming identical, only swapping the state source.
- **taxonomy-scoped taxon URL:** the `list?taxonomyId=` endpoint already exists (`TaxonApi.getList`); the url-as-function form of `usePagedQuery` is already supported.

## Out of Scope

- `authStore` refactor, Pinia removal, backend changes, other features' views, commenting of pre-existing code.
