# Admin SPA API Service Correction Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix every API endpoint mismatch in the Admin SPA so all service calls resolve to actual backend routes.

**Architecture:** Root cause is 11 incorrect route constants in `constants.ts` plus flat CRUD paths where backend nests resources under parents. Fix constants first (foundation), then rewrite 4 service files (catalog, identity, inventory, ordering), update auth, remove dead dashboard/report calls, and fix token refresh.

**Tech Stack:** TypeScript, Vue 3, Pinia, Axios, Vite

## Global Constraints

- No proxy or middleware rewrites paths — backend routes are literal `api/{module}/{resource}`
- `api.client.ts:7` baseURL is `/api` — all paths in this plan are relative to it
- TypeScript 6, strict mode, no `any` where avoidable
- TDD: each step writes/updates test first
- Commit after each green test run

## File Structure

```
app/Admin/src/shared/api/constants.ts          ← MODIFY: fix all constant values
app/Admin/src/shared/api/http/refresh-handler.ts ← MODIFY: fix refresh URL
app/Admin/src/features/catalog/services/catalog.api.ts  ← REWRITE
app/Admin/src/features/catalog/products/services/product.service.ts  ← MODIFY
app/Admin/src/features/catalog/products/stores/product.store.ts  ← MODIFY
app/Admin/src/features/catalog/option-types/option-values/services/option-value.service.ts  ← MODIFY
app/Admin/src/features/catalog/taxonomies/services/taxonomy.service.ts  ← no change
app/Admin/src/features/catalog/taxonomies/taxa/services/taxon.service.ts  ← REWRITE
app/Admin/src/features/catalog/taxonomies/stores/taxonomy.store.ts  ← MODIFY
app/Admin/src/features/catalog/taxonomies/taxa/stores/taxon.store.ts  ← MODIFY
app/Admin/src/features/identity/services/identity.api.ts  ← REWRITE
app/Admin/src/features/inventories/services/inventory.api.ts  ← REWRITE
app/Admin/src/features/inventories/services/inventory.service.ts  ← MODIFY
app/Admin/src/features/inventories/stores/inventory.store.ts  ← MODIFY
app/Admin/src/features/ordering/services/ordering.api.ts  ← REWRITE
app/Admin/src/features/ordering/services/order.service.ts  ← MODIFY
app/Admin/src/features/ordering/stores/order.store.ts  ← MODIFY
app/Admin/src/features/ordering/fulfillment/services/fulfillment.service.ts  ← MODIFY
app/Admin/src/features/auth/services/auth.service.ts  ← REWRITE
app/Admin/src/features/auth/stores/auth.store.ts  ← MODIFY
app/Admin/src/features/profile/services/profile.api.ts  ← MODIFY
app/Admin/src/features/reports/services/reports.api.ts  ← MODIFY (remove dead routes)
app/Admin/src/features/reports/stores/report.store.ts  ← MODIFY
app/Admin/src/features/reports/types/report.types.ts  ← MODIFY (remove unused types)
app/Admin/src/features/catalog/dashboard/services/catalog-dashboard.service.ts  ← MODIFY
app/Admin/src/features/catalog/dashboard/stores/catalog-dashboard.store.ts  ← MODIFY

# Test files
app/Admin/src/features/catalog/_tests/catalog.api.spec.ts  ← CREATE
app/Admin/src/features/identity/_tests/identity.api.spec.ts  ← CREATE
app/Admin/src/features/inventories/_tests/inventory.api.spec.ts  ← CREATE
app/Admin/src/features/ordering/_tests/ordering.api.spec.ts  ← CREATE
```

---

### Task 1: Fix base route constants

**Files:**
- Modify: `app/Admin/src/shared/api/constants.ts` (entire file)
- Test: verify all 8 constants match backend routes

**Interfaces:**
- Consumes: nothing
- Produces: `CATALOG = 'api/catalog'`, `IDENTITY = 'api/identity'`, `LOCATIONS = 'api/locations'`, `PROFILES = 'api/profiles'`, `INVENTORY = 'api/inventory'`, `ORDERS = 'api/ordering'`, `PAYMENTS = 'api/payment'`, `SHIPPING = 'api/shipping'`

- [ ] **Step 1: Write the failing test**

```typescript
// app/Admin/src/features/shared/_tests/constants.spec.ts
import { describe, it, expect } from 'vitest'
import { CATALOG, IDENTITY, LOCATIONS, PROFILES, INVENTORY, ORDERS, PAYMENTS, SHIPPING } from '@/shared/api/constants'

describe('API constants', () => {
  it('CATALOG matches backend', () => {
    expect(CATALOG).toBe('api/catalog')
  })
  it('IDENTITY matches backend', () => {
    expect(IDENTITY).toBe('api/identity')
  })
  it('LOCATIONS matches backend', () => {
    expect(LOCATIONS).toBe('api/locations')
  })
  it('PROFILES matches backend', () => {
    expect(PROFILES).toBe('api/profiles')
  })
  it('INVENTORY matches backend', () => {
    expect(INVENTORY).toBe('api/inventory')
  })
  it('ORDERS matches backend', () => {
    expect(ORDERS).toBe('api/ordering')
  })
  it('PAYMENTS matches backend', () => {
    expect(PAYMENTS).toBe('api/payment')
  })
  it('SHIPPING matches backend', () => {
    expect(SHIPPING).toBe('api/shipping')
  })
})
```

Run: `npx vitest run src/features/shared/_tests/constants.spec.ts`
Expected: FAIL — current values are `/admin/catalog` etc.

- [ ] **Step 2: Run test to verify it fails**

```bash
npx vitest run app/Admin/src/features/shared/_tests/constants.spec.ts -v
```
Expected: FAIL — asserts `'api/catalog'` but value is `'/admin/catalog'`

- [ ] **Step 3: Rewrite constants.ts**

```typescript
// app/Admin/src/shared/api/constants.ts
export const CATALOG = 'api/catalog'
export const IDENTITY = 'api/identity'
export const LOCATIONS = 'api/locations'
export const PROFILES = 'api/profiles'
export const INVENTORY = 'api/inventory'
export const ORDERS = 'api/ordering'
export const PAYMENTS = 'api/payment'
export const SHIPPING = 'api/shipping'
```

- [ ] **Step 4: Update all imports referencing old constants**

Search-replace across `src/features/`:
- `from '@/shared/api/constants'` → unchanged (module path same)
- Any `LOCATION` (singular) → `LOCATIONS`
- Any `{ DASHBOARD, SEARCH, FILES, ACCOUNT, AUTH }` in destructured imports from constants — remove those identifiers. Only `DASHBOARD` is used (in `catalog.api.ts` dashboard section); the rest are never imported.

In `catalog.api.ts` line 598: change `import { CATALOG } from '@/shared/api/constants'` — stays same (only CATALOG is used from constants).

- [ ] **Step 5: Run test to verify it passes**

