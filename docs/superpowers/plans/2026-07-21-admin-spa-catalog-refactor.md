# Catalog Module SPA Refactor — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Refactor all 11 Catalog module entities in Admin SPA to match backend patterns: per-field Zod validation, models/types split, explicit API mappers, flattened entity folders, and route extraction.

**Architecture:** Each entity gets `types/` (per-field Zod schemas + query types), `models/` (parameter/request/response interfaces), `api/` (repository + mapper), `store/` (Pinia with mapper integration), `pages/`, `components/`, `routes.ts`, `index.ts`. Child entities move from nested folders to catalog level.

**Tech Stack:** Vue 3, TypeScript 6, Pinia, Zod, Vitest, pnpm

## Global Constraints

- Backend API endpoints and response shapes unchanged
- Existing test assertions stay green — only import paths update
- Pinia store IDs unchanged (`'option-type'`, `'option-value'`, etc.)
- i18n keys preserved as-is in field schemas
- No `as Type` casts in store → mapper → repository chain
- `types/` contains only Zod schemas and query types
- `models/` contains only data interfaces (no Zod)
- Every entity gets `routes.ts`; `catalog.routes.ts` aggregates them

---

### Task 1: Refactor option-types — per-field schemas, models, mapper, routes

**Files:**
- Create: `app/Admin/src/features/catalog/option-types/models/option-type.parameters.ts`
- Create: `app/Admin/src/features/catalog/option-types/models/option-type.request.ts`
- Create: `app/Admin/src/features/catalog/option-types/models/option-type.response.ts`
- Create: `app/Admin/src/features/catalog/option-types/api/option-type.mapper.ts`
- Create: `app/Admin/src/features/catalog/option-types/routes.ts`
- Modify: `app/Admin/src/features/catalog/option-types/types/option-type.field.ts`
- Modify: `app/Admin/src/features/catalog/option-types/types/option-type.parameters.ts`
- Modify: `app/Admin/src/features/catalog/option-types/types/option-type.request.ts`
- Modify: `app/Admin/src/features/catalog/option-types/api/option-type.api.ts`
- Modify: `app/Admin/src/features/catalog/option-types/store/option-type.store.ts`
- Modify: `app/Admin/src/features/catalog/option-types/index.ts`
- Delete: `app/Admin/src/features/catalog/option-types/types/option-type.response.ts`
- Modify: `app/Admin/src/features/catalog/option-types/__tests__/option-type.schema.spec.ts`
- Modify: `app/Admin/src/features/catalog/option-types/__tests__/option-type.store.spec.ts`

**Interfaces:**
- Produces: `nameSchema`, `presentationSchema`, `positionSchema`, `filterableSchema`, `createOptionTypeSchema`, `OptionTypeParameters`, `CreateOptionTypeRequest`, `UpdateOptionTypeRequest`, `OptionTypeListItem`, `OptionTypeDetail`, `mapToListItem`, `mapToDetail`, `useOptionTypeStore`, `optionTypeRoutes`

- [ ] **Step 1: Rewrite `types/option-type.field.ts` — per-field exported schemas**

Replace the current monolithic schema with independently exported per-field schemas:

```typescript
import { z } from 'zod'

export function nameSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string()
    .min(1, t('catalog.validation.name.required'))
    .max(100, t('catalog.validation.name.max_length'))
}

export function presentationSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string()
    .min(1, t('catalog.validation.presentation.required'))
    .max(100, t('catalog.validation.presentation.max_length'))
}

export function positionSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number()
    .int(t('catalog.validation.position.whole'))
    .min(0, t('catalog.validation.position.min'))
    .default(0)
}

export function filterableSchema() {
  return z.boolean().default(false)
}

export function descriptionSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string()
    .max(500, t('catalog.validation.description.max_length'))
    .optional()
    .nullable()
}

export function createOptionTypeSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
    name: nameSchema(t),
    presentation: presentationSchema(t),
    description: descriptionSchema(t),
    filterable: filterableSchema(),
    position: positionSchema(t),
  })
}
```

- [ ] **Step 2: Create `models/option-type.parameters.ts`**

```typescript
export interface OptionTypeParameters {
  name: string
  presentation: string
  position: number
  filterable: boolean
}
```

- [ ] **Step 3: Create `models/option-type.request.ts`**

```typescript
import type { OptionTypeParameters } from './option-type.parameters'

export type CreateOptionTypeRequest = OptionTypeParameters
export type UpdateOptionTypeRequest = OptionTypeParameters
```

- [ ] **Step 4: Create `models/option-type.response.ts`**

```typescript
export interface OptionTypeListItem {
  id: string
  name: string
  presentation: string
  position: number
  filterable: boolean
  optionValuesCount: number
  productsCount: number
  createdAtUtc: string
  modifiedAtUtc: string
}

export type OptionTypeDetail = OptionTypeListItem
```

- [ ] **Step 5: Delete old `types/option-type.response.ts`**

```bash
rm app/Admin/src/features/catalog/option-types/types/option-type.response.ts
```

- [ ] **Step 6: Update `types/option-type.parameters.ts` — re-export from models**

```typescript
import type { OptionTypeParameters } from '../models/option-type.parameters'
export type { OptionTypeParameters }
```

- [ ] **Step 7: Update `types/option-type.request.ts` — re-export from models**

```typescript
import type { CreateOptionTypeRequest, UpdateOptionTypeRequest } from '../models/option-type.request'
export type { CreateOptionTypeRequest, UpdateOptionTypeRequest }
```

- [ ] **Step 8: Create `api/option-type.mapper.ts` — explicit mapping**

```typescript
import type { OptionTypeListItem, OptionTypeDetail } from '../models/option-type.response'

export function mapToListItem(dto: Record<string, unknown>): OptionTypeListItem {
  return {
    id: String(dto.id ?? ''),
    name: String(dto.name ?? ''),
    presentation: String(dto.presentation ?? ''),
    position: Number(dto.position ?? 0),
    filterable: Boolean(dto.filterable),
    optionValuesCount: Number(dto.optionValuesCount ?? 0),
    productsCount: Number(dto.productsCount ?? 0),
    createdAtUtc: String(dto.createdAtUtc ?? ''),
    modifiedAtUtc: String(dto.modifiedAtUtc ?? ''),
  }
}

export function mapToDetail(dto: Record<string, unknown>): OptionTypeDetail {
  return mapToListItem(dto)
}
```

- [ ] **Step 9: Update `api/option-type.api.ts` — fix import path**

Change line 10 from:
```typescript
import type { OptionTypeListItem } from "../../products/option-types/types/product-option-type.response";
```
To:
```typescript
import type { OptionTypeListItem, OptionTypeDetail } from '../models/option-type.response'
```

Also add the `OptionTypeDetail` import on the same line.

- [ ] **Step 10: Update `store/option-type.store.ts` — wire mapper, fix imports**

```typescript
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { usePagedList } from '@/common/composables/paged-list.use'
import { optionTypeRepository } from '../api/option-type.api'
import { mapToListItem, mapToDetail } from '../api/option-type.mapper'
import type { OptionTypeListItem, OptionTypeDetail } from '../models/option-type.response'
import type { CreateOptionTypeRequest, UpdateOptionTypeRequest } from '../models/option-type.request'
import type { OptionTypeQuery } from '../types/option-type.query'
import type { ServerResult } from '@/common/api/types/result.types'

export const useOptionTypeStore = defineStore('option-type', () => {
  const currentItem = ref<OptionTypeDetail | null>(null)

  const { items, loading, totalRecords, params, fetch: fetchList } = usePagedList<OptionTypeListItem, OptionTypeQuery>(
    async (p) => {
      const result = await optionTypeRepository.list(p)
      return { ...result, items: result.items?.map(mapToListItem) ?? [] }
    },
    { sort: ['position'] },
  )

  async function fetchById(id: string) {
    loading.value = true
    const result = await optionTypeRepository.getById(id)
    if (result.isSuccess && result.value) {
      currentItem.value = mapToDetail(result.value)
    }
    loading.value = false
    return result
  }

  async function create(request: CreateOptionTypeRequest): Promise<ServerResult<OptionTypeDetail>> {
    loading.value = true
    const result = await optionTypeRepository.create(request)
    if (result.isSuccess && result.value) {
      currentItem.value = mapToDetail(result.value)
    }
    loading.value = false
    return { ...result, value: currentItem.value }
  }

  async function update(id: string, request: UpdateOptionTypeRequest): Promise<ServerResult<OptionTypeDetail>> {
    loading.value = true
    const result = await optionTypeRepository.update(id, request)
    if (result.isSuccess && result.value) {
      currentItem.value = mapToDetail(result.value)
    }
    loading.value = false
    return { ...result, value: currentItem.value }
  }

  async function remove(id: string): Promise<ServerResult<void>> {
    loading.value = true
    const result = await optionTypeRepository.delete(id)
    if (result.isSuccess) {
      items.value = items.value.filter(i => i.id !== id)
      totalRecords.value--
    }
    loading.value = false
    return result
  }

  function clearCurrent() {
    currentItem.value = null
  }

  return { items, currentItem, loading, totalRecords, params, fetchList, fetchById, create, update, remove, clearCurrent }
})
```

