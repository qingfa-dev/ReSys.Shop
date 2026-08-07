# Store SPA Rebuild Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rebuild the Store SPA data layer from the ground up — types, Zod validations, API services, Pinia stores, skeleton PrimeVue pages — across 8 feature domains with zero UI feature implementation.

**Architecture:** Per-domain co-located layers (types/validations/services/stores/composables/views) following Admin SPA patterns. 15 Pinia stores act as feature plugins coordinating through a typed event bus. Static service classes validate all API responses with Zod.

**Tech Stack:** Vue 3.5, Pinia 4, Zod 4.4, Axios 1.18, PrimeVue 5, Vite 8, TypeScript 6

## Global Constraints

- Types are pure TypeScript interfaces matching backend DTOs exactly — Zod schemas validate at runtime
- Every API service method must call `Schema.parse()` before returning
- Every store action returns `Result<T>`, has `loading` + `error` state
- No `.vue` file imports from `services/` directly — only stores/composables
- Cross-store communication via `useStoreEvents` event bus — never direct imports
- All stores have `_initialized` guard on `init()` — idempotent
- No hardcoded API paths in service files — use `@/shared/constants/api`
- Skeleton pages use PrimeVue Breadcrumb + Card + Skeleton + Message — no feature UI
- Layer barrels (`index.ts`) re-export all public exports from that layer
- Components not in scope — deferred

---

## Phase Ledger

| Phase | Scope | Files Created | Files Modified | Verification |
|-------|-------|---------------|----------------|-------------|
| 1 | Shared foundation | 6 | 0 | `npx tsc --noEmit` |
| 2 | Catalog domain | ~40 | 0 | Tests + typecheck |
| 3 | Identity domain | ~15 | 0 | Tests + typecheck |
| 4 | Ordering domain | ~20 | 0 | Tests + typecheck |
| 5 | Profile domain | ~20 | 0 | Tests + typecheck |
| 6 | Inventory/Payment/Shipping/Location | ~15 | 0 | Tests + typecheck |
| 7 | Layout + router wiring | ~15 | ~3 | Build + dev server |
| 8 | Final verification | 0 | 0 | Full build + lint + tests |

---

## Phase 1: Shared Foundation

### Task 1.1: API Path Constants

**Files:**
- Create: `app/Store/src/shared/constants/api.ts`
- Create: `app/Store/src/shared/constants/index.ts`

**Produces:**
- `STOREFRONT`, `STORE`, `CATALOG`, `IDENTITY`, `ORDERS`, `CART`, `PAYMENT`, `SHIPPING`, `AVAILABILITY`, `PROFILES`, `LOCATIONS` — string constants
- Barrel re-export from `index.ts`

- [ ] **Step 1: Write `api.ts`**

```typescript
// app/Store/src/shared/constants/api.ts
export const STOREFRONT = 'api/storefront'
export const STORE = 'api/store'
export const CATALOG = `${STOREFRONT}`
export const IDENTITY = `${STORE}/identity`
export const PROFILES = `${STORE}/profiles`
export const LOCATIONS = `${STORE}/locations`
export const ORDERS = `${STOREFRONT}/orders`
export const CART = `${STOREFRONT}/cart`
export const PAYMENT = `${STOREFRONT}/payment`
export const SHIPPING = `${STOREFRONT}/shipping`
export const AVAILABILITY = `${STOREFRONT}/availability`
```

- [ ] **Step 2: Write `index.ts`**

```typescript
// app/Store/src/shared/constants/index.ts
export * from './api'
```

- [ ] **Step 3: Verify no compilation errors**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 4: Commit**

```bash
git add app/Store/src/shared/constants/
git commit -m "feat: add API path constants"
```

### Task 1.2: Shared Type Definitions

**Files:**
- Create: `app/Store/src/shared/types/result.ts`
- Create: `app/Store/src/shared/types/error.ts`
- Create: `app/Store/src/shared/types/index.ts`

**Produces:**
- `Result<T>` interface — `{ isSuccess, statusCode, message?, errors[], value: T }`
- `PagedResult<T>` interface — extends Result pattern, adds `{ items: T[], page, pageSize, totalCount, totalPages }`
- `ErrorType` enum, `ApiError` interface, `StatusCode` enum
- Barrel re-export

- [ ] **Step 1: Write `result.ts`**

```typescript
// app/Store/src/shared/types/result.ts
export interface Result<T> {
  isSuccess: boolean
  statusCode: number
  message: string | null
  errors: ApiError[]
  value: T
}

export interface PagedResult<T> {
  isSuccess: boolean
  statusCode: number
  message: string | null
  errors: ApiError[]
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

import type { ApiError } from './error'
```

- [ ] **Step 2: Write `error.ts`**

```typescript
// app/Store/src/shared/types/error.ts
import type { Result } from './result'

export enum StatusCode {
  Ok = 200,
  Created = 201,
  NoContent = 204,
  BadRequest = 400,
  Unauthorized = 401,
  Forbidden = 403,
  NotFound = 404,
  Conflict = 409,
  Validation = 422,
  TooManyRequests = 429,
  InternalServerError = 500,
}

export enum ErrorType {
  BadRequest = 400,
  Unauthorized = 401,
  Forbidden = 403,
  NotFound = 404,
  Conflict = 409,
  Validation = 422,
  Unexpected = 500,
}

export interface ApiError {
  code: string
  message: string
  type: number
  metadata?: Record<string, unknown> | null
}
```

- [ ] **Step 3: Write `index.ts`**

```typescript
// app/Store/src/shared/types/index.ts
export type { Result, PagedResult } from './result'
export { StatusCode, ErrorType } from './error'
export type { ApiError } from './error'
```

- [ ] **Step 4: Verify compilation**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 5: Commit**

```bash
git add app/Store/src/shared/types/
git commit -m "feat: add shared Result<T>, PagedResult<T>, ErrorType types"
```

### Task 1.3: Shared Zod Validations

**Files:**
- Create: `app/Store/src/shared/validations/result.ts`
- Create: `app/Store/src/shared/validations/error.ts`
- Create: `app/Store/src/shared/validations/index.ts`

**Produces:**
- `ResultSchema<T>` — generic Zod schema factory for `Result<T>`
- `PagedResultSchema<T>` — generic Zod schema factory for `PagedResult<T>`
- `ErrorSchema`, `ApiErrorSchema` — Zod schemas for error types
- Barrel re-export

- [ ] **Step 1: Write `result.ts`**

```typescript
// app/Store/src/shared/validations/result.ts
import { z } from 'zod'

export const ApiErrorSchema = z.object({
  code: z.string(),
  message: z.string(),
  type: z.number(),
  metadata: z.record(z.unknown()).nullable().optional(),
})

export function ResultSchema<T extends z.ZodTypeAny>(valueSchema: T) {
  return z.object({
    isSuccess: z.boolean(),
    statusCode: z.number(),
    message: z.string().nullable(),
    errors: z.array(ApiErrorSchema),
    value: valueSchema,
  })
}

export function PagedResultSchema<T extends z.ZodTypeAny>(itemSchema: T) {
  return z.object({
    isSuccess: z.boolean(),
    statusCode: z.number(),
    message: z.string().nullable(),
    errors: z.array(ApiErrorSchema),
    items: z.array(itemSchema),
    page: z.number(),
    pageSize: z.number(),
    totalCount: z.number(),
    totalPages: z.number(),
  })
}
```

- [ ] **Step 2: Write `error.ts`**

```typescript
// app/Store/src/shared/validations/error.ts
export { ApiErrorSchema } from './result'
```

- [ ] **Step 3: Write `index.ts`**

```typescript
// app/Store/src/shared/validations/index.ts
export { ResultSchema, PagedResultSchema, ApiErrorSchema } from './result'
```

- [ ] **Step 4: Verify compilation**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 5: Commit**

```bash
git add app/Store/src/shared/validations/
git commit -m "feat: add shared Zod validations for Result<T> and PagedResult<T>"
```

### Task 1.4: Store Event Bus

**Files:**
- Create: `app/Store/src/shared/composables/useStoreEvents.ts`

**Produces:**
- `StoreEvent` discriminated union type — `auth:login`, `auth:logout`, `auth:init-done`, `filter:changed`, `checkout:placed`, `cart:updated`, `profile:deleted`
- `useStoreEvents()` composable — `emit(event)`, `on(type, handler)`, `off(type, handler)`

- [ ] **Step 1: Write `useStoreEvents.ts`**

```typescript
// app/Store/src/shared/composables/useStoreEvents.ts

export type StoreEvent =
  | { type: 'auth:login'; userId: string }
  | { type: 'auth:logout' }
  | { type: 'auth:init-done'; userId: string }
  | { type: 'filter:changed' }
  | { type: 'checkout:placed'; orderId: string }
  | { type: 'cart:updated'; itemCount: number }
  | { type: 'profile:deleted' }

type EventHandler<T extends StoreEvent> = (event: T) => void

const listeners = new Map<string, Set<EventHandler<any>>>()

function getListeners<T extends StoreEvent>(type: string): Set<EventHandler<T>> {
  if (!listeners.has(type)) {
    listeners.set(type, new Set())
  }
  return listeners.get(type) as Set<EventHandler<T>>
}

export function emit<T extends StoreEvent>(event: T): void {
  for (const handler of getListeners<T>(event.type)) {
    handler(event)
  }
}

export function on<T extends StoreEvent>(
  type: T['type'],
  handler: EventHandler<T>
): void {
  getListeners<T>(type).add(handler)
}

export function off<T extends StoreEvent>(
  type: T['type'],
  handler: EventHandler<T>
): void {
  getListeners<T>(type).delete(handler)
}
```

- [ ] **Step 2: Verify compilation**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/shared/composables/useStoreEvents.ts
git commit -m "feat: add typed cross-store event bus"
```

### Task 1.5: Page Title Composable

**Files:**
- Create: `app/Store/src/shared/composables/usePageTitle.ts`

**Produces:**
- `usePageTitle(title: string)` — sets `document.title = "${title} | ReSys.Shop"`, watches for changes

- [ ] **Step 1: Write `usePageTitle.ts`**

```typescript
// app/Store/src/shared/composables/usePageTitle.ts
import { watchEffect } from 'vue'

const SUFFIX = ' | ReSys.Shop'

export function usePageTitle(title: string | (() => string)): void {
  const resolved = typeof title === 'function' ? title : () => title
  watchEffect(() => {
    document.title = `${resolved()}${SUFFIX}`
  })
}
```

- [ ] **Step 2: Verify compilation**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/shared/composables/usePageTitle.ts
git commit -m "feat: add usePageTitle composable"
```

---

## Phase 2: Catalog Domain

### Phase 2 Ledger

| Task | Deliverable | Files |
|------|-------------|-------|
| 2.1 | Catalog types (8 entities) | 8 files in `types/` |
| 2.2 | Catalog Zod validations | 5 files in `validations/` |
| 2.3 | Catalog API services | 4 files in `services/` |
| 2.4 | catalogStore | 1 file in `stores/` |
| 2.5 | productListStore | 1 file in `stores/` |
| 2.6 | productDetailStore | 1 file in `stores/` |
| 2.7 | visualSearchStore | 1 file in `stores/` |
| 2.8 | useSearch composable | 1 file in `composables/` |
| 2.9 | useVisualSearch composable | 1 file in `composables/` |
| 2.10 | HomeView skeleton | 1 file in `views/` |
| 2.11 | ShopView skeleton | 1 file in `views/` |
| 2.12 | ProductDetailView skeleton | 1 file in `views/` |
| 2.13 | CollectionsView skeleton | 1 file in `views/` |
| 2.14 | VisualSearchView skeleton | 1 file in `views/` |
| 2.15 | Static pages (NotFound, About, Terms, Privacy) | 4 files in `views/` |
| 2.16 | Layer barrels | 5 `index.ts` files |

### Task 2.1: Catalog Types

**Files:**
- Create: `app/Store/src/features/catalog/types/product.ts`
- Create: `app/Store/src/features/catalog/types/variant.ts`
- Create: `app/Store/src/features/catalog/types/taxon.ts`
- Create: `app/Store/src/features/catalog/types/taxonTree.ts`
- Create: `app/Store/src/features/catalog/types/taxonBreadcrumb.ts`
- Create: `app/Store/src/features/catalog/types/optionType.ts`
- Create: `app/Store/src/features/catalog/types/searchByImage.ts`
- Create: `app/Store/src/features/catalog/types/catalogQuery.ts`

**Produces:** All TypeScript interfaces matching backend DTOs, filter param types, and `toProductQueryParams()` mapper function.

- [ ] **Step 1: Write `product.ts`**

```typescript
// app/Store/src/features/catalog/types/product.ts
export interface StoreVariantStockInfo {
  availableQuantity: number
  backorderable: boolean
}

export interface StoreVariantOptionValueResponse {
  id: string
  variantOptionValueId: string
  name: string
  presentation: string | null
  position: number
  optionTypeId: string
  optionTypeName: string | null
}

export interface StoreProductImageResponse {
  id: string
  url: string
  alt: string | null
  position: number
}

export interface StoreVariantPriceResponse {
  id: string
  amount: number | null
  currency: string
  compareAtAmount: number | null
  countryIso: string | null
}

export interface StoreProductVariantResponse {
  id: string
  sku: string | null
  isMaster: boolean
  price: number | null
  currency: string | null
  optionValues: StoreVariantOptionValueResponse[]
  images: StoreProductImageResponse[]
  prices: StoreVariantPriceResponse[]
  stock: StoreVariantStockInfo
}

export interface StoreProductListItemResponse {
  id: string
  masterVariantId: string
  name: string
  status: string
  description: string | null
  slug: string
  styleCode: string | null
  seasonName: string | null
  materialComposition: string | null
  careInstructions: string | null
  fitNotes: string | null
  department: string | null
  genderTarget: string | null
  variantsCount: number
  availableOn: string | null
  masterVariant: StoreProductVariantResponse | null
  classifications: any[]
}

export interface StoreProductDetailResponse extends StoreProductListItemResponse {
  variants: StoreProductVariantResponse[]
}
```