```bash
npx vitest run app/Admin/src/features/shared/_tests/constants.spec.ts -v
```
Expected: PASS — all 8 assertions

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/shared/api/constants.ts app/Admin/src/features/shared/_tests/constants.spec.ts
git commit -m "fix(admin): correct API base route constants to match backend"
```

---

### Task 2: Rewrite catalog.api.ts — products + variants

**Files:**
- Rewrite: `app/Admin/src/features/catalog/services/catalog.api.ts`
- Modify: `app/Admin/src/features/catalog/products/services/product.service.ts`
- Modify: `app/Admin/src/features/catalog/products/stores/product.store.ts`
- Create: `app/Admin/src/features/catalog/_tests/catalog.api.spec.ts`

**Interfaces:**
- Consumes: `CATALOG` from constants (now `'api/catalog'`)
- Produces: corrected `catalogApi` with proper nested resource paths

**Backend reference:**
- Products CRUD: `api/catalog/products`, `api/catalog/products/{id}`
- Product option types: `api/catalog/products/{id}/option-types`, `.../option-types/sync`
- Product classifications: `api/catalog/products/{id}/classifications`, `.../classifications/sync`
- Product variants: `api/catalog/products/{productId}/variants`, `api/catalog/products/variants/{id}`
- Variant prices: `api/catalog/products/variants/{variantId}/prices`
- Variant images: `api/catalog/products/variants/{variantId}/images`
- Variant option values: `api/catalog/products/variants/{variantId}/option-values/sync`

- [ ] **Step 1: Write the failing test**

```typescript
// app/Admin/src/features/catalog/_tests/catalog.api.spec.ts
import { describe, it, expect, vi } from 'vitest'
import apiClient from '@/shared/api/http/api.client'
import { catalogApi } from '../services/catalog.api'

vi.mock('@/shared/api/http/api.client', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  }
}))

describe('catalogApi.products', () => {
  it('list calls correct route', async () => {
    await catalogApi.products.list({ page: 1, pageSize: 10 })
    expect(apiClient.get).toHaveBeenCalledWith('api/catalog/products', expect.any(Object))
  })
  it('getById calls correct route', async () => {
    await catalogApi.products.getById('guid-1')
    expect(apiClient.get).toHaveBeenCalledWith('api/catalog/products/guid-1')
  })
  it('create calls correct route', async () => {
    await catalogApi.products.create({ name: 'Test', price: 10 })
    expect(apiClient.post).toHaveBeenCalledWith('api/catalog/products', expect.any(Object))
  })
  it('delete calls correct route', async () => {
    await catalogApi.products.delete('guid-1')
    expect(apiClient.delete).toHaveBeenCalledWith('api/catalog/products/guid-1')
  })
})

describe('catalogApi.variants', () => {
  it('create calls correct route', async () => {
    await catalogApi.variants.create('prod-1', { sku: 'TST', price: 10 } as any)
    expect(apiClient.post).toHaveBeenCalledWith('api/catalog/products/prod-1/variants', expect.any(Object))
  })
  it('getById calls correct route', async () => {
    await catalogApi.variants.getById('var-1')
    expect(apiClient.get).toHaveBeenCalledWith('api/catalog/products/variants/var-1')
  })
})
```

Run: `npx vitest run app/Admin/src/features/catalog/_tests/catalog.api.spec.ts`
Expected: FAIL — old routes use `/admin/catalog/...`

- [ ] **Step 2: Rewrite catalog.api.ts**

```typescript
// app/Admin/src/features/catalog/services/catalog.api.ts
import apiClient from '@/shared/api/http/api.client'
import { CATALOG } from '@/shared/api/constants'
import type { ApiResult } from '@/shared/api/types/api.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'
import type { ProductDetail, ProductSummary, CreateProductRequest, UpdateProductRequest } from '../products/types/product.types'
import type { VariantDetail, VariantSummary, CreateVariantRequest, UpdateVariantRequest } from '../products/types/variant.types'
import type { OptionTypeDetail, OptionTypeListItem } from '../option-types/types/option-type.types'
import type { OptionValueListItem, CreateOptionValueRequest, UpdateOptionValueRequest } from '../option-types/option-values/types/option-value.types'
import type { PropertyTypeDetail } from '../property-types/types/property-type.types'
import type { TaxonomyDetail, TaxonomyListItem, CreateTaxonomyRequest, UpdateTaxonomyRequest } from '../taxonomies/types/taxonomy.types'
import type { TaxonDetail, TaxonListItem, TaxonTreeItem, CreateTaxonRequest, UpdateTaxonRequest, TaxonRuleListItem, CreateTaxonRuleRequest, UpdateTaxonRuleRequest } from '../taxonomies/taxa/types/taxon.types'

