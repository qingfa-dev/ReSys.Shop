# Catalog Products Admin UI

**Date**: 2026-07-30
**Scope**: Admin SPA — Catalog module, Product CRUD + OptionType + Classification assignment
**Decision**: Full Location-module replication (types, services, stores, validations, views). 6-tab Product detail form (General, SEO, Fashion, Timing, Option Types, Classifications). Dual-panel PrimeVue PickList for OptionType and Classification assignment using Sync endpoints.

## Motivation

- Catalog backend has 15 admin endpoints for Products (7), Product OptionTypes (4), and Product Classifications (4), but the Admin SPA has only stub views.
- Products have 17 form fields across 5 logical groups, warranting tabbed organization.
- OptionTypes and Classifications use a multi-select assignment pattern with an `isAssigned` flag — PrimeVue PickList is the natural component for dual-panel selection.
- The Location/OptionTypes/Taxonomies modules establish a proven layered pattern (types → services → stores → validations → views) that keeps each file small, focused, and testable.

## Architecture

### File Structure

All new files under `app/Admin/src/features/catalog/`. No modifications to shared components or composables.

```
catalog/
├── types/
│   ├── product.ts              (new)
│   └── index.ts                (modify — add product exports, keep existing)
├── services/
│   ├── productApi.ts           (new — 7 methods)
│   ├── productOptionTypeApi.ts (new — 2 methods: get, sync)
│   ├── productClassificationApi.ts (new — 2 methods: get, sync)
│   └── index.ts                (modify — add 3 exports, keep existing)
├── stores/
│   ├── productStore.ts         (new — Pinia dropdown cache)
│   └── index.ts                (modify — add export, keep existing)
├── validations/
│   ├── product.ts              (new — Zod: 17 fields across 5 groups)
│   └── index.ts                (modify — add exports, keep existing)
├── views/
│   ├── ProductsList.vue        (modify — replace stub)
│   ├── ProductDetail.vue       (modify — replace stub, 6-tab form + PickLists)
│   └── index.ts                (modify — add exports if missing, keep existing)
├── __tests__/
│   ├── types/
│   │   └── product.spec.ts     (new)
│   ├── services/
│   │   └── productApi.spec.ts  (new)
│   └── validations/
│       └── product.spec.ts     (new)
└── routes/
    └── index.ts                (no changes — already wired)
```

### Existing Files to Replace

| File | Current | New |
|------|---------|-----|
| `views/ProductsList.vue` | Stub (`<PageShell>` wrapper) | Full DataTable page |
| `views/ProductDetail.vue` | Stub (`<PageShell>` wrapper) | 6-tab form + PickLists |

Routes in `features/catalog/routes/index.ts` and menu items in `AppMenu.vue` are already wired and need no changes:
- `catalog/products` → ProductsList (already defined)
- `catalog/products/:id` → ProductDetail (already defined)
- `catalog-products` → ProductsList (name already defined)
- `catalog-product-detail` → ProductDetail (name already defined)

---

## Types Layer

### product.ts

