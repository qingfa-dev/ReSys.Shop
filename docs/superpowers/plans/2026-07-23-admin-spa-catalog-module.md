# Admin SPA — Catalog Module Pages

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development
> (recommended) or superpowers:executing-plans to implement this plan task-by-task.
> Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build real Catalog module pages (7 pages) — TypeScript models, API services,
DashboardPage, ProductListPage, ProductDetailPage, TaxonomyListPage, TaxonomyDetailPage,
OptionTypeListPage, OptionTypeDetailPage. Every task uses TDD: write failing test first,
then implement.

**Architecture:** Pages use existing shared components directly (`DataTable`, `PageHeader`,
`FormField`, `TableToolbar`, `ActionMenu`, etc.). API calls use existing Axios `apiClient` +
`resultToMapped`/`pagedResultToMapped`. Each entity gets 1 ListPage + 1 DetailPage
(handles create/view/edit via route-driven mode). Sub-entities (Taxons on TaxonomyDetailPage,
OptionValues on OptionTypeDetailPage) render as `<Fieldset>` sections with inline slideover forms.

**Tech Stack:** Vue 3.5, TypeScript 6, PrimeVue 5, Vitest 4, @vue/test-utils 2

## Global Constraints

- PrimeVue v5 + Aura preset
- Tailwind v4
- Existing shared components at `src/shared/components/`
- Existing API infra at `src/shared/api/` — Axios client, interceptors, mappers
- `src/shared/models/` — `Result<T>`, `PagedResult<T>`, `PaginationMeta` types
- No new npm dependencies
- `useToastNotify` for feedback, `useConfirm` for destructive actions
- Route `:id` param is GUID string
- Every task follows TDD: test first, verify it fails, implement, verify it passes, commit
- Spec: `spec/design-admin-spa-list-detail-pattern.md`

**Prerequisites:** Infrastructure setup plan completed (folders restructured, routes updated, menu updated, deprecated files removed).

---

### Task 1: Catalog TypeScript models

**Files:**
- Create: `app/Admin/src/features/catalog/models/Product.ts`
- Create: `app/Admin/src/features/catalog/models/Taxonomy.ts`
- Create: `app/Admin/src/features/catalog/models/OptionType.ts`
- Create: `app/Admin/src/features/catalog/models/index.ts`

**Interfaces:**
- Consumes: nothing
- Produces: `ProductResponse`, `ProductRequest`, `ProductListParams`, `ProductStatus`, `TaxonomyResponse`, `TaxonomyRequest`, `TaxonomyListParams`, `TaxonResponse`, `TaxonRequest`, `OptionTypeResponse`, `OptionTypeRequest`, `OptionTypeListParams`, `OptionValueResponse`, `OptionValueRequest`

Models are pure TypeScript interfaces — no tests needed. Verification via typecheck.

- [ ] **Step 1: Write Product models**

`app/Admin/src/features/catalog/models/Product.ts`:

```typescript
export interface ProductResponse {
  id: string
  name: string
  slug: string
  description: string | null
  status: ProductStatus
  styleCode: string | null
  seasonName: string | null
  department: string | null
  genderTarget: string | null
  metaTitle: string | null
  metaDescription: string | null
  metaKeywords: string | null
  availableOn: string | null
  discontinueOn: string | null
  materialComposition: string | null
  careInstructions: string | null
  fitNotes: string | null
  createdAt: string
  updatedAt: string
}

export type ProductStatus = 'Draft' | 'Active' | 'Archived'

export interface ProductRequest {
  name: string
  slug: string
  description?: string | null
  status?: ProductStatus
  styleCode?: string | null
  seasonName?: string | null
  department?: string | null
  genderTarget?: string | null
  metaTitle?: string | null
  metaDescription?: string | null
  metaKeywords?: string | null
  availableOn?: string | null
  discontinueOn?: string | null
  materialComposition?: string | null
  careInstructions?: string | null
  fitNotes?: string | null
}

export interface ProductListParams {
  page?: number
  pageSize?: number
  search?: string
  sortBy?: string
  sortDirection?: 'asc' | 'desc'
  status?: ProductStatus
}
```

- [ ] **Step 2: Write Taxonomy models**

`app/Admin/src/features/catalog/models/Taxonomy.ts`:

```typescript
export interface TaxonomyResponse {
  id: string
  name: string
  presentation: string | null
  position: number
  createdAt: string
  updatedAt: string
}

export interface TaxonomyRequest {
  name: string
  presentation?: string | null
  position?: number
}

export interface TaxonomyListParams {
  page?: number
  pageSize?: number
  search?: string
}

export interface TaxonResponse {
  id: string
  name: string
  presentation: string | null
  description: string | null
  slug: string
  position: number
  depth: number
  lft: number
  rgt: number
  childrenCount: number
  hideFromNav: boolean
  automatic: boolean
  taxonomyId: string
  parentId: string | null
  createdAt: string
  updatedAt: string
}

export interface TaxonRequest {
  name: string
  presentation?: string | null
  description?: string | null
  slug?: string
  position?: number
  hideFromNav?: boolean
  automatic?: boolean
  parentId?: string | null
}
```

- [ ] **Step 3: Write OptionType models**

`app/Admin/src/features/catalog/models/OptionType.ts`:

```typescript
export interface OptionTypeResponse {
  id: string
  name: string
  presentation: string | null
  position: number
  filterable: boolean
  createdAt: string
  updatedAt: string
}

export interface OptionTypeRequest {
  name: string
  presentation?: string | null
  position?: number
  filterable?: boolean
}

export interface OptionTypeListParams {
  page?: number
  pageSize?: number
  search?: string
}

export interface OptionValueResponse {
  id: string
  name: string
  presentation: string | null
  position: number
  optionTypeId: string
}

export interface OptionValueRequest {
  name: string
  presentation?: string | null
  position?: number
}
```

- [ ] **Step 4: Write barrel export**

`app/Admin/src/features/catalog/models/index.ts`:

```typescript
export * from './Product'
export * from './Taxonomy'
export * from './OptionType'
```

- [ ] **Step 5: Verify typecheck**