- [ ] **Step 2: Write `variant.ts`** — re-export from `product.ts` for separation

```typescript
// app/Store/src/features/catalog/types/variant.ts
export type {
  StoreVariantStockInfo,
  StoreVariantOptionValueResponse,
  StoreProductImageResponse,
  StoreVariantPriceResponse,
  StoreProductVariantResponse,
} from './product'
```

- [ ] **Step 3: Write `taxon.ts`**

```typescript
// app/Store/src/features/catalog/types/taxon.ts
export interface StoreTaxonomyListItem {
  id: string
  name: string
  presentation: string | null
  position: number
}

export interface TaxonBreadcrumbItem {
  id: string
  name: string
  permalink: string
}

export interface StoreTaxonListItemResponse {
  id: string
  name: string
  permalink: string
  depth: number
  slug: string
  presentation: string | null
  taxonomyId: string
  parentId: string | null
  position: number
  imageUrl: string | null
  taxonCount: number | null
  childrenCount: number | null
  prettyName: string
  breadcrumb: TaxonBreadcrumbItem[]
}
```

- [ ] **Step 4: Write `taxonTree.ts`**

```typescript
// app/Store/src/features/catalog/types/taxonTree.ts
export interface TaxonTreeNode {
  id: string
  name: string
  presentation: string | null
  permalink: string
  depth: number
  hasChildren: boolean
  children: TaxonTreeNode[]
}

export interface TaxonomyGroup {
  taxonomy: { id: string; name: string; presentation: string | null }
  tree: TaxonTreeNode[]
}
```

- [ ] **Step 5: Write `taxonBreadcrumb.ts`** — re-export

```typescript
// app/Store/src/features/catalog/types/taxonBreadcrumb.ts
export type { TaxonBreadcrumbItem } from './taxon'
```

- [ ] **Step 6: Write `optionType.ts`**

```typescript
// app/Store/src/features/catalog/types/optionType.ts
export interface StoreOptionValueListItemResponse {
  id: string
  name: string
  presentation: string | null
  position: number
  optionTypeId: string
  optionTypeName: string | null
}

export interface StoreOptionTypeListItem {
  id: string
  name: string
  presentation: string | null
  position: number
  filterable: boolean
}

export interface FilterableOptionType extends StoreOptionTypeListItem {
  values: StoreOptionValueListItemResponse[]
}
```

- [ ] **Step 7: Write `searchByImage.ts`**

```typescript
// app/Store/src/features/catalog/types/searchByImage.ts
export interface SearchByImageResponse {
  variantId: string
  productId: string
  productName: string
  sku: string
  price: number
  imageUrl: string | null
  similarityScore: number
}

export interface VisualSearchModel {
  id: string
  name: string
  description: string | null
  dimension: number
  isOnnx: boolean
}
```

- [ ] **Step 8: Write `catalogQuery.ts`**

```typescript
// app/Store/src/features/catalog/types/catalogQuery.ts
import type { QueryingParameters } from '@/shared/types/querying'

export interface CatalogFilterParams {
  searchQuery?: string
  taxonIds?: string[]
  optionValueIds?: string[]
  minPrice?: number
  maxPrice?: number
}

export interface ProductQuery {
  pageNumber?: number
  pageSize?: number
  search?: string
  filter?: string
  sort?: string[]
}

export function toProductQueryParams(q: ProductQuery): Record<string, unknown> {
  const params: Record<string, unknown> = {}
  if (q.pageNumber) params.pageNumber = q.pageNumber
  if (q.pageSize) params.pageSize = q.pageSize
  if (q.search) params.search = q.search
  if (q.filter) params.filter = q.filter
  if (q.sort) params.sort = q.sort
  return params
}
```

- [ ] **Step 9: Write `index.ts` barrel for types**

```typescript
// app/Store/src/features/catalog/types/index.ts
export type { StoreProductListItemResponse, StoreProductDetailResponse, StoreProductVariantResponse, StoreVariantStockInfo, StoreVariantOptionValueResponse, StoreProductImageResponse, StoreVariantPriceResponse } from './product'
export type { StoreTaxonomyListItem, StoreTaxonListItemResponse, TaxonBreadcrumbItem } from './taxon'
export type { TaxonTreeNode, TaxonomyGroup } from './taxonTree'
export type { StoreOptionValueListItemResponse, StoreOptionTypeListItem, FilterableOptionType } from './optionType'
export type { SearchByImageResponse, VisualSearchModel } from './searchByImage'
export type { CatalogFilterParams, ProductQuery } from './catalogQuery'
export { toProductQueryParams } from './catalogQuery'
```

- [ ] **Step 10: Verify compilation**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 11: Commit**

```bash
git add app/Store/src/features/catalog/types/
git commit -m "feat(catalog): add TypeScript types for all catalog entities"
```

### Task 2.2: Catalog Zod Validations

**Files:**
- Create: `app/Store/src/features/catalog/validations/product.ts`
- Create: `app/Store/src/features/catalog/validations/taxon.ts`
- Create: `app/Store/src/features/catalog/validations/optionType.ts`
- Create: `app/Store/src/features/catalog/validations/searchByImage.ts`
- Create: `app/Store/src/features/catalog/validations/index.ts`

**Produces:** Zod schemas for all catalog API responses. Used by service classes for runtime validation.

- [ ] **Step 1: Write `product.ts`**

```typescript
// app/Store/src/features/catalog/validations/product.ts
import { z } from 'zod'

const VariantStockInfoSchema = z.object({
  availableQuantity: z.number().int().min(0),
  backorderable: z.boolean(),
})

const VariantOptionValueSchema = z.object({
  id: z.string(),
  variantOptionValueId: z.string(),
  name: z.string(),
  presentation: z.string().nullable(),
  position: z.number().int().min(0),
  optionTypeId: z.string(),
  optionTypeName: z.string().nullable(),
})

const ProductImageSchema = z.object({
  id: z.string(),
  url: z.string(),
  alt: z.string().nullable(),
  position: z.number().int().min(0),
})

const VariantPriceSchema = z.object({
  id: z.string(),
  amount: z.number().nullable(),
  currency: z.string(),
  compareAtAmount: z.number().nullable(),
  countryIso: z.string().nullable(),
})

const ProductVariantSchema = z.object({
  id: z.string(),
  sku: z.string().nullable(),
  isMaster: z.boolean(),
  price: z.number().nullable(),
  currency: z.string().nullable(),
  optionValues: z.array(VariantOptionValueSchema),
  images: z.array(ProductImageSchema),
  prices: z.array(VariantPriceSchema),
  stock: VariantStockInfoSchema,
})

export const ProductListItemSchema = z.object({
  id: z.string(),
  masterVariantId: z.string(),
  name: z.string(),
  status: z.string(),
  description: z.string().nullable(),
  slug: z.string(),
  styleCode: z.string().nullable(),
  seasonName: z.string().nullable(),
  materialComposition: z.string().nullable(),
  careInstructions: z.string().nullable(),
  fitNotes: z.string().nullable(),
  department: z.string().nullable(),
  genderTarget: z.string().nullable(),
  variantsCount: z.number().int().min(0),
  availableOn: z.string().nullable(),
  masterVariant: ProductVariantSchema.nullable(),
  classifications: z.array(z.any()),
})

export const ProductDetailSchema = ProductListItemSchema.extend({
  variants: z.array(ProductVariantSchema),
})

export const ProductSearchFormSchema = z.object({
  search: z.string().optional(),
  sort: z.enum(['-createdAtUtc', 'price', '-price', 'name', '-name']).optional(),
})

export type ProductSearchForm = z.infer<typeof ProductSearchFormSchema>
```

- [ ] **Step 2: Write `taxon.ts`**

```typescript
// app/Store/src/features/catalog/validations/taxon.ts
import { z } from 'zod'

export const TaxonBreadcrumbItemSchema = z.object({
  id: z.string(),
  name: z.string(),
  permalink: z.string(),
})

export const TaxonListItemSchema = z.object({
  id: z.string(),
  name: z.string(),
  permalink: z.string(),
  depth: z.number().int().min(0),
  slug: z.string(),
  presentation: z.string().nullable(),
  taxonomyId: z.string(),
  parentId: z.string().nullable(),
  position: z.number().int().min(0),
  imageUrl: z.string().nullable(),
  taxonCount: z.number().int().nullable(),
  childrenCount: z.number().int().nullable(),
  prettyName: z.string(),
  breadcrumb: z.array(TaxonBreadcrumbItemSchema),
})

export const TaxonomySchema = z.object({
  id: z.string(),
  name: z.string(),
  presentation: z.string().nullable(),
  position: z.number().int().min(0),
})

const TaxonTreeNodeSchema: z.ZodType<any> = z.lazy(() =>
  z.object({
    id: z.string(),
    name: z.string(),
    presentation: z.string().nullable(),
    permalink: z.string(),
    depth: z.number().int().min(0),
    hasChildren: z.boolean(),
    children: z.array(TaxonTreeNodeSchema),
  })
)

export const TaxonomyGroupSchema = z.object({
  taxonomy: z.object({
    id: z.string(),
    name: z.string(),
    presentation: z.string().nullable(),
    position: z.number().int().min(0),
  }),
  tree: z.array(TaxonTreeNodeSchema),
})
```

- [ ] **Step 3: Write `optionType.ts`**

```typescript
// app/Store/src/features/catalog/validations/optionType.ts
import { z } from 'zod'

export const OptionValueSchema = z.object({
  id: z.string(),
  name: z.string(),
  presentation: z.string().nullable(),
  position: z.number().int().min(0),
  optionTypeId: z.string(),
  optionTypeName: z.string().nullable(),
})

export const OptionTypeSchema = z.object({
  id: z.string(),
  name: z.string(),
  presentation: z.string().nullable(),
  position: z.number().int().min(0),
  filterable: z.boolean(),
})
```

- [ ] **Step 4: Write `searchByImage.ts`**

```typescript
// app/Store/src/features/catalog/validations/searchByImage.ts
import { z } from 'zod'

export const SearchByImageResponseSchema = z.object({
  variantId: z.string(),
  productId: z.string(),
  productName: z.string(),
  sku: z.string(),
  price: z.number(),
  imageUrl: z.string().nullable(),
  similarityScore: z.number().min(0).max(1),
})

export const VisualSearchModelSchema = z.object({
  id: z.string(),
  name: z.string(),
  description: z.string().nullable(),
  dimension: z.number(),
  isOnnx: z.boolean(),
})
```

- [ ] **Step 5: Write `index.ts` barrel**

```typescript
// app/Store/src/features/catalog/validations/index.ts
export {
  ProductListItemSchema,
  ProductDetailSchema,
  ProductSchema,
  ProductSearchFormSchema,
} from './product'
export type { ProductSearchForm } from './product'
export { TaxonListItemSchema, TaxonomyGroupSchema, TaxonomySchema } from './taxon'
export { OptionValueSchema, OptionTypeSchema } from './optionType'
export { SearchByImageResponseSchema, VisualSearchModelSchema } from './searchByImage'
```

- [ ] **Step 6: Verify compilation**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 7: Commit**

```bash
git add app/Store/src/features/catalog/validations/
git commit -m "feat(catalog): add Zod validation schemas for all catalog entities"
```

### Task 2.3: Catalog API Services

**Files:**
- Create: `app/Store/src/features/catalog/services/productApi.ts`
- Create: `app/Store/src/features/catalog/services/taxonApi.ts`
- Create: `app/Store/src/features/catalog/services/optionTypeApi.ts`
- Create: `app/Store/src/features/catalog/services/searchByImageApi.ts`
- Create: `app/Store/src/features/catalog/services/index.ts`

**Produces:** 4 static service classes with Zod-validated API calls.

- [ ] **Step 1: Write `productApi.ts`**

```typescript
// app/Store/src/features/catalog/services/productApi.ts
import { get, post } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import { ProductListItemSchema, ProductDetailSchema } from '../validations/product'
import { PagedResultSchema } from '@/shared/validations/result'
import type { PagedResult } from '@/shared/types'
import type { StoreProductListItemResponse, StoreProductDetailResponse, ProductQuery } from '../types'
import { toProductQueryParams } from '../types'

const validatedPagedList = PagedResultSchema(ProductListItemSchema)

export class ProductApi {
  private static readonly BASE = `${CATALOG}/products`

  static async getProducts(q: ProductQuery): Promise<PagedResult<StoreProductListItemResponse>> {
    const params = toProductQueryParams(q)
    const result = await getPaged<unknown>(this.BASE, params)
    if (!result.isSuccess) return result as PagedResult<StoreProductListItemResponse>
    const parsed = validatedPagedList.parse({ ...result, items: result.items })
    return parsed as PagedResult<StoreProductListItemResponse>
  }

  static async getProductBySlug(slug: string): Promise<PagedResult<StoreProductDetailResponse>> {
    const data = await get<unknown>(`${this.BASE}/${slug}`)
    if (!data.isSuccess) return data as PagedResult<StoreProductDetailResponse>
    data.value = ProductDetailSchema.parse(data.value)
    return data as PagedResult<StoreProductDetailResponse>
  }

  static async getSimilar(productId: string, topK?: number): Promise<PagedResult<StoreProductListItemResponse>> {
    const params: Record<string, unknown> = { productId }
    if (topK) params.topK = topK
    const result = await getPaged<unknown>(`${this.BASE}/similar`, params)
    if (!result.isSuccess) return result as PagedResult<StoreProductListItemResponse>
    const parsed = validatedPagedList.parse({ ...result, items: result.items })
    return parsed as PagedResult<StoreProductListItemResponse>
  }

  static async getRelated(productId: string, q: ProductQuery): Promise<PagedResult<StoreProductListItemResponse>> {
    const params: Record<string, unknown> = { productId, ...toProductQueryParams(q) }
    const result = await getPaged<unknown>(`${this.BASE}/related`, params)
    if (!result.isSuccess) return result as PagedResult<StoreProductListItemResponse>
    const parsed = validatedPagedList.parse({ ...result, items: result.items })
    return parsed as PagedResult<StoreProductListItemResponse>
  }
}
```

