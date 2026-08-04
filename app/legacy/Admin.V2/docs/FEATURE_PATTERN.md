# Admin Feature Pattern — Agent Instructions

## Overview

Every feature in `src/features/` mirrors a backend API module using a vertical slice pattern.
Each feature is self-contained — no cross-feature imports allowed.

---

## Required directories (every feature MUST have all)

```
features/{feature}/
  api/            Static API client classes (one class per entity, all static methods)
  components/     Reusable Vue components (form, table, etc.)
  composables/    Thin factory functions wrapping store for component use
  pages/          Thin page-level components composing UI
  store/          Pinia store (composition API, one per feature)
  schemas/        Zod validation (1 file per entity, merged fields+forms)
  types/          Request/Response TypeScript interfaces
  routes.ts       Route definition + ROUTE const for type-safe names
  index.ts        Barrel exports (routes, store, composables, schemas, types)
```

## Optional directories (only when needed)

```
  mappers/    When form != API contract (request, response, or query params)
  services/   Non-API abstractions (e.g., token.service.ts)
  utils/      Pure utility functions (e.g., permissions.ts, roles.ts)
```

---

## Directory-by-directory rules

### `api/` — One static class per entity

Transform query params before sending when the API contract differs from the frontend shape. Use `ProductQueryMapper` when defined, or pass params directly when they match 1:1.

```ts
// api/product.api.ts
import apiClient from '@/shared/api/client'
import type { Result, PagedResult } from '@/shared/models'
import type { ProductResponse, CreateProductRequest, UpdateProductRequest, ProductListParams } from '../types'

export class ProductApi {
  static async getMany(params: ProductListParams = {}): Promise<PagedResult<ProductResponse>> {
    const res = await apiClient.get<PagedResult<ProductResponse>>('/catalog/products', { params })
    return res.data
  }
  static async get(id: string): Promise<Result<ProductResponse>> {
    const res = await apiClient.get<Result<ProductResponse>>(`/catalog/products/${id}`)
    return res.data
  }
  static async create(data: CreateProductRequest): Promise<Result<ProductResponse>> {
    const res = await apiClient.post<Result<ProductResponse>>('/catalog/products', data)
    return res.data
  }
  static async update(id: string, data: UpdateProductRequest): Promise<Result<ProductResponse>> {
    const res = await apiClient.put<Result<ProductResponse>>(`/catalog/products/${id}`, data)
    return res.data
  }
  static async delete(id: string): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>(`/catalog/products/${id}`)
    return res.data
  }
}
```

**For list/getMany methods**, use a query param mapper when the API expects different naming or format:

```ts
// api/product.api.ts — with query param mapping
import { ProductQueryMapper } from '../mappers/product.query.mapper'

export class ProductApi {
  static async getMany(params: ProductListParams = {}): Promise<PagedResult<ProductResponse>> {
    const apiParams = ProductQueryMapper.toApi(params)
    const res = await apiClient.get<PagedResult<ProductResponse>>('/catalog/products', { params: apiParams })
    return res.data
  }
}
```

Barrel: `api/index.ts` re-exports all XxxApi classes.

---

### `components/` — Vue SFCs

**Forms:** Accept `mode` prop (`'create' | 'view' | 'edit'`), use `vee-validate` with `toTypedSchema` from Zod schemas, use store actions for submit.

**Tables:** Use pagination/search, emit navigation events, use store for data.

Use shared components: `PageHeader`, `FormField`, `FormActions`, `LoadingSkeleton`, `ErrorState`.

---

### `composables/` — Thin factory wrappers

Each composable wraps the store for component convenience. Extracts route-state wiring only.

```ts
// composables/useProduct.ts
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useProductStore } from '../store/product.store'

export function useProduct() {
  const store = useProductStore()
  const route = useRoute()
  const router = useRouter()
  const id = computed(() => route.params.id as string | undefined)
  const mode = computed<'create' | 'view' | 'edit'>(() => {
    if (!id.value) return 'create'
    return route.name?.toString().endsWith('.edit') ? 'edit' : 'view'
  })
  return { id, mode, route, router, store }
}
```