```bash
cd app/Admin && pnpm run typecheck
```

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/features/catalog/models/
git commit -m "feat: add catalog TypeScript model interfaces"
```

---

### Task 2: Catalog API services — Products

**Files:**
- Create: `app/Admin/src/features/catalog/api/products.ts`
- Create: `app/Admin/src/features/catalog/api/__tests__/products.spec.ts`

**Interfaces:**
- Consumes: `apiClient` from `@/shared/api/client`, `resultToMapped`/`pagedResultToMapped` from `@/shared/api/utils/result.mapper`, `ProductResponse`/`ProductRequest`/`ProductListParams` from `../models/Product`
- Produces: `getProducts(params)` → `MappedResult<ProductResponse[]>`, `getProduct(id)` → `MappedResult<ProductResponse>`, `createProduct(data)` → `MappedResult<ProductResponse>`, `updateProduct(id, data)` → `MappedResult<ProductResponse>`, `deleteProduct(id)` → `MappedResult<void>`

- [ ] **Step 1: Write test for products API**

`app/Admin/src/features/catalog/api/__tests__/products.spec.ts`:

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { getProducts, getProduct, createProduct, updateProduct, deleteProduct } from '../products'

const mockGet = vi.fn()
const mockPost = vi.fn()
const mockPut = vi.fn()
const mockDelete = vi.fn()

vi.mock('@/shared/api/client', () => ({
  default: {
    get: (...args: unknown[]) => mockGet(...args),
    post: (...args: unknown[]) => mockPost(...args),
    put: (...args: unknown[]) => mockPut(...args),
    delete: (...args: unknown[]) => mockDelete(...args),
  },
}))

describe('products API', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('getProducts', () => {
    it('calls GET /catalog/products with pagination params', async () => {
      mockGet.mockResolvedValue({
        data: { isSuccess: true, items: [], page: 1, pageSize: 20, totalCount: 0, statusCode: 200 },
      })
      await getProducts({ page: 1, pageSize: 20, search: 'test' })
      expect(mockGet).toHaveBeenCalledWith('/catalog/products', {
        params: { page: 1, pageSize: 20, search: 'test' },
      })
    })

    it('maps paged success response', async () => {
      mockGet.mockResolvedValue({
        data: {
          isSuccess: true,
          value: null,
          items: [{ id: '1', name: 'Test', slug: 'test', status: 'Draft' }],
          page: 1,
          pageSize: 20,
          totalCount: 1,
          statusCode: 200,
        },
      })
      const result = await getProducts()
      expect(result.success).toBe(true)
      expect(result.data).toHaveLength(1)
      expect(result.meta?.totalCount).toBe(1)
    })

    it('maps failure response', async () => {
      mockGet.mockResolvedValue({
        data: { isSuccess: false, value: null, items: [], errors: [{ code: 'ERR', message: 'fail' }], statusCode: 400 },
      })
      const result = await getProducts()
      expect(result.success).toBe(false)
      expect(result.error?.message).toBeDefined()
    })
  })

  describe('getProduct', () => {
    it('calls GET /catalog/products/:id', async () => {
      mockGet.mockResolvedValue({
        data: { isSuccess: true, value: { id: 'abc', name: 'Test', slug: 'test', status: 'Draft' }, statusCode: 200 },
      })
      await getProduct('abc')
      expect(mockGet).toHaveBeenCalledWith('/catalog/products/abc')
    })
  })

  describe('createProduct', () => {
    it('calls POST /catalog/products with body', async () => {
      mockPost.mockResolvedValue({
        data: { isSuccess: true, value: { id: 'new', name: 'New', slug: 'new', status: 'Draft' }, statusCode: 201 },
      })
      await createProduct({ name: 'New', slug: 'new' })
      expect(mockPost).toHaveBeenCalledWith('/catalog/products', { name: 'New', slug: 'new' })
    })
  })

  describe('updateProduct', () => {
    it('calls PUT /catalog/products/:id with body', async () => {
      mockPut.mockResolvedValue({
        data: { isSuccess: true, value: { id: 'abc', name: 'Updated', slug: 'updated', status: 'Active' }, statusCode: 200 },
      })
      await updateProduct('abc', { name: 'Updated', slug: 'updated' })
      expect(mockPut).toHaveBeenCalledWith('/catalog/products/abc', { name: 'Updated', slug: 'updated' })
    })
  })

  describe('deleteProduct', () => {
    it('calls DELETE /catalog/products/:id', async () => {
      mockDelete.mockResolvedValue({
        data: { isSuccess: true, value: null, statusCode: 200 },
      })
      await deleteProduct('abc')
      expect(mockDelete).toHaveBeenCalledWith('/catalog/products/abc')
    })
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

```bash
cd app/Admin && pnpm run test:unit -- --run catalog/api/__tests__/products
```
Expected: FAIL — module `../products` not found.

- [ ] **Step 3: Implement products API**

`app/Admin/src/features/catalog/api/products.ts`:

```typescript
import apiClient from '@/shared/api/client'
import { resultToMapped, pagedResultToMapped } from '@/shared/api/utils/result.mapper'
import type { MappedResult } from '@/shared/api/utils/result.mapper'
import type { Result, PagedResult, PaginationMeta } from '@/shared/models'
import type { ProductResponse, ProductRequest, ProductListParams } from '../models/Product'

export async function getProducts(
  params: ProductListParams = {},
): Promise<MappedResult<ProductResponse[]> & { meta?: PaginationMeta }> {
  const res = await apiClient.get<PagedResult<ProductResponse>>('/catalog/products', { params })
  return pagedResultToMapped(res.data)
}

export async function getProduct(id: string): Promise<MappedResult<ProductResponse>> {
  const res = await apiClient.get<Result<ProductResponse>>(`/catalog/products/${id}`)
  return resultToMapped(res.data)
}

export async function createProduct(data: ProductRequest): Promise<MappedResult<ProductResponse>> {
  const res = await apiClient.post<Result<ProductResponse>>('/catalog/products', data)
  return resultToMapped(res.data)
}

export async function updateProduct(
  id: string,
  data: ProductRequest,
): Promise<MappedResult<ProductResponse>> {
  const res = await apiClient.put<Result<ProductResponse>>(`/catalog/products/${id}`, data)
  return resultToMapped(res.data)
}

export async function deleteProduct(id: string): Promise<MappedResult<void>> {
  const res = await apiClient.delete<Result<void>>(`/catalog/products/${id}`)
  return resultToMapped(res.data)
}
```

- [ ] **Step 4: Run test to verify it passes**

```bash
cd app/Admin && pnpm run test:unit -- --run catalog/api/__tests__/products
```
Expected: PASS — all 7 tests pass.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/catalog/api/
git commit -m "feat: add catalog products API service with tests"
```

---

### Task 3: Catalog API services — Taxonomies and OptionTypes

**Files:**
- Create: `app/Admin/src/features/catalog/api/taxonomies.ts`
- Create: `app/Admin/src/features/catalog/api/optionTypes.ts`
- Create: `app/Admin/src/features/catalog/api/__tests__/taxonomies.spec.ts`
- Create: `app/Admin/src/features/catalog/api/__tests__/optionTypes.spec.ts`
- Create: `app/Admin/src/features/catalog/api/index.ts`

**Interfaces:**
- Consumes: same API infrastructure as Task 2, types from `../models/Taxonomy` and `../models/OptionType`
- Produces: `getTaxonomies`, `getTaxonomy`, `createTaxonomy`, `updateTaxonomy`, `deleteTaxonomy`, `getTaxons`, `createTaxon`, `updateTaxon`, `deleteTaxon`, `getOptionTypes`, `getOptionType`, `createOptionType`, `updateOptionType`, `deleteOptionType`, `getOptionValues`, `createOptionValue`, `updateOptionValue`, `deleteOptionValue`

Pattern identical to Task 2 — same test structure, different endpoints and types.

- [ ] **Step 1: Write tests for taxonomies API**

`app/Admin/src/features/catalog/api/__tests__/taxonomies.spec.ts`:

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'
import {
  getTaxonomies, getTaxonomy, createTaxonomy, updateTaxonomy, deleteTaxonomy,
  getTaxons, createTaxon, updateTaxon, deleteTaxon,
} from '../taxonomies'

const mockGet = vi.fn()
const mockPost = vi.fn()
const mockPut = vi.fn()
const mockDelete = vi.fn()

vi.mock('@/shared/api/client', () => ({
  default: {
    get: (...args: unknown[]) => mockGet(...args),
    post: (...args: unknown[]) => mockPost(...args),
    put: (...args: unknown[]) => mockPut(...args),
    delete: (...args: unknown[]) => mockDelete(...args),
  },
}))

