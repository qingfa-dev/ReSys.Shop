# Admin Catalog Variants — Pattern Alignment Design

**Date:** 2026-07-31
**Status:** Approved
**Branch:** `feature/implement-admin-panel`

## Purpose

Align the frontend Variants feature with the established Admin Catalog list pattern. The backend `GetVariantsPagedOrAll` endpoint already conforms (returns `PagedResult`, supports `productId` filter, paging, filter/search/sort). The deviations are entirely on the frontend, which wraps list responses in `Result<{ items }>` instead of `PagedResult<T>`, validates with a single inline zod schema instead of per-field schemas, and filters client-side instead of server-side.

## Scope

Frontend only. Backend is out of scope and untouched.

### In Scope
- `app/Admin/src/features/catalog/types/variant.ts`
- `app/Admin/src/features/catalog/validations/variant.ts`
- `app/Admin/src/features/catalog/validations/index.ts`
- `app/Admin/src/features/catalog/services/variantApi.ts`
- `app/Admin/src/features/catalog/services/variantImageApi.ts`
- `app/Admin/src/features/catalog/services/variantPriceApi.ts`
- `app/Admin/src/features/catalog/views/VariantsList.vue`
- `app/Admin/src/features/catalog/views/VariantDetail.vue`

### Out of Scope
- Backend changes (already conformant).
- Taxonomies/Taxons/OptionTypes/OptionValues lists (already conformant).
- Master-variant row behavior, new UI features.
- ProductsList/ProductDetail pages (reference implementations, not changed).

## Approach

Approach A — full pattern alignment, mirroring `ProductApi`/`ProductsList`/`TaxonApi` as references. Rejected alternatives:
- B — minimal envelope fix only (leaves the list off-pattern, no paginator/server-side filtering).
- C — new generic list infrastructure beyond the existing `usePagedQuery` composable (YAGNI).

## Section 2 — Types & Query Mapping (`types/variant.ts`)

Mirrors `types/product.ts`.

### Interfaces
- `VariantListItem` — list-item shape. The backend list endpoint maps with `MapToDetail`, so list items include `discontinuedOn` and `pricesCount` in addition to the parameters:

  ```ts
  export interface VariantListItem extends VariantParameters {
    id: string
    productId: string
    isMaster: boolean
    discontinuedOn?: string | null
    pricesCount: number
  }
  ```

- `VariantDetail` — alias: `export type VariantDetail = VariantListItem` (same as `ProductDetail = ProductListItem`; the list endpoint already returns the full detail payload).
- `Variant` (old response type) is renamed to `VariantListItem`; the old `VariantRequest`/`VariantParameters` stay unchanged.

### Query interface

```ts
export interface VariantQuery {
  search?: string
  isMaster?: boolean
  sortBy?: 'sku' | 'position' | 'price' | 'weight' | 'height' | 'width' | 'depth'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}
```

`isMaster` is surfaced as the primary list discriminator; sort options limited to the backend's allowed sort fields.

### Fixed constants

Current frontend constants do not match the backend's allowed fields. Corrected:

- `VARIANT_FILTER_FIELDS = ['isMaster', 'trackInventory', 'discontinuedOn', 'dimensionsUnit', 'weightUnit']`
- `VARIANT_SORT_FIELDS = ['sku', 'position', 'price', 'weight', 'height', 'width', 'depth']`
- `VARIANT_SEARCH_FIELDS = ['sku', 'barcode', 'hsCode']`

### Query params mapper

```ts
export function toVariantQueryParams(query: VariantQuery): QueryingParameters
```

Builds the `filter` DSL (`isMaster=true` when set), `search`, `sort` (`-field` for desc), `pageNumber`, `pageSize`. Same shape as `toProductQueryParams`.

## Section 3 — Per-Field Zod Validation (`validations/variant.ts`)

Mirrors `validations/product.ts` / `taxon.ts`: one named schema const per field with explicit messages, composed into `variantSchema`. Constraint semantics are unchanged from the current implementation (nullable-with-default behavior preserved so the form semantics do not change).

