# Remove Forced Pinia Stores from Admin Frontend — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Delete all Pinia stores except `authStore` from `app/Admin`, replacing the 7 used ones with composables + api services, and fully rewrite the 9 affected views with Code Commenting Standard v3.0 labels.

**Architecture:** A shared `useActiveList` composable (fetch-once Select-option pattern) replaces 4 near-identical stores; feature-level wrappers (`useActiveCountries`, `useActiveStockLocations`, `useActiveTaxonomies`, `useActiveUsers`) sit on top. The existing `usePagedQuery` infrastructure absorbs the paged-list stores (`useTaxonList` extended with a reactive taxonomy scope, new `useTaxonDetail`). Views are rewritten in place, keeping behavior identical.

**Tech Stack:** Vue 3 + TypeScript (script setup), PrimeVue 5, Vitest, pnpm. No new dependencies.

**Spec:** `docs/superpowers/specs/2026-08-03-admin-remove-forced-pinia-stores-design.md`

## Global Constraints

- `app/Admin` only. NEVER touch `features/auth/stores/authStore.ts`, `features/auth/stores/index.ts`, or `main.ts` — Pinia stays.
- Comments on new/rewritten code follow `guide/code-commenting/` v3.0: one label per comment, `Label: Capitalised imperative body`, max 100 chars, WHY not WHAT, no comments on trivial lines (AP-1/AP-3).
- View behavior must stay identical — only the state source changes.
- Verify every task with: `cd app/Admin && pnpm exec vue-tsc --build && pnpm run lint` (fast) and `pnpm run test:unit` where tests are touched. NOTE: `vue-tsc --noEmit` is a false pass — the root `tsconfig.json` is solution-style (`files: []` + references); `vue-tsc --build` is the repo's real typecheck (the `type-check` script).
- Commit only the files listed in the task. NEVER stage: `app/Storefront`, `app/legacy/sakai-vue`, `service/Api` WIP, or anything outside `app/Admin`.
- Grep gate (final state): `use\w+Store\(` may only match `authStore` (views, `router/guards.ts`) and `features/auth/stores/__tests__/authStore.spec.ts`.

---

### Task 1: Shared `useActiveList` composable + spec

**Files:**
- Create: `app/Admin/src/shared/composables/useActiveList.ts`
- Create: `app/Admin/src/shared/composables/__tests__/useActiveList.spec.ts`
- Modify: `app/Admin/src/shared/composables/index.ts`

**Interfaces:**
- Produces: `useActiveList<T>(fetcher: () => Promise<PagedResult<T>>, options?: { immediate?: boolean })` → `{ items: Ref<T[]>, loaded: Ref<boolean>, loading: Ref<boolean>, error: Ref<string | null>, load: () => Promise<void>, reset: () => void }`. `load()` dedupes via `loaded`; failure leaves `loaded` false so a later call retries.

- [ ] **Step 1: Write the failing spec**

Create `app/Admin/src/shared/composables/__tests__/useActiveList.spec.ts`:

```ts
import { describe, it, expect, vi } from 'vitest'
import { useActiveList } from '../useActiveList'
import type { PagedResult } from '@/shared/types/result'

function okResult(overrides: Partial<PagedResult<{ id: string; name: string }>> = {}): PagedResult<{ id: string; name: string }> {
  return {
    isSuccess: true,
    statusCode: 200,
    items: [{ id: '1', name: 'Test' }],
    page: 1,
    pageSize: 20,
    totalCount: 1,
    totalPages: 1,
    errors: [],
    message: null,
    metadata: null,
    ...overrides,
  }
}

describe('useActiveList', () => {
  it('loads items on demand and dedupes subsequent load calls', async () => {
    const fetcher = vi.fn().mockResolvedValue(okResult())
    const { items, load } = useActiveList<{ id: string; name: string }>(fetcher)

    await load()
    await load()

    expect(fetcher).toHaveBeenCalledTimes(1)
    expect(items.value).toHaveLength(1)
    expect(items.value[0]!.name).toBe('Test')
  })

  it('exposes the failure message and allows retry after reset', async () => {
    const fetcher = vi.fn()
      .mockResolvedValueOnce({ ...okResult(), isSuccess: false, message: 'boom' })
      .mockResolvedValueOnce(okResult())
    const { error, items, load, reset } = useActiveList<{ id: string; name: string }>(fetcher)

    await load()
    expect(error.value).toBe('boom')

    reset()
    await load()
    expect(items.value).toHaveLength(1)
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd app/Admin && pnpm exec vitest run src/shared/composables/__tests__/useActiveList.spec.ts`
Expected: FAIL — cannot find module `../useActiveList`

- [ ] **Step 3: Write the composable**

Create `app/Admin/src/shared/composables/useActiveList.ts`:

```ts
import { ref } from 'vue'
import type { Ref } from 'vue'
import type { PagedResult } from '@/shared/types/result'

export interface UseActiveListOptions {
  immediate?: boolean
}

export interface ActiveListState<T> {
  items: Ref<T[]>
  loaded: Ref<boolean>
  loading: Ref<boolean>
  error: Ref<string | null>
  load: () => Promise<void>
  reset: () => void
}

export function useActiveList<T>(
  fetcher: () => Promise<PagedResult<T>>,
  options?: UseActiveListOptions,
): ActiveListState<T> {
  const items = ref<T[]>([]) as Ref<T[]>
  const loaded = ref(false)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function load(): Promise<void> {
    // Cache: Skip refetch once the options list was loaded for this mount
    if (loaded.value) return
    loading.value = true
    error.value = null
    const result = await fetcher()
    loading.value = false
    if (result.isSuccess) {
      items.value = result.items
      loaded.value = true
    } else {
      error.value = result.message ?? result.errors[0]?.message ?? 'Request failed.'
    }
  }

  function reset() {
    items.value = []
    loaded.value = false
    loading.value = false
    error.value = null
  }

  if (options?.immediate) {
    load()
  }

  return { items, loaded, loading, error, load, reset }
}
```

- [ ] **Step 4: Export from the shared barrel**

Modify `app/Admin/src/shared/composables/index.ts` — after the `usePagedQuery` exports add:

```ts
export { useActiveList } from './useActiveList'
export type { ActiveListState, UseActiveListOptions } from './useActiveList'
```

- [ ] **Step 5: Run tests to verify they pass**