- [ ] **Step 2: Write `taxonApi.ts`**

```typescript
// app/Store/src/features/catalog/services/taxonApi.ts
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import { TaxonListItemSchema, TaxonomyGroupSchema } from '../validations/taxon'
import { PagedResultSchema } from '@/shared/validations/result'
import type { PagedResult } from '@/shared/types'
import type { StoreTaxonListItemResponse, StoreTaxonomyListItem, TaxonomyGroup } from '../types'
import type { QueryingParameters } from '@/shared/types/querying'

const taxonList = PagedResultSchema(TaxonListItemSchema)
const taxonomyList = PagedResultSchema(TaxonomyGroupSchema)

export class TaxonApi {
  static async getTaxonomies(q: QueryingParameters): Promise<PagedResult<StoreTaxonomyListItem>> {
    const result = await getPaged<unknown>(`${CATALOG}/taxonomies`, q)
    if (!result.isSuccess) return result as PagedResult<StoreTaxonomyListItem>
    return result as PagedResult<StoreTaxonomyListItem>
  }

  static async getTaxons(q: QueryingParameters): Promise<PagedResult<StoreTaxonListItemResponse>> {
    const result = await getPaged<unknown>(`${CATALOG}/taxons`, q)
    if (!result.isSuccess) return result as PagedResult<StoreTaxonListItemResponse>
    const parsed = taxonList.parse({ ...result, items: result.items })
    return parsed as PagedResult<StoreTaxonListItemResponse>
  }
}
```

- [ ] **Step 3: Write `optionTypeApi.ts`**

```typescript
// app/Store/src/features/catalog/services/optionTypeApi.ts
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import { OptionTypeSchema, OptionValueSchema } from '../validations/optionType'
import { PagedResultSchema } from '@/shared/validations/result'
import type { PagedResult } from '@/shared/types'
import type { StoreOptionTypeListItem, StoreOptionValueListItemResponse } from '../types'
import type { QueryingParameters } from '@/shared/types/querying'

const optionTypeList = PagedResultSchema(OptionTypeSchema)
const optionValueList = PagedResultSchema(OptionValueSchema)

export class OptionTypeApi {
  static async getOptionTypes(q: QueryingParameters): Promise<PagedResult<StoreOptionTypeListItem>> {
    const result = await getPaged<unknown>(`${CATALOG}/option-types`, q)
    if (!result.isSuccess) return result as PagedResult<StoreOptionTypeListItem>
    const parsed = optionTypeList.parse({ ...result, items: result.items })
    return parsed as PagedResult<StoreOptionTypeListItem>
  }

  static async getOptionValues(q: QueryingParameters): Promise<PagedResult<StoreOptionValueListItemResponse>> {
    const result = await getPaged<unknown>(`${CATALOG}/option-values`, q)
    if (!result.isSuccess) return result as PagedResult<StoreOptionValueListItemResponse>
    const parsed = optionValueList.parse({ ...result, items: result.items })
    return parsed as PagedResult<StoreOptionValueListItemResponse>
  }
}
```

- [ ] **Step 4: Write `searchByImageApi.ts`**

```typescript
// app/Store/src/features/catalog/services/searchByImageApi.ts
import { get, post } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import { SearchByImageResponseSchema, VisualSearchModelSchema } from '../validations/searchByImage'
import { ResultSchema, PagedResultSchema } from '@/shared/validations/result'
import type { Result, PagedResult } from '@/shared/types'
import type { SearchByImageResponse, VisualSearchModel } from '../types'

const modelResult = ResultSchema(z.array(VisualSearchModelSchema))
const searchList = PagedResultSchema(SearchByImageResponseSchema)

export class SearchByImageApi {
  static async getVisualSearchModels(): Promise<Result<VisualSearchModel[]>> {
    const result = await get<unknown>(`${CATALOG}/products/visual-search/models`)
    if (!result.isSuccess) return result as Result<VisualSearchModel[]>
    result.value = VisualSearchModelSchema.array().parse(result.value)
    return result as Result<VisualSearchModel[]>
  }

  static async searchByImage(
    file: File,
    topK?: number,
    model?: string
  ): Promise<PagedResult<SearchByImageResponse>> {
    const form = new FormData()
    form.append('image', file)
    if (topK) form.append('topK', String(topK))
    if (model) form.append('model', model)
    const result = await post<unknown>(`${CATALOG}/products/images/search`, form)
    if (!result.isSuccess) return result as PagedResult<SearchByImageResponse>
    return result as PagedResult<SearchByImageResponse>
  }
}
```

- [ ] **Step 5: Write `index.ts` barrel**

```typescript
// app/Store/src/features/catalog/services/index.ts
export { ProductApi } from './productApi'
export { TaxonApi } from './taxonApi'
export { OptionTypeApi } from './optionTypeApi'
export { SearchByImageApi } from './searchByImageApi'
```

- [ ] **Step 6: Verify compilation**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 7: Commit**

```bash
git add app/Store/src/features/catalog/services/
git commit -m "feat(catalog): add Zod-validated API service classes"
```

### Task 2.4: catalogStore (Filters)

**Files:**
- Create: `app/Store/src/features/catalog/stores/catalogStore.ts`

**Produces:** `useCatalogStore` — manages all filter state (search, taxon, option, price, sort), derives querying parameters, loads taxonomy/option data for the filter sidebar.

- [ ] **Step 1: Write `catalogStore.ts`**

```typescript
// app/Store/src/features/catalog/stores/catalogStore.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { TaxonApi } from '../services/taxonApi'
import { OptionTypeApi } from '../services/optionTypeApi'
import { emit } from '@/shared/composables/useStoreEvents'
import type { TaxonomyGroup, StoreOptionTypeListItem, StoreOptionValueListItemResponse } from '../types'

export const useCatalogStore = defineStore('catalog', () => {
  const searchQuery = ref('')
  const selectedTaxonIds = ref<string[]>([])
  const selectedOptionValueIds = ref<string[]>([])
  const minPrice = ref<number | null>(null)
  const maxPrice = ref<number | null>(null)
  const sortField = ref('-createdAtUtc')

  const taxonomyGroups = ref<TaxonomyGroup[]>([])
  const optionTypes = ref<(StoreOptionTypeListItem & { values: StoreOptionValueListItemResponse[] })[]>([])
  const taxonsLoading = ref(false)
  const optionsLoading = ref(false)

  const activeFilterCount = computed(() => {
    let count = 0
    if (searchQuery.value) count++
    count += selectedTaxonIds.value.length
    count += selectedOptionValueIds.value.length
    if (minPrice.value != null) count++
    if (maxPrice.value != null) count++
    return count
  })

  function setSearch(query: string): void {
    searchQuery.value = query
    emitFilterChanged()
  }

  function toggleTaxon(id: string): void {
    const idx = selectedTaxonIds.value.indexOf(id)
    if (idx === -1) selectedTaxonIds.value.push(id)
    else selectedTaxonIds.value.splice(idx, 1)
    emitFilterChanged()
  }

  function toggleOptionValue(id: string): void {
    const idx = selectedOptionValueIds.value.indexOf(id)
    if (idx === -1) selectedOptionValueIds.value.push(id)
    else selectedOptionValueIds.value.splice(idx, 1)
    emitFilterChanged()
  }

  function setPriceRange(min: number | null, max: number | null): void {
    minPrice.value = min
    maxPrice.value = max
    emitFilterChanged()
  }

  function clearFilters(): void {
    searchQuery.value = ''
    selectedTaxonIds.value = []
    selectedOptionValueIds.value = []
    minPrice.value = null
    maxPrice.value = null
    emit({ type: 'filter:changed' })
  }

  async function loadTaxonomyGroups(): Promise<void> {
    if (taxonomyGroups.value.length > 0) return
    taxonsLoading.value = true
    const [taxonomiesResult, taxonsResult] = await Promise.all([
      TaxonApi.getTaxonomies({ pageNumber: 1, pageSize: 50 }),
      TaxonApi.getTaxons({ pageNumber: 1, pageSize: 500 }),
    ])
    if (taxonomiesResult.isSuccess && taxonsResult.isSuccess) {
      taxonomyGroups.value = taxonomiesResult.items.map(t => ({
        taxonomy: { id: t.id, name: t.name, presentation: t.presentation },
        tree: buildTree(taxonsResult.items, t.id),
      }))
    }
    taxonsLoading.value = false
  }

  async function loadOptionTypes(): Promise<void> {
    if (optionTypes.value.length > 0) return
    optionsLoading.value = true
    const [typesResult, valuesResult] = await Promise.all([
      OptionTypeApi.getOptionTypes({ pageNumber: 1, pageSize: 50 }),
      OptionTypeApi.getOptionValues({ pageNumber: 1, pageSize: 500 }),
    ])
    if (typesResult.isSuccess && valuesResult.isSuccess) {
      optionTypes.value = typesResult.items
        .filter(t => t.filterable)
        .map(t => ({
          ...t,
          values: valuesResult.items.filter(v => v.optionTypeId === t.id),
        }))
    }
    optionsLoading.value = false
  }

  function emitFilterChanged(): void {
    emit({ type: 'filter:changed' })
  }

  return {
    searchQuery, selectedTaxonIds, selectedOptionValueIds, minPrice, maxPrice, sortField,
    taxonomyGroups, optionTypes, taxonsLoading, optionsLoading,
    activeFilterCount,
    setSearch, toggleTaxon, toggleOptionValue, setPriceRange, clearFilters,
    loadTaxonomyGroups, loadOptionTypes,
  }
})

function buildTree(items: any[], taxonomyId: string, parentId: string | null = null): any[] {
  return items
    .filter(i => i.taxonomyId === taxonomyId && i.parentId === parentId)
    .map(i => ({
      id: i.id,
      name: i.name,
      presentation: i.presentation,
      permalink: i.permalink,
      depth: i.depth,
      hasChildren: items.some(c => c.parentId === i.id),
      children: buildTree(items, taxonomyId, i.id),
    }))
}
```

- [ ] **Step 2: Verify compilation**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/features/catalog/stores/catalogStore.ts
git commit -m "feat(catalog): add catalogStore for filter state management"
```

### Task 2.5: productListStore

**Files:**
- Create: `app/Store/src/features/catalog/stores/productListStore.ts`

**Produces:** `useProductListStore` — paged product list with auto-fetch on filter changes.

- [ ] **Step 1: Write `productListStore.ts`**

```typescript
// app/Store/src/features/catalog/stores/productListStore.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { ProductApi } from '../services/productApi'
import { useCatalogStore } from './catalogStore'
import { on } from '@/shared/composables/useStoreEvents'
import type { StoreProductListItemResponse } from '../types'

export const useProductListStore = defineStore('productList', () => {
  const items = ref<StoreProductListItemResponse[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const page = ref(1)
  const pageSize = ref(20)
  const totalCount = ref(0)
  const isInitialLoad = ref(true)
  let _fetchTimer: ReturnType<typeof setTimeout> | null = null

  const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))

  async function fetch(): Promise<void> {
    if (loading.value) return
    loading.value = true
    error.value = null
    const catalog = useCatalogStore()
    const result = await ProductApi.getProducts({
      pageNumber: page.value,
      pageSize: pageSize.value,
      search: catalog.searchQuery || undefined,
      sort: [catalog.sortField],
    })
    if (result.isSuccess) {
      items.value = result.items
      totalCount.value = result.totalCount
    } else {
      error.value = result.message ?? 'Failed to load products'
    }
    loading.value = false
    isInitialLoad.value = false
  }

  function markStale(): void {
    page.value = 1
    if (_fetchTimer) clearTimeout(_fetchTimer)
    _fetchTimer = setTimeout(() => fetch(), 300)
  }

  function nextPage(): void { if (page.value < totalPages.value) { page.value++; fetch() } }
  function prevPage(): void { if (page.value > 1) { page.value--; fetch() } }
  function goToPage(p: number): void { page.value = Math.max(1, Math.min(p, totalPages.value)); fetch() }
  function refresh(): void { fetch() }

  function init(): void {
    on('filter:changed', () => markStale())
    fetch()
  }

  return {
    items, loading, error, page, pageSize, totalCount, totalPages, isInitialLoad,
    fetch, nextPage, prevPage, goToPage, refresh, init,
  }
})
```

- [ ] **Step 2: Verify compilation + commit**

```bash
cd app/Store && npx tsc --noEmit
git add app/Store/src/features/catalog/stores/productListStore.ts
git commit -m "feat(catalog): add productListStore with filter-aware paged fetch"
```

### Task 2.6: productDetailStore

**Files:**
- Create: `app/Store/src/features/catalog/stores/productDetailStore.ts`

**Produces:** `useProductDetailStore` — single product load, variant selection, stock, similar/related, add-to-cart.

- [ ] **Step 1: Write `productDetailStore.ts`**

```typescript
// app/Store/src/features/catalog/stores/productDetailStore.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { ProductApi } from '../services/productApi'
import { useCartStore } from '@/features/ordering/stores/cartStore'
import { useAvailabilityStore } from '@/features/inventory/stores/availabilityStore'
import { useRecentlyViewed } from '@/shared/composables/useRecentlyViewed'
import type { StoreProductDetailResponse, StoreProductListItemResponse, StoreProductVariantResponse } from '../types'

