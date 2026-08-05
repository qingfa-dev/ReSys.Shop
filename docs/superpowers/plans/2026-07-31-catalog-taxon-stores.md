# Catalog Taxon Stores + View Endpoint Corrections — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Split taxon state across three dedicated Pinia stores (`taxonStore`, `taxonTreeStore`, `taxonDetailStore`) and correct the taxon views to route data through them, eliminating the broken empty-URL request in `TaxonDetail.vue` and the hardcoded-URL bypass in `TaxonsList.vue`.

**Architecture:** Three single-purpose Pinia stores under `app/Admin/src/features/catalog/stores/` mirror the existing `taxonomyStore` composition pattern. `taxonStore` owns list/paging + a cached active list; `taxonTreeStore` owns the per-taxonomy tree cache shared by both views; `taxonDetailStore` owns the edited taxon + its rules. Views bind directly to store state and call store actions; mutations (create/update/delete) stay in the views via `TaxonApi`/`TaxonRuleApi`. `TaxonQuery` gains optional `filter`/`searchFields`/`searchMode` so the store can pass the list view's search config through the service layer.

**Tech Stack:** Vue 3 + TypeScript, Pinia (setup-style stores), PrimeVue DataTable/TreeTable/Select, Vitest + `@pinia/testing`-style mocking (plain `createPinia` + mocked service modules).

**Spec:** `docs/superpowers/specs/2026-07-31-catalog-taxon-store-design.md`

## Global Constraints