export const catalogApi = {
  products: {
    async list(params?: ServerQueryingParameters): Promise<ApiResult<ProductSummary[]>> {
      return apiClient.get(`${CATALOG}/products`, { params })
    },
    async getById(id: string): Promise<ApiResult<ProductDetail>> {
      return apiClient.get(`${CATALOG}/products/${id}`)
    },
    async create(data: CreateProductRequest): Promise<ApiResult<ProductDetail>> {
      return apiClient.post(`${CATALOG}/products`, data)
    },
    async update(id: string, data: UpdateProductRequest): Promise<ApiResult<ProductDetail>> {
      return apiClient.put(`${CATALOG}/products/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/products/${id}`)
    },
    async activate(id: string): Promise<ApiResult<void>> {
      return apiClient.patch(`${CATALOG}/products/${id}/activate`)
    },
    async discontinue(id: string): Promise<ApiResult<void>> {
      return apiClient.patch(`${CATALOG}/products/${id}/discontinue`)
    },
    // Option Types
    async getOptionTypes(productId: string): Promise<ApiResult<OptionTypeDetail[]>> {
      return apiClient.get(`${CATALOG}/products/${productId}/option-types`)
    },
    async syncOptionTypes(productId: string, optionTypeIds: string[]): Promise<ApiResult<void>> {
      return apiClient.put(`${CATALOG}/products/${productId}/option-types/sync`, { optionTypeIds })
    },
    // Classifications
    async getClassifications(productId: string): Promise<ApiResult<any[]>> {
      return apiClient.get(`${CATALOG}/products/${productId}/classifications`)
    },
    async syncClassifications(productId: string, data: { taxonIds: string[]; mainTaxonId?: string }): Promise<ApiResult<void>> {
      return apiClient.put(`${CATALOG}/products/${productId}/classifications/sync`, data)
    },
  },

  variants: {
    async getById(id: string): Promise<ApiResult<VariantDetail>> {
      return apiClient.get(`${CATALOG}/products/variants/${id}`)
    },
    async listByProductId(productId: string): Promise<ApiResult<VariantSummary[]>> {
      return apiClient.get(`${CATALOG}/products/${productId}/variants`)
    },
    async create(productId: string, data: CreateVariantRequest): Promise<ApiResult<VariantDetail>> {
      return apiClient.post(`${CATALOG}/products/${productId}/variants`, data)
    },
    async update(id: string, data: UpdateVariantRequest): Promise<ApiResult<VariantDetail>> {
      return apiClient.put(`${CATALOG}/products/variants/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/products/variants/${id}`)
    },
    // Prices
    async listPrices(variantId: string): Promise<ApiResult<any[]>> {
      return apiClient.get(`${CATALOG}/products/variants/${variantId}/prices`)
    },
    async setPrice(variantId: string, data: { amount: number; currency: string }): Promise<ApiResult<any>> {
      return apiClient.post(`${CATALOG}/products/variants/${variantId}/prices`, data)
    },
    async deletePrice(variantId: string, priceId: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/products/variants/${variantId}/prices/${priceId}`)
    },
    async syncPrices(variantId: string, prices: Array<{ amount: number; currency: string }>): Promise<ApiResult<void>> {
      return apiClient.post(`${CATALOG}/products/variants/${variantId}/prices/sync`, prices)
    },
    // Option Values
    async syncOptionValues(variantId: string, optionValueIds: string[]): Promise<ApiResult<void>> {
      return apiClient.put(`${CATALOG}/products/variants/${variantId}/option-values/sync`, { optionValueIds })
    },
    // Images
    async listImages(variantId: string): Promise<ApiResult<any[]>> {
      return apiClient.get(`${CATALOG}/products/variants/${variantId}/images`)
    },
    async uploadImage(variantId: string, file: File, role?: number): Promise<ApiResult<any>> {
      const formData = new FormData()
      formData.append('file', file)
      let url = `${CATALOG}/products/variants/${variantId}/images`
      if (role !== undefined) url += `?role=${role}`
      return apiClient.post(url, formData, { headers: { 'Content-Type': 'multipart/form-data' } })
    },
    async deleteImage(imageId: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/products/variants/images/${imageId}`)
    },
    async updateImage(imageId: string, data: { alt?: string; role?: number }): Promise<ApiResult<void>> {
      return apiClient.put(`${CATALOG}/products/variants/images/${imageId}`, data)
    },
  },

  optionTypes: {
    async list(params?: ServerQueryingParameters): Promise<ApiResult<OptionTypeListItem[]>> {
      return apiClient.get(`${CATALOG}/option-types`, { params })
    },
    async getById(id: string): Promise<ApiResult<OptionTypeDetail>> {
      return apiClient.get(`${CATALOG}/option-types/${id}`)
    },
    async create(data: { name: string; presentation: string; filterable?: boolean; position?: number }): Promise<ApiResult<OptionTypeDetail>> {
      return apiClient.post(`${CATALOG}/option-types`, data)
    },
    async update(id: string, data: Partial<{ name: string; presentation: string; filterable: boolean; position: number }>): Promise<ApiResult<OptionTypeDetail>> {
      return apiClient.put(`${CATALOG}/option-types/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/option-types/${id}`)
    },
    // Nested Option Values
    async listValues(optionTypeId: string, params?: ServerQueryingParameters): Promise<ApiResult<OptionValueListItem[]>> {
      return apiClient.get(`${CATALOG}/option-types/${optionTypeId}/values`, { params })
    },
    async createValue(optionTypeId: string, data: { name: string; presentation: string; position?: number }): Promise<ApiResult<OptionValueListItem>> {
      return apiClient.post(`${CATALOG}/option-types/${optionTypeId}/values`, data)
    },
    async updateValue(optionTypeId: string, valueId: string, data: { name?: string; presentation?: string; position?: number }): Promise<ApiResult<OptionValueListItem>> {
      return apiClient.put(`${CATALOG}/option-types/${optionTypeId}/values/${valueId}`, data)
    },
    async deleteValue(optionTypeId: string, valueId: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/option-types/${optionTypeId}/values/${valueId}`)
    },
  },

  propertyTypes: {
    async list(params?: ServerQueryingParameters): Promise<ApiResult<PropertyTypeDetail[]>> {
      return apiClient.get(`${CATALOG}/property-types`, { params })
    },
    async getById(id: string): Promise<ApiResult<PropertyTypeDetail>> {
      return apiClient.get(`${CATALOG}/property-types/${id}`)
    },
    async create(data: { name: string; presentation: string; kind?: number; filterable?: boolean }): Promise<ApiResult<PropertyTypeDetail>> {
      return apiClient.post(`${CATALOG}/property-types`, data)
    },
    async update(id: string, data: Partial<{ name: string; presentation: string; kind: number; filterable: boolean }>): Promise<ApiResult<PropertyTypeDetail>> {
      return apiClient.put(`${CATALOG}/property-types/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/property-types/${id}`)
    },
  },

  taxonomies: {
    async list(params?: ServerQueryingParameters): Promise<ApiResult<TaxonomyListItem[]>> {
      return apiClient.get(`${CATALOG}/taxonomies`, { params })
    },
    async getById(id: string): Promise<ApiResult<TaxonomyDetail>> {
      return apiClient.get(`${CATALOG}/taxonomies/${id}`)
    },
    async create(data: CreateTaxonomyRequest): Promise<ApiResult<TaxonomyDetail>> {
      return apiClient.post(`${CATALOG}/taxonomies`, data)
    },
    async update(id: string, data: UpdateTaxonomyRequest): Promise<ApiResult<TaxonomyDetail>> {
      return apiClient.put(`${CATALOG}/taxonomies/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/taxonomies/${id}`)
    },
    async restore(id: string): Promise<ApiResult<void>> {
      return apiClient.patch(`${CATALOG}/taxonomies/${id}/restore`)
    },
    // Nested Taxons
    async listTaxons(taxonomyId: string, params?: ServerQueryingParameters & { includeLeavesOnly?: boolean }): Promise<ApiResult<TaxonListItem[]>> {
      return apiClient.get(`${CATALOG}/taxonomies/${taxonomyId}/taxons`, { params })
    },
    async getTaxonTree(taxonomyId: string): Promise<ApiResult<TaxonTreeItem[]>> {
      return apiClient.get(`${CATALOG}/taxonomies/${taxonomyId}/taxons/tree`)
    },
    async getTaxonById(taxonomyId: string, taxonId: string): Promise<ApiResult<TaxonDetail>> {
      return apiClient.get(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}`)
    },
    async createTaxon(taxonomyId: string, data: CreateTaxonRequest): Promise<ApiResult<TaxonDetail>> {
      return apiClient.post(`${CATALOG}/taxonomies/${taxonomyId}/taxons`, data)
    },
    async updateTaxon(taxonomyId: string, taxonId: string, data: UpdateTaxonRequest): Promise<ApiResult<TaxonDetail>> {
      return apiClient.put(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}`, data)
    },
    async deleteTaxon(taxonomyId: string, taxonId: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}`)
    },
    async repositionTaxon(taxonomyId: string, taxonId: string, position: number): Promise<ApiResult<void>> {
      return apiClient.post(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/reposition`, { position })
    },
    async restoreTaxon(taxonomyId: string, taxonId: string): Promise<ApiResult<void>> {
      return apiClient.patch(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/restore`)
    },
    // Nested Taxons → Rules
    async listTaxonRules(taxonomyId: string, taxonId: string): Promise<ApiResult<TaxonRuleListItem[]>> {
      return apiClient.get(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/rules`)
    },
    async createTaxonRule(taxonomyId: string, taxonId: string, data: CreateTaxonRuleRequest): Promise<ApiResult<TaxonRuleListItem>> {
      return apiClient.post(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/rules`, data)
    },
    async updateTaxonRule(taxonomyId: string, taxonId: string, ruleId: string, data: UpdateTaxonRuleRequest): Promise<ApiResult<TaxonRuleListItem>> {
      return apiClient.put(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/rules/${ruleId}`, data)
    },
    async deleteTaxonRule(taxonomyId: string, taxonId: string, ruleId: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/rules/${ruleId}`)
    },
    async syncTaxonRules(taxonomyId: string, taxonId: string, rules: CreateTaxonRuleRequest[]): Promise<ApiResult<void>> {
      return apiClient.post(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/rules/sync`, rules)
    },
    async regenerateTaxonProducts(taxonomyId: string, taxonId: string): Promise<ApiResult<void>> {
      return apiClient.post(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/rules/regenerate`)
    },
  },
}
```

- [ ] **Step 3: Update product.service.ts to delegate to new catalogApi**

```typescript
// app/Admin/src/features/catalog/products/services/product.service.ts
import { catalogApi } from '../../services/catalog.api'

export const productService = {
  list: catalogApi.products.list,
  getById: catalogApi.products.getById,
  create: catalogApi.products.create,
  update: catalogApi.products.update,
  delete: catalogApi.products.delete,
  activate: catalogApi.products.activate,
  discontinue: catalogApi.products.discontinue,
  getOptionTypes: catalogApi.products.getOptionTypes,
  syncOptionTypes: catalogApi.products.syncOptionTypes,
  getClassifications: catalogApi.products.getClassifications,
  syncClassifications: catalogApi.products.syncClassifications,
}
```

- [ ] **Step 4: Update product.store.ts — remove hardcoded `/admin/...` calls**

In `product.store.ts`, replace the two hardcoded paths:

Line ~1549: Replace:
```typescript
const result = await apiClient.get(`/admin/catalog/products/${productId}/classifications`) as unknown as ApiResult<any>
```
With:
```typescript
const result = await productService.getClassifications(productId)
```

Line ~1562: Replace:
```typescript
const result = await apiClient.put(`/admin/catalog/products/${productId}/classifications`, data) as unknown as ApiResult<any>
```
With:
```typescript
const result = await productService.syncClassifications(productId, data)
```

- [ ] **Step 5: Update option-value.service.ts to delegate to catalogApi.optionTypes**

```typescript
// app/Admin/src/features/catalog/option-types/option-values/services/option-value.service.ts
import { catalogApi } from '../../../services/catalog.api'

