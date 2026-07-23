---
goal: Feature Module Convention — Auth Reference Pattern
version: 1.0
date_created: 2026-07-23
owner: Admin SPA team
status: Completed
tags: [process, pattern, convention, vue, feature-module]
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Feature module convention derived from `features/auth/`. All modules must match this structure exactly.

## 1. Directory Structure

```
features/{module}/
├── index.ts                       # Barrel: routes, store, composable, types, schemas, mappers, utils
├── routes.ts                      # ROUTE const + RouteRecordRaw definitions
├── api/
│   ├── index.ts                   # Barrel
│   ├── {module}.api.ts            # Static class {Module}Api — one per module
│   └── __tests__/
│       └── {module}.api.spec.ts   # Vitest: mocks apiClient, tests endpoints
├── types/
│   ├── index.ts                   # `export type * from './...'`
│   ├── {module}.request.ts        # Type aliases deriving from schemas
│   └── {module}.response.ts       # Plain response interfaces
├── schemas/
│   ├── index.ts                   # Barrel
│   ├── {module}.fields.ts         # Class {Module}Fields( private t: TFunction )
│   └── {module}.forms.ts          # Class {Module}Forms(t), exports type XForm = z.input<...>
├── mappers/
│   ├── index.ts
│   ├── {module}.request.mapper.ts # Static class mapping form → API request
│   └── {module}.response.mapper.ts# Static class mapping API response → domain
├── composables/
│   ├── index.ts
│   └── use{Module}.ts             # Wraps store: computed loading/errors + action methods
├── store/
│   ├── index.ts
│   └── {module}.store.ts          # Pinia store: isLoading, serverErrors, fieldErrors, CRUD
├── components/
│   └── {Entity}Form.vue           # vee-validate + Zod + i18n form component
├── utils/
│   └── {module}.ts                # Constants, helpers
└── pages/
    ├── {Entity}ListPage.vue       # Thin wrapper
    └── {Entity}DetailPage.vue     # Thin wrapper
```

## 2. Routes Pattern

Route names as `const ROUTE` object, NOT separate file:

`routes.ts`:
```typescript
const ROUTE = {
  DASHBOARD: '{module}.dashboard',
  ENTITY: {
    LIST: '{module}.entity.list',
    CREATE: '{module}.entity.create',
    VIEW: '{module}.entity.view',
    EDIT: '{module}.entity.edit',
  },
} as const

export { ROUTE }

export const {module}Routes: RouteRecordRaw = {
  path: '{module}',
  children: [
    { path: '', redirect: { name: ROUTE.DASHBOARD } },
    { path: 'dashboard', name: ROUTE.DASHBOARD, component: () => import('./pages/DashboardPage.vue'), meta: { icon: '...' } },
    // Entity routes use ROUTE.ENTITY.LIST etc
  ],
}
```

## 3. API Pattern

Static class, one per module. Returns raw `Result<T>` / `PagedResult<T>`:

`api/{module}.api.ts`:
```typescript
export class ProductsApi {
  static async getMany(params?: ListParams): Promise<PagedResult<ProductResponse>> {
    const { data } = await apiClient.get('/catalog/products', { params })
    return data
  }
  static async get(id: string): Promise<Result<ProductResponse>> {
    const { data } = await apiClient.get(`/catalog/products/${id}`)
    return data
  }
  static async create(dto: CreateRequest): Promise<Result<ProductResponse>> {
    const { data } = await apiClient.post('/catalog/products', dto)
    return data
  }
  static async update(id: string, dto: UpdateRequest): Promise<Result<ProductResponse>> {
    const { data } = await apiClient.put(`/catalog/products/${id}`, dto)
    return data
  }
  static async delete(id: string): Promise<Result<void>> {
    const { data } = await apiClient.delete(`/catalog/products/${id}`)
    return data
  }
}
```

## 4. Types Pattern

Request types DERIVE from Zod schemas. Response types are plain interfaces.

`types/{module}.response.ts`:
```typescript
export interface ProductResponse {
  id: string
  name: string
  slug: string
  status: 'Draft' | 'Active' | 'Archived'
  // ... all backend fields
}
```

`types/{module}.request.ts`:
```typescript
import type { CreateProductForm, UpdateProductForm } from '../schemas'
export type CreateProductRequest = CreateProductForm
export type UpdateProductRequest = UpdateProductForm
// NEVER redeclare interfaces — always derive from schemas
```

## 5. Schemas Pattern

