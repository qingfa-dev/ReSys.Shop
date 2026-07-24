# Universal Query Params Converter — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a shared `toQueryParams()` serializer and `getPagedList()` helper that all features use to convert `ListQuery` into dot-notation API query params, plus migrate the catalog feature to use it with Pinia stores.

**Architecture:** A `ListQuery` interface (frontend-facing) is serialized by a single `toQueryParams()` function into the structured dot-notation format the C# backend expects (`page.page`, `filter.root.logic`, `sort.clauses[0].field`, etc.). A thin `getPagedList()` wrapper around `apiClient.get` applies the serializer automatically. Feature stores own pagination state and expose `setPage`/`setSearch`/`setSort`/`setFilter` actions. Feature API classes call `getPagedList()`.

**Tech Stack:** TypeScript, Vue 3, Pinia, Axios, Vitest

## Global Constraints

- All new types in `shared/models/list-query.ts`
- All new functions in `shared/api/utils/query-serializer.ts`
- No changes to existing `shared/models/querying.ts` — reuse its sub-types as-is
- Feature stores live in `features/{module}/store/` as Pinia composition API stores
- List table components consume stores, never call `ProductApi` directly
- Tests for `query-serializer` live in `shared/api/utils/__tests__/query-serializer.spec.ts`
- API class tests keep mocking apiClient, just update expected params

---

## File Structure

### New files
- `src/shared/models/list-query.ts` — `ListQuery` interface + `defaultListQuery()` factory
- `src/shared/api/utils/query-serializer.ts` — `toQueryParams()` + `getPagedList()` + `getPagedList()`
- `src/shared/api/utils/__tests__/query-serializer.spec.ts` — serializer unit tests
- `src/features/catalog/store/product.store.ts` — `useProductStore`
- `src/features/catalog/store/taxonomy.store.ts` — `useTaxonomyStore`
- `src/features/catalog/store/option-type.store.ts` — `useOptionTypeStore`

### Modified files
- `src/shared/models/index.ts` — add `ListQuery` + `defaultListQuery` export
- `src/features/catalog/types/product.request.ts` — remove `ProductListParams`
- `src/features/catalog/types/taxonomy.request.ts` — remove `TaxonomyListParams`
- `src/features/catalog/types/option-type.request.ts` — remove `OptionTypeListParams`
- `src/features/catalog/api/product.api.ts` — use `getPagedList` with `ListQuery`
- `src/features/catalog/api/taxonomy.api.ts` — use `getPagedList` with `ListQuery`
- `src/features/catalog/api/option-type.api.ts` — use `getPagedList` with `ListQuery`
- `src/features/catalog/api/__tests__/products.spec.ts` — update param expectations
- `src/features/catalog/api/__tests__/taxonomies.spec.ts` — update param expectations
- `src/features/catalog/api/__tests__/optionTypes.spec.ts` — update param expectations
- `src/features/catalog/components/ProductListTable.vue` — use `useProductStore`
- `src/features/catalog/components/TaxonomyListTable.vue` — use `useTaxonomyStore`
- `src/features/catalog/components/OptionTypeListTable.vue` — use `useOptionTypeStore`
- `src/features/catalog/index.ts` — add store exports

---

### Task 1: Create `ListQuery` model

**Files:**
- Create: `src/shared/models/list-query.ts`
- Modify: `src/shared/models/index.ts`

**Interfaces:**
- Produces: `ListQuery` interface, `defaultListQuery()` factory function

- [ ] **Step 1: Create `list-query.ts`**

```ts
// shared/models/list-query.ts
import type { FilterGroup, SortClause } from './querying'

export interface ListQuery {
  page: number
  pageSize: number
  search?: {
    value: string
    fields?: string[]
    mode?: 'Any' | 'All'
    caseSensitive?: boolean
  }
  sort?: SortClause[]
  filters?: FilterGroup
}

export function defaultListQuery(pageSize = 20): ListQuery {
  return { page: 1, pageSize, sort: [{ field: 'createdAt', direction: 'Descending' }] }
}
```

- [ ] **Step 2: Export from `shared/models/index.ts`**

Add to existing `index.ts` barrel:

```ts
export type { ListQuery } from './list-query'
export { defaultListQuery } from './list-query'
```

- [ ] **Step 3: Commit**

```bash
git add src/shared/models/list-query.ts src/shared/models/index.ts
git commit -m "feat(admin): add ListQuery model and defaultListQuery factory"
```

---

### Task 2: Create `toQueryParams` serializer and `getPagedList` helper

**Files:**
- Create: `src/shared/api/utils/query-serializer.ts`

**Interfaces:**
- Consumes: `ListQuery` (from Task 1), `FilterGroup` (from existing `querying.ts`)
- Produces: `toQueryParams(query: ListQuery): Record<string, string | number | undefined>`, `getPagedList<T>(url, query): Promise<PagedResult<T>>`

- [ ] **Step 1: Create `query-serializer.ts`**

```ts
// shared/api/utils/query-serializer.ts
import apiClient from '@/shared/api/client'
import type { ListQuery } from '@/shared/models'
import type { PagedResult } from '@/shared/models'
import type { FilterGroup } from '@/shared/models/querying'

function flattenFilter(group: FilterGroup, prefix: string): Record<string, string> {
  const params: Record<string, string> = {}
  params[`${prefix}.logic`] = group.logic
  group.conditions.forEach((c, i) => {
    params[`${prefix}.conditions[${i}].field`] = c.field
    params[`${prefix}.conditions[${i}].operator`] = c.operator
    params[`${prefix}.conditions[${i}].value`] = c.value
  })
  group.groups?.forEach((g, i) => {
    Object.assign(params, flattenFilter(g, `${prefix}.groups[${i}]`))
  })
  return params
}

export function toQueryParams(query: ListQuery): Record<string, string | number | undefined> {
  return {
    'page.page': query.page,
    'page.pageSize': query.pageSize,
    'search.term.value': query.search?.value,
    'search.term.caseSensitive': query.search?.caseSensitive,
    'search.fields': query.search?.fields?.join(','),
    'search.mode': query.search?.mode,
    ...query.sort?.reduce((acc, s, i) => ({
      ...acc,
      [`sort.clauses[${i}].field`]: s.field,
      [`sort.clauses[${i}].direction`]: s.direction,
      ...(s.nulls && { [`sort.clauses[${i}].nulls`]: s.nulls }),
    }), {}),
    ...(query.filters ? flattenFilter(query.filters, 'filter.root') : {}),
  }
}

export async function getPagedList<T>(url: string, query: ListQuery): Promise<PagedResult<T>> {
  const res = await apiClient.get<PagedResult<T>>(url, { params: toQueryParams(query) })
  return res.data
}
```

- [ ] **Step 2: Commit**

```bash
git add src/shared/api/utils/query-serializer.ts
git commit -m "feat(admin): add toQueryParams serializer and getPagedList helper"
```

---

### Task 3: Write serializer tests

**Files:**
- Create: `src/shared/api/utils/__tests__/query-serializer.spec.ts`

**Interfaces:**
- Consumes: `toQueryParams`, `getPagedList` (from Task 2)

- [ ] **Step 1: Create test file**