export const useProductDetailStore = defineStore('productDetail', () => {
  const product = ref<StoreProductDetailResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const selectedVariantId = ref<string | null>(null)
  const quantity = ref(1)
  const similarProducts = ref<StoreProductListItemResponse[]>([])
  const relatedProducts = ref<StoreProductListItemResponse[]>([])
  const relatedLoading = ref(false)

  const selectedVariant = computed<StoreProductVariantResponse | null>(() =>
    product.value?.variants.find(v => v.id === selectedVariantId.value) ?? null
  )

  const stockLabel = computed(() => {
    const stock = selectedVariant.value?.stock
    if (!stock) return null
    if (stock.availableQuantity > 5) return null
    if (stock.availableQuantity > 0) return `Only ${stock.availableQuantity} left`
    if (stock.backorderable) return 'Available for backorder'
    return 'Out of stock'
  })

  const isInStock = computed(() => {
    const stock = selectedVariant.value?.stock
    return stock ? stock.availableQuantity > 0 || stock.backorderable : false
  })

  async function load(slug: string): Promise<void> {
    loading.value = true
    error.value = null
    const result = await ProductApi.getProductBySlug(slug)
    if (result.isSuccess) {
      product.value = result.value as any // ProductDetail
      selectedVariantId.value = product.value?.masterVariant?.id ?? null
      useRecentlyViewed().add({
        productId: product.value!.id,
        productName: product.value!.name,
        slug: product.value!.slug,
        thumbnailUrl: product.value!.masterVariant?.images?.[0]?.url ?? null,
        minPrice: product.value!.masterVariant?.price ?? null,
        viewedAt: Date.now(),
      })
      ProductApi.getSimilar(product.value!.id).then(r => {
        if (r.isSuccess) similarProducts.value = r.items
      })
      relatedLoading.value = true
      ProductApi.getRelated(product.value!.id, { pageNumber: 1, pageSize: 12 }).then(r => {
        if (r.isSuccess) relatedProducts.value = r.items
        relatedLoading.value = false
      })
    } else {
      error.value = result.message ?? 'Product not found'
    }
    loading.value = false
  }

  function selectVariant(variantId: string): void {
    selectedVariantId.value = variantId
    useAvailabilityStore().check(variantId)
  }

  async function addToCart(): Promise<boolean> {
    if (!selectedVariantId.value) return false
    return await useCartStore().addItem(selectedVariantId.value, quantity.value)
  }

  function incrementQuantity(): void { if (quantity.value < 99) quantity.value++ }
  function decrementQuantity(): void { if (quantity.value > 1) quantity.value-- }
  function reset(): void {
    product.value = null
    loading.value = false
    error.value = null
    selectedVariantId.value = null
    quantity.value = 1
    similarProducts.value = []
    relatedProducts.value = []
    relatedLoading.value = false
  }

  return {
    product, loading, error, selectedVariantId, quantity, similarProducts, relatedProducts, relatedLoading,
    selectedVariant, stockLabel, isInStock,
    load, selectVariant, addToCart, incrementQuantity, decrementQuantity, reset,
  }
})
```

- [ ] **Step 2: Verify compilation + commit**

```bash
cd app/Store && npx tsc --noEmit
git add app/Store/src/features/catalog/stores/productDetailStore.ts
git commit -m "feat(catalog): add productDetailStore"
```

### Task 2.7: visualSearchStore

**Files:**
- Create: `app/Store/src/features/catalog/stores/visualSearchStore.ts`

**Produces:** `useVisualSearchStore` — image upload, model selection, visual search execution.

- [ ] **Step 1: Write `visualSearchStore.ts`**

```typescript
// app/Store/src/features/catalog/stores/visualSearchStore.ts
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { SearchByImageApi } from '../services/searchByImageApi'
import type { SearchByImageResponse, VisualSearchModel } from '../types'

type VisualSearchState = 'empty' | 'upload' | 'loading' | 'results'

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp']
const MAX_SIZE = 10 * 1024 * 1024

export const useVisualSearchStore = defineStore('visualSearch', () => {
  const state = ref<VisualSearchState>('empty')
  const selectedFile = ref<File | null>(null)
  const previewUrl = ref<string | null>(null)
  const selectedModelId = ref<string | null>(null)
  const availableModels = ref<VisualSearchModel[]>([])
  const results = ref<SearchByImageResponse[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const validationError = ref<string | null>(null)

  function validateFile(file: File): boolean {
    if (!ALLOWED_TYPES.includes(file.type)) {
      validationError.value = 'Invalid file type. Use JPEG, PNG, or WebP.'
      return false
    }
    if (file.size > MAX_SIZE) {
      validationError.value = 'File exceeds 10 MB limit.'
      return false
    }
    validationError.value = null
    return true
  }

  function selectFile(file: File): void {
    if (!validateFile(file)) return
    selectedFile.value = file
    previewUrl.value = URL.createObjectURL(file)
    state.value = 'upload'
  }

  async function search(topK?: number, model?: string): Promise<void> {
    if (!selectedFile.value) return
    state.value = 'loading'
    loading.value = true
    error.value = null
    const result = await SearchByImageApi.searchByImage(selectedFile.value, topK ?? 20, model ?? selectedModelId.value ?? undefined)
    if (result.isSuccess) {
      results.value = result.items
      state.value = 'results'
    } else {
      error.value = result.message ?? 'Visual search failed'
      state.value = 'upload'
    }
    loading.value = false
  }

  async function loadModels(): Promise<void> {
    const result = await SearchByImageApi.getVisualSearchModels()
    if (result.isSuccess) availableModels.value = result.value
  }

  function reset(): void {
    if (previewUrl.value) URL.revokeObjectURL(previewUrl.value)
    selectedFile.value = null
    previewUrl.value = null
    results.value = []
    error.value = null
    validationError.value = null
    state.value = 'empty'
  }

  return {
    state, selectedFile, previewUrl, selectedModelId, availableModels, results,
    loading, error, validationError,
    validateFile, selectFile, search, loadModels, reset,
  }
})
```

- [ ] **Step 2: Verify compilation + commit**

```bash
cd app/Store && npx tsc --noEmit
git add app/Store/src/features/catalog/stores/visualSearchStore.ts
git commit -m "feat(catalog): add visualSearchStore"
```

### Task 2.8: useSearch Composable

**Files:**
- Create: `app/Store/src/features/catalog/composables/useSearch.ts`

**Produces:** Singleton keyword search overlay state with debounced search.

- [ ] **Step 1: Write `useSearch.ts`**

```typescript
// app/Store/src/features/catalog/composables/useSearch.ts
import { ref } from 'vue'
import { ProductApi } from '../services/productApi'
import type { StoreProductListItemResponse } from '../types'

let shared: ReturnType<typeof createSearch> | null = null

function createSearch() {
  const isOpen = ref(false)
  const query = ref('')
  const results = ref<StoreProductListItemResponse[]>([])
  const loading = ref(false)
  const selectedIndex = ref(0)
  const error = ref<string | null>(null)

  let debounceTimer: ReturnType<typeof setTimeout> | null = null

  function open(): void {
    isOpen.value = true
    selectedIndex.value = 0
  }

  function close(): void {
    isOpen.value = false
    query.value = ''
    results.value = []
    error.value = null
  }

  function clear(): void {
    query.value = ''
    results.value = []
    error.value = null
  }

  async function search(): Promise<void> {
    if (!query.value.trim()) { results.value = []; return }
    if (debounceTimer) clearTimeout(debounceTimer)
    debounceTimer = setTimeout(async () => {
      loading.value = true
      error.value = null
      const result = await ProductApi.getProducts({ pageNumber: 1, pageSize: 5, search: query.value.trim() })
      if (result.isSuccess) results.value = result.items
      else error.value = result.message ?? 'Search failed'
      loading.value = false
    }, 300)
  }

  function navigateToResult(index: number): void {
    const item = results.value[index]
    if (!item) return
    close()
    window.location.href = `/products/${item.slug}`
  }

  return { isOpen, query, results, loading, selectedIndex, error, open, close, clear, search, navigateToResult }
}

export function useSearch() {
  if (!shared) shared = createSearch()
  return shared
}
```

- [ ] **Step 2: Verify compilation + commit**

```bash
cd app/Store && npx tsc --noEmit
git add app/Store/src/features/catalog/composables/useSearch.ts
git commit -m "feat(catalog): add useSearch composable (singleton)"
```

### Task 2.9: Catalog Store Barrel

**Files:**
- Create: `app/Store/src/features/catalog/stores/index.ts`
- Create: `app/Store/src/features/catalog/composables/index.ts`
- Create: `app/Store/src/features/catalog/index.ts`

- [ ] **Step 1: Write barrel files**

```typescript
// stores/index.ts
export { useCatalogStore } from './catalogStore'
export { useProductListStore } from './productListStore'
export { useProductDetailStore } from './productDetailStore'
export { useVisualSearchStore } from './visualSearchStore'

// composables/index.ts
export { useSearch } from './useSearch'

// index.ts
export * from './types'
export * from './services'
export * from './stores'
```

- [ ] **Step 2: Verify compilation + commit**

```bash
cd app/Store && npx tsc --noEmit
git add app/Store/src/features/catalog/stores/index.ts app/Store/src/features/catalog/composables/index.ts app/Store/src/features/catalog/index.ts
git commit -m "feat(catalog): add layer barrels and domain index"
```

### Task 2.10: Catalog Skeleton Pages

**Files:**
- Create: `app/Store/src/features/catalog/views/HomeView.vue`
- Create: `app/Store/src/features/catalog/views/ShopView.vue`
- Create: `app/Store/src/features/catalog/views/ProductDetailView.vue`
- Create: `app/Store/src/features/catalog/views/CollectionsView.vue`
- Create: `app/Store/src/features/catalog/views/VisualSearchView.vue`
- Create: `app/Store/src/features/catalog/views/NotFoundView.vue`
- Create: `app/Store/src/features/catalog/views/AboutView.vue`
- Create: `app/Store/src/features/catalog/views/TermsView.vue`
- Create: `app/Store/src/features/catalog/views/PrivacyView.vue`

**Produces:** 9 skeleton/placeholder pages with PrimeVue components.

- [ ] **Step 1: Write HomeView skeleton**

```vue
<!-- app/Store/src/features/catalog/views/HomeView.vue -->
<script setup lang="ts">
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useCatalogStore } from '../stores/catalogStore'
import { useProductListStore } from '../stores/productListStore'

usePageTitle('Home')
const catalog = useCatalogStore()
const productList = useProductListStore()
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Home</h1>
    <Card>
      <template #content>
        <div class="space-y-4">
          <Skeleton width="100%" height="2rem" />
          <Skeleton width="75%" height="1rem" />
          <Skeleton width="50%" height="1rem" />
        </div>
      </template>
    </Card>
    <Message severity="info" class="mt-4">
      Home page content will be implemented here — hero banner, categories, new arrivals, featured products.
    </Message>
  </div>
</template>
```

- [ ] **Step 2: Write ShopView skeleton**

```vue
<!-- app/Store/src/features/catalog/views/ShopView.vue -->
<script setup lang="ts">
import { onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useCatalogStore } from '../stores/catalogStore'
import { useProductListStore } from '../stores/productListStore'

usePageTitle('Shop')
const catalog = useCatalogStore()
const productList = useProductListStore()

onMounted(() => {
  catalog.loadTaxonomyGroups()
  catalog.loadOptionTypes()
  productList.init()
})
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Shop' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Shop</h1>
    <Card>
      <template #content>
        <div class="space-y-4">
          <Skeleton width="100%" height="2rem" />
          <Skeleton width="75%" height="1rem" />
          <Skeleton width="50%" height="1rem" />
        </div>
      </template>
    </Card>
    <Message severity="info" class="mt-4">
      Product grid, filter sidebar, sort controls, and pagination will be implemented here.
    </Message>
  </div>
</template>
```

- [ ] **Step 3: Write ProductDetailView skeleton**

```vue
<!-- app/Store/src/features/catalog/views/ProductDetailView.vue -->
<script setup lang="ts">
import { watch } from 'vue'
import { useRoute } from 'vue-router'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useProductDetailStore } from '../stores/productDetailStore'

usePageTitle('Product')
const route = useRoute()
const detail = useProductDetailStore()

watch(() => route.params.slug, (slug) => {
  if (typeof slug === 'string') detail.load(slug)
}, { immediate: true })
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Shop', to: '/shop' }, { label: 'Product' }]" />
    <Skeleton v-if="detail.loading" width="100%" height="20rem" />
    <Card v-else-if="detail.product">
      <template #content>
        <h1 class="text-2xl font-bold text-neutral-900">{{ detail.product.name }}</h1>
      </template>
    </Card>
    <Message severity="error" v-else-if="detail.error" class="mt-4">{{ detail.error }}</Message>
    <Message severity="info" class="mt-4">
      Product gallery, variant selector, add-to-cart, description tabs, similar products will be implemented here.
    </Message>
  </div>
</template>
```

- [ ] **Step 4: Write CollectionsView skeleton**

```vue
<!-- app/Store/src/features/catalog/views/CollectionsView.vue -->
<script setup lang="ts">
import { usePageTitle } from '@/shared/composables/usePageTitle'
usePageTitle('Collections')
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Collections' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Collections</h1>
    <Card>
      <template #content>
        <div class="space-y-4">
          <Skeleton width="100%" height="2rem" />
          <Skeleton width="75%" height="1rem" />
        </div>
      </template>
    </Card>
    <Message severity="info" class="mt-4">
      Collection list and category browsing will be implemented here.
    </Message>
  </div>
</template>
```

- [ ] **Step 5: Write VisualSearchView skeleton**

```vue
<!-- app/Store/src/features/catalog/views/VisualSearchView.vue -->
<script setup lang="ts">
import { onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useVisualSearchStore } from '../stores/visualSearchStore'

usePageTitle('Visual Search')
const vs = useVisualSearchStore()
onMounted(() => vs.loadModels())
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Visual Search' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Visual Search</h1>
    <Card>
      <template #content>
        <div class="space-y-4">
          <Skeleton width="100%" height="2rem" />
          <Skeleton width="75%" height="1rem" />
        </div>
      </template>
    </Card>
    <Message severity="info" class="mt-4">
      Image upload drop zone, model selector, search button, and results grid will be implemented here.
    </Message>
  </div>