Run: `cd app/Admin && pnpm exec vitest run src/shared/composables/__tests__/useActiveList.spec.ts`
Expected: 2 passed

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/shared/composables/useActiveList.ts app/Admin/src/shared/composables/__tests__/useActiveList.spec.ts app/Admin/src/shared/composables/index.ts
git commit -m "feat(admin): add shared useActiveList composable for fetch-once option lists"
```

---

### Task 2: Feature wrappers — `useActiveCountries`, `useActiveStockLocations`, `useActiveTaxonomies`, `useActiveUsers`

**Files:**
- Create: `app/Admin/src/features/location/composables/useActiveCountries.ts`
- Create: `app/Admin/src/features/location/composables/__tests__/useActiveCountries.spec.ts`
- Modify: `app/Admin/src/features/location/composables/index.ts`
- Create: `app/Admin/src/features/inventory/composables/useActiveStockLocations.ts`
- Create: `app/Admin/src/features/inventory/composables/__tests__/useActiveStockLocations.spec.ts`
- Modify: `app/Admin/src/features/inventory/composables/index.ts`
- Create: `app/Admin/src/features/catalog/composables/useActiveTaxonomies.ts`
- Create: `app/Admin/src/features/catalog/composables/__tests__/useActiveTaxonomies.spec.ts`
- Modify: `app/Admin/src/features/catalog/composables/index.ts`
- Create: `app/Admin/src/features/identity/composables/useActiveUsers.ts`
- Create: `app/Admin/src/features/identity/composables/__tests__/useActiveUsers.spec.ts`
- Modify: `app/Admin/src/features/identity/composables/index.ts`

**Interfaces:**
- Consumes: `useActiveList<T>` from Task 1.
- Produces: `useActiveCountries()` → `ActiveListState<CountryListItem>`; `useActiveStockLocations()` → `ActiveListState<StockLocationListItem>`; `useActiveTaxonomies()` → `ActiveListState<TaxonomyListItem>`; `useActiveUsers()` → `ActiveListState<UserListItem>`. Each returns `{ items, loaded, loading, error, load, reset }`.

- [ ] **Step 1: Write the four wrappers**

Create `app/Admin/src/features/location/composables/useActiveCountries.ts`:

```ts
import { useActiveList } from '@/shared/composables'
import type { CountryListItem } from '../types/country'
import { CountryApi } from '../services/countryApi'

export function useActiveCountries() {
  // Call: Location service — active countries for form Select options
  return useActiveList<CountryListItem>(() => CountryApi.getCountries({ isActive: true }))
}
```

Create `app/Admin/src/features/inventory/composables/useActiveStockLocations.ts`:

```ts
import { useActiveList } from '@/shared/composables'
import type { StockLocationListItem } from '../types/stockLocation'
import { StockLocationApi } from '../services/stockLocationApi'

export function useActiveStockLocations() {
  // Call: Inventory service — active stock locations for filter and form Selects
  return useActiveList<StockLocationListItem>(() =>
    StockLocationApi.getStockLocations({ pageSize: 100, sortBy: 'name', sortDirection: 'asc' }),
  )
}
```

Create `app/Admin/src/features/catalog/composables/useActiveTaxonomies.ts`:

```ts
import { useActiveList } from '@/shared/composables'
import type { TaxonomyListItem } from '../types/taxonomy'
import { TaxonomyApi } from '../services/taxonomyApi'

export function useActiveTaxonomies() {
  // Call: Catalog service — taxonomies for filter and form Selects
  return useActiveList<TaxonomyListItem>(() => TaxonomyApi.getTaxonomies({}))
}
```

Create `app/Admin/src/features/identity/composables/useActiveUsers.ts`:

```ts
import { useActiveList } from '@/shared/composables'
import type { UserListItem } from '../types/user'
import { UserApi } from '../services/userApi'

export function useActiveUsers() {
  // Call: Identity service — registered users for the dashboard stat card
  return useActiveList<UserListItem>(() => UserApi.getUsers({}))
}
```

`UserListItem` is imported from `types/user.ts` (verified — it is defined there and re-exported by the types barrel).

- [ ] **Step 2: Write the four specs** (one per wrapper; the api module is mocked)

Create `app/Admin/src/features/location/composables/__tests__/useActiveCountries.spec.ts`:

```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useActiveCountries } from '../useActiveCountries'
import { CountryApi } from '../../services/countryApi'
import type { PagedResult } from '@/shared/types/result'
import type { CountryListItem } from '../../types/country'

vi.mock('../../services/countryApi', () => ({
  CountryApi: { getCountries: vi.fn() },
}))

const mockGetCountries = vi.mocked(CountryApi.getCountries)

function okResult(items: CountryListItem[] = [{ id: 'us', name: 'United States', isoCode: 'US', callingCode: '+1', statesRequired: true, isActive: true }]): PagedResult<CountryListItem> {
  return { isSuccess: true, statusCode: 200, items, page: 1, pageSize: 20, totalCount: items.length, totalPages: 1, errors: [], message: null, metadata: null }
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useActiveCountries', () => {
  it('loads active countries via the CountryApi', async () => {
    mockGetCountries.mockResolvedValue(okResult())
    const { items, load } = useActiveCountries()

    await load()

    expect(mockGetCountries).toHaveBeenCalledWith({ isActive: true })
    expect(items.value).toHaveLength(1)
    expect(items.value[0]!.name).toBe('United States')
  })
})
```

Create `app/Admin/src/features/inventory/composables/__tests__/useActiveStockLocations.spec.ts`:

```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useActiveStockLocations } from '../useActiveStockLocations'
import { StockLocationApi } from '../../services/stockLocationApi'
import type { PagedResult } from '@/shared/types/result'
import type { StockLocationListItem } from '../../types/stockLocation'

vi.mock('../../services/stockLocationApi', () => ({
  StockLocationApi: { getStockLocations: vi.fn() },
}))

const mockGetStockLocations = vi.mocked(StockLocationApi.getStockLocations)

function okResult(items: StockLocationListItem[] = [{ id: 'loc1', name: 'Warehouse A', code: 'WH-A', active: true, default: false, backorderableDefault: false, propagateAllVariants: false, position: 1, createdAtUtc: '2026-01-01T00:00:00Z' }]): PagedResult<StockLocationListItem> {
  return { isSuccess: true, statusCode: 200, items, page: 1, pageSize: 20, totalCount: items.length, totalPages: 1, errors: [], message: null, metadata: null }
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useActiveStockLocations', () => {
  it('loads all stock locations sorted by name', async () => {
    mockGetStockLocations.mockResolvedValue(okResult())
    const { items, load } = useActiveStockLocations()

    await load()

    expect(mockGetStockLocations).toHaveBeenCalledWith({ pageSize: 100, sortBy: 'name', sortDirection: 'asc' })
    expect(items.value).toHaveLength(1)
  })
})
```

Create `app/Admin/src/features/catalog/composables/__tests__/useActiveTaxonomies.spec.ts`:

```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useActiveTaxonomies } from '../useActiveTaxonomies'
import { TaxonomyApi } from '../../services/taxonomyApi'
import type { PagedResult } from '@/shared/types/result'
import type { TaxonomyListItem } from '../../types/taxonomy'