```ts
// shared/api/utils/__tests__/query-serializer.spec.ts
import { describe, it, expect } from 'vitest'
import { toQueryParams } from '../query-serializer'
import type { ListQuery } from '@/shared/models'

describe('toQueryParams', () => {
  it('serializes page and pageSize', () => {
    const params = toQueryParams({ page: 2, pageSize: 50 })
    expect(params).toMatchObject({ 'page.page': 2, 'page.pageSize': 50 })
  })

  it('serializes default sort', () => {
    const params = toQueryParams({ page: 1, pageSize: 20, sort: [{ field: 'name', direction: 'Ascending' }] })
    expect(params).toMatchObject({ 'sort.clauses[0].field': 'name', 'sort.clauses[0].direction': 'Ascending' })
  })

  it('serializes search term', () => {
    const params = toQueryParams({
      page: 1, pageSize: 20,
      search: { value: 'red', fields: ['name', 'slug'], mode: 'Any' },
    })
    expect(params).toMatchObject({
      'search.term.value': 'red',
      'search.fields': 'name,slug',
      'search.mode': 'Any',
    })
  })

  it('serializes filter group with conditions', () => {
    const query: ListQuery = {
      page: 1, pageSize: 20,
      filters: {
        logic: 'And',
        conditions: [{ field: 'status', operator: 'Equal', value: 'Active' }],
      },
    }
    const params = toQueryParams(query)
    expect(params).toMatchObject({
      'filter.root.logic': 'And',
      'filter.root.conditions[0].field': 'status',
      'filter.root.conditions[0].operator': 'Equal',
      'filter.root.conditions[0].value': 'Active',
    })
  })

  it('serializes nested filter groups', () => {
    const query: ListQuery = {
      page: 1, pageSize: 20,
      filters: {
        logic: 'Or',
        conditions: [{ field: 'status', operator: 'Equal', value: 'Active' }],
        groups: [{
          logic: 'And',
          conditions: [{ field: 'price', operator: 'GreaterThan', value: '100' }],
        }],
      },
    }
    const params = toQueryParams(query)
    expect(params).toMatchObject({
      'filter.root.logic': 'Or',
      'filter.root.conditions[0].field': 'status',
      'filter.root.groups[0].logic': 'And',
      'filter.root.groups[0].conditions[0].field': 'price',
    })
  })

  it('omits undefined fields', () => {
    const params = toQueryParams({ page: 1, pageSize: 20 })
    expect(params).not.toHaveProperty('search.term.value')
    expect(params).not.toHaveProperty('sort.clauses')
    expect(params).not.toHaveProperty('filter.root')
  })

  it('serializes sort with nulls', () => {
    const params = toQueryParams({
      page: 1, pageSize: 20,
      sort: [{ field: 'name', direction: 'Ascending', nulls: 'Last' }],
    })
    expect(params).toMatchObject({ 'sort.clauses[0].nulls': 'Last' })
  })

  it('serializes search with caseSensitive', () => {
    const params = toQueryParams({
      page: 1, pageSize: 20,
      search: { value: 'Red', caseSensitive: true },
    })
    expect(params).toMatchObject({ 'search.term.caseSensitive': true })
  })
})
```

- [ ] **Step 2: Run tests to verify they pass**

Run: `npx vitest run src/shared/api/utils/__tests__/query-serializer.spec.ts`
Expected: All 8 tests PASS

- [ ] **Step 3: Commit**

```bash
git add src/shared/api/utils/__tests__/query-serializer.spec.ts
git commit -m "test(admin): add toQueryParams serializer unit tests"
```

---

### Task 4: Create catalog feature stores

**Files:**
- Create: `src/features/catalog/store/product.store.ts`
- Create: `src/features/catalog/store/taxonomy.store.ts`
- Create: `src/features/catalog/store/option-type.store.ts`

**Interfaces:**
- Consumes: `PagedResult` (existing), `ListQuery` / `defaultListQuery` (Task 1), `ProductApi` / `TaxonomyApi` / `OptionTypeApi` (existing, will be updated in Task 5)
- Produces: `useProductStore`, `useTaxonomyStore`, `useOptionTypeStore` — each with `items`, `loading`, `error`, `totalRecords`, `query`, `fetchMany`, `setPage`, `setSearch`, `setSort`, `setFilter`, `resetQuery`

- [ ] **Step 1: Create `product.store.ts`**

```ts
// features/catalog/store/product.store.ts
import { ref, readonly } from 'vue'
import { defineStore } from 'pinia'
import { defaultListQuery } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { ProductResponse } from '../types'
import { ProductApi } from '../api/product.api'
import type { FilterGroup } from '@/shared/models/querying'

export const useProductStore = defineStore('catalog-product', () => {
  const items = ref<ProductResponse[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const totalRecords = ref(0)
  const query = ref<ListQuery>(defaultListQuery())

  async function fetchMany() {
    loading.value = true
    error.value = null
    try {
      const result = await ProductApi.getMany(query.value)
      items.value = result.items ?? []
      totalRecords.value = result.totalCount ?? 0
    } catch {
      error.value = 'Failed to load'
    }
    loading.value = false
  }

  function setPage(page: number) { query.value.page = page; return fetchMany() }
  function setSearch(value: string) {
    query.value.search = { value, mode: 'Any' }
    query.value.page = 1
    return fetchMany()
  }
  function setSort(field: string, direction: 'Asc' | 'Desc') {
    query.value.sort = [{ field, direction }]
    return fetchMany()
  }
  function setFilter(group: FilterGroup) {
    query.value.filters = group
    query.value.page = 1
    return fetchMany()
  }
  function resetQuery() { query.value = defaultListQuery(); return fetchMany() }

  return {
    items: readonly(items), loading: readonly(loading),
    error: readonly(error), totalRecords: readonly(totalRecords),
    query: readonly(query),
    fetchMany, setPage, setSearch, setSort, setFilter, resetQuery,
  }
})
```