```typescript
import type { QueryingParameters } from '@/shared/types/querying'

export interface ProductRequest {
  name: string
  slug: string
  description: string | null
  metaTitle: string | null
  metaDescription: string | null
  metaKeywords: string | null
  availableOn: string | null
  discontinueOn: string | null
  trackInventory: boolean
  styleCode: string | null
  seasonName: string | null
  materialComposition: string | null
  careInstructions: string | null
  fitNotes: string | null
  department: string | null
  genderTarget: string | null
}

export interface ProductListItem extends ProductRequest {
  id: string
  status: 'Draft' | 'Active' | 'Archived'
  masterVariantId: string
  variantsCount: number
  createdAtUtc: string
  modifiedAtUtc: string | null
}

export type ProductDetail = ProductListItem

export interface ProductQuery {
  status?: 'Draft' | 'Active' | 'Archived'
  season?: string
  taxonId?: string
  search?: string
  sortBy?: 'name' | 'createdAtUtc' | 'modifiedAtUtc' | 'availableOn' | 'variantsCount'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const PRODUCT_FILTER_FIELDS = [
  'status',
  'seasonName',
  'department',
  'createdAtUtc',
  'availableOn',
]

export const PRODUCT_SORT_FIELDS = [
  'name',
  'createdAtUtc',
  'modifiedAtUtc',
  'availableOn',
  'variantsCount',
]

export function toProductQueryParams(query: ProductQuery): QueryingParameters {
  const filters: string[] = []

  if (query.status !== undefined && query.status !== '') {
    filters.push(`status=${query.status}`)
  }
  if (query.season !== undefined && query.season !== '') {
    filters.push(`seasonName*=${query.season}`)
  }
  if (query.taxonId !== undefined && query.taxonId !== '') {
    filters.push(`taxonId=${query.taxonId}`)
  }

  let sort: string[] | null = null
  if (query.sortBy) {
    const dir = query.sortDirection === 'desc' ? '-' : ''
    sort = [`${dir}${query.sortBy}`]
  }

  return {
    filter: filters.length > 0 ? filters.join(',') : null,
    search: query.search ?? null,
    sort,
    pageNumber: query.page ?? null,
    pageSize: query.pageSize ?? null,
  }
}
```

**OptionTypes and Classifications types** are defined inline in their service files (no separate type files needed — the response shapes are simple lists):

```typescript
// In productOptionTypeApi.ts
export interface OptionTypeAssignment {
  optionTypeId: string
  name: string
  presentation: string | null
  position: number
  isAssigned: boolean
}

// In productClassificationApi.ts
export interface ClassificationAssignment {
  taxonId: string
  name: string
  prettyName: string | null
  position: number
  isAssigned: boolean
}
```

**Filter DSL (converter):**
- `status=Active` → exact match on ProductStatus enum
- `seasonName*=Summer` → contains match on season name
- `taxonId=:guid` → exact match on classification taxon ID
- `department*=Men` → contains match (if needed later)
- Sort: prefix `-` for desc, no prefix for asc
- Search: uses backend's allowed search fields (name, description, slug, styleCode, seasonName, department, genderTarget)

### types/index.ts (barrel)

Add after existing exports:
```typescript
export type {
  ProductRequest,
  ProductListItem,
  ProductDetail,
  ProductQuery,
} from './product'
export {
  PRODUCT_FILTER_FIELDS,
  PRODUCT_SORT_FIELDS,
  toProductQueryParams,
} from './product'
```

---

## Services Layer

### ProductApi

Static class following `OptionTypeApi`/`TaxonomyApi` pattern.

Base: `${CATALOG}/products`

| Method | HTTP | URL | Response |
|--------|------|-----|----------|
| `getProducts(query)` | GET | `/api/catalog/products` | `PagedResult<ProductListItem>` |
| `getProduct(id)` | GET | `/api/catalog/products/{id}` | `Result<ProductDetail>` |
| `createProduct(req)` | POST | `/api/catalog/products` | `Result<ProductDetail>` |
| `updateProduct(id, req)` | PUT | `/api/catalog/products/{id}` | `Result<ProductDetail>` |
| `deleteProduct(id)` | DELETE | `/api/catalog/products/{id}` | `Result<ProductListItem>` |
| `activateProduct(id)` | POST | `/api/catalog/products/{id}/activate` | `Result<void>` |
| `discontinueProduct(id)` | POST | `/api/catalog/products/{id}/discontinue` | `Result<void>` |

**getProducts** passes `PRODUCT_FILTER_FIELDS` and `PRODUCT_SORT_FIELDS` to `getPaged()` for server-side whitelist enforcement.

**createProduct** and **updateProduct** send `ProductRequest` as JSON body. The backend treats all fields as required in the schema but the update handler coalesces missing fields from the existing entity.

**activateProduct** and **discontinueProduct** are POST requests with no body, returning `Result<void>`.

### ProductOptionTypeApi

Base: `${CATALOG}/products/{id}/option-types`

