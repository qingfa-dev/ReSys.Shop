# Catalog Products — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build Admin SPA management UI for Catalog Product CRUD with 6-tab detail form (General, SEO, Fashion, Timing, Option Types, Classifications) using PrimeVue PickList for multi-select assignment.

**Architecture:** Full Location-module replication (types → services → stores → validations → views). 6-tab Product detail form. Dual-panel PickList for OptionType and Classification assignment using backend Sync endpoints.

**Tech Stack:** Vue 3 + TypeScript, PrimeVue v5 (PickList, Tabs, DataTable, Dialog), Zod, Pinia, Vitest, @primeicons/vue

## Global Constraints

- Must pass `pnpm run lint` and `pnpm run build-only` with zero errors
- Must pass all existing 537 tests (no regressions)
- No new npm dependencies — reuse existing PrimeVue PickList component
- Routes already wired — no changes to `catalog/routes/index.ts`
- `catalog-products` and `catalog-product-detail` route names already defined
- `isEdit` must exclude `route.params.id === 'new'`
- Follow existing conventions: no comments, static API classes, Zod individual + combined schema, Pinia loaded guard, inline PrimeVue components
- `ProductApi.activateProduct` and `ProductApi.discontinueProduct` return `Result<void>` — check `result.isSuccess`
- PickList uses PrimeVue `PickList` component with `v-model:model`, `:source`, `:target`, `@move-to-target`, `@move-to-source`

## File Structure

```
catalog/
├── types/
│   ├── product.ts              (new)
│   └── index.ts                (modify)
├── services/
│   ├── productApi.ts           (new)
│   ├── productOptionTypeApi.ts (new)
│   ├── productClassificationApi.ts (new)
│   └── index.ts                (modify)
├── stores/
│   ├── productStore.ts         (new)
│   └── index.ts                (modify)
├── validations/
│   ├── product.ts              (new)
│   └── index.ts                (modify)
├── views/
│   ├── ProductsList.vue        (modify — replace stub)
│   ├── ProductDetail.vue       (modify — replace stub)
│   └── index.ts                (modify)
├── __tests__/
│   ├── types/product.spec.ts     (new)
│   ├── services/productApi.spec.ts (new)
│   └── validations/product.spec.ts (new)
└── routes/
    └── index.ts                (no changes)
```

### Task Summary

| Task | Files |
|------|-------|
| 1 | `types/product.ts`, `types/index.ts`, `__tests__/types/product.spec.ts` |
| 2 | `services/productApi.ts`, `services/productOptionTypeApi.ts`, `services/productClassificationApi.ts`, `services/index.ts`, `__tests__/services/productApi.spec.ts` |
| 3 | `stores/productStore.ts`, `stores/index.ts` |
| 4 | `validations/product.ts`, `validations/index.ts`, `__tests__/validations/product.spec.ts` |
| 5 | `views/ProductsList.vue` |
| 6 | `views/ProductDetail.vue`, `views/index.ts` |

---
### Task 1: Product Types Layer

**Files:**
- Create: `app/Admin/src/features/catalog/types/product.ts`
- Modify: `app/Admin/src/features/catalog/types/index.ts`
- Create: `app/Admin/src/features/catalog/__tests__/types/product.spec.ts`

**Interfaces:**
- Consumes: `QueryingParameters` from `@/shared/types/querying`
- Produces: `ProductRequest`, `ProductListItem`, `ProductDetail`, `ProductQuery`, `PRODUCT_FILTER_FIELDS`, `PRODUCT_SORT_FIELDS`, `toProductQueryParams`

- [ ] **Step 1: Write `types/product.ts`**

```ts
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

- [ ] **Step 2: Modify `types/index.ts`** — append:

```ts
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

- [ ] **Step 3: Write `__tests__/types/product.spec.ts`**

```ts
import { describe, it, expect } from 'vitest'
import { toProductQueryParams, PRODUCT_FILTER_FIELDS, PRODUCT_SORT_FIELDS } from '../../types/product'

describe('toProductQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toProductQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
  })

  it('builds filter for status exact', () => {
    const result = toProductQueryParams({ status: 'Active' })
    expect(result.filter).toBe('status=Active')
  })

  it('builds filter for seasonName contains', () => {
    const result = toProductQueryParams({ season: 'Summer' })
    expect(result.filter).toBe('seasonName*=Summer')
  })

  it('builds filter for taxonId exact', () => {
    const result = toProductQueryParams({ taxonId: 'abc-123' })
    expect(result.filter).toBe('taxonId=abc-123')
  })

  it('combines multiple filters', () => {
    const result = toProductQueryParams({ status: 'Active', season: 'Winter' })
    expect(result.filter).toBe('status=Active,seasonName*=Winter')
  })

  it('builds sort ascending', () => {
    const result = toProductQueryParams({ sortBy: 'name', sortDirection: 'asc' })
    expect(result.sort).toEqual(['name'])
  })

  it('builds sort descending', () => {
    const result = toProductQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-createdAtUtc'])
  })

  it('passes pagination', () => {
    const result = toProductQueryParams({ page: 3, pageSize: 15 })
    expect(result.pageNumber).toBe(3)
    expect(result.pageSize).toBe(15)
  })
})

describe('PRODUCT_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(PRODUCT_FILTER_FIELDS).toEqual([
      'status',
      'seasonName',
      'department',
      'createdAtUtc',
      'availableOn',
    ])
  })
})

describe('PRODUCT_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(PRODUCT_SORT_FIELDS).toEqual([
      'name',
      'createdAtUtc',
      'modifiedAtUtc',
      'availableOn',
      'variantsCount',
    ])
  })
})
```