export const optionValueService = {
  list: catalogApi.optionTypes.listValues,
  getById: (_optionTypeId: string, id: string) => {
    // optionTypeId is needed for the path — caller must pass it
    throw new Error('Use catalogApi.optionTypes directly — requires optionTypeId')
  },
  create: catalogApi.optionTypes.createValue,
  update: catalogApi.optionTypes.updateValue,
  delete: catalogApi.optionTypes.deleteValue,
}
```

- [ ] **Step 6: Update taxon.service.ts to delegate to catalogApi.taxonomies**

```typescript
// app/Admin/src/features/catalog/taxonomies/taxa/services/taxon.service.ts
import { catalogApi } from '../../../services/catalog.api'

export const taxonService = {
  list: catalogApi.taxonomies.listTaxons,
  getTree: catalogApi.taxonomies.getTaxonTree,
  getById: catalogApi.taxonomies.getTaxonById,
  create: catalogApi.taxonomies.createTaxon,
  update: catalogApi.taxonomies.updateTaxon,
  delete: catalogApi.taxonomies.deleteTaxon,
  reposition: catalogApi.taxonomies.repositionTaxon,
  restore: catalogApi.taxonomies.restoreTaxon,
  getRules: catalogApi.taxonomies.listTaxonRules,
  addRule: catalogApi.taxonomies.createTaxonRule,
  updateRule: catalogApi.taxonomies.updateTaxonRule,
  deleteRule: catalogApi.taxonomies.deleteTaxonRule,
  syncRules: catalogApi.taxonomies.syncTaxonRules,
  regenerateProducts: catalogApi.taxonomies.regenerateTaxonProducts,
}
```

- [ ] **Step 7: Update taxonomy.store.ts — replace hardcoded rebuild route**

Line ~2243 of `taxonomy.store.ts`:
Replace:
```typescript
const result = (await apiClient.post(`/admin/catalog/taxonomies/${id}/rebuild`)) as unknown as ApiResult<void>
```
With:
```typescript
const result = await catalogApi.taxonomies.restore(id)
```

Add import at top:
```typescript
import { catalogApi } from '../../services/catalog.api'
```

- [ ] **Step 8: Update taxon.store.ts — pass taxonomyId to all service calls**

All calls in `taxon.store.ts` must pass `taxonomyId` as first argument. For example:

Replace:
```typescript
const result = await taxonService.create({ ...request, taxonomyId: taxonomyId } as CreateTaxonRequest)
```
With:
```typescript
const result = await taxonService.create(taxonomyId, { ...request })
```

Replace:
```typescript
const result = await taxonService.update(taxonId, { ...request, taxonomyId: taxonomyId })
```
With:
```typescript
const result = await taxonService.update(taxonomyId, taxonId, { ...request })
```

Replace:
```typescript
const result = await taxonService.delete(taxonId)
```
With:
```typescript
const result = await taxonService.delete(taxonomyId, taxonId)
```

- [ ] **Step 9: Run all catalog tests to verify**

```bash
npx vitest run app/Admin/src/features/catalog/_tests/
```
Expected: PASS

- [ ] **Step 10: Commit**

```bash
git add app/Admin/src/features/catalog/
git commit -m "fix(admin): rewrite catalog API service with correct nested routes"
```

---

### Task 3: Rewrite identity.api.ts

**Files:**
- Rewrite: `app/Admin/src/features/identity/services/identity.api.ts`
- Create: `app/Admin/src/features/identity/_tests/identity.api.spec.ts`

**Interfaces:**
- Consumes: `IDENTITY` from constants (now `'api/identity'`)
- Produces: corrected `identityApi` with proper sub-resource verbs

**Backend routes:**
- User roles: `GET api/identity/users/{id}/roles`, `POST .../roles/assign`, `POST .../roles/revoke`, `PATCH .../roles/sync`
- User permissions: `GET api/identity/users/{id}/permissions`, `POST .../permissions/assign`, `DELETE .../permissions/revoke`, `PUT .../permissions/sync`
- Role permissions: `GET api/identity/roles/{id}/permissions`, `PUT .../permissions/assign`, `DELETE .../permissions/revoke`, `PATCH .../permissions/sync`

- [ ] **Step 1: Write the failing test**

```typescript
// app/Admin/src/features/identity/_tests/identity.api.spec.ts
import { describe, it, expect, vi } from 'vitest'
import apiClient from '@/shared/api/http/api.client'
import { identityApi } from '../services/identity.api'

vi.mock('@/shared/api/http/api.client', () => ({
  default: { get: vi.fn(), post: vi.fn(), put: vi.fn(), patch: vi.fn(), delete: vi.fn() }
}))