| Method | HTTP | URL | Response |
|--------|------|-----|----------|
| `getOptionTypes(productId)` | GET | `/api/catalog/products/{id}/option-types` | `Result<{ items: OptionTypeAssignment[] }>` |
| `syncOptionTypes(productId, items)` | POST | `/api/catalog/products/{id}/option-types/sync` | `Result<void>` |

**syncOptionTypes request body:**
```json
{
  "items": [
    { "optionTypeId": "guid", "position": 0 },
    { "optionTypeId": "guid", "position": 1 }
  ]
}
```

The Sync endpoint accepts the full desired set — it creates missing assignments and removes obsolete ones in a single call.

### ProductClassificationApi

Base: `${CATALOG}/products/{id}/classifications`

| Method | HTTP | URL | Response |
|--------|------|-----|----------|
| `getClassifications(productId)` | GET | `/api/catalog/products/{id}/classifications` | `Result<{ items: ClassificationAssignment[] }>` |
| `syncClassifications(productId, items)` | POST | `/api/catalog/products/{id}/classifications/sync` | `Result<void>` |

**syncClassifications request body:**
```json
{
  "items": [
    { "taxonId": "guid", "position": 0 },
    { "taxonId": "guid", "position": 1 }
  ]
}
```

Same sync semantics: full desired set replaces existing.

### services/index.ts (barrel)

Add after existing exports:
```typescript
export { ProductApi } from './productApi'
export { ProductOptionTypeApi } from './productOptionTypeApi'
export { ProductClassificationApi } from './productClassificationApi'
```

---

## Store Layer

### productStore (Pinia)

```typescript
import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { ProductListItem } from '../types/product'
import { ProductApi } from '../services/productApi'

export const useProductStore = defineStore('products', () => {
  const activeProducts = ref<ProductListItem[]>([])
  const loaded = ref(false)

  async function fetchActive(): Promise<void> {
    if (loaded.value) return

    const result = await ProductApi.getProducts({
      status: 'Active',
      pageSize: 100,
      sortBy: 'name',
      sortDirection: 'asc',
    })

    if (result.isSuccess) {
      activeProducts.value = result.items
      loaded.value = true
    }
  }

  return { activeProducts, loaded, fetchActive }
})
```

Lazy-once pattern identical to `useOptionTypeStore` and `useTaxonomyStore`. Used by future cross-sell/related-product pickers.

### stores/index.ts (barrel)

Add after existing exports:
```typescript
export { useProductStore } from './productStore'
```

---

## Validations Layer

### product.ts (Zod)

```typescript
import { z } from 'zod'

export const productName = z.string()
  .min(1, 'Product name is required.')
  .max(255, 'Product name must not exceed 255 characters.')

export const productSlug = z.string()
  .min(1, 'Slug is required.')
  .max(255, 'Slug must not exceed 255 characters.')
  .regex(/^[a-z0-9]+(?:-[a-z0-9]+)*$/, 'Slug must be lowercase alphanumeric with hyphens.')

export const productDescription = z.string()
  .max(2000, 'Description must not exceed 2000 characters.')
  .nullable()
  .optional()

export const productMetaTitle = z.string()
  .max(100, 'Meta title must not exceed 100 characters.')
  .nullable()
  .optional()

export const productMetaDescription = z.string()
  .max(255, 'Meta description must not exceed 255 characters.')
  .nullable()
  .optional()

export const productMetaKeywords = z.string()
  .max(255, 'Meta keywords must not exceed 255 characters.')
  .nullable()
  .optional()

export const productAvailableOn = z.string()
  .nullable()
  .optional()

export const productDiscontinueOn = z.string()
  .nullable()
  .optional()

export const productTrackInventory = z.boolean()

export const productStyleCode = z.string()
  .max(50, 'Style code must not exceed 50 characters.')
  .nullable()
  .optional()

export const productSeasonName = z.string()
  .max(50, 'Season name must not exceed 50 characters.')
  .nullable()
  .optional()

export const productMaterialComposition = z.string()
  .max(500, 'Material composition must not exceed 500 characters.')
  .nullable()
  .optional()

export const productCareInstructions = z.string()
  .max(500, 'Care instructions must not exceed 500 characters.')
  .nullable()
  .optional()

export const productFitNotes = z.string()
  .max(500, 'Fit notes must not exceed 500 characters.')
  .nullable()
  .optional()

export const productDepartment = z.string()
  .max(50, 'Department must not exceed 50 characters.')
  .nullable()
  .optional()

export const productGenderTarget = z.string()
  .max(20, 'Gender target must not exceed 20 characters.')
  .nullable()
  .optional()

export const productSchema = z.object({
  name: productName,
  slug: productSlug,
  description: productDescription,
  metaTitle: productMetaTitle,
  metaDescription: productMetaDescription,
  metaKeywords: productMetaKeywords,
  availableOn: productAvailableOn,
  discontinueOn: productDiscontinueOn,
  trackInventory: productTrackInventory,
  styleCode: productStyleCode,
  seasonName: productSeasonName,
  materialComposition: productMaterialComposition,
  careInstructions: productCareInstructions,
  fitNotes: productFitNotes,
  department: productDepartment,
  genderTarget: productGenderTarget,
})

export type ProductForm = z.infer<typeof productSchema>
```