- [ ] **Step 2: Create `taxonomy.store.ts`**

```ts
// features/catalog/store/taxonomy.store.ts
import { ref, readonly } from 'vue'
import { defineStore } from 'pinia'
import { defaultListQuery } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { TaxonomyResponse } from '../types'
import { TaxonomyApi } from '../api/taxonomy.api'
import type { FilterGroup } from '@/shared/models/querying'

export const useTaxonomyStore = defineStore('catalog-taxonomy', () => {
  const items = ref<TaxonomyResponse[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const totalRecords = ref(0)
  const query = ref<ListQuery>(defaultListQuery())

  async function fetchMany() {
    loading.value = true
    error.value = null
    try {
      const result = await TaxonomyApi.getMany(query.value)
      items.value = result.items ?? []
      totalRecords.value = result.totalCount ?? 0
    } catch {
      error.value = 'Failed to load'
    }
    loading.value = false
  }

  function setPage(page: number) { query.value.page = page; return fetchMany() }
  function setSearch(value: string) {
    query.value.search = { value, mode: 'Any' }
    query.value.page = 1
    return fetchMany()
  }
  function setSort(field: string, direction: 'Asc' | 'Desc') {
    query.value.sort = [{ field, direction }]
    return fetchMany()
  }
  function setFilter(group: FilterGroup) {
    query.value.filters = group
    query.value.page = 1
    return fetchMany()
  }
  function resetQuery() { query.value = defaultListQuery(); return fetchMany() }

  return {
    items: readonly(items), loading: readonly(loading),
    error: readonly(error), totalRecords: readonly(totalRecords),
    query: readonly(query),
    fetchMany, setPage, setSearch, setSort, setFilter, resetQuery,
  }
})
```

- [ ] **Step 3: Create `option-type.store.ts`**

```ts
// features/catalog/store/option-type.store.ts
import { ref, readonly } from 'vue'
import { defineStore } from 'pinia'
import { defaultListQuery } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { OptionTypeResponse } from '../types'
import { OptionTypeApi } from '../api/option-type.api'
import type { FilterGroup } from '@/shared/models/querying'

export const useOptionTypeStore = defineStore('catalog-option-type', () => {
  const items = ref<OptionTypeResponse[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const totalRecords = ref(0)
  const query = ref<ListQuery>(defaultListQuery())

  async function fetchMany() {
    loading.value = true
    error.value = null
    try {
      const result = await OptionTypeApi.getMany(query.value)
      items.value = result.items ?? []
      totalRecords.value = result.totalCount ?? 0
    } catch {
      error.value = 'Failed to load'
    }
    loading.value = false
  }

  function setPage(page: number) { query.value.page = page; return fetchMany() }
  function setSearch(value: string) {
    query.value.search = { value, mode: 'Any' }
    query.value.page = 1
    return fetchMany()
  }
  function setSort(field: string, direction: 'Asc' | 'Desc') {
    query.value.sort = [{ field, direction }]
    return fetchMany()
  }
  function setFilter(group: FilterGroup) {
    query.value.filters = group
    query.value.page = 1
    return fetchMany()
  }
  function resetQuery() { query.value = defaultListQuery(); return fetchMany() }

  return {
    items: readonly(items), loading: readonly(loading),
    error: readonly(error), totalRecords: readonly(totalRecords),
    query: readonly(query),
    fetchMany, setPage, setSearch, setSort, setFilter, resetQuery,
  }
})
```