describe('identityApi.users', () => {
  it('listAdmins calls correct route', async () => {
    await identityApi.users.listAdmins({ page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('api/identity/users', expect.any(Object))
  })
  it('assignRole calls correct route', async () => {
    await identityApi.users.assignRole('uid-1', 'admin')
    expect(apiClient.post).toHaveBeenCalledWith('api/identity/users/uid-1/roles/assign', { roleName: 'admin' })
  })
  it('syncRoles calls correct route', async () => {
    await identityApi.users.syncRoles('uid-1', ['admin'])
    expect(apiClient.patch).toHaveBeenCalledWith('api/identity/users/uid-1/roles/sync', { roleNames: ['admin'] })
  })
  it('assignPermission calls correct route', async () => {
    await identityApi.users.assignPermission('uid-1', 'catalog.read')
    expect(apiClient.post).toHaveBeenCalledWith('api/identity/users/uid-1/permissions/assign', { permissionName: 'catalog.read' })
  })
  it('unassignPermission calls correct route', async () => {
    await identityApi.users.unassignPermission('uid-1', 'catalog.read')
    expect(apiClient.delete).toHaveBeenCalledWith('api/identity/users/uid-1/permissions/revoke', expect.any(Object))
  })
})
```

Run: `npx vitest run app/Admin/src/features/identity/_tests/identity.api.spec.ts`
Expected: FAIL

- [ ] **Step 2: Rewrite identity.api.ts**

```typescript
// app/Admin/src/features/identity/services/identity.api.ts
import apiClient from '@/shared/api/http/api.client'
import { IDENTITY } from '@/shared/api/constants'
import type { ApiResult } from '@/shared/api/types/api.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'
import type { AdminUserSummary, CustomerSummary, CreateAdminUserRequest, UpdateAdminUserRequest } from '../../users/types/user.types'
import type { RoleSummary, CreateRoleRequest, UpdateRoleRequest } from '../../users/types/user.types'
import type { PermissionSummary } from '../../users/types/user.types'

export const identityApi = {
  users: {
    async list(params?: ServerQueryingParameters): Promise<ApiResult<AdminUserSummary[]>> {
      return apiClient.get(`${IDENTITY}/users`, { params })
    },
    async listCustomers(params?: ServerQueryingParameters): Promise<ApiResult<CustomerSummary[]>> {
      return apiClient.get(`${IDENTITY}/users`, { params: { ...params, role: 'Storefront.Customer' } })
    },
    async getById(id: string): Promise<ApiResult<AdminUserSummary>> {
      return apiClient.get(`${IDENTITY}/users/${id}`)
    },
    async create(data: CreateAdminUserRequest): Promise<ApiResult<AdminUserSummary>> {
      return apiClient.post(`${IDENTITY}/users`, data)
    },
    async update(id: string, data: UpdateAdminUserRequest): Promise<ApiResult<AdminUserSummary>> {
      return apiClient.put(`${IDENTITY}/users/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${IDENTITY}/users/${id}`)
    },
    async updateStatus(id: string, isActive: boolean): Promise<ApiResult<void>> {
      return apiClient.patch(`${IDENTITY}/users/${id}/status`, { isActive })
    },

    // Roles
    async getRoles(id: string): Promise<ApiResult<string[]>> {
      return apiClient.get(`${IDENTITY}/users/${id}/roles`)
    },
    async assignRole(id: string, roleName: string): Promise<ApiResult<void>> {
      return apiClient.post(`${IDENTITY}/users/${id}/roles/assign`, { roleName })
    },
    async revokeRole(id: string, roleName: string): Promise<ApiResult<void>> {
      return apiClient.post(`${IDENTITY}/users/${id}/roles/revoke`, { roleName })
    },
    async syncRoles(id: string, roleNames: string[]): Promise<ApiResult<void>> {
      return apiClient.patch(`${IDENTITY}/users/${id}/roles/sync`, { roleNames })
    },

    // Permissions
    async getPermissions(id: string): Promise<ApiResult<string[]>> {
      return apiClient.get(`${IDENTITY}/users/${id}/permissions`)
    },
    async assignPermission(id: string, permissionName: string): Promise<ApiResult<void>> {
      return apiClient.post(`${IDENTITY}/users/${id}/permissions/assign`, { permissionName })
    },
    async revokePermission(id: string, permissionName: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${IDENTITY}/users/${id}/permissions/revoke`, { data: { permissionName } })
    },
    async syncPermissions(id: string, permissionNames: string[]): Promise<ApiResult<void>> {
      return apiClient.put(`${IDENTITY}/users/${id}/permissions/sync`, { permissionNames })
    },
  },

  roles: {
    async list(params?: ServerQueryingParameters): Promise<ApiResult<RoleSummary[]>> {
      return apiClient.get(`${IDENTITY}/roles`, { params })
    },
    async getById(id: string): Promise<ApiResult<RoleSummary>> {
      return apiClient.get(`${IDENTITY}/roles/${id}`)
    },
    async create(data: CreateRoleRequest): Promise<ApiResult<RoleSummary>> {
      return apiClient.post(`${IDENTITY}/roles`, data)
    },
    async update(id: string, data: UpdateRoleRequest): Promise<ApiResult<RoleSummary>> {
      return apiClient.put(`${IDENTITY}/roles/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${IDENTITY}/roles/${id}`)
    },

    // Permissions
    async getPermissions(id: string): Promise<ApiResult<string[]>> {
      return apiClient.get(`${IDENTITY}/roles/${id}/permissions`)
    },
    async assignPermission(id: string, permissionName: string): Promise<ApiResult<void>> {
      return apiClient.put(`${IDENTITY}/roles/${id}/permissions/assign`, { permissionName })
    },
    async revokePermission(id: string, permissionName: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${IDENTITY}/roles/${id}/permissions/revoke`, { data: { permissionName } })
    },
    async syncPermissions(id: string, permissionNames: string[]): Promise<ApiResult<void>> {
      return apiClient.patch(`${IDENTITY}/roles/${id}/permissions/sync`, { permissionNames })
    },
  },

  permissions: {
    async list(params?: ServerQueryingParameters): Promise<ApiResult<PermissionSummary[]>> {
      return apiClient.get(`${IDENTITY}/permissions`, { params })
    },
  },
}
```

**Remove from identity.api.ts**: `updateStaffProfile`, `resetPassword`, `unlockAccount`, `verifyAccount`, `roles.getUsersInRole`, `permissions.getSelect` — no backend routes exist.

- [ ] **Step 3: Run tests**

```bash
npx vitest run app/Admin/src/features/identity/_tests/identity.api.spec.ts
```
Expected: PASS

- [ ] **Step 4: Update user.service.ts to match new identityApi**

```typescript
// app/Admin/src/features/users/services/user.service.ts
import { identityApi } from '../../identity/services/identity.api'

export const userService = identityApi.users
```

Remove: `updateStaffProfile`, `resetPassword`, `unlockAccount`, `verifyAccount` — no backend routes.

- [ ] **Step 5: Update role.service.ts**

```typescript
// app/Admin/src/features/users/services/role.service.ts
import { identityApi } from '../../identity/services/identity.api'

export const roleService = identityApi.roles
```

Remove: `getUsersInRole` — no backend route.

- [ ] **Step 6: Update permission.service.ts**

```typescript
// app/Admin/src/features/users/services/permission.service.ts
import { identityApi } from '../../identity/services/identity.api'

export const permissionService = identityApi.permissions
```

Remove: `getPermissionSelect` — no backend route.

- [ ] **Step 7: Commit**

```bash
git add app/Admin/src/features/identity/ app/Admin/src/features/users/
git commit -m "fix(admin): rewrite identity API service with correct sub-resource routes"
```

---

### Task 4: Rewrite inventory.api.ts

**Files:**
- Rewrite: `app/Admin/src/features/inventories/services/inventory.api.ts`
- Modify: `app/Admin/src/features/inventories/services/inventory.service.ts`
- Create: `app/Admin/src/features/inventories/_tests/inventory.api.spec.ts`

**Interfaces:**
- Consumes: `INVENTORY` from constants (now `'api/inventory'`)
- Produces: corrected `inventoryApi` with backend resource names

**Backend routes:**
- Stock items: `api/inventory/stock-items`, `.../stock-items/{id}`, `.../stock-items/{id}/restock`, `.../stock-items/bulk-adjust`, `.../stock-items/low-stock`, `.../stock-items/summary`
- Stock locations: `api/inventory/stock-locations`, `.../stock-locations/{id}/default`
- Stock transfers: `api/inventory/stock-transfers`, `.../stock-transfers/{id}/transfer`, `.../stock-transfers/{id}/receive`, `.../stock-transfers/{id}/cancel`
- Stock reservations: `api/inventory/stock-reservations`, `.../stock-reservations/{id}/cancel`
- Stock movements: `api/inventory/stock-movements`

- [ ] **Step 1: Write the failing test**

```typescript
// app/Admin/src/features/inventories/_tests/inventory.api.spec.ts
import { describe, it, expect, vi } from 'vitest'
import apiClient from '@/shared/api/http/api.client'
import { inventoryApi } from '../services/inventory.api'

vi.mock('@/shared/api/http/api.client', () => ({
  default: { get: vi.fn(), post: vi.fn(), put: vi.fn(), patch: vi.fn(), delete: vi.fn() }
}))

describe('inventoryApi', () => {
  it('stocks.list calls correct route', async () => {
    await inventoryApi.stocks.list({ page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('api/inventory/stock-items', expect.any(Object))
  })
  it('stocks.restock calls correct route', async () => {
    await inventoryApi.stocks.restock('sid-1', { quantity: 10, type: 0 })
    expect(apiClient.post).toHaveBeenCalledWith('api/inventory/stock-items/sid-1/restock', expect.any(Object))
  })
  it('locations.list calls correct route', async () => {
    await inventoryApi.locations.list({ page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('api/inventory/stock-locations', expect.any(Object))
  })
  it('transfers.transfer calls correct route', async () => {
    await inventoryApi.transfers.transfer('tid-1')
    expect(apiClient.post).toHaveBeenCalledWith('api/inventory/stock-transfers/tid-1/transfer')
  })
  it('transfers.receive calls correct route', async () => {
    await inventoryApi.transfers.receive('tid-1')
    expect(apiClient.post).toHaveBeenCalledWith('api/inventory/stock-transfers/tid-1/receive')
  })
})
```

Run: `npx vitest run app/Admin/src/features/inventories/_tests/inventory.api.spec.ts`
Expected: FAIL

- [ ] **Step 2: Rewrite inventory.api.ts**

```typescript
// app/Admin/src/features/inventories/services/inventory.api.ts
import apiClient from '@/shared/api/http/api.client'
import { INVENTORY } from '@/shared/api/constants'
import type { ApiResult } from '@/shared/api/types/api.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'
import type { StockItem, StockItemDetail, StockLocation, StockLocationDetail, InventoryUnit, StockMovement, StockTransfer, StockTransferDetail, StockAdjustmentRequest, StockAuditRequest, CreateStockLocationRequest, CreateStockTransferRequest, InventorySearchParams } from '../types/inventory.types'

export const inventoryApi = {
  stocks: {
    async list(params: InventorySearchParams): Promise<ApiResult<StockItem[]>> {
      return apiClient.get(`${INVENTORY}/stock-items`, { params })
    },
    async getById(id: string): Promise<ApiResult<StockItemDetail>> {
      return apiClient.get(`${INVENTORY}/stock-items/${id}`)
    },
    async create(data: { variantId: string; stockLocationId: string; countOnHand?: number }): Promise<ApiResult<StockItemDetail>> {
      return apiClient.post(`${INVENTORY}/stock-items`, data)
    },
    async update(id: string, data: { countOnHand?: number; backorderable?: boolean; backorderLimit?: number }): Promise<ApiResult<void>> {
      return apiClient.put(`${INVENTORY}/stock-items/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${INVENTORY}/stock-items/${id}`)
    },
    async restock(id: string, data: StockAdjustmentRequest): Promise<ApiResult<void>> {
      return apiClient.post(`${INVENTORY}/stock-items/${id}/restock`, data)
    },
    async getLowStock(params?: ServerQueryingParameters): Promise<ApiResult<StockItem[]>> {
      return apiClient.get(`${INVENTORY}/stock-items/low-stock`, { params })
    },
    async getSummary(): Promise<ApiResult<any>> {
      return apiClient.get(`${INVENTORY}/stock-items/summary`)
    },
    async bulkAdjust(data: { items: Array<{ id: string; quantity: number; type: number }> }): Promise<ApiResult<void>> {
      return apiClient.post(`${INVENTORY}/stock-items/bulk-adjust`, data)
    },
  },

  locations: {
    async list(params: ServerQueryingParameters): Promise<ApiResult<StockLocation[]>> {
      return apiClient.get(`${INVENTORY}/stock-locations`, { params })
    },
    async getById(id: string): Promise<ApiResult<StockLocationDetail>> {
      return apiClient.get(`${INVENTORY}/stock-locations/${id}`)
    },
    async create(data: CreateStockLocationRequest): Promise<ApiResult<StockLocationDetail>> {
      return apiClient.post(`${INVENTORY}/stock-locations`, data)
    },
    async update(id: string, data: Partial<CreateStockLocationRequest>): Promise<ApiResult<StockLocationDetail>> {
      return apiClient.put(`${INVENTORY}/stock-locations/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${INVENTORY}/stock-locations/${id}`)
    },
    async setDefault(id: string): Promise<ApiResult<void>> {
      return apiClient.put(`${INVENTORY}/stock-locations/${id}/default`)
    },
  },

  reservations: {
    async list(params: ServerQueryingParameters): Promise<ApiResult<InventoryUnit[]>> {
      return apiClient.get(`${INVENTORY}/stock-reservations`, { params })
    },
    async getById(id: string): Promise<ApiResult<InventoryUnit>> {
      return apiClient.get(`${INVENTORY}/stock-reservations/${id}`)
    },
    async cancel(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`${INVENTORY}/stock-reservations/${id}/cancel`)
    },
  },

  transfers: {
    async list(params: ServerQueryingParameters): Promise<ApiResult<StockTransfer[]>> {
      return apiClient.get(`${INVENTORY}/stock-transfers`, { params })
    },
    async getById(id: string): Promise<ApiResult<StockTransferDetail>> {
      return apiClient.get(`${INVENTORY}/stock-transfers/${id}`)
    },
    async create(data: CreateStockTransferRequest): Promise<ApiResult<StockTransferDetail>> {
      return apiClient.post(`${INVENTORY}/stock-transfers`, data)
    },
    async transfer(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`${INVENTORY}/stock-transfers/${id}/transfer`)
    },
    async receive(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`${INVENTORY}/stock-transfers/${id}/receive`)
    },
    async cancel(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`${INVENTORY}/stock-transfers/${id}/cancel`)
    },
  },

  movements: {
    async list(params: ServerQueryingParameters): Promise<ApiResult<StockMovement[]>> {
      return apiClient.get(`${INVENTORY}/stock-movements`, { params })
    },
    async getById(id: string): Promise<ApiResult<StockMovement>> {
      return apiClient.get(`${INVENTORY}/stock-movements/${id}`)
    },
  },
}
```

**Removed from inventory API**: `audit`, `updateBackorderPolicy`, `units.updateSerialNumber`, `units.markDamaged`, `units.restore`, `locations.getTree`, `locations.toggleStatus`, `transfers.addItem` — no backend routes exist for these.

- [ ] **Step 3: Update inventory.service.ts**

```typescript
// app/Admin/src/features/inventories/services/inventory.service.ts
import { inventoryApi } from './inventory.api'

export const inventoryService = {
  listStocks: inventoryApi.stocks.list,
  getStockDetail: inventoryApi.stocks.getById,
  createStock: inventoryApi.stocks.create,
  restock: inventoryApi.stocks.restock,
  deleteStock: inventoryApi.stocks.delete,
  getLowStock: inventoryApi.stocks.getLowStock,
  getStockSummary: inventoryApi.stocks.getSummary,
  bulkAdjust: inventoryApi.stocks.bulkAdjust,
  listLocations: inventoryApi.locations.list,
  getLocationDetail: inventoryApi.locations.getById,
  createLocation: inventoryApi.locations.create,
  updateLocation: inventoryApi.locations.update,
  deleteLocation: inventoryApi.locations.delete,
  setDefaultLocation: inventoryApi.locations.setDefault,
  listReservations: inventoryApi.reservations.list,
  getReservationDetail: inventoryApi.reservations.getById,
  cancelReservation: inventoryApi.reservations.cancel,
  listTransfers: inventoryApi.transfers.list,
  getTransferDetail: inventoryApi.transfers.getById,
  createTransfer: inventoryApi.transfers.create,
  transferStock: inventoryApi.transfers.transfer,
  receiveTransfer: inventoryApi.transfers.receive,
  cancelTransfer: inventoryApi.transfers.cancel,
  listMovements: inventoryApi.movements.list,
  getMovementDetail: inventoryApi.movements.getById,
}
```

- [ ] **Step 4: Update inventory.store.ts to use updated service**

In `inventory.store.ts`, replace:
- `inventoryService.listInventoryUnits` → `inventoryService.listReservations`
- `inventoryService.getInventoryUnitDetail` → `inventoryService.getReservationDetail`
- Remove calls to: `inventoryService.updateInventoryUnitSerialNumber`, `inventoryService.markInventoryUnitDamaged`, `inventoryService.restoreInventoryUnit`, `inventoryService.toggleLocationStatus`, `inventoryService.getLocationTree`

- [ ] **Step 5: Run tests**

```bash
npx vitest run app/Admin/src/features/inventories/_tests/inventory.api.spec.ts
```
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/features/inventories/
git commit -m "fix(admin): rewrite inventory API service with correct resource names (stock-items, stock-locations, stock-transfers)"
```

---

### Task 5: Rewrite ordering.api.ts

**Files:**
- Rewrite: `app/Admin/src/features/ordering/services/ordering.api.ts`
- Modify: `app/Admin/src/features/ordering/services/order.service.ts`
- Modify: `app/Admin/src/features/ordering/stores/order.store.ts`
- Modify: `app/Admin/src/features/ordering/fulfillment/services/fulfillment.service.ts`
- Create: `app/Admin/src/features/ordering/_tests/ordering.api.spec.ts`

**Interfaces:**
- Consumes: `ORDERS` from constants (now `'api/ordering'`)
- Produces: corrected `orderingApi` with backend route names

**Backend routes:**
- `GET/POST api/ordering/orders`, `GET/PUT/DELETE api/ordering/orders/{id}`
- `POST api/ordering/orders/{id}/cancel`, `POST api/ordering/orders/{id}/complete`, `POST .../approve`, `POST .../resume`
- `PUT api/ordering/orders/{id}/status`, `PUT .../ship-address`, `PUT .../bill-address`, `PUT .../shipping-method`
- `GET/POST api/ordering/orders/{id}/line-items`, `GET/PUT/DELETE .../line-items/{lineItemId}`

- [ ] **Step 1: Write the failing test**

```typescript
// app/Admin/src/features/ordering/_tests/ordering.api.spec.ts
import { describe, it, expect, vi } from 'vitest'
import apiClient from '@/shared/api/http/api.client'
import { orderingApi } from '../services/ordering.api'

vi.mock('@/shared/api/http/api.client', () => ({
  default: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() }
}))

describe('orderingApi', () => {
  it('list calls correct route', async () => {
    await orderingApi.orders.list({ page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('api/ordering/orders', expect.any(Object))
  })
  it('addItem calls correct route', async () => {
    await orderingApi.orders.addItem('ord-1', { variantId: 'v-1', quantity: 2 })
    expect(apiClient.post).toHaveBeenCalledWith('api/ordering/orders/ord-1/line-items', { variantId: 'v-1', quantity: 2 })
  })
  it('cancel calls correct route', async () => {
    await orderingApi.orders.cancel('ord-1', 'out of stock')
    expect(apiClient.post).toHaveBeenCalledWith('api/ordering/orders/ord-1/cancel', { reason: 'out of stock' })
  })
})
```

- [ ] **Step 2: Rewrite ordering.api.ts**

```typescript
// app/Admin/src/features/ordering/services/ordering.api.ts
import apiClient from '@/shared/api/http/api.client'
import { ORDERS } from '@/shared/api/constants'
import type { ApiResult } from '@/shared/api/types/api.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'
import type { OrderListItem, OrderDetail, CreateOrderRequest, AddOrderItemRequest, UpdateAddressesRequest, CancelOrderRequest, CreateShipmentRequest, RefundPaymentRequest } from '../types/order.types'

export const orderingApi = {
  orders: {
    async list(params?: ServerQueryingParameters): Promise<ApiResult<OrderListItem[]>> {
      return apiClient.get(`${ORDERS}/orders`, { params })
    },
    async getById(id: string): Promise<ApiResult<OrderDetail>> {
      return apiClient.get(`${ORDERS}/orders/${id}`)
    },
    async create(data: CreateOrderRequest): Promise<ApiResult<OrderDetail>> {
      return apiClient.post(`${ORDERS}/orders`, data)
    },
    async update(id: string, data: Partial<CreateOrderRequest>): Promise<ApiResult<OrderDetail>> {
      return apiClient.put(`${ORDERS}/orders/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${ORDERS}/orders/${id}`)
    },
    // State transitions
    async updateStatus(id: string, status: string): Promise<ApiResult<void>> {
      return apiClient.put(`${ORDERS}/orders/${id}/status`, { status })
    },
    async cancel(id: string, reason?: string): Promise<ApiResult<void>> {
      return apiClient.post(`${ORDERS}/orders/${id}/cancel`, { reason } as CancelOrderRequest)
    },
    async complete(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`${ORDERS}/orders/${id}/complete`)
    },
    async approve(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`${ORDERS}/orders/${id}/approve`)
    },
    async resume(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`${ORDERS}/orders/${id}/resume`)
    },
    // Addresses
    async updateShipAddress(id: string, address: Record<string, unknown>): Promise<ApiResult<void>> {
      return apiClient.put(`${ORDERS}/orders/${id}/ship-address`, address)
    },
    async updateBillAddress(id: string, address: Record<string, unknown>): Promise<ApiResult<void>> {
      return apiClient.put(`${ORDERS}/orders/${id}/bill-address`, address)
    },
    async updateShippingMethod(id: string, shippingMethodId: string): Promise<ApiResult<void>> {
      return apiClient.put(`${ORDERS}/orders/${id}/shipping-method`, { shippingMethodId })
    },
    // Line items
    async listLineItems(id: string): Promise<ApiResult<any[]>> {
      return apiClient.get(`${ORDERS}/orders/${id}/line-items`)
    },
    async addLineItem(id: string, data: AddOrderItemRequest): Promise<ApiResult<void>> {
      return apiClient.post(`${ORDERS}/orders/${id}/line-items`, data)
    },
    async updateLineItem(id: string, lineItemId: string, data: { quantity?: number }): Promise<ApiResult<void>> {
      return apiClient.put(`${ORDERS}/orders/${id}/line-items/${lineItemId}`, data)
    },
    async removeLineItem(id: string, lineItemId: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${ORDERS}/orders/${id}/line-items/${lineItemId}`)
    },
  },

  fulfillments: {
    async getQueue(params?: ServerQueryingParameters): Promise<ApiResult<OrderListItem[]>> {
      return apiClient.get(`${ORDERS}/orders`, { params: { ...params, state: 'Processing' } })
    },
  },
}
```

**Removed from ordering API**: `createShipment`, `cancelShipment`, `refundPayment`, `updateState` (was POST `/advance`) — no backend routes. Refund belongs to Payment module.

- [ ] **Step 3: Update order.service.ts**

```typescript
// app/Admin/src/features/ordering/services/order.service.ts
import { orderingApi } from './ordering.api'