- [ ] **Step 4: Run tests and commit**
```bash
cd app/Admin && pnpm run test:unit -- run 2>&1 | grep 'Test Files\|Tests'
git add app/Admin/src/features/catalog/types/
git add app/Admin/src/features/catalog/__tests__/types/
git commit -m "feat(catalog): add product type definitions"
```

---

### Task 2: Product Services Layer

**Files:**
- Create: `app/Admin/src/features/catalog/services/productApi.ts`
- Create: `app/Admin/src/features/catalog/services/productOptionTypeApi.ts`
- Create: `app/Admin/src/features/catalog/services/productClassificationApi.ts`
- Modify: `app/Admin/src/features/catalog/services/index.ts`
- Create: `app/Admin/src/features/catalog/__tests__/services/productApi.spec.ts`

**Interfaces:**
- Consumes: `post/get/put/del` from `@/shared/api/client`, `getPaged` from `@/shared/api`, `CATALOG` from `@/shared/constants/api`, types from Task 1
- Produces: `ProductApi`, `ProductOptionTypeApi`, `ProductClassificationApi` static classes for Tasks 5-6

- [ ] **Step 1: Write `services/productApi.ts`**

```ts
import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  ProductRequest,
  ProductListItem,
  ProductDetail,
  ProductQuery,
} from '../types/product'
import {
  toProductQueryParams,
  PRODUCT_FILTER_FIELDS,
  PRODUCT_SORT_FIELDS,
} from '../types/product'

export class ProductApi {
  private static readonly BASE = `${CATALOG}/products`

  static getProducts(query: ProductQuery): Promise<PagedResult<ProductListItem>> {
    return getPaged<ProductListItem>(ProductApi.BASE, toProductQueryParams(query), {
      allowedFilterFields: PRODUCT_FILTER_FIELDS,
      allowedSortFields: PRODUCT_SORT_FIELDS,
    })
  }

  static getProduct(id: string): Promise<Result<ProductDetail>> {
    return get<Result<ProductDetail>>(`${ProductApi.BASE}/${id}`)
  }

  static createProduct(request: ProductRequest): Promise<Result<ProductDetail>> {
    return post<Result<ProductDetail>>(ProductApi.BASE, request)
  }

  static updateProduct(id: string, request: ProductRequest): Promise<Result<ProductDetail>> {
    return put<Result<ProductDetail>>(`${ProductApi.BASE}/${id}`, request)
  }

  static deleteProduct(id: string): Promise<Result<ProductListItem>> {
    return del<Result<ProductListItem>>(`${ProductApi.BASE}/${id}`)
  }

  static activateProduct(id: string): Promise<Result<void>> {
    return post<Result<void>>(`${ProductApi.BASE}/${id}/activate`, {})
  }

  static discontinueProduct(id: string): Promise<Result<void>> {
    return post<Result<void>>(`${ProductApi.BASE}/${id}/discontinue`, {})
  }
}
```

- [ ] **Step 2: Write `services/productOptionTypeApi.ts`**

```ts
import { post, get } from '@/shared/api/client'
import { CATALOG } from '@/shared/constants/api'
import type { Result } from '@/shared/types'

export interface OptionTypeAssignment {
  optionTypeId: string
  name: string
  presentation: string | null
  position: number
  isAssigned: boolean
}

interface OptionTypeSyncItem {
  optionTypeId: string
  position: number
}

export class ProductOptionTypeApi {
  private static getBase(productId: string): string {
    return `${CATALOG}/products/${productId}/option-types`
  }

  static getOptionTypes(productId: string): Promise<Result<{ items: OptionTypeAssignment[] }>> {
    return get<Result<{ items: OptionTypeAssignment[] }>>(ProductOptionTypeApi.getBase(productId))
  }

  static syncOptionTypes(productId: string, items: OptionTypeSyncItem[]): Promise<Result<void>> {
    return post<Result<void>>(`${ProductOptionTypeApi.getBase(productId)}/sync`, { items })
  }
}
```

- [ ] **Step 3: Write `services/productClassificationApi.ts`**