const pagedEmpty = { isSuccess: true, items: [], page: 1, pageSize: 20, totalCount: 0, statusCode: 200 }
const singleOk = (value: unknown) => ({ isSuccess: true, value, statusCode: 200 })

describe('taxonomies API', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('getTaxonomies calls GET /catalog/taxonomies', async () => {
    mockGet.mockResolvedValue({ data: pagedEmpty })
    await getTaxonomies({ page: 1 })
    expect(mockGet).toHaveBeenCalledWith('/catalog/taxonomies', { params: { page: 1 } })
  })

  it('getTaxonomy calls GET /catalog/taxonomies/:id', async () => {
    mockGet.mockResolvedValue({ data: singleOk({ id: '1', name: 'Test' }) })
    await getTaxonomy('1')
    expect(mockGet).toHaveBeenCalledWith('/catalog/taxonomies/1')
  })

  it('createTaxonomy calls POST /catalog/taxonomies', async () => {
    mockPost.mockResolvedValue({ data: singleOk({ id: 'new', name: 'New' }) })
    await createTaxonomy({ name: 'New' })
    expect(mockPost).toHaveBeenCalledWith('/catalog/taxonomies', { name: 'New' })
  })

  it('updateTaxonomy calls PUT /catalog/taxonomies/:id', async () => {
    mockPut.mockResolvedValue({ data: singleOk({ id: '1', name: 'Updated' }) })
    await updateTaxonomy('1', { name: 'Updated' })
    expect(mockPut).toHaveBeenCalledWith('/catalog/taxonomies/1', { name: 'Updated' })
  })

  it('deleteTaxonomy calls DELETE /catalog/taxonomies/:id', async () => {
    mockDelete.mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
    await deleteTaxonomy('1')
    expect(mockDelete).toHaveBeenCalledWith('/catalog/taxonomies/1')
  })

  it('getTaxons calls GET /catalog/taxonomies/:id/taxons', async () => {
    mockGet.mockResolvedValue({ data: singleOk([]) })
    await getTaxons('1')
    expect(mockGet).toHaveBeenCalledWith('/catalog/taxonomies/1/taxons')
  })

  it('createTaxon calls POST /catalog/taxonomies/:id/taxons', async () => {
    mockPost.mockResolvedValue({ data: singleOk({ id: 'new', name: 'Child' }) })
    await createTaxon('1', { name: 'Child' })
    expect(mockPost).toHaveBeenCalledWith('/catalog/taxonomies/1/taxons', { name: 'Child' })
  })

  it('updateTaxon calls PUT with taxonomy and taxon ids', async () => {
    mockPut.mockResolvedValue({ data: singleOk({ id: '2', name: 'Updated' }) })
    await updateTaxon('1', '2', { name: 'Updated' })
    expect(mockPut).toHaveBeenCalledWith('/catalog/taxonomies/1/taxons/2', { name: 'Updated' })
  })

  it('deleteTaxon calls DELETE with taxonomy and taxon ids', async () => {
    mockDelete.mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
    await deleteTaxon('1', '2')
    expect(mockDelete).toHaveBeenCalledWith('/catalog/taxonomies/1/taxons/2')
  })
})
```

- [ ] **Step 2: Write tests for optionTypes API**

`app/Admin/src/features/catalog/api/__tests__/optionTypes.spec.ts`:

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'
import {
  getOptionTypes, getOptionType, createOptionType, updateOptionType, deleteOptionType,
  getOptionValues, createOptionValue, updateOptionValue, deleteOptionValue,
} from '../optionTypes'

const mockGet = vi.fn()
const mockPost = vi.fn()
const mockPut = vi.fn()
const mockDelete = vi.fn()

vi.mock('@/shared/api/client', () => ({
  default: {
    get: (...args: unknown[]) => mockGet(...args),
    post: (...args: unknown[]) => mockPost(...args),
    put: (...args: unknown[]) => mockPut(...args),
    delete: (...args: unknown[]) => mockDelete(...args),
  },
}))

const pagedEmpty = { isSuccess: true, items: [], page: 1, pageSize: 20, totalCount: 0, statusCode: 200 }
const singleOk = (value: unknown) => ({ isSuccess: true, value, statusCode: 200 })

describe('optionTypes API', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('getOptionTypes calls GET /catalog/option-types', async () => {
    mockGet.mockResolvedValue({ data: pagedEmpty })
    await getOptionTypes({ page: 1 })
    expect(mockGet).toHaveBeenCalledWith('/catalog/option-types', { params: { page: 1 } })
  })

  it('getOptionType calls GET /catalog/option-types/:id', async () => {
    mockGet.mockResolvedValue({ data: singleOk({ id: '1', name: 'Size' }) })
    await getOptionType('1')
    expect(mockGet).toHaveBeenCalledWith('/catalog/option-types/1')
  })

  it('createOptionType calls POST /catalog/option-types', async () => {
    mockPost.mockResolvedValue({ data: singleOk({ id: 'new', name: 'Color' }) })
    await createOptionType({ name: 'Color' })
    expect(mockPost).toHaveBeenCalledWith('/catalog/option-types', { name: 'Color' })
  })

  it('updateOptionType calls PUT /catalog/option-types/:id', async () => {
    mockPut.mockResolvedValue({ data: singleOk({ id: '1', name: 'Updated' }) })
    await updateOptionType('1', { name: 'Updated' })
    expect(mockPut).toHaveBeenCalledWith('/catalog/option-types/1', { name: 'Updated' })
  })

  it('deleteOptionType calls DELETE /catalog/option-types/:id', async () => {
    mockDelete.mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
    await deleteOptionType('1')
    expect(mockDelete).toHaveBeenCalledWith('/catalog/option-types/1')
  })

  it('getOptionValues calls GET /catalog/option-types/:id/values', async () => {
    mockGet.mockResolvedValue({ data: singleOk([]) })
    await getOptionValues('1')
    expect(mockGet).toHaveBeenCalledWith('/catalog/option-types/1/values')
  })

  it('createOptionValue calls POST /catalog/option-types/:id/values', async () => {
    mockPost.mockResolvedValue({ data: singleOk({ id: 'new', name: 'Red' }) })
    await createOptionValue('1', { name: 'Red' })
    expect(mockPost).toHaveBeenCalledWith('/catalog/option-types/1/values', { name: 'Red' })
  })

  it('updateOptionValue calls PUT with type and value ids', async () => {
    mockPut.mockResolvedValue({ data: singleOk({ id: '2', name: 'Blue' }) })
    await updateOptionValue('1', '2', { name: 'Blue' })
    expect(mockPut).toHaveBeenCalledWith('/catalog/option-types/1/values/2', { name: 'Blue' })
  })

  it('deleteOptionValue calls DELETE with type and value ids', async () => {
    mockDelete.mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
    await deleteOptionValue('1', '2')
    expect(mockDelete).toHaveBeenCalledWith('/catalog/option-types/1/values/2')
  })
})
```

- [ ] **Step 3: Run tests to verify they fail**

```bash
cd app/Admin && pnpm run test:unit -- --run catalog/api/__tests__/taxonomies catalog/api/__tests__/optionTypes
```
Expected: FAIL — modules not found.

- [ ] **Step 4: Implement taxonomies API**

`app/Admin/src/features/catalog/api/taxonomies.ts`:

```typescript
import apiClient from '@/shared/api/client'
import { resultToMapped, pagedResultToMapped } from '@/shared/api/utils/result.mapper'
import type { MappedResult } from '@/shared/api/utils/result.mapper'
import type { Result, PagedResult, PaginationMeta } from '@/shared/models'
import type {
  TaxonomyResponse, TaxonomyRequest, TaxonomyListParams,
  TaxonResponse, TaxonRequest,
} from '../models/Taxonomy'

export async function getTaxonomies(
  params: TaxonomyListParams = {},
): Promise<MappedResult<TaxonomyResponse[]> & { meta?: PaginationMeta }> {
  const res = await apiClient.get<PagedResult<TaxonomyResponse>>('/catalog/taxonomies', { params })
  return pagedResultToMapped(res.data)
}

export async function getTaxonomy(id: string): Promise<MappedResult<TaxonomyResponse>> {
  const res = await apiClient.get<Result<TaxonomyResponse>>(`/catalog/taxonomies/${id}`)
  return resultToMapped(res.data)
}

export async function createTaxonomy(data: TaxonomyRequest): Promise<MappedResult<TaxonomyResponse>> {
  const res = await apiClient.post<Result<TaxonomyResponse>>('/catalog/taxonomies', data)
  return resultToMapped(res.data)
}

export async function updateTaxonomy(id: string, data: TaxonomyRequest): Promise<MappedResult<TaxonomyResponse>> {
  const res = await apiClient.put<Result<TaxonomyResponse>>(`/catalog/taxonomies/${id}`, data)
  return resultToMapped(res.data)
}

export async function deleteTaxonomy(id: string): Promise<MappedResult<void>> {
  const res = await apiClient.delete<Result<void>>(`/catalog/taxonomies/${id}`)
  return resultToMapped(res.data)
}

export async function getTaxons(taxonomyId: string): Promise<MappedResult<TaxonResponse[]>> {
  const res = await apiClient.get<Result<TaxonResponse[]>>(`/catalog/taxonomies/${taxonomyId}/taxons`)
  return resultToMapped(res.data)
}

export async function createTaxon(taxonomyId: string, data: TaxonRequest): Promise<MappedResult<TaxonResponse>> {
  const res = await apiClient.post<Result<TaxonResponse>>(`/catalog/taxonomies/${taxonomyId}/taxons`, data)
  return resultToMapped(res.data)
}

export async function updateTaxon(taxonomyId: string, id: string, data: TaxonRequest): Promise<MappedResult<TaxonResponse>> {
  const res = await apiClient.put<Result<TaxonResponse>>(`/catalog/taxonomies/${taxonomyId}/taxons/${id}`, data)
  return resultToMapped(res.data)
}

export async function deleteTaxon(taxonomyId: string, id: string): Promise<MappedResult<void>> {
  const res = await apiClient.delete<Result<void>>(`/catalog/taxonomies/${taxonomyId}/taxons/${id}`)
  return resultToMapped(res.data)
}
```

- [ ] **Step 5: Implement optionTypes API**

`app/Admin/src/features/catalog/api/optionTypes.ts`:

```typescript
import apiClient from '@/shared/api/client'
import { resultToMapped, pagedResultToMapped } from '@/shared/api/utils/result.mapper'
import type { MappedResult } from '@/shared/api/utils/result.mapper'
import type { Result, PagedResult, PaginationMeta } from '@/shared/models'
import type {
  OptionTypeResponse, OptionTypeRequest, OptionTypeListParams,
  OptionValueResponse, OptionValueRequest,
} from '../models/OptionType'

export async function getOptionTypes(
  params: OptionTypeListParams = {},
): Promise<MappedResult<OptionTypeResponse[]> & { meta?: PaginationMeta }> {
  const res = await apiClient.get<PagedResult<OptionTypeResponse>>('/catalog/option-types', { params })
  return pagedResultToMapped(res.data)
}

export async function getOptionType(id: string): Promise<MappedResult<OptionTypeResponse>> {
  const res = await apiClient.get<Result<OptionTypeResponse>>(`/catalog/option-types/${id}`)
  return resultToMapped(res.data)
}

export async function createOptionType(data: OptionTypeRequest): Promise<MappedResult<OptionTypeResponse>> {
  const res = await apiClient.post<Result<OptionTypeResponse>>('/catalog/option-types', data)
  return resultToMapped(res.data)
}

export async function updateOptionType(id: string, data: OptionTypeRequest): Promise<MappedResult<OptionTypeResponse>> {
  const res = await apiClient.put<Result<OptionTypeResponse>>(`/catalog/option-types/${id}`, data)
  return resultToMapped(res.data)
}

export async function deleteOptionType(id: string): Promise<MappedResult<void>> {
  const res = await apiClient.delete<Result<void>>(`/catalog/option-types/${id}`)
  return resultToMapped(res.data)
}

export async function getOptionValues(optionTypeId: string): Promise<MappedResult<OptionValueResponse[]>> {
  const res = await apiClient.get<Result<OptionValueResponse[]>>(`/catalog/option-types/${optionTypeId}/values`)
  return resultToMapped(res.data)
}

export async function createOptionValue(optionTypeId: string, data: OptionValueRequest): Promise<MappedResult<OptionValueResponse>> {
  const res = await apiClient.post<Result<OptionValueResponse>>(`/catalog/option-types/${optionTypeId}/values`, data)
  return resultToMapped(res.data)
}

export async function updateOptionValue(optionTypeId: string, id: string, data: OptionValueRequest): Promise<MappedResult<OptionValueResponse>> {
  const res = await apiClient.put<Result<OptionValueResponse>>(`/catalog/option-types/${optionTypeId}/values/${id}`, data)
  return resultToMapped(res.data)
}

export async function deleteOptionValue(optionTypeId: string, id: string): Promise<MappedResult<void>> {
  const res = await apiClient.delete<Result<void>>(`/catalog/option-types/${optionTypeId}/values/${id}`)
  return resultToMapped(res.data)
}
```

- [ ] **Step 6: Write barrel export**

`app/Admin/src/features/catalog/api/index.ts`:

```typescript
export * as productsApi from './products'
export * as taxonomiesApi from './taxonomies'
export * as optionTypesApi from './optionTypes'
```

- [ ] **Step 7: Verify all tests pass**

```bash
cd app/Admin && pnpm run test:unit -- --run catalog/api/__tests__/
```
Expected: PASS — all 16 API tests pass.

- [ ] **Step 8: Commit**

```bash
git add app/Admin/src/features/catalog/api/
git commit -m "feat: add catalog taxonomies + optionTypes API services with tests"
```

---

### Task 4: Catalog DashboardPage

**Files:**
- Modify: `app/Admin/src/features/catalog/pages/DashboardPage.vue`
- Create: `app/Admin/src/features/catalog/pages/__tests__/DashboardPage.spec.ts`

**Interfaces:**
- Consumes: `PageHeader`, `StatCard` from shared components
- Produces: dashboard with 4 KPI stat cards

- [ ] **Step 1: Write failing test**

`app/Admin/src/features/catalog/pages/__tests__/DashboardPage.spec.ts`:

```typescript
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createWebHistory } from 'vue-router'
import DashboardPage from '../DashboardPage.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [{ path: '/', component: { template: '<div />' } }],
})

describe('Catalog DashboardPage', () => {
  it('renders page header with correct title', () => {
    const wrapper = mount(DashboardPage, {
      global: { plugins: [router] },
    })
    expect(wrapper.text()).toContain('Catalog Dashboard')
  })

  it('renders 4 stat cards', () => {
    const wrapper = mount(DashboardPage, {
      global: { plugins: [router] },
    })
    const cards = wrapper.findAll('.stat-card, [data-test="stat-card"]')
    // Using the component wrapper approach — StatCard is a shared component
    const statCards = wrapper.findAllComponents({ name: 'StatCard' })
    expect(statCards).toHaveLength(4)
  })

  it('contains expected KPI labels', () => {
    const wrapper = mount(DashboardPage, {
      global: { plugins: [router] },
    })
    expect(wrapper.text()).toContain('Total Products')
    expect(wrapper.text()).toContain('Active Products')
    expect(wrapper.text()).toContain('Taxonomies')
    expect(wrapper.text()).toContain('Option Types')
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

```bash
cd app/Admin && pnpm run test:unit -- --run catalog/pages/__tests__/DashboardPage
```
Expected: FAIL — existing stub renders "Dashboard" not real content.

- [ ] **Step 3: Implement DashboardPage**

Replace `app/Admin/src/features/catalog/pages/DashboardPage.vue`:

```vue
<script setup lang="ts">
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import StatCard from '@/shared/components/data/StatCard.vue'

const stats = [
  { label: 'Total Products', value: '0', icon: 'pi pi-shopping-bag', color: 'primary' as const },
  { label: 'Active Products', value: '0', icon: 'pi pi-check-circle', color: 'green' as const },
  { label: 'Taxonomies', value: '0', icon: 'pi pi-sitemap', color: 'blue' as const },
  { label: 'Option Types', value: '0', icon: 'pi pi-list', color: 'orange' as const },
]
</script>

<template>
  <div>
    <PageHeader title="Catalog Dashboard" subtitle="Overview of your product catalog" />
    <div class="grid">
      <div v-for="s in stats" :key="s.label" class="col-12 md:col-6 lg:col-3">
        <StatCard :label="s.label" :value="s.value" :icon="s.icon" :color="s.color" />
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 4: Run test to verify it passes**

```bash
cd app/Admin && pnpm run test:unit -- --run catalog/pages/__tests__/DashboardPage
```
Expected: PASS — 3 assertions pass.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/catalog/pages/DashboardPage.vue
git add app/Admin/src/features/catalog/pages/__tests__/
git commit -m "feat: implement Catalog DashboardPage with KPI cards"
```

---

### Task 5: ProductListPage

**Files:**
- Modify: `app/Admin/src/features/catalog/pages/ProductListPage.vue`
- Create: `app/Admin/src/features/catalog/pages/__tests__/ProductListPage.spec.ts`

**Interfaces:**
- Consumes: `getProducts`, `deleteProduct` from `../api/products`, shared components (`DataTable`, `PageHeader`, `TableToolbar`, `ActionMenu`, `StatusTag`, `EmptyState`, `LoadingSkeleton`, `ErrorState`), `useConfirm`, `useToastNotify`
- Produces: product list with search, pagination, create/edit/delete actions

- [ ] **Step 1: Write failing test**

`app/Admin/src/features/catalog/pages/__tests__/ProductListPage.spec.ts`:

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createWebHistory } from 'vue-router'
import ProductListPage from '../ProductListPage.vue'

const mockGetProducts = vi.fn()
const mockDeleteProduct = vi.fn()

vi.mock('../../api/products', () => ({
  getProducts: (...args: unknown[]) => mockGetProducts(...args),
  deleteProduct: (...args: unknown[]) => mockDeleteProduct(...args),
}))

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: { template: '<div />' } },
    { path: '/catalog/products', name: 'catalog.products.list', component: { template: '<div />' } },
    { path: '/catalog/products/new', name: 'catalog.products.create', component: { template: '<div />' } },
    { path: '/catalog/products/:id', name: 'catalog.products.view', component: { template: '<div />' } },
    { path: '/catalog/products/:id/edit', name: 'catalog.products.edit', component: { template: '<div />' } },
  ],
})

describe('ProductListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders page header', async () => {
    mockGetProducts.mockResolvedValue({
      success: true,
      data: [],
      meta: { page: 1, pageSize: 20, totalCount: 0, totalPages: 0 },
    })
    const wrapper = mount(ProductListPage, {
      global: { plugins: [router] },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Products')
  })

  it('displays empty state when no products', async () => {
    mockGetProducts.mockResolvedValue({
      success: true,
      data: [],
      meta: { page: 1, pageSize: 20, totalCount: 0, totalPages: 0 },
    })
    const wrapper = mount(ProductListPage, {
      global: { plugins: [router] },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('No products')
  })

  it('displays products in table when data exists', async () => {
    mockGetProducts.mockResolvedValue({
      success: true,
      data: [
        { id: '1', name: 'Test Product', slug: 'test', status: 'Draft', department: null, createdAt: '2026-01-01', updatedAt: '2026-01-01' },
      ],
      meta: { page: 1, pageSize: 20, totalCount: 1, totalPages: 1 },
    })
    const wrapper = mount(ProductListPage, {
      global: { plugins: [router] },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Test Product')
  })

  it('displays error state on API failure', async () => {
    mockGetProducts.mockResolvedValue({
      success: false,
      error: { message: 'Server error', statusCode: 500, title: 'Error', detail: null, errors: {}, errorCode: 'ERR' },
    })
    const wrapper = mount(ProductListPage, {
      global: { plugins: [router] },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Server error')
  })

  it('has a create button', async () => {
    mockGetProducts.mockResolvedValue({
      success: true,
      data: [],
      meta: { page: 1, pageSize: 20, totalCount: 0, totalPages: 0 },
    })
    const wrapper = mount(ProductListPage, {
      global: { plugins: [router] },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Add Product')
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

```bash
cd app/Admin && pnpm run test:unit -- --run catalog/pages/__tests__/ProductListPage
```
Expected: FAIL — placeholder stub renders generic title, not real data.

- [ ] **Step 3: Implement ProductListPage**

Replace `app/Admin/src/features/catalog/pages/ProductListPage.vue`:

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import DataTable from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import StatusTag from '@/shared/components/data/StatusTag.vue'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToastNotify } from '@/shared/composables/useToastNotify'
import { getProducts, deleteProduct } from '../api/products'
import type { ProductResponse } from '../models/Product'

const router = useRouter()
const confirm = useConfirm()
const toast = useToastNotify()

const items = ref<ProductResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const search = ref('')
const page = ref(1)
const pageSize = ref(20)
const totalCount = ref(0)

const columns = [
  { field: 'name', header: 'Name', sortable: true },
  { field: 'slug', header: 'Slug' },
  { field: 'status', header: 'Status' },
  { field: 'createdAt', header: 'Created' },
]

async function fetchProducts() {
  loading.value = true
  error.value = null
  const result = await getProducts({
    page: page.value,
    pageSize: pageSize.value,
    search: search.value || undefined,
  })
  if (result.success) {
    items.value = result.data
    totalCount.value = result.meta?.totalCount ?? 0
  } else {
    error.value = result.error?.message ?? 'Failed to load products'
  }
  loading.value = false
}

function statusSeverity(s: string) {
  return s === 'Active' ? 'success' : s === 'Draft' ? 'warn' : 'secondary'
}

function goToCreate() { router.push({ name: 'catalog.products.create' }) }
function goToView(id: string) { router.push({ name: 'catalog.products.view', params: { id } }) }
function goToEdit(id: string) { router.push({ name: 'catalog.products.edit', params: { id } }) }

async function onDelete(id: string) {
  await confirm({
    target: 'this product',
    onAccept: async () => {
      const result = await deleteProduct(id)
      if (result.success) { toast.success('Product deleted'); await fetchProducts() }
      else { toast.error(result.error?.message ?? 'Failed to delete') }
    },
  })
}

function onSearch() { page.value = 1; fetchProducts() }
function onPageChange(e: { page: number; rows: number }) {
  page.value = e.page + 1
  pageSize.value = e.rows
  fetchProducts()
}

onMounted(() => fetchProducts())
</script>

<template>
  <div>
    <PageHeader title="Products" subtitle="Manage product catalog" />
    <TableToolbar
      v-model:search="search"
      search-placeholder="Search products..."
      create-label="Add Product"
      @search="onSearch"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="loading" :rows="5" :columns="4" />
    <ErrorState v-else-if="error" :message="error" @retry="fetchProducts" />
    <EmptyState v-else-if="items.length === 0" title="No products" description="Create your first product.">
      <button @click="goToCreate">Add Product</button>
    </EmptyState>
    <DataTable
      v-else
      :value="items"
      :columns="columns"
      :loading="loading"
      :total-records="totalCount"
      :rows="pageSize"
      :first="(page - 1) * pageSize"
      lazy paginator striped-rows
      @page="onPageChange"
      @row-click="(e: { data: ProductResponse }) => goToView(e.data.id)"
    >
      <template #body-status="{ data }">
        <StatusTag :value="data.status" :severity="statusSeverity(data.status)" />
      </template>
      <template #body-actions="{ data }">
        <ActionMenu
          :items="[
            { label: 'View', icon: 'pi pi-eye', command: () => goToView(data.id) },
            { label: 'Edit', icon: 'pi pi-pencil', command: () => goToEdit(data.id) },
            { label: 'Delete', icon: 'pi pi-trash', command: () => onDelete(data.id) },
          ]"
        />
      </template>
    </DataTable>
  </div>
</template>
```