### validations/index.ts (barrel)

Add after existing exports (keep all optionType/optionValue/taxonomy/taxon/taxonRule exports):
```typescript
export {
  productName,
  productSlug,
  productDescription,
  productSchema,
} from './product'
export type { ProductForm } from './product'
```

---

## Views

### ProductsList.vue

Same pattern as `OptionTypesList.vue`:
- Composable stack: `useRouter`, `useConfirm`, `useNotify`, `useDataTableExport`, `usePagedQuery`
- `selectedItems` ref, `searchTerm` ref
- Card + Toolbar (New + Delete + Export) + DataTable with checkbox column + header search
- Columns: name (sortable, filterable), slug, status (Tag with severity: Draft=info, Active=success, Archived=danger), department, seasonName, variantsCount, createdAtUtc, actions
- Search whitelist: `['name', 'slug']`
- Status filter: Select dropdown in toolbar or as a DataTable column filter
- Delete: `selectedItems` loop → sequential single deletes → partial-failure notice
- CSV export via `useDataTableExport`
- Action column: Edit (pencil, navigates to `/catalog/products/:id`) + Delete (trash)

```vue
<!-- Key DataTable columns -->
<Column field="name" header="Name" :sortable="true" :filter="true" filter-field="name" />
<Column field="slug" header="Slug" :sortable="true" />
<Column field="status" header="Status" :sortable="true" :filter="true" filter-field="status" body-style="text-align: center">
  <template #body="{ data }">
    <Tag :value="data.status" :severity="data.status === 'Active' ? 'success' : data.status === 'Draft' ? 'info' : 'danger'" />
  </template>
</Column>
<Column field="department" header="Department" :sortable="true" />
<Column field="seasonName" header="Season" :sortable="true" />
<Column field="variantsCount" header="Variants" :sortable="true" body-style="text-align: center" />
<Column field="createdAtUtc" header="Created" :sortable="true" />
```

### ProductDetail.vue

6-tab form page:
- Composable stack: `useRoute`, `useRouter`, `useNotify`, `useConfirm`, `useApiErrorHandler`
- `isEdit` computed from `route.params.id !== 'new'` (same fix as OptionTypeDetail)
- `ProductForm` ref, `fieldErrors` ref
- On mount (edit): fetch product → populate form. On mount (create): empty form.
- On save: Zod parse → create or update → notify → redirect (create: `router.replace` to edit URL + watch trigger).
- After create: `router.replace` to edit URL, `watch(route.params.id)` triggers `initEditMode`.

