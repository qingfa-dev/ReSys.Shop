# Catalog Products — Admin UI Design

**Date**: 2026-07-30
**Scope**: Admin SPA — Catalog module, Product CRUD + OptionTypes + Classifications assignment
**Decision**: Full Location-module replication (types, services, stores, validations, views). 6-tab Product detail form (General, SEO, Fashion, Timing, Option Types, Classifications). Dual-panel PickList for OptionType and Classification assignment using Sync endpoints.

## Motivation

- Catalog backend has 15 admin endpoints for Products (7), Product OptionTypes (4), and Product Classifications (4), but the Admin SPA has only stub views.
- Products have ~20 fields across 5 logical groups, warranting tabbed organization.
- OptionTypes and Classifications use a multi-select assignment pattern — a PickList is the natural PrimeVue component for this.
- The Location/OptionTypes/Taxonomies modules have established a proven layered pattern.

## Architecture

### File Structure

All new files under `app/Admin/src/features/catalog/`. No modifications to shared components or composables.

```
catalog/
├── types/
│   ├── product.ts              (new)
│   └── index.ts                (modify — add product exports)
├── services/
│   ├── productApi.ts           (new — 7 methods)
│   ├── productOptionTypeApi.ts (new — 2 methods)
│   ├── productClassificationApi.ts (new — 2 methods)
│   └── index.ts                (modify)
├── stores/
│   ├── productStore.ts         (new — Pinia dropdown cache)
│   └── index.ts                (modify)
├── validations/
│   ├── product.ts              (new — Zod ~20 fields)
│   └── index.ts                (modify)
├── views/
│   ├── ProductsList.vue        (modify — replace stub)
│   ├── ProductDetail.vue       (modify — replace stub, 6-tab form + PickLists)
│   └── index.ts                (modify)
├── __tests__/
│   ├── types/
│   │   └── product.spec.ts
│   ├── services/
│   │   └── productApi.spec.ts
│   └── validations/
│       └── product.spec.ts
└── routes/
    └── index.ts                (no changes — already wired)
```

### Routes

Already wired — no changes needed:
- `GET /catalog/products` → ProductsList
- `GET /catalog/products/:id` → ProductDetail (create via `/catalog/products/new`)

---

## Types Layer

### product.ts

```typescript
interface ProductRequest {
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

interface ProductListItem extends ProductRequest {
  id: string
  status: 'Draft' | 'Active' | 'Archived'
  masterVariantId: string
  variantsCount: number
  createdAtUtc: string
  modifiedAtUtc: string | null
}

type ProductDetail = ProductListItem

interface ProductQuery {
  status?: 'Draft' | 'Active' | 'Archived'
  season?: string
  taxonId?: string
  search?: string
  sortBy?: 'name' | 'createdAtUtc' | 'modifiedAtUtc' | 'availableOn' | 'variantsCount'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

const PRODUCT_FILTER_FIELDS = ['status', 'seasonName', 'department', 'createdAtUtc', 'availableOn']
const PRODUCT_SORT_FIELDS = ['name', 'createdAtUtc', 'modifiedAtUtc', 'availableOn', 'variantsCount']

function toProductQueryParams(query: ProductQuery): QueryingParameters
```

Filter DSL: `status=Active` (exact), `seasonName*=Summer` (contains).
Sort: prefix `-` for desc.

---

## Services Layer

### ProductApi

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

### ProductOptionTypeApi

Base: `${CATALOG}/products`

| Method | HTTP | URL | Response |
|--------|------|-----|----------|
| `getOptionTypes(productId)` | GET | `/api/catalog/products/{id}/option-types` | `Result<{ items: OptionTypeAssignment[] }>` |
| `syncOptionTypes(productId, items)` | POST | `/api/catalog/products/{id}/option-types/sync` | `Result<void>` |

Where `OptionTypeAssignment = { optionTypeId: string, name: string, presentation: string | null, position: number, isAssigned: boolean }`.

### ProductClassificationApi

Base: `${CATALOG}/products`

| Method | HTTP | URL | Response |
|--------|------|-----|----------|
| `getClassifications(productId)` | GET | `/api/catalog/products/{id}/classifications` | `Result<{ items: ClassificationAssignment[] }>` |
| `syncClassifications(productId, items)` | POST | `/api/catalog/products/{id}/classifications/sync` | `Result<void>` |

Where `ClassificationAssignment = { taxonId: string, name: string, prettyName: string | null, position: number, isAssigned: boolean }`.

---

## Store Layer

### productStore (Pinia)