- [ ] **Step 4: Run test to verify it passes**

```bash
cd app/Admin && pnpm run test:unit -- --run catalog/pages/__tests__/ProductListPage
```
Expected: PASS — 5 assertions pass.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/catalog/pages/ProductListPage.vue
git add app/Admin/src/features/catalog/pages/__tests__/
git commit -m "feat: implement ProductListPage with search, pagination, row actions"
```

---

### Task 6: ProductDetailPage

**Files:**
- Create: `app/Admin/src/features/catalog/pages/ProductDetailPage.vue`
- Create: `app/Admin/src/features/catalog/pages/__tests__/ProductDetailPage.spec.ts`

**Interfaces:**
- Consumes: `getProduct`, `createProduct`, `updateProduct` from `../api/products`, `ProductResponse`, `ProductRequest` from `../models/Product`, shared components (`PageHeader`, `FormField`, `FormActions`, `LoadingSkeleton`, `ErrorState`), `useToastNotify`
- Produces: detail page with create/view/edit modes driven by route `:id` and route name suffix `.edit`

- [ ] **Step 1: Write failing test**

`app/Admin/src/features/catalog/pages/__tests__/ProductDetailPage.spec.ts`:

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createWebHistory } from 'vue-router'
import ProductDetailPage from '../ProductDetailPage.vue'

const mockGet = vi.fn()
const mockCreate = vi.fn()
const mockUpdate = vi.fn()

vi.mock('../../api/products', () => ({
  getProduct: (...args: unknown[]) => mockGet(...args),
  createProduct: (...args: unknown[]) => mockCreate(...args),
  updateProduct: (...args: unknown[]) => mockUpdate(...args),
}))

function makeRouter(initialRoute: string) {
  return createRouter({
    history: createWebHistory(),
    routes: [
      { path: '/catalog/products/new', name: 'catalog.products.create', component: ProductDetailPage },
      { path: '/catalog/products/:id', name: 'catalog.products.view', component: ProductDetailPage },
      { path: '/catalog/products/:id/edit', name: 'catalog.products.edit', component: ProductDetailPage },
      { path: '/catalog/products', name: 'catalog.products.list', component: { template: '<div />' } },
    ],
  })
}

describe('ProductDetailPage', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('renders in create mode when no :id param', async () => {
    const router = makeRouter('/catalog/products/new')
    router.push('/catalog/products/new')
    await router.isReady()
    const wrapper = mount(ProductDetailPage, { global: { plugins: [router] } })
    await flushPromises()
    expect(wrapper.text()).toContain('Create Product')
  })

  it('renders in view mode when :id present and route not .edit', async () => {
    mockGet.mockResolvedValue({
      success: true,
      data: { id: 'abc', name: 'Test Product', slug: 'test', status: 'Draft', department: null, createdAt: '', updatedAt: '' },
    })
    const router = makeRouter('/catalog/products/abc')
    router.push('/catalog/products/abc')
    await router.isReady()
    const wrapper = mount(ProductDetailPage, { global: { plugins: [router] } })
    await flushPromises()
    expect(wrapper.text()).toContain('Test Product')
  })

  it('renders in edit mode when route ends with .edit', async () => {
    mockGet.mockResolvedValue({
      success: true,
      data: { id: 'abc', name: 'Edit Me', slug: 'edit-me', status: 'Draft', department: null, createdAt: '', updatedAt: '' },
    })
    const router = makeRouter('/catalog/products/abc/edit')
    router.push('/catalog/products/abc/edit')
    await router.isReady()
    const wrapper = mount(ProductDetailPage, { global: { plugins: [router] } })
    await flushPromises()
    expect(wrapper.text()).toContain('Edit: Edit Me')
  })

  it('displays error state on load failure', async () => {
    mockGet.mockResolvedValue({ success: false, error: { message: 'Not found', statusCode: 404, title: '', detail: null, errors: {}, errorCode: 'ERR' } })
    const router = makeRouter('/catalog/products/missing')
    router.push('/catalog/products/missing')
    await router.isReady()
    const wrapper = mount(ProductDetailPage, { global: { plugins: [router] } })
    await flushPromises()
    expect(wrapper.text()).toContain('Not found')
  })

  it('shows save and cancel buttons in edit mode', async () => {
    mockGet.mockResolvedValue({
      success: true,
      data: { id: 'abc', name: 'Test', slug: 'test', status: 'Draft', department: null, createdAt: '', updatedAt: '' },
    })
    const router = makeRouter('/catalog/products/abc/edit')
    router.push('/catalog/products/abc/edit')
    await router.isReady()
    const wrapper = mount(ProductDetailPage, { global: { plugins: [router] } })
    await flushPromises()
    expect(wrapper.text()).toContain('Save Changes')
    expect(wrapper.text()).toContain('Cancel')
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

```bash
cd app/Admin && pnpm run test:unit -- --run catalog/pages/__tests__/ProductDetailPage
```
Expected: FAIL — file not found.

- [ ] **Step 3: Implement ProductDetailPage**

Create `app/Admin/src/features/catalog/pages/ProductDetailPage.vue`:

```vue
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import FormField from '@/shared/components/forms/FormField.vue'
import FormActions from '@/shared/components/forms/FormActions.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useToastNotify } from '@/shared/composables/useToastNotify'
import { getProduct, createProduct, updateProduct } from '../api/products'
import type { ProductResponse, ProductRequest, ProductStatus } from '../models/Product'