</template>
```

- [ ] **Step 6: Write NotFoundView, AboutView, TermsView, PrivacyView**

Same skeleton pattern. Use `<Card>` + `<Skeleton>` + `<Message>` with appropriate page titles and descriptions of future content.

- [ ] **Step 7: Write views barrel**

```typescript
// app/Store/src/features/catalog/views/index.ts
export { default as HomeView } from './HomeView.vue'
export { default as ShopView } from './ShopView.vue'
export { default as ProductDetailView } from './ProductDetailView.vue'
export { default as CollectionsView } from './CollectionsView.vue'
export { default as VisualSearchView } from './VisualSearchView.vue'
export { default as NotFoundView } from './NotFoundView.vue'
export { default as AboutView } from './AboutView.vue'
export { default as TermsView } from './TermsView.vue'
export { default as PrivacyView } from './PrivacyView.vue'
```

- [ ] **Step 8: Verify compilation + commit**

```bash
cd app/Store && npx tsc --noEmit
git add app/Store/src/features/catalog/views/
git commit -m "feat(catalog): add 9 skeleton view pages"
```

---

## Phase 3: Identity Domain

### Phase 3 Ledger

| Task | Deliverable | Files |
|------|-------------|-------|
| 3.1 | Identity types | 1 file in `types/` |
| 3.2 | Identity validations | 1 file in `validations/` |
| 3.3 | Identity API services | 4 files in `services/` |
| 3.4 | authStore | 1 file in `stores/` |
| 3.5 | Skeleton pages (5 views) | 5 files in `views/` |
| 3.6 | Barrels | 4 `index.ts` |

### Task 3.1: Identity Types

**Files:** Create `app/Store/src/features/identity/types/auth.ts`

- [ ] **Step 1: Write `auth.ts`**

```typescript
// app/Store/src/features/identity/types/auth.ts
export interface TokenPair {
  accessToken: string
  accessTokenExpiresIn: number
  refreshToken: string
  refreshTokenExpiresIn: number
}

export interface AuthUser {
  userId: string
  userName: string
  email: string
  roles: string[]
  permissions: string[]
  isAuthenticated: boolean
}

export interface LoginRequest {
  credential: string
  password: string
}

export interface RegisterRequest {
  fullName: string
  email: string
  password: string
}

export interface SessionUser {
  id: string
  userName: string
  email: string
  roles: string[]
  permissions: string[]
}

export interface SessionInfo {
  id: string
  deviceName: string
  ipAddress: string
  lastActivityAt: string
  isCurrent: boolean
}
```

### Task 3.2: Identity Validations

**Files:** Create `app/Store/src/features/identity/validations/auth.ts`

- [ ] **Step 1: Write `auth.ts`**

```typescript
// app/Store/src/features/identity/validations/auth.ts
import { z } from 'zod'

export const LoginRequestSchema = z.object({
  credential: z.string().min(1),
  password: z.string().min(1),
})

export const RegisterRequestSchema = z.object({
  fullName: z.string().min(1).max(200),
  email: z.string().email(),
  password: z.string().min(8),
})

export const TokenPairSchema = z.object({
  accessToken: z.string(),
  accessTokenExpiresIn: z.number(),
  refreshToken: z.string(),
  refreshTokenExpiresIn: z.number(),
})

export const SessionUserSchema = z.object({
  id: z.string(),
  userName: z.string(),
  email: z.string(),
  roles: z.array(z.string()),
  permissions: z.array(z.string()),
})

export const SessionInfoSchema = z.object({
  id: z.string(),
  deviceName: z.string(),
  ipAddress: z.string(),
  lastActivityAt: z.string(),
  isCurrent: z.boolean(),
})

export const ForgotPasswordSchema = z.object({ email: z.string().email() })
export const ResetPasswordSchema = z.object({ token: z.string(), newPassword: z.string().min(8) })
export const ChangePasswordSchema = z.object({ currentPassword: z.string(), newPassword: z.string().min(8) })
export const EmailSchema = z.object({ email: z.string().email() })

export const LoginFormSchema = z.object({ credential: z.string().min(1), password: z.string().min(1) })
export type LoginForm = z.infer<typeof LoginFormSchema>
export const RegisterFormSchema = z.object({
  fullName: z.string().min(1).max(200),
  email: z.string().email(),
  password: z.string().min(8),
  confirmPassword: z.string(),
}).refine(d => d.password === d.confirmPassword, { message: 'Passwords do not match', path: ['confirmPassword'] })
export type RegisterForm = z.infer<typeof RegisterFormSchema>
```

### Task 3.3: Identity Services

**Files:**
- Create: `app/Store/src/features/identity/services/authApi.ts`
- Create: `app/Store/src/features/identity/services/emailApi.ts`
- Create: `app/Store/src/features/identity/services/sessionApi.ts`
- Create: `app/Store/src/features/identity/services/tokenService.ts`

- [ ] **Step 1: Write `authApi.ts`**

```typescript
// app/Store/src/features/identity/services/authApi.ts
import { get, post } from '@/shared/api/client'
import { IDENTITY } from '@/shared/constants/api'
import { TokenPairSchema, SessionUserSchema } from '../validations/auth'
import { ResultSchema } from '@/shared/validations/result'
import type { Result } from '@/shared/types'
import type { LoginRequest, RegisterRequest, TokenPair, SessionUser } from '../types'
import { z } from 'zod'

export class AuthApi {
  static async login(req: LoginRequest): Promise<Result<TokenPair>> {
    const result = await post<unknown>(`${IDENTITY}/auth/login/password`, req)
    if (!result.isSuccess) return result as Result<TokenPair>
    result.value = TokenPairSchema.parse(result.value)
    return result as Result<TokenPair>
  }

  static async register(req: RegisterRequest): Promise<Result<void>> {
    return await post(`${IDENTITY}/auth/register`, req)
  }

  static async logout(req?: { refreshToken?: string; revokeAll?: boolean }): Promise<Result<void>> {
    return await post(`${IDENTITY}/auth/logout`, req ?? {})
  }

  static async getSession(): Promise<Result<SessionUser>> {
    const result = await get<unknown>(`${IDENTITY}/auth/sessions`)
    if (!result.isSuccess) return result as Result<SessionUser>
    result.value = SessionUserSchema.parse(result.value)
    return result as Result<SessionUser>
  }

  static async getLoginProviders(): Promise<Result<{ name: string; url: string }[]>> {
    const result = await get<unknown>(`${IDENTITY}/auth/login/providers`)
    return result as Result<{ name: string; url: string }[]>
  }

  static async forgotPassword(email: string): Promise<Result<void>> {
    return await post(`${IDENTITY}/passwords/forgot`, { email })
  }

  static async resetPassword(token: string, newPassword: string): Promise<Result<void>> {
    return await post(`${IDENTITY}/passwords/reset`, { token, newPassword })
  }

  static async changePassword(currentPassword: string, newPassword: string): Promise<Result<void>> {
    return await post(`${IDENTITY}/passwords/change`, { currentPassword, newPassword })
  }
}
```

- [ ] **Step 2: Write `emailApi.ts`**

```typescript
// app/Store/src/features/identity/services/emailApi.ts
import { post } from '@/shared/api/client'
import { IDENTITY } from '@/shared/constants/api'
import type { Result } from '@/shared/types'

export class EmailApi {
  static async changeEmail(newEmail: string): Promise<Result<void>> {
    return await post(`${IDENTITY}/emails/change`, { newEmail })
  }

  static async confirmEmail(token: string): Promise<Result<void>> {
    return await post(`${IDENTITY}/emails/confirm`, { token })
  }

  static async resendVerification(): Promise<Result<void>> {
    return await post(`${IDENTITY}/emails/resend`, {})
  }
}
```

- [ ] **Step 3: Write `sessionApi.ts`**

```typescript
// app/Store/src/features/identity/services/sessionApi.ts
import { get, post } from '@/shared/api/client'
import { IDENTITY } from '@/shared/constants/api'
import { SessionInfoSchema } from '../validations/auth'
import { ResultSchema } from '@/shared/validations/result'
import type { Result } from '@/shared/types'
import type { SessionInfo } from '../types'
import { z } from 'zod'

export class SessionApi {
  static async getSessions(): Promise<Result<SessionInfo[]>> {
    const result = await get<unknown>(`${IDENTITY}/auth/sessions`)
    if (!result.isSuccess) return result as Result<SessionInfo[]>
    return result as Result<SessionInfo[]>
  }

  static async revokeCurrentDevice(): Promise<Result<void>> {
    return await post(`${IDENTITY}/auth/logout`, { revokeAll: false })
  }

  static async revokeAll(): Promise<Result<void>> {
    return await post(`${IDENTITY}/auth/logout`, { revokeAll: true })
  }
}
```

- [ ] **Step 4: Write `tokenService.ts`**

```typescript
// app/Store/src/features/identity/services/tokenService.ts
import type { TokenPair } from '../types'

const ACCESS_KEY = 'accessToken'
const REFRESH_KEY = 'refreshToken'
const ACCESS_EXPIRY_KEY = 'accessTokenExpiresAt'
const REFRESH_EXPIRY_KEY = 'refreshTokenExpiresAt'

export function getAccessToken(): string | null {
  return localStorage.getItem(ACCESS_KEY)
}

export function getRefreshToken(): string | null {
  return localStorage.getItem(REFRESH_KEY)
}

export function setTokens(pair: TokenPair): void {
  localStorage.setItem(ACCESS_KEY, pair.accessToken)
  localStorage.setItem(REFRESH_KEY, pair.refreshToken)
  const accessExpiresAt = Date.now() + pair.accessTokenExpiresIn * 1000
  const refreshExpiresAt = Date.now() + pair.refreshTokenExpiresIn * 1000
  localStorage.setItem(ACCESS_EXPIRY_KEY, String(accessExpiresAt))
  localStorage.setItem(REFRESH_EXPIRY_KEY, String(refreshExpiresAt))
}

export function clearTokens(): void {
  localStorage.removeItem(ACCESS_KEY)
  localStorage.removeItem(REFRESH_KEY)
  localStorage.removeItem(ACCESS_EXPIRY_KEY)
  localStorage.removeItem(REFRESH_EXPIRY_KEY)
}

export function hasValidAccessToken(): boolean {
  const token = getAccessToken()
  if (!token) return false
  const expiry = localStorage.getItem(ACCESS_EXPIRY_KEY)
  if (!expiry) return false
  return Date.now() < Number(expiry) - 30_000 // 30s buffer
}
```

### Task 3.4: authStore

**Files:** Create `app/Store/src/features/identity/stores/authStore.ts`

- [ ] **Step 1: Write `authStore.ts`**

```typescript
// app/Store/src/features/identity/stores/authStore.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { AuthApi, EmailApi } from '../services'
import { getAccessToken, setTokens, clearTokens, hasValidAccessToken, getRefreshToken } from '../services/tokenService'
import { emit } from '@/shared/composables/useStoreEvents'
import type { AuthUser, LoginRequest, RegisterRequest } from '../types'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<AuthUser | null>(null)
  const status = ref<'idle' | 'loading' | 'authenticated' | 'error'>('idle')
  const error = ref<string | null>(null)
  const _initialized = ref(false)

  const isAuthenticated = computed(() => status.value === 'authenticated' && user.value !== null)

  async function init(): Promise<void> {
    if (_initialized.value) return
    _initialized.value = true
    if (!getAccessToken()) { status.value = 'idle'; return }
    status.value = 'loading'
    try {
      const result = await AuthApi.getSession()
      if (result.isSuccess && result.value) {
        user.value = { ...result.value, isAuthenticated: true }
        status.value = 'authenticated'
        emit({ type: 'auth:login', userId: result.value.id })
      } else {
        clearTokens()
        status.value = 'error'
      }
    } catch {
      clearTokens()
      status.value = 'error'
    }
    emit({ type: 'auth:init-done', userId: user.value?.userId ?? '' })
  }

  async function login(credential: string, password: string): Promise<boolean> {
    status.value = 'loading'
    error.value = null
    const result = await AuthApi.login({ credential, password })
    if (result.isSuccess) {
      setTokens(result.value)
      const session = await AuthApi.getSession()
      if (session.isSuccess && session.value) {
        user.value = { ...session.value, isAuthenticated: true }
        status.value = 'authenticated'
        emit({ type: 'auth:login', userId: session.value.id })
        return true
      }
    }
    error.value = result.message ?? 'Login failed'
    status.value = 'error'
    return false
  }

  async function loginWithGoogle(): Promise<void> {
    const result = await AuthApi.getLoginProviders()
    if (result.isSuccess) {
      const google = result.value.find(p => p.name.toLowerCase().includes('google'))
      if (google) window.location.href = google.url
    }
  }

  async function register(req: RegisterRequest): Promise<boolean> {
    status.value = 'loading'
    error.value = null
    const result = await AuthApi.register(req)
    if (result.isSuccess) {
      status.value = 'idle'
      return true
    }
    error.value = result.message ?? 'Registration failed'
    status.value = 'error'
    return false
  }

  async function logout(revokeAll = false): Promise<void> {
    try { await AuthApi.logout({ revokeAll }) } catch {}
    clearTokens()
    user.value = null
    status.value = 'idle'
    emit({ type: 'auth:logout' })
  }

  async function changePassword(current: string, newPwd: string): Promise<boolean> { return (await AuthApi.changePassword(current, newPwd)).isSuccess }
  async function forgotPassword(email: string): Promise<boolean> { return (await AuthApi.forgotPassword(email)).isSuccess }
  async function resetPassword(token: string, newPwd: string): Promise<boolean> { return (await AuthApi.resetPassword(token, newPwd)).isSuccess }
  async function changeEmail(newEmail: string): Promise<boolean> { return (await EmailApi.changeEmail(newEmail)).isSuccess }
  async function confirmEmail(token: string): Promise<boolean> { return (await EmailApi.confirmEmail(token)).isSuccess }
  async function resendVerification(): Promise<boolean> { return (await EmailApi.resendVerification()).isSuccess }

  return {
    user, status, error, isAuthenticated,
    init, login, loginWithGoogle, register, logout,
    changePassword, forgotPassword, resetPassword, changeEmail, confirmEmail, resendVerification,
  }
})
```

### Task 3.5: Identity Skeleton Pages

Create 5 skeleton pages. Each: PrimeVue Breadcrumb + Card + Skeleton + Message placeholder.

- [ ] **Step 1: LoginView.vue**

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useAuthStore } from '../stores/authStore'

usePageTitle('Sign In')
const auth = useAuthStore()
const credential = ref('')
const password = ref('')
</script>
<template>
  <div class="max-w-md mx-auto py-16 px-4">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Sign In' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Sign In</h1>
    <Card>
      <template #content>
        <div class="space-y-4">
          <Skeleton width="100%" height="2.5rem" />
          <Skeleton width="100%" height="2.5rem" />
          <Skeleton width="100%" height="2.5rem" />
        </div>
      </template>
    </Card>
    <Message severity="info" class="mt-4">Login form with PrimeVue InputText, Password, Button, and validation will be implemented here.</Message>
  </div>
</template>
```