```ts
export const variantSku = z.string()
  .min(1, 'SKU is required.')
  .max(255, 'SKU must not exceed 255 characters.')
  .refine((s) => s.trim().length > 0, 'SKU is required.')

export const variantPosition = z.number()
  .int('Position must be an integer.')
  .min(-1, 'Position must be at least -1.')
  .default(0)

export const variantIsMaster = z.boolean().default(false)
export const variantTrackInventory = z.boolean().default(true)

export const variantWeight = z.number()
  .min(0, 'Weight must be at least 0.')
  .nullable().optional().default(null)
export const variantWeightUnit = z.string()
  .max(50, 'Weight unit must not exceed 50 characters.')
  .nullable().optional().default(null)
export const variantHeight = z.number()
  .min(0, 'Height must be at least 0.')
  .nullable().optional().default(null)
export const variantWidth = z.number()
  .min(0, 'Width must be at least 0.')
  .nullable().optional().default(null)
export const variantDepth = z.number()
  .min(0, 'Depth must be at least 0.')
  .nullable().optional().default(null)
export const variantDimensionsUnit = z.string()
  .max(50, 'Dimensions unit must not exceed 50 characters.')
  .nullable().optional().default(null)
export const variantPrice = z.number()
  .min(0, 'Price must be at least 0.')
  .nullable().optional().default(null)
export const variantCostPrice = z.number()
  .min(0, 'Cost price must be at least 0.')
  .nullable().optional().default(null)
export const variantCostCurrency = z.string()
  .max(3, 'Cost currency must be a 3-letter code.')
  .nullable().optional().default(null)

export const variantSchema = z.object({
  sku: variantSku,
  position: variantPosition,
  isMaster: variantIsMaster,
  trackInventory: variantTrackInventory,
  weight: variantWeight,
  weightUnit: variantWeightUnit,
  height: variantHeight,
  width: variantWidth,
  depth: variantDepth,
  dimensionsUnit: variantDimensionsUnit,
  price: variantPrice,
  costPrice: variantCostPrice,
  costCurrency: variantCostCurrency,
})

export type VariantForm = z.infer<typeof variantSchema>
```

### Barrel (`validations/index.ts`)

Replace `export { variantSchema }` with named exports for all 13 field consts plus `variantSchema` and `VariantForm`, matching the `product*` block style.

## Section 4 — API Services

### `services/variantApi.ts`

Follows `productApi.ts`: uses `getPaged`, returns `PagedResult`.

```ts
static getVariants(
  productId: string,
  query: VariantQuery,
): Promise<PagedResult<VariantListItem>> {
  return getPaged<VariantListItem>(
    `${CATALOG}/products/${productId}/variants`,
    toVariantQueryParams(query),
    {
      allowedFilterFields: VARIANT_FILTER_FIELDS,
      allowedSortFields: VARIANT_SORT_FIELDS,
      allowedSearchFields: VARIANT_SEARCH_FIELDS,
    },
  )
}
```

- `productId` remains a path segment (backend route requires it), mirroring `taxonApi.getList(taxonomyId, query)`.
- `getVariant(id)` / `createVariant` / `updateVariant` return `Result<VariantDetail>` (updated to the new detail type after the `Variant` → `VariantListItem` rename).
- `deleteVariant` unchanged (returns `Result<void>`).
- `assignOptionValues`/`revokeOptionValues` unchanged.
- `getOptionValues(variantId)` envelope fix only: `Result<{ items: OptionValueAssignment[] }>` → `PagedResult<OptionValueAssignment>` (backend returns `PagedResult`).

### `services/variantImageApi.ts`

```ts
static listImages(variantId: string): Promise<PagedResult<VariantImage>> {
  return getPaged<VariantImage>(`${BASE}/${variantId}/images`, {
    pageNumber: 1,
    pageSize: 100,
  })
}
```

- `uploadImage`/`deleteImage` unchanged.

### `services/variantPriceApi.ts`

```ts
static listPrices(variantId: string): Promise<PagedResult<Price>> {
  return getPaged<Price>(`${BASE}/${variantId}/prices`, {
    pageNumber: 1,
    pageSize: 100,
  })
}
```

- `setPrice`/`removePrice` unchanged. `PriceRequest` unchanged.

## Section 5 — Views

### `VariantsList.vue`

Convert to the `usePagedQuery` pattern like ProductsList:

```ts
const { items, loading, error, totalCount, totalPages, page, setPage, pageSize, setPageSize, search, setSearch, refresh, sort, setSort } =
  usePagedQuery<VariantListItem>(() => `api/catalog/products/${productId.value}/variants`, {
    allowedFilterFields: VARIANT_FILTER_FIELDS,
    allowedSortFields: VARIANT_SORT_FIELDS,
    allowedSearchFields: VARIANT_SEARCH_FIELDS,
    defaultSearchFields: VARIANT_SEARCH_FIELDS,
    defaultSearchMode: 'any',
    defaultSort: ['position'],
    defaultPageSize: 20,
  })
```

- `productId` comes from the route query (`route.query.productId`), unchanged.
- Empty state: when `productId` is absent, show the current "Select a product to view its variants" panel and skip fetching. When present, `refresh()` on mount and re-fetch when `productId` changes (watch).
- DataTable: add PrimeVue `paginator` bound to `page`/`pageSize`/`totalCount`/`totalPages`; replace client-side `searchTerm`/`filteredItems` with server-side `setSearch`; search input calls `setSearch`.
- Keep `navigateToNew`, `navigateToEdit`, `navigateToProduct`, `confirmDelete`, `New Variant`/`Reload` buttons.
- Column data reads unchanged (`data.sku`, `data.isMaster`, `data.price`, `data.pricesCount`, `data.position`).
- `confirmDelete` reads `result.errors?.[0]?.message` — still valid because `Result` and `PagedResult` share the same `errors` shape.

### `VariantDetail.vue`

Update the three tab loaders to the PagedResult envelope:

- `loadImages` → `if (result.isSuccess) images.value = result.items` (was `result.value.images`).
- `loadPrices` → `prices.value = result.items` (was `result.value.items`).
- `loadOptionValues` → `optionValueAssignments.value = result.items` and `selectedOptionValueIds.value = result.items` (was `result.value.items`).
- Keep the existing `handleResult`/`notify.error` failure handling in each loader's else branch.

## Section 6 — Error Handling & Testing

### Error handling

- **PagedResult failures (`VariantsList`):** `usePagedQuery` exposes an `error` ref. When set, render a visible inline error alert above the table with the message and a Reload action (user chose the visible banner over parity-with-ProductsList).
- **Delete in list:** unchanged — `result.errors?.[0]?.message` works because `Result`/`PagedResult` share the `errors` shape.
- **Tab loaders (`VariantDetail`):** keep the existing per-loader `handleResult`/`notify.error` fallback.
- **No backend change:** server-side filter/search/sort errors come back as `PagedResult` failures normalized by `getPaged`/`usePagedQuery`.

### Testing

- Type-check: `pnpm run type-check` — 0 errors (rename + envelope changes are type-driven, catching missed consumers).
- Unit: `pnpm run test:unit -- run` — existing 584 tests stay green; check for existing `*.spec.ts` in `features/catalog` and add/update tests for `toVariantQueryParams` to mirror the reference helper tests.
- Build: `pnpm run build-only` (Admin).
- C# build untouched (no backend changes).

## Files Changed

| File | Change |
|------|--------|
| `app/Admin/src/features/catalog/types/variant.ts` | Rename `Variant` → `VariantListItem`, add `VariantDetail` alias, add `VariantQuery`, `toVariantQueryParams`, `VARIANT_SEARCH_FIELDS`, fix filter/sort constants |
| `app/Admin/src/features/catalog/validations/variant.ts` | Per-field schema consts with messages; compose `variantSchema` |
| `app/Admin/src/features/catalog/validations/index.ts` | Named field exports for variants |
| `app/Admin/src/features/catalog/services/variantApi.ts` | `getVariants(productId, query)` → `PagedResult` via `getPaged`; `getOptionValues` → `PagedResult` |
| `app/Admin/src/features/catalog/services/variantImageApi.ts` | `listImages` → `PagedResult` via `getPaged` |
| `app/Admin/src/features/catalog/services/variantPriceApi.ts` | `listPrices` → `PagedResult` via `getPaged` |
| `app/Admin/src/features/catalog/views/VariantsList.vue` | `usePagedQuery` + paginator + server-side search + error banner |
| `app/Admin/src/features/catalog/views/VariantDetail.vue` | Tab loaders read `.items` from `PagedResult` |