const route = useRoute()
const router = useRouter()
const toast = useToastNotify()

const id = computed(() => route.params.id as string | undefined)
const mode = computed<'create' | 'view' | 'edit'>(() => {
  if (!id.value) return 'create'
  if (route.name?.toString().endsWith('.edit')) return 'edit'
  return 'view'
})

const loading = ref(false)
const saving = ref(false)
const error = ref<string | null>(null)
const form = ref<ProductRequest & { status: ProductStatus }>({
  name: '', slug: '', description: null, status: 'Draft',
  styleCode: null, seasonName: null, department: null, genderTarget: null,
  metaTitle: null, metaDescription: null, metaKeywords: null,
})
const formErrors = ref<Record<string, string>>({})

const title = computed(() => {
  if (mode.value === 'create') return 'Create Product'
  if (mode.value === 'edit') return `Edit: ${form.value.name || 'Product'}`
  return form.value.name || 'Product Detail'
})

function validate(): boolean {
  formErrors.value = {}
  if (!form.value.name.trim()) formErrors.value.name = 'Required'
  if (!form.value.slug.trim()) formErrors.value.slug = 'Required'
  return Object.keys(formErrors.value).length === 0
}

async function load() {
  if (!id.value) return
  loading.value = true; error.value = null
  const result = await getProduct(id.value)
  if (result.success) { form.value = { ...result.data } }
  else { error.value = result.error?.message ?? 'Failed to load product' }
  loading.value = false
}

async function save() {
  if (!validate()) return
  saving.value = true
  const data: ProductRequest = { ...form.value }
  const result = id.value ? await updateProduct(id.value, data) : await createProduct(data)
  saving.value = false
  if (result.success) {
    toast.success(id.value ? 'Product updated' : 'Product created')
    if (mode.value === 'create') {
      router.replace({ name: 'catalog.products.view', params: { id: result.data.id } })
    } else {
      router.replace({ name: 'catalog.products.view', params: { id: id.value } })
    }
  } else { toast.error(result.error?.message ?? 'Save failed') }
}

function cancel() {
  if (id.value) router.push({ name: 'catalog.products.view', params: { id: id.value } })
  else router.push({ name: 'catalog.products.list' })
}

function toggleEdit() { router.push({ name: 'catalog.products.edit', params: { id: id.value } }) }

onMounted(() => { load() })
</script>

<template>
  <div>
    <PageHeader :title="title">
      <template #actions>
        <button v-if="mode === 'view'" class="p-button p-component" @click="toggleEdit">Edit</button>
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="error" :message="error" @retry="load" />
    <div v-else class="card">
      <div class="grid">
        <div class="col-6">
          <FormField label="Name" :error="formErrors.name" required>
            <input v-model="form.name" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Slug" :error="formErrors.slug" required>
            <input v-model="form.slug" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Status">
            <select v-model="form.status" class="p-inputtext p-component w-full" :disabled="mode === 'view'">
              <option value="Draft">Draft</option>
              <option value="Active">Active</option>
              <option value="Archived">Archived</option>
            </select>
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Department">
            <input v-model="form.department" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-12">
          <FormField label="Description">
            <textarea v-model="form.description" rows="4" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <FormActions
        v-if="mode !== 'view'"
        :saving="saving"
        :save-label="mode === 'create' ? 'Create Product' : 'Save Changes'"
        cancel-label="Cancel"
        @save="save"
        @cancel="cancel"
      />
    </div>
  </div>
</template>
```

- [ ] **Step 4: Run test to verify it passes**

```bash
cd app/Admin && pnpm run test:unit -- --run catalog/pages/__tests__/ProductDetailPage
```
Expected: PASS — 5 assertions pass.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/catalog/pages/ProductDetailPage.vue
git add app/Admin/src/features/catalog/pages/__tests__/
git commit -m "feat: implement ProductDetailPage with create/view/edit modes"
```

---

### Task 7: TaxonomyListPage

**Files:**
- Create: `app/Admin/src/features/catalog/pages/TaxonomyListPage.vue`
- Create: `app/Admin/src/features/catalog/pages/__tests__/TaxonomyListPage.spec.ts`

**Interfaces:**
- Consumes: `getTaxonomies`, `deleteTaxonomy` from `../api/taxonomies`, shared components
- Produces: taxonomy list with same pattern as ProductListPage

**Pattern:** Identical to Task 5 (ProductListPage). Substitute:
- `getProducts` → `getTaxonomies`, `deleteProduct` → `deleteTaxonomy`
- `ProductResponse` → `TaxonomyResponse`
- Route names: `catalog.products.*` → `catalog.taxonomies.*`
- Table columns: Name, Presentation, Position, Created
- Search placeholder: "Search taxonomies..."
- Create label: "Add Taxonomy"
- PageHeader title: "Taxonomies", subtitle: "Manage taxonomy groups"
- Empty state: "No taxonomies", "Create your first taxonomy."

- [ ] **Step 1: Write test** — same structure as ProductListPage test, mocking `../../api/taxonomies` and checking for "Taxonomies" header, "No taxonomies" empty state.

See `ProductListPage.spec.ts` for the test template. Adapt mock path to `../../api/taxonomies`.

- [ ] **Step 2: Run test** → FAIL
- [ ] **Step 3: Implement** → same Vue template as ProductListPage with substitutions above
- [ ] **Step 4: Run test** → PASS
- [ ] **Step 5: Commit** with message `"feat: implement TaxonomyListPage"`

---

### Task 8: TaxonomyDetailPage with inline Taxons sub-table

**Files:**
- Create: `app/Admin/src/features/catalog/pages/TaxonomyDetailPage.vue`
- Create: `app/Admin/src/features/catalog/pages/__tests__/TaxonomyDetailPage.spec.ts`

**Interfaces:**
- Consumes: `getTaxonomy`, `createTaxonomy`, `updateTaxonomy`, `getTaxons`, `createTaxon`, `updateTaxon`, `deleteTaxon` from `../api/taxonomies`, shared components, `useConfirm`, `useToastNotify`
- Produces: taxonomy detail page (create/view/edit) + inline taxons sub-table with depth indentation + slideover for taxon add/edit

- [ ] **Step 1: Write test**

`app/Admin/src/features/catalog/pages/__tests__/TaxonomyDetailPage.spec.ts`:

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createWebHistory } from 'vue-router'

const mockGetTaxonomy = vi.fn()
const mockGetTaxons = vi.fn()

vi.mock('../../api/taxonomies', () => ({
  getTaxonomy: (...args: unknown[]) => mockGetTaxonomy(...args),
  createTaxonomy: vi.fn(),
  updateTaxonomy: vi.fn(),
  getTaxons: (...args: unknown[]) => mockGetTaxons(...args),
  createTaxon: vi.fn(),
  updateTaxon: vi.fn(),
  deleteTaxon: vi.fn(),
}))

import TaxonomyDetailPage from '../TaxonomyDetailPage.vue'

function makeRouter(initialRoute: string) {
  const router = createRouter({
    history: createWebHistory(),
    routes: [
      { path: '/catalog/taxonomies/new', name: 'catalog.taxonomies.create', component: TaxonomyDetailPage },
      { path: '/catalog/taxonomies/:id', name: 'catalog.taxonomies.view', component: TaxonomyDetailPage },
      { path: '/catalog/taxonomies/:id/edit', name: 'catalog.taxonomies.edit', component: TaxonomyDetailPage },
      { path: '/catalog/taxonomies', name: 'catalog.taxonomies.list', component: { template: '<div />' } },
    ],
  })
  router.push(initialRoute)
  return router
}