- [ ] **Step 2: RegisterView, ForgotPasswordView, ResetPasswordView, SessionsView** — Same skeleton pattern. Each imports `usePageTitle` and appropriate store, shows Card + Skeleton + Message with feature description.

### Task 3.6: Identity Barrels

Create: `types/index.ts`, `validations/index.ts`, `services/index.ts`, `stores/index.ts`, `views/index.ts`, `index.ts`

```typescript
// types/index.ts
export type { TokenPair, AuthUser, LoginRequest, RegisterRequest, SessionUser, SessionInfo } from './auth'
// services/index.ts
export { AuthApi } from './authApi'
export { EmailApi } from './emailApi'
export { SessionApi } from './sessionApi'
// stores/index.ts
export { useAuthStore } from './authStore'
// index.ts
export * from './types'
export * from './services'
export * from './stores'
```

- [ ] Verify compilation + commit: `cd app/Store && npx tsc --noEmit && git add ... && git commit -m "feat(identity): add types, validations, services, authStore, 5 skeleton pages"`

---

## Phase 4: Ordering Domain

### Phase 4 Ledger

| Task | Deliverable | Files |
|------|-------------|-------|
| 4.1 | Ordering types | 3 files in `types/` |
| 4.2 | Ordering validations | 3 files in `validations/` |
| 4.3 | Ordering services | 3 files in `services/` |
| 4.4 | cartStore | 1 file in `stores/` |
| 4.5 | checkoutStore | 1 file in `stores/` |
| 4.6 | orderStore | 1 file in `stores/` |
| 4.7 | useQuickAdd composable | 1 file in `composables/` |
| 4.8 | Skeleton pages (4 views) | 4 files in `views/` |
| 4.9 | Barrels | 6 `index.ts` |

### Task 4.1: Ordering Types

**Files:**
- Create: `app/Store/src/features/ordering/types/cart.ts`
- Create: `app/Store/src/features/ordering/types/checkout.ts`
- Create: `app/Store/src/features/ordering/types/order.ts`

- [ ] **Step 1: Write `cart.ts`**

```typescript
export interface CartLineItem {
  id: string
  variantId: string
  variantName: string
  sku: string
  productName: string | null
  productImageUrl: string | null
  quantity: number
  price: number
  total: number
}

export interface CartResponse {
  id: string
  itemTotal: number
  total: number
  currency: string
  itemCount: number
  checkoutState: string
  items: CartLineItem[]
}

export interface AddCartItemRequest {
  variantId: string
  quantity: number
}

export interface UpdateCartItemRequest {
  quantity: number
}
```

- [ ] **Step 2: Write `checkout.ts`**

```typescript
export interface UpdateCheckoutRequest {
  shipAddressId?: string
  billAddressId?: string
  currency?: string
  email?: string
}

export interface SelectShippingRateRequest {
  shippingMethodId: string
}

export interface CreatePaymentIntentRequest {
  orderId: string
  paymentMethodId: string
  returnUrl?: string
}

export interface PlaceOrderRequest {
  paymentIntentId?: string
}

export interface PlaceOrderResponse { id: string }
export interface PaymentIntentResponse { id: string; clientSecret: string; responseCode?: string }
```

- [ ] **Step 3: Write `order.ts`**

```typescript
export type OrderStatus = 'Draft' | 'Placed' | 'Canceled' | 'Expired'
export type CheckoutState = 'Address' | 'Delivery' | 'Payment' | 'Confirm' | 'Complete'

export interface OrderListItem {
  id: string
  number: string
  status: OrderStatus
  total: number
  createdAtUtc: string
}

export interface OrderDetail extends OrderListItem {
  checkoutState: CheckoutState
  currency: string
  email: string | null
  shipAddressId: string | null
  billAddressId: string | null
  shippingMethodId: string | null
  itemTotal: number
  adjustmentTotal: number
  shipmentTotal: number
  paymentTotal: number
  outstandingBalance: number
  paymentState: string | null
  shipmentState: string | null
  userId: string | null
  storeId: string | null
  approvedById: string | null
  approvedAtUtc: string | null
  completedAtUtc: string | null
  canceledAtUtc: string | null
  modifiedAtUtc: string | null
}

export interface OrderTrackingResponse {
  orderId: string
  orderCreatedAt: string
  orderApprovedAt: string | null
  orderCompletedAt: string | null
  orderCanceledAt: string | null
  shippedAt: string | null
  deliveredAt: string | null
  estimatedDeliveryAt: string | null
}
```

### Task 4.2: Ordering Validations

Create Zod schemas mirroring the types in `validations/cart.ts`, `validations/checkout.ts`, `validations/order.ts`. Pattern identical to catalog — one schema per interface, export `{Entity}Schema`, derive form types where applicable.

### Task 4.3: Ordering Services

Create `services/cartApi.ts`, `services/checkoutApi.ts`, `services/orderApi.ts`. Each: static class, `private static readonly BASE`, Zod-validated API methods. Endpoints use `CART`, `ORDERS`, `PAYMENT` constants.

Key methods:
- `CartApi`: `getCart()`, `addItem(req)`, `updateItem(id, req)`, `removeItem(id)`, `emptyCart()`, `associateCart(guestOrderId)`
- `CheckoutApi`: `updateCheckout(req)`, `selectShippingRate(req)`, `validateCheckout()`, `createPaymentIntent(req)`, `placeOrder(req)`
- `OrderApi`: `getOrders(q)`, `getOrder(id)`, `getOrderTracking(id)`, `cancelOrder(id)`

### Task 4.4: cartStore

```typescript
// stores/cartStore.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { CartApi } from '../services/cartApi'
import { emit, on } from '@/shared/composables/useStoreEvents'
import type { CartLineItem, CartResponse, CartReservationStatus } from '../types'

export const useCartStore = defineStore('cart', () => {
  const id = ref<string | null>(null)
  const items = ref<CartLineItem[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const lastFetchedAt = ref(0)
  const cartToken = crypto.randomUUID()

  const itemCount = computed(() => items.value.reduce((s, i) => s + i.quantity, 0))
  const subtotal = computed(() => items.value.reduce((s, i) => s + i.total, 0))
  const isEmpty = computed(() => items.value.length === 0)

  async function fetchCart(): Promise<boolean> {
    if (loading.value) return false
    if (Date.now() - lastFetchedAt.value < 30_000 && items.value.length > 0) return true
    loading.value = true
    error.value = null
    const result = await CartApi.getCart()
    if (result.isSuccess) {
      id.value = result.value.id
      items.value = result.value.items
      lastFetchedAt.value = Date.now()
      emit({ type: 'cart:updated', itemCount: itemCount.value })
    } else {
      error.value = result.message ?? 'Failed to load cart'
    }
    loading.value = false
    return result.isSuccess
  }

  async function addItem(variantId: string, quantity = 1): Promise<boolean> {
    loading.value = true
    error.value = null
    const result = await CartApi.addItem({ variantId, quantity })
    if (result.isSuccess) {
      id.value = result.value.id
      items.value = result.value.items
      lastFetchedAt.value = Date.now()
      emit({ type: 'cart:updated', itemCount: itemCount.value })
    } else {
      error.value = result.message ?? 'Failed to add item'
    }
    loading.value = false
    return result.isSuccess
  }

  async function updateQuantity(lineItemId: string, quantity: number): Promise<boolean> {
    const prev = items.value.find(i => i.id === lineItemId)
    if (prev) prev.quantity = quantity
    const result = await CartApi.updateItem(lineItemId, { quantity })
    if (result.isSuccess) {
      items.value = result.value.items
      emit({ type: 'cart:updated', itemCount: itemCount.value })
    } else if (prev) {
      prev.quantity = (await CartApi.getCart()).value?.items.find(i => i.id === lineItemId)?.quantity ?? prev.quantity
      error.value = result.message
    }
    return result.isSuccess
  }

  async function removeItem(lineItemId: string): Promise<boolean> {
    const removed = items.value.filter(i => i.id !== lineItemId)
    items.value = removed
    const result = await CartApi.removeItem(lineItemId)
    if (!result.isSuccess) { error.value = result.message; await fetchCart() }
    else emit({ type: 'cart:updated', itemCount: itemCount.value })
    return result.isSuccess
  }

  async function clearCart(): Promise<void> {
    await CartApi.emptyCart()
    items.value = []
    id.value = null
    emit({ type: 'cart:updated', itemCount: 0 })
  }

  async function associateGuestCart(): Promise<void> {
    if (!id.value) return
    await CartApi.associateCart(id.value)
    await fetchCart()
  }

  function reset(): void {
    items.value = []
    id.value = null
    error.value = null
  }

  on('auth:login', () => associateGuestCart())
  on('auth:logout', () => reset())

  return { id, items, loading, error, itemCount, subtotal, isEmpty, cartToken,
    fetchCart, addItem, updateQuantity, removeItem, clearCart, associateGuestCart, reset }
})
```

### Task 4.5: checkoutStore

```typescript
// stores/checkoutStore.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { CheckoutApi } from '../services/checkoutApi'
import { useCartStore } from './cartStore'
import { useRouter } from 'vue-router'
import { emit } from '@/shared/composables/useStoreEvents'

type Step = 1 | 2 | 3 | 4 | 5

export const useCheckoutStore = defineStore('checkout', () => {
  const currentStep = ref<Step>(1)
  const shipAddressId = ref<string | null>(null)
  const shippingMethodId = ref<string | null>(null)
  const paymentMethodId = ref<string | null>(null)
  const paymentIntentId = ref<string | null>(null)
  const paymentClientSecret = ref<string | null>(null)
  const orderId = ref<string | null>(null)
  const email = ref('')
  const loading = ref(false)
  const error = ref<string | null>(null)

  const steps = computed(() => [
    { label: 'Address', number: 1, complete: currentStep.value > 1, current: currentStep.value === 1 },
    { label: 'Delivery', number: 2, complete: currentStep.value > 2, current: currentStep.value === 2 },
    { label: 'Payment', number: 3, complete: currentStep.value > 3, current: currentStep.value === 3 },
    { label: 'Confirm', number: 4, complete: currentStep.value > 4, current: currentStep.value === 4 },
    { label: 'Complete', number: 5, complete: currentStep.value === 5, current: currentStep.value === 5 },
  ])

  function init(): void {
    const cart = useCartStore()
    const router = useRouter()
    if (cart.isEmpty) { router.push('/cart'); return }
    cart.fetchCart()
  }

  async function saveAddress(addressId: string, userEmail: string): Promise<boolean> {
    loading.value = true
    error.value = null
    const result = await CheckoutApi.updateCheckout({ shipAddressId: addressId, billAddressId: addressId, email: userEmail })
    if (result.isSuccess) {
      shipAddressId.value = addressId
      email.value = userEmail
      currentStep.value = 2
    } else { error.value = result.message }
    loading.value = false
    return result.isSuccess
  }

  async function selectShippingRate(methodId: string): Promise<boolean> {
    loading.value = true
    const result = await CheckoutApi.selectShippingRate({ shippingMethodId: methodId })
    if (result.isSuccess) {
      shippingMethodId.value = methodId
      currentStep.value = 3
    } else { error.value = result.message }
    loading.value = false
    return result.isSuccess
  }

  async function createPaymentIntent(methodId: string): Promise<boolean> {
    loading.value = true
    const cart = useCartStore()
    const result = await CheckoutApi.createPaymentIntent({ orderId: cart.id!, paymentMethodId: methodId })
    if (result.isSuccess) {
      paymentIntentId.value = result.value.id
      paymentClientSecret.value = result.value.clientSecret
      paymentMethodId.value = methodId
      currentStep.value = 4
    } else { error.value = result.message }
    loading.value = false
    return result.isSuccess
  }

  async function placeOrder(): Promise<boolean> {
    loading.value = true
    const result = await CheckoutApi.placeOrder(paymentIntentId.value ? { paymentIntentId: paymentIntentId.value } : {})
    if (result.isSuccess) {
      orderId.value = result.value.id
      currentStep.value = 5
      emit({ type: 'checkout:placed', orderId: result.value.id })
    } else { error.value = result.message }
    loading.value = false
    return result.isSuccess
  }

  function reset(): void {
    currentStep.value = 1
    shipAddressId.value = null
    shippingMethodId.value = null
    paymentMethodId.value = null
    paymentIntentId.value = null
    paymentClientSecret.value = null
    orderId.value = null
    error.value = null
  }

  return { currentStep, shipAddressId, shippingMethodId, paymentMethodId, paymentIntentId, paymentClientSecret, orderId, email, loading, error, steps,
    init, saveAddress, selectShippingRate, createPaymentIntent, placeOrder, reset }
})
```

### Task 4.6: orderStore

