# Admin Catalog Views Corrections — Design

**Date:** 2026-08-02
**Status:** Approved
**Scope:** Frontend-only (Admin SPA). Zero backend changes.

## Context

Four defects exist in the admin catalog views. All required backend endpoints already exist; the views either never call them, wire pagination client-side only, or bind PrimeVue components incorrectly.

## Issues and Root Causes

| View | Defect | Root cause |
| --- | --- | --- |
| `ProductsList.vue` | Missing status action button | Backend `Activate`/`Discontinue` endpoints and `ProductApi.activateProduct/discontinueProduct` already exist; the view never calls them. |
| `ProductsList.vue` | Paginator shows only first page | DataTable is client-side only: `:rows="20"`, no `total-records`, no `@page`/`@sort`. |
| `ProductDetail.vue` | Classification tab broken (stale / doesn't reload) | `initEditMode` reloads the form on product switch but never resets classification/option-type arrays; `watch(activeTab)` only loads when both lists are empty, so a save or product switch skips reload. |
| `ProductDetail.vue` | PickList moves snap back | Source bound one-way (`:source="unassignedClassifications"`) instead of `v-model`; PrimeVue PickList needs two-way source binding. |
| `OptionTypesList.vue` | Paginator shows only first page | Same client-side-only DataTable pattern as ProductsList. |
| `VariantsList.vue` | Dead "Select a product" state | List requires `?productId=` from route; backend `GetVariantsPagedOrAll` already accepts `ProductId` as optional. |
| `variantApi.ts` / `variantImageApi.ts` | Option values / images truncated | `getOptionValues` and `listImages` send `pageNumber: 1, pageSize: 100`; backend returns the full set when no paging params are sent, but truncates to 100 with them. |

## Approach

In-place fixes to existing views following established patterns (`usePagedQuery`, TaxonsList's Select-in-header). One new composable for the lazy product selector. No store refactor, no backend changes.

## Design

### 1. ProductsList fixes

**Status action button.** In the row actions column, add a status toggle button between the Variants/Edit/Delete icons:

- `status` is `'Draft'` or `'Archived'` → **Activate** button (`pi pi-check-circle`, success) → `ProductApi.activateProduct(id)` behind a confirm dialog.
- `status` is `'Active'` → **Discontinue** button (`pi pi-pause-circle`, warning) → `ProductApi.discontinueProduct(id)` behind a confirm dialog.
- On success: `notify.success` + `refresh()`. On failure: `notify.error` with the first error message.
- Status Tag column unchanged.

**Server paging.** Destructure `totalCount`, `page`, `pageSize`, `setPage`, `setPageSize`, `setSort` from the existing `usePagedQuery`. Wire `:total-records="totalCount"`, `:first="first"` (`(page - 1) * pageSize`), `@page`, `@update:rows`, `@sort` — mirroring `VariantsList.vue:87-99`. Replace `:rows="20"` with `:rows="pageSize"`.

**Error state.** Add the `v-else-if="error"` Message block that VariantsList has for server-error state.

### 2. ProductDetail tabs (classification + option types)

Both tabs share identical defects; both get fixed the same way.

**Reset on product switch.** Add `resetAssignments()` clearing all four arrays (`unassignedClassifications`, `assignedClassifications`, `unassignedOptionTypes`, `assignedOptionTypes`). Call it in `initEditMode` after a successful load and reset `activeTab` to `'0'`.

**Reload flags.** Replace the array-length guards in `watch(activeTab)` with `classificationsLoaded` / `optionTypesLoaded` flags so revisiting a tab after a save always reloads.

**Two-way PickList source.** Change `:source="unassignedClassifications"` → `v-model="unassignedClassifications"` and `:source="unassignedOptionTypes"` → `v-model="unassignedOptionTypes"`, keeping `v-model:target` as-is.

**Save flow.** Unchanged (`syncClassifications`/`syncOptionTypes`), but the reload after save is now guaranteed by the reset flags.

**Empty state.** Add a "No classifications available" / "No option types available" message when a loaded catalog is empty, instead of a blank panel.

### 3. OptionTypesList paging

Identical server-paging fix as ProductsList: destructure paging state, wire `total-records`/`first`/`@page`/`@update:rows`/`@sort`, keep `:rows="pageSize"`, add the error Message block.

### 4. VariantsList refactor (list + product select)

**New composable `useProductOptions`** in `app/Admin/src/features/catalog/composables/`:

- State: `options`, `loading`, `search`, `selectedId`, loaded cache key.
- `searchProducts(term)`: `ProductApi.getProducts({ search: term, page: 1, pageSize: 25, sortBy: 'name' })`, cached by term.
- On open/clear: load initial 25 (empty term). Debounced as-you-type (~300ms) → refetch.
- Exposes `ProductListItem`-shaped options.

**Select in header** (mirrors TaxonsList's taxonomy Select): `option-label="name"`, `option-value="id"`, `show-clear`, `filter`, `:loading`. Cleared = "All products" placeholder. Picking a product sets `productId`, updates the URL query, refetches variants.

**List behavior.**

- Remove the dead "Select a product" state (`VariantsList.vue:145-151`).
- `usePagedQuery` URL becomes `() => productId ? 'api/catalog/variants?productId=...' : 'api/catalog/variants'` — backend treats `ProductId` as optional.
- On select change: `setSearch('')`, reset to page 1, `refresh()`.
- ProductId syncs both ways: `route.query.productId` on mount populates the Select; selecting updates the URL via `router.replace`.
- "New Variant" requires a selected product (`?productId=`), disabled with a hint when "All products" is active.

### 5. Service alignment

- `VariantApi.getOptionValues`: drop `{ pageNumber: 1, pageSize: 100 }` → `{}`.
- `VariantImageApi.listImages`: drop `{ pageNumber: 1, pageSize: 100 }` → `{}`.
- `VariantApi.getVariants` already matches the current API; no change.

### 6. VariantDetail create flow

No change needed — the "New Variant" route already carries `productId` in the query. Verify `productId` is present when navigating from the refactored list.

## Error Handling

- Activate/discontinue failures: `notify.error`.
- Paged query failures: existing `error` state rendered via Message block.
- Product search failures: silent (Select shows empty options) — loading state cleared.

## Testing

- Update `variantApi.spec.ts` expectations (no `pageSize`/`pageNumber` in option-values/images URLs).
- Add `useProductOptions` composable spec (search/debounce/cache, mocked `ProductApi`).
- Verify with `cd app/Admin && pnpm run lint && pnpm run test:unit`.
- Backend untouched; `dotnet build` unchanged.

## Scope Guard

- No backend changes.
- No cross-module references.
- No new dependencies.
- Files: `ProductsList.vue`, `ProductDetail.vue`, `OptionTypesList.vue`, `VariantsList.vue`, `variantApi.ts`, `variantImageApi.ts`, new `useProductOptions.ts`, tests.