export const orderService = {
  list: orderingApi.orders.list,
  getById: orderingApi.orders.getById,
  create: orderingApi.orders.create,
  update: orderingApi.orders.update,
  delete: orderingApi.orders.delete,
  cancel: orderingApi.orders.cancel,
  complete: orderingApi.orders.complete,
  approve: orderingApi.orders.approve,
  addItem: orderingApi.orders.addLineItem,
  removeItem: orderingApi.orders.removeLineItem,
  updateStatus: orderingApi.orders.updateStatus,
  updateShipAddress: orderingApi.orders.updateShipAddress,
  updateBillAddress: orderingApi.orders.updateBillAddress,
}
```

**Removed**: `createShipment`, `cancelShipment`, `updateAddresses` (split into ship + bill), `updateState` (no backend), `refundPayment`.

- [ ] **Step 4: Update order.store.ts**

In `order.store.ts`:
- Replace `orderService.addItem` calls → `orderService.addItem` (still works, same name)
- Replace `orderService.updateAddresses(id, data)` → two calls: `orderService.updateShipAddress(id, data.shippingAddress)` and `orderService.updateBillAddress(id, data.billingAddress)`
- Replace `orderService.updateState(id)` → `orderService.updateStatus(id, 'next_state')` — caller must pass desired state
- Replace `orderService.cancelShipment(orderId, shipmentId)` → remove, no backend
- Replace `orderService.refundPayment(...)` → remove, no payment feature in frontend

- [ ] **Step 5: Update fulfillment.service.ts**

```typescript
// app/Admin/src/features/ordering/fulfillment/services/fulfillment.service.ts
import { orderingApi } from '../../services/ordering.api'