```ts
import { post, get } from '@/shared/api/client'
import { CATALOG } from '@/shared/constants/api'
import type { Result } from '@/shared/types'

export interface ClassificationAssignment {
  taxonId: string
  name: string
  prettyName: string | null
  position: number
  isAssigned: boolean
}

interface ClassificationSyncItem {
  taxonId: string
  position: number
}

export class ProductClassificationApi {
  private static getBase(productId: string): string {
    return `${CATALOG}/products/${productId}/classifications`
  }

  static getClassifications(productId: string): Promise<Result<{ items: ClassificationAssignment[] }>> {
    return get<Result<{ items: ClassificationAssignment[] }>>(ProductClassificationApi.getBase(productId))
  }

  static syncClassifications(productId: string, items: ClassificationSyncItem[]): Promise<Result<void>> {
    return post<Result<void>>(`${ProductClassificationApi.getBase(productId)}/sync`, { items })
  }
}
```

- [ ] **Step 4: Modify `services/index.ts`** — append:

```ts
export { ProductApi } from './productApi'
export { ProductOptionTypeApi } from './productOptionTypeApi'
export { ProductClassificationApi } from './productClassificationApi'
```

- [ ] **Step 5: Write `__tests__/services/productApi.spec.ts`**

```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockPut, mockDel, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<any>(),
  mockGet: vi.fn<any>(),
  mockPut: vi.fn<any>(),
  mockDel: vi.fn<any>(),
  mockGetPaged: vi.fn<any>(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  get: mockGet,
  put: mockPut,
  del: mockDel,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { ProductApi } from '../../services/productApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('ProductApi.getProducts', () => {
  it('calls getPaged with query params', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1, pageSize: 20, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })

    await ProductApi.getProducts({ status: 'Active', page: 1, pageSize: 10 })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/products',
      { filter: 'status=Active', search: null, sort: null, pageNumber: 1, pageSize: 10 },
      expect.objectContaining({ allowedFilterFields: expect.any(Array) }),
    )
  })
})

describe('ProductApi.getProduct', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', name: 'Shirt' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ProductApi.getProduct('abc-123')
    expect(mockGet).toHaveBeenCalledWith('api/catalog/products/abc-123')
  })
})

describe('ProductApi.createProduct', () => {
  it('calls POST with request body', async () => {
    const req = { name: 'Shirt', slug: 'shirt', description: null, metaTitle: null, metaDescription: null, metaKeywords: null, availableOn: null, discontinueOn: null, trackInventory: true, styleCode: null, seasonName: null, materialComposition: null, careInstructions: null, fitNotes: null, department: null, genderTarget: null }
    mockPost.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await ProductApi.createProduct(req)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/products', req)
  })
})

describe('ProductApi.updateProduct', () => {
  it('calls PUT with request body', async () => {
    const req = { name: 'Shirt', slug: 'shirt', description: null, metaTitle: null, metaDescription: null, metaKeywords: null, availableOn: null, discontinueOn: null, trackInventory: false, styleCode: null, seasonName: null, materialComposition: null, careInstructions: null, fitNotes: null, department: null, genderTarget: null }
    mockPut.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ProductApi.updateProduct('abc-123', req)
    expect(mockPut).toHaveBeenCalledWith('api/catalog/products/abc-123', req)
  })
})

describe('ProductApi.deleteProduct', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { id: '1', name: 'Shirt' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ProductApi.deleteProduct('abc-123')
    expect(mockDel).toHaveBeenCalledWith('api/catalog/products/abc-123')
  })
})

describe('ProductApi.activateProduct', () => {
  it('calls POST with activate URL', async () => {
    mockPost.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ProductApi.activateProduct('abc-123')
    expect(mockPost).toHaveBeenCalledWith('api/catalog/products/abc-123/activate', {})
  })
})

describe('ProductApi.discontinueProduct', () => {
  it('calls POST with discontinue URL', async () => {
    mockPost.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ProductApi.discontinueProduct('abc-123')
    expect(mockPost).toHaveBeenCalledWith('api/catalog/products/abc-123/discontinue', {})
  })
})
```

- [ ] **Step 6: Run tests and commit**
```bash
cd app/Admin && pnpm run test:unit -- run 2>&1 | grep 'Test Files\|Tests'
git add app/Admin/src/features/catalog/services/
git add app/Admin/src/features/catalog/__tests__/services/
git commit -m "feat(catalog): add product API services"
```

---

### Task 3: Product Store Layer

**Files:**
- Create: `app/Admin/src/features/catalog/stores/productStore.ts`
- Modify: `app/Admin/src/features/catalog/stores/index.ts`

**Interfaces:**
- Consumes: `defineStore` from `pinia`, `ref` from `vue`, `ProductListItem` from Task 1, `ProductApi` from Task 2
- Produces: `useProductStore` — Pinia store with lazy-once pattern

- [ ] **Step 1: Write `stores/productStore.ts`**