Tab layout:
```
PageShell
  PageHeading: "Products" > "{Name or New}"
    actions: [Save] [Cancel]
  Tabs (v-model:value = activeTab)
    Tab "General" (value="0")
      FormSection "Product Details"
        name, slug, description (textarea), status (Select: Draft/Active/Archived)

    Tab "SEO" (value="1")
      FormSection "Search Engine Optimization"
        metaTitle, metaDescription (textarea), metaKeywords

    Tab "Fashion" (value="2")
      FormSection "Fashion Attributes"
        styleCode, seasonName, department, genderTarget
        materialComposition (textarea), careInstructions (textarea), fitNotes (textarea)

    Tab "Timing" (value="3")
      FormSection "Availability"
        availableOn (date), discontinueOn (date), makeActiveAt (date)
        trackInventory (ToggleSwitch)

    Tab "Option Types" (value="4", v-if="isEdit")
      PickList (PrimeVue):
        sourceHeader="Available", targetHeader="Assigned"
        :source="unassignedOptionTypes"   // isAssigned=false
        :target="assignedOptionTypes"     // isAssigned=true
        @move-to-target / @move-all-to-target / @move-to-source / @move-all-to-source
        Save Option Types button → syncOptionTypes()

    Tab "Classsifications" (value="5", v-if="isEdit")
      PickList (PrimeVue):
        sourceHeader="Unassigned", targetHeader="Assigned"
        :source="unassignedClassifications"
        :target="assignedClassifications"
        Save Classifications button → syncClassifications()
```

### PickList Data Flow

**On Tab Activation (Option Types):**
1. `onMounted` or `watch(activeTab)`: call `ProductOptionTypeApi.getOptionTypes(productId)`
2. Split response: `unassigned = items.filter(i => !i.isAssigned)`
3. Split response: `assigned = items.filter(i => i.isAssigned)`
4. PickList renders two panels

**On Save (Option Types):**
1. Collect `assigned` array from PickList target
2. Map to sync items: `assigned.map((a, i) => ({ optionTypeId: a.optionTypeId, position: i }))`
3. Call `ProductOptionTypeApi.syncOptionTypes(productId, items)`
4. On success: toast, re-fetch (in case new option types were created elsewhere)

**Same pattern for Classifications** using `ProductClassificationApi`.

### PickList Configuration

```vue
<PickList
  v-model:model="assignedOptionTypes"
  :source="unassignedOptionTypes"
  :target="assignedOptionTypes"
  source-header="Available"
  target-header="Assigned"
  filter
  filter-placeholder="Search..."
  list-style="height: 300px"
  @move-to-target="onMoveToTarget"
  @move-to-source="onMoveToSource"
>
  <template #item="{ item }">
    <div>{{ item.name }}</div>
  </template>
</PickList>
```

Note: The PickList manages `source` and `target` arrays. When items move, `assignedOptionTypes` is updated in-place and `unassignedOptionTypes` is updated via the source sync. We track changes and save only when the Save button is clicked.

### initEditMode + watch pattern

Same initEditMode/watch pattern as OptionTypeDetail:
```typescript
const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')

async function initEditMode(id: string) {
  const result = await ProductApi.getProduct(id)
  if (result.isSuccess) {
    const p = result.value
    form.value = { name: p.name, slug: p.slug, ...all fields }
  } else {
    handleResult(result)
    router.push('/catalog/products')
  }
}

onMounted(() => {
  if (isEdit.value) {
    initEditMode(route.params.id as string)
  }
})

watch(() => route.params.id, (newId) => {
  if (newId && newId !== 'new') {
    initEditMode(newId as string)
  }
})
```

---

## Edge Cases