`schemas/{module}.fields.ts`:
```typescript
import { z } from 'zod'

export type TFunction = (key: string) => string

export class CatalogFields {
  constructor(private t: TFunction) {}

  name() { return z.string().min(1, this.t('catalog.validation.name.required')) }
  slug() { return z.string().min(1, this.t('catalog.validation.slug.required')) }
  description() { return z.string().optional() }
}
```

`schemas/{module}.forms.ts`:
```typescript
import { z } from 'zod'
import { CatalogFields } from './catalog.fields'

export class CatalogForms {
  private f: CatalogFields
  constructor(private t: TFunction) {
    this.f = new CatalogFields(t)
  }

  createProduct() {
    return z.object({
      name: this.f.name(),
      slug: this.f.slug(),
      description: this.f.description(),
      status: z.union([z.literal('Draft'), z.literal('Active'), z.literal('Archived')]).optional(),
    })
  }

  updateProduct() {
    return z.object({
      name: this.f.name().optional(),
      slug: this.f.slug().optional(),
      description: this.f.description(),
    })
  }
}

export type CreateProductForm = z.input<ReturnType<CatalogForms['createProduct']>>
export type UpdateProductForm = z.input<ReturnType<CatalogForms['updateProduct']>>
```

## 6. Mappers Pattern

`mappers/{module}.request.mapper.ts`:
```typescript
import type { CreateProductForm, UpdateProductForm } from '../schemas'
import type { CreateProductRequest, UpdateProductRequest } from '../types'

export class ProductRequestMapper {
  static toCreate(form: CreateProductForm): CreateProductRequest {
    return form  // identity for simple cases
  }
  static toUpdate(form: UpdateProductForm): UpdateProductRequest {
    return form
  }
}
```

## 7. Composable Pattern

`composables/use{Module}.ts`:
```typescript
import { computed } from 'vue'
import { use{Module}Store } from '../store/{module}.store'
import type { CreateProductRequest, UpdateProductRequest } from '../types'

export function use{Module}() {
  const store = use{Module}Store()

  return {
    isLoading: computed(() => store.isLoading),
    serverErrors: computed(() => store.serverErrors),
    fieldErrors: computed(() => store.fieldErrors),
    items: computed(() => store.items),
    current: computed(() => store.current),

    fetchAll: (params?: unknown) => store.fetchAll(params),
    fetchById: (id: string) => store.fetchById(id),
    create: (data: CreateProductRequest) => store.create(data),
    update: (id: string, data: UpdateProductRequest) => store.update(id, data),
    remove: (id: string) => store.remove(id),
  }
}
```

## 8. Store Pattern

`store/{module}.store.ts`:
```typescript
import { ref, readonly } from 'vue'
import { defineStore } from 'pinia'
import { useToast } from '@/shared/composables/useToast'
import type { ApiProblemDetail } from '@/shared/models'
import { ProductsApi } from '../api/products.api'
import type { ProductResponse, CreateProductRequest, UpdateProductRequest } from '../types'

function fieldNameFromCode(code: string): string | null {
  const segments = code.split('.')
  if (segments.length < 2) return null
  const field = segments[1]
  return field ? field.charAt(0).toLowerCase() + field.slice(1) : null
}

function mapErrors(
  errors: ApiProblemDetail[],
  fieldErrors: { value: Record<string, string[]> },
  serverErrors: { value: ApiProblemDetail[] },
) {
  const fields: Record<string, string[]> = {}
  const server: ApiProblemDetail[] = []
  for (const error of errors) {
    server.push(error)
    const mapped = fieldNameFromCode(error.code)
    if (mapped) {
      if (!fields[mapped]) fields[mapped] = []
      fields[mapped].push(error.message)
    }
  }
  fieldErrors.value = fields
  serverErrors.value = server
}

export const useCatalogStore = defineStore('catalog', () => {
  const toast = useToast()
  const isLoading = ref(false)
  const serverErrors = ref<ApiProblemDetail[]>([])
  const fieldErrors = ref<Record<string, string[]>>({})
  const items = ref<ProductResponse[]>([])
  const current = ref<ProductResponse | null>(null)

  function resetFormState() {
    isLoading.value = false
    serverErrors.value = []
    fieldErrors.value = {}
  }

  async function fetchAll(params?: unknown) {
    resetFormState()
    isLoading.value = true
    const result = await ProductsApi.getMany(params)
    if (result.isSuccess) {
      items.value = result.items
    } else {
      mapErrors(result.errors, fieldErrors, serverErrors)
    }
    isLoading.value = false
  }

  async function fetchById(id: string) {
    resetFormState()
    isLoading.value = true
    const result = await ProductsApi.get(id)
    if (result.isSuccess) {
      current.value = result.value
    } else {
      mapErrors(result.errors, fieldErrors, serverErrors)
    }
    isLoading.value = false
  }

  async function create(data: CreateProductRequest) {
    resetFormState()
    isLoading.value = true
    const result = await ProductsApi.create(data)
    if (result.isSuccess) {
      toast.add({ severity: 'success', summary: 'Product created', life: 3000 })
    } else {
      mapErrors(result.errors, fieldErrors, serverErrors)
    }
    isLoading.value = false
    return result
  }

  async function update(id: string, data: UpdateProductRequest) {
    resetFormState()
    isLoading.value = true
    const result = await ProductsApi.update(id, data)
    if (result.isSuccess) {
      toast.add({ severity: 'success', summary: 'Product updated', life: 3000 })
    } else {
      mapErrors(result.errors, fieldErrors, serverErrors)
    }
    isLoading.value = false
    return result
  }

  async function remove(id: string) {
    resetFormState()
    isLoading.value = true
    const result = await ProductsApi.delete(id)
    if (result.isSuccess) {
      toast.add({ severity: 'success', summary: 'Product deleted', life: 3000 })
    } else {
      mapErrors(result.errors, fieldErrors, serverErrors)
    }
    isLoading.value = false
    return result
  }

  return {
    isLoading: readonly(isLoading),
    serverErrors: readonly(serverErrors),
    fieldErrors: readonly(fieldErrors),
    items: readonly(items),
    current: readonly(current),
    fetchAll, fetchById, create, update, remove,
  }
})
```