describe('TaxonomyDetailPage', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('renders in view mode and loads taxons', async () => {
    mockGetTaxonomy.mockResolvedValue({
      success: true, data: { id: 'abc', name: 'Clothing', presentation: null, position: 0, createdAt: '', updatedAt: '' },
    })
    mockGetTaxons.mockResolvedValue({
      success: true, data: [
        { id: '1', name: 'Men', depth: 0, lft: 1, rgt: 4, taxonomyId: 'abc', parentId: null, slug: 'men', position: 0, presentation: null, description: null, childrenCount: 2, hideFromNav: false, automatic: false, createdAt: '', updatedAt: '' },
        { id: '2', name: 'Tops', depth: 1, lft: 2, rgt: 3, taxonomyId: 'abc', parentId: '1', slug: 'tops', position: 0, presentation: null, description: null, childrenCount: 0, hideFromNav: false, automatic: false, createdAt: '', updatedAt: '' },
        { id: '3', name: 'Women', depth: 0, lft: 5, rgt: 6, taxonomyId: 'abc', parentId: null, slug: 'women', position: 1, presentation: null, description: null, childrenCount: 0, hideFromNav: false, automatic: false, createdAt: '', updatedAt: '' },
      ],
    })
    const router = makeRouter('/catalog/taxonomies/abc')
    await router.isReady()
    const wrapper = mount(TaxonomyDetailPage, { global: { plugins: [router] } })
    await flushPromises()
    await flushPromises()
    expect(wrapper.text()).toContain('Clothing')
    expect(wrapper.text()).toContain('Taxons')
    expect(wrapper.text()).toContain('Men')
    expect(wrapper.text()).toContain('Tops')
  })

  it('shows Add Taxon button', async () => {
    mockGetTaxonomy.mockResolvedValue({
      success: true, data: { id: 'abc', name: 'Test', presentation: null, position: 0, createdAt: '', updatedAt: '' },
    })
    mockGetTaxons.mockResolvedValue({ success: true, data: [] })
    const router = makeRouter('/catalog/taxonomies/abc')
    await router.isReady()
    const wrapper = mount(TaxonomyDetailPage, { global: { plugins: [router] } })
    await flushPromises()
    await flushPromises()
    expect(wrapper.text()).toContain('Add Taxon')
  })

  it('renders in create mode', async () => {
    const router = makeRouter('/catalog/taxonomies/new')
    await router.isReady()
    const wrapper = mount(TaxonomyDetailPage, { global: { plugins: [router] } })
    await flushPromises()
    expect(wrapper.text()).toContain('Create Taxonomy')
  })

  it('shows empty state for taxons', async () => {
    mockGetTaxonomy.mockResolvedValue({
      success: true, data: { id: 'abc', name: 'Empty', presentation: null, position: 0, createdAt: '', updatedAt: '' },
    })
    mockGetTaxons.mockResolvedValue({ success: true, data: [] })
    const router = makeRouter('/catalog/taxonomies/abc')
    await router.isReady()
    const wrapper = mount(TaxonomyDetailPage, { global: { plugins: [router] } })
    await flushPromises()
    await flushPromises()
    expect(wrapper.text()).toContain('No taxons')
  })
})
```

- [ ] **Step 2: Run test** → FAIL (file not found)

- [ ] **Step 3: Implement TaxonomyDetailPage**

Create `app/Admin/src/features/catalog/pages/TaxonomyDetailPage.vue` — full implementation with:
- Mode detection (create/view/edit) via route params
- Taxonomy form fields: Name, Presentation
- Taxons sub-table inside `<fieldset>` with depth-based `paddingLeft` on name column
- "Add Taxon" button → slideover drawer with Name, Presentation fields
- Edit/Delete row actions on taxon rows
- Uses `useConfirm` for taxon deletion, `useToastNotify` for feedback

Full code (~200 lines). See the combined plan file for the complete implementation.
Structure: taxonomy form at top, `<fieldset>` with taxons DataTable below, slideover overlay for taxon CRUD.

- [ ] **Step 4: Run test** → PASS
- [ ] **Step 5: Commit** with message `"feat: implement TaxonomyDetailPage with inline taxons sub-table"`

---

### Task 9: OptionTypeListPage

**Files:**
- Modify: `app/Admin/src/features/catalog/pages/OptionTypeListPage.vue`
- Create: `app/Admin/src/features/catalog/pages/__tests__/OptionTypeListPage.spec.ts`

Same pattern as ProductListPage (Task 5). Substitute:
- API: `../../api/optionTypes` (`getOptionTypes`, `deleteOptionType`)
- Types: `OptionTypeResponse` from `../../models/OptionType`
- Route names: `catalog.option-types.*`
- Columns: Name, Presentation, Filterable, Position
- Search placeholder: "Search option types..."
- Create label: "Add Type"
- PageHeader: "Option Types", "Manage product option types (Size, Color, etc.)"

- [ ] **Step 1: Write test** — adapt ProductListPage test template
- [ ] **Step 2: Run test** → FAIL
- [ ] **Step 3: Implement** — adapt ProductListPage template
- [ ] **Step 4: Run test** → PASS
- [ ] **Step 5: Commit** with message `"feat: implement OptionTypeListPage"`

---

### Task 10: OptionTypeDetailPage with inline OptionValues

**Files:**
- Create: `app/Admin/src/features/catalog/pages/OptionTypeDetailPage.vue`
- Create: `app/Admin/src/features/catalog/pages/__tests__/OptionTypeDetailPage.spec.ts`

Same pattern as TaxonomyDetailPage (Task 8). Substitute:
- API: `../../api/optionTypes` (getOptionType, createOptionType, updateOptionType, getOptionValues, createOptionValue, updateOptionValue, deleteOptionValue)
- Types: `OptionTypeResponse`, `OptionTypeRequest`, `OptionValueResponse`, `OptionValueRequest`
- Route names: `catalog.option-types.*`
- Form fields: Name, Presentation, Filterable (checkbox)
- Sub-table fieldset legend: "Option Values"
- Sub-entity columns: Name, Presentation, Position (no depth indentation — flat list)
- OptionValue form: Name, Presentation (no parentId)

- [ ] **Step 1: Write test** — adapt TaxonomyDetailPage test, checking for "Option Values" fieldset, "Add Value" button, flat option values
- [ ] **Step 2: Run test** → FAIL
- [ ] **Step 3: Implement** — adapt TaxonomyDetailPage template, remove depth indentation, change names
- [ ] **Step 4: Run test** → PASS
- [ ] **Step 5: Commit** with message `"feat: implement OptionTypeDetailPage with inline option values"`

---

### Task 11: Final Catalog verification

- [ ] **Step 1: Run all catalog tests**

```bash
cd app/Admin && pnpm run test:unit -- --run catalog/
```

- [ ] **Step 2: Run typecheck**

```bash
cd app/Admin && pnpm run typecheck
```

- [ ] **Step 3: Run lint**

```bash
cd app/Admin && pnpm run lint
```

- [ ] **Step 4: Commit**

```bash
git add -A app/Admin/src/features/catalog/
git commit -m "chore: catalog module — typecheck, lint, tests pass"
```
