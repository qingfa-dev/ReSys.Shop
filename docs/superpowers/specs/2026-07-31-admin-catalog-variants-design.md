# Admin Catalog Variants — Design Spec

**Date:** 2026-07-31
**Status:** Approved
**Part of:** Catalog domain completion (sub-project 1 of 4)

## Overview

Add standalone Variants CRUD (list + detail) to the admin SPA, following the
same route pattern as Taxons — flat routes under `/catalog/variants`, product
context via query param `?productId=xxx`.

## Routes

| Path | Route Name | Page |
|------|-----------|------|
| `catalog/variants` | `catalog-variants` | `VariantsList.vue` |
| `catalog/variants/new` | `catalog-variant-new` | `VariantDetail.vue` |
| `catalog/variants/:id` | `catalog-variant-detail` | `VariantDetail.vue` |

Product context flows via `?productId=xxx` query param. The ProductsList gets a
row action button per product row that navigates to
`/catalog/variants?productId=xxx`. The VariantsList "Back" link returns to
`/catalog/products/:productId`.

Sidebar: new `Variants` item under the Catalog group (alongside Products,
Taxonomies, Taxons, Option Types).

## New Frontend Files

```
app/Admin/src/features/catalog/
├── views/
│   ├── VariantsList.vue          (NEW)
│   └── VariantDetail.vue         (NEW)
├── services/
│   ├── variantApi.ts             (NEW)
│   ├── variantImageApi.ts        (NEW)
│   └── variantPriceApi.ts        (NEW)
├── types/
│   └── variant.ts                (NEW)
├── routes/
│   └── index.ts                  (MODIFIED — add 3 routes + menu item)
└── views/
    └── ProductsList.vue          (MODIFIED — add row "Variants" button)
```

## VariantsList Layout

Flex full-height pattern (`flex flex-col h-full p-4`). DataTable `#header` with:

- Left: FloatLabel "Search" + IconField InputIcon pi-search + InputText + "Clear" button (outlined)
- Right: "New Variant" (primary, pi-plus) + "Reload" (secondary, pi-sync)

Product filter: applied via `usePagedQuery` initial filter `productId=xxx` from
route query param. If no productId is present, show all variants for all
products.

Columns:

| Column | Source | Notes |
|--------|--------|-------|
| Master | `isMaster` | Tag badge, info severity if true |
| SKU | `sku` | sortable, filterable |
| Position | `position` | sortable |
| Price | `price` | formatted with costCurrency |
| Actions | — | Edit (pencil, navigate to detail), Delete (trash, confirm dialog) |

Checkbox selection: none (single-row delete only). No batch delete (follows the
toolbar-consolidated pattern). No Export button.

## VariantDetail Layout

Flex full-height pattern. Top bar: title ("New Variant" / "Edit Variant") +
description + Save/Cancel buttons. Form `id="variant-form"`, Save button
`form="variant-form" type="submit"`.

### General Tab

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| SKU | InputText | yes | max 255 |
| Position | InputNumber | no | min -1, default 0 |
| IsMaster | ToggleSwitch | no | default false |
| TrackInventory | ToggleSwitch | no | default true |

### Physical Tab

| Field | Type | Constraints |
|-------|------|-------------|
| Weight | InputNumber | min 0 |
| WeightUnit | Select | g, kg, lb, oz |
| Height | InputNumber | min 0 |
| Width | InputNumber | min 0 |
| Depth | InputNumber | min 0 |
| DimensionsUnit | Select | in, cm, mm |

### Pricing Tab

Section 1 — inline form fields (part of VariantRequest submitted with the form):

| Field | Type | Constraints |
|-------|------|-------------|
| Price | InputNumber | min 0, scale 2 |
| CostPrice | InputNumber | min 0, scale 2 |
| CostCurrency | InputText | max 3 chars (ISO 4217) |

Section 2 — Price History (edit mode only, below form fields):

- DataTable: Amount, Currency, CompareAtAmount, CountryIso, Remove button
- "Add Price" button opens dialog with fields: Amount, Currency (required),
  CompareAtAmount, CountryIso
- Remove triggers confirm dialog, calls `variantPriceApi.remove()`
- Component manages its own load/refresh independently of the main form

APIs: `GET variants/{id}/prices` (list), `POST variants/{id}/prices` (upsert),
`DELETE variants/{id}/prices/{priceId}` (remove).

### Images Tab (edit mode only)

- "Upload" button opens file picker (JPEG/PNG/GIF/WebP, max 10 MB)
- Grid of image cards: thumbnail, type badge, Position input, Alt input,
  Delete button