Composables do NOT contain business logic — that lives in the store.

---

### `pages/` — Thin wrappers

Pages compose components and pass props. No business logic.

```vue
<!-- pages/ProductDetailPage.vue -->
<script setup lang="ts">
import { useProduct } from '../composables/useProduct'
import { ProductForm } from '../components/ProductForm.vue'
const { id, mode, store } = useProduct()
</script>
<template>
  <ProductForm :id :mode :store />
</template>
```

---

### `store/` — Pinia store (required)

Every feature has exactly one Pinia store. It owns all state and API orchestration.

```ts
// store/product.store.ts
import { ref, readonly } from 'vue'
import { defineStore } from 'pinia'
import { ProductApi } from '../api/product.api'
import type { ProductResponse, CreateProductRequest, UpdateProductRequest, ProductListParams } from '../types'

export const useProductStore = defineStore('catalog-product', () => {
  const items = ref<ProductResponse[]>([])
  const selected = ref<ProductResponse | null>(null)
  const totalCount = ref(0)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function fetchMany(params: ProductListParams = {}) {
    isLoading.value = true
    error.value = null
    try {
      const result = await ProductApi.getMany(params)
      // PagedResult has items/totalCount
      items.value = result.items ?? []
      totalCount.value = result.totalCount ?? 0
    } catch (e) {
      error.value = 'Failed to load items'
    }
    isLoading.value = false
  }

  async function fetchOne(id: string) {
    isLoading.value = true
    error.value = null
    const result = await ProductApi.get(id)
    if (result.isSuccess) {
      selected.value = result.value
    } else {
      error.value = result.message ?? 'Not found'
    }
    isLoading.value = false
  }

  async function create(data: CreateProductRequest) {
    const result = await ProductApi.create(data)
    if (result.isSuccess) return result.value
    throw result
  }

  async function remove(id: string) {
    await ProductApi.delete(id)
    items.value = items.value.filter(x => x.id !== id)
  }

  return {
    items: readonly(items),
    selected: readonly(selected),
    totalCount: readonly(totalCount),
    isLoading: readonly(isLoading),
    error: readonly(error),
    fetchMany, fetchOne, create, remove,
  }
})
```

**Store rules:**
- State is `ref()`, publicly exposed as `readonly()`
- Mutations only via store actions (no direct mutation from components)
- API calls go through the store, never from components or composables
- Error handling (fieldErrors, serverErrors) follows `auth.store.ts` pattern

---

### `schemas/` — Zod validation, 1 file per entity

Single class with field methods + `create()` / `update()` schemas:

```ts
// schemas/product.schema.ts
import { z } from 'zod'

export type TFunction = (key: string) => string

export class ProductSchema {
  constructor(private t: TFunction) {}

  create() {
    return z.object({
      name: z.string().min(1, this.t('catalog.validation.name.required')),
      slug: z.string().min(1, this.t('catalog.validation.slug.required')),
      description: z.string().optional(),
      status: z.enum(['Draft', 'Active', 'Archived']).optional(),
      department: z.string().optional(),
      genderTarget: z.string().optional(),
      styleCode: z.string().optional(),
    })
  }

  update() {
    return z.object({
      name: z.string().min(1, this.t('catalog.validation.name.required')),
      slug: z.string().min(1, this.t('catalog.validation.slug.required')),
      description: z.string().optional(),
      status: z.enum(['Draft', 'Active', 'Archived']).optional(),
    })
  }
}

export type CreateProductForm = z.input<ReturnType<ProductSchema['create']>>
export type UpdateProductForm = z.input<ReturnType<ProductSchema['update']>>
```

Barrel: `schemas/index.ts` exports schema classes + form types.

---

### `types/` — Request/Response interfaces

One file per entity per direction:

```ts
// types/product.request.ts
import type { CreateProductForm, UpdateProductForm } from '../schemas'

export type CreateProductRequest = CreateProductForm   // alias when same shape
export type UpdateProductRequest = UpdateProductForm   // alias when same shape

export interface ProductListParams {
  page?: number
  pageSize?: number
  search?: string
  sortBy?: string
  sortDirection?: 'asc' | 'desc'
  status?: 'Draft' | 'Active' | 'Archived'
}
```

```ts
// types/product.response.ts
export interface ProductResponse {
  id: string
  name: string
  slug: string
  description: string | null
  status: 'Draft' | 'Active' | 'Archived'
  styleCode: string | null
  department: string | null
  genderTarget: string | null
  createdAt: string
  updatedAt: string
}
```

Barrel: `types/index.ts` re-exports all types via `export type * from '...'`.

---

### `routes.ts` — Route config + ROUTE const

```ts
// routes.ts
import type { RouteRecordRaw } from 'vue-router'

export const ROUTE = {
  DASHBOARD: 'inventory.dashboard',
  STOCK: { LIST: 'inventory.stock.list', VIEW: 'inventory.stock.view', CREATE: 'inventory.stock.create', EDIT: 'inventory.stock.edit' },
  LOCATIONS: { LIST: 'inventory.locations.list', VIEW: 'inventory.locations.view', CREATE: 'inventory.locations.create', EDIT: 'inventory.locations.edit' },
  MOVEMENTS: { LIST: 'inventory.movements.list', VIEW: 'inventory.movements.view' },
  TRANSFERS: { LIST: 'inventory.transfers.list', VIEW: 'inventory.transfers.view', CREATE: 'inventory.transfers.create' },
  RESERVATIONS: { LIST: 'inventory.reservations.list', VIEW: 'inventory.reservations.view' },
} as const

export const inventoryRoutes: RouteRecordRaw = {
  path: 'inventory',
  children: [
    { path: '', redirect: { name: ROUTE.DASHBOARD } },
    { path: 'dashboard', name: ROUTE.DASHBOARD, component: () => import('./pages/DashboardPage.vue') },
    { path: 'stock', name: ROUTE.STOCK.LIST, component: () => import('./pages/StockListPage.vue') },
    { path: 'stock/new', name: ROUTE.STOCK.CREATE, component: () => import('./pages/StockItemDetailPage.vue') },
    { path: 'stock/:id', name: ROUTE.STOCK.VIEW, component: () => import('./pages/StockItemDetailPage.vue') },
    { path: 'stock/:id/edit', name: ROUTE.STOCK.EDIT, component: () => import('./pages/StockItemDetailPage.vue') },
    { path: 'locations', name: ROUTE.LOCATIONS.LIST, component: () => import('./pages/LocationListPage.vue') },
    { path: 'locations/new', name: ROUTE.LOCATIONS.CREATE, component: () => import('./pages/LocationDetailPage.vue') },
    { path: 'locations/:id', name: ROUTE.LOCATIONS.VIEW, component: () => import('./pages/LocationDetailPage.vue') },
    { path: 'locations/:id/edit', name: ROUTE.LOCATIONS.EDIT, component: () => import('./pages/LocationDetailPage.vue') },
    { path: 'movements', name: ROUTE.MOVEMENTS.LIST, component: () => import('./pages/MovementListPage.vue') },
    { path: 'transfers', name: ROUTE.TRANSFERS.LIST, component: () => import('./pages/TransferListPage.vue') },
    { path: 'transfers/:id', name: ROUTE.TRANSFERS.VIEW, component: () => import('./pages/TransferDetailPage.vue') },
    { path: 'transfers/new', name: ROUTE.TRANSFERS.CREATE, component: () => import('./pages/TransferDetailPage.vue') },
    { path: 'reservations', name: ROUTE.RESERVATIONS.LIST, component: () => import('./pages/StockReservationListPage.vue') },
  ],
}
```

**Rules:**
- Always use relative `import('./pages/...')` for lazy loading
- ROUTE const uses dot-notation names: `'inventory.stock.list'`
- ROUTE const is `as const` for type safety in components
- Shared page for create/view/edit (mode inferred from route)