export const fulfillmentService = {
  getQueue: orderingApi.fulfillments.getQueue,
  markAsShipped: orderingApi.orders.complete,
}
```

- [ ] **Step 6: Run tests**

```bash
npx vitest run app/Admin/src/features/ordering/_tests/ordering.api.spec.ts
```
Expected: PASS

- [ ] **Step 7: Commit**

```bash
git add app/Admin/src/features/ordering/
git commit -m "fix(admin): rewrite ordering API service with correct line-items and state transition routes"
```

---

### Task 6: Fix location constant + profile service

**Files:**
- Modify: `app/Admin/src/features/location/services/location.api.ts`

**Backend routes:** `api/locations/countries`, `api/locations/states`

- [ ] **Step 1: Update location.api.ts — rename constant import**

```typescript
// app/Admin/src/features/location/services/location.api.ts
import apiClient from '@/shared/api/http/api.client'
import { LOCATIONS } from '@/shared/api/constants'
import type { ApiResult } from '@/shared/api/types/api.types'
import type { Country, CountryCreateRequest, CountryUpdateRequest } from '../types/country.types'
import type { State, StateCreateRequest, StateUpdateRequest } from '../types/state.types'

export const locationApi = {
  countries: {
    async list(params?: any): Promise<ApiResult<Country[]>> { return apiClient.get(`${LOCATIONS}/countries`, { params }) },
    async getById(id: string): Promise<ApiResult<Country>> { return apiClient.get(`${LOCATIONS}/countries/${id}`) },
    async create(data: CountryCreateRequest): Promise<ApiResult<Country>> { return apiClient.post(`${LOCATIONS}/countries`, data) },
    async update(id: string, data: CountryUpdateRequest): Promise<ApiResult<Country>> { return apiClient.put(`${LOCATIONS}/countries/${id}`, data) },
    async delete(id: string): Promise<ApiResult<void>> { return apiClient.delete(`${LOCATIONS}/countries/${id}`) },
  },
  states: {
    async list(params?: any): Promise<ApiResult<State[]>> { return apiClient.get(`${LOCATIONS}/states`, { params }) },
    async getById(id: string): Promise<ApiResult<State>> { return apiClient.get(`${LOCATIONS}/states/${id}`) },
    async create(data: StateCreateRequest): Promise<ApiResult<State>> { return apiClient.post(`${LOCATIONS}/states`, data) },
    async update(id: string, data: StateUpdateRequest): Promise<ApiResult<State>> { return apiClient.put(`${LOCATIONS}/states/${id}`, data) },
    async delete(id: string): Promise<ApiResult<void>> { return apiClient.delete(`${LOCATIONS}/states/${id}`) },
  },
}
```

- [ ] **Step 2: Update profile.api.ts — fix base path**

Profile backend routes are at `api/profiles/profiles` (note: double `profiles`). Change:

```typescript
// app/Admin/src/features/profile/services/profile.api.ts
import apiClient from '@/shared/api/http/api.client'
import { PROFILES } from '@/shared/api/constants'
import type { ApiResult } from '@/shared/api/types/api.types'
import type { Profile, ProfileUpdateRequest } from '../types/profile.types'