vi.mock('../../services/taxonomyApi', () => ({
  TaxonomyApi: { getTaxonomies: vi.fn() },
}))

const mockGetTaxonomies = vi.mocked(TaxonomyApi.getTaxonomies)

function okResult(items: TaxonomyListItem[] = [{ id: 't1', name: 'Category', presentation: 'Category', position: 1, taxonsCount: 0, createdAtUtc: '2026-01-01T00:00:00Z', modifiedAtUtc: null }]): PagedResult<TaxonomyListItem> {
  return { isSuccess: true, statusCode: 200, items, page: 1, pageSize: 20, totalCount: items.length, totalPages: 1, errors: [], message: null, metadata: null }
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useActiveTaxonomies', () => {
  it('loads all taxonomies via the TaxonomyApi', async () => {
    mockGetTaxonomies.mockResolvedValue(okResult())
    const { items, load } = useActiveTaxonomies()

    await load()

    expect(mockGetTaxonomies).toHaveBeenCalledWith({})
    expect(items.value).toHaveLength(1)
  })
})
```

Create `app/Admin/src/features/identity/composables/__tests__/useActiveUsers.spec.ts`:

```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useActiveUsers } from '../useActiveUsers'
import { UserApi } from '../../services/userApi'
import type { PagedResult } from '@/shared/types/result'
import type { UserListItem } from '../../types/user'

vi.mock('../../services/userApi', () => ({
  UserApi: { getUsers: vi.fn() },
}))

const mockGetUsers = vi.mocked(UserApi.getUsers)

function okResult(items: UserListItem[] = [{ id: 'u1', email: 'admin@shop.local', userName: 'admin', firstName: 'Admin', lastName: 'User', emailConfirmed: true, phoneNumberConfirmed: false, fullName: 'Admin User', isActive: true }]): PagedResult<UserListItem> {
  return { isSuccess: true, statusCode: 200, items, page: 1, pageSize: 20, totalCount: items.length, totalPages: 1, errors: [], message: null, metadata: null }
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useActiveUsers', () => {
  it('loads all users via the UserApi', async () => {
    mockGetUsers.mockResolvedValue(okResult())
    const { items, load } = useActiveUsers()

    await load()

    expect(mockGetUsers).toHaveBeenCalledWith({})
    expect(items.value).toHaveLength(1)
  })
})
```

The fixture matches `UserListItem` exactly (verified against `app/Admin/src/features/identity/types/user.ts`).

- [ ] **Step 3: Export wrappers from feature barrels**

- `app/Admin/src/features/location/composables/index.ts` — replace the whole file (currently `export {}`) with:

```ts
export { useActiveCountries } from './useActiveCountries'
```

- `app/Admin/src/features/inventory/composables/index.ts` — add `export { useActiveStockLocations } from './useActiveStockLocations'` (keep existing exports).
- `app/Admin/src/features/catalog/composables/index.ts` — add `export { useActiveTaxonomies } from './useActiveTaxonomies'` (keep existing exports).
- `app/Admin/src/features/identity/composables/index.ts` — add `export { useActiveUsers } from './useActiveUsers'` (keep existing exports).

- [ ] **Step 4: Run the new specs**

Run: `cd app/Admin && pnpm exec vitest run src/features/location/composables src/features/inventory/composables src/features/catalog/composables src/features/identity/composables`
Expected: all new specs pass (4 files)

- [ ] **Step 5: Verify and commit**

Run: `cd app/Admin && pnpm exec vue-tsc --build && pnpm run lint`
Expected: no errors

```bash
git add app/Admin/src/features/location/composables app/Admin/src/features/inventory/composables app/Admin/src/features/catalog/composables app/Admin/src/features/identity/composables
git commit -m "feat(admin): add fetch-active composables for countries, stock locations, taxonomies, users"
```

---

### Task 3: Extend `useTaxonList` with taxonomy scope + new `useTaxonDetail`

**Files:**
- Modify: `app/Admin/src/features/catalog/composables/useTaxonList.ts`
- Create: `app/Admin/src/features/catalog/composables/useTaxonDetail.ts`
- Create: `app/Admin/src/features/catalog/composables/__tests__/useTaxonDetail.spec.ts`
- Modify: `app/Admin/src/features/catalog/composables/index.ts`

**Interfaces:**
- Consumes: `usePagedQuery` (shared), `TaxonApi`, `TaxonRuleApi`.
- Produces: `useTaxonList(taxonomyId?: Ref<string | null>, options?: UsePagedQueryOptions)` → `PagedQueryState<TaxonListItem>` (URL is `api/catalog/taxons/list?taxonomyId=...` when scoped, else `api/catalog/taxons`; default sort `['position']`).
- Produces: `useTaxonDetail()` → `{ currentTaxon: Ref<TaxonDetail | null>, detailLoading: Ref<boolean>, fetchDetail(id: string): Promise<Result<TaxonDetail>>, rules: Ref<TaxonRuleListItem[]>, rulesLoading: Ref<boolean>, fetchRules(taxonId: string): Promise<PagedResult<TaxonRuleListItem>> }`.

- [ ] **Step 1: Extend `useTaxonList`**

Replace the whole content of `app/Admin/src/features/catalog/composables/useTaxonList.ts`:

```ts
import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'
import { CATALOG } from '@/shared/constants/api'
import type { Ref } from 'vue'
import { TAXON_FILTER_FIELDS, TAXON_SORT_FIELDS } from '../types/taxon'
import type { TaxonListItem } from '../types/taxon'

export function useTaxonList(taxonomyId?: Ref<string | null>, options?: UsePagedQueryOptions) {
  return usePagedQuery<TaxonListItem>(
    // Transform: Scope the taxon endpoint to the selected taxonomy when set
    () => (taxonomyId?.value ? `${CATALOG}/taxons/list?taxonomyId=${taxonomyId.value}` : `${CATALOG}/taxons`),
    {
      allowedFilterFields: TAXON_FILTER_FIELDS,
      allowedSortFields: TAXON_SORT_FIELDS,
      defaultSort: ['position'],
      ...options,
    },
  )
}
```

- [ ] **Step 2: Create `useTaxonDetail`**

Create `app/Admin/src/features/catalog/composables/useTaxonDetail.ts`:

```ts
import { ref } from 'vue'
import type { Ref } from 'vue'
import type { Result, PagedResult } from '@/shared/types'
import type { TaxonDetail } from '../types/taxon'
import type { TaxonRuleListItem } from '../types/taxonRule'
import { TaxonApi } from '../services/taxonApi'
import { TaxonRuleApi } from '../services/taxonRuleApi'