```ts
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

- [ ] **Step 2: Modify `stores/index.ts`** — append:

```ts
export { useProductStore } from './productStore'
```

- [ ] **Step 3: Build and commit**
```bash
cd app/Admin && pnpm run build-only 2>&1 | tail -1
git add app/Admin/src/features/catalog/stores/
git commit -m "feat(catalog): add product store for dropdown caching"
```

---

### Task 4: Product Validations Layer

**Files:**
- Create: `app/Admin/src/features/catalog/validations/product.ts`
- Modify: `app/Admin/src/features/catalog/validations/index.ts`
- Create: `app/Admin/src/features/catalog/__tests__/validations/product.spec.ts`

**Interfaces:**
- Consumes: `z` from `zod`
- Produces: `productName`, `productSlug`, `productSchema`, `ProductForm` for Tasks 5-6

- [ ] **Step 1: Write `validations/product.ts`**

```ts
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

- [ ] **Step 2: Modify `validations/index.ts`** — append:

```ts
export {
  productName,
  productSlug,
  productSchema,
} from './product'
export type { ProductForm } from './product'
```

- [ ] **Step 3: Write `__tests__/validations/product.spec.ts`**

```ts
import { describe, it, expect } from 'vitest'
import {
  productName,
  productSlug,
  productDepartment,
  productGenderTarget,
  productSchema,
} from '../../validations/product'

const validProduct = {
  name: 'Cotton T-Shirt',
  slug: 'cotton-t-shirt',
  description: null,
  metaTitle: null,
  metaDescription: null,
  metaKeywords: null,
  availableOn: null,
  discontinueOn: null,
  trackInventory: true,
  styleCode: null,
  seasonName: null,
  materialComposition: null,
  careInstructions: null,
  fitNotes: null,
  department: null,
  genderTarget: null,
}

describe('productName', () => {
  it('accepts valid name', () => {
    expect(productName.safeParse('Cotton T-Shirt').success).toBe(true)
  })

  it('rejects empty', () => {
    expect(productName.safeParse('').success).toBe(false)
  })

  it('rejects over 255 chars', () => {
    expect(productName.safeParse('A'.repeat(256)).success).toBe(false)
  })
})

describe('productSlug', () => {
  it('accepts valid slug', () => {
    expect(productSlug.safeParse('cotton-t-shirt').success).toBe(true)
  })

  it('rejects uppercase', () => {
    expect(productSlug.safeParse('Cotton-T-Shirt').success).toBe(false)
  })

  it('rejects spaces', () => {
    expect(productSlug.safeParse('cotton t shirt').success).toBe(false)
  })

  it('rejects empty', () => {
    expect(productSlug.safeParse('').success).toBe(false)
  })
})

describe('productDepartment', () => {
  it('accepts valid department', () => {
    expect(productDepartment.safeParse('Mens').success).toBe(true)
  })

  it('rejects over 50 chars', () => {
    expect(productDepartment.safeParse('A'.repeat(51)).success).toBe(false)
  })

  it('accepts null', () => {
    expect(productDepartment.safeParse(null).success).toBe(true)
  })
})

describe('productGenderTarget', () => {
  it('accepts valid gender', () => {
    expect(productGenderTarget.safeParse('Unisex').success).toBe(true)
  })

  it('rejects over 20 chars', () => {
    expect(productGenderTarget.safeParse('A'.repeat(21)).success).toBe(false)
  })
})

describe('productSchema', () => {
  it('accepts valid form', () => {
    const result = productSchema.safeParse(validProduct)
    expect(result.success).toBe(true)
  })

  it('rejects empty name', () => {
    const result = productSchema.safeParse({ ...validProduct, name: '' })
    expect(result.success).toBe(false)
  })

  it('rejects invalid slug', () => {
    const result = productSchema.safeParse({ ...validProduct, slug: 'Invalid Slug' })
    expect(result.success).toBe(false)
  })

  it('accepts null optional fields', () => {
    const result = productSchema.safeParse(validProduct)
    expect(result.success).toBe(true)
  })
})
```

- [ ] **Step 4: Run tests and commit**
```bash
cd app/Admin && pnpm run test:unit -- run 2>&1 | grep 'Test Files\|Tests'
git add app/Admin/src/features/catalog/validations/
git add app/Admin/src/features/catalog/__tests__/validations/
git commit -m "feat(catalog): add product Zod validations"
```

---
### Task 5: ProductsList View

**Files:**
- Modify: `app/Admin/src/features/catalog/views/ProductsList.vue` (replace stub)

**Interfaces:**
- Consumes: `useRouter`, `useConfirm`, `useNotify`, `useDataTableExport`, `usePagedQuery`, `PageShell`, `ProductApi` (Task 2), `ProductListItem` (Task 1), `PRODUCT_FILTER_FIELDS`, `PRODUCT_SORT_FIELDS` (Task 1)
- Produces: View at route `/catalog/products`