- [ ] **Step 4: Update `features/catalog/index.ts` to export stores**

```ts
export { useProductStore } from './store/product.store'
export { useTaxonomyStore } from './store/taxonomy.store'
export { useOptionTypeStore } from './store/option-type.store'
```

- [ ] **Step 5: Commit**

```bash
git add src/features/catalog/store/ src/features/catalog/index.ts
git commit -m "feat(admin): add catalog Pinia stores for paginated list state"
```

---

### Task 5: Migrate catalog API classes to `getPagedList`

**Files:**
- Modify: `src/features/catalog/api/product.api.ts`
- Modify: `src/features/catalog/api/taxonomy.api.ts`
- Modify: `src/features/catalog/api/option-type.api.ts`

**Interfaces:**
- Consumes: `getPagedList` (Task 2), `ListQuery` (Task 1)
- Produces: Updated `ProductApi.getMany(query: ListQuery)`, `TaxonomyApi.getMany(query: ListQuery)`, `OptionTypeApi.getMany(query: ListQuery)`

- [ ] **Step 1: Update `product.api.ts`**

Replace `getMany` method:

```ts
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { ProductResponse, CreateProductRequest, UpdateProductRequest } from '../types'

export class ProductApi {
  static getMany(query: ListQuery): Promise<PagedResult<ProductResponse>> {
    return getPagedList<ProductResponse>('/catalog/products', query)
  }
  // get, create, update, delete stay unchanged
}
```

Remove unused import: `import apiClient from '@/shared/api/client'`
Remove unused type imports: `ProductListParams`

- [ ] **Step 2: Update `taxonomy.api.ts`**

Same pattern — replace `getMany`:

```ts
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { TaxonomyResponse, CreateTaxonomyRequest, UpdateTaxonomyRequest } from '../types'

export class TaxonomyApi {
  static getMany(query: ListQuery): Promise<PagedResult<TaxonomyResponse>> {
    return getPagedList<TaxonomyResponse>('/catalog/taxonomies', query)
  }
  // get, create, update, delete stay unchanged
}
```

- [ ] **Step 3: Update `option-type.api.ts`**

```ts
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { PagedResult } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { OptionTypeResponse, CreateOptionTypeRequest, UpdateOptionTypeRequest } from '../types'

export class OptionTypeApi {
  static getMany(query: ListQuery): Promise<PagedResult<OptionTypeResponse>> {
    return getPagedList<OptionTypeResponse>('/catalog/option-types', query)
  }
  // get, create, update, delete stay unchanged
}
```

- [ ] **Step 4: Commit**

```bash
git add src/features/catalog/api/product.api.ts src/features/catalog/api/taxonomy.api.ts src/features/catalog/api/option-type.api.ts
git commit -m "refactor(admin): migrate catalog API getMany to use getPagedList with ListQuery"
```

---

### Task 6: Migrate list table components to use stores

**Files:**
- Modify: `src/features/catalog/components/ProductListTable.vue`
- Modify: `src/features/catalog/components/TaxonomyListTable.vue`
- Modify: `src/features/catalog/components/OptionTypeListTable.vue`

- [ ] **Step 1: Update `ProductListTable.vue`**

Replace component-scoped refs with store:

```vue
<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Column from 'primevue/column'
import DataTable from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import StatusTag from '@/shared/components/data/StatusTag.vue'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useI18n } from 'vue-i18n'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { useProductStore } from '../store/product.store'
import { ProductApi } from '../api'
import { ROUTE } from '../routes'

const router = useRouter()
const { confirmDelete } = useConfirm()
const toast = useToast()
const { t } = useI18n()
const store = useProductStore()

onMounted(() => store.fetchMany())

function goToCreate() { router.push({ name: ROUTE.PRODUCTS.CREATE }) }
function goToView(id: string) { router.push({ name: ROUTE.PRODUCTS.VIEW, params: { id } }) }
function goToEdit(id: string) { router.push({ name: ROUTE.PRODUCTS.EDIT, params: { id } }) }

async function onDelete(id: string) {
  confirmDelete({
    target: 'this product',
    onAccept: async () => {
      const result = await ProductApi.delete(id)
      if (result.isSuccess) { toast.success(t('catalog.products.messages.delete_success')); await store.fetchMany() }
      else { toast.error(result.message ?? 'Failed to delete') }
    },
  })
}

function onSearch(value: string) { store.setSearch(value) }
function onPageChange(e: { page: number; rows: number }) { store.setPage(e.page + 1) }
</script>

<template>
  <div>
    <TableToolbar
      :search-placeholder="t('catalog.products.placeholders.search')"
      :create-label="t('catalog.products.actions.new')"
      @search="onSearch"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="store.loading.value && store.items.value.length === 0" :rows="5" :columns="4" />
    <ErrorState v-else-if="store.error.value" :description="store.error.value" @retry="store.fetchMany" />
    <EmptyState v-else-if="store.items.value.length === 0" :title="t('catalog.products.messages.empty_list')" description="Create your first product." />
    <DataTable
      v-else
      :rows="store.items.value"
      :loading="store.loading.value"
      :total-records="store.totalRecords.value"
      :page-size="store.query.value.pageSize"
      :first="(store.query.value.page - 1) * store.query.value.pageSize"
      @page="onPageChange"
    >
      <Column field="name" header="Name" sortable />
      <Column field="slug" header="Slug" />
      <Column field="status" header="Status">
        <template #body="slotProps">
          <StatusTag :status="slotProps.data.status" />
        </template>
      </Column>
      <Column field="createdAt" header="Created" />
      <template #rowActions="{ data }">
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

- [ ] **Step 2: Update `TaxonomyListTable.vue`**

Same pattern — replace `ref<...>` with `useTaxonomyStore()`:

```vue
<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Column from 'primevue/column'
import DataTable from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useI18n } from 'vue-i18n'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { useTaxonomyStore } from '../store/taxonomy.store'
import { TaxonomyApi } from '../api'
import { ROUTE } from '../routes'

const router = useRouter()
const { confirmDelete } = useConfirm()
const toast = useToast()
const { t } = useI18n()
const store = useTaxonomyStore()

onMounted(() => store.fetchMany())

function goToCreate() { router.push({ name: ROUTE.TAXONOMIES.CREATE }) }
function goToView(id: string) { router.push({ name: ROUTE.TAXONOMIES.VIEW, params: { id } }) }
function goToEdit(id: string) { router.push({ name: ROUTE.TAXONOMIES.EDIT, params: { id } }) }

async function onDelete(id: string) {
  confirmDelete({
    target: 'this taxonomy',
    onAccept: async () => {
      const result = await TaxonomyApi.delete(id)
      if (result.isSuccess) { toast.success(t('catalog.taxonomies.messages.delete_success')); await store.fetchMany() }
      else { toast.error(result.message ?? 'Failed to delete') }
    },
  })
}

function onSearch(value: string) { store.setSearch(value) }
function onPageChange(e: { page: number; rows: number }) { store.setPage(e.page + 1) }
</script>

<template>
  <div>
    <TableToolbar
      search-placeholder="Search taxonomies..."
      :create-label="t('catalog.taxonomies.actions.create')"
      @search="onSearch"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="store.loading.value && store.items.value.length === 0" :rows="5" :columns="4" />
    <ErrorState v-else-if="store.error.value" :description="store.error.value" @retry="store.fetchMany" />
    <EmptyState v-else-if="store.items.value.length === 0" :title="t('catalog.taxonomies.messages.empty_list')" description="Create your first taxonomy." />
    <DataTable
      v-else
      :rows="store.items.value"
      :loading="store.loading.value"
      :total-records="store.totalRecords.value"
      :page-size="store.query.value.pageSize"
      :first="(store.query.value.page - 1) * store.query.value.pageSize"
      @page="onPageChange"
    >
      <Column field="name" header="Name" sortable />
      <Column field="presentation" header="Presentation" />
      <Column field="position" header="Position" />
      <Column field="createdAt" header="Created" />
      <template #rowActions="{ data }">
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

- [ ] **Step 3: Update `OptionTypeListTable.vue`**