export interface UseTaxonDetailState {
  currentTaxon: Ref<TaxonDetail | null>
  detailLoading: Ref<boolean>
  rules: Ref<TaxonRuleListItem[]>
  rulesLoading: Ref<boolean>
  fetchDetail: (id: string) => Promise<Result<TaxonDetail>>
  fetchRules: (taxonId: string) => Promise<PagedResult<TaxonRuleListItem>>
}

export function useTaxonDetail(): UseTaxonDetailState {
  const currentTaxon = ref<TaxonDetail | null>(null)
  const detailLoading = ref(false)
  const rules = ref<TaxonRuleListItem[]>([])
  const rulesLoading = ref(false)

  async function fetchDetail(id: string): Promise<Result<TaxonDetail>> {
    detailLoading.value = true
    // Call: Catalog service — taxon detail that backs the edit form
    const result = await TaxonApi.getTaxon(id)
    detailLoading.value = false
    if (result.isSuccess) {
      currentTaxon.value = result.value
    }
    return result
  }

  async function fetchRules(taxonId: string): Promise<PagedResult<TaxonRuleListItem>> {
    rulesLoading.value = true
    // Call: Catalog service — taxon rules for the Rules tab
    const result = await TaxonRuleApi.getRules(taxonId)
    rulesLoading.value = false
    if (result.isSuccess) {
      rules.value = result.items
    }
    return result
  }

  return { currentTaxon, detailLoading, fetchDetail, rules, rulesLoading, fetchRules }
}
```

- [ ] **Step 3: Write the spec**

Create `app/Admin/src/features/catalog/composables/__tests__/useTaxonDetail.spec.ts`:

```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useTaxonDetail } from '../useTaxonDetail'
import { TaxonApi } from '../../services/taxonApi'
import { TaxonRuleApi } from '../../services/taxonRuleApi'
import type { Result, PagedResult } from '@/shared/types'
import type { TaxonDetail } from '../../types/taxon'
import type { TaxonRuleListItem } from '../../types/taxonRule'

vi.mock('../../services/taxonApi', () => ({
  TaxonApi: { getTaxon: vi.fn() },
}))
vi.mock('../../services/taxonRuleApi', () => ({
  TaxonRuleApi: { getRules: vi.fn() },
}))

const mockGetTaxon = vi.mocked(TaxonApi.getTaxon)
const mockGetRules = vi.mocked(TaxonRuleApi.getRules)

function okDetail(): Result<TaxonDetail> {
  const detail: TaxonDetail = {
    id: 't1', taxonomyId: 'tax1', parentId: null,
    name: 'Shoes', presentation: 'Shoes', description: null, slug: 'shoes', position: 1,
    metaTitle: null, metaDescription: null, metaKeywords: null,
    imageUrl: null, squareImageUrl: null, automatic: false,
    rulesMatchPolicy: 'All', sortOrder: 'Manual', hideFromNav: false,
    parentName: null, taxonomyName: null, lft: 1, rgt: 2, depth: 0,
    childrenCount: 0, taxonRuleCount: 0, productCount: 0,
    permalink: '/shoes', prettyName: 'Shoes',
    createdAtUtc: '2026-01-01T00:00:00Z', modifiedAtUtc: null,
  }
  return { isSuccess: true, statusCode: 200, value: detail, errors: [], message: null, metadata: null }
}