- [ ] **Step 11: Create `routes.ts`**

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const optionTypeRoutes: RouteRecordRaw[] = [
  {
    path: 'option-types',
    component: () => import('./pages/OptionTypeManagerPage.vue'),
    children: [
      {
        path: '',
        name: 'catalog.option-types.list',
        component: () => import('@/shared/components/feedback/ManagerWelcome.vue'),
        props: {
          title: 'Option Type Manager',
          description: 'Select an option type from the left to edit its configuration and values, or create a new one to add more product attributes.',
          icon: 'pi-list',
        },
      },
      {
        path: 'create',
        name: 'catalog.option-types.create',
        component: () => import('./pages/OptionTypeFormPage.vue'),
      },
      {
        path: ':id/edit',
        name: 'catalog.option-types.edit',
        component: () => import('./pages/OptionTypeFormPage.vue'),
      },
    ],
  },
]
```

- [ ] **Step 12: Update `index.ts` — barrel exports**

```typescript
export { optionTypeRoutes } from './routes'
export { optionTypeRepository } from './api/option-type.api'
export { useOptionTypeStore } from './store/option-type.store'
export * from './types/option-type.field'
export * from './types/option-type.query'
export * from './models/option-type.parameters'
export * from './models/option-type.request'
export * from './models/option-type.response'
```

- [ ] **Step 13: Update `__tests__/option-type.schema.spec.ts` — fix import path**

Change:
```typescript
import { createOptionTypeSchema } from '../types/option-type.field'
```
The import stays the same since the file path is unchanged. Update any test to import individual field schemas if needed.

- [ ] **Step 14: Update `__tests__/option-type.store.spec.ts` — fix import paths**

Change all imports from `../types/option-type.response` to `../models/option-type.response`.

Find: `import type { OptionTypeListItem } from '../types/option-type.response'`
Replace with: `import type { OptionTypeListItem } from '../models/option-type.response'`

- [ ] **Step 15: Commit**

```bash
git add app/Admin/src/features/catalog/option-types/
git commit -m "refactor(admin): add per-field schemas, models, mapper, routes to option-types"
```

---

### Task 2: Flatten option-values + full pattern

**Files:**
- Create: `app/Admin/src/features/catalog/option-values/` (entire new directory)
- Delete: `app/Admin/src/features/catalog/option-types/option-values/`

**Interfaces:**
- Consumes: `nameSchema`, `presentationSchema`, `positionSchema` from option-types (re-defined independently with different i18n keys)
- Produces: `optionValueRoutes`, `useOptionValueStore`, `optionValueRepository`

- [ ] **Step 1: Create directory structure**

```bash
mkdir -p app/Admin/src/features/catalog/option-values/{api,store,types,models,pages,components,composables,__tests__}
```

- [ ] **Step 2: Copy all files from old location to new**

```bash
cp -r app/Admin/src/features/catalog/option-types/option-values/* app/Admin/src/features/catalog/option-values/
rm app/Admin/src/features/catalog/option-values/types/option-value.parameters.ts
```

- [ ] **Step 3: Create `types/option-value.field.ts` — per-field schemas**

```typescript
import { z } from 'zod'

export function nameSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string()
    .min(1, t('catalog.validation.internal_name.required'))
    .max(100, t('catalog.validation.name.max_length'))
}

export function presentationSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string()
    .min(1, t('catalog.validation.display_name.required'))
    .max(100, t('catalog.validation.display_name.max_length'))
}

export function positionSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number()
    .int(t('catalog.validation.position.whole'))
    .min(0, t('catalog.validation.position.min'))
    .default(0)
}

export function createOptionValueSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
    name: nameSchema(t),
    presentation: presentationSchema(t),
    position: positionSchema(t),
  })
}
```

- [ ] **Step 4: Create `models/option-value.parameters.ts`**

```typescript
export interface OptionValueParameters {
  name: string
  presentation: string
  position: number
}
```

- [ ] **Step 5: Create `models/option-value.request.ts`**

```typescript
import type { OptionValueParameters } from './option-value.parameters'

export type CreateOptionValueRequest = OptionValueParameters & { optionTypeId: string }
export type UpdateOptionValueRequest = OptionValueParameters & { optionTypeId?: string }

export interface UpdateOptionValuePositionsRequest {
  optionTypeId: string
  positions: { id: string; position: number }[]
}
```

- [ ] **Step 6: Create `models/option-value.response.ts`**

```typescript
export interface OptionValueListItem {
  id: string
  optionTypeId: string
  name: string
  presentation: string
  position: number
}
```

- [ ] **Step 7: Delete old types files being replaced**

```bash
rm app/Admin/src/features/catalog/option-values/types/option-value.parameters.ts
rm app/Admin/src/features/catalog/option-values/types/option-value.request.ts
rm app/Admin/src/features/catalog/option-values/types/option-value.response.ts
rm app/Admin/src/features/catalog/option-values/types/option-value.field.ts
```

Note: `types/option-value.query.ts` stays in `types/` (it's a query type, not a model).

- [ ] **Step 8: Create `api/option-value.mapper.ts`**

```typescript
import type { OptionValueListItem } from '../models/option-value.response'

export function mapToListItem(dto: Record<string, unknown>): OptionValueListItem {
  return {
    id: String(dto.id ?? ''),
    optionTypeId: String(dto.optionTypeId ?? ''),
    name: String(dto.name ?? ''),
    presentation: String(dto.presentation ?? ''),
    position: Number(dto.position ?? 0),
  }
}
```

- [ ] **Step 9: Update `api/option-value.api.ts` — fix import paths**

Replace all `../types/` imports with `../models/` for response and request types:

Change lines 5-8 from:
```typescript
import type { OptionValueListItem } from "../types/option-value.response";
import type { OptionValueParameters } from "../types/option-value.field";
import type { UpdateOptionValueRequest } from "../types/option-value.request";
import type { OptionValueQuery } from "../types/option-value.query";
```
To:
```typescript
import type { OptionValueListItem } from '../models/option-value.response'
import type { OptionValueParameters } from '../models/option-value.parameters'
import type { UpdateOptionValueRequest } from '../models/option-value.request'
import type { OptionValueQuery } from '../types/option-value.query'
```

- [ ] **Step 10: Update `store/option-value.store.ts` — wire mapper, fix imports**

```typescript
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { optionValueRepository } from '../api/option-value.api'
import { mapToListItem } from '../api/option-value.mapper'
import type { ServerResult } from '@/common/api/types/result.types'
import type { OptionValueListItem } from '../models/option-value.response'
import type { OptionValueQuery } from '../types/option-value.query'
import type { CreateOptionValueRequest, UpdateOptionValueRequest } from '../models/option-value.request'

export const useOptionValueStore = defineStore('option-value', () => {
  const values = ref<OptionValueListItem[]>([])
  const items = ref<OptionValueListItem[]>([])
  const totalRecords = ref(0)
  const query = ref<OptionValueQuery>({ page: 1, pageSize: 10, sort: ['position'] })
  const loading = ref(false)

  async function fetchValues(optionTypeId: string, queryParams?: Partial<OptionValueQuery>) {
    loading.value = true
    const result = await optionValueRepository.list({ ...queryParams, optionTypeId } as OptionValueQuery)
    if (result.isSuccess) {
      values.value = result.items?.map(mapToListItem) ?? []
      values.value.sort((a, b) => a.position - b.position)
    }
    loading.value = false
    return result
  }

  async function fetchList(params?: Partial<OptionValueQuery>) {
    loading.value = true
    if (params) query.value = { ...query.value, ...params }
    const result = await optionValueRepository.list(query.value)
    if (result.isSuccess) {
      items.value = result.items?.map(mapToListItem) ?? []
      totalRecords.value = result.totalCount || 0
    }
    loading.value = false
    return result
  }

  async function create(optionTypeId: string, payload: Omit<CreateOptionValueRequest, 'optionTypeId'>): Promise<ServerResult<OptionValueListItem>> {
    loading.value = true
    const result = await optionValueRepository.create(optionTypeId, payload)
    if (result.isSuccess && result.value) {
      const item = mapToListItem(result.value)
      values.value.push(item)
      values.value.sort((a, b) => a.position - b.position)
    }
    loading.value = false
    return result
  }

  async function update(id: string, request: UpdateOptionValueRequest): Promise<ServerResult<OptionValueListItem>> {
    loading.value = true
    const optionTypeId = request.optionTypeId || values.value.find(v => v.id === id)?.optionTypeId || ''
    const result = await optionValueRepository.update(optionTypeId, id, request)
    if (result.isSuccess && result.value) {
      const item = mapToListItem(result.value)
      const idx = values.value.findIndex(v => v.id === id)
      if (idx !== -1) values.value[idx] = item
      values.value.sort((a, b) => a.position - b.position)
    }
    loading.value = false
    return result
  }

  async function remove(id: string): Promise<ServerResult<void>> {
    loading.value = true
    const optionTypeId = values.value.find(v => v.id === id)?.optionTypeId || ''
    const result = await optionValueRepository.delete(optionTypeId, id)
    if (result.isSuccess) values.value = values.value.filter(v => v.id !== id)
    loading.value = false
    return result
  }

  async function updatePositions(optionTypeId: string, positions: { id: string; position: number }[]): Promise<ServerResult<void>> {
    loading.value = true
    const result = await optionValueRepository.reorder({ optionTypeId, positions })
    if (result.isSuccess) {
      positions.forEach(p => {
        const val = values.value.find(v => v.id === p.id)
        if (val) val.position = p.position
      })
      values.value.sort((a, b) => a.position - b.position)
    }
    loading.value = false
    return result
  }

  function clearValues() { values.value = [] }

  return { values, items, totalRecords, query, loading, fetchValues, fetchList, create, update, remove, updatePositions, clearValues }
})
```

- [ ] **Step 11: Update existing test files — fix import paths**

In `__tests__/option-value.schema.spec.ts`, update:
```typescript
import { createOptionValueSchema } from '../types/option-value.field'
```
(Path unchanged — file still in `types/`.)

In `__tests__/option-value.store.spec.ts`, update all imports from `../types/option-value.response` to `../models/option-value.response` and `../types/option-value.request` to `../models/option-value.request`.

- [ ] **Step 12: Create `routes.ts`**

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const optionValueRoutes: RouteRecordRaw[] = [
  {
    path: 'option-values',
    name: 'catalog.option-values.list',
    component: () => import('./pages/OptionValueListPage.vue'),
  },
]
```

- [ ] **Step 13: Create `index.ts`**

```typescript
export { optionValueRoutes } from './routes'
export { optionValueRepository } from './api/option-value.api'
export { useOptionValueStore } from './store/option-value.store'
export * from './types/option-value.field'
export * from './types/option-value.query'
export * from './models/option-value.parameters'
export * from './models/option-value.request'
export * from './models/option-value.response'
```

- [ ] **Step 14: Create `.gitkeep` in empty folders**

```bash
touch app/Admin/src/features/catalog/option-values/composables/.gitkeep
```

- [ ] **Step 15: Delete old nested directory**

```bash
rm -rf app/Admin/src/features/catalog/option-types/option-values/
```

- [ ] **Step 16: Commit**

```bash
git add app/Admin/src/features/catalog/option-values/ app/Admin/src/features/catalog/option-types/option-values/
git commit -m "refactor(admin): flatten option-values, add per-field schemas, mapper, models"
```

---

### Task 3: Refactor products — per-field schemas, models, mapper, routes

**Files:**
- Create: `app/Admin/src/features/catalog/products/models/product.parameters.ts`
- Create: `app/Admin/src/features/catalog/products/models/product.request.ts`
- Create: `app/Admin/src/features/catalog/products/models/product.response.ts`
- Create: `app/Admin/src/features/catalog/products/api/product.mapper.ts`
- Create: `app/Admin/src/features/catalog/products/routes.ts`
- Modify: `app/Admin/src/features/catalog/products/types/create-product.field.ts` (per-field)
- Modify: `app/Admin/src/features/catalog/products/types/update-product.field.ts` (per-field)
- Modify: `app/Admin/src/features/catalog/products/types/product.parameters.ts`
- Modify: `app/Admin/src/features/catalog/products/types/product.request.ts`
- Modify: `app/Admin/src/features/catalog/products/api/product.api.ts`
- Modify: `app/Admin/src/features/catalog/products/store/product.store.ts`
- Modify: `app/Admin/src/features/catalog/products/index.ts`
- Delete: `app/Admin/src/features/catalog/products/types/product.response.ts`

**Interfaces:**
- Produces: `productRoutes`, `useProductStore`, `productRepository`, per-field schemas, product models

- [ ] **Step 1: Rewrite `types/create-product.field.ts` — per-field schemas**

Replace current content with per-field exported schemas plus composed schema:

```typescript
import { z } from 'zod'

export function nameSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().min(1, t('catalog.validation.name.required')).max(100)
}

export function descriptionSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().max(2000).optional().nullable()
}

export function slugSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().min(1, t('catalog.validation.slug.required')).max(100)
}

export function metaTitleSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().max(100).optional().nullable()
}

export function metaDescriptionSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().max(255).optional().nullable()
}

export function metaKeywordsSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().max(255).optional().nullable()
}

export function createProductSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
    name: nameSchema(t),
    slug: slugSchema(t),
    description: descriptionSchema(t),
    metaTitle: metaTitleSchema(t),
    metaDescription: metaDescriptionSchema(t),
    metaKeywords: metaKeywordsSchema(t),
  })
}

export type CreateProductParameters = z.infer<ReturnType<typeof createProductSchema>>
```

- [ ] **Step 2: Rewrite `types/update-product.field.ts` — per-field schemas**

```typescript
import { z } from 'zod'

export function statusSchema() {
  return z.number().int().min(0).optional()
}

import { createProductSchema } from './create-product.field'

export function updateProductSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return createProductSchema(t).extend({
    status: statusSchema(),
  })
}

export type UpdateProductParameters = z.infer<ReturnType<typeof updateProductSchema>>
```

Note: This file reuses `createProductSchema` from the sibling file and adds `status`.

- [ ] **Step 3: Create `models/product.parameters.ts`**

```typescript
export interface ProductParameters {
  name: string
  slug: string
  description: string | null
  metaTitle: string | null
  metaDescription: string | null
  metaKeywords: string | null
}
```

- [ ] **Step 4: Create `models/product.request.ts`**

```typescript
import type { ProductParameters } from './product.parameters'

export type CreateProductRequest = ProductParameters
export type UpdateProductRequest = Partial<ProductParameters> & { status?: number }
```

- [ ] **Step 5: Create `models/product.response.ts`**

```typescript
export interface ProductSummary {
  id: string
  name: string
  slug: string
  description: string | null
  masterVariantId: string
  status: number
  availableOn: string | null
  discontinueOn: string | null
  trackInventory: boolean
  variantsCount: number
  createdAtUtc: string
  modifiedAtUtc: string | null
}

export interface ProductDetail extends ProductSummary {
  metaTitle: string | null
  metaDescription: string | null
  metaKeywords: string | null
}
```

- [ ] **Step 6: Delete `types/product.response.ts`**

```bash
rm app/Admin/src/features/catalog/products/types/product.response.ts
```

- [ ] **Step 7: Update `types/product.parameters.ts`**

```typescript
import type { ProductParameters } from '../models/product.parameters'
export type { ProductParameters }
```

- [ ] **Step 8: Update `types/product.request.ts`**

```typescript
import type { CreateProductRequest, UpdateProductRequest } from '../models/product.request'
export type { CreateProductRequest, UpdateProductRequest }
```

Remove the classification import — it should move to the classifications entity:

```typescript
import type { ManageClassificationsParameters } from '../classifications/types/product-classification.field'
export type { ManageClassificationsParameters }
```

- [ ] **Step 9: Create `api/product.mapper.ts`**

```typescript
import type { ProductSummary, ProductDetail } from '../models/product.response'
import { ProductStatusMap } from '@/shared/utils/enums'

export function mapToSummary(dto: Record<string, unknown>): ProductSummary {
  return {
    id: String(dto.id ?? ''),
    name: String(dto.name ?? ''),
    slug: String(dto.slug ?? ''),
    description: dto.description as string | null ?? null,
    masterVariantId: String(dto.masterVariantId ?? ''),
    status: Number(dto.status ?? 0),
    availableOn: dto.availableOn as string | null ?? null,
    discontinueOn: dto.discontinueOn as string | null ?? null,
    trackInventory: Boolean(dto.trackInventory),
    variantsCount: Number(dto.variantsCount ?? 0),
    createdAtUtc: String(dto.createdAtUtc ?? ''),
    modifiedAtUtc: dto.modifiedAtUtc as string | null ?? null,
  }
}

export function mapToDetail(dto: Record<string, unknown>): ProductDetail {
  return {
    ...mapToSummary(dto),
    metaTitle: dto.metaTitle as string | null ?? null,
    metaDescription: dto.metaDescription as string | null ?? null,
    metaKeywords: dto.metaKeywords as string | null ?? null,
  }
}

export function mapToSummaryModel(dto: Record<string, unknown>) {
  const summary = mapToSummary(dto)
  return { ...summary, statusLabel: ProductStatusMap[summary.status] ?? 'Unknown' }
}

export function mapToDetailModel(dto: Record<string, unknown>) {
  const detail = mapToDetail(dto)
  return { ...detail, statusLabel: ProductStatusMap[detail.status] ?? 'Unknown' }
}
```

- [ ] **Step 10: Update `api/product.api.ts` — use mapper, fix imports**

Replace the `mapValue`, `mapItems` transform calls with mapper functions. Change imports at top:

```typescript
import apiClient from '@/common/api/http/api.client'
import { CATALOG } from '@/common/api/constants'
import type { ServerPagedResult, ServerResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type { ProductDetail, ProductSummary } from '../models/product.response'
import type { CreateProductRequest, UpdateProductRequest } from '../models/product.request'
import type { ProductSummaryModel, ProductDetailModel } from '../models/product.model'
import type { ProductImage } from '../types/product-image.response'
import { mapToSummaryModel, mapToDetailModel } from './product.mapper'
```

Then replace the `mapItems(result, d => ...)` and `mapValue(result, d => ...)` calls with mapper functions:

For `list`:
```typescript
const result = res.data as ServerPagedResult<ProductSummary>
return { ...result, items: result.items?.map(mapToSummaryModel) ?? [] }
```

For `getById`:
```typescript
const result = res.data as ServerResult<ProductDetail>
if (result.value) result.value = mapToDetailModel(result.value)
return result
```

For `create`:
```typescript
const result = res.data as ServerResult<ProductDetail>
if (result.value) result.value = mapToDetailModel(result.value)
return result
```

For `update`:
```typescript
const result = res.data as ServerResult<ProductDetail>
if (result.value) result.value = mapToDetailModel(result.value)
return result
```

- [ ] **Step 11: Update `store/product.store.ts` — fix import paths**

Change all imports from `../types/product.response` to `../models/product.response` and `../types/product.request` to `../models/product.request`.

Also update any `../types/product.parameters` to `../models/product.parameters`.

- [ ] **Step 12: Create `routes.ts`**

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const productRoutes: RouteRecordRaw[] = [
  {
    path: 'products',
    children: [
      {
        path: '',
        name: 'catalog.products.list',
        component: () => import('./pages/ProductListPage.vue'),
      },
      {
        path: 'create',
        name: 'catalog.products.create',
        component: () => import('./pages/ProductFormPage.vue'),
      },
      {
        path: ':id/edit',
        name: 'catalog.products.edit',
        component: () => import('./pages/ProductFormPage.vue'),
      },
    ],
  },
]
```

- [ ] **Step 13: Update `index.ts`**

```typescript
export { productRoutes } from './routes'
export { productRepository } from './api/product.api'
export { useProductStore } from './store/product.store'
export * from './types/create-product.field'
export * from './types/update-product.field'
export * from './types/product-image.response'
export * from './types/product.query'
export * from './models/product.parameters'
export * from './models/product.request'
export * from './models/product.response'
export * from './models/product.model'
```

Remove the duplicate `export * from './types/create-product.field'`.

- [ ] **Step 14: Update `__tests__/product.store.spec.ts` — fix imports**

Update any `../types/product.response` to `../models/product.response` and `../types/product.request` to `../models/product.request`.

- [ ] **Step 15: Commit**

```bash
git add app/Admin/src/features/catalog/products/
git commit -m "refactor(admin): add per-field schemas, models, mapper, routes to products"
```

---

### Task 4: Flatten product-option-types + classifications (join entities)

These are join entities with no standalone pages — only components, api, store, types.

**Files for product-option-types:**
- Create: `app/Admin/src/features/catalog/product-option-types/` (entire directory)
- Move from: `app/Admin/src/features/catalog/products/option-types/`

**Files for classifications:**
- Create: `app/Admin/src/features/catalog/classifications/` (entire directory)
- Move from: `app/Admin/src/features/catalog/products/classifications/`

- [ ] **Step 1: Create product-option-types directory and move files**

```bash
mkdir -p app/Admin/src/features/catalog/product-option-types/{api,store,types,models,components,composables}
cp -r app/Admin/src/features/catalog/products/option-types/* app/Admin/src/features/catalog/product-option-types/
```

- [ ] **Step 2: Update product-option-types imports**

In `api/product-option-type.api.ts`, change the import from:
```typescript
import type { OptionTypeDetail } from '../../../option-types/types/option-type.response'
```
To:
```typescript
import type { OptionTypeDetail } from '../../option-types/models/option-type.response'
```

In `store/product-option-type.store.ts`, change the same import.

In `types/product-option-type.response.ts`, change:
```typescript
import type { OptionTypeListItem, OptionTypeDetail } from '../../../option-types/types/option-type.response'
```
To:
```typescript
import type { OptionTypeListItem, OptionTypeDetail } from '../../option-types/models/option-type.response'
```

- [ ] **Step 3: Create `models/` for product-option-types**

Create `models/product-option-type.parameters.ts`:
```typescript
export interface ProductOptionTypeParameters {
  optionTypeIds: string[]
}
```

Create `models/product-option-type.request.ts`:
```typescript
export interface SyncProductOptionTypesRequest {
  optionTypeIds: string[]
}
```

Create `models/product-option-type.response.ts`:
```typescript
import type { OptionTypeListItem, OptionTypeDetail } from '../../option-types/models/option-type.response'
export type { OptionTypeListItem, OptionTypeDetail }
export type ProductOptionTypeAssignment = OptionTypeDetail
```

Delete old types files being replaced:
```bash
rm app/Admin/src/features/catalog/product-option-types/types/product-option-type.response.ts
rm app/Admin/src/features/catalog/product-option-types/types/product-option-type.request.ts
rm app/Admin/src/features/catalog/product-option-types/types/product-option-type.parameters.ts
```

- [ ] **Step 4: Create `routes.ts` and `index.ts` for product-option-types**

`routes.ts`:
```typescript
import type { RouteRecordRaw } from 'vue-router'

export const productOptionTypeRoutes: RouteRecordRaw[] = []
```

`index.ts`:
```typescript
export { productOptionTypeRoutes } from './routes'
export { productOptionTypeApi } from './api/product-option-type.api'
export { useProductOptionTypeStore } from './store/product-option-type.store'
export * from './types/product-option-type.field'
export * from './models/product-option-type.parameters'
export * from './models/product-option-type.request'
export * from './models/product-option-type.response'
```

```bash
touch app/Admin/src/features/catalog/product-option-types/composables/.gitkeep
```

- [ ] **Step 5: Create classifications directory and move files**

```bash
mkdir -p app/Admin/src/features/catalog/classifications/{api,store,types,models,components,composables}
cp -r app/Admin/src/features/catalog/products/classifications/* app/Admin/src/features/catalog/classifications/
```

- [ ] **Step 6: Update classifications imports**

`api/product-classification.api.ts` — imports are relative (`../types/...`) so they work unchanged in the new location.

- [ ] **Step 7: Create `models/` for classifications**

Create `models/classification.parameters.ts`:
```typescript
export interface ClassificationParameters {
  taxonIds: string[]
  mainTaxonId?: string | null
}
```

Create `models/classification.request.ts`:
```typescript
export interface SyncClassificationsRequest {
  taxonIds: string[]
  mainTaxonId?: string
}
```

Create `models/classification.response.ts`:
```typescript
export interface ProductClassification {
  id: string
  productId: string
  taxonId: string
  position: number
  isAutomatic: boolean
  isMain: boolean
  taxonName?: string
  taxonomyName?: string
}

export type ClassificationListItem = ProductClassification
```

Delete old types files:
```bash
rm app/Admin/src/features/catalog/classifications/types/classification.request.ts
rm app/Admin/src/features/catalog/classifications/types/classification.response.ts
```

`types/product-classification.field.ts` stays in `types/` (it's a Zod schema).

- [ ] **Step 8: Create `routes.ts` and `index.ts` for classifications**

`routes.ts`:
```typescript
import type { RouteRecordRaw } from 'vue-router'
export const classificationRoutes: RouteRecordRaw[] = []
```

`index.ts`:
```typescript
export { classificationRoutes } from './routes'
export { productClassificationApi } from './api/product-classification.api'
export { useClassificationStore } from './store/classification.store'
export * from './types/product-classification.field'
export * from './models/classification.parameters'
export * from './models/classification.request'
export * from './models/classification.response'
```

```bash
touch app/Admin/src/features/catalog/classifications/composables/.gitkeep
```

- [ ] **Step 9: Update `products/api/product.api.ts` — fix sub-entity import paths**

Change:
```typescript
import { productOptionTypeApi } from '../option-types/api/product-option-type.api'
import { productClassificationApi } from '../classifications/api/product-classification.api'
```
To:
```typescript
import { productOptionTypeApi } from '../../product-option-types/api/product-option-type.api'
import { productClassificationApi } from '../../classifications/api/product-classification.api'
```

Also update classification type imports:
```typescript
import type { ProductClassification } from '../classifications/types/classification.response'
import type { SyncClassificationsRequest } from '../classifications/types/classification.request'
```
To:
```typescript
import type { ProductClassification } from '../../classifications/models/classification.response'
import type { SyncClassificationsRequest } from '../../classifications/models/classification.request'
```

And the option-type import:
```typescript
import type { OptionTypeDetail } from '../../option-types/types/option-type.response'
```
To:
```typescript
import type { OptionTypeDetail } from '../../option-types/models/option-type.response'
```

- [ ] **Step 10: Delete old nested directories**

```bash
rm -rf app/Admin/src/features/catalog/products/option-types/
rm -rf app/Admin/src/features/catalog/products/classifications/
```

- [ ] **Step 11: Commit**

```bash
git add app/Admin/src/features/catalog/product-option-types/ app/Admin/src/features/catalog/classifications/ app/Admin/src/features/catalog/products/api/product.api.ts
git rm -r app/Admin/src/features/catalog/products/option-types/ app/Admin/src/features/catalog/products/classifications/
git commit -m "refactor(admin): flatten product-option-types and classifications"
```

---

### Task 5: Flatten variants — full pattern (pages, routes)

**Files:**
- Create: `app/Admin/src/features/catalog/variants/` (move + restructure)
- Move: `models/variant.model.ts` stays (already in Models-like location)
- Source: `app/Admin/src/features/catalog/products/variants/`

- [ ] **Step 1: Move all variant files**

```bash
mkdir -p app/Admin/src/features/catalog/variants/{api,store,types,models,pages,components,composables}
cp -r app/Admin/src/features/catalog/products/variants/api app/Admin/src/features/catalog/variants/
cp -r app/Admin/src/features/catalog/products/variants/types app/Admin/src/features/catalog/variants/
cp -r app/Admin/src/features/catalog/products/variants/models app/Admin/src/features/catalog/variants/
cp -r app/Admin/src/features/catalog/products/variants/components app/Admin/src/features/catalog/variants/
```

- [ ] **Step 2: Rewrite `types/variant.field.ts` — per-field schemas**

```typescript
import { z } from 'zod'

export function skuSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().min(1, t('catalog.validation.sku.required')).max(100, t('catalog.validation.sku.max_length'))
}

export function barcodeSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().max(50, t('catalog.validation.barcode.max_length')).optional()
}

export function priceSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number().min(0, t('catalog.validation.price.min')).default(0)
}

export function compareAtPriceSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number().min(0, t('catalog.validation.compare_at_price.min')).optional().nullable()
}

export function costPriceSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number().min(0, t('catalog.validation.cost_price.min')).optional().nullable()
}

export function positionSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number().int(t('catalog.validation.position.whole')).min(0, t('catalog.validation.position.min')).default(0)
}

export function trackInventorySchema() {
  return z.boolean().default(true)
}

export function weightSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number().min(0, t('catalog.validation.weight.min')).optional().nullable()
}

export function heightSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number().min(0, t('catalog.validation.height.min')).optional().nullable()
}

export function widthSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number().min(0, t('catalog.validation.width.min')).optional().nullable()
}

export function depthSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number().min(0, t('catalog.validation.depth.min')).optional().nullable()
}

export function optionValueIdsSchema() {
  return z.array(z.string().uuid()).optional()
}

export function createVariantSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
    sku: skuSchema(t),
    barcode: barcodeSchema(t),
    price: priceSchema(t),
    compareAtPrice: compareAtPriceSchema(t),
    costPrice: costPriceSchema(t),
    position: positionSchema(t),
    trackInventory: trackInventorySchema(),
    weight: weightSchema(t),
    height: heightSchema(t),
    width: widthSchema(t),
    depth: depthSchema(t),
    optionValueIds: optionValueIdsSchema(),
  })
}

export type VariantParameters = z.infer<ReturnType<typeof createVariantSchema>>
```

- [ ] **Step 3: Create `models/variant.parameters.ts`**

```typescript
export interface VariantParameters {
  sku: string
  barcode?: string
  price: number
  compareAtPrice?: number | null
  costPrice?: number | null
  position: number
  trackInventory: boolean
  weight?: number | null
  height?: number | null
  width?: number | null
  depth?: number | null
}
```

- [ ] **Step 4: Create `models/variant.request.ts`**

```typescript
import type { VariantParameters } from './variant.parameters'

export type CreateVariantRequest = VariantParameters & { productId?: string; optionValueIds?: string[] }
export type UpdateVariantRequest = Partial<CreateVariantRequest>
```

- [ ] **Step 5: Create `models/variant.response.ts`**

```typescript
export interface VariantSummary {
  id: string
  productId: string
  sku: string | null
  price: number
  costPrice: number | null
  costCurrency: string
  isMaster: boolean
  position: number
  trackInventory: boolean
  weightUnit: string
  dimensionsUnit: string
}

export interface VariantDetail extends VariantSummary {
  weight: number | null
  height: number | null
  width: number | null
  depth: number | null
  pricesCount: number
  discontinuedOn: string | null
}
```

- [ ] **Step 6: Clean up old types files**

```bash
rm app/Admin/src/features/catalog/variants/types/variant.response.ts
rm app/Admin/src/features/catalog/variants/types/variant.request.ts
rm app/Admin/src/features/catalog/variants/types/variant.parameters.ts
```

- [ ] **Step 7: Create `api/variant.mapper.ts`**

```typescript
import type { VariantSummary, VariantDetail } from '../models/variant.response'
import { decimalToDisplay } from '@/shared/utils/currency'

export function mapToSummary(dto: Record<string, unknown>): VariantSummary {
  return {
    id: String(dto.id ?? ''),
    productId: String(dto.productId ?? ''),
    sku: dto.sku as string | null ?? null,
    price: Number(dto.price ?? 0),
    costPrice: dto.costPrice as number | null ?? null,
    costCurrency: String(dto.costCurrency ?? ''),
    isMaster: Boolean(dto.isMaster),
    position: Number(dto.position ?? 0),
    trackInventory: Boolean(dto.trackInventory),
    weightUnit: String(dto.weightUnit ?? ''),
    dimensionsUnit: String(dto.dimensionsUnit ?? ''),
  }
}

export function mapToDetail(dto: Record<string, unknown>): VariantDetail {
  return {
    ...mapToSummary(dto),
    weight: dto.weight as number | null ?? null,
    height: dto.height as number | null ?? null,
    width: dto.width as number | null ?? null,
    depth: dto.depth as number | null ?? null,
    pricesCount: Number(dto.pricesCount ?? 0),
    discontinuedOn: dto.discontinuedOn as string | null ?? null,
  }
}

export function mapToSummaryModel(dto: Record<string, unknown>) {
  const summary = mapToSummary(dto)
  return { ...summary, priceDisplay: decimalToDisplay(summary.price) }
}

export function mapToDetailModel(dto: Record<string, unknown>) {
  const detail = mapToDetail(dto)
  return { ...detail, priceDisplay: decimalToDisplay(detail.price) }
}
```

- [ ] **Step 8: Update `api/variant.api.ts` — use mapper**

Replace `mapValue(result, d => ...)` calls with mapper functions. Update imports:

```typescript
import apiClient from '@/common/api/http/api.client'
import { CATALOG } from '@/common/api/constants'
import type { ServerResult } from '@/common/api/types/result.types'
import type { VariantDetail, VariantSummary } from '../models/variant.response'
import type { CreateVariantRequest, UpdateVariantRequest } from '../models/variant.request'
import type { VariantSummaryModel, VariantDetailModel } from '../models/variant.model'
import { mapToSummaryModel, mapToDetailModel } from './variant.mapper'
import { mapValue, decimalToDisplay } from '@/shared/utils/currency'
```

Replace the body of `getById`:
```typescript
const result = await apiClient.get(`${CATALOG}/variants/${id}`).then(res => res.data as ServerResult<VariantDetail>)
if (result.value) result.value = mapToDetailModel(result.value)
return result
```

Replace `listByProductId`:
```typescript
const result = await apiClient.get(`${CATALOG}/products/${productId}/variants`).then(res => res.data as ServerResult<VariantSummary[]>)
if (result.value) result.value = (result.value as unknown as Record<string, unknown>[]).map(mapToSummaryModel) as unknown as VariantSummaryModel[]
return result
```

Replace `create`:
```typescript
const result = await apiClient.post(`${CATALOG}/products/${productId}/variants`, data).then(res => res.data as ServerResult<VariantDetail>)
if (result.value) result.value = mapToDetailModel(result.value)
return result
```

Replace `update`:
```typescript
const result = await apiClient.put(`${CATALOG}/variants/${id}`, data).then(res => res.data as ServerResult<VariantDetail>)
if (result.value) result.value = mapToDetailModel(result.value)
return result
```

- [ ] **Step 9: Delete old `products/variants/api/variant.api.ts` references to `mapValue` and `decimalToDisplay` directly — remove them**

Remove the import for `mapValue` and `decimalToDisplay` if no longer used after mapper replacement:
```typescript
// Remove: import { mapValue } from '@/common/utils/transform'
// Remove: import { decimalToDisplay } from '@/shared/utils/currency'
```

- [ ] **Step 10: Create `routes.ts`**

```typescript
import type { RouteRecordRaw } from 'vue-router'
export const variantRoutes: RouteRecordRaw[] = []
```

- [ ] **Step 11: Create `index.ts`**

```typescript
export { variantRoutes } from './routes'
export { variantRepository } from './api/variant.api'
export * from './types/variant.field'
export * from './types/variant.query'
export * from './models/variant.parameters'
export * from './models/variant.request'
export * from './models/variant.response'
```

```bash
touch app/Admin/src/features/catalog/variants/composables/.gitkeep
```

- [ ] **Step 12: Update cross-entity imports in variant components**

The variant components (`ProductVariantManager.vue`, `VariantFormDialog.vue`, etc.) import from relative paths. After moving, update any imports referencing `../../option-types/` paths to `../../../option-types/`.

Check each `.vue` file in `variants/components/` for import path issues and fix.

- [ ] **Step 13: Commit**

```bash
git add app/Admin/src/features/catalog/variants/
git rm -rf app/Admin/src/features/catalog/products/variants/api/ app/Admin/src/features/catalog/products/variants/types/ app/Admin/src/features/catalog/products/variants/models/ app/Admin/src/features/catalog/products/variants/components/
git commit -m "refactor(admin): flatten variants, add per-field schemas, mapper, models"
```

---

### Task 6: Flatten variant-images + variant-prices (child entities)

These are child entities with no standalone pages — quick moves with import fixes.

**Files:**
- Create: `app/Admin/src/features/catalog/variant-images/` (move from `products/variants/images/`)
- Create: `app/Admin/src/features/catalog/variant-prices/` (move from `products/variants/prices/`)

- [ ] **Step 1: Move variant-images**

```bash
mkdir -p app/Admin/src/features/catalog/variant-images/{api,store,types,models,components,composables,__tests__}
cp -r app/Admin/src/features/catalog/products/variants/images/* app/Admin/src/features/catalog/variant-images/
```

- [ ] **Step 2: Create `models/image.response.ts` for variant-images**

```typescript
export interface VariantImage {
  id: string
  variantId: string
  url: string
  alt: string | null
  position: number
  role: number
  fileSize: number | null
  isDefault: boolean
}
```

Delete old `types/image.response.ts`.

- [ ] **Step 3: Create `routes.ts` and `index.ts` for variant-images**

`routes.ts`:
```typescript
import type { RouteRecordRaw } from 'vue-router'
export const variantImageRoutes: RouteRecordRaw[] = []
```

`index.ts`:
```typescript
export { variantImageRoutes } from './routes'
export { imageApi } from './api/image.api'
export { useImageStore } from './store/image.store'
export * from './types/image.field'
export * from './types/image.request'
export * from './models/image.response'
```

```bash
touch app/Admin/src/features/catalog/variant-images/composables/.gitkeep
```

- [ ] **Step 4: Move variant-prices**

```bash
mkdir -p app/Admin/src/features/catalog/variant-prices/{api,store,types,models,components,composables}
cp -r app/Admin/src/features/catalog/products/variants/prices/* app/Admin/src/features/catalog/variant-prices/
```

- [ ] **Step 5: Create `models/price.response.ts` for variant-prices**

```typescript
export interface PriceRecord {
  id: string
  amount: number
  currency: string
}
```

Delete old `types/price.response.ts`.

- [ ] **Step 6: Create `routes.ts` and `index.ts` for variant-prices**

`routes.ts`:
```typescript
import type { RouteRecordRaw } from 'vue-router'
export const variantPriceRoutes: RouteRecordRaw[] = []
```

`index.ts`:
```typescript
export { variantPriceRoutes } from './routes'
export { priceApi } from './api/price.api'
export { usePriceStore } from './store/price.store'
export * from './types/price.field'
export * from './types/price.request'
export * from './models/price.response'
```

```bash
touch app/Admin/src/features/catalog/variant-prices/composables/.gitkeep
```

- [ ] **Step 7: Update variant components referencing images/prices**

In `variants/components/ProductImageManager.vue`, update import to:
```typescript
import { imageApi } from '../../../variant-images/api/image.api'
```

In any component importing from `./images/` or `./prices/`.

- [ ] **Step 8: Delete old directories**

```bash
rm -rf app/Admin/src/features/catalog/products/variants/images/
rm -rf app/Admin/src/features/catalog/products/variants/prices/
rm -rf app/Admin/src/features/catalog/products/variants/
```

- [ ] **Step 9: Commit**

```bash
git add app/Admin/src/features/catalog/variant-images/ app/Admin/src/features/catalog/variant-prices/
git rm -rf app/Admin/src/features/catalog/products/variants/
git commit -m "refactor(admin): flatten variant-images and variant-prices"
```

---

### Task 7: Refactor taxonomies — per-field schemas, models, mapper, routes

**Files:**
- Create: `app/Admin/src/features/catalog/taxonomies/models/taxonomy.parameters.ts`
- Create: `app/Admin/src/features/catalog/taxonomies/models/taxonomy.request.ts`
- Create: `app/Admin/src/features/catalog/taxonomies/models/taxonomy.response.ts`
- Create: `app/Admin/src/features/catalog/taxonomies/api/taxonomy.mapper.ts`
- Create: `app/Admin/src/features/catalog/taxonomies/routes.ts`
- Modify: `app/Admin/src/features/catalog/taxonomies/types/taxonomy.field.ts` (per-field)
- Modify: `app/Admin/src/features/catalog/taxonomies/types/taxonomy.parameters.ts`
- Modify: `app/Admin/src/features/catalog/taxonomies/types/taxonomy.request.ts`
- Modify: `app/Admin/src/features/catalog/taxonomies/api/taxonomy.api.ts`
- Modify: `app/Admin/src/features/catalog/taxonomies/store/taxonomy.store.ts`
- Modify: `app/Admin/src/features/catalog/taxonomies/index.ts`
- Delete: `app/Admin/src/features/catalog/taxonomies/types/taxonomy.response.ts`

- [ ] **Step 1: Rewrite `types/taxonomy.field.ts` — per-field schemas**

```typescript
import { z } from 'zod'

export function nameSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().min(1, t('catalog.validation.name.required')).max(100)
}

export function presentationSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().min(1, t('catalog.validation.presentation.required')).max(100)
}

export function positionSchema() {
  return z.number().int().min(0).default(0)
}

export function createTaxonomySchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
    name: nameSchema(t),
    presentation: presentationSchema(t),
    position: positionSchema(),
  })
}

export type TaxonomyParameters = z.infer<ReturnType<typeof createTaxonomySchema>>
```

- [ ] **Step 2: Create `models/taxonomy.parameters.ts`**

```typescript
export interface TaxonomyParameters {
  name: string
  presentation: string
  position: number
}
```

- [ ] **Step 3: Create `models/taxonomy.request.ts`**

```typescript
import type { TaxonomyParameters } from './taxonomy.parameters'
export type CreateTaxonomyRequest = TaxonomyParameters
export type UpdateTaxonomyRequest = Partial<TaxonomyParameters>
```

- [ ] **Step 4: Create `models/taxonomy.response.ts`**

```typescript
export interface TaxonomyListItem {
  id: string
  name: string
  presentation: string | null
  position: number
  taxonsCount: number
  createdAtUtc: string
  modifiedAtUtc: string
}

export interface TaxonNode {
  id: string
  name: string
  slug: string
  position: number
  child: TaxonNode[]
}

export interface TaxonomyDetail extends TaxonomyListItem {
  root: TaxonNode | null
}
```

- [ ] **Step 5: Delete `types/taxonomy.response.ts`**

```bash
rm app/Admin/src/features/catalog/taxonomies/types/taxonomy.response.ts
```

- [ ] **Step 6: Update `types/taxonomy.parameters.ts`**

```typescript
import type { TaxonomyParameters } from '../models/taxonomy.parameters'
export type { TaxonomyParameters }
```

- [ ] **Step 7: Update `types/taxonomy.request.ts`**

```typescript
import type { CreateTaxonomyRequest, UpdateTaxonomyRequest } from '../models/taxonomy.request'
export type { CreateTaxonomyRequest, UpdateTaxonomyRequest }
```

- [ ] **Step 8: Create `api/taxonomy.mapper.ts`**

```typescript
import type { TaxonomyListItem, TaxonomyDetail } from '../models/taxonomy.response'

export function mapToListItem(dto: Record<string, unknown>): TaxonomyListItem {
  return {
    id: String(dto.id ?? ''),
    name: String(dto.name ?? ''),
    presentation: dto.presentation as string | null ?? null,
    position: Number(dto.position ?? 0),
    taxonsCount: Number(dto.taxonsCount ?? 0),
    createdAtUtc: String(dto.createdAtUtc ?? ''),
    modifiedAtUtc: String(dto.modifiedAtUtc ?? ''),
  }
}

export function mapToDetail(dto: Record<string, unknown>): TaxonomyDetail {
  return {
    ...mapToListItem(dto),
    root: dto.root as TaxonNode | null ?? null,
  }
}
```

- [ ] **Step 9: Update `api/taxonomy.api.ts` — fix import paths**

Change:
```typescript
import type { TaxonomyDetail, TaxonomyListItem } from "../types/taxonomy.response";
import type { CreateTaxonomyRequest, UpdateTaxonomyRequest } from "../types/taxonomy.request";
```
To:
```typescript
import type { TaxonomyDetail, TaxonomyListItem } from '../models/taxonomy.response'
import type { CreateTaxonomyRequest, UpdateTaxonomyRequest } from '../models/taxonomy.request'
```

- [ ] **Step 10: Update `store/taxonomy.store.ts` — fix import paths, wire mapper**

Change all `../types/taxonomy.response` imports to `../models/taxonomy.response`.
Change all `../types/taxonomy.request` imports to `../models/taxonomy.request`.

Add mapper usage in `fetchTaxonomies`:
```typescript
import { mapToListItem, mapToDetail } from '../api/taxonomy.mapper'

// In fetchTaxonomies:
if (result.isSuccess && result.items) {
  taxonomies.value = result.items.map(mapToListItem)
  totalRecords.value = result.totalCount || 0
}

// In fetchTaxonomyById:
if (result.isSuccess && result.value) {
  current_taxonomy.value = mapToDetail(result.value)
}
```

- [ ] **Step 11: Create `routes.ts`**

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const taxonomyRoutes: RouteRecordRaw[] = [
  {
    path: 'taxonomies',
    component: () => import('./pages/TaxonomyManagerPage.vue'),
    children: [
      {
        path: '',
        name: 'catalog.taxonomies.list',
        component: () => import('@/shared/components/feedback/ManagerWelcome.vue'),
        props: {
          title: 'Hierarchy Manager',
          description: 'Select a taxonomy from the left to edit its configuration, or create a new one to start a new product hierarchy.',
          icon: 'pi-sitemap',
        },
      },
      {
        path: 'create',
        name: 'catalog.taxonomies.create',
        component: () => import('./pages/TaxonomyFormPage.vue'),
      },
      {
        path: ':id/edit',
        name: 'catalog.taxonomies.edit',
        component: () => import('./pages/TaxonomyFormPage.vue'),
      },
    ],
  },
]
```

- [ ] **Step 12: Update `index.ts`**

```typescript
export { taxonomyRoutes } from './routes'
export { taxonomyRepository } from './api/taxonomy.api'
export { useTaxonomyStore } from './store/taxonomy.store'
export * from './types/taxonomy.field'
export * from './types/taxonomy.query'
export * from './models/taxonomy.parameters'
export * from './models/taxonomy.request'
export * from './models/taxonomy.response'
```

- [ ] **Step 13: Update tests**

In `__tests__/taxonomy.schema.spec.ts` and `__tests__/taxonomy.store.spec.ts`, update import paths from `../types/taxonomy.response` to `../models/taxonomy.response`.

- [ ] **Step 14: Commit**

```bash
git add app/Admin/src/features/catalog/taxonomies/
git commit -m "refactor(admin): add per-field schemas, models, mapper, routes to taxonomies"
```

---

### Task 8: Flatten taxa — full pattern

**Files:**
- Create: `app/Admin/src/features/catalog/taxa/` (move from `taxonomies/taxa/`)
- Source: `app/Admin/src/features/catalog/taxonomies/taxa/`

- [ ] **Step 1: Move taxa files**

```bash
mkdir -p app/Admin/src/features/catalog/taxa/{api,store,types,models,pages,components,composables,__tests__}
cp -r app/Admin/src/features/catalog/taxonomies/taxa/api app/Admin/src/features/catalog/taxa/
cp -r app/Admin/src/features/catalog/taxonomies/taxa/store app/Admin/src/features/catalog/taxa/
cp -r app/Admin/src/features/catalog/taxonomies/taxa/pages app/Admin/src/features/catalog/taxa/
cp -r app/Admin/src/features/catalog/taxonomies/taxa/components app/Admin/src/features/catalog/taxa/
cp -r app/Admin/src/features/catalog/taxonomies/taxa/types app/Admin/src/features/catalog/taxa/
cp -r app/Admin/src/features/catalog/taxonomies/taxa/__tests__ app/Admin/src/features/catalog/taxa/
```

- [ ] **Step 2: Copy taxon field schemas from taxonomies/types/ into taxa/types/**

```bash
cp app/Admin/src/features/catalog/taxonomies/types/taxon.field.ts app/Admin/src/features/catalog/taxa/types/taxon.field.ts
cp app/Admin/src/features/catalog/taxonomies/types/taxon-rule.field.ts app/Admin/src/features/catalog/taxa/types/taxon-rule.field.ts
```

- [ ] **Step 3: Update imports in `taxa/api/taxon.api.ts`**

Change the import:
```typescript
import type { ProductSummaryModel } from '../../../products/models/product.model'
```
To:
```typescript
import type { ProductSummaryModel } from '../../products/models/product.model'
```

Change:
```typescript
import type { CreateTaxonRequest, UpdateTaxonRequest } from "../types/taxon.request";
```
To:
```typescript
import type { CreateTaxonRequest, UpdateTaxonRequest } from '../types/taxon.request'
```

- [ ] **Step 4: Update `taxa/store/taxon.store.ts` imports**

The relative imports within taxa/ work unchanged since the internal folder structure is preserved.

- [ ] **Step 5: Create `models/` for taxa**

Create `models/taxon.parameters.ts`:
```typescript
export interface TaxonParameters {
  taxonomyId: string
  name: string
  presentation: string
  description?: string | null
  slug: string
  position: number
  hideFromNav: boolean
  parentId?: string | null
  automatic: boolean
  rulesMatchPolicy: string
  sortOrder: string
  metaTitle?: string | null
  metaDescription?: string | null
  metaKeywords?: string | null
}
```

Create `models/taxon.request.ts`:
```typescript
import type { TaxonParameters } from './taxon.parameters'
import type { TaxonRuleParameters } from './taxon-rule.parameters'

export type CreateTaxonRequest = TaxonParameters & { rules?: TaxonRuleParameters[] }
export type UpdateTaxonRequest = CreateTaxonRequest
```

Create `models/taxon.response.ts`:
```typescript
import type { TaxonRuleListItem } from './taxon-rule.response'

export interface TaxonListItem {
  id: string; taxonomyId: string; parentId?: string; name: string; presentation: string
  description?: string; slug: string; permalink: string; prettyName: string
  position: number; hideFromNav: boolean; depth: number; productCount: number
  childrenCount: number; lft: number; rgt: number; hasChildren: boolean
  automatic: boolean; createdAtUtc: string; modifiedAtUtc: string
}

export interface TaxonTreeItem extends TaxonListItem {
  key: string; isExpanded?: boolean; children: TaxonTreeItem[]
}

export interface TaxonDetail extends TaxonListItem {
  rulesMatchPolicy: string; sortOrder: string; metaTitle?: string
  metaDescription?: string; metaKeywords?: string; taxonRuleCount: number
  rules?: TaxonRuleListItem[]
}
```

Create `models/taxon-rule.parameters.ts`:
```typescript
export interface TaxonRuleParameters {
  type: string
  value: string
  matchPolicy: string
}
```

Create `models/taxon-rule.request.ts`:
```typescript
import type { TaxonRuleParameters } from './taxon-rule.parameters'

export type CreateTaxonRuleRequest = TaxonRuleParameters
export type UpdateTaxonRuleRequest = CreateTaxonRuleRequest
```

Create `models/taxon-rule.response.ts`:
```typescript
export interface TaxonRuleListItem {
  id: string
  taxonId: string
  type: string
  value: string
  matchPolicy: string
}
```

Now delete the old types files that have been moved to models:
```bash
rm app/Admin/src/features/catalog/taxa/types/taxon.response.ts
rm app/Admin/src/features/catalog/taxa/types/taxon.request.ts
rm app/Admin/src/features/catalog/taxa/types/taxon.parameters.ts
rm app/Admin/src/features/catalog/taxa/types/taxon-rule.parameters.ts
rm app/Admin/src/features/catalog/taxa/types/taxon-rule.request.ts
rm app/Admin/src/features/catalog/taxa/types/taxon-rule.response.ts
```

- [ ] **Step 6: Update `taxa/api/taxon.api.ts` and `taxa/store/taxon.store.ts` — fix import paths to models**

In `api/taxon.api.ts`, change these imports from `../types/` to `../models/`:
```typescript
import type { TaxonDetail, TaxonListItem, TaxonTreeItem } from '../models/taxon.response'
import type { CreateTaxonRequest, UpdateTaxonRequest } from '../models/taxon.request'
import type { CreateTaxonRuleRequest, UpdateTaxonRuleRequest } from '../models/taxon-rule.request'
import type { TaxonRuleListItem } from '../models/taxon-rule.response'
```

In `store/taxon.store.ts`, change all `../types/taxon.response` to `../models/taxon.response`, `../types/taxon.request` to `../models/taxon.request`, `../types/taxon-rule.response` to `../models/taxon-rule.response`, `../types/taxon-rule.request` to `../models/taxon-rule.request`.

Also fix the `taxon-rule.parameters` import in `store/taxon.store.ts`:
```typescript
import type { CreateTaxonRuleRequest, UpdateTaxonRuleRequest } from '../models/taxon-rule.request'
```

- [ ] **Step 8: Create `routes.ts` for taxa**

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const taxonRoutes: RouteRecordRaw[] = [
  {
    path: 'categories',
    children: [
      {
        path: '',
        name: 'catalog.taxa.list',
        component: () => import('./pages/TaxonListPage.vue'),
      },
      {
        path: ':taxonomyId/manage',
        component: () => import('./pages/TaxonTreeManagerPage.vue'),
        name: 'catalog.taxa.manager',
        children: [
          {
            path: 'create',
            name: 'catalog.taxa.create',
            component: () => import('./pages/TaxonFormPage.vue'),
          },
          {
            path: ':id/edit',
            name: 'catalog.taxa.edit',
            component: () => import('./pages/TaxonFormPage.vue'),
          },
        ],
      },
    ],
  },
]
```

- [ ] **Step 9: Create `index.ts` for taxa**

```typescript
export { taxonRoutes } from './routes'
export { taxonRepository } from './api/taxon.api'
export { useTaxonStore } from './store/taxon.store'
export * from './types/taxon.field'
export * from './types/taxon.query'
export * from './types/taxon-rule.field'
export * from './models/taxon.parameters'
export * from './models/taxon.request'
export * from './models/taxon.response'
export * from './models/taxon-rule.parameters'
export * from './models/taxon-rule.request'
export * from './models/taxon-rule.response'
```

```bash
touch app/Admin/src/features/catalog/taxa/composables/.gitkeep
```

- [ ] **Step 10: Update tests — fix imports in `__tests__/`**

Update `__tests__/taxon.schema.spec.ts` and `__tests__/taxon.store.spec.ts` — fix any `../types/` paths to `../models/` for response/request types.

- [ ] **Step 11: Delete old nested directory**

```bash
rm -rf app/Admin/src/features/catalog/taxonomies/taxa/
```

- [ ] **Step 12: Commit**

```bash
git add app/Admin/src/features/catalog/taxa/
git rm -rf app/Admin/src/features/catalog/taxonomies/taxa/
git commit -m "refactor(admin): flatten taxa, add models, routes"
```

---

### Task 9: Update catalog.routes.ts — aggregate all entity routes

**Files:**
- Modify: `app/Admin/src/features/catalog/catalog.routes.ts`

- [ ] **Step 1: Rewrite `catalog.routes.ts`**

```typescript
import type { RouteRecordRaw } from 'vue-router'
import { optionTypeRoutes } from './option-types/routes'
import { optionValueRoutes } from './option-values/routes'
import { productRoutes } from './products/routes'
import { taxonomyRoutes } from './taxonomies/routes'
import { taxonRoutes } from './taxa/routes'

export const catalogRoutes: RouteRecordRaw = {
  path: 'catalog',
  meta: { breadcrumb: 'navigation.catalog' },
  children: [
    {
      path: '',
      name: 'catalog.dashboard',
      component: () => import('@/features/catalog/dashboard/pages/CatalogDashboardPage.vue'),
    },
    ...productRoutes,
    ...taxonomyRoutes,
    ...taxonRoutes,
    ...optionTypeRoutes,
    ...optionValueRoutes,
  ],
}
```

Remove all inline route definitions that are now in entity route files.

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/features/catalog/catalog.routes.ts
git commit -m "refactor(admin): update catalog.routes.ts to aggregate entity routes"
```

---

### Task 10: Fix cross-entity import paths in Vue pages and components

**Files:**
- All `.vue` files in `option-types/pages/`, `option-values/pages/`, `products/pages/`, `taxonomies/pages/`, `taxa/pages/`, `variants/components/`, etc.

This task hunts down all broken imports after the folder restructuring.

- [ ] **Step 1: Fix option-type page imports**

In `OptionTypeFormPage.vue` and `OptionTypeListPage.vue`, any import referencing `../../option-values/` or `../option-values/` needs to change to `../../option-values/` (from `option-types/pages/` to `option-values/`).

In `OptionTypeManagerPage.vue`, update stores/apis imports — they're relative to `option-types/` so they may be unchanged.

Run a search for broken imports:
```bash
cd app/Admin && grep -r "from.*option-types.*option-values" src/features/catalog/ --include="*.ts" --include="*.vue"
```

Fix each reference from `../option-values/types/option-value.field` to `../../option-values/types/option-value.field` (adjusting relative depth).

- [ ] **Step 2: Fix product page imports**

In `ProductFormPage.vue`, update imports that referenced nested directories:
- `../option-types/` → `../../product-option-types/`
- `../classifications/` → `../../classifications/`
- `../variants/` → `../../variants/`

- [ ] **Step 3: Fix variant component imports**

In `variants/components/*.vue`, update:
- `../images/` → `../../../variant-images/`
- `../prices/` → `../../../variant-prices/`

- [ ] **Step 4: Fix taxonomy/taxon page imports**

In `TaxonomyManagerPage.vue`, `TaxonomyFormPage.vue`, update:
- `../taxa/` → `../../taxa/`

In `taxa/pages/*.vue`, update:
- `../../types/taxon.field` → `../types/taxon.field` (taxon.field.ts now living in taxa/types/)

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/catalog/
git commit -m "fix(admin): update cross-entity import paths after flattening"
```

---

### Task 11: Remove remaining dead files and old directories

**Files to clean up:**
- `taxonomies/types/taxon.field.ts` (moved to taxa/types/)
- `taxonomies/types/taxon-rule.field.ts` (if exists — moved to taxa/types/)
- Any remaining empty type files in old locations

- [ ] **Step 1: Clean up taxonomy types**

```bash
rm -f app/Admin/src/features/catalog/taxonomies/types/taxon.field.ts
rm -f app/Admin/src/features/catalog/taxonomies/types/taxon-rule.field.ts
```

- [ ] **Step 2: Verify no orphaned references**

```bash
cd app/Admin && grep -r "from.*taxonomies.*taxa/" src/ --include="*.ts" --include="*.vue"
```

If any references found, fix them to point to `../../taxa/`.

- [ ] **Step 3: Commit**

```bash
git add -u app/Admin/src/features/catalog/
git commit -m "chore(admin): remove dead files after catalog refactor"
```

---

### Task 12: Run lint and tests — verify everything works

- [ ] **Step 1: Run lint**

```bash
cd app/Admin && pnpm run lint
```

Expected: zero errors. If errors found, fix import paths and re-run.

- [ ] **Step 2: Run unit tests**

```bash
cd app/Admin && pnpm run test:unit
```

Expected: all tests pass. If failures, check import paths in spec files.

- [ ] **Step 3: Run tests specifically for Catalog module**

```bash
cd app/Admin && pnpm run test:unit -- --reporter=verbose src/features/catalog/
```

- [ ] **Step 4: Fix any remaining issues and commit**

```bash
git add -A app/Admin/
git commit -m "fix(admin): fix lint and test issues after catalog refactor"
```

---

### Verify Checklist (after all tasks)

1. `cd app/Admin && pnpm run lint` passes with zero warnings
2. `cd app/Admin && pnpm run test:unit` passes all tests
3. No `as Type` casts in store files — mapper used instead
4. `types/` contains only Zod schemas and query types
5. `models/` contains only data interfaces
6. All 11 entities have the 9-slot structure (pages if applicable, otherwise empty folders)
7. `catalog.routes.ts` imports from entity `routes.ts` files
8. No nested entity folders remain (option-values, taxa, variants, etc. at catalog level)