---

### `index.ts` — Barrel exports

```ts
export { inventoryRoutes, ROUTE } from './routes'
export { useInventoryStore } from './store/inventory.store'
export { useStockItem } from './composables/useStockItem'
export { StockItemSchema } from './schemas'
export type * from './types'
export type { CreateStockItemForm, UpdateStockItemForm } from './schemas'
```

---

## Optional directories

### `mappers/` — Only when shape differs between layers

Three mapper categories — create only the ones you need:

**1. Request mapper** — form schema != API contract:
```ts
// mappers/auth.request.mapper.ts
import type { LoginForm } from '../schemas'
import type { LoginRequest } from '../types'
export class AuthRequestMapper {
  static toLogin(form: LoginForm): LoginRequest {
    return { email: form.email, password: form.password }
  }
}
```

**2. Response mapper** — API response != domain shape:
```ts
// mappers/product.response.mapper.ts
import type { ProductResponse } from '../types'
export class ProductResponseMapper {
  static fromApi(response: ProductResponse) {
    return { ...response, displayName: `${response.name} (${response.styleCode ?? 'N/A'})` }
  }
}
```

**3. Query mapper** — frontend list params != API query params:
```ts
// mappers/product.query.mapper.ts
import type { ProductListParams } from '../types'

export class ProductQueryMapper {
  static toApi(params: ProductListParams) {
    return {
      pageNumber: params.page,
      pageSize: params.pageSize ?? 20,
      searchTerm: params.search,
      sort: params.sortBy ? `${params.sortBy} ${params.sortDirection ?? 'asc'}` : undefined,
      statusFilter: params.status,
    }
  }
}
```

Naming convention:
- `XxxRequestMapper` — methods `toCreate`, `toUpdate`, `toLogin`, etc.
- `XxxResponseMapper` — methods `fromApi`, `fromJwt`, `fromSession`, etc.
- `XxxQueryMapper` — method `toApi(params)`

### `services/` — Non-API service abstractions

```ts
// services/token.service.ts
export class TokenService {
  static setTokens(access: string, refresh: string) { ... }
  static hasValidAccessToken(): boolean { ... }
  static getAccessTokenPayload(): Record<string, unknown> | null { ... }
  static clearTokens() { ... }
}
```

### `utils/` — Pure utility functions

```ts
// utils/permissions.ts
export function hasPermission(user: User, permission: string): boolean { ... }
```

---

## API to Frontend mapping

| API route prefix | Feature directory | Route name prefix |
|---|---|---|
| `catalog` | `features/catalog/` | `catalog.*` |
| `identity` | `features/auth/`, `features/users/` | `auth.*`, `users.*` |
| `inventory` | `features/inventory/` | `inventory.*` |
| `location` | `features/location/` | `location.*` |
| `ordering` | `features/ordering/` | `ordering.*` |
| `payment` | `features/payment/` | `payment.*` |
| `profile` | `features/profile/` | `profile.*` |
| `shipping` | `features/shipping/` | `shipping.*` |
| `dashboard` | `features/reports/` | `reports.*` |

Route name convention: `{module}.{entity}(.{subentity}).{action}`

---

## Enforcement checklist

When creating or migrating a feature, verify:
- [ ] All 9 required directories/files exist
- [ ] `store/` has exactly one Pinia store (composition API)
- [ ] API calls funnel through store actions only
- [ ] `schemas/` uses 1 file per entity (not separate fields/forms)
- [ ] `routes.ts` exports both route config AND `ROUTE` const
- [ ] `types/` has separate `.request.ts` and `.response.ts` per entity
- [ ] `api/` getMany methods use `XxxQueryMapper.toApi()` when API params differ from frontend
- [ ] `mappers/` only exists when shape differs between layers
- [ ] No cross-feature imports (no `from '@/features/other-feature'`)
- [ ] Component imports use relative paths within same feature
- [ ] Pages are thin wrappers with no business logic