function okRules(): PagedResult<TaxonRuleListItem> {
  return { isSuccess: true, statusCode: 200, items: [{ id: 'r1', taxonId: 't1', type: 'Name', matchPolicy: 'All', value: 'x' }], page: 1, pageSize: 20, totalCount: 1, totalPages: 1, errors: [], message: null, metadata: null }
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useTaxonDetail', () => {
  it('loads the taxon detail and stores it in currentTaxon', async () => {
    mockGetTaxon.mockResolvedValue(okDetail())
    const { currentTaxon, fetchDetail } = useTaxonDetail()

    const result = await fetchDetail('t1')

    expect(mockGetTaxon).toHaveBeenCalledWith('t1')
    expect(result.isSuccess).toBe(true)
    expect(currentTaxon.value?.name).toBe('Shoes')
  })

  it('loads the taxon rules into rules', async () => {
    mockGetRules.mockResolvedValue(okRules())
    const { rules, fetchRules } = useTaxonDetail()

    const result = await fetchRules('t1')

    expect(mockGetRules).toHaveBeenCalledWith('t1')
    expect(result.isSuccess).toBe(true)
    expect(rules.value).toHaveLength(1)
  })
})
```

All fixtures above match the exact type shapes verified against `types/country.ts`, `types/stockLocation.ts`, `types/taxonomy.ts`, `types/taxon.ts`, `types/taxonRule.ts`, and `types/user.ts`.

- [ ] **Step 4: Export from the catalog barrel**

Modify `app/Admin/src/features/catalog/composables/index.ts` — add:

```ts
export { useTaxonDetail } from './useTaxonDetail'
export type { UseTaxonDetailState } from './useTaxonDetail'
```

- [ ] **Step 5: Run the new spec**

Run: `cd app/Admin && pnpm exec vitest run src/features/catalog/composables/__tests__/useTaxonDetail.spec.ts`
Expected: 2 passed

- [ ] **Step 6: Verify and commit**

Run: `cd app/Admin && pnpm exec vue-tsc --build && pnpm run lint`
Expected: no errors

```bash
git add app/Admin/src/features/catalog/composables/useTaxonList.ts app/Admin/src/features/catalog/composables/useTaxonDetail.ts app/Admin/src/features/catalog/composables/__tests__/useTaxonDetail.spec.ts app/Admin/src/features/catalog/composables/index.ts
git commit -m "feat(catalog): extend useTaxonList with taxonomy scope and add useTaxonDetail composable"
```

---

### Task 4: Delete the 12 dead stores, their specs, and emptied barrels

**Files:**
- Delete: `app/Admin/src/features/catalog/stores/productStore.ts`, `app/Admin/src/features/catalog/stores/optionTypeStore.ts`
- Delete: `app/Admin/src/features/identity/stores/roleStore.ts`
- Delete: `app/Admin/src/features/inventory/stores/stockTransferStore.ts`, `app/Admin/src/features/inventory/stores/stockItemStore.ts`
- Delete: `app/Admin/src/features/location/stores/stateStore.ts`
- Delete: `app/Admin/src/features/ordering/stores/orderStore.ts` + `app/Admin/src/features/ordering/stores/index.ts`
- Delete: `app/Admin/src/features/payment/stores/paymentMethodStore.ts` + `app/Admin/src/features/payment/stores/index.ts`
- Delete: `app/Admin/src/features/profile/stores/profileStore.ts`, `app/Admin/src/features/profile/stores/addressStore.ts` + `app/Admin/src/features/profile/stores/index.ts`
- Delete: `app/Admin/src/features/shipping/stores/shippingRateStore.ts`, `app/Admin/src/features/shipping/stores/shippingMethodStore.ts` + `app/Admin/src/features/shipping/stores/index.ts`
- Delete specs: `features/identity/__tests__/stores/roleStore.spec.ts`, `features/inventory/__tests__/stores/stockTransferStore.spec.ts`, `features/inventory/__tests__/stores/stockItemStore.spec.ts`, `features/ordering/__tests__/stores/orderStore.spec.ts`, `features/payment/__tests__/stores/paymentMethodStore.spec.ts`, `features/profile/__tests__/stores/profileStore.spec.ts`, `features/profile/__tests__/stores/addressStore.spec.ts`, `features/shipping/__tests__/stores/shippingRateStore.spec.ts`, `features/shipping/__tests__/stores/shippingMethodStore.spec.ts` (all under `app/Admin/src/`)
- Modify: `app/Admin/src/features/catalog/stores/index.ts` — remove `export { useOptionTypeStore } ...` and `export { useProductStore } ...` (keep taxonomy/taxon/taxonDetail exports)
- Modify: `app/Admin/src/features/identity/stores/index.ts` — remove `export { useRoleStore } ...` (keep userStore)
- Modify: `app/Admin/src/features/inventory/stores/index.ts` — remove `export { useStockItemStore } ...` and `export { useStockTransferStore } ...` (keep stockLocationStore)
- Modify: `app/Admin/src/features/ordering/index.ts`, `app/Admin/src/features/payment/index.ts`, `app/Admin/src/features/profile/index.ts`, `app/Admin/src/features/shipping/index.ts` — remove the `export * from './stores'` line (the barrels are deleted)

**Interfaces:**
- Consumes: nothing — these stores have zero consumers (verified by design exploration).
- Produces: nothing. After this task the following `stores/` dirs are gone: ordering, payment, profile, shipping. Catalog/identity/inventory barrels keep their still-used exports (pruned of the deleted ones).

- [ ] **Step 1: Verify the dead stores have zero consumers**

Run: `cd app/Admin && grep -rn "useProductStore\|useOptionTypeStore\|useRoleStore\|useStockTransferStore\|useStockItemStore\|useStateStore\|useOrderStore\|usePaymentMethodStore\|useProfileStore\|useAddressStore\|useShippingRateStore\|useShippingMethodStore" src --include=*.vue --include=*.ts | grep -v __tests__`
Expected: no output

- [ ] **Step 2: Delete the files and prune the barrels**

```bash
cd app/Admin
git rm \
  src/features/catalog/stores/productStore.ts \
  src/features/catalog/stores/optionTypeStore.ts \
  src/features/identity/stores/roleStore.ts \
  src/features/inventory/stores/stockTransferStore.ts \
  src/features/inventory/stores/stockItemStore.ts \
  src/features/location/stores/stateStore.ts \
  src/features/ordering/stores/orderStore.ts \
  src/features/ordering/stores/index.ts \
  src/features/payment/stores/paymentMethodStore.ts \
  src/features/payment/stores/index.ts \
  src/features/profile/stores/profileStore.ts \
  src/features/profile/stores/addressStore.ts \
  src/features/profile/stores/index.ts \
  src/features/shipping/stores/shippingRateStore.ts \
  src/features/shipping/stores/shippingMethodStore.ts \
  src/features/shipping/stores/index.ts \
  src/features/identity/__tests__/stores/roleStore.spec.ts \
  src/features/inventory/__tests__/stores/stockTransferStore.spec.ts \
  src/features/inventory/__tests__/stores/stockItemStore.spec.ts \
  src/features/ordering/__tests__/stores/orderStore.spec.ts \
  src/features/payment/__tests__/stores/paymentMethodStore.spec.ts \
  src/features/profile/__tests__/stores/profileStore.spec.ts \
  src/features/profile/__tests__/stores/addressStore.spec.ts \
  src/features/shipping/__tests__/stores/shippingRateStore.spec.ts \
  src/features/shipping/__tests__/stores/shippingMethodStore.spec.ts
```

If `git rm` reports a missing file, it did not exist — note it and continue (that matches the spec's "verify during implementation" note).

Then prune the surviving barrels and feature indexes (plan amendment 2026-08-03, approved by human partner):

```bash
cd app/Admin
# catalog/stores/index.ts: remove the optionTypeStore and productStore export lines
# identity/stores/index.ts: remove the roleStore export line
# inventory/stores/index.ts: remove the stockItemStore and stockTransferStore export lines
# features/ordering/index.ts, features/payment/index.ts, features/profile/index.ts,
# features/shipping/index.ts: remove the 'export * from ./stores' line each
```

- [ ] **Step 3: Verify typecheck**

Run: `cd app/Admin && pnpm exec vue-tsc --build && pnpm run lint`
Expected: no errors (proves nothing imported the deleted stores or barrels)

- [ ] **Step 4: Run the full test suite**

Run: `cd app/Admin && pnpm run test:unit`
Expected: all pass, with 9 fewer spec files than before

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src
git commit -m "chore(admin): remove dead Pinia stores and empty store barrels"
```

---

### Task 5: Rewrite DashboardPage without stores

**Files:**
- Modify: `app/Admin/src/features/dashboard/views/DashboardPage.vue`

**Interfaces:**
- Consumes: `useDashboard()` from `../composables/useDashboard` (exists), `useActiveUsers()` from `@/features/identity/composables/useActiveUsers` (Task 2).
- Produces: none — behavior identical: 4 metric cards, navigate on click.

- [ ] **Step 1: Rewrite the script block**

Replace the entire `<script setup>` block of `app/Admin/src/features/dashboard/views/DashboardPage.vue` (lines 1–51) with:

```vue
<script setup lang="ts">
import { onMounted, computed } from 'vue'
import Card from 'primevue/card'
import { useRouter } from 'vue-router'
import { useDashboard } from '../composables/useDashboard'
import { useActiveUsers } from '@/features/identity/composables/useActiveUsers'

const router = useRouter()
const { summary, fetchDashboard } = useDashboard()
const { items: activeUsers, load: loadActiveUsers } = useActiveUsers()

const metrics = computed(() => [
  {
    label: 'Total Products',
    // Compute: Default to zero until the dashboard summary arrives
    value: summary.value?.catalog.totalProducts ?? 0,
    icon: 'pi pi-box',
    color: 'border-t-blue-500',
    link: '/catalog/products',
  },
  {
    label: 'Orders Today',
    value: summary.value?.sales.orderCount ?? 0,
    icon: 'pi pi-shopping-cart',
    color: 'border-t-green-500',
    link: '/ordering/orders',
  },
  {
    label: 'Registered Users',
    value: activeUsers.value.length,
    icon: 'pi pi-users',
    color: 'border-t-purple-500',
    link: '/identity/users',
  },
  {
    label: 'Low Stock Items',
    value: summary.value?.inventory.lowStockCount ?? 0,
    icon: 'pi pi-exclamation-triangle',
    color: 'border-t-orange-500',
    link: '/inventory/stock-items',
  },
])

function navigateTo(path: string) {
  router.push(path)
}

onMounted(async () => {
  // Await: Summary and user count load in parallel on first paint
  await Promise.all([fetchDashboard(), loadActiveUsers()])
})
</script>
```

