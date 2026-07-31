# Admin Catalog Variants — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add standalone Variants CRUD (list + detail) to the admin SPA with flat /catalog/variants routes, including image upload/delete and multi-currency price management.

**Architecture:** Follows existing patterns — stacked API service classes (static methods returning `Result<T>`), flex full-height layout pages, PrimeVue Form + zodResolver, DataTable with `#header` toolbar, standalone page with product context via query param.

**Tech Stack:** Vue 3 + TypeScript, PrimeVue 5 (Form/FormField, Tabs, DataTable), zodResolver, pnpm

## Global Constraints

- `TreatWarningsAsErrors=true` — no warnings allowed in C# build
- All pages use flex full-height layout: `flex flex-col h-full p-4`
- Detail forms: top bar with Save/Cancel, Form wraps Tabs, Save uses `form="..."` + `type="submit"`
- List pages: DataTable `#header` with FloatLabel Search + New/Reload, no batch delete, no Export
- API services: class with static methods, return `Result<T>` or `PagedResult<T>`
- All barrel exports must be updated when adding files
- 570 frontend tests must continue to pass

---

## File Structure

```
app/Admin/src/features/catalog/
├── types/
│   └── variant.ts                        (NEW — all TS interfaces + constants)
├── validations/
│   └── variant.ts                        (NEW — zod schema + form type)
├── services/
│   ├── variantApi.ts                     (NEW — CRUD)
│   ├── variantImageApi.ts                (NEW — images)
│   └── variantPriceApi.ts               (NEW — prices)
├── views/
│   ├── VariantsList.vue                  (NEW — list page)
│   └── VariantDetail.vue                (NEW — detail page)
├── views/ProductsList.vue                (MODIFY — add row "Variants" button)
├── routes/index.ts                       (MODIFY — 3 routes + menu item)
├── types/index.ts                        (MODIFY — variant re-exports)
├── services/index.ts                     (MODIFY — 3 service re-exports)
└── views/index.ts                        (MODIFY — 2 view re-exports)
```

### Interfaces (what each file produces for downstream consumers)

| File | Produces | Consumed by |
|------|----------|-------------|
| `types/variant.ts` | `VariantParameters`, `VariantRequest`, `Variant`, `VariantImage`, `Price`, `OptionValueAssignment`, `VARIANT_FILTER_FIELDS`, `VARIANT_SORT_FIELDS` | variantApi, variantImageApi, variantPriceApi, VariantsList, VariantDetail |
| `validations/variant.ts` | `variantSchema`, `VariantForm` | VariantDetail |
| `services/variantApi.ts` | `VariantApi` class (getVariants, getVariant, createVariant, updateVariant, deleteVariant, getOptionValues) | VariantsList, VariantDetail |
| `services/variantImageApi.ts` | `VariantImageApi` class (listImages, uploadImage, deleteImage) | VariantDetail |
| `services/variantPriceApi.ts` | `VariantPriceApi` class (listPrices, setPrice, removePrice) | VariantDetail |
| `views/VariantsList.vue` | Default component | routes/index.ts |
| `views/VariantDetail.vue` | Default component | routes/index.ts |

---

### Task 1: TypeScript Types

**Files:**
- Create: `app/Admin/src/features/catalog/types/variant.ts`
- Modify: `app/Admin/src/features/catalog/types/index.ts`

**Interfaces:**
- Produces: `VariantParameters`, `VariantRequest`, `Variant`, `VariantImage`, `Price`, `OptionValueAssignment`, `VARIANT_FILTER_FIELDS`, `VARIANT_SORT_FIELDS`

- [ ] **Step 1: Write the types file**

Create `app/Admin/src/features/catalog/types/variant.ts`:

```typescript
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

export interface Variant extends VariantParameters {
  id: string
  productId: string
  isMaster: boolean
  discontinuedOn?: string
  pricesCount: number
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

export const VARIANT_FILTER_FIELDS = [
  'sku',
  'position',
  'isMaster',
  'discontinuedOn',
]

export const VARIANT_SORT_FIELDS = [
  'sku',
  'position',
  'isMaster',
]
```

- [ ] **Step 2: Update barrel export**

In `app/Admin/src/features/catalog/types/index.ts`, append at end before closing:

```typescript
export type {
  VariantParameters,
  VariantRequest,
  Variant,
  VariantImage,
  Price,
  OptionValueAssignment,
} from './variant'
export {
  VARIANT_FILTER_FIELDS,
  VARIANT_SORT_FIELDS,
} from './variant'
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/catalog/types/variant.ts app/Admin/src/features/catalog/types/index.ts
git commit -m "feat(catalog): add Variant TypeScript types"
```

---

### Task 2: Validation Schema

**Files:**
- Create: `app/Admin/src/features/catalog/validations/variant.ts`

**Interfaces:**
- Produces: `variantSchema`, `VariantForm`

- [ ] **Step 1: Write validation file**

Create `app/Admin/src/features/catalog/validations/variant.ts`:

```typescript
import { z } from 'zod'

export const variantSchema = z.object({
  sku: z.string().min(1, 'SKU is required').max(255),
  position: z.number().int().min(-1).default(0),
  isMaster: z.boolean().default(false),
  trackInventory: z.boolean().default(true),
  weight: z.number().min(0).nullable().optional().default(null),
  weightUnit: z.string().nullable().optional().default(null),
  height: z.number().min(0).nullable().optional().default(null),
  width: z.number().min(0).nullable().optional().default(null),
  depth: z.number().min(0).nullable().optional().default(null),
  dimensionsUnit: z.string().nullable().optional().default(null),
  price: z.number().min(0).nullable().optional().default(null),
  costPrice: z.number().min(0).nullable().optional().default(null),
  costCurrency: z.string().max(3).nullable().optional().default(null),
})

export type VariantForm = z.infer<typeof variantSchema>
```

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/features/catalog/validations/variant.ts
git commit -m "feat(catalog): add Variant zod validation schema"
```

---

### Task 3: variantApi Service

**Files:**
- Create: `app/Admin/src/features/catalog/services/variantApi.ts`

**Interfaces:**
- Consumes: `Variant`, `VariantRequest`, `VARIANT_FILTER_FIELDS`, `VARIANT_SORT_FIELDS` from `types/variant`
- Consumes: `post`, `get`, `put`, `del` from `@/shared/api/client`
- Consumes: `Result` from `@/shared/types`
- Consumes: `CATALOG` from `@/shared/constants/api`
- Produces: `VariantApi` class

- [ ] **Step 1: Write the service**

Create `app/Admin/src/features/catalog/services/variantApi.ts`:

```typescript
import { post, get, put, del } from '@/shared/api/client'
import { CATALOG } from '@/shared/constants/api'
import type { Result } from '@/shared/types'
import type {
  VariantRequest,
  Variant,
  OptionValueAssignment,
} from '../types/variant'
import {
  VARIANT_FILTER_FIELDS,
  VARIANT_SORT_FIELDS,
} from '../types/variant'

export class VariantApi {
  private static readonly BASE = `${CATALOG}/variants`

  static getVariants(
    productId: string,
  ): Promise<Result<{ items: Variant[] }>> {
    return get<Result<{ items: Variant[] }>>(
      `${CATALOG}/products/${productId}/variants`,
    )
  }

  static getVariant(id: string): Promise<Result<Variant>> {
    return get<Result<Variant>>(`${VariantApi.BASE}/${id}`)
  }

  static createVariant(
    productId: string,
    request: VariantRequest,
  ): Promise<Result<Variant>> {
    return post<Result<Variant>>(
      `${CATALOG}/products/${productId}/variants`,
      request,
    )
  }

  static updateVariant(
    id: string,
    request: VariantRequest,
  ): Promise<Result<Variant>> {
    return put<Result<Variant>>(`${VariantApi.BASE}/${id}`, request)
  }

  static deleteVariant(id: string): Promise<Result<void>> {
    return del<Result<void>>(`${VariantApi.BASE}/${id}`)
  }

  static getOptionValues(
    variantId: string,
  ): Promise<Result<{ items: OptionValueAssignment[] }>> {
    return get<Result<{ items: OptionValueAssignment[] }>>(
      `${VariantApi.BASE}/${variantId}/option-values`,
    )
  }

  static assignOptionValues(
    variantId: string,
    optionValueIds: string[],
  ): Promise<Result<void>> {
    return post<Result<void>>(
      `${VariantApi.BASE}/${variantId}/option-values/assign`,
      { optionValueIds },
    )
  }

  static revokeOptionValues(
    variantId: string,
    optionValueIds: string[],
  ): Promise<Result<void>> {
    return post<Result<void>>(
      `${VariantApi.BASE}/${variantId}/option-values/revoke`,
      { optionValueIds },
    )
  }
}
```

Wait — `VariantApi.BASE` usage inside static method definitions fails because `BASE` is not yet initialized at parse time for the first method that references it. Fix:

```typescript
import { post, get, put, del } from '@/shared/api/client'
import { CATALOG } from '@/shared/constants/api'
import type { Result } from '@/shared/types'
import type {
  VariantRequest,
  Variant,
  OptionValueAssignment,
} from '../types/variant'

const BASE = `${CATALOG}/variants`

export class VariantApi {
  static getVariants(
    productId: string,
  ): Promise<Result<{ items: Variant[] }>> {
    return get<Result<{ items: Variant[] }>>(
      `${CATALOG}/products/${productId}/variants`,
    )
  }

  static getVariant(id: string): Promise<Result<Variant>> {
    return get<Result<Variant>>(`${BASE}/${id}`)
  }

  static createVariant(
    productId: string,
    request: VariantRequest,
  ): Promise<Result<Variant>> {
    return post<Result<Variant>>(
      `${CATALOG}/products/${productId}/variants`,
      request,
    )
  }

  static updateVariant(
    id: string,
    request: VariantRequest,
  ): Promise<Result<Variant>> {
    return put<Result<Variant>>(`${BASE}/${id}`, request)
  }

  static deleteVariant(id: string): Promise<Result<void>> {
    return del<Result<void>>(`${BASE}/${id}`)
  }