Same pattern — replace `ref<...>` with `useOptionTypeStore()`:

```vue
<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Column from 'primevue/column'
import DataTable from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useI18n } from 'vue-i18n'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { useOptionTypeStore } from '../store/option-type.store'
import { OptionTypeApi } from '../api'
import { ROUTE } from '../routes'

const router = useRouter()
const { confirmDelete } = useConfirm()
const toast = useToast()
const { t } = useI18n()
const store = useOptionTypeStore()

onMounted(() => store.fetchMany())

function goToCreate() { router.push({ name: ROUTE.OPTION_TYPES.CREATE }) }
function goToView(id: string) { router.push({ name: ROUTE.OPTION_TYPES.VIEW, params: { id } }) }
function goToEdit(id: string) { router.push({ name: ROUTE.OPTION_TYPES.EDIT, params: { id } }) }

async function onDelete(id: string) {
  confirmDelete({
    target: 'this option type',
    onAccept: async () => {
      const result = await OptionTypeApi.delete(id)
      if (result.isSuccess) { toast.success(t('catalog.option_types.messages.delete_success')); await store.fetchMany() }
      else { toast.error(result.message ?? 'Failed to delete') }
    },
  })
}

function onSearch(value: string) { store.setSearch(value) }
function onPageChange(e: { page: number; rows: number }) { store.setPage(e.page + 1) }
</script>

<template>
  <div>
    <TableToolbar
      search-placeholder="Search option types..."
      :create-label="t('catalog.option_types.actions.create')"
      @search="onSearch"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="store.loading.value && store.items.value.length === 0" :rows="5" :columns="4" />
    <ErrorState v-else-if="store.error.value" :description="store.error.value" @retry="store.fetchMany" />
    <EmptyState v-else-if="store.items.value.length === 0" :title="t('catalog.option_types.messages.empty_list')" description="Create your first option type." />
    <DataTable
      v-else
      :rows="store.items.value"
      :loading="store.loading.value"
      :total-records="store.totalRecords.value"
      :page-size="store.query.value.pageSize"
      :first="(store.query.value.page - 1) * store.query.value.pageSize"
      @page="onPageChange"
    >
      <Column field="name" header="Name" sortable />
      <Column field="presentation" header="Presentation" />
      <Column field="filterable" header="Filterable">
        <template #body="{ data }">
          <i v-if="data.filterable" class="pi pi-check" style="color: var(--p-green-500)" />
          <i v-else class="pi pi-times" style="color: var(--p-red-400)" />
        </template>
      </Column>
      <Column field="position" header="Position" />
      <template #rowActions="{ data }">
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

- [ ] **Step 4: Commit**

```bash
git add src/features/catalog/components/ProductListTable.vue src/features/catalog/components/TaxonomyListTable.vue src/features/catalog/components/OptionTypeListTable.vue
git commit -m "refactor(admin): migrate catalog list tables to use Pinia stores"
```

---

### Task 7: Update API tests and remove old `*ListParams` types

**Files:**
- Modify: `src/features/catalog/api/__tests__/products.spec.ts`
- Modify: `src/features/catalog/api/__tests__/taxonomies.spec.ts`
- Modify: `src/features/catalog/api/__tests__/optionTypes.spec.ts`
- Modify: `src/features/catalog/types/product.request.ts`
- Modify: `src/features/catalog/types/taxonomy.request.ts`
- Modify: `src/features/catalog/types/option-type.request.ts`

- [ ] **Step 1: Update `products.spec.ts`**

Replace the `getMany` test to verify serialized params:

```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { ProductApi } from '../product.api'

vi.mock('@/shared/api/client', () => ({
  default: {
    get: vi.fn<(...args: unknown[]) => unknown>(),
    post: vi.fn<(...args: unknown[]) => unknown>(),
    put: vi.fn<(...args: unknown[]) => unknown>(),
    delete: vi.fn<(...args: unknown[]) => unknown>(),
  },
}))

const pagedEmpty = { isSuccess: true, items: [], page: 1, pageSize: 20, totalCount: 0, statusCode: 200 }
const singleOk = (value: unknown) => ({ isSuccess: true, value, statusCode: 200 })
const defaultQuery = { page: 1, pageSize: 20, sort: [{ field: 'createdAt', direction: 'Descending' as const }] }

