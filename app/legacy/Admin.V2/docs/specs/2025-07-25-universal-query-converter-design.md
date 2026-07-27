# Universal Query Params Converter — Design

## Overview

A shared serializer that converts feature-level `ListQuery` objects into the dot-notation query parameter format the C# backend API expects. All features use the same converter, ensuring consistent parameter syntax across the entire application.

---

## Data Structures

Reuses existing sub-types from `shared/models/querying.ts` (`FilterCondition`, `FilterGroup`, `FilterOperator`, `FilterLogic`, `SortClause`, `SortDirection`, `SortNulls`, `SearchTerm`, `SearchMode`) without modification.

Introduces `ListQuery` — the frontend-facing query object:

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
```

Default factory:

```ts
export function defaultListQuery(pageSize = 20): ListQuery {
  return { page: 1, pageSize, sort: [{ field: 'createdAt', direction: 'Descending' }] }
}
```

---

## Serializer — `toQueryParams`

```ts
// shared/api/utils/query-serializer.ts
import type { ListQuery } from '@/shared/models'
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
```

### Wire format produced

```
?page.page=1&page.pageSize=20
&search.term.value=red&search.term.caseSensitive=false&search.mode=Any
&sort.clauses[0].field=name&sort.clauses[0].direction=Ascending
&filter.root.logic=And
&filter.root.conditions[0].field=status&filter.root.conditions[0].operator=Equal&filter.root.conditions[0].value=Active
```

---

## API Client Helper

```ts
// shared/api/utils/query-serializer.ts (continued)
import apiClient from '@/shared/api/client'

export async function getPagedList<T>(url: string, query: ListQuery): Promise<T> {
  const res = await apiClient.get<T>(url, { params: toQueryParams(query) })
  return res.data
}
```

---

## Integration — Feature Store

Every feature store owns its own `ListQuery` state and pagination logic. The feature API class uses the shared `getPagedList` helper.

### Feature API class

```ts
// features/catalog/api/product.api.ts
import { getPagedList } from '@/shared/api/utils/query-serializer'
import type { PagedResult, ListQuery } from '@/shared/models'
import type { ProductResponse } from '../types'

export class ProductApi {
  static getMany(query: ListQuery): Promise<PagedResult<ProductResponse>> {
    return getPagedList<PagedResult<ProductResponse>>('/catalog/products', query)
  }
}
```

### Feature store

```ts
// features/catalog/store/product.store.ts
import { ref, readonly } from 'vue'
import { defineStore } from 'pinia'
import { defaultListQuery } from '@/shared/models'
import { ProductApi } from '../api/product.api'
import type { ListQuery, PagedResult } from '@/shared/models'
import type { FilterGroup } from '@/shared/models/querying'
import type { ProductResponse } from '../types'

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
  function setFilter(condition: { field: string; operator: string; value: string }) {
    query.value.filters = { logic: 'And', conditions: [condition] }
    query.value.page = 1
    return fetchMany()
  }
  function setFilterGroup(group: FilterGroup) {
    query.value.filters = group
    query.value.page = 1
    return fetchMany()
  }
  function resetQuery() { query.value = defaultListQuery(); return fetchMany() }

  return {
    items: readonly(items), loading: readonly(loading),
    error: readonly(error), totalRecords: readonly(totalRecords),
    query: readonly(query),
    fetchMany, setPage, setSearch, setSort, setFilter, setFilterGroup, resetQuery,
  }
})
```

---

## Page component usage

```vue
<script setup lang="ts">
const store = useProductStore()
store.fetchMany()

function onPage(event: any) { store.setPage(event.page + 1) }
function onSearch(value: string) { store.setSearch(value) }
function onSort(event: any) {
  store.setSort(event.sortField, event.sortOrder === 1 ? 'Asc' : 'Desc')
}
</script>

<template>
  <DataTable
    :value="store.items.value"
    :loading="store.loading.value"
    :totalRecords="store.totalRecords.value"
    lazy
    @page="onPage"
    @sort="onSort"
  >
    <Column field="name" header="Name" :sortable="true" />
    <Column field="status" header="Status" />
  </DataTable>
</template>
```

---

## File structure

```
shared/
  models/
    list-query.ts          ← ListQuery interface + defaultListQuery()
  api/
    utils/
      query-serializer.ts  ← toQueryParams() + getPagedList()
    (client.ts, interceptors/ unchanged)
```

No changes to existing `querying.ts` — all existing types stay. No changes to `result.ts` or `pagination.ts`.

---

## Error handling

- `getPagedList()` does NOT catch axios errors — errors propagate to the store's try/catch
- The store sets `error = 'Failed to load'` on any exception
- The serializer does NOT validate — invalid `ListQuery` fields produce undefined query params (axios drops them)
- Empty `search.value` sets `search.term.value` as empty string — the backend ignores empty searches
- Zero `page` or `pageSize` is a frontend bug, not handled (backend validation will return 400)

---

## Backward compatibility

Existing catalog feature code that uses `ProductListParams` directly will be migrated to use `ListQuery`. The old `ProductListParams` type is removed. No other feature currently has implemented API calls, so no other migration needed.
