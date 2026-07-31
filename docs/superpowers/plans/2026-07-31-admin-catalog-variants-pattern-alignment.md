# Admin Catalog Variants Pattern Alignment Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Align the Admin frontend Variants feature with the established Catalog list pattern — server-side `PagedResult` lists via `getPaged`, per-field zod schemas, and a `usePagedQuery`-driven list page.

**Architecture:** The backend already conforms (GetVariantsPagedOrAll returns `PagedResult`). All work is frontend-only: fix `types/variant.ts`, `validations/variant.ts`, three API services, and the `VariantsList.vue`/`VariantDetail.vue` views to mirror `productApi.ts`/`ProductsList.vue`/`taxonApi.ts`. `VariantsList` uses the shared `usePagedQuery` composable (server-side search/sort/paging); the detail page tab loaders read `.items` off `PagedResult`.

**Tech Stack:** Vue 3, TypeScript, PrimeVue DataTable, `@primevue/forms` + zod, Vitest, `@/shared/api` (`getPaged`), `@/shared/composables/usePagedQuery`.

## Global Constraints

- No backend/C# changes — frontend only.
- `Variant` response type is renamed to `VariantListItem`; `VariantDetail = VariantListItem` alias added.
- Filter/sort/search field constants must match the backend: filter `['isMaster','trackInventory','discontinuedOn','dimensionsUnit','weightUnit']`, sort `['sku','position','price','weight','height','width','depth']`, search `['sku','barcode','hsCode']`.
- Every constraint in the zod schemas carries an explicit message (no zod-default messages).
- List services return `Promise<PagedResult<T>>` via `getPaged`, never `Result<{ items }>`.
- `VariantsList` keeps the "Select a product" empty state when no `productId` route query; shows a visible inline error banner when the paged fetch fails.
- Work from `app/Admin/`. Commit after each task. Verify with `pnpm run type-check`, `pnpm run test:unit -- run`, `pnpm run build-only`.

---
### Task 1: Types & Query Mapping

**Files:**
- Modify: `app/Admin/src/features/catalog/types/variant.ts`
- Test: `app/Admin/src/features/catalog/__tests__/types/variant.spec.ts` (create)

**Interfaces:**
- Produces: `VariantListItem` (interface extends `VariantParameters` with `id`, `productId`, `isMaster`, `discontinuedOn?: string | null`, `pricesCount: number`), `VariantDetail` (`type VariantDetail = VariantListItem`), `VariantQuery` (`{ search?: string; isMaster?: boolean; sortBy?: 'sku'|'position'|'price'|'weight'|'height'|'width'|'depth'; sortDirection?: 'asc'|'desc'; page?: number; pageSize?: number }`), `VARIANT_FILTER_FIELDS`, `VARIANT_SORT_FIELDS`, `VARIANT_SEARCH_FIELDS` constants, `toVariantQueryParams(query: VariantQuery): QueryingParameters`. Keeps `VariantParameters`, `VariantRequest`, `VariantImage`, `Price`, `OptionValueAssignment` unchanged. Exports `QueryingParameters` import from `@/shared/types/querying`.

- [ ] **Step 1: Write the failing test**

Create `app/Admin/src/features/catalog/__tests__/types/variant.spec.ts`:

```ts
import { describe, it, expect } from 'vitest'
import {
  toVariantQueryParams,
  VARIANT_FILTER_FIELDS,
  VARIANT_SORT_FIELDS,
  VARIANT_SEARCH_FIELDS,
} from '../../types/variant'

describe('toVariantQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toVariantQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
  })

  it('builds filter for isMaster true', () => {
    const result = toVariantQueryParams({ isMaster: true })
    expect(result.filter).toBe('isMaster=true')
  })

  it('omits filter when isMaster is false', () => {
    const result = toVariantQueryParams({ isMaster: false })
    expect(result.filter).toBeNull()
  })

  it('builds sort ascending', () => {
    const result = toVariantQueryParams({ sortBy: 'position', sortDirection: 'asc' })
    expect(result.sort).toEqual(['position'])
  })

  it('builds sort descending', () => {
    const result = toVariantQueryParams({ sortBy: 'sku', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-sku'])
  })

  it('passes search and pagination', () => {
    const result = toVariantQueryParams({ search: 'ABC', page: 2, pageSize: 25 })
    expect(result.search).toBe('ABC')
    expect(result.pageNumber).toBe(2)
    expect(result.pageSize).toBe(25)
  })
})

describe('VARIANT_FILTER_FIELDS', () => {
  it('matches backend allowed filter fields', () => {
    expect(VARIANT_FILTER_FIELDS).toEqual([
      'isMaster',
      'trackInventory',
      'discontinuedOn',
      'dimensionsUnit',
      'weightUnit',
    ])
  })
})

describe('VARIANT_SORT_FIELDS', () => {
  it('matches backend allowed sort fields', () => {
    expect(VARIANT_SORT_FIELDS).toEqual([
      'sku',
      'position',
      'price',
      'weight',
      'height',
      'width',
      'depth',
    ])
  })
})

describe('VARIANT_SEARCH_FIELDS', () => {
  it('matches backend allowed search fields', () => {
    expect(VARIANT_SEARCH_FIELDS).toEqual(['sku', 'barcode', 'hsCode'])
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `pnpm run test:unit -- run src/features/catalog/__tests__/types/variant.spec.ts`
Expected: FAIL — module `../../types/variant` has no exported `toVariantQueryParams`/`VARIANT_SEARCH_FIELDS`/`VariantListItem`.

- [ ] **Step 3: Implement types and query mapper**

Rewrite `app/Admin/src/features/catalog/types/variant.ts` to:

```ts
import type { QueryingParameters } from '@/shared/types/querying'