- [ ] **Step 1: Replace `views/ProductsList.vue`**

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Toolbar from 'primevue/toolbar'
import Tag from 'primevue/tag'
import Plus from '@primeicons/vue/plus'
import Trash from '@primeicons/vue/trash'
import Upload from '@primeicons/vue/upload'
import { PageShell } from '@panel'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { ProductApi } from '../services/productApi'
import type { ProductListItem } from '../types/product'
import { PRODUCT_FILTER_FIELDS, PRODUCT_SORT_FIELDS } from '../types/product'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()

const { dt, exportCSV } = useDataTableExport()
const selectedItems = ref<ProductListItem[]>([])
const searchTerm = ref('')
const allowedSearchFields = ['name', 'slug']

const {
  items,
  loading,
  setSearch,
  refresh,
} = usePagedQuery<ProductListItem>('api/catalog/products', {
  allowedFilterFields: PRODUCT_FILTER_FIELDS,
  allowedSortFields: PRODUCT_SORT_FIELDS,
  allowedSearchFields,
  defaultSearchFields: allowedSearchFields,
  defaultSearchMode: 'any',
  defaultSort: ['name'],
  defaultPageSize: 20,
})

function navigateToNew() {
  router.push('/catalog/products/new')
}

function navigateToEdit(id: string) {
  router.push(`/catalog/products/${id}`)
}

function onSearch(value: string) {
  searchTerm.value = value
  setSearch(value)
}

function clearSearch() {
  searchTerm.value = ''
  setSearch('')
}