```
useProductStore()
  activeProducts: Ref<ProductListItem[]>
  loaded: Ref<boolean>
  fetchActive(): Promise<void>  — calls getProducts({ status: 'Active', pageSize: 100, sortBy: 'name' })
```

Lazy-once pattern identical to `useOptionTypeStore`. Used by future cross-sell/related-product pickers.

---

## Validations Layer

### product.ts (Zod)

| Field | Rule |
|-------|------|
| `name` | string, min 1, max 255 |
| `slug` | string, min 1, max 255, regex `/^[a-z0-9]+(?:-[a-z0-9]+)*$/` |
| `description` | string, max 2000, nullable |
| `metaTitle` | string, max 100, nullable |
| `metaDescription` | string, max 255, nullable |
| `metaKeywords` | string, max 255, nullable |
| `availableOn` | string, nullable |
| `discontinueOn` | string, nullable |
| `trackInventory` | boolean, default true |
| `styleCode` | string, max 50, nullable |
| `seasonName` | string, max 50, nullable |
| `materialComposition` | string, max 500, nullable |
| `careInstructions` | string, max 500, nullable |
| `fitNotes` | string, max 500, nullable |
| `department` | string, max 50, nullable |
| `genderTarget` | string, max 20, nullable |

Combined `productSchema`, inferred `ProductForm` type. Nullable fields use `.nullable().optional()`.

---

## Views

### ProductsList.vue

Same pattern as OptionTypesList/TaxonomiesList:
- `usePagedQuery` + `useDataTableExport` + `useConfirm`
- Card + Toolbar (New, Delete, Export)
- DataTable columns: checkbox, name, slug, status (Tag), department, season, variantsCount, createdAtUtc, actions
- Status filter dropdown in toolbar (All, Draft, Active, Archived)
- Search: name + slug
- Multi-select delete via sequential single deletes
- Action column: edit + delete per row

### ProductDetail.vue

6-tab form page:
- `isEdit` computed from `route.params.id !== 'new'`
- On mount (edit): fetch product detail → populate all form fields
- On save: Zod parse → create or update → notify + redirect
- After create: `router.replace` to edit URL + init edit mode (same pattern as OptionTypeDetail)

```
Tab "General":
  name, slug, description (textarea), status (Select: Draft/Active/Archived)

Tab "SEO":
  metaTitle, metaDescription (textarea), metaKeywords

Tab "Fashion":
  styleCode, seasonName, department, genderTarget,
  materialComposition (textarea), careInstructions (textarea), fitNotes (textarea)

Tab "Timing":
  availableOn (datetime), discontinueOn (datetime), makeActiveAt (datetime),
  trackInventory (ToggleSwitch)

Tab "Option Types" (hidden on create):
  PickList: Source (unassigned) → Target (assigned)
  Save button synces the target list

Tab "Classifications" (hidden on create):
  PickList: Source (unassigned taxons) → Target (assigned taxons)
  Save button synces the target list
```

**PickList data flow:**
1. On tab activation: fetch `getOptionTypes(productId)` / `getClassifications(productId)`
2. Split response into source (isAssigned=false) and target (isAssigned=true)
3. User moves items between panels via PickList buttons/drag
4. On save: collect target items, assign sequential positions, call sync endpoint

---

## Edge Cases

| Scenario | Behavior |
|----------|----------|
| Create product | OptionTypes + Classifications tabs hidden until saved (no productId) |
| Create product redirect | `router.replace` to edit URL, tabs become visible |
| Delete product with variants | Backend constraint — show error toast |
| Activate product | From list or detail, POST activate → refresh |
| Discontinue product | From list or detail, POST discontinue → refresh |
| Duplicate slug | Backend validation error — show toast |
| PickList empty source | "All option types assigned" message |
| PickList empty target | "No option types assigned" message |

---

## Testing Strategy

| File | What to test |
|------|-------------|
| `types/product.spec.ts` | `toProductQueryParams` DSL, const arrays |
| `services/productApi.spec.ts` | 7 methods: HTTP verb + URL (mock post/get/put/del + getPaged + patch) |
| `validations/product.spec.ts` | Zod: required fields, max lengths, slug regex, nullable handling |

All 537 existing tests must pass. No new lint errors.

## Constraints

- Must pass `pnpm run lint` and `pnpm run build-only` with zero errors
- No new npm dependencies
- Routes already wired — no changes to `routes/index.ts`
- `isEdit` excludes `route.params.id === 'new'`
- PickList uses `v-model:model` PrimeVue component

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