```typescript
// stores/orderStore.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { OrderApi } from '../services/orderApi'
import { on } from '@/shared/composables/useStoreEvents'
import type { OrderListItem, OrderDetail, OrderTrackingResponse, OrderStatus } from '../types'

export const useOrderStore = defineStore('orders', () => {
  const items = ref<OrderListItem[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const page = ref(1)
  const pageSize = ref(20)
  const totalCount = ref(0)
  const statusFilter = ref<OrderStatus | 'All'>('All')
  const currentOrder = ref<OrderDetail | null>(null)
  const detailLoading = ref(false)
  const cancelLoading = ref(false)

  const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))

  async function fetchOrders(): Promise<void> {
    if (loading.value) return
    loading.value = true
    error.value = null
    const result = await OrderApi.getOrders({ pageNumber: page.value, pageSize: pageSize.value })
    if (result.isSuccess) { items.value = result.items; totalCount.value = result.totalCount }
    else error.value = result.message ?? 'Failed to load orders'
    loading.value = false
  }

  async function fetchOrder(id: string): Promise<void> {
    detailLoading.value = true
    const [detail, tracking] = await Promise.all([OrderApi.getOrder(id), OrderApi.getOrderTracking(id)])
    if (detail.isSuccess) currentOrder.value = detail.value
    else error.value = detail.message
    detailLoading.value = false
  }

  async function cancelOrder(id: string): Promise<boolean> {
    cancelLoading.value = true
    const result = await OrderApi.cancelOrder(id)
    if (result.isSuccess) {
      const item = items.value.find(o => o.id === id)
      if (item) item.status = 'Canceled'
      if (currentOrder.value?.id === id) currentOrder.value.status = 'Canceled'
    } else error.value = result.message
    cancelLoading.value = false
    return result.isSuccess
  }

  function nextPage(): void { if (page.value < totalPages.value) { page.value++; fetchOrders() } }
  function prevPage(): void { if (page.value > 1) { page.value--; fetchOrders() } }
  function refresh(): void { fetchOrders() }

  on('checkout:placed', () => refresh())

  return { items, loading, error, page, pageSize, totalCount, totalPages, statusFilter, currentOrder, detailLoading, cancelLoading,
    fetchOrders, fetchOrder, cancelOrder, nextPage, prevPage, refresh }
})
```

### Task 4.7: useQuickAdd Composable

```typescript
// composables/useQuickAdd.ts
import { useCartStore } from '../stores/cartStore'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'

export function useQuickAdd() {
  const cart = useCartStore()
  const notify = useNotify()
  const { handleError } = useApiErrorHandler()

  async function add(variantId: string): Promise<boolean> {
    if (!variantId) { notify.warn('Unavailable'); return false }
    const ok = await cart.addItem(variantId, 1)
    if (ok) notify.success('Added to cart')
    else handleError(new Error(cart.error ?? 'Failed'))
    return ok
  }

  return { add }
}
```

### Task 4.8: Ordering Skeleton Pages

4 skeleton pages — same PrimeVue pattern. Each: Breadcrumb + Card + Skeleton + Message with feature description.

- **CartView.vue** — `useCartStore` init, fetch cart on mount. Placeholder for cart items list, order summary, checkout CTA.
- **CheckoutView.vue** — `useCheckoutStore` init. Placeholder for 5-step stepper, address/delivery/payment/confirm steps.
- **OrderListView.vue** — `useOrderStore` init, fetch orders. Placeholder for order cards list, status filter, pagination.
- **OrderDetailView.vue** — `useOrderStore.fetchOrder(id)` from route param. Placeholder for order details, timeline, cancel action.

### Task 4.9: Ordering Barrels

Create all `index.ts` barrels — types, validations, services, stores, composables, views, domain root.

---

## Phase 5: Profile Domain

### Phase 5 Ledger

| Task | Deliverable | Files |
|------|-------------|-------|
| 5.1 | Profile types | 5 files in `types/` |
| 5.2 | Profile validations | 5 files in `validations/` |
| 5.3 | Profile services | 5 files in `services/` |
| 5.4 | profileStore | 1 file in `stores/` |
| 5.5 | addressStore | 1 file in `stores/` |
| 5.6 | wishlistStore | 1 file in `stores/` |
| 5.7 | Skeleton pages (6 views) | 6 files in `views/` |
| 5.8 | Barrels | 5 `index.ts` |

### Task 5.1: Profile Types

Create `profile.ts`, `address.ts`, `wishlist.ts`, `notification.ts`, `preferences.ts`:

```typescript
// profile.ts
export interface ProfileDetail {
  id: string; userId: string; fullName: string; firstName: string; lastName: string;
  email: string; phoneNumber: string | null; dateOfBirth: string | null;
  preferences: Record<string, unknown> | null; notifications: Record<string, boolean> | null;
  emailConfirmed: boolean; phoneNumberConfirmed: boolean;
  createdAtUtc: string; modifiedAtUtc: string | null;
}
export interface UpdateProfileRequest { firstName: string; lastName: string; email: string; phoneNumber?: string }

// address.ts
export type AddressType = 'Shipping' | 'Billing' | 'Other'
export interface Address {
  id: string; userId: string; addressType: AddressType; firstName: string; lastName: string | null;
  address1: string; address2: string | null; city: string; zipCode: string | null;
  phone: string | null; label: string | null; isDefault: boolean;
  countryName: string; stateProvince: string | null; countryCode: string | null; stateCode: string | null;
}
export interface AddressInput {
  addressType: AddressType; firstName: string; lastName?: string; address1: string; address2?: string;
  city: string; zipCode?: string; phone?: string; label?: string; isDefault: boolean;
  countryName: string; stateProvince?: string; countryCode?: string; stateCode?: string;
}

// wishlist.ts
export interface WishlistListItem { id: string; name: string; isPrivate: boolean; itemCount: number }
export interface WishedItem { id: string; variantId: string; quantity: number; addedAtUtc: string }
export interface WishlistDetail { id: string; name: string; isPrivate: boolean; itemCount: number; token: string; isDefault: boolean; wishedItems: WishedItem[] }
export interface CreateWishlistRequest { name: string; isPrivate: boolean }
export interface UpdateWishlistRequest { name?: string; isPrivate?: boolean; isDefault?: boolean }
export interface AddWishlistItemRequest { variantId: string; quantity: number }

// notification.ts
export interface NotificationPreferences { enableSms: boolean; enableEmail: boolean; enableNewsfeeds: boolean }
```

### Task 5.2: Profile Validations

Zod schemas per entity file — mirror types with runtime validation. Each `validations/{entity}.ts` exports `{Entity}Schema` + derived form types.

### Task 5.3: Profile Services

5 static service classes using `PROFILES` constant:
- `ProfileApi`: `getProfile()`, `updateProfile(req)`, `deleteProfile()`
- `AddressApi`: `getAddresses()`, `createAddress(req)`, `updateAddress(id, req)`, `deleteAddress(id)`, `getDefaultAddress()`
- `WishlistApi`: `getWishlists()`, `getWishlist(id)`, `createWishlist(req)`, `updateWishlist(id, req)`, `deleteWishlist(id)`, `addWishlistItem(id, req)`, `removeWishlistItem(listId, itemId)`
- `NotificationApi`: `getNotificationPreferences()`, `updateNotificationPreferences(req)`
- `AccountApi`: `deleteProfile()` — POST `PROFILES/profiles` with DELETE body

### Task 5.4: profileStore

```typescript
// stores/profileStore.ts
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { ProfileApi } from '../services/profileApi'
import { AccountApi } from '../services/accountApi'
import { useAuthStore } from '@/features/identity/stores/authStore'
import type { ProfileDetail, UpdateProfileRequest } from '../types'

export const useProfileStore = defineStore('profile', () => {
  const profile = ref<ProfileDetail | null>(null)
  const loading = ref(false)
  const saving = ref(false)
  const error = ref<string | null>(null)
  const _initialized = ref(false)

  async function init(): Promise<void> {
    if (_initialized.value) return
    _initialized.value = true
    await fetchProfile()
  }

  async function fetchProfile(): Promise<void> {
    loading.value = true; error.value = null
    const result = await ProfileApi.getProfile()
    if (result.isSuccess) profile.value = result.value
    else error.value = result.message
    loading.value = false
  }

  async function updateProfile(req: UpdateProfileRequest): Promise<boolean> {
    saving.value = true; error.value = null
    const prev = profile.value
    if (prev) Object.assign(prev, req)
    const result = await ProfileApi.updateProfile(req)
    if (!result.isSuccess) { error.value = result.message; profile.value = prev }
    saving.value = false
    return result.isSuccess
  }

  async function deleteProfile(): Promise<boolean> {
    saving.value = true
    const result = await AccountApi.deleteProfile()
    if (result.isSuccess) {
      profile.value = null
      await useAuthStore().logout()
    } else error.value = result.message
    saving.value = false
    return result.isSuccess
  }

  function reset(): void { profile.value = null; error.value = null; _initialized.value = false }

  return { profile, loading, saving, error, init, fetchProfile, updateProfile, deleteProfile, reset }
})
```

### Task 5.5: addressStore

```typescript
// stores/addressStore.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { AddressApi } from '../services/addressApi'
import type { Address, AddressInput } from '../types'

export const useAddressStore = defineStore('addresses', () => {
  const addresses = ref<Address[]>([])
  const loading = ref(false); const saving = ref(false); const error = ref<string | null>(null)

  const defaultAddress = computed(() => addresses.value.find(a => a.isDefault))
  const shippingAddresses = computed(() => addresses.value.filter(a => a.addressType === 'Shipping' || a.addressType === 'Other'))

  async function fetchAddresses(): Promise<void> {
    loading.value = true
    const result = await AddressApi.getAddresses()
    if (result.isSuccess) addresses.value = result.items
    else error.value = result.message
    loading.value = false
  }

  async function createAddress(req: AddressInput): Promise<boolean> {
    saving.value = true
    const result = await AddressApi.createAddress(req)
    if (result.isSuccess) addresses.value.push(result.value)
    else error.value = result.message
    saving.value = false
    return result.isSuccess
  }

  async function updateAddress(id: string, req: AddressInput): Promise<boolean> {
    saving.value = true
    const result = await AddressApi.updateAddress(id, req)
    if (result.isSuccess) {
      const idx = addresses.value.findIndex(a => a.id === id)
      if (idx !== -1) addresses.value[idx] = result.value
    } else error.value = result.message
    saving.value = false
    return result.isSuccess
  }

  async function deleteAddress(id: string): Promise<boolean> {
    saving.value = true
    const result = await AddressApi.deleteAddress(id)
    if (result.isSuccess) addresses.value = addresses.value.filter(a => a.id !== id)
    else error.value = result.message
    saving.value = false
    return result.isSuccess
  }

  return { addresses, loading, saving, error, defaultAddress, shippingAddresses, fetchAddresses, createAddress, updateAddress, deleteAddress }
})
```

### Task 5.6: wishlistStore

```typescript
// stores/wishlistStore.ts
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { WishlistApi } from '../services/wishlistApi'
import { on } from '@/shared/composables/useStoreEvents'
import type { WishlistListItem, WishlistDetail, CreateWishlistRequest, UpdateWishlistRequest, AddWishlistItemRequest } from '../types'

export const useWishlistStore = defineStore('wishlists', () => {
  const lists = ref<WishlistListItem[]>([])
  const loading = ref(false); const saving = ref(false); const error = ref<string | null>(null)
  const details = ref<Record<string, WishlistDetail>>({})
  const wishlistedVariantIds = ref<Set<string>>(new Set())

  async function fetchWishlists(): Promise<void> {
    loading.value = true; error.value = null
    const result = await WishlistApi.getWishlists()
    if (result.isSuccess) {
      lists.value = result.items
      const ids = new Set<string>()
      for (const list of result.items) {
        const detail = await WishlistApi.getWishlist(list.id)
        if (detail.isSuccess) {
          details.value[list.id] = detail.value
          for (const item of detail.value.wishedItems) ids.add(item.variantId)
        }
      }
      wishlistedVariantIds.value = ids
    } else error.value = result.message
    loading.value = false
  }

  async function createWishlist(req: CreateWishlistRequest): Promise<boolean> { saving.value = true; const r = await WishlistApi.createWishlist(req); if (r.isSuccess) lists.value.unshift(r.value); else error.value = r.message; saving.value = false; return r.isSuccess }
  async function updateWishlist(id: string, req: UpdateWishlistRequest): Promise<boolean> { saving.value = true; const r = await WishlistApi.updateWishlist(id, req); if (r.isSuccess && details.value[id]) Object.assign(details.value[id], r.value); else error.value = r.message; saving.value = false; return r.isSuccess }
  async function deleteWishlist(id: string): Promise<boolean> { saving.value = true; const r = await WishlistApi.deleteWishlist(id); if (r.isSuccess) { lists.value = lists.value.filter(l => l.id !== id); delete details.value[id] } else error.value = r.message; saving.value = false; return r.isSuccess }
  async function addItem(listId: string, req: AddWishlistItemRequest): Promise<boolean> { saving.value = true; const r = await WishlistApi.addWishlistItem(listId, req); if (r.isSuccess) { details.value[listId] = r.value; wishlistedVariantIds.value.add(req.variantId) } else error.value = r.message; saving.value = false; return r.isSuccess }
  async function removeItem(listId: string, itemId: string): Promise<boolean> { saving.value = true; const r = await WishlistApi.removeWishlistItem(listId, itemId); if (r.isSuccess) { details.value[listId] = r.value; await fetchWishlists() } else error.value = r.message; saving.value = false; return r.isSuccess }

  on('auth:init-done', () => fetchWishlists())

  return { lists, loading, saving, error, details, wishlistedVariantIds, fetchWishlists, createWishlist, updateWishlist, deleteWishlist, addItem, removeItem }
})
```

### Task 5.7: Profile Skeleton Pages

6 skeleton pages — same PrimeVue pattern: Card + Skeleton + Message with feature description.

- **ProfileView.vue** — Profile form fields with vee-validate placeholder
- **AddressBookView.vue** — Address list + create/edit dialog placeholder
- **ChangePasswordView.vue** — Current/new/confirm password form placeholder
- **NotificationPrefsView.vue** — Toggle switches for email/SMS/newsfeeds
- **PreferencesView.vue** — Style/size/color preferences form placeholder
- **WishlistsView.vue** — Wishlist cards grid + create form placeholder

### Task 5.8: Profile Barrels

Standard barrel files for types, validations, services, stores, views, domain root.

---

## Phase 6: Supporting Domains (Inventory, Payment, Shipping, Location)

### Phase 6 Ledger