function confirmDelete() {
  if (selectedItems.value.length === 0) return

  confirm.require({
    message: `Are you sure you want to delete ${selectedItems.value.length > 1 ? 'these products' : 'this product'}?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const ids = selectedItems.value.map(i => i.id)
      const names = selectedItems.value.map(i => i.name)
      let failed = 0
      for (const id of ids) {
        const result = await ProductApi.deleteProduct(id)
        if (!result.isSuccess) failed++
      }
      selectedItems.value = []
      refresh()
      if (failed === 0) {
        notify.success(
          ids.length > 1 ? 'Products deleted' : 'Product deleted',
          ids.length > 1
            ? `${ids.length} products have been removed.`
            : `${names[0]} has been removed.`,
        )
      } else {
        notify.error(
          'Delete failed',
          `${failed} of ${ids.length} could not be deleted.`,
        )
      }
    },
  })
}
</script>

<template>
  <PageShell title="Products" description="Manage the product catalog">
    <Card>
      <template #content>
        <Toolbar>
          <template #start>
            <Button label="New" severity="secondary" class="mr-2" @click="navigateToNew">
              <Plus />
            </Button>
            <Button label="Delete" severity="secondary" :disabled="selectedItems.length === 0" @click="confirmDelete">
              <Trash />
            </Button>
          </template>
          <template #end>
            <Button label="Export" severity="secondary" @click="exportCSV">
              <Upload />
            </Button>
          </template>
        </Toolbar>
      </template>
    </Card>

    <DataTable
      ref="dt"
      v-model:selection="selectedItems"
      :value="items"
      :loading="loading"
      :paginator="true"
      :rows="20"
      filter-display="menu"
      data-key="id"
      :global-filter-fields="allowedSearchFields"
      paginator-template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
      :rows-per-page-options="[5, 10, 25]"
      current-page-report-template="Showing {first} to {last} of {totalRecords}"
    >
      <Column selection-mode="multiple" header-style="width: 3rem" />
      <template #header>
        <div class="flex justify-between items-center">
          <IconField>
            <InputIcon><i class="pi pi-search" /></InputIcon>
            <InputText
              :model-value="searchTerm"
              placeholder="Search products..."
              @update:model-value="onSearch($event ?? '')"
            />
          </IconField>
          <Button label="Clear" outlined @click="clearSearch" />
        </div>
      </template>
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
      <Column header="" body-style="text-align: right; width: 6rem">
        <template #body="{ data }">
          <div class="flex justify-end gap-2">
            <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
            <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [data]; confirmDelete()" />
          </div>
        </template>
      </Column>
      <template #empty>
        <div class="text-center py-8 text-muted-color">No products found.</div>
      </template>
    </DataTable>
  </PageShell>
</template>
```

- [ ] **Step 2: Build and commit**
```bash
cd app/Admin && pnpm run build-only 2>&1 | tail -1 && pnpm run lint 2>&1 | tail -3
git add app/Admin/src/features/catalog/views/ProductsList.vue
git commit -m "feat(catalog): implement Products list view"
```

---

### Task 6: ProductDetail View (6-tab form + PickLists)

**Files:**
- Modify: `app/Admin/src/features/catalog/views/ProductDetail.vue` (replace stub)
- Modify: `app/Admin/src/features/catalog/views/index.ts` (if ProductDetail not exported yet)

**Interfaces:**
- Consumes: `useRoute`, `useRouter`, `useNotify`, `useConfirm`, `useApiErrorHandler`, `Tabs`, `TabList`, `Tab`, `TabPanels`, `TabPanel`, `PickList`, `PageShell`, `PageHeading`, `FormSection`, `FormField`, `ProductApi` (Task 2), `ProductOptionTypeApi` (Task 2), `ProductClassificationApi` (Task 2), `productSchema` (Task 4), `ProductForm` (Task 4), `OptionTypeAssignment`, `ClassificationAssignment` (Task 2)
- Produces: View at route `/catalog/products/:id`

- [ ] **Step 1: Replace `views/ProductDetail.vue`**

```vue
<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
import PickList from 'primevue/picklist'
import { PageShell, PageHeading } from '@panel'
import { FormSection, FormField } from '@form'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { ProductApi } from '../services/productApi'
import { ProductOptionTypeApi } from '../services/productOptionTypeApi'
import type { OptionTypeAssignment } from '../services/productOptionTypeApi'
import { ProductClassificationApi } from '../services/productClassificationApi'
import type { ClassificationAssignment } from '../services/productClassificationApi'
import { productSchema } from '../validations/product'
import type { ProductForm } from '../validations/product'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const confirm = useConfirm()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => isEdit.value ? 'Edit Product' : 'New Product')
const activeTab = ref('0')

const form = ref<ProductForm>({
  name: '',
  slug: '',
  description: null,
  metaTitle: null,
  metaDescription: null,
  metaKeywords: null,
  availableOn: null,
  discontinueOn: null,
  trackInventory: true,
  styleCode: null,
  seasonName: null,
  materialComposition: null,
  careInstructions: null,
  fitNotes: null,
  department: null,
  genderTarget: null,
})

const fieldErrors = ref<Record<string, string>>({})
const saving = ref(false)

const unassignedOptionTypes = ref<OptionTypeAssignment[]>([])
const assignedOptionTypes = ref<OptionTypeAssignment[]>([])
const optionTypesLoading = ref(false)

const unassignedClassifications = ref<ClassificationAssignment[]>([])
const assignedClassifications = ref<ClassificationAssignment[]>([])
const classificationsLoading = ref(false)

async function initEditMode(id: string) {
  const result = await ProductApi.getProduct(id)
  if (result.isSuccess) {
    const p = result.value
    form.value = {
      name: p.name,
      slug: p.slug,
      description: p.description,
      metaTitle: p.metaTitle,
      metaDescription: p.metaDescription,
      metaKeywords: p.metaKeywords,
      availableOn: p.availableOn,
      discontinueOn: p.discontinueOn,
      trackInventory: p.trackInventory,
      styleCode: p.styleCode,
      seasonName: p.seasonName,
      materialComposition: p.materialComposition,
      careInstructions: p.careInstructions,
      fitNotes: p.fitNotes,
      department: p.department,
      genderTarget: p.genderTarget,
    }
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

watch(activeTab, (tab) => {
  if (isEdit.value && tab === '4' && unassignedOptionTypes.value.length === 0 && assignedOptionTypes.value.length === 0) {
    loadOptionTypes()
  }
  if (isEdit.value && tab === '5' && unassignedClassifications.value.length === 0 && assignedClassifications.value.length === 0) {
    loadClassifications()
  }
})

async function loadOptionTypes() {
  optionTypesLoading.value = true
  const result = await ProductOptionTypeApi.getOptionTypes(route.params.id as string)
  if (result.isSuccess && result.value?.items) {
    unassignedOptionTypes.value = result.value.items.filter(i => !i.isAssigned)
    assignedOptionTypes.value = result.value.items.filter(i => i.isAssigned)
  }
  optionTypesLoading.value = false
}

async function loadClassifications() {
  classificationsLoading.value = true
  const result = await ProductClassificationApi.getClassifications(route.params.id as string)
  if (result.isSuccess && result.value?.items) {
    unassignedClassifications.value = result.value.items.filter(i => !i.isAssigned)
    assignedClassifications.value = result.value.items.filter(i => i.isAssigned)
  }
  classificationsLoading.value = false
}

async function saveOptionTypes() {
  const items = assignedOptionTypes.value.map((a, i) => ({
    optionTypeId: a.optionTypeId,
    position: i,
  }))
  const result = await ProductOptionTypeApi.syncOptionTypes(route.params.id as string, items)
  if (result.isSuccess) {
    notify.success('Option types saved')
    await loadOptionTypes()
  } else {
    notify.error('Failed to save option types', result.errors?.[0]?.message)
  }
}

async function saveClassifications() {
  const items = assignedClassifications.value.map((a, i) => ({
    taxonId: a.taxonId,
    position: i,
  }))
  const result = await ProductClassificationApi.syncClassifications(route.params.id as string, items)
  if (result.isSuccess) {
    notify.success('Classifications saved')
    await loadClassifications()
  } else {
    notify.error('Failed to save classifications', result.errors?.[0]?.message)
  }
}

async function onSave() {
  fieldErrors.value = {}
  const parsed = productSchema.safeParse(form.value)

  if (!parsed.success) {
    for (const issue of parsed.error.issues) {
      const field = String(issue.path[0])
      if (!fieldErrors.value[field]) {
        fieldErrors.value[field] = issue.message
      }
    }
    return
  }

  saving.value = true
  const data = parsed.data
  const request = {
    name: data.name,
    slug: data.slug,
    description: data.description ?? null,
    metaTitle: data.metaTitle ?? null,
    metaDescription: data.metaDescription ?? null,
    metaKeywords: data.metaKeywords ?? null,
    availableOn: data.availableOn ?? null,
    discontinueOn: data.discontinueOn ?? null,
    trackInventory: data.trackInventory,
    styleCode: data.styleCode ?? null,
    seasonName: data.seasonName ?? null,
    materialComposition: data.materialComposition ?? null,
    careInstructions: data.careInstructions ?? null,
    fitNotes: data.fitNotes ?? null,
    department: data.department ?? null,
    genderTarget: data.genderTarget ?? null,
  }

  const result = isEdit.value
    ? await ProductApi.updateProduct(route.params.id as string, request)
    : await ProductApi.createProduct(request)

  saving.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Product updated' : 'Product created')
    if (!isEdit.value && result.value) {
      const created = result.value
      form.value = {
        ...form.value,
        name: created.name,
        slug: created.slug,
        description: created.description,
        metaTitle: created.metaTitle,
        metaDescription: created.metaDescription,
        metaKeywords: created.metaKeywords,
        availableOn: created.availableOn,
        discontinueOn: created.discontinueOn,
        trackInventory: created.trackInventory,
        styleCode: created.styleCode,
        seasonName: created.seasonName,
        materialComposition: created.materialComposition,
        careInstructions: created.careInstructions,
        fitNotes: created.fitNotes,
        department: created.department,
        genderTarget: created.genderTarget,
      }
      router.replace(`/catalog/products/${created.id}`)
    }
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/catalog/products')
}
</script>

<template>
  <PageShell :title="pageTitle">
    <PageHeading
      title=""
      :breadcrumbs="[
        { label: 'Home', to: '/' },
        { label: 'Products', to: '/catalog/products' },
        { label: pageTitle },
      ]"
      :actions="[
        { label: 'Save', icon: 'pi pi-check', severity: 'primary' },
        { label: 'Cancel', icon: 'pi pi-times' },
      ]"
      @action="(i: number) => i === 0 ? onSave() : onCancel()"
    />

    <Tabs v-model:value="activeTab">
      <TabList>
        <Tab value="0">General</Tab>
        <Tab value="1">SEO</Tab>
        <Tab value="2">Fashion</Tab>
        <Tab value="3">Timing</Tab>
        <Tab v-if="isEdit" value="4">Option Types</Tab>
        <Tab v-if="isEdit" value="5">Classifications</Tab>
      </TabList>

      <TabPanels>
        <TabPanel value="0">
          <FormSection title="Product Details">
            <FormField label="Name" :required="true" :invalid="!!fieldErrors.name">
              <InputText v-model="form.name" fluid class="w-full" />
              <small v-if="fieldErrors.name" class="text-red-500">{{ fieldErrors.name }}</small>
            </FormField>
            <FormField label="Slug" :required="true" :invalid="!!fieldErrors.slug" help-text="Lowercase alphanumeric with hyphens">
              <InputText v-model="form.slug" fluid class="w-full" />
              <small v-if="fieldErrors.slug" class="text-red-500">{{ fieldErrors.slug }}</small>
            </FormField>
            <FormField label="Description" :invalid="!!fieldErrors.description">
              <Textarea v-model="form.description" fluid class="w-full" rows="4" />
              <small v-if="fieldErrors.description" class="text-red-500">{{ fieldErrors.description }}</small>
            </FormField>
            <FormField v-if="isEdit" label="Status">
              <Select v-model="form.status" :options="['Draft', 'Active', 'Archived']" fluid />
            </FormField>
          </FormSection>
        </TabPanel>

        <TabPanel value="1">
          <FormSection title="Search Engine Optimization">
            <FormField label="Meta Title" :invalid="!!fieldErrors.metaTitle">
              <InputText v-model="form.metaTitle" fluid class="w-full" />
              <small v-if="fieldErrors.metaTitle" class="text-red-500">{{ fieldErrors.metaTitle }}</small>
            </FormField>
            <FormField label="Meta Description" :invalid="!!fieldErrors.metaDescription">
              <Textarea v-model="form.metaDescription" fluid class="w-full" rows="3" />
              <small v-if="fieldErrors.metaDescription" class="text-red-500">{{ fieldErrors.metaDescription }}</small>
            </FormField>
            <FormField label="Meta Keywords" :invalid="!!fieldErrors.metaKeywords">
              <InputText v-model="form.metaKeywords" fluid class="w-full" />
              <small v-if="fieldErrors.metaKeywords" class="text-red-500">{{ fieldErrors.metaKeywords }}</small>
            </FormField>
          </FormSection>
        </TabPanel>

        <TabPanel value="2">
          <FormSection title="Fashion Attributes">
            <div class="grid grid-cols-2 gap-4">
              <FormField label="Style Code" :invalid="!!fieldErrors.styleCode">
                <InputText v-model="form.styleCode" fluid />
                <small v-if="fieldErrors.styleCode" class="text-red-500">{{ fieldErrors.styleCode }}</small>
              </FormField>
              <FormField label="Season" :invalid="!!fieldErrors.seasonName">
                <InputText v-model="form.seasonName" fluid />
                <small v-if="fieldErrors.seasonName" class="text-red-500">{{ fieldErrors.seasonName }}</small>
              </FormField>
              <FormField label="Department" :invalid="!!fieldErrors.department">
                <InputText v-model="form.department" fluid />
                <small v-if="fieldErrors.department" class="text-red-500">{{ fieldErrors.department }}</small>
              </FormField>
              <FormField label="Gender Target" :invalid="!!fieldErrors.genderTarget">
                <InputText v-model="form.genderTarget" fluid />
                <small v-if="fieldErrors.genderTarget" class="text-red-500">{{ fieldErrors.genderTarget }}</small>
              </FormField>
            </div>
            <FormField label="Material Composition" :invalid="!!fieldErrors.materialComposition">
              <Textarea v-model="form.materialComposition" fluid rows="2" />
              <small v-if="fieldErrors.materialComposition" class="text-red-500">{{ fieldErrors.materialComposition }}</small>
            </FormField>
            <FormField label="Care Instructions" :invalid="!!fieldErrors.careInstructions">
              <Textarea v-model="form.careInstructions" fluid rows="2" />
              <small v-if="fieldErrors.careInstructions" class="text-red-500">{{ fieldErrors.careInstructions }}</small>
            </FormField>
            <FormField label="Fit Notes" :invalid="!!fieldErrors.fitNotes">
              <Textarea v-model="form.fitNotes" fluid rows="2" />
              <small v-if="fieldErrors.fitNotes" class="text-red-500">{{ fieldErrors.fitNotes }}</small>
            </FormField>
          </FormSection>
        </TabPanel>

        <TabPanel value="3">
          <FormSection title="Availability">
            <FormField label="Available On">
              <InputText v-model="form.availableOn" fluid type="date" />
            </FormField>
            <FormField label="Discontinue On">
              <InputText v-model="form.discontinueOn" fluid type="date" />
            </FormField>
            <FormField label="Track Inventory" help-text="Enable inventory tracking for this product">
              <ToggleSwitch v-model="form.trackInventory" />
            </FormField>
          </FormSection>
        </TabPanel>

        <TabPanel v-if="isEdit" value="4">
          <PickList
            v-model:target="assignedOptionTypes"
            :source="unassignedOptionTypes"
            source-header="Available"
            target-header="Assigned"
            :loading="optionTypesLoading"
            list-style="height: 300px"
            source-filter-placeholder="Search..."
            target-filter-placeholder="Search..."
          >
            <template #item="{ item }">
              <div class="flex items-center gap-2">
                <span class="font-medium">{{ item.name }}</span>
                <span class="text-muted-color text-sm">({{ item.presentation }})</span>
              </div>
            </template>
          </PickList>
          <div class="mt-3">
            <Button label="Save Option Types" severity="primary" @click="saveOptionTypes" />
          </div>
        </TabPanel>

        <TabPanel v-if="isEdit" value="5">
          <PickList
            v-model:target="assignedClassifications"
            :source="unassignedClassifications"
            source-header="Unassigned"
            target-header="Assigned"
            :loading="classificationsLoading"
            list-style="height: 300px"
            source-filter-placeholder="Search..."
            target-filter-placeholder="Search..."
          >
            <template #item="{ item }">
              <div class="flex items-center gap-2">
                <span class="font-medium">{{ item.name }}</span>
                <span v-if="item.prettyName" class="text-muted-color text-sm">({{ item.prettyName }})</span>
              </div>
            </template>
          </PickList>
          <div class="mt-3">
            <Button label="Save Classifications" severity="primary" @click="saveClassifications" />
          </div>
        </TabPanel>
      </TabPanels>
    </Tabs>
  </PageShell>
</template>
```

- [ ] **Step 2: Run full build, tests, lint**
```bash
cd app/Admin && pnpm run build-only 2>&1 | tail -1 && pnpm run test:unit -- run 2>&1 | grep 'Test Files\|Tests' && pnpm run lint 2>&1 | tail -3
```

- [ ] **Step 3: Commit**
```bash
git add app/Admin/src/features/catalog/views/
git commit -m "feat(catalog): implement Product detail view with 6-tab form and PickList assignment"
```