- [ ] **Step 2: Verify no template changes are needed**

The template already binds only `metrics` — no `dashboardStore`/`userStore` references. Confirm with:
Run: `cd app/Admin && grep -n "Store" src/features/dashboard/views/DashboardPage.vue`
Expected: no output

- [ ] **Step 3: Verify and commit**

Run: `cd app/Admin && pnpm exec vue-tsc --build && pnpm run lint`
Expected: no errors

```bash
git add app/Admin/src/features/dashboard/views/DashboardPage.vue
git commit -m "refactor(admin): rewrite dashboard page on useDashboard and useActiveUsers composables"
```

---

### Task 6: Rewrite TaxonsList on `useTaxonList` + `useActiveTaxonomies`

**Files:**
- Modify: `app/Admin/src/features/catalog/views/TaxonsList.vue`

**Interfaces:**
- Consumes: `useTaxonList(taxonomyId: Ref<string | null>, options)` (Task 3), `useActiveTaxonomies()` (Task 2), `TaxonApi`.
- Produces: none — behavior identical: taxonomy Select scopes the list, DataTable paging/sort/search, multi-delete.

- [ ] **Step 1: Rewrite the script block**

Replace the entire `<script setup>` block of `app/Admin/src/features/catalog/views/TaxonsList.vue` (lines 1–114) with:

```vue
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import type { DataTablePageEvent, DataTableSortEvent } from 'primevue/datatable'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { useNotify } from '@/shared/composables/useNotify'
import { TaxonApi } from '../services/taxonApi'
import { useTaxonList } from '../composables/useTaxonList'
import { useActiveTaxonomies } from '../composables/useActiveTaxonomies'
import type { TaxonListItem } from '../types/taxon'

const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()

const { dt, exportCSV } = useDataTableExport()
const selectedItems = ref<TaxonListItem[]>([])
const searchTerm = ref('')
const allowedSearchFields = ['name', 'slug']
// Initialize: Taxonomy scope from the route query; null means all taxonomies
const taxonomyId = ref<string | null>((route.query.taxonomyId as string) || null)

const {
  items,
  loading,
  totalCount,
  page,
  pageSize,
  setSearch,
  setPage,
  setPageSize,
  setSort,
  fetch: fetchTaxons,
  refresh,
} = useTaxonList(taxonomyId, {
  defaultSort: ['position'],
  defaultSearchFields: allowedSearchFields,
  defaultSearchMode: 'any',
  immediate: false,
})

const { items: activeTaxonomies, load: loadActiveTaxonomies } = useActiveTaxonomies()
const first = computed(() => (page.value - 1) * pageSize.value)

onMounted(async () => {
  // Await: Taxonomy options and the first taxon page load in parallel
  await Promise.all([loadActiveTaxonomies(), fetchTaxons()])
})

function onTaxonomyChange(id: string | null) {
  taxonomyId.value = id || null
  fetchTaxons()
  router.replace({ query: { ...route.query, taxonomyId: taxonomyId.value ?? undefined } })
}

function navigateToNew() {
  const query = taxonomyId.value ? `?taxonomyId=${taxonomyId.value}` : ''
  router.push(`/catalog/taxons/new${query}`)
}

function navigateToEdit(id: string) {
  router.push(`/catalog/taxons/${id}`)
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
      refresh()
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

- [ ] **Step 2: Apply the template edits**

In `app/Admin/src/features/catalog/views/TaxonsList.vue` template:

1. `:rows="taxonStore.pageSize"` → `:rows="pageSize"`
2. The taxonomy Select (lines ~162–171): `:model-value="taxonStore.selectedTaxonomyId"` → `:model-value="taxonomyId"` and `:options="taxonomyStore.activeTaxonomies"` → `:options="activeTaxonomies"`
3. Reload button: `@click="taxonStore.refresh"` → `@click="refresh"`

- [ ] **Step 3: Verify and commit**

Run: `cd app/Admin && pnpm exec vue-tsc --build && pnpm run lint`
Expected: no errors

```bash
git add app/Admin/src/features/catalog/views/TaxonsList.vue
git commit -m "refactor(catalog): rewrite taxons list on useTaxonList and useActiveTaxonomies composables"
```

---

### Task 7: Rewrite TaxonDetail on `useTaxonDetail` + `useActiveTaxonomies`

**Files:**
- Modify: `app/Admin/src/features/catalog/views/TaxonDetail.vue`

**Interfaces:**
- Consumes: `useTaxonDetail()` (Task 3), `useActiveTaxonomies()` (Task 2), `TaxonApi`, `TaxonRuleApi`.
- Produces: none — behavior identical: tabbed edit form, taxonomy Select, parent Select from `getList`, Rules tab with dialog.

- [ ] **Step 1: Replace the store bindings in the script block**

In `app/Admin/src/features/catalog/views/TaxonDetail.vue` `<script setup>` (lines 1–249):

1. Remove imports: `import { useTaxonomyStore } from '../stores/taxonomyStore'` and `import { useTaxonDetailStore } from '../stores/taxonDetailStore'`.
2. Add imports: `import { useTaxonDetail } from '../composables/useTaxonDetail'` and `import { useActiveTaxonomies } from '../composables/useActiveTaxonomies'`.
3. Replace (line 35–36):

```ts
const taxonomyStore = useTaxonomyStore()
const detailStore = useTaxonDetailStore()
```

with:

```ts
const { items: activeTaxonomies, load: loadActiveTaxonomies } = useActiveTaxonomies()
const { fetchDetail, fetchRules, rules, rulesLoading } = useTaxonDetail()
```

4. In `initEditMode` (line 90): `await detailStore.fetchDetail(id)` → `await fetchDetail(id)`; line 113 `await Promise.all([loadParents(result.value.taxonomyId), loadRules(id)])` stays.
5. In `loadRules` (line 131): `await detailStore.fetchRules(taxonId)` → `await fetchRules(taxonId)`.
6. In `onMounted` (line 135): `await taxonomyStore.fetchActive()` → `await loadActiveTaxonomies()`.
7. Add labeled comments at the load sites (guide v3.0), e.g. above `loadParents` add `// Call: Catalog service — sibling taxons scoped to the selected taxonomy` and above the onMounted `loadActiveTaxonomies()` add `// Await: Taxonomy options needed by the taxonomy Select`.

- [ ] **Step 2: Apply the template edits**