  static getOptionValues(
    variantId: string,
  ): Promise<Result<{ items: OptionValueAssignment[] }>> {
    return get<Result<{ items: OptionValueAssignment[] }>>(
      `${BASE}/${variantId}/option-values`,
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

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/features/catalog/services/variantApi.ts
git commit -m "feat(catalog): add VariantApi service"
```

---

### Task 4: variantImageApi Service

**Files:**
- Create: `app/Admin/src/features/catalog/services/variantImageApi.ts`

**Interfaces:**
- Consumes: `VariantImage` from `types/variant`
- Consumes: `post`, `get`, `del` from `@/shared/api/client`
- Consumes: `Result` from `@/shared/types`
- Consumes: `CATALOG` from `@/shared/constants/api`
- Produces: `VariantImageApi` class

- [ ] **Step 1: Write the service**

Create `app/Admin/src/features/catalog/services/variantImageApi.ts`:

```typescript
import { post, get, del } from '@/shared/api/client'
import { CATALOG } from '@/shared/constants/api'
import type { Result } from '@/shared/types'
import type { VariantImage } from '../types/variant'

const BASE = `${CATALOG}/variants`

export class VariantImageApi {
  static listImages(
    variantId: string,
  ): Promise<Result<{ images: VariantImage[] }>> {
    return get<Result<{ images: VariantImage[] }>>(
      `${BASE}/${variantId}/images`,
    )
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

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/features/catalog/services/variantImageApi.ts
git commit -m "feat(catalog): add VariantImageApi service"
```

---

### Task 5: variantPriceApi Service

**Files:**
- Create: `app/Admin/src/features/catalog/services/variantPriceApi.ts`

**Interfaces:**
- Consumes: `Price` from `types/variant`
- Consumes: `post`, `get`, `del` from `@/shared/api/client`
- Consumes: `Result`, `PagedResult` from `@/shared/types`
- Consumes: `CATALOG` from `@/shared/constants/api`
- Produces: `VariantPriceApi` class

- [ ] **Step 1: Write the service**

Create `app/Admin/src/features/catalog/services/variantPriceApi.ts`:

```typescript
import { post, get, del } from '@/shared/api/client'
import { CATALOG } from '@/shared/constants/api'
import type { Result } from '@/shared/types'
import type { Price } from '../types/variant'

const BASE = `${CATALOG}/variants`

export interface PriceRequest {
  amount?: number
  currency: string
  compareAtAmount?: number
  countryIso?: string
}

export class VariantPriceApi {
  static listPrices(
    variantId: string,
  ): Promise<Result<{ items: Price[] }>> {
    return get<Result<{ items: Price[] }>>(
      `${BASE}/${variantId}/prices`,
    )
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

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/features/catalog/services/variantPriceApi.ts
git commit -m "feat(catalog): add VariantPriceApi service"
```

---

### Task 6: Routes, Menu Item, Barrel Exports

**Files:**
- Modify: `app/Admin/src/features/catalog/routes/index.ts`
- Modify: `app/Admin/src/features/catalog/services/index.ts`
- Modify: `app/Admin/src/features/catalog/views/index.ts`

**Interfaces:**
- Consumes: `VariantsList.vue`, `VariantDetail.vue` (lazy-imported)
- Consumes: `VariantApi`, `VariantImageApi`, `VariantPriceApi` from service files
- Produces: 3 new routes + 1 menu item + barrel re-exports

- [ ] **Step 1: Add lazy imports and routes**

In `app/Admin/src/features/catalog/routes/index.ts`, add lazy import lines AFTER the existing imports:

```typescript
const VariantsList = () => import('../views/VariantsList.vue')
const VariantDetail = () => import('../views/VariantDetail.vue')
```

Add 3 routes to the `catalogRoutes` array, for example after the taxons routes:

```typescript
  {
    path: 'catalog/variants',
    name: 'catalog-variants',
    component: VariantsList,
    meta: { title: 'Variants' },
  },
  {
    path: 'catalog/variants/:id',
    name: 'catalog-variant-detail',
    component: VariantDetail,
    meta: { title: 'Variant Detail' },
  },
  {
    path: 'catalog/variants/new',
    name: 'catalog-variant-new',
    component: VariantDetail,
    meta: { title: 'New Variant' },
  },
```

IMPORTANT: `catalog/variants/new` MUST come AFTER `catalog/variants/:id` so the `:id` route does not match "new" as an id.

- [ ] **Step 2: Add menu item**

In the `catalogMenuItems` array, add `Variants` after `Taxons`:

```typescript
  { label: 'Variants', icon: 'pi pi-fw pi-box', to: '/catalog/variants' },
```

- [ ] **Step 3: Update services barrel**

In `app/Admin/src/features/catalog/services/index.ts`, add:

```typescript
export { VariantApi } from './variantApi'
export { VariantImageApi } from './variantImageApi'
export { VariantPriceApi } from './variantPriceApi'
```

- [ ] **Step 4: Update views barrel**

Read `app/Admin/src/features/catalog/views/index.ts` first, then add re-exports for `VariantsList.vue` and `VariantDetail.vue` following the existing pattern.

- [ ] **Step 5: Verify type-check and build**

Run: `pnpm run type-check` and `pnpm run build-only` from `app/Admin/`.
Expected: 0 type errors, build succeeds.

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/features/catalog/routes/index.ts app/Admin/src/features/catalog/services/index.ts app/Admin/src/features/catalog/views/index.ts
git commit -m "feat(catalog): add Variants routes, menu item, and barrel exports"
```

---

### Task 7: VariantsList Page

**Files:**
- Create: `app/Admin/src/features/catalog/views/VariantsList.vue`

**Interfaces:**
- Consumes: `VariantApi` from `services/variantApi`
- Consumes: `Variant` from `types/variant`
- Consumes: `usePagedQuery` from `@/shared/composables/usePagedQuery`
- Consumes: Router (useRouter, useRoute)
- Produces: Default component exported

- [ ] **Step 1: Write the component script**

Create `app/Admin/src/features/catalog/views/VariantsList.vue`:

```vue
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import { useNotify } from '@/shared/composables/useNotify'
import { VariantApi } from '../services/variantApi'
import type { Variant } from '../types/variant'

const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()

const productId = computed(() => route.query.productId as string | undefined)
const items = ref<Variant[]>([])
const loading = ref(false)
const searchTerm = ref('')

async function loadVariants() {
  if (!productId.value) {
    items.value = []
    return
  }
  loading.value = true
  const result = await VariantApi.getVariants(productId.value)
  if (result.isSuccess && result.value) {
    items.value = result.value.items
  }
  loading.value = false
}

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
}

const filteredItems = computed(() => {
  if (!searchTerm.value) return items.value
  const q = searchTerm.value.toLowerCase()
  return items.value.filter(
    (v) => v.sku.toLowerCase().includes(q),
  )
})

function confirmDelete(variant: Variant) {
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
        await loadVariants()
      } else {
        notify.error('Delete failed', result.errors?.[0]?.message ?? 'Could not delete variant.')
      }
    },
  })
}

function refresh() {
  loadVariants()
}

function clearSearch() {
  searchTerm.value = ''
}

onMounted(() => {
  loadVariants()
})
</script>
```

- [ ] **Step 2: Write the template**

```vue
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

      <DataTable
        v-else
        :value="filteredItems"
        :loading="loading"
        scrollable
        data-key="id"
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
              <Button label="Reload" icon="pi pi-sync" severity="secondary" @click="refresh" />
            </div>
          </div>
        </template>
        <Column field="isMaster" header="Master" body-style="text-align: center">
          <template #body="{ data }">
            <Tag v-if="data.isMaster" value="Master" severity="info" />
            <span v-else class="text-muted-color">—</span>
          </template>
        </Column>
        <Column field="sku" header="SKU">
          <template #body="{ data }">
            <span :class="{ 'text-muted-color': !data.sku }">{{ data.sku || '—' }}</span>
          </template>
        </Column>
        <Column field="position" header="Position" body-style="text-align: center" />
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

- [ ] **Step 3: Verify type-check and build**

Run: `pnpm run type-check && pnpm run build-only` from `app/Admin/`.
Expected: 0 type errors, build succeeds.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/catalog/views/VariantsList.vue
git commit -m "feat(catalog): add VariantsList page"
```

---

### Task 8: VariantDetail Page

**Files:**
- Create: `app/Admin/src/features/catalog/views/VariantDetail.vue`

**Interfaces:**
- Consumes: All three API services, variant types, variant validation, router, PrimeVue Form/Tabs
- Produces: Default component exported

- [ ] **Step 1: Write the script section**

Create `app/Admin/src/features/catalog/views/VariantDetail.vue`:

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
import Card from 'primevue/card'
import Select from 'primevue/select'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { VariantApi } from '../services/variantApi'
import { VariantImageApi } from '../services/variantImageApi'
import { VariantPriceApi } from '../services/variantPriceApi'
import type { PriceRequest } from '../services/variantPriceApi'
import type { VariantForm } from '../validations/variant'
import { variantSchema } from '../validations/variant'
import type { VariantImage, Price, OptionValueAssignment } from '../types/variant'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const confirm = useConfirm()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const productId = computed(() => route.query.productId as string)
const pageTitle = computed(() => isEdit.value ? 'Edit Variant' : 'New Variant')
const pageDescription = computed(() =>
  isEdit.value
    ? 'Edit variant details.'
    : 'Create a new variant.',
)
const activeTab = ref('0')

const resolver = zodResolver(variantSchema)

const form = ref<VariantForm>({
  sku: '',
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
})

const loading = ref(false)
const formLoaded = ref(!isEdit.value)

const weightUnitOptions = [
  { label: 'Gram (g)', value: 'g' },
  { label: 'Kilogram (kg)', value: 'kg' },
  { label: 'Pound (lb)', value: 'lb' },
  { label: 'Ounce (oz)', value: 'oz' },
]

const dimensionsUnitOptions = [
  { label: 'Inch (in)', value: 'in' },
  { label: 'Centimeter (cm)', value: 'cm' },
  { label: 'Millimeter (mm)', value: 'mm' },
]

async function initEditMode(id: string) {
  const result = await VariantApi.getVariant(id)
  if (result.isSuccess) {
    const v = result.value!
    form.value = {
      sku: v.sku,
      position: v.position,
      isMaster: v.isMaster,
      trackInventory: v.trackInventory,
      weight: v.weight ?? null,
      weightUnit: v.weightUnit ?? null,
      height: v.height ?? null,
      width: v.width ?? null,
      depth: v.depth ?? null,
      dimensionsUnit: v.dimensionsUnit ?? null,
      price: v.price ?? null,
      costPrice: v.costPrice ?? null,
      costCurrency: v.costCurrency ?? null,
    }
    formLoaded.value = true
  } else {
    handleResult(result)
    router.push('/catalog/variants')
  }
}

onMounted(() => {
  if (isEdit.value) {
    initEditMode(route.params.id as string)
  }
})

watch(() => route.params.id, (newId) => {
  if (newId && newId !== 'new') {
    formLoaded.value = false
    initEditMode(newId as string)
  }
})

watch(activeTab, (tab) => {
  if (isEdit.value && tab === '3' && images.value.length === 0 && !imagesLoaded.value) {
    loadImages()
  }
  if (isEdit.value && tab === '4' && optionValueAssignments.value.length === 0) {
    loadOptionValues()
  }
})

async function onSubmit(event: FormSubmitEvent) {
  if (!event.valid) return

  const data = event.values as VariantForm
  loading.value = true

  const request = {
    sku: data.sku,
    position: data.position,
    trackInventory: data.trackInventory,
    isMaster: data.isMaster,
    weight: data.weight ?? null,
    weightUnit: data.weightUnit ?? null,
    height: data.height ?? null,
    width: data.width ?? null,
    depth: data.depth ?? null,
    dimensionsUnit: data.dimensionsUnit ?? null,
    price: data.price ?? null,
    costPrice: data.costPrice ?? null,
    costCurrency: data.costCurrency ?? null,
    optionValueIds: isEdit.value
      ? undefined
      : selectedOptionValueIds.value.length > 0
        ? selectedOptionValueIds.value
        : undefined,
  }

  let result
  if (isEdit.value) {
    result = await VariantApi.updateVariant(route.params.id as string, request)
  } else {
    const pid = productId.value
    if (!pid) {
      notify.error('Product ID is required')
      loading.value = false
      return
    }
    result = await VariantApi.createVariant(pid, request)
  }

  loading.value = false

  if (result.isSuccess) {
    if (isEdit.value) {
      const variantId = route.params.id as string
      const originalAssignedIds = optionValueAssignments.value
        .filter((o) => o.isAssigned)
        .map((o) => o.optionValueId)
      const toAssign = selectedOptionValueIds.value.filter(
        (id) => !originalAssignedIds.includes(id),
      )
      const toRevoke = originalAssignedIds.filter(
        (id) => !selectedOptionValueIds.value.includes(id),
      )
      if (toAssign.length > 0) {
        await VariantApi.assignOptionValues(variantId, toAssign)
      }
      if (toRevoke.length > 0) {
        await VariantApi.revokeOptionValues(variantId, toRevoke)
      }
      notify.success('Variant updated')
    } else {
      notify.success('Variant created')
      const created = result.value!
      router.replace(`/catalog/variants/${created.id}?productId=${productId.value}`)
    }
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push(`/catalog/variants${productId.value ? `?productId=${productId.value}` : ''}`)
}

const images = ref<VariantImage[]>([])
const imagesLoaded = ref(false)
const uploadLoading = ref(false)

async function loadImages() {
  if (!isEdit.value) return
  const result = await VariantImageApi.listImages(route.params.id as string)
  if (result.isSuccess && result.value) {
    images.value = result.value.images
    imagesLoaded.value = true
  }
}

function onFileSelect(event: Event) {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file || !isEdit.value) return

  const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp']
  if (!allowedTypes.includes(file.type)) {
    notify.error('Invalid file type', 'Allowed: JPEG, PNG, GIF, WebP')
    return
  }
  if (file.size > 10 * 1024 * 1024) {
    notify.error('File too large', 'File must be under 10 MB')
    return
  }

  uploadImage(file)
  target.value = ''
}

async function uploadImage(file: File) {
  uploadLoading.value = true
  const result = await VariantImageApi.uploadImage(route.params.id as string, file)
  if (result.isSuccess) {
    notify.success('Image uploaded')
    await loadImages()
  } else {
    notify.error('Upload failed', result.errors?.[0]?.message)
  }
  uploadLoading.value = false
}

function confirmDeleteImage(image: VariantImage) {
  confirm.require({
    message: 'This permanently deletes the image. Continue?',
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const result = await VariantImageApi.deleteImage(image.id)
      if (result.isSuccess) {
        notify.success('Image deleted')
        await loadImages()
      } else {
        notify.error('Delete failed', result.errors?.[0]?.message)
      }
    },
  })
}

const optionValueAssignments = ref<OptionValueAssignment[]>([])
const selectedOptionValueIds = ref<string[]>([])
const optionValuesLoading = ref(false)

async function loadOptionValues() {
  if (!isEdit.value) return
  optionValuesLoading.value = true
  const result = await VariantApi.getOptionValues(route.params.id as string)
  if (result.isSuccess && result.value) {
    optionValueAssignments.value = result.value.items
    selectedOptionValueIds.value = result.value.items
      .filter((o) => o.isAssigned)
      .map((o) => o.optionValueId)
  }
  optionValuesLoading.value = false
}

function toggleOptionValue(optionValueId: string) {
  const idx = selectedOptionValueIds.value.indexOf(optionValueId)
  if (idx >= 0) {
    selectedOptionValueIds.value.splice(idx, 1)
  } else {
    selectedOptionValueIds.value.push(optionValueId)
  }
}

const optionValuesByType = computed(() => {
  const groups = new Map<string, OptionValueAssignment[]>()
  for (const ov of optionValueAssignments.value) {
    const key = ov.optionTypeName || ov.optionTypeId
    if (!groups.has(key)) groups.set(key, [])
    groups.get(key)!.push(ov)
  }
  return [...groups.entries()]
})

const prices = ref<Price[]>([])
const pricesLoaded = ref(false)
const priceDialogVisible = ref(false)
const priceForm = ref<PriceRequest>({
  amount: undefined,
  currency: '',
  compareAtAmount: undefined,
  countryIso: undefined,
})

async function loadPrices() {
  if (!isEdit.value) return
  const result = await VariantPriceApi.listPrices(route.params.id as string)
  if (result.isSuccess && result.value) {
    prices.value = result.value.items
    pricesLoaded.value = true
  }
}

watch(activeTab, (tab) => {
  if (isEdit.value && tab === '2' && !pricesLoaded.value) {
    loadPrices()
  }
})

function openPriceDialog() {
  priceForm.value = { amount: undefined, currency: '', compareAtAmount: undefined, countryIso: undefined }
  priceDialogVisible.value = true
}

async function savePrice() {
  if (!priceForm.value.currency) return
  const result = await VariantPriceApi.setPrice(
    route.params.id as string,
    priceForm.value,
  )
  if (result.isSuccess) {
    notify.success('Price saved')
    priceDialogVisible.value = false
    await loadPrices()
  } else {
    notify.error('Failed to save price', result.errors?.[0]?.message)
  }
}

function confirmRemovePrice(price: Price) {
  confirm.require({
    message: 'Remove this price entry?',
    header: 'Confirm',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Remove',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const result = await VariantPriceApi.removePrice(
        route.params.id as string,
        price.id,
      )
      if (result.isSuccess) {
        notify.success('Price removed')
        await loadPrices()
      } else {
        notify.error('Remove failed', result.errors?.[0]?.message)
      }
    },
  })
}
</script>
```

- [ ] **Step 2: Write the template**

```vue
<template>
  <div class="flex flex-col h-full p-4">
    <div class="flex-none flex justify-between items-start gap-4 mb-4">
      <div>
        <div class="font-semibold text-xl">{{ pageTitle }}</div>
        <p v-if="pageDescription" class="text-muted-color mt-1">{{ pageDescription }}</p>
      </div>
      <div class="flex items-center gap-2 shrink-0">
        <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="loading" form="variant-form" />
        <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel" />
      </div>
    </div>

    <div class="flex-1 min-h-0 overflow-auto">
      <Card>
        <template #content>
          <Form id="variant-form" v-slot="$form" :resolver="resolver" :initial-values="form" :key="String(formLoaded)" @submit="onSubmit">
            <Tabs v-model:value="activeTab">
              <TabList>
                <Tab value="0">General</Tab>
                <Tab value="1">Physical</Tab>
                <Tab value="2">Pricing</Tab>
                <Tab v-if="isEdit" value="3">Images</Tab>
                <Tab v-if="isEdit" value="4">Option Values</Tab>
              </TabList>
              <TabPanels>
                <TabPanel value="0">
                  <div class="grid grid-cols-2 gap-4">
                    <FormField v-slot="$field" :resolver="undefined" name="sku" class="flex flex-col gap-1">
                      <label>SKU <span class="text-red-500">*</span></label>
                      <InputText v-model="form.sku" />
                      <small v-if="$field?.invalid" class="text-red-500">{{ $field.error?.message }}</small>
                    </FormField>
                    <FormField v-slot="$field" :resolver="undefined" name="position" class="flex flex-col gap-1">
                      <label>Position</label>
                      <InputNumber v-model="form.position" :min="-1" />
                      <small v-if="$field?.invalid" class="text-red-500">{{ $field.error?.message }}</small>
                    </FormField>
                  </div>
                  <div class="flex gap-8 mt-4">
                    <div class="flex items-center gap-2">
                      <ToggleSwitch v-model="form.isMaster" />
                      <label>Master Variant</label>
                    </div>
                    <div class="flex items-center gap-2">
                      <ToggleSwitch v-model="form.trackInventory" />
                      <label>Track Inventory</label>
                    </div>
                  </div>
                </TabPanel>

                <TabPanel value="1">
                  <div class="grid grid-cols-2 gap-4">
                    <div class="flex gap-2 items-end">
                      <FormField v-slot="$field" :resolver="undefined" name="weight" class="flex flex-col gap-1 flex-1">
                        <label>Weight</label>
                        <InputNumber v-model="form.weight" :min="0" :min-fraction-digits="0" :max-fraction-digits="4" />
                        <small v-if="$field?.invalid" class="text-red-500">{{ $field.error?.message }}</small>
                      </FormField>
                      <div class="flex flex-col gap-1">
                        <label class="text-xs">&nbsp;</label>
                        <Select v-model="form.weightUnit" :options="weightUnitOptions" option-label="label" option-value="value" placeholder="Unit" class="w-36" show-clear />
                      </div>
                    </div>
                    <div />
                    <div class="flex gap-2 items-end">
                      <FormField v-slot="$field" :resolver="undefined" name="height" class="flex flex-col gap-1 flex-1">
                        <label>Height</label>
                        <InputNumber v-model="form.height" :min="0" :min-fraction-digits="0" :max-fraction-digits="4" />
                        <small v-if="$field?.invalid" class="text-red-500">{{ $field.error?.message }}</small>
                      </FormField>
                      <FormField v-slot="$field" :resolver="undefined" name="width" class="flex flex-col gap-1 flex-1">
                        <label>Width</label>
                        <InputNumber v-model="form.width" :min="0" :min-fraction-digits="0" :max-fraction-digits="4" />
                        <small v-if="$field?.invalid" class="text-red-500">{{ $field.error?.message }}</small>
                      </FormField>
                      <FormField v-slot="$field" :resolver="undefined" name="depth" class="flex flex-col gap-1 flex-1">
                        <label>Depth</label>
                        <InputNumber v-model="form.depth" :min="0" :min-fraction-digits="0" :max-fraction-digits="4" />
                        <small v-if="$field?.invalid" class="text-red-500">{{ $field.error?.message }}</small>
                      </FormField>
                      <div class="flex flex-col gap-1">
                        <label class="text-xs">&nbsp;</label>
                        <Select v-model="form.dimensionsUnit" :options="dimensionsUnitOptions" option-label="label" option-value="value" placeholder="Unit" class="w-36" show-clear />
                      </div>
                    </div>
                  </div>
                </TabPanel>

                <TabPanel value="2">
                  <div class="grid grid-cols-3 gap-4 mb-6">
                    <FormField v-slot="$field" :resolver="undefined" name="price" class="flex flex-col gap-1">
                      <label>Base Price</label>
                      <InputNumber v-model="form.price" :min="0" :min-fraction-digits="2" :max-fraction-digits="2" />
                      <small v-if="$field?.invalid" class="text-red-500">{{ $field.error?.message }}</small>
                    </FormField>
                    <FormField v-slot="$field" :resolver="undefined" name="costPrice" class="flex flex-col gap-1">
                      <label>Cost Price</label>
                      <InputNumber v-model="form.costPrice" :min="0" :min-fraction-digits="2" :max-fraction-digits="2" />
                      <small v-if="$field?.invalid" class="text-red-500">{{ $field.error?.message }}</small>
                    </FormField>
                    <FormField v-slot="$field" :resolver="undefined" name="costCurrency" class="flex flex-col gap-1">
                      <label>Currency</label>
                      <InputText v-model="form.costCurrency" placeholder="USD" maxlength="3" />
                      <small v-if="$field?.invalid" class="text-red-500">{{ $field.error?.message }}</small>
                    </FormField>
                  </div>

                  <div v-if="isEdit">
                    <div class="flex items-center justify-between mb-3">
                      <div class="font-semibold">Price History</div>
                      <Button label="Add Price" icon="pi pi-plus" severity="secondary" size="small" @click="openPriceDialog" />
                    </div>
                    <DataTable :value="prices" data-key="id">
                      <Column field="amount" header="Amount" />
                      <Column field="currency" header="Currency" />
                      <Column field="compareAtAmount" header="Compare At" />
                      <Column field="countryIso" header="Country" />
                      <Column header="" body-style="text-align: right; width: 4rem">
                        <template #body="{ data }">
                          <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Remove" @click="confirmRemovePrice(data)" />
                        </template>
                      </Column>
                      <template #empty>
                        <div class="text-center py-4 text-muted-color text-sm">No price entries.</div>
                      </template>
                    </DataTable>
                  </div>
                </TabPanel>

                <TabPanel v-if="isEdit" value="3">
                  <div class="mb-3">
                    <input type="file" accept="image/jpeg,image/png,image/gif,image/webp" class="hidden" ref="fileInput" @change="onFileSelect" />
                    <Button label="Upload Image" icon="pi pi-upload" severity="secondary" :loading="uploadLoading" @click="($refs.fileInput as HTMLInputElement).click()" />
                  </div>
                  <div v-if="images.length === 0" class="text-center py-8 text-muted-color">No images uploaded.</div>
                  <div v-else class="grid grid-cols-4 gap-4">
                    <div v-for="image in images" :key="image.id" class="border rounded-lg overflow-hidden">
                      <img :src="image.url" :alt="image.alt || image.fileName" class="w-full h-32 object-cover" />
                      <div class="p-2 text-xs">
                        <div class="truncate" :title="image.fileName">{{ image.fileName }}</div>
                        <div class="text-muted-color">{{ (image.fileSize / 1024).toFixed(0) }} KB</div>
                        <div class="flex justify-between items-center mt-1">
                          <Tag :value="image.type" severity="info" />
                          <Button icon="pi pi-trash" severity="secondary" text rounded size="small" aria-label="Delete image" @click="confirmDeleteImage(image)" />
                        </div>
                      </div>
                    </div>
                  </div>
                </TabPanel>

                <TabPanel v-if="isEdit" value="4">
                  <div v-if="optionValuesLoading" class="text-center py-4 text-muted-color">Loading option values...</div>
                  <div v-else-if="optionValuesByType.length === 0" class="text-center py-8 text-muted-color">No option types assigned to this product.</div>
                  <div v-else class="space-y-6">
                    <div v-for="[typeName, values] in optionValuesByType" :key="typeName">
                      <div class="font-semibold mb-2">{{ typeName }}</div>
                      <div class="flex flex-wrap gap-3">
                        <div v-for="ov in values" :key="ov.optionValueId" class="flex items-center gap-2">
                          <Checkbox
                            :model-value="selectedOptionValueIds.includes(ov.optionValueId)"
                            :input-id="`ov-${ov.optionValueId}`"
                            @change="toggleOptionValue(ov.optionValueId)"
                          />
                          <label :for="`ov-${ov.optionValueId}`">{{ ov.presentation || ov.name }}</label>
                        </div>
                      </div>
                    </div>
                  </div>
                </TabPanel>
              </TabPanels>
            </Tabs>
          </Form>
        </template>
      </Card>
    </div>

    <Dialog v-model:visible="priceDialogVisible" header="Add Price" :modal="true" :style="{ width: '24rem' }">
      <div class="flex flex-col gap-3">
        <div class="flex flex-col gap-1">
          <label>Currency <span class="text-red-500">*</span></label>
          <InputText v-model="priceForm.currency" placeholder="USD" maxlength="3" />
        </div>
        <div class="flex flex-col gap-1">
          <label>Amount</label>
          <InputNumber v-model="priceForm.amount" :min="0" :min-fraction-digits="2" :max-fraction-digits="2" />
        </div>
        <div class="flex flex-col gap-1">
          <label>Compare At Amount</label>
          <InputNumber v-model="priceForm.compareAtAmount" :min="0" :min-fraction-digits="2" :max-fraction-digits="2" />
        </div>
        <div class="flex flex-col gap-1">
          <label>Country (ISO)</label>
          <InputText v-model="priceForm.countryIso" placeholder="US" maxlength="2" />
        </div>
      </div>
      <template #footer>
        <Button label="Cancel" severity="secondary" @click="priceDialogVisible = false" />
        <Button label="Save" severity="primary" @click="savePrice" />
      </template>
    </Dialog>
  </div>
</template>
```

- [ ] **Step 2: Fix the file input ref**

Replace `<input ... ref="fileInput" />` and the `@click` with a simpler pattern — use a local `ref` for the file input element:

Update the script section — add: `const fileInputRef = ref<HTMLInputElement>()`

And update the template file input section:
```html
<div class="mb-3">
  <input type="file" accept="image/jpeg,image/png,image/gif,image/webp" class="hidden" ref="fileInputRef" @change="onFileSelect" />
  <Button label="Upload Image" icon="pi pi-upload" severity="secondary" :loading="uploadLoading" @click="fileInputRef?.click()" />
</div>
```

- [ ] **Step 3: Verify type-check and build**

Run: `pnpm run type-check && pnpm run build-only` from `app/Admin/`.
Expected: 0 type errors, build succeeds.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/catalog/views/VariantDetail.vue
git commit -m "feat(catalog): add VariantDetail page with tabs for general, physical, pricing, images, option values"
```

---

### Task 9: ProductsList Variants Button

**Files:**
- Modify: `app/Admin/src/features/catalog/views/ProductsList.vue`

**Interfaces:**
- Consumes: Router from existing component
- Produces: "Variants" button in each product row

- [ ] **Step 1: Add navigate function**

In the script section of `ProductsList.vue`, add:

```typescript
function navigateToVariants(productId: string) {
  router.push(`/catalog/variants?productId=${productId}`)
}
```

- [ ] **Step 2: Add Variants button to the actions column**

The current actions column (line 157-163) has edit + delete buttons. Add a Variants button between:

```html
<Column header="" body-style="text-align: right; width: 9rem">
  <template #body="{ data }">
    <div class="flex justify-end gap-2">
      <Button icon="pi pi-box" severity="secondary" text rounded aria-label="Variants" @click="navigateToVariants(data.id)" />
      <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
      <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [data]; confirmDelete()" />
    </div>
  </template>
</Column>
```

Note: increase `width: 6rem` to `width: 9rem` to accommodate the extra button.

- [ ] **Step 3: Verify type-check and build**

Run: `pnpm run type-check && pnpm run build-only` from `app/Admin/`.
Expected: 0 type errors, build succeeds.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/catalog/views/ProductsList.vue
git commit -m "feat(catalog): add Variants row button to ProductsList"
```

---

### Task 10: Full Verification

**Files:** No new files — verify all changes end-to-end.

- [ ] **Step 1: Run type-check**

```bash
cd app/Admin && pnpm run type-check
```
Expected: 0 errors.

- [ ] **Step 2: Run lint**

```bash
cd app/Admin && pnpm run lint
```
Expected: 0 errors, 0 warnings.

- [ ] **Step 3: Run unit tests**

```bash
cd app/Admin && pnpm run test:unit -- run
```
Expected: All 570 tests pass (no regressions).

- [ ] **Step 4: Run C# build (warnings-as-errors)**

```bash
dotnet build service/Api
```
Expected: Build succeeds with 0 warnings.

- [ ] **Step 5: Commit (if anything was fixed)**

If any fixes were needed for type/lint/test/build:
```bash
git add -A
git commit -m "fix(catalog): resolve verification issues for Variants feature"
```