export interface VariantParameters {
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

export interface VariantRequest extends VariantParameters {
  isMaster: boolean
  optionValueIds?: string[]
}

export interface VariantListItem extends VariantParameters {
  id: string
  productId: string
  isMaster: boolean
  discontinuedOn?: string | null
  pricesCount: number
}

export type VariantDetail = VariantListItem

export interface VariantQuery {
  search?: string
  isMaster?: boolean
  sortBy?: 'sku' | 'position' | 'price' | 'weight' | 'height' | 'width' | 'depth'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const VARIANT_FILTER_FIELDS = [
  'isMaster',
  'trackInventory',
  'discontinuedOn',
  'dimensionsUnit',
  'weightUnit',
]

export const VARIANT_SORT_FIELDS = [
  'sku',
  'position',
  'price',
  'weight',
  'height',
  'width',
  'depth',
]

export const VARIANT_SEARCH_FIELDS = ['sku', 'barcode', 'hsCode']

export function toVariantQueryParams(query: VariantQuery): QueryingParameters {
  const filters: string[] = []

  if (query.isMaster === true) {
    filters.push('isMaster=true')
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

export interface VariantImage {
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
  type: string
  createdAtUtc: string
}

export interface Price {
  id: string
  variantId: string
  amount?: number
  currency: string
  compareAtAmount?: number
  countryIso?: string
}

export interface OptionValueAssignment {
  optionValueId: string
  optionTypeId: string
  optionTypeName: string
  name: string
  presentation: string
  isAssigned: boolean
}
```

Note: `variantApi.ts` still imports `Variant` after this task and will fail type-check. Fix that in Step 4 by re-pointing it to the new type, keeping this task's commit green.

- [ ] **Step 4: Fix the dependent service import**

In `app/Admin/src/features/catalog/services/variantApi.ts`, change line 6 `Variant,` in the type import to `VariantDetail,` and update the three usages `Result<Variant>` → `Result<VariantDetail>` (in `getVariant`, `createVariant`, `updateVariant` return types). Do NOT change method signatures or bodies yet — only the type references. This keeps the file compiling after the `Variant` rename and matches the final Task 3 return types.

- [ ] **Step 5: Run test to verify it passes**

Run: `pnpm run test:unit -- run src/features/catalog/__tests__/types/variant.spec.ts`
Expected: PASS (all cases).

- [ ] **Step 6: Run type-check**

Run: `pnpm run type-check`
Expected: 0 errors.

- [ ] **Step 7: Commit**

```bash
git add app/Admin/src/features/catalog/types/variant.ts app/Admin/src/features/catalog/services/variantApi.ts app/Admin/src/features/catalog/__tests__/types/variant.spec.ts
git commit -m "feat(catalog): add VariantListItem types and paged query mapping"
```

---
### Task 2: Per-Field Zod Validation

**Files:**
- Modify: `app/Admin/src/features/catalog/validations/variant.ts`
- Modify: `app/Admin/src/features/catalog/validations/index.ts`
- Test: `app/Admin/src/features/catalog/__tests__/validations/variant.spec.ts` (create)

**Interfaces:**
- Consumes: `variantSchema` from `validations/variant.ts` (imported by `views/VariantDetail.vue:26`).
- Produces: `variantSku`, `variantPosition`, `variantIsMaster`, `variantTrackInventory`, `variantWeight`, `variantWeightUnit`, `variantHeight`, `variantWidth`, `variantDepth`, `variantDimensionsUnit`, `variantPrice`, `variantCostPrice`, `variantCostCurrency`, `variantSchema`, `VariantForm` — all named exports from `validations/variant.ts` and re-exported from `validations/index.ts`. Field constraint semantics (nullable-with-default) are UNCHANGED.

- [ ] **Step 1: Write the failing test**

Create `app/Admin/src/features/catalog/__tests__/validations/variant.spec.ts`:

```ts
import { describe, it, expect } from 'vitest'
import {
  variantSku,
  variantPosition,
  variantPrice,
  variantCostCurrency,
  variantSchema,
} from '../../validations/variant'

const validVariant = {
  sku: 'SHIRT-M',
  position: 0,
  isMaster: false,
  trackInventory: true,
  weight: null,
  weightUnit: null,
  height: null,
  width: null,
  depth: null,
  dimensionsUnit: null,
  price: null,
  costPrice: null,
  costCurrency: null,
}

describe('variantSku', () => {
  it('accepts valid sku', () => {
    expect(variantSku.safeParse('SHIRT-M').success).toBe(true)
  })

  it('rejects empty', () => {
    expect(variantSku.safeParse('').success).toBe(false)
  })

  it('rejects whitespace-only', () => {
    expect(variantSku.safeParse('   ').success).toBe(false)
  })

  it('rejects over 255 chars', () => {
    expect(variantSku.safeParse('A'.repeat(256)).success).toBe(false)
  })
})

describe('variantPosition', () => {
  it('accepts 0 and -1', () => {
    expect(variantPosition.safeParse(0).success).toBe(true)
    expect(variantPosition.safeParse(-1).success).toBe(true)
  })

  it('rejects below -1', () => {
    expect(variantPosition.safeParse(-2).success).toBe(false)
  })
})

describe('variantPrice', () => {
  it('accepts null and non-negative', () => {
    expect(variantPrice.safeParse(null).success).toBe(true)
    expect(variantPrice.safeParse(12.5).success).toBe(true)
  })

  it('rejects negative', () => {
    expect(variantPrice.safeParse(-1).success).toBe(false)
  })
})

describe('variantCostCurrency', () => {
  it('accepts 3-letter code', () => {
    expect(variantCostCurrency.safeParse('USD').success).toBe(true)
  })

  it('rejects longer than 3 chars', () => {
    expect(variantCostCurrency.safeParse('USDT').success).toBe(false)
  })
})

describe('variantSchema', () => {
  it('accepts valid form', () => {
    const result = variantSchema.safeParse(validVariant)
    expect(result.success).toBe(true)
  })

  it('rejects empty sku', () => {
    const result = variantSchema.safeParse({ ...validVariant, sku: '' })
    expect(result.success).toBe(false)
  })

  it('accepts null optional fields', () => {
    const result = variantSchema.safeParse(validVariant)
    expect(result.success).toBe(true)
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `pnpm run test:unit -- run src/features/catalog/__tests__/validations/variant.spec.ts`
Expected: FAIL — `variantSku` etc. are not exported from `../../validations/variant`.

- [ ] **Step 3: Implement per-field schemas**

Rewrite `app/Admin/src/features/catalog/validations/variant.ts` to:

```ts
import { z } from 'zod'

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

- [ ] **Step 4: Update the validations barrel**

In `app/Admin/src/features/catalog/validations/index.ts`, replace the variant block (currently lines 47-50):

```ts
export {
  variantSku,
  variantPosition,
  variantIsMaster,
  variantTrackInventory,
  variantWeight,
  variantWeightUnit,
  variantHeight,
  variantWidth,
  variantDepth,
  variantDimensionsUnit,
  variantPrice,
  variantCostPrice,
  variantCostCurrency,
  variantSchema,
} from './variant'
export type { VariantForm } from './variant'
```

- [ ] **Step 5: Run test to verify it passes**

Run: `pnpm run test:unit -- run src/features/catalog/__tests__/validations/variant.spec.ts`
Expected: PASS (all cases).

- [ ] **Step 6: Run type-check**

Run: `pnpm run type-check`
Expected: 0 errors.

- [ ] **Step 7: Commit**

```bash
git add app/Admin/src/features/catalog/validations/variant.ts app/Admin/src/features/catalog/validations/index.ts app/Admin/src/features/catalog/__tests__/validations/variant.spec.ts
git commit -m "feat(catalog): add per-field zod schemas for variants"
```

---
### Task 3: API Services — PagedResult Envelope

**Files:**
- Modify: `app/Admin/src/features/catalog/services/variantApi.ts`
- Modify: `app/Admin/src/features/catalog/services/variantImageApi.ts`
- Modify: `app/Admin/src/features/catalog/services/variantPriceApi.ts`
- Test: `app/Admin/src/features/catalog/__tests__/services/variantApi.spec.ts` (create)
- Test: `app/Admin/src/features/catalog/__tests__/services/variantImageApi.spec.ts` (create)
- Test: `app/Admin/src/features/catalog/__tests__/services/variantPriceApi.spec.ts` (create)

**Interfaces:**
- Consumes: from Task 1 — `VariantListItem`, `VariantDetail`, `VariantQuery`, `toVariantQueryParams`, `VARIANT_FILTER_FIELDS`, `VARIANT_SORT_FIELDS`, `VARIANT_SEARCH_FIELDS`; existing `VariantImage`, `Price`, `OptionValueAssignment`, `VariantRequest`.
- Produces: `VariantApi.getVariants(productId: string, query: VariantQuery): Promise<PagedResult<VariantListItem>>`, `VariantApi.getVariant(id): Promise<Result<VariantDetail>>`, `VariantApi.createVariant(productId, request): Promise<Result<VariantDetail>>`, `VariantApi.updateVariant(id, request): Promise<Result<VariantDetail>>`, `VariantApi.getOptionValues(variantId): Promise<PagedResult<OptionValueAssignment>>`, unchanged `deleteVariant`/`assignOptionValues`/`revokeOptionValues`; `VariantImageApi.listImages(variantId): Promise<PagedResult<VariantImage>>` (unchanged `uploadImage`/`deleteImage`); `VariantPriceApi.listPrices(variantId): Promise<PagedResult<Price>>` (unchanged `setPrice`/`removePrice`/`PriceRequest`).
- View consumers (Task 4/5) rely on: `getVariants(productId, query)` and the `.items` field of PagedResult.

- [ ] **Step 1: Write the failing tests**

Create `app/Admin/src/features/catalog/__tests__/services/variantApi.spec.ts`:

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

import { VariantApi } from '../../services/variantApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('VariantApi.getVariants', () => {
  it('calls getPaged with product URL and query params', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1, pageSize: 20, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })

    await VariantApi.getVariants('prod-1', { search: 'M', page: 2, pageSize: 25 })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/products/prod-1/variants',
      expect.objectContaining({ search: 'M', pageNumber: 2, pageSize: 25 }),
      expect.objectContaining({ allowedFilterFields: expect.any(Array) }),
    )
  })
})

describe('VariantApi.getVariant', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', sku: 'M' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantApi.getVariant('abc-123')
    expect(mockGet).toHaveBeenCalledWith('api/catalog/variants/abc-123')
  })
})

describe('VariantApi.createVariant', () => {
  it('calls POST with request body', async () => {
    const req = { sku: 'SHIRT-M', position: 0, trackInventory: true, isMaster: false, optionValueIds: [] } as any
    mockPost.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await VariantApi.createVariant('prod-1', req)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/products/prod-1/variants', req)
  })
})

describe('VariantApi.updateVariant', () => {
  it('calls PUT with request body', async () => {
    const req = { sku: 'SHIRT-M', position: 1, trackInventory: true, isMaster: false } as any
    mockPut.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantApi.updateVariant('abc-123', req)
    expect(mockPut).toHaveBeenCalledWith('api/catalog/variants/abc-123', req)
  })
})

describe('VariantApi.deleteVariant', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantApi.deleteVariant('abc-123')
    expect(mockDel).toHaveBeenCalledWith('api/catalog/variants/abc-123')
  })
})

describe('VariantApi.getOptionValues', () => {
  it('calls getPaged with option-values URL', async () => {
    mockGetPaged.mockResolvedValue({
      items: [], page: 1, pageSize: 0, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })
    await VariantApi.getOptionValues('abc-123')
    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/variants/abc-123/option-values',
      expect.any(Object),
      expect.any(Object),
    )
  })
})
```

Create `app/Admin/src/features/catalog/__tests__/services/variantImageApi.spec.ts`:

```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockDel, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<any>(),
  mockGet: vi.fn<any>(),
  mockDel: vi.fn<any>(),
  mockGetPaged: vi.fn<any>(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  get: mockGet,
  del: mockDel,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { VariantImageApi } from '../../services/variantImageApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('VariantImageApi.listImages', () => {
  it('calls getPaged with images URL', async () => {
    mockGetPaged.mockResolvedValue({
      items: [], page: 1, pageSize: 100, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })
    await VariantImageApi.listImages('abc-123')
    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/variants/abc-123/images',
      expect.objectContaining({ pageNumber: 1, pageSize: 100 }),
      expect.any(Object),
    )
  })
})

describe('VariantImageApi.uploadImage', () => {
  it('calls POST with form data', async () => {
    const file = new File(['x'], 'a.png', { type: 'image/png' })
    const formData = new FormData()
    formData.append('file', file)
    mockPost.mockResolvedValue({ value: { id: '1' }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await VariantImageApi.uploadImage('abc-123', file)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/variants/abc-123/images', expect.any(FormData))
  })
})

describe('VariantImageApi.deleteImage', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { message: 'ok' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantImageApi.deleteImage('img-1')
    expect(mockDel).toHaveBeenCalledWith('api/catalog/variants/images/img-1')
  })
})
```

Create `app/Admin/src/features/catalog/__tests__/services/variantPriceApi.spec.ts`:

```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockDel, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<any>(),
  mockGet: vi.fn<any>(),
  mockDel: vi.fn<any>(),
  mockGetPaged: vi.fn<any>(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  get: mockGet,
  del: mockDel,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { VariantPriceApi } from '../../services/variantPriceApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('VariantPriceApi.listPrices', () => {
  it('calls getPaged with prices URL', async () => {
    mockGetPaged.mockResolvedValue({
      items: [], page: 1, pageSize: 100, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })
    await VariantPriceApi.listPrices('abc-123')
    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/variants/abc-123/prices',
      expect.objectContaining({ pageNumber: 1, pageSize: 100 }),
      expect.any(Object),
    )
  })
})

describe('VariantPriceApi.setPrice', () => {
  it('calls POST with request body', async () => {
    const req = { amount: 10, currency: 'USD' }
    mockPost.mockResolvedValue({ value: { variantId: 'abc-123' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantPriceApi.setPrice('abc-123', req)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/variants/abc-123/prices', req)
  })
})

describe('VariantPriceApi.removePrice', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await VariantPriceApi.removePrice('abc-123', 'price-1')
    expect(mockDel).toHaveBeenCalledWith('api/catalog/variants/abc-123/prices/price-1')
  })
})
```

- [ ] **Step 2: Run tests to verify they fail**

Run:
`pnpm run test:unit -- run src/features/catalog/__tests__/services/variantApi.spec.ts`
`pnpm run test:unit -- run src/features/catalog/__tests__/services/variantImageApi.spec.ts`
`pnpm run test:unit -- run src/features/catalog/__tests__/services/variantPriceApi.spec.ts`
Expected: FAIL — services still return `Result<{ items }>` / use `get` instead of `getPaged`.

- [ ] **Step 3: Implement the services**

Rewrite `app/Admin/src/features/catalog/services/variantApi.ts` to:

```ts
import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  VariantRequest,
  VariantListItem,
  VariantDetail,
  VariantQuery,
  OptionValueAssignment,
} from '../types/variant'
import {
  toVariantQueryParams,
  VARIANT_FILTER_FIELDS,
  VARIANT_SORT_FIELDS,
  VARIANT_SEARCH_FIELDS,
} from '../types/variant'

const BASE = `${CATALOG}/variants`

export class VariantApi {
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

  static getVariant(id: string): Promise<Result<VariantDetail>> {
    return get<Result<VariantDetail>>(`${BASE}/${id}`)
  }

  static createVariant(
    productId: string,
    request: VariantRequest,
  ): Promise<Result<VariantDetail>> {
    return post<Result<VariantDetail>>(
      `${CATALOG}/products/${productId}/variants`,
      request,
    )
  }

  static updateVariant(
    id: string,
    request: VariantRequest,
  ): Promise<Result<VariantDetail>> {
    return put<Result<VariantDetail>>(`${BASE}/${id}`, request)
  }

  static deleteVariant(id: string): Promise<Result<void>> {
    return del<Result<void>>(`${BASE}/${id}`)
  }

  static getOptionValues(
    variantId: string,
  ): Promise<PagedResult<OptionValueAssignment>> {
    return getPaged<OptionValueAssignment>(
      `${BASE}/${variantId}/option-values`,
      { pageNumber: 1, pageSize: 100 },
    )
  }

  static assignOptionValues(
    variantId: string,
    optionValueIds: string[],
  ): Promise<Result<void>> {
    return post<Result<void>>(
      `${BASE}/${variantId}/option-values/assign`,
      { optionValueIds },
    )
  }

  static revokeOptionValues(
    variantId: string,
    optionValueIds: string[],
  ): Promise<Result<void>> {
    return post<Result<void>>(
      `${BASE}/${variantId}/option-values/revoke`,
      { optionValueIds },
    )
  }
}
```

Rewrite `app/Admin/src/features/catalog/services/variantImageApi.ts` to:

```ts
import { post, get, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type { VariantImage } from '../types/variant'

const BASE = `${CATALOG}/variants`

export class VariantImageApi {
  static listImages(variantId: string): Promise<PagedResult<VariantImage>> {
    return getPaged<VariantImage>(`${BASE}/${variantId}/images`, {
      pageNumber: 1,
      pageSize: 100,
    })
  }

  static uploadImage(
    variantId: string,
    file: File,
  ): Promise<Result<VariantImage>> {
    const formData = new FormData()
    formData.append('file', file)
    return post<Result<VariantImage>>(`${BASE}/${variantId}/images`, formData)
  }

  static deleteImage(imageId: string): Promise<Result<{ message: string }>> {
    return del<Result<{ message: string }>>(`${CATALOG}/variants/images/${imageId}`)
  }
}
```

Rewrite `app/Admin/src/features/catalog/services/variantPriceApi.ts` to:

```ts
import { post, get, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type { Price } from '../types/variant'

const BASE = `${CATALOG}/variants`

export interface PriceRequest {
  amount?: number
  currency: string
  compareAtAmount?: number
  countryIso?: string
}

export class VariantPriceApi {
  static listPrices(variantId: string): Promise<PagedResult<Price>> {
    return getPaged<Price>(`${BASE}/${variantId}/prices`, {
      pageNumber: 1,
      pageSize: 100,
    })
  }

  static setPrice(
    variantId: string,
    request: PriceRequest,
  ): Promise<Result<{ variantId: string }>> {
    return post<Result<{ variantId: string }>>(
      `${BASE}/${variantId}/prices`,
      request,
    )
  }

  static removePrice(
    variantId: string,
    priceId: string,
  ): Promise<Result<void>> {
    return del<Result<void>>(`${BASE}/${variantId}/prices/${priceId}`)
  }
}
```

- [ ] **Step 4: Update the detail view tab loaders (PagedResult consumers)**

In `app/Admin/src/features/catalog/views/VariantDetail.vue`:

- `loadImages` (around line 222): change `if (result.isSuccess && result.value) { images.value = result.value.images }` to `if (result.isSuccess) { images.value = result.items }`.
- `loadPrices` (around line 332): change `if (result.isSuccess && result.value) { prices.value = result.value.items }` to `if (result.isSuccess) { prices.value = result.items }`.
- `loadOptionValues` (around line 289): change `if (result.isSuccess && result.value) { optionValueAssignments.value = result.value.items; selectedOptionValueIds.value = result.value.items }` to `if (result.isSuccess) { optionValueAssignments.value = result.items; selectedOptionValueIds.value = result.items }`.
- Also add `error` reporting in the else branch of all three loaders via `handleResult(result)` (import already present at line 20). For `loadOptionValues`, the else branch currently only logs; replace with `handleResult(result)`.

- [ ] **Step 5: Run tests to verify they pass**

Run all three spec files (commands from Step 2).
Expected: PASS.

- [ ] **Step 6: Run type-check and full unit suite**

Run: `pnpm run type-check`
Expected: 0 errors.
Run: `pnpm run test:unit -- run`
Expected: 584 + new tests passing (585 total tests added across the three new spec files — final count depends on test-case tally).

- [ ] **Step 7: Commit**

```bash
git add app/Admin/src/features/catalog/services/variantApi.ts app/Admin/src/features/catalog/services/variantImageApi.ts app/Admin/src/features/catalog/services/variantPriceApi.ts app/Admin/src/features/catalog/views/VariantDetail.vue app/Admin/src/features/catalog/__tests__/services/variantApi.spec.ts app/Admin/src/features/catalog/__tests__/services/variantImageApi.spec.ts app/Admin/src/features/catalog/__tests__/services/variantPriceApi.spec.ts
git commit -m "feat(catalog): return PagedResult from variant list services"
```

---
### Task 4: VariantsList — usePagedQuery + Paginator + Error Banner

**Files:**
- Modify: `app/Admin/src/features/catalog/views/VariantsList.vue`

**Interfaces:**
- Consumes: from Task 1 — `VariantListItem`, `VARIANT_FILTER_FIELDS`, `VARIANT_SORT_FIELDS`, `VARIANT_SEARCH_FIELDS`; `usePagedQuery` from `@/shared/composables/usePagedQuery`; `VariantApi` (unchanged `deleteVariant`).
- Produces: a server-paged VariantsList page. Route flow unchanged: `catalog/variants?productId=<id>` from ProductsList; `catalog/variants/new?productId=<id>` for new; `catalog/variants/:id` for edit.

- [ ] **Step 1: Verify current behavior compiles (baseline)**

Run: `pnpm run type-check`
Expected: 0 errors (Task 3 left the page compiling with the old inline `getVariants(productId)` call now returning PagedResult — confirm the script section currently calls `VariantApi.getVariants(productId.value)` and reads `result.value.items`; it must have been updated by Task 3 Step 4 only for VariantDetail, so VariantsList may currently fail type-check. If so, proceed — Task 4 fully rewrites it.)

- [ ] **Step 2: Rewrite the script block**

Replace the entire `<script setup lang="ts">` block of `app/Admin/src/features/catalog/views/VariantsList.vue` with:

```ts
import { ref, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import type { DataTablePageEvent, DataTableSortEvent } from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Message from 'primevue/message'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { VariantApi } from '../services/variantApi'
import type { VariantListItem } from '../types/variant'
import {
  VARIANT_FILTER_FIELDS,
  VARIANT_SORT_FIELDS,
  VARIANT_SEARCH_FIELDS,
} from '../types/variant'

const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()

const productId = computed(() => route.query.productId as string | undefined)
const searchTerm = ref('')

const {
  items,
  loading,
  error,
  totalCount,
  page,
  pageSize,
  totalPages,
  setPage,
  setPageSize,
  setSearch,
  setSort,
  refresh,
} = usePagedQuery<VariantListItem>(
  () => `api/catalog/products/${productId.value}/variants`,
  {
    allowedFilterFields: VARIANT_FILTER_FIELDS,
    allowedSortFields: VARIANT_SORT_FIELDS,
    allowedSearchFields: VARIANT_SEARCH_FIELDS,
    defaultSearchFields: VARIANT_SEARCH_FIELDS,
    defaultSearchMode: 'any',
    defaultSort: ['position'],
    defaultPageSize: 20,
    immediate: false,
  },
)

const first = computed(() => (page.value - 1) * pageSize.value)

watch(productId, (id) => {
  if (id) {
    setSearch('')
    refresh()
  }
})

function navigateToNew() {
  if (!productId.value) return
  router.push(`/catalog/variants/new?productId=${productId.value}`)
}

function navigateToEdit(id: string) {
  router.push(`/catalog/variants/${id}`)
}

function navigateToProduct() {
  if (productId.value) {
    router.push(`/catalog/products/${productId.value}`)
  }
}

function onSearch(value: string) {
  searchTerm.value = value
  setSearch(value)
}

function clearSearch() {
  searchTerm.value = ''
  setSearch('')
}

function onPage(event: DataTablePageEvent) {
  setPage(event.page + 1)
}

function onRows(rows: number) {
  setPageSize(rows)
}

function onSort(event: DataTableSortEvent) {
  const field = event.sortField
  if (typeof field !== 'string') return
  setSort(event.sortOrder === -1 ? [`-${field}`] : [field])
}

function confirmDelete(variant: VariantListItem) {
  confirm.require({
    message: `Are you sure you want to delete variant "${variant.sku}"?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const result = await VariantApi.deleteVariant(variant.id)
      if (result.isSuccess) {
        notify.success('Variant deleted', `${variant.sku} has been removed.`)
        refresh()
      } else {
        notify.error('Delete failed', result.errors?.[0]?.message ?? 'Could not delete variant.')
      }
    },
  })
}

function refreshPage() {
  refresh()
}
```

- [ ] **Step 3: Rewrite the template block**

Replace the entire `<template>` block with:

```html
<template>
  <div class="flex flex-col h-full p-4">
    <div class="flex-none flex flex-col gap-4">
      <div class="flex justify-between items-start">
        <div>
          <div class="font-semibold text-xl">Variants</div>
          <p class="text-muted-color mt-1">Manage product variants</p>
        </div>
        <Button
          label="Back to Product"
          icon="pi pi-arrow-left"
          severity="secondary"
          outlined
          @click="navigateToProduct"
        />
      </div>
    </div>

    <div class="flex-1 min-h-0 mt-4">
      <div v-if="!productId" class="flex items-center justify-center h-full">
        <div class="text-center text-muted-color">
          <i class="pi pi-info-circle text-4xl mb-3 block" />
          <p class="text-lg">Select a product to view its variants.</p>
          <p class="text-sm mt-1">Navigate from the Products list to manage variants.</p>
        </div>
      </div>

      <div v-else-if="error" class="flex items-center justify-center h-full">
        <Message severity="error" :closable="false" class="w-full max-w-lg">
          <div class="flex flex-col gap-2">
            <span>{{ error }}</span>
            <Button label="Reload" icon="pi pi-sync" severity="secondary" size="small" @click="refreshPage" />
          </div>
        </Message>
      </div>

      <DataTable
        v-else
        size="large"
        :value="items"
        :loading="loading"
        :total-records="totalCount"
        :first="first"
        :rows="pageSize"
        scrollable
        :paginator="true"
        data-key="id"
        :global-filter-fields="VARIANT_SEARCH_FIELDS"
        paginator-template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :rows-per-page-options="[5, 10, 25]"
        current-page-report-template="Showing {first} to {last} of {totalRecords}"
        @page="onPage"
        @update:rows="onRows"
        @sort="onSort"
        :pt="{ wrapper: { class: 'h-full' }, tableContainer: { class: 'h-full' } }"
      >
        <template #header>
          <div class="flex justify-between items-center">
            <div class="flex items-center gap-2">
              <FloatLabel variant="on">
                <IconField>
                  <InputIcon class="pi pi-search" />
                  <InputText
                    :model-value="searchTerm"
                    placeholder="Search variants..."
                    @update:model-value="onSearch($event ?? '')"
                  />
                </IconField>
                <label>Search</label>
              </FloatLabel>
              <Button label="Clear" outlined @click="clearSearch" />
            </div>
            <div class="flex items-center gap-2">
              <Button label="New Variant" icon="pi pi-plus" severity="primary" @click="navigateToNew" />
              <Button label="Reload" icon="pi pi-sync" severity="secondary" @click="refreshPage" />
            </div>
          </div>
        </template>
        <Column field="isMaster" header="Master" body-style="text-align: center">
          <template #body="{ data }">
            <Tag v-if="data.isMaster" value="Master" severity="info" />
            <span v-else class="text-muted-color">—</span>
          </template>
        </Column>
        <Column field="sku" header="SKU" :sortable="true">
          <template #body="{ data }">
            <span :class="{ 'text-muted-color': !data.sku }">{{ data.sku || '—' }}</span>
          </template>
        </Column>
        <Column field="position" header="Position" :sortable="true" body-style="text-align: center" />
        <Column field="price" header="Price">
          <template #body="{ data }">
            <span v-if="data.price != null">
              {{ data.price.toLocaleString() }} {{ data.costCurrency || '' }}
            </span>
            <span v-else class="text-muted-color">—</span>
          </template>
        </Column>
        <Column field="pricesCount" header="Prices" body-style="text-align: center" />
        <Column header="" body-style="text-align: right; width: 6rem">
          <template #body="{ data }">
            <div class="flex justify-end gap-2">
              <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
              <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="confirmDelete(data)" />
            </div>
          </template>
        </Column>
        <template #empty>
          <div class="text-center py-8 text-muted-color">No variants found for this product.</div>
        </template>
      </DataTable>
    </div>
  </div>
</template>
```

- [ ] **Step 4: Run type-check**

Run: `pnpm run type-check`
Expected: 0 errors.

- [ ] **Step 5: Run build and unit suite**

Run: `pnpm run build-only`
Expected: build succeeds.
Run: `pnpm run test:unit -- run`
Expected: all tests pass (no regressions).

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/features/catalog/views/VariantsList.vue
git commit -m "feat(catalog): drive VariantsList with server-side paged query"
```

---
### Task 5: VariantDetail — PagedResult Tab Consumers Finalize

**Files:**
- Modify: `app/Admin/src/features/catalog/views/VariantDetail.vue`

**Interfaces:**
- Consumes: Task 3 services (`VariantApi.getVariant` → `Result<VariantDetail>`, `VariantApi.getOptionValues` → `PagedResult<OptionValueAssignment>`, `VariantImageApi.listImages` → `PagedResult<VariantImage>`, `VariantPriceApi.listPrices` → `PagedResult<Price>`).
- Produces: final consistent `.items` reads in all three tab loaders (already done in Task 3 Step 4). This task is a verification + fix pass in case Task 3 left any residual `result.value` access.

- [ ] **Step 1: Audit remaining envelope accesses**

Run: `rg "result\.value|\.value\.items|\.value\.images" app/Admin/src/features/catalog/views/VariantDetail.vue`
Expected: no matches in `loadImages`/`loadPrices`/`loadOptionValues`. If any remain, replace `result.value.X` with the corresponding `result.items` read (task 3 pattern).

- [ ] **Step 2: Confirm failure paths use handleResult**

Inspect the else branches of `loadImages`, `loadPrices`, `loadOptionValues` in `VariantDetail.vue`. Each must call `handleResult(result)` (imported from `@/shared/composables/useApiErrorHandler`). Add it where missing.

- [ ] **Step 3: Run type-check**

Run: `pnpm run type-check`
Expected: 0 errors.

- [ ] **Step 4: Run full unit suite and build**

Run: `pnpm run test:unit -- run`
Expected: all pass.
Run: `pnpm run build-only`
Expected: build succeeds.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/catalog/views/VariantDetail.vue
git commit -m "fix(catalog): read PagedResult items in variant tab loaders"
```

---
### Task 6: Final Verification

**Files:**
- No source changes.

- [ ] **Step 1: Full Admin verification**

Run: `pnpm run type-check` — Expected: 0 errors.
Run: `pnpm run test:unit -- run` — Expected: all tests pass.
Run: `pnpm run build-only` — Expected: build succeeds.
Run: `pnpm run lint` — Expected: no NEW lint errors in changed files (pre-existing `parsers.spec.ts` failures are unrelated and not to be fixed here).

- [ ] **Step 2: C# build safety net (no changes expected)**

Run: `dotnet build service/Api/src/Module/Module.csproj`
Expected: 0 warnings, 0 errors (backend untouched — confirms no accidental C# edits).

- [ ] **Step 3: Confirm clean tree**

Run: `git status --short`
Expected: clean working tree after the Task 5 commit.