describe('ProductApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('getMany: GET /catalog/products with serialized query params', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
    await ProductApi.getMany(defaultQuery)
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/products', {
      params: {
        'page.page': 1,
        'page.pageSize': 20,
        'sort.clauses[0].field': 'createdAt',
        'sort.clauses[0].direction': 'Descending',
      },
    })
  })

  it('get: GET /catalog/products/:id', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', name: 'Test' }) })
    await ProductApi.get('1')
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/products/1')
  })

  it('create: POST /catalog/products', async () => {
    vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: 'new', name: 'New' }) })
    await ProductApi.create({ name: 'New', slug: 'new' })
    expect(apiClient.post).toHaveBeenCalledWith('/catalog/products', { name: 'New', slug: 'new' })
  })

  it('update: PUT /catalog/products/:id', async () => {
    vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1', name: 'Updated' }) })
    await ProductApi.update('1', { name: 'Updated', slug: 'updated' })
    expect(apiClient.put).toHaveBeenCalledWith('/catalog/products/1', { name: 'Updated', slug: 'updated' })
  })

  it('delete: DELETE /catalog/products/:id', async () => {
    vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
    await ProductApi.delete('1')
    expect(apiClient.delete).toHaveBeenCalledWith('/catalog/products/1')
  })
})
```

- [ ] **Step 2: Update `taxonomies.spec.ts`**

```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { TaxonomyApi } from '../taxonomy.api'

vi.mock('@/shared/api/client', () => ({
  default: {
    get: vi.fn<(...args: unknown[]) => unknown>(),
    post: vi.fn<(...args: unknown[]) => unknown>(),
    put: vi.fn<(...args: unknown[]) => unknown>(),
    delete: vi.fn<(...args: unknown[]) => unknown>(),
  },
}))

const pagedEmpty = { isSuccess: true, items: [], page: 1, pageSize: 20, totalCount: 0, statusCode: 200 }
const singleOk = (value: unknown) => ({ isSuccess: true, value, statusCode: 200 })
const defaultQuery = { page: 1, pageSize: 20, sort: [{ field: 'createdAt', direction: 'Descending' as const }] }

describe('TaxonomyApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('getMany: GET /catalog/taxonomies with serialized params', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
    await TaxonomyApi.getMany(defaultQuery)
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/taxonomies', {
      params: {
        'page.page': 1,
        'page.pageSize': 20,
        'sort.clauses[0].field': 'createdAt',
        'sort.clauses[0].direction': 'Descending',
      },
    })
  })
  // get, create, update, delete tests same pattern as products.spec.ts
})
```

- [ ] **Step 3: Update `optionTypes.spec.ts`**

Same pattern as taxonomies — update `getMany` test to verify serialized params.

- [ ] **Step 4: Remove old `*ListParams` types**

From `features/catalog/types/product.request.ts` — remove the `ProductListParams` interface entirely (keep `CreateProductRequest` and `UpdateProductRequest`).

From `features/catalog/types/taxonomy.request.ts` — remove `TaxonomyListParams`.

From `features/catalog/types/option-type.request.ts` — remove `OptionTypeListParams`.

- [ ] **Step 5: Run all tests to verify they pass**

Run: `npx vitest run src/features/catalog/api/__tests__/ src/shared/api/utils/__tests__/`
Expected: All tests PASS

- [ ] **Step 6: Commit**

```bash
git add src/features/catalog/api/__tests__/ src/features/catalog/types/
git commit -m "test(admin): update API tests for serialized params, remove old *ListParams types"
```

---

### Task 8: Verify build passes

- [ ] **Step 1: Run TypeScript check**

Run: `npx vue-tsc --noEmit`
Expected: No type errors

- [ ] **Step 2: Run lint**

Run: `pnpm run lint`
Expected: No lint errors

- [ ] **Step 3: Run full test suite**

Run: `npx vitest run`
Expected: All tests pass

- [ ] **Step 4: Final commit if needed**

```bash
git commit -m "chore: fix lint/type issues after query converter migration"
```