| Task | Domain | Deliverable | Files |
|------|--------|-------------|-------|
| 6.1 | inventory | Types + validations + services + store | 5 files |
| 6.2 | payment | Types + validations + services + composable | 5 files |
| 6.3 | shipping | Types + validations + services + store | 5 files |
| 6.4 | location | Types + validations + services + store + composable | 7 files |
| 6.5 | All | Barrels | 4 domain `index.ts` |

### Task 6.1: Inventory Domain

**Types (`availability.ts`):**
```typescript
export interface AvailabilityEntry { stockLocationId: string; locationName: string; countOnHand: number; reservedCount: number; availableCount: number; backorderable: boolean; available: boolean }
export interface ReserveStockRequest { variantId: string; stockLocationId?: string; quantity: number; orderId?: string; ttlMinutes?: number; reason?: string }
export interface CartReservation { id: string; variantId: string; stockLocationId: string | null; orderId: string | null; quantity: number; state: string; expiresAtUtc: string; reason: string | null }
export interface CartReservationStatus extends CartReservation { remainingSeconds: number }
```

**Services (`availabilityApi.ts`, `reservationApi.ts`):**
- `AvailabilityApi.check(variantId)` — GET `AVAILABILITY/{variantId}`
- `ReservationApi.reserveStock(req, token)`, `releaseReservation(id)`, `getCartReservations(token)`

**Store (`availabilityStore.ts`):**
```typescript
export const useAvailabilityStore = defineStore('availability', () => {
  const cache = ref<Record<string, { entry: AvailabilityEntry; fetchedAt: number }>>({})
  const loading = ref(false)
  const pendingIds = ref<Set<string>>(new Set())

  async function check(variantId: string): Promise<AvailabilityEntry | null> {
    const cached = cache.value[variantId]
    if (cached && Date.now() - cached.fetchedAt < 60_000) return cached.entry
    if (pendingIds.value.has(variantId)) return null
    pendingIds.value.add(variantId)
    const result = await AvailabilityApi.check(variantId)
    pendingIds.value.delete(variantId)
    if (result.isSuccess && result.items.length > 0) {
      cache.value[variantId] = { entry: result.items[0], fetchedAt: Date.now() }
      return result.items[0]
    }
    return null
  }

  async function checkBatch(variantIds: string[]): Promise<void> {
    const uncached = variantIds.filter(id => !cache.value[id] || Date.now() - cache.value[id].fetchedAt > 60_000)
    await Promise.all(uncached.slice(0, 10).map(id => check(id)))
  }

  function invalidate(variantId: string): void { delete cache.value[variantId] }

  return { cache, loading, pendingIds, check, checkBatch, invalidate }
})
```

### Task 6.2: Payment Domain

**Types (`payment.ts`):**
```typescript
export interface PaymentMethod { id: string; name: string; code: string | null; description: string | null; providerKey: string; active: boolean; autoCapture: boolean; position: number }
export interface PaymentIntent { id: string; amount: number; currency: string; orderId: string; paymentMethodId: string; state: string; clientSecret: string | null }
export interface CreatePaymentIntentRequest { orderId: string; paymentMethodId?: string; returnUrl?: string }
export interface CreateSetupIntentRequest { paymentMethodId: string }
```

**Services (`paymentApi.ts`):** `PaymentApi.getPaymentMethods()`, `confirmPayment(id)`, `createSetupIntent(req)`

**Composable (`useStripe.ts`):**
```typescript
import { ref } from 'vue'
import { loadStripe, type Stripe, type StripeElements } from '@stripe/stripe-js'

let stripePromise: Promise<Stripe | null> | null = null
let cardElement: StripeElements | null = null
const loading = ref(false)
const error = ref<string | null>(null)

export function useStripe() {
  async function init(publishableKey?: string): Promise<void> {
    if (stripePromise) return
    loading.value = true
    stripePromise = loadStripe(publishableKey ?? import.meta.env.VITE_STRIPE_KEY)
    loading.value = false
  }

  async function mount(clientSecret: string, container: HTMLElement): Promise<boolean> {
    const stripe = await stripePromise
    if (!stripe) { error.value = 'Stripe not loaded'; return false }
    cardElement = stripe.elements({ clientSecret })
    cardElement.create('payment').mount(container)
    return true
  }

  function unmount(): void {
    cardElement?.getElement('payment')?.unmount()
    cardElement = null
  }

  return { loading, error, stripePromise, init, mount, unmount }
}
```

### Task 6.3: Shipping Domain

**Types (`shipping.ts`):**
```typescript
export interface ShippingMethod { id: string; name: string; adminName: string | null; code: string | null; calculatorType: string; position: number }
export interface ShippingRate { id: string; shippingMethodId: string; name: string; cost: number; finalPrice: number; deliveryRange: string | null; freeShippingThreshold: number | null }
export interface ShippingCalculation { shippingMethodId: string; methodName: string; cost: number; currency: string; isFreeShipping: boolean }
```

**Services (`shippingApi.ts`):** `ShippingApi.getShippingMethods()`, `getShippingRates()`, `calculateShipping(req)`

**Store (`shippingStore.ts`):**
```typescript
export const useShippingStore = defineStore('shipping', () => {
  const methods = ref<ShippingMethod[]>([]); const rates = ref<ShippingRate[]>([])
  const selectedMethodId = ref<string | null>(null)
  const loading = ref(false); const error = ref<string | null>(null)

  async function fetchMethods(): Promise<void> {
    if (methods.value.length > 0) return
    loading.value = true; const result = await ShippingApi.getShippingMethods()
    if (result.isSuccess) methods.value = result.items; else error.value = result.message
    loading.value = false
  }

  async function fetchRates(orderId: string): Promise<void> {
    loading.value = true; const result = await ShippingApi.getShippingRates({ orderId })
    if (result.isSuccess) rates.value = result.items; else error.value = result.message
    loading.value = false
  }

  function selectMethod(id: string): void { selectedMethodId.value = id }

  return { methods, rates, selectedMethodId, loading, error, fetchMethods, fetchRates, selectMethod }
})
```

### Task 6.4: Location Domain

**Types (`location.ts`):**
```typescript
export interface Country { id: string; name: string; isoCode: string; callingCode: string | null; statesRequired: boolean; isActive: boolean }
export interface State { id: string; name: string; abbreviation: string; countryId: string; isActive: boolean; countryName: string | null }
```

**Services:** `CountryApi.getCountries()`, `StateApi.getStates()`

**Store (`locationStore.ts`):**
```typescript
export const useLocationStore = defineStore('location', () => {
  const countries = ref<Country[]>([]); const states = ref<State[]>([])
  const selectedCountryId = ref<string | null>(null); const selectedStateId = ref<string | null>(null)
  const loading = ref(false); const _initialized = ref(false)

  const filteredStates = computed(() => states.value.filter(s => s.countryId === selectedCountryId.value))
  const statesRequired = computed(() => countries.value.find(c => c.id === selectedCountryId.value)?.statesRequired ?? false)

  async function loadAll(): Promise<void> {
    if (_initialized.value) return; _initialized.value = true; loading.value = true
    const [c, s] = await Promise.all([CountryApi.getCountries(), StateApi.getStates()])
    if (c.isSuccess) countries.value = c.items; if (s.isSuccess) states.value = s.items
    loading.value = false
  }

  function selectCountry(id: string): void { selectedCountryId.value = id; selectedStateId.value = null }

  return { countries, states, selectedCountryId, selectedStateId, loading, filteredStates, statesRequired, loadAll, selectCountry }
})
```

**Composable (`useLocationCascade.ts`):** Wraps `useLocationStore` for address forms. Exposes `countries`, `states`, `selectedCountryId`, `selectedStateId`, `loading`, `loadCountries()`.

### Task 6.5: Supporting Domain Barrels

Standard `index.ts` barrels for each domain — types, validations, services, stores, composables, domain root.

---

## Phase 7: Layout + Router Wiring

### Phase 7 Ledger

| Task | Deliverable | Files |
|------|-------------|-------|
| 7.1 | App.vue | 1 file (modify) |
| 7.2 | DefaultLayout | 1 file (modify) |
| 7.3 | AuthLayout | 1 file (modify) |
| 7.4 | AccountLayout | 1 file (modify) |
| 7.5 | AppHeader | 1 file (modify) |
| 7.6 | MobileNav, AppFooter, ThemeToggle | 3 files |

### Task 7.1: App.vue

```vue
<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import { setNotifyToast } from '@/shared/api/notify'
import { useTheme } from '@/shared/composables/useTheme'
import { useSearch } from '@/features/catalog/composables/useSearch'
import SearchOverlay from '@/features/catalog/components/SearchOverlay.vue'

useTheme()
const toast = useToast()
setNotifyToast(toast)
const search = useSearch()

function onGlobalKeyDown(e: KeyboardEvent): void {
  if ((e.metaKey || e.ctrlKey) && e.key === 'k') { e.preventDefault(); search.open() }
}

onMounted(() => document.addEventListener('keydown', onGlobalKeyDown))
onUnmounted(() => document.removeEventListener('keydown', onGlobalKeyDown))
</script>
<template>
  <Toast />
  <ScrollTop :threshold="500" icon="pi pi-arrow-up" />
  <SearchOverlay />
  <router-view />
</template>
```

### Task 7.2-7.4: Layouts

**DefaultLayout.vue:** Wraps `<AppHeader />` + `<main><router-view /></main>` + `<AppFooter />`
**AuthLayout.vue:** Centered card layout with logo + `<router-view />`
**AccountLayout.vue:** Header + sidebar nav + content area. Sidebar uses `<router-link>` with active state. 8 nav items (Orders, Addresses, Profile, Sessions, Wishlists, Notifications, Change Password, Preferences).

### Task 7.5: AppHeader.vue

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/features/identity/stores/authStore'
import { useCartStore } from '@/features/ordering/stores/cartStore'
import { useSearch } from '@/features/catalog/composables/useSearch'
import MobileNav from './MobileNav.vue'
import CartDrawer from '@/features/ordering/components/CartDrawer.vue'
import ThemeToggle from '@/app/components/ThemeToggle.vue'

const auth = useAuthStore()
const cart = useCartStore()
const search = useSearch()
const mobileMenuOpen = ref(false)
const cartDrawerOpen = ref(false)
</script>
<template>
  <header class="bg-white border-b border-neutral-200 sticky top-0 z-50">
    <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex items-center justify-between h-14 gap-4">
        <router-link to="/" class="text-lg font-semibold tracking-tight text-neutral-900 shrink-0">ReSys.Shop</router-link>
        <nav class="hidden md:flex items-center gap-6">
          <router-link to="/shop" class="text-sm font-medium text-neutral-600 hover:text-neutral-900 transition-colors">Shop</router-link>
          <router-link to="/collections" class="text-sm font-medium text-neutral-600 hover:text-neutral-900 transition-colors">Collections</router-link>
          <router-link to="/recommendations" class="text-sm font-medium text-neutral-600 hover:text-neutral-900 transition-colors">Visual Search</router-link>
        </nav>
        <div class="flex items-center gap-1">
          <Button icon="pi pi-search" text rounded aria-label="Search" @click="search.open()" />
          <Button icon="pi pi-shopping-cart" text rounded class="relative" @click="cartDrawerOpen = true" />
          <Tag v-if="cart.itemCount > 0" :value="String(cart.itemCount)" severity="contrast" class="absolute -top-0.5 -right-0.5 min-w-[18px] h-[18px] text-[10px] p-0" />
          <ThemeToggle />
          <template v-if="auth.isAuthenticated">
            <Button icon="pi pi-user" text rounded as="router-link" to="/account/orders" aria-label="Account" class="hidden md:flex" />
          </template>
          <template v-else>
            <Button label="Sign In" text size="small" as="router-link" to="/login" class="hidden md:inline-flex" />
          </template>
          <Button icon="pi pi-bars" text rounded class="md:hidden" @click="mobileMenuOpen = !mobileMenuOpen" />
        </div>
      </div>
    </div>
    <MobileNav v-if="mobileMenuOpen" @close="mobileMenuOpen = false" />
    <CartDrawer v-model:visible="cartDrawerOpen" />
  </header>
</template>
```

### Task 7.6: MobileNav, AppFooter, ThemeToggle

**MobileNav.vue:** PrimeVue components — overlay + slide-out panel with `<Button icon="pi pi-times">` close, `<router-link>` items (Shop, Cart, Orders).

**AppFooter.vue:** 4-column grid. Logo + social icons (PrimeVue `<Button icon>`). Link columns using `<router-link>` for internal, `<a>` for external (Help Center, Shipping Info, Returns, Size Guide, Careers). Bottom bar with copyright + payment icons.

**ThemeToggle.vue:**
```vue
<script setup lang="ts">
import { useTheme } from '@/shared/composables/useTheme'
const { isDark, toggle } = useTheme()
</script>
<template>
  <Button :icon="isDark ? 'pi pi-sun' : 'pi pi-moon'" severity="secondary" text rounded :aria-label="isDark ? 'Switch to light mode' : 'Switch to dark mode'" @click="toggle" />
</template>
```

---

## Phase 8: Final Verification

### Task 8.1: Full TypeScript Check

```bash
cd app/Store && npx tsc --noEmit
```

All type errors must resolve. Check imports across domains match barrel exports.

### Task 8.2: Vite Build

```bash
cd app/Store && npx vite build
```

Check for: missing module errors, unresolved imports, circular dependency warnings.

### Task 8.3: Run Test Suites

```bash
cd app/Store && npx vitest run
```

All 257 existing tests must pass. No new tests in this plan — only ensuring existing tests still pass after the rebuild.

---

## Final Phase Ledger Summary

| Phase | Tasks | Files Created | Files Modified |
|-------|-------|---------------|----------------|
| 1 — Shared | 5 | 6 | 0 |
| 2 — Catalog | 16 | ~40 | 0 |
| 3 — Identity | 12 | ~15 | 0 |
| 4 — Ordering | 14 | ~20 | 0 |
| 5 — Profile | 14 | ~20 | 0 |
| 6 — Supporting | 8 | ~15 | 0 |
| 7 — Layout | 6 | ~15 | ~3 |
| 8 — Verification | 3 | 0 | 0 |
| **Total** | **78** | **~130** | **~3** |