## 9. Component Pattern

Components use `useForm` + `toTypedSchema` + `useI18n`. All strings via `t()`:

```vue
<script setup lang="ts">
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import { CatalogForms } from '../schemas'
import { ProductRequestMapper } from '../mappers'

const { t } = useI18n()
const schemas = new CatalogForms(t)
const { handleSubmit, defineField, errors } = useForm({
  validationSchema: toTypedSchema(schemas.createProduct()),
})

const [name] = defineField('name')
const [slug] = defineField('slug')
</script>

<template>
  <form @submit="handleSubmit((vals) => emit('save', ProductRequestMapper.toCreate(vals)))">
    <label>{{ t('catalog.products.labels.name') }}</label>
    <InputText v-model="name" :invalid="!!errors.name" />
    <small v-if="errors.name">{{ errors.name }}</small>

    <Button type="submit" :label="t('catalog.actions.create')" />
  </form>
</template>
```

## 10. Page Pattern

Pages are thin wrappers, delegate to form components:

```vue
<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import ProductForm from '../components/ProductForm.vue'

const { t } = useI18n()
const route = useRoute()
</script>

<template>
  <PageHeader :title="t('catalog.products.titles.list')" :icon="route.meta?.icon as string" />
  <ProductForm />
</template>
```

## 11. i18n Rules

- All user-facing strings use `t('{module}.section.key')` from `src/shared/localization/messages/en/`
- NEVER hardcode strings in `.vue` files (titles, labels, placeholders, buttons, toasts, confirm dialogs)
- Zod validation messages come from `this.t('{module}.validation.*')` in fields class
- Existing catalog keys at `src/shared/localization/messages/en/catalog.json`

## 12. Tests

- API tests: mock `apiClient` directly, verify endpoint URL + method + body
- Component tests: mock `vue-i18n` with `useI18n: () => ({ t: (key: string) => key })` (returns key as string for assertion)
- Page tests: mount with PrimeVue + ConfirmationService + ToastService + router plugins
- Store tests: use `setActivePinia(createPinia())`, mock API class directly

## 13. Prohibited Patterns

| Pattern | Why |
|---------|-----|
| `routers/` directory | Auth doesn't have it. Routes stay in `routes.ts` + `ROUTE` const |
| `models/` directory | Use `types/` (auth convention) |
| Function-based API exports | Use static class (auth convention) |
| Hardcoded strings in templates | Must use `t()` from `vue-i18n` |
| `resultToMapped` / `pagedResultToMapped` | Return raw `Result<T>` from API layer (auth convention) |
| Independent request interfaces | Must derive from Zod schemas |
| Inline CRUD logic in pages | Must delegate to store → composable → component |