- Upload: `FormData` multipart POST to `variants/{id}/images`
- Delete: confirm dialog ("This permanently deletes the image"), calls
  `DELETE variants/images/{imageId}`

### Option Values Tab (edit mode only)

- Groups option values by OptionType name
- Each group: section header, checkbox list of option values
- Checked = assigned, unchecked = not assigned
- On save: compute diff of checked IDs vs previously-assigned IDs, call
  `assign` for new ones and `revoke` for removed ones
- Load: `GET variants/{id}/option-values` returns all values with
  `isAssigned` flag

## API Services

### variantApi.ts

| Method | Endpoint | Method |
|--------|----------|--------|
| getVariants(productId) | `api/catalog/products/{productId}/variants` | GET |
| getVariant(id) | `api/catalog/variants/{id}` | GET |
| createVariant(productId, req) | `api/catalog/products/{productId}/variants` | POST |
| updateVariant(id, req) | `api/catalog/variants/{id}` | PUT |
| deleteVariant(id) | `api/catalog/variants/{id}` | DELETE |

### variantImageApi.ts

| Method | Endpoint | Method |
|--------|----------|--------|
| listImages(variantId) | `api/catalog/variants/{variantId}/images` | GET |
| uploadImage(variantId, fd) | `api/catalog/variants/{variantId}/images` | POST multipart |
| deleteImage(imageId) | `api/catalog/variants/images/{imageId}` | DELETE |

### variantPriceApi.ts

| Method | Endpoint | Method |
|--------|----------|--------|
| listPrices(variantId) | `api/catalog/variants/{variantId}/prices` | GET |
| setPrice(variantId, req) | `api/catalog/variants/{variantId}/prices` | POST (upsert) |
| removePrice(variantId, priceId) | `api/catalog/variants/{variantId}/prices/{priceId}` | DELETE |

## TypeScript Types

```typescript
interface VariantParameters {
  sku: string
  position: number
  trackInventory: boolean
  weight?: number
  weightUnit?: string
  height?: number
  width?: number
  depth?: number
  dimensionsUnit?: string
  price?: number
  costPrice?: number
  costCurrency?: string
}

interface VariantRequest extends VariantParameters {
  isMaster: boolean
  optionValueIds?: string[]
}

interface Variant extends VariantParameters {
  id: string
  productId: string
  isMaster: boolean
  discontinuedOn?: string
  pricesCount: number
}

interface VariantImage {
  id: string
  variantId: string
  url: string
  contentType: string
  fileName: string
  fileSize: number
  width?: number
  height?: number
  alt?: string
  position: number
  type: 'Default' | 'Thumbnail' | 'Square' | 'Gallery' | 'Search'
  createdAtUtc: string
}

interface Price {
  id: string
  variantId: string
  amount?: number
  currency: string
  compareAtAmount?: number
  countryIso?: string
}

interface OptionValueAssignment {
  optionValueId: string
  optionTypeId: string
  optionTypeName: string
  name: string
  presentation: string
  isAssigned: boolean
}
```

Filter/sort constants: `VARIANT_FILTER_FIELDS`, `VARIANT_SORT_FIELDS`.

## Error Handling

| Scenario | Code | Frontend behavior |
|----------|------|-------------------|
| SKU already exists | 409 | Error toast, keep form open |
| Product not found | 404 | Error toast, navigate to products |
| Variant not found | 404 | Error toast, navigate to variants |
| Validation errors | 400 | Field-level via Form resolver |
| Image > 10 MB | 400 | Toast: "File must be under 10 MB" |
| Image bad format | 400 | Toast: "Allowed: JPEG, PNG, GIF, WebP" |
| Delete conflict | 409/404 | Toast with backend message |
| Network/unknown | — | "Something went wrong" toast |

Image deletion uses confirm: "This permanently deletes the image. Continue?"
Variant deletion uses standard `useConfirm()` dialog.

## Out of Scope

- Image embedding management (Create/Regenerate endpoints)
- Batch variant operations
- Variant search-by-image or similarity
- Product name display in variants list (requires backend change)

## Checklist

- [ ] Create `types/variant.ts`
- [ ] Create `services/variantApi.ts`
- [ ] Create `services/variantImageApi.ts`
- [ ] Create `services/variantPriceApi.ts`
- [ ] Create `views/VariantsList.vue`
- [ ] Create `views/VariantDetail.vue`
- [ ] Add 3 routes + menu item to `routes/index.ts`
- [ ] Add "Variants" row button to `ProductsList.vue`
- [ ] Update barrel exports
- [ ] Type-check + lint + unit tests pass