1. Line 284: `:options="taxonomyStore.activeTaxonomies"` → `:options="activeTaxonomies"`
2. Line 398: `:value="detailStore.rules" :loading="detailStore.rulesLoading"` → `:value="rules" :loading="rulesLoading"`

- [ ] **Step 3: Verify and commit**

Run: `cd app/Admin && pnpm exec vue-tsc --build && pnpm run lint`
Expected: no errors

```bash
git add app/Admin/src/features/catalog/views/TaxonDetail.vue
git commit -m "refactor(catalog): rewrite taxon detail on useTaxonDetail and useActiveTaxonomies composables"
```

---

### Task 8: Rewrite StatesList and StateDetail on `useActiveCountries`

**Files:**
- Modify: `app/Admin/src/features/location/views/StatesList.vue`
- Modify: `app/Admin/src/features/location/views/StateDetail.vue`

**Interfaces:**
- Consumes: `useActiveCountries()` (Task 2).
- Produces: none.

- [ ] **Step 1: Rewrite StatesList script bindings**

In `app/Admin/src/features/location/views/StatesList.vue`:

1. Replace `import { useCountryStore } from '../stores/countryStore'` with `import { useActiveCountries } from '../composables/useActiveCountries'`.
2. Replace `const countryStore = useCountryStore()` with:

```ts
const { items: activeCountries, load: loadActiveCountries } = useActiveCountries()
```

3. In `onMounted` (line 40): `countryStore.fetchActive()` → `loadActiveCountries()` with preceding comment `// Await: Country options for the country filter Select`.

- [ ] **Step 2: Edit StatesList template**

Line 153: `:options="countryStore.activeCountries"` → `:options="activeCountries"`

- [ ] **Step 3: Rewrite StateDetail script bindings**

In `app/Admin/src/features/location/views/StateDetail.vue`:

1. Replace `import { useCountryStore } from '../stores/countryStore'` with `import { useActiveCountries } from '../composables/useActiveCountries'`.
2. Replace `const countryStore = useCountryStore()` with:

```ts
const { items: activeCountries, load: loadActiveCountries } = useActiveCountries()
```

3. In `onMounted` (line 45): `countryStore.fetchActive()` → `loadActiveCountries()` with preceding comment `// Await: Country options for the country field Select`.

- [ ] **Step 4: Edit StateDetail template**

Line 126: `:options="countryStore.activeCountries"` → `:options="activeCountries"`

- [ ] **Step 5: Verify and commit**

Run: `cd app/Admin && pnpm exec vue-tsc --build && pnpm run lint`
Expected: no errors

```bash
git add app/Admin/src/features/location/views/StatesList.vue app/Admin/src/features/location/views/StateDetail.vue
git commit -m "refactor(location): rewrite states views on useActiveCountries composable"
```

---

### Task 9: Rewrite StockItemsList and StockItemDetail on `useActiveStockLocations`

**Files:**
- Modify: `app/Admin/src/features/inventory/views/StockItemsList.vue`
- Modify: `app/Admin/src/features/inventory/views/StockItemDetail.vue`

**Interfaces:**
- Consumes: `useActiveStockLocations()` (Task 2).
- Produces: none.

- [ ] **Step 1: Rewrite StockItemsList script bindings**

In `app/Admin/src/features/inventory/views/StockItemsList.vue`:

1. Replace `import { useStockLocationStore } from '../stores/stockLocationStore'` with `import { useActiveStockLocations } from '../composables/useActiveStockLocations'`.
2. Replace `const stockLocationStore = useStockLocationStore()` with:

```ts
const { items: activeStockLocations, load: loadActiveStockLocations } = useActiveStockLocations()
```

3. Keep the setup-level call (line 30) but replace `stockLocationStore.fetchActive()` with `loadActiveStockLocations()` (behavior identical: fetch starts immediately at setup).

- [ ] **Step 2: Edit StockItemsList template**

Line 108: `:options="stockLocationStore.activeStockLocations"` → `:options="activeStockLocations"`

- [ ] **Step 3: Rewrite StockItemDetail script bindings**

In `app/Admin/src/features/inventory/views/StockItemDetail.vue`:

1. Replace the store import with `import { useActiveStockLocations } from '../composables/useActiveStockLocations'` (find the import line in the script header, lines 1–30).
2. Replace `const stockLocationStore = useStockLocationStore()` with:

```ts
const { items: activeStockLocations, load: loadActiveStockLocations } = useActiveStockLocations()
```

3. In `onMounted` (line 93): `stockLocationStore.fetchActive()` → `loadActiveStockLocations()` with preceding comment `// Await: Stock location options for the form Select`.

- [ ] **Step 4: Edit StockItemDetail template**

Line 165: `:options="stockLocationStore.activeStockLocations"` → `:options="activeStockLocations"`

- [ ] **Step 5: Verify and commit**

Run: `cd app/Admin && pnpm exec vue-tsc --build && pnpm run lint`
Expected: no errors

```bash
git add app/Admin/src/features/inventory/views/StockItemsList.vue app/Admin/src/features/inventory/views/StockItemDetail.vue
git commit -m "refactor(inventory): rewrite stock item views on useActiveStockLocations composable"
```

---

### Task 10: Rewrite StockTransfersList and StockTransferDetail on `useActiveStockLocations`

**Files:**
- Modify: `app/Admin/src/features/inventory/views/StockTransfersList.vue`
- Modify: `app/Admin/src/features/inventory/views/StockTransferDetail.vue`

**Interfaces:**
- Consumes: `useActiveStockLocations()` (Task 2).
- Produces: none.

- [ ] **Step 1: Rewrite StockTransfersList script bindings**

In `app/Admin/src/features/inventory/views/StockTransfersList.vue`:

1. Replace `import { useStockLocationStore } from '../stores/stockLocationStore'` with `import { useActiveStockLocations } from '../composables/useActiveStockLocations'`.
2. Replace `const stockLocationStore = useStockLocationStore()` with:

```ts
const { items: activeStockLocations, load: loadActiveStockLocations } = useActiveStockLocations()
```

3. Keep the setup-level call (line 28) but replace `stockLocationStore.fetchActive()` with `loadActiveStockLocations()`.

- [ ] **Step 2: Edit StockTransfersList template**

Lines 104 and 114: `:options="stockLocationStore.activeStockLocations"` → `:options="activeStockLocations"` (both the source and destination Selects)

- [ ] **Step 3: Rewrite StockTransferDetail script bindings**

In `app/Admin/src/features/inventory/views/StockTransferDetail.vue`:

1. Replace the store import with `import { useActiveStockLocations } from '../composables/useActiveStockLocations'` (find the import in the script header, lines 1–30).
2. Replace `const stockLocationStore = useStockLocationStore()` with:

```ts
const { items: activeStockLocations, load: loadActiveStockLocations } = useActiveStockLocations()
```

3. In `locationName` (line 164–166):

```ts
function locationName(locationId: string): string {
  // Filter: Resolve the location name from the active locations list; fall back to the raw ID
  return activeStockLocations.value.find((l) => l.id === locationId)?.name ?? locationId
}
```

4. In `onMounted` (line 247): `stockLocationStore.fetchActive()` → `loadActiveStockLocations()`.

- [ ] **Step 4: Edit StockTransferDetail template**

Line 293 (source location Select in the create form): `:options="stockLocationStore.activeStockLocations"` → `:options="activeStockLocations"` — check for any other occurrences of `stockLocationStore` in the template (lines 280–378) and replace them the same way.

- [ ] **Step 5: Verify and commit**

Run: `cd app/Admin && pnpm exec vue-tsc --build && pnpm run lint`
Expected: no errors

```bash
git add app/Admin/src/features/inventory/views/StockTransfersList.vue app/Admin/src/features/inventory/views/StockTransferDetail.vue
git commit -m "refactor(inventory): rewrite stock transfer views on useActiveStockLocations composable"
```

---

### Task 11: Delete the 7 replaced stores, their specs, and remaining barrels + final gate

**Files:**
- Delete: `app/Admin/src/features/catalog/stores/taxonomyStore.ts`, `app/Admin/src/features/catalog/stores/taxonStore.ts`, `app/Admin/src/features/catalog/stores/taxonDetailStore.ts` + `app/Admin/src/features/catalog/stores/index.ts`
- Delete: `app/Admin/src/features/dashboard/stores/dashboardStore.ts` + `app/Admin/src/features/dashboard/stores/index.ts`
- Delete: `app/Admin/src/features/identity/stores/userStore.ts` + `app/Admin/src/features/identity/stores/index.ts`
- Delete: `app/Admin/src/features/inventory/stores/stockLocationStore.ts` + `app/Admin/src/features/inventory/stores/index.ts`
- Delete: `app/Admin/src/features/location/stores/countryStore.ts` + `app/Admin/src/features/location/stores/index.ts`
- Delete specs: `features/catalog/__tests__/stores/taxonStore.spec.ts`, `features/catalog/__tests__/stores/taxonDetailStore.spec.ts`, `features/dashboard/__tests__/stores/dashboardStore.spec.ts`, `features/identity/__tests__/stores/userStore.spec.ts`, `features/inventory/__tests__/stores/stockLocationStore.spec.ts` (all under `app/Admin/src/`)
- Modify: `app/Admin/src/features/catalog/index.ts`, `app/Admin/src/features/identity/index.ts`, `app/Admin/src/features/inventory/index.ts`, `app/Admin/src/features/location/index.ts`, `app/Admin/src/features/dashboard/index.ts` — remove the `export * from './stores'` line each (their stores barrels are deleted in this task)
- Result: only `features/auth/stores/authStore.ts` + `features/auth/stores/index.ts` + `features/auth/stores/__tests__/authStore.spec.ts` remain.

**Interfaces:**
- Consumes: all 7 stores were fully unwired by Tasks 5–10 (only `authStore` must remain, per spec).

- [ ] **Step 1: Verify no remaining consumers**

Run: `cd app/Admin && grep -rn "useTaxonomyStore\|useTaxonStore\|useTaxonDetailStore\|useDashboardStore\|useUserStore\|useCountryStore\|useStockLocationStore" src --include=*.vue --include=*.ts | grep -v __tests__`
Expected: no output

- [ ] **Step 2: Delete the files**

```bash
cd app/Admin
git rm \
  src/features/catalog/stores/taxonomyStore.ts \
  src/features/catalog/stores/taxonStore.ts \
  src/features/catalog/stores/taxonDetailStore.ts \
  src/features/catalog/stores/index.ts \
  src/features/dashboard/stores/dashboardStore.ts \
  src/features/dashboard/stores/index.ts \
  src/features/identity/stores/userStore.ts \
  src/features/identity/stores/index.ts \
  src/features/inventory/stores/stockLocationStore.ts \
  src/features/inventory/stores/index.ts \
  src/features/location/stores/countryStore.ts \
  src/features/location/stores/index.ts \
  src/features/catalog/__tests__/stores/taxonStore.spec.ts \
  src/features/catalog/__tests__/stores/taxonDetailStore.spec.ts \
  src/features/dashboard/__tests__/stores/dashboardStore.spec.ts \
  src/features/identity/__tests__/stores/userStore.spec.ts \
  src/features/inventory/__tests__/stores/stockLocationStore.spec.ts
```

Then remove the `export * from './stores'` line from `src/features/catalog/index.ts`, `src/features/identity/index.ts`, `src/features/inventory/index.ts`, `src/features/location/index.ts`, and `src/features/dashboard/index.ts` (plan amendment 2026-08-03, approved by human partner — their stores barrels are deleted above).

- [ ] **Step 3: Run the grep gate**

Run: `cd app/Admin && grep -rn "use[A-Za-z]*Store(" src --include=*.vue --include=*.ts`
Expected: only `authStore` occurrences — in `src/features/auth/stores/__tests__/authStore.spec.ts`, `src/app/router/guards.ts`, `src/shared/components/layout/AppMenu.vue`, `src/shared/components/layout/UserMenu.vue`, `src/features/auth/views/LoginPage.vue`, `src/features/profile/views/AddressDetail.vue`, `src/features/profile/views/AddressesList.vue`

- [ ] **Step 4: Full verification**

Run: `cd app/Admin && pnpm exec vue-tsc --build && pnpm run lint && pnpm run test:unit`
Expected: typecheck clean, lint clean, all tests pass (authStore.spec still present; useActiveList + wrapper + useTaxonDetail specs present)

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src
git commit -m "chore(admin): remove replaced Pinia stores, keep authStore as the single store"
```

---

### Task 12: Final review against the spec

**Files:**
- Review only: no file changes expected.

- [ ] **Step 1: Confirm end state matches the spec**

Run:
```bash
cd /home/ngtphat/Projects/ReSys.Shop
ls app/Admin/src/features/*/stores/ | grep -v auth
# Expected: no output (only features/auth/stores exists)
grep -rn "from '../stores" app/Admin/src/features --include=*.vue --include=*.ts
# Expected: no output (views no longer import from stores)
```

- [ ] **Step 2: Confirm the full test suite + build pass once more**

Run: `cd app/Admin && pnpm run test:unit`
Expected: all pass. Also run the repo-wide admin gate if the executor has it scripted (`pnpm run lint`).

- [ ] **Step 3: Report completion**

Summarize: 19 stores removed (12 dead + 7 replaced), 14 spec files deleted, 5 new composables + 5 new specs, 9 views rewritten, `authStore` the only Pinia store left.