export const profileApi = {
  async get(): Promise<ApiResult<Profile>> {
    return apiClient.get(`${PROFILES}/profiles`)
  },
  async update(data: ProfileUpdateRequest): Promise<ApiResult<Profile>> {
    return apiClient.put(`${PROFILES}/profiles`, data)
  },
}
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/location/services/location.api.ts app/Admin/src/features/profile/services/profile.api.ts
git commit -m "fix(admin): correct location constant import and profile base path"
```

---

### Task 7: Fix auth service + refresh handler

**Files:**
- Modify: `app/Admin/src/features/auth/services/auth.service.ts`
- Modify: `app/Admin/src/features/auth/stores/auth.store.ts`
- Modify: `app/Admin/src/shared/api/http/refresh-handler.ts`

**Context:** Backend has no admin auth endpoints. Auth is storefront-only at `api/store/identity/auth/*`. These changes either:
- (a) Point admin auth at storefront endpoints (works but not ideal), or
- (b) Document the gap and stub out for future admin auth.

Option (b) chosen — the admin auth feature needs backend work outside this plan's scope. For now, keep existing route structure but update the refresh handler to the correct storefront path so token refresh works while the full auth solution is designed.

- [ ] **Step 1: Update refresh-handler.ts — point at known storefront endpoint**

```typescript
// app/Admin/src/shared/api/http/refresh-handler.ts
// Line 16: change URL
const refreshResponse = await axios.post('/api/store/identity/auth/sessions/refresh', {
  refreshToken: token,
})
```

- [ ] **Step 2: Document gap in auth.service.ts**

```typescript
// app/Admin/src/features/auth/services/auth.service.ts
// NOTE: Backend has no admin auth endpoints yet.
// Admin auth uses storefront identity routes as a temporary bridge.
// Full admin auth endpoints should be added to the Identity module.
// See: spec/spec-design-admin-api-services.md §4.1
```

Leave the existing `/auth/*` paths as-is for now. They will all 401 until the Identity module gets admin auth endpoints. This is a known gap, not a regression.

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/shared/api/http/refresh-handler.ts app/Admin/src/features/auth/services/auth.service.ts
git commit -m "fix(admin): point refresh handler at storefront auth endpoint; document auth gap"
```

---

### Task 8: Remove dead dashboard/report calls

**Files:**
- Modify: `app/Admin/src/features/reports/services/reports.api.ts`
- Modify: `app/Admin/src/features/reports/stores/report.store.ts`
- Modify: `app/Admin/src/features/catalog/dashboard/services/catalog-dashboard.service.ts`
- Modify: `app/Admin/src/features/catalog/dashboard/stores/catalog-dashboard.store.ts`

**Context:** All 4 report endpoints and the catalog dashboard summary call hit no backend routes. Remove them. The dashboard page UI reads JWT claims only — no service dependency.

- [ ] **Step 1: Remove dead report endpoints**

```typescript
// app/Admin/src/features/reports/services/reports.api.ts
// All endpoints removed — no backend routes exist.
// Re-add when Dashboard module endpoints are added to backend.
export const reportsApi = {}
```

- [ ] **Step 2: Remove report.store.ts data fetch**

```typescript
// app/Admin/src/features/reports/stores/report.store.ts
// Remove fetchDashboardData entirely. Store becomes empty.
// Re-add when backend has dashboard endpoints.
```

- [ ] **Step 3: Remove catalog dashboard service**

```typescript
// app/Admin/src/features/catalog/dashboard/services/catalog-dashboard.service.ts
// Removed — no backend endpoint.
// Re-add when backend adds GET api/catalog/dashboard/summary
```

- [ ] **Step 4: Clear catalog-dashboard store**

```typescript
// app/Admin/src/features/catalog/dashboard/stores/catalog-dashboard.store.ts
// Remove fetchSummary. Store becomes stub.
```

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/reports/services/reports.api.ts app/Admin/src/features/reports/stores/report.store.ts app/Admin/src/features/catalog/dashboard/
git commit -m "fix(admin): remove dead dashboard/report API calls with no backend routes"
```

---

### Task 9: Run full test suite + lint

- [ ] **Step 1: Run all new tests**

```bash
npx vitest run app/Admin/src/features/catalog/_tests/ app/Admin/src/features/identity/_tests/ app/Admin/src/features/inventories/_tests/ app/Admin/src/features/ordering/_tests/ app/Admin/src/features/shared/_tests/
```
Expected: ALL PASS

- [ ] **Step 2: Build check**

```bash
npx vue-tsc --build
```
Expected: no type errors from changed files

- [ ] **Step 3: Lint**

```bash
npm run lint
```
Expected: clean

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "chore(admin): full test suite pass after API service corrections"
```