| Scenario | Behavior |
|----------|----------|
| Create product | OptionTypes + Classifications tabs hidden (no productId). User saves → `router.replace` to edit URL → watch fires initEditMode → tabs appear. |
| Delete product with variants | Backend constraint: returns error. Display toast with message. Entity remains in list. |
| Activate product | POST to `/api/catalog/products/{id}/activate`. On success: refresh list or navigate to edit to see updated status. |
| Discontinue product | POST to `/api/catalog/products/{id}/discontinue`. Same flow as activate. |
| Duplicate slug | Backend returns validation error. Show error toast. Form stays open with data intact. |
| Duplicate product name | Backend returns conflict error. Show error toast. |
| PickList empty source | Shows "All option types assigned" message below the source panel (PickList handles this natively). |
| PickList empty target | Shows "No option types assigned" message. Save button sends empty array → sync removes all. |
| Concurrent edit | Backend returns error. Form stays open, toast shown. |
| Nullable fields | Description, meta fields, fashion fields, and datetimes are all nullable. Form sends `null` for empty values. Zod uses `.nullable().optional()`. |
| Status dropdown values | `Draft`, `Active`, `Archived` — matching `ProductStatus` enum string values. |

## Testing Strategy

### Unit Tests (Vitest)

| File | What to test |
|------|-------------|
| `__tests__/types/product.spec.ts` | `toProductQueryParams` produces correct DSL (status exact, seasonName contains, taxonId exact, sort, pagination). `PRODUCT_FILTER_FIELDS` and `PRODUCT_SORT_FIELDS` contain expected values. |
| `__tests__/services/productApi.spec.ts` | All 7 methods (getProducts, getProduct, createProduct, updateProduct, deleteProduct, activateProduct, discontinueProduct) call correct HTTP methods and URLs with mocked post/get/put/del/getPaged. |
| `__tests__/validations/product.spec.ts` | Zod: required fields (name, slug) rejected when empty. Max lengths enforced (255, 50, 20, 2000, etc.). Slug regex: rejects uppercase/spaces/special chars, accepts valid hyphenated slugs. Nullable fields accept null/undefined. Full schema test with valid and invalid data. |

### Manual Verification

- Products list: search, sort, filter by status/season, pagination, CSV export
- Product create: navigate via New button, fill form, save, verify redirect to edit page, tabs appear
- Product edit: navigate from list, modify fields across tabs, save, verify
- Product delete: single and multi-select, confirm dialog, verify removed
- Activate/discontinue: click button, verify status change
- OptionTypes: PickList select/deselect, save, verify assignment persists
- Classifications: PickList select/deselect, save, verify assignment persists
- Tab navigation: switch tabs, form state preserved across all 6 tabs
- Breadcrumbs and back navigation correct

## Constraints

- Must pass `pnpm run lint` and `pnpm run build-only` with zero errors
- Must pass all existing 537 tests (no regressions)
- No new npm dependencies — reuse existing PrimeVue (PickList, Tabs, DataTable, etc.), Zod, Pinia, Vitest
- Routes already wired — no changes to `catalog/routes/index.ts`
- `catalog-products` and `catalog-product-detail` route names already defined
- `isEdit` must exclude `route.params.id === 'new'` (same fix as OptionTypeDetail)
- Follow existing conventions: no comments, static API classes, Zod individual + combined schema, Pinia loaded guard, inline PrimeVue components
- `ProductApi.activateProduct` and `ProductApi.discontinueProduct` return `Result<void>` — check via `result.isSuccess`, no `.value`

## Backend API Reference

| Method | URL | Permission |
|--------|-----|------------|
| GET | `/api/catalog/products` | `Products.List` |
| POST | `/api/catalog/products` | `Products.Create` |
| GET | `/api/catalog/products/{id}` | `Products.List` |
| PUT | `/api/catalog/products/{id}` | `Products.Update` |
| DELETE | `/api/catalog/products/{id}` | `Products.Delete` |
| POST | `/api/catalog/products/{id}/activate` | `Products.Manage` |
| POST | `/api/catalog/products/{id}/discontinue` | `Products.Manage` |
| GET | `/api/catalog/products/{id}/option-types` | `ProductsOptionTypes.Read` |
| POST | `/api/catalog/products/{id}/option-types/sync` | `ProductsOptionTypes.Sync` |
| GET | `/api/catalog/products/{id}/classifications` | `ProductsClassifications.Read` |
| POST | `/api/catalog/products/{id}/classifications/sync` | `ProductsClassifications.Sync` |