- Must pass `pnpm run lint`, `pnpm run test:unit`, and `pnpm run build` with zero errors (run from `app/Admin/`).
- No new npm dependencies.
- `taxonApi.ts` / `taxonomyApi.ts` / `taxonRuleApi.ts` are unchanged.
- `TaxonomyDetail.vue` and `TaxonRuleFormDialog.vue` are out of scope.
- Tree response wrapper stays `Result<{ tree: TaxonTreeItem[] }>` — extract via `result.value.tree`.
- Backend clamps paged results to default page size 10, max 100 — always pass `pageSize` when a dropdown needs the full set.
- Existing feature-file convention: subdirectory is always `Storefront`, never `Store` (C# only — not applicable to this Vue work).

---

### Task 1: Create `taxonTreeStore.ts`

**Files:**
- Create: `app/Admin/src/features/catalog/stores/taxonTreeStore.ts`
- Modify: `app/Admin/src/features/catalog/stores/index.ts`
- Test: `app/Admin/src/features/catalog/__tests__/stores/taxonTreeStore.spec.ts`

**Interfaces:**
- Consumes: `TaxonApi.getTree(taxonomyId: string): Promise<Result<{ tree: TaxonTreeItem[] }>>`; `ok(value: T): Result<T>` and `Result<T>` from `@/shared/types`; `TaxonTreeItem` from `../types/taxon`.
- Produces: `useTaxonTreeStore(): { tree: Ref<TaxonTreeItem[]>, treeLoading: Ref<boolean>, treeTaxonomyId: Ref<string | null>, fetchTree(taxonomyId: string): Promise<Result<{ tree: TaxonTreeItem[] }>> }`. Cache hit returns `ok({ tree: tree.value })` without hitting the network.

- [ ] **Step 1: Write the failing test**

Create `app/Admin/src/features/catalog/__tests__/stores/taxonTreeStore.spec.ts`:

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { mockGetTree } = vi.hoisted(() => ({
  mockGetTree: vi.fn<any>(),
}))

vi.mock('../../services/taxonApi', () => ({
  TaxonApi: {
    getTree: mockGetTree,
  },
}))

import { useTaxonTreeStore } from '../../stores/taxonTreeStore'

function treeResult() {
  return {
    isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    value: { tree: [{ id: 'n1', name: 'Root', children: [] }] },
  }
}

describe('useTaxonTreeStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('fetchTree fetches and caches on first call', async () => {
    mockGetTree.mockResolvedValue(treeResult())
    const store = useTaxonTreeStore()
    const result = await store.fetchTree('tax-1')
    expect(result.isSuccess).toBe(true)
    expect(mockGetTree).toHaveBeenCalledWith('tax-1')
    expect(store.tree).toHaveLength(1)
    expect(store.treeTaxonomyId).toBe('tax-1')
  })

  it('fetchTree does not refetch for the same taxonomy', async () => {
    mockGetTree.mockResolvedValue(treeResult())
    const store = useTaxonTreeStore()
    await store.fetchTree('tax-1')
    await store.fetchTree('tax-1')
    expect(mockGetTree).toHaveBeenCalledTimes(1)
  })

  it('fetchTree refetches for a different taxonomy', async () => {
    mockGetTree.mockResolvedValue(treeResult())
    const store = useTaxonTreeStore()
    await store.fetchTree('tax-1')
    await store.fetchTree('tax-2')
    expect(mockGetTree).toHaveBeenCalledTimes(2)
    expect(store.treeTaxonomyId).toBe('tax-2')
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd app/Admin && pnpm run test:unit -- run src/features/catalog/__tests__/stores/taxonTreeStore.spec.ts`
Expected: FAIL — module `../../stores/taxonTreeStore` not found.

- [ ] **Step 3: Write minimal implementation**

Create `app/Admin/src/features/catalog/stores/taxonTreeStore.ts`:

```typescript
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { ok, type Result } from '@/shared/types'
import type { TaxonTreeItem } from '../types/taxon'
import { TaxonApi } from '../services/taxonApi'

export const useTaxonTreeStore = defineStore('taxonTree', () => {
  const tree = ref<TaxonTreeItem[]>([])
  const treeLoading = ref(false)
  const treeTaxonomyId = ref<string | null>(null)

  async function fetchTree(taxonomyId: string): Promise<Result<{ tree: TaxonTreeItem[] }>> {
    if (treeTaxonomyId.value === taxonomyId) {
      return ok({ tree: tree.value })
    }

    treeLoading.value = true
    const result = await TaxonApi.getTree(taxonomyId)
    treeLoading.value = false

    if (result.isSuccess) {
      tree.value = result.value?.tree ?? []
      treeTaxonomyId.value = taxonomyId
    }

    return result
  }

  return { tree, treeLoading, treeTaxonomyId, fetchTree }
})
```

Add the export to `app/Admin/src/features/catalog/stores/index.ts`:

```typescript
export { useTaxonTreeStore } from './taxonTreeStore'
```

- [ ] **Step 4: Run test to verify it passes**

Run: `cd app/Admin && pnpm run test:unit -- run src/features/catalog/__tests__/stores/taxonTreeStore.spec.ts`
Expected: PASS (3 tests).

- [ ] **Step 5: Lint and commit**

```bash
cd app/Admin && pnpm run lint
git add app/Admin/src/features/catalog/stores/taxonTreeStore.ts app/Admin/src/features/catalog/stores/index.ts app/Admin/src/features/catalog/__tests__/stores/taxonTreeStore.spec.ts
git commit -m "feat(admin): add taxon tree store with per-taxonomy cache"
```

---

### Task 2: Create `taxonDetailStore.ts`

**Files:**
- Create: `app/Admin/src/features/catalog/stores/taxonDetailStore.ts`
- Modify: `app/Admin/src/features/catalog/stores/index.ts`
- Test: `app/Admin/src/features/catalog/__tests__/stores/taxonDetailStore.spec.ts`

**Interfaces:**
- Consumes: `TaxonApi.getTaxon(id: string): Promise<Result<TaxonDetail>>`; `TaxonRuleApi.getRules(taxonId: string): Promise<PagedResult<TaxonRuleListItem>>`; `Result`/`PagedResult` from `@/shared/types`; `TaxonDetail` from `../types/taxon`; `TaxonRuleListItem` from `../types/taxonRule`.
- Produces: `useTaxonDetailStore(): { currentTaxon: Ref<TaxonDetail | null>, detailLoading: Ref<boolean>, fetchDetail(id: string): Promise<Result<TaxonDetail>>, rules: Ref<TaxonRuleListItem[]>, rulesLoading: Ref<boolean>, fetchRules(taxonId: string): Promise<PagedResult<TaxonRuleListItem>> }`. On failure the API `Result` is returned untouched (view shows the toast) and state refs keep their previous values.

- [ ] **Step 1: Write the failing test**

Create `app/Admin/src/features/catalog/__tests__/stores/taxonDetailStore.spec.ts`:

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { mockGetTaxon, mockGetRules } = vi.hoisted(() => ({
  mockGetTaxon: vi.fn<any>(),
  mockGetRules: vi.fn<any>(),
}))

vi.mock('../../services/taxonApi', () => ({
  TaxonApi: {
    getTaxon: mockGetTaxon,
  },
}))

vi.mock('../../services/taxonRuleApi', () => ({
  TaxonRuleApi: {
    getRules: mockGetRules,
  },
}))

import { useTaxonDetailStore } from '../../stores/taxonDetailStore'

describe('useTaxonDetailStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('fetchDetail populates currentTaxon on success', async () => {
    const detail = { id: 't1', name: 'Shoes' }
    mockGetTaxon.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null, value: detail })
    const store = useTaxonDetailStore()
    const result = await store.fetchDetail('t1')
    expect(result.isSuccess).toBe(true)
    expect(store.currentTaxon).toEqual(detail)
  })

  it('fetchDetail returns failure and keeps currentTaxon unchanged', async () => {
    mockGetTaxon.mockResolvedValue({ isSuccess: false, statusCode: 404, message: 'Not found', errors: [{ code: 'NotFound', message: 'Not found', type: 404 }], metadata: null, value: null })
    const store = useTaxonDetailStore()
    store.currentTaxon = { id: 'old' } as any
    const result = await store.fetchDetail('t1')
    expect(result.isSuccess).toBe(false)
    expect(store.currentTaxon).toEqual({ id: 'old' })
  })

  it('fetchRules populates rules on success', async () => {
    const items = [{ id: 'r1', type: 'product_name', matchPolicy: 'contains', value: 'shoes', taxonId: 't1' }]
    mockGetRules.mockResolvedValue({ isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null, items, page: 1, pageSize: 10, totalCount: 1, totalPages: 1 })
    const store = useTaxonDetailStore()
    const result = await store.fetchRules('t1')
    expect(result.isSuccess).toBe(true)
    expect(store.rules).toEqual(items)
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd app/Admin && pnpm run test:unit -- run src/features/catalog/__tests__/stores/taxonDetailStore.spec.ts`
Expected: FAIL — module `../../stores/taxonDetailStore` not found.

- [ ] **Step 3: Write minimal implementation**

Create `app/Admin/src/features/catalog/stores/taxonDetailStore.ts`:

```typescript
import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { Result, PagedResult } from '@/shared/types'
import type { TaxonDetail } from '../types/taxon'
import type { TaxonRuleListItem } from '../types/taxonRule'
import { TaxonApi } from '../services/taxonApi'
import { TaxonRuleApi } from '../services/taxonRuleApi'

export const useTaxonDetailStore = defineStore('taxonDetail', () => {
  const currentTaxon = ref<TaxonDetail | null>(null)
  const detailLoading = ref(false)
  const rules = ref<TaxonRuleListItem[]>([])
  const rulesLoading = ref(false)

  async function fetchDetail(id: string): Promise<Result<TaxonDetail>> {
    detailLoading.value = true
    const result = await TaxonApi.getTaxon(id)
    detailLoading.value = false

    if (result.isSuccess) {
      currentTaxon.value = result.value
    }

    return result
  }

  async function fetchRules(taxonId: string): Promise<PagedResult<TaxonRuleListItem>> {
    rulesLoading.value = true
    const result = await TaxonRuleApi.getRules(taxonId)
    rulesLoading.value = false

    if (result.isSuccess) {
      rules.value = result.items
    }

    return result
  }

  return { currentTaxon, detailLoading, fetchDetail, rules, rulesLoading, fetchRules }
})
```

Add the export to `app/Admin/src/features/catalog/stores/index.ts`:

```typescript
export { useTaxonDetailStore } from './taxonDetailStore'
```

- [ ] **Step 4: Run test to verify it passes**

Run: `cd app/Admin && pnpm run test:unit -- run src/features/catalog/__tests__/stores/taxonDetailStore.spec.ts`
Expected: PASS (3 tests).

- [ ] **Step 5: Lint and commit**

```bash
cd app/Admin && pnpm run lint
git add app/Admin/src/features/catalog/stores/taxonDetailStore.ts app/Admin/src/features/catalog/stores/index.ts app/Admin/src/features/catalog/__tests__/stores/taxonDetailStore.spec.ts
git commit -m "feat(admin): add taxon detail store with rules state"
```

---

### Task 3: Extend `TaxonQuery` for store-driven listing

**Files:**
- Modify: `app/Admin/src/features/catalog/types/taxon.ts`
- Test: `app/Admin/src/features/catalog/__tests__/types/taxon.spec.ts`

**Interfaces:**
- Consumes: `QueryingParameters` from `@/shared/types/querying` (unchanged).
- Produces: `TaxonQuery` gains optional `filter?: string`, `searchFields?: string[]`, `searchMode?: string`. `toTaxonQueryParams(query)` now also merges `query.filter` into the filter DSL and forwards `searchFields`/`searchMode`. Existing fields (`taxonomyId`, `name`, `search`, `sortBy`, `sortDirection`, `page`, `pageSize`) behave identically — existing tests must still pass.

- [ ] **Step 1: Write the failing tests**

Append to `app/Admin/src/features/catalog/__tests__/types/taxon.spec.ts` inside the existing `describe('toTaxonQueryParams', ...)` block:

```typescript
  it('passes through a raw filter string', () => {
    const result = toTaxonQueryParams({ filter: 'depth=1' })
    expect(result.filter).toBe('depth=1')
  })

  it('forwards search fields and mode', () => {
    const result = toTaxonQueryParams({ search: 'shoes', searchFields: ['name', 'slug'], searchMode: 'any' })
    expect(result.search).toBe('shoes')
    expect(result.searchFields).toEqual(['name', 'slug'])
    expect(result.searchMode).toBe('any')
  })

  it('merges raw filter with taxonomyId filter', () => {
    const result = toTaxonQueryParams({ filter: 'depth=1', taxonomyId: 'abc-123' })
    expect(result.filter).toBe('depth=1,taxonomyId=abc-123')
  })
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd app/Admin && pnpm run test:unit -- run src/features/catalog/__tests__/types/taxon.spec.ts`
Expected: FAIL — `toTaxonQueryParams({ filter: 'depth=1' })` returns `filter: null`.

- [ ] **Step 3: Implement the extension**

In `app/Admin/src/features/catalog/types/taxon.ts`, extend the `TaxonQuery` interface:

```typescript
export interface TaxonQuery {
  taxonomyId?: string
  name?: string
  filter?: string
  search?: string
  searchFields?: string[]
  searchMode?: string
  sortBy?: 'name' | 'slug' | 'position' | 'depth' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}
```

Replace the body of `toTaxonQueryParams`:

```typescript
export function toTaxonQueryParams(query: TaxonQuery): QueryingParameters {
  const filters: string[] = []

  if (query.filter !== undefined && query.filter !== '') {
    filters.push(query.filter)
  }
  if (query.taxonomyId !== undefined && query.taxonomyId !== '') {
    filters.push(`taxonomyId=${query.taxonomyId}`)
  }
  if (query.name !== undefined && query.name !== '') {
    filters.push(`name*=${query.name}`)
  }

  let sort: string[] | null = null
  if (query.sortBy) {
    const dir = query.sortDirection === 'desc' ? '-' : ''
    sort = [`${dir}${query.sortBy}`]
  }

  return {
    filter: filters.length > 0 ? filters.join(',') : null,
    search: query.search ?? null,
    searchFields: query.searchFields && query.searchFields.length > 0 ? query.searchFields : null,
    searchMode: query.searchMode ?? null,
    sort,
    pageNumber: query.page ?? null,
    pageSize: query.pageSize ?? null,
  }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `cd app/Admin && pnpm run test:unit -- run src/features/catalog/__tests__/types/taxon.spec.ts`
Expected: PASS (all existing + 3 new tests). Also run `pnpm run test:unit -- run src/features/catalog/__tests__/services/taxonApi.spec.ts` to confirm the service spec (which asserts `filter: 'taxonomyId=abc-123'`) still passes.

- [ ] **Step 5: Lint and commit**

```bash
cd app/Admin && pnpm run lint
git add app/Admin/src/features/catalog/types/taxon.ts app/Admin/src/features/catalog/__tests__/types/taxon.spec.ts
git commit -m "feat(admin): extend taxon query params with filter, search fields and mode"
```

---

### Task 4: Create `taxonStore.ts`

**Files:**
- Create: `app/Admin/src/features/catalog/stores/taxonStore.ts`
- Modify: `app/Admin/src/features/catalog/stores/index.ts`
- Test: `app/Admin/src/features/catalog/__tests__/stores/taxonStore.spec.ts`

**Interfaces:**
- Consumes: `TaxonApi.getTaxons(query: TaxonQuery): Promise<PagedResult<TaxonListItem>>`; `TaxonApi.getList(taxonomyId: string, query: TaxonQuery): Promise<PagedResult<TaxonListItem>>`; extended `TaxonQuery`/`toTaxonQueryParams` from Task 3; `TaxonListItem` from `../types/taxon`.
- Produces: `useTaxonStore(): { activeTaxons, loaded, fetchActive, items, loading, error, page, pageSize, totalCount, totalPages, filter, sort, search, searchFields, searchMode, selectedTaxonomyId, fetchList, setPage, setPageSize, setSort, setSearch, setFilter, setSearchFields, setSearchMode, setSelectedTaxonomy, refresh, reset }`. `fetchList` calls `getList` when `selectedTaxonomyId` is set, else `getTaxons`. Default `sort=['position']`, `searchFields=['name','slug']`, `searchMode='any'`, `pageSize=20`.

- [ ] **Step 1: Write the failing test**

Create `app/Admin/src/features/catalog/__tests__/stores/taxonStore.spec.ts`:

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { mockGetTaxons, mockGetList } = vi.hoisted(() => ({
  mockGetTaxons: vi.fn<any>(),
  mockGetList: vi.fn<any>(),
}))

vi.mock('../../services/taxonApi', () => ({
  TaxonApi: {
    getTaxons: mockGetTaxons,
    getList: mockGetList,
  },
}))

import { useTaxonStore } from '../../stores/taxonStore'

function pagedResult(items: any[] = []) {
  return {
    isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    items, page: 1, pageSize: 20, totalCount: items.length, totalPages: 0,
  }
}

describe('useTaxonStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('fetchList calls getTaxons when no taxonomy selected', async () => {
    mockGetTaxons.mockResolvedValue(pagedResult([]))
    const store = useTaxonStore()
    await store.fetchList()
    expect(mockGetTaxons).toHaveBeenCalledWith(expect.objectContaining({ pageSize: 20, searchFields: ['name', 'slug'], searchMode: 'any' }))
    expect(mockGetList).not.toHaveBeenCalled()
  })

  it('fetchList calls getList when a taxonomy is selected', async () => {
    mockGetList.mockResolvedValue(pagedResult([]))
    const store = useTaxonStore()
    store.selectedTaxonomyId = 'tax-1'
    await store.fetchList()
    expect(mockGetList).toHaveBeenCalledWith('tax-1', expect.objectContaining({ pageSize: 20 }))
    expect(mockGetTaxons).not.toHaveBeenCalled()
  })

  it('setSearch updates search and refetches', async () => {
    mockGetTaxons.mockResolvedValue(pagedResult([]))
    const store = useTaxonStore()
    await store.setSearch('shoes')
    expect(store.search).toBe('shoes')
    expect(store.page).toBe(1)
    expect(mockGetTaxons).toHaveBeenCalledWith(expect.objectContaining({ search: 'shoes' }))
  })

  it('setSelectedTaxonomy switches endpoint and refetches', async () => {
    mockGetTaxons.mockResolvedValue(pagedResult([]))
    mockGetList.mockResolvedValue(pagedResult([]))
    const store = useTaxonStore()
    await store.setSelectedTaxonomy('tax-1')
    expect(store.selectedTaxonomyId).toBe('tax-1')
    expect(mockGetList).toHaveBeenCalled()
    await store.setSelectedTaxonomy(null)
    expect(store.selectedTaxonomyId).toBeNull()
    expect(mockGetTaxons).toHaveBeenCalled()
  })

  it('fetchActive is lazy-once and populates activeTaxons', async () => {
    const items = [{ id: '1', name: 'Shoes' }]
    mockGetTaxons.mockResolvedValue(pagedResult(items))
    const store = useTaxonStore()
    await store.fetchActive()
    await store.fetchActive()
    expect(mockGetTaxons).toHaveBeenCalledTimes(1)
    expect(store.activeTaxons).toEqual(items)
    expect(store.loaded).toBe(true)
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd app/Admin && pnpm run test:unit -- run src/features/catalog/__tests__/stores/taxonStore.spec.ts`
Expected: FAIL — module `../../stores/taxonStore` not found.

- [ ] **Step 3: Write minimal implementation**

Create `app/Admin/src/features/catalog/stores/taxonStore.ts`:

```typescript
import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import type { TaxonListItem, TaxonQuery } from '../types/taxon'
import { TaxonApi } from '../services/taxonApi'

export const useTaxonStore = defineStore('taxons', () => {
  const activeTaxons = ref<TaxonListItem[]>([])
  const loaded = ref(false)

  const items = ref<TaxonListItem[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const page = ref(1)
  const pageSize = ref(20)
  const totalCount = ref(0)
  const filter = ref('')
  const sort = ref<string[]>(['position'])
  const search = ref('')
  const searchFields = ref<string[]>(['name', 'slug'])
  const searchMode = ref('any')
  const selectedTaxonomyId = ref<string | null>(null)

  const totalPages = computed(() => {
    if (pageSize.value <= 0) return 0
    return Math.ceil(totalCount.value / pageSize.value)
  })

  function buildQuery(): TaxonQuery {
    const query: TaxonQuery = {
      filter: filter.value || undefined,
      search: search.value || undefined,
      searchFields: searchFields.value.length > 0 ? searchFields.value : undefined,
      searchMode: searchMode.value || undefined,
      page: page.value,
      pageSize: pageSize.value,
    }

    if (sort.value.length > 0) {
      const raw = sort.value[0]
      const descending = raw.startsWith('-')
      const field = descending ? raw.slice(1) : raw
      if (field) {
        query.sortBy = field as TaxonQuery['sortBy']
        query.sortDirection = descending ? 'desc' : 'asc'
      }
    }

    return query
  }

  async function fetchList(): Promise<void> {
    loading.value = true
    error.value = null

    const query = buildQuery()
    const result = selectedTaxonomyId.value
      ? await TaxonApi.getList(selectedTaxonomyId.value, query)
      : await TaxonApi.getTaxons(query)

    if (result.isSuccess) {
      items.value = result.items
      totalCount.value = result.totalCount
      page.value = result.page
      pageSize.value = result.pageSize
    } else {
      error.value = result.message ?? result.errors[0]?.message ?? 'Request failed.'
    }

    loading.value = false
  }

  function setPage(p: number) {
    page.value = Math.max(1, p)
    fetchList()
  }

  function setPageSize(s: number) {
    pageSize.value = Math.max(1, s)
    page.value = 1
    fetchList()
  }

  function setSort(s: string[]) {
    sort.value = s
    page.value = 1
    fetchList()
  }

  function setSearch(s: string) {
    search.value = s
    page.value = 1
    fetchList()
  }

  function setFilter(f: string) {
    filter.value = f
    page.value = 1
    fetchList()
  }

  function setSearchFields(sf: string[]) {
    searchFields.value = sf
    page.value = 1
    fetchList()
  }

  function setSearchMode(m: string) {
    searchMode.value = m
    page.value = 1
    fetchList()
  }

  function setSelectedTaxonomy(id: string | null) {
    selectedTaxonomyId.value = id
    page.value = 1
    fetchList()
  }

  function refresh(): Promise<void> {
    return fetchList()
  }

  function reset() {
    items.value = []
    loading.value = false
    error.value = null
    page.value = 1
    pageSize.value = 20
    totalCount.value = 0
    filter.value = ''
    sort.value = ['position']
    search.value = ''
    searchFields.value = ['name', 'slug']
    searchMode.value = 'any'
    selectedTaxonomyId.value = null
  }

  async function fetchActive(): Promise<void> {
    if (loaded.value) return

    const result = await TaxonApi.getTaxons({
      pageSize: 100,
      sortBy: 'name',
      sortDirection: 'asc',
    })

    if (result.isSuccess) {
      activeTaxons.value = result.items
      loaded.value = true
    }
  }

  return {
    activeTaxons,
    loaded,
    items,
    loading,
    error,
    page,
    pageSize,
    totalCount,
    totalPages,
    filter,
    sort,
    search,
    searchFields,
    searchMode,
    selectedTaxonomyId,
    fetchList,
    setPage,
    setPageSize,
    setSort,
    setSearch,
    setFilter,
    setSearchFields,
    setSearchMode,
    setSelectedTaxonomy,
    refresh,
    reset,
    fetchActive,
  }
})
```

Add the export to `app/Admin/src/features/catalog/stores/index.ts`:

```typescript
export { useTaxonStore } from './taxonStore'
```

- [ ] **Step 4: Run test to verify it passes**

Run: `cd app/Admin && pnpm run test:unit -- run src/features/catalog/__tests__/stores/taxonStore.spec.ts`
Expected: PASS (5 tests).

- [ ] **Step 5: Lint and commit**

```bash
cd app/Admin && pnpm run lint
git add app/Admin/src/features/catalog/stores/taxonStore.ts app/Admin/src/features/catalog/stores/index.ts app/Admin/src/features/catalog/__tests__/stores/taxonStore.spec.ts
git commit -m "feat(admin): add taxon store with endpoint selection"
```

---

### Task 5: Fix `taxonomyStore` page size truncation

**Files:**
- Modify: `app/Admin/src/features/catalog/stores/taxonomyStore.ts`

**Interfaces:**
- Consumes: `TaxonomyApi.getTaxonomies(query: TaxonomyQuery): Promise<PagedResult<TaxonomyListItem>>`.
- Produces: unchanged store shape; `fetchActive()` now passes `pageSize: 100` so the full taxonomy set (max backend page size) is cached instead of the first 10.

- [ ] **Step 1: Implement the fix**

In `app/Admin/src/features/catalog/stores/taxonomyStore.ts`, change the `getTaxonomies` call in `fetchActive`:

```typescript
    const result = await TaxonomyApi.getTaxonomies({
      pageSize: 100,
      sortBy: 'name',
      sortDirection: 'asc',
    })
```

- [ ] **Step 2: Verify nothing regresses**

Run: `cd app/Admin && pnpm run test:unit -- run src/features/catalog/__tests__/services/taxonomyApi.spec.ts`
Expected: PASS (service spec unaffected).

- [ ] **Step 3: Lint and commit**

```bash
cd app/Admin && pnpm run lint
git add app/Admin/src/features/catalog/stores/taxonomyStore.ts
git commit -m "fix(admin): load all taxonomies in taxonomy store dropdown"
```

---

### Task 6: Rewrite `TaxonsList.vue` to use the stores

**Files:**
- Rewrite: `app/Admin/src/features/catalog/views/TaxonsList.vue`

**Interfaces:**
- Consumes: `useTaxonStore` (Task 4) — `items`, `loading`, `totalCount`, `page`, `pageSize`, `selectedTaxonomyId`, `setSearch`, `setSort`, `setPage`, `setPageSize`, `setSelectedTaxonomy`, `refresh`; `useTaxonTreeStore` (Task 1) — `tree`, `treeLoading`, `fetchTree`; `useTaxonomyStore` — `activeTaxonomies`, `fetchActive`; `TaxonApi.deleteTaxon(id)`.
- Produces: a fully working list page with server-side search, sort, pagination, a taxonomy selector, and a tree view. Fixes the latent `defaultSort: ['lft']` bug — `lft` is not in `TAXON_SORT_FIELDS`, so the old `usePagedQuery` request always failed validation; the store defaults to `['position']`.

- [ ] **Step 1: Rewrite the script block**

Replace the entire `<script setup>` block of `app/Admin/src/features/catalog/views/TaxonsList.vue`:

```vue
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import TreeTable from 'primevue/treetable'
import Column from 'primevue/column'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { useNotify } from '@/shared/composables/useNotify'
import { TaxonApi } from '../services/taxonApi'
import { useTaxonStore } from '../stores/taxonStore'
import { useTaxonTreeStore } from '../stores/taxonTreeStore'
import { useTaxonomyStore } from '../stores/taxonomyStore'
import type { TaxonListItem } from '../types/taxon'

const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const taxonStore = useTaxonStore()
const taxonTreeStore = useTaxonTreeStore()
const taxonomyStore = useTaxonomyStore()

const { dt, exportCSV } = useDataTableExport()
const selectedItems = ref<TaxonListItem[]>([])
const searchTerm = ref('')
const treeFilter = ref('')
const viewMode = ref<'table' | 'tree'>('table')
const allowedSearchFields = ['name', 'slug']

const items = taxonStore.items
const loading = taxonStore.loading
const totalCount = taxonStore.totalCount
const first = computed(() => (taxonStore.page - 1) * taxonStore.pageSize)

function addTreeNodeKeys(nodes: any[]): any[] {
  return nodes.map(n => ({
    ...n,
    key: n.id,
    children: n.children ? addTreeNodeKeys(n.children) : [],
  }))
}

const treeData = computed(() => addTreeNodeKeys(taxonTreeStore.tree) as any[])
const treeLoading = taxonTreeStore.treeLoading

onMounted(async () => {
  await taxonomyStore.fetchActive()
  const taxonomyId = route.query.taxonomyId as string | undefined
  await taxonStore.setSelectedTaxonomy(taxonomyId ?? null)
})

async function loadTree() {
  const taxonomyId = taxonStore.selectedTaxonomyId
  if (!taxonomyId) return
  await taxonTreeStore.fetchTree(taxonomyId)
}

function toggleViewMode() {
  if (viewMode.value === 'table') {
    viewMode.value = 'tree'
    if (taxonTreeStore.tree.length === 0 && taxonStore.selectedTaxonomyId) {
      loadTree()
    }
  } else {
    viewMode.value = 'table'
  }
}

function onTaxonomyChange(id: string | null) {
  const taxonomyId = id || null
  taxonStore.setSelectedTaxonomy(taxonomyId)
  router.replace({ query: { ...route.query, taxonomyId: taxonomyId ?? undefined } })
}

function navigateToNew() {
  const taxonomyId = taxonStore.selectedTaxonomyId
  const query = taxonomyId ? `?taxonomyId=${taxonomyId}` : ''
  router.push(`/catalog/taxons/new${query}`)
}

function navigateToEdit(id: string) {
  router.push(`/catalog/taxons/${id}`)
}

function onSearch(value: string) {
  searchTerm.value = value
  taxonStore.setSearch(value)
}

function clearSearch() {
  searchTerm.value = ''
  taxonStore.setSearch('')
}

function filterTree(name: string) {
  treeFilter.value = name ? name.toLowerCase() : ''
}

function onPage(event: any) {
  taxonStore.setPage(event.page + 1)
}

function onRows(rows: number) {
  taxonStore.setPageSize(rows)
}

function onSort(event: any) {
  if (!event.sortField) return
  taxonStore.setSort(event.sortOrder === -1 ? [`-${event.sortField}`] : [event.sortField])
}

function confirmDelete() {
  if (selectedItems.value.length === 0) return

  confirm.require({
    message: `Are you sure you want to delete ${selectedItems.value.length > 1 ? 'these taxons' : 'this taxon'}?`,
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
        const result = await TaxonApi.deleteTaxon(id)
        if (!result.isSuccess) failed++
      }
      selectedItems.value = []
      taxonStore.refresh()
      if (viewMode.value === 'tree') loadTree()
      if (failed === 0) {
        notify.success(
          ids.length > 1 ? 'Taxons deleted' : 'Taxon deleted',
          ids.length > 1
            ? `${ids.length} taxons have been removed.`
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
```

- [ ] **Step 2: Replace the template**

Replace the `<template>` block with:

```vue
<template>
  <div class="flex flex-col h-full p-4">
    <div class="flex-none flex flex-col gap-4">
      <div>
        <div class="font-semibold text-xl">Taxons</div>
        <p class="text-muted-color mt-1">Manage product classification taxons</p>
      </div>
    </div>

    <div class="flex-1 min-h-0 mt-4">
      <DataTable size="large"
        v-if="viewMode === 'table'"
        ref="dt"
        v-model:selection="selectedItems"
        :value="items"
        :loading="loading"
        :total-records="totalCount"
        :first="first"
        :rows="taxonStore.pageSize"
        scrollable
        :paginator="true"
        filter-display="menu"
        data-key="id"
        :global-filter-fields="allowedSearchFields"
        paginator-template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :rows-per-page-options="[5, 10, 25]"
        current-page-report-template="Showing {first} to {last} of {totalRecords}"
        @page="onPage"
        @update:rows="onRows"
        @sort="onSort"
        :pt="{ wrapper: { class: 'h-full' }, tableContainer: { class: 'h-full' } }"
      >
        <Column selection-mode="multiple" header-style="width: 3rem" />
        <template #header>
          <div class="flex justify-between items-center">
            <div class="flex items-center gap-2">
              <FloatLabel variant="on">
                <IconField>
                  <InputIcon class="pi pi-search" />
                  <InputText
                    :model-value="searchTerm"
                    placeholder="Search taxons..."
                    @update:model-value="onSearch($event ?? '')"
                  />
                </IconField>
                <label>Search</label>
              </FloatLabel>
              <Select
                :model-value="taxonStore.selectedTaxonomyId"
                :options="taxonomyStore.activeTaxonomies"
                option-label="name"
                option-value="id"
                placeholder="All taxonomies"
                show-clear
                class="w-64"
                @update:model-value="onTaxonomyChange"
              />
              <Button label="Clear" outlined @click="clearSearch" />
            </div>
            <div class="flex items-center gap-2">
              <Button label="New Taxon" icon="pi pi-plus" severity="primary" @click="navigateToNew" />
              <Button label="Reload" icon="pi pi-sync" severity="secondary" @click="taxonStore.refresh" />
              <Button
                :label="viewMode === 'table' ? 'Tree' : 'Table'"
                severity="secondary"
                :icon="viewMode === 'table' ? 'pi pi-sitemap' : 'pi pi-list'"
                @click="toggleViewMode"
              />
              <Button v-if="viewMode === 'table'" label="Export" icon="pi pi-upload" severity="secondary" @click="exportCSV" />
            </div>
          </div>
        </template>
        <Column field="name" header="Name" :sortable="true" :filter="true" filter-field="name" />
        <Column field="slug" header="Slug" :sortable="true" />
        <Column field="taxonomyName" header="Taxonomy" />
        <Column field="parentName" header="Parent" />
        <Column field="depth" header="Depth" :sortable="true" body-style="text-align: center" />
        <Column field="position" header="Position" :sortable="true" />
        <Column field="taxonRuleCount" header="Rules" body-style="text-align: center" />
        <Column field="productCount" header="Products" body-style="text-align: center" />
        <Column header="" body-style="text-align: right; width: 6rem">
          <template #body="{ data }">
            <div class="flex justify-end gap-2">
              <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
              <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [data]; confirmDelete()" />
            </div>
          </template>
        </Column>
        <template #empty>
          <div class="text-center py-8 text-muted-color">No taxons found.</div>
        </template>
      </DataTable>

      <div v-if="viewMode === 'tree'" class="h-full overflow-auto">
        <div class="flex justify-between items-center mb-3">
          <IconField>
            <InputIcon><i class="pi pi-search" /></InputIcon>
            <InputText
              v-model="treeFilter"
              placeholder="Filter tree..."
              @update:model-value="filterTree($event ?? '')"
            />
          </IconField>
        </div>

        <TreeTable
          :value="treeData"
          :loading="treeLoading"
          v-model:filter-value="treeFilter"
        >
          <Column field="name" header="Name" :expander="true" />
          <Column field="slug" header="Slug" />
          <Column field="position" header="Position" />
          <Column field="taxonRuleCount" header="Rules" />
          <Column field="productCount" header="Products" />
          <Column header="" body-style="text-align: right; width: 6rem">
            <template #body="{ node }">
              <div class="flex justify-end gap-2">
                <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(node.data.id)" />
                <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [{ ...node.data }] as any; confirmDelete()" />
              </div>
            </template>
          </Column>
          <template #empty>
            <div class="text-center py-8 text-muted-color">No taxons in tree.</div>
          </template>
        </TreeTable>
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 3: Run lint, type-check, and the unit suite**

Run: `cd app/Admin && pnpm run lint && pnpm run build`
Expected: no lint errors; `vue-tsc` and `vite build` succeed. (`build` runs `type-check` + `build-only`.)

- [ ] **Step 4: Commit**

```bash
cd app/Admin
git add app/Admin/src/features/catalog/views/TaxonsList.vue
git commit -m "feat(admin): wire taxon list to stores and add taxonomy selector"
```

---

### Task 7: Rewrite `TaxonDetail.vue` to use the stores

**Files:**
- Modify: `app/Admin/src/features/catalog/views/TaxonDetail.vue`

**Interfaces:**
- Consumes: `useTaxonDetailStore` (Task 2) — `fetchDetail`, `currentTaxon`, `fetchRules`, `rules`, `rulesLoading`; `useTaxonTreeStore` (Task 1) — `fetchTree`, `tree`; `useTaxonomyStore` — `activeTaxonomies`, `fetchActive`; `TaxonApi.getTaxon/getTree/createTaxon/updateTaxon` (view no longer uses `getTaxon`/`getTree` directly); `TaxonRuleApi` (unchanged usage in dialog only).
- Produces: TaxonDetail that loads detail, parent tree, and rules through the stores; the broken `usePagedQuery<TaxonRuleListItem>('', ...)` is removed.

- [ ] **Step 1: Update imports**

In `app/Admin/src/features/catalog/views/TaxonDetail.vue`:
- Remove `import { usePagedQuery } from '@/shared/composables/usePagedQuery'`.
- Remove `import { TaxonRuleApi } from '../services/taxonRuleApi'` (the rules now load through the store; the dialog still imports its own copy).
- Keep `import { TaxonApi } from '../services/taxonApi'` (used by create/update).
- Add:

```typescript
import { useTaxonDetailStore } from '../stores/taxonDetailStore'
import { useTaxonTreeStore } from '../stores/taxonTreeStore'
```

- [ ] **Step 2: Replace the rules query and add store instances**

Remove the `usePagedQuery` block:

```typescript
const {
  items: rules,
  loading: rulesLoading,
  refresh: refreshRules,
} = usePagedQuery<TaxonRuleListItem>('', {
  allowedFilterFields: [],
  allowedSortFields: [],
  defaultPageSize: 100,
})
```

and replace it with:

```typescript
const detailStore = useTaxonDetailStore()
const treeStore = useTaxonTreeStore()
```

After this, remove the now-unused `import type { TaxonRuleListItem } from '../types/taxonRule'` only if the type is no longer referenced; it IS still referenced by `editingRule` and `openEditRule`, so keep it.

- [ ] **Step 3: Rewrite `initEditMode`, `loadParents`, `loadRules`, `onRuleSaved`**

Replace the three functions:

```typescript
async function initEditMode(id: string) {
  const result = await detailStore.fetchDetail(id)
  if (result.isSuccess) {
    const t = result.value
    form.value = {
      taxonomyId: t.taxonomyId,
      parentId: t.parentId,
      name: t.name,
      presentation: t.presentation,
      slug: t.slug,
      description: t.description,
      position: t.position,
      metaTitle: t.metaTitle,
      metaDescription: t.metaDescription,
      metaKeywords: t.metaKeywords,
      imageUrl: t.imageUrl,
      squareImageUrl: t.squareImageUrl,
      automatic: t.automatic,
      rulesMatchPolicy: t.rulesMatchPolicy,
      sortOrder: t.sortOrder,
      hideFromNav: t.hideFromNav,
    }
    formLoaded.value = true

    await Promise.all([loadParents(result.value.taxonomyId), loadRules(id)])
  } else {
    handleResult(result)
    router.push('/catalog/taxons')
  }
}

async function loadParents(taxonomyId: string) {
  await treeStore.fetchTree(taxonomyId)
  const flat: { label: string; value: string }[] = [{ label: '(None — root level)', value: '' }]
  function walk(nodes: any[], depth: number) {
    for (const n of nodes) {
      flat.push({ label: '  '.repeat(depth) + '|-- ' + n.name, value: n.id })
      if (n.children?.length) walk(n.children, depth + 1)
    }
  }
  walk(treeStore.tree, 1)
  parentOptions.value = flat
}

async function loadRules(taxonId: string) {
  await detailStore.fetchRules(taxonId)
}
```

Replace `onRuleSaved`:

```typescript
function onRuleSaved() {
  loadRules(route.params.id as string)
}
```

- [ ] **Step 4: Update the rules table bindings**

In the template, change the Rules DataTable bindings:

```html
<DataTable size="large" :value="detailStore.rules" :loading="detailStore.rulesLoading" data-key="id">
```

(Previously `:value="rules" :loading="rulesLoading"`.)

- [ ] **Step 5: Verify no stale references**

Grep the file for `rulesLoading`, `refreshRules`, `rules.value`, and `usePagedQuery` — none should remain. The `watch(() => route.params.id, ...)` and `onMounted` (which still calls `taxonomyStore.fetchActive()`, then `initEditMode`/`loadParents`) are unchanged.

- [ ] **Step 6: Run lint, type-check, and the unit suite**

Run: `cd app/Admin && pnpm run lint && pnpm run build`
Expected: no lint errors; `vue-tsc` and `vite build` succeed.

- [ ] **Step 7: Commit**

```bash
cd app/Admin
git add app/Admin/src/features/catalog/views/TaxonDetail.vue
git commit -m "fix(admin): load taxon detail, parents and rules via stores"
```

---

### Task 8: Full verification

**Files:** none (verification only).

- [ ] **Step 1: Run the full Admin verification**

```bash
cd app/Admin && pnpm run lint && pnpm run test:unit && pnpm run build
```

Expected: all lint checks pass, the full Vitest suite (existing ~500 tests + 11 new store/type tests) passes, `vue-tsc` and `vite build` succeed with zero errors.

- [ ] **Step 2: Confirm the design doc is current**

The spec at `docs/superpowers/specs/2026-07-31-catalog-taxon-store-design.md` describes the 3-store split. If any implementation detail diverged (e.g., a field name), update the spec and commit:

```bash
git add docs/superpowers/specs/2026-07-31-catalog-taxon-store-design.md
git commit -m "docs: sync taxon store design spec with implementation"
```

- [ ] **Step 3: Report completion**

Summarize: three stores created, both views rewired, `taxonomyStore` page-size fix, and the `defaultSort: ['lft']` validation bug fixed (now `['position']`).
