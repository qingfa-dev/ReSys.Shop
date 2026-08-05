# Admin Catalog Views Corrections Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix four catalog admin defects — add product status actions and real server paging to ProductsList, fix the broken ProductDetail classification/option-type tabs, wire OptionTypesList to server paging, and refactor VariantsList into a "list + product select" view aligned with the current API services.

**Architecture:** Frontend-only changes to the Admin Vue 3 SPA. Views use `usePagedQuery` (existing shared composable) and the established TaxonsList Select-in-header pattern. One new `useProductOptions` composable provides lazy, debounced, search-as-you-type product options for the variants selector. Backend endpoints already exist for every operation.

**Tech Stack:** Vue 3 + TypeScript 6, PrimeVue 5 (DataTable, PickList, Select, Tag, ConfirmDialog), Pinia, Vitest, pnpm.

## Global Constraints

- Zero backend changes — all required endpoints already exist.
- No cross-module references, no new dependencies.
- Warnings-as-errors applies to the .NET build only; for the Admin SPA, `pnpm run lint` must pass with 0 errors.
- Views are NOT unit-tested in this codebase (no view specs exist). Verify view changes with `pnpm run lint` + `pnpm run test:unit` + manual checks. Unit-test services and composables.
- Follow existing patterns: `usePagedQuery` state destructuring, `first = (page - 1) * pageSize`, `onPage`/`onRows`/`onSort` handlers as in `VariantsList.vue:87-99`.
- The route for product variants is `api/catalog/variants`; `GetVariantsPagedOrAll` treats `ProductId` as optional.

---

### Task 1: Full-set paging mechanism + service alignment

> **Plan correction (review-driven):** The original Task 1 told services to pass `{}` to `getPaged` assuming that emits no paging params. It does NOT: `getPaged` calls `parseAll`, whose `parsePageValues(undefined, undefined)` returns `{ page: 1, pageSize: 20, isEmpty: false }` (parsers.ts:455-461), so `queryingModelToParams` emits `page=1&pageSize=20` and the backend truncates to 20 rows. The empty-params short-circuit only exists in `queryingParamsToModel` (mappers.ts:42-44), which `getPaged` never calls. This also means the ProductDetail classification/option-type tabs today load only the first 20 taxons — a likely root cause of the "classification tab broken" report. The corrected mechanism: fix shared `getPaged` so an all-empty param object routes through `queryingParamsToModel`, which short-circuits to `emptyQueryingModel` (`isEmpty: true`) and emits NO `page`/`pageSize`. Backend `PageModelExtensions.FromValues` returns `isEmpty: true` only when `page is null && pageSize is null`, so no paging params ⇒ full set.

**Files:**
- Modify: `app/Admin/src/shared/api/paged.ts:32-37`
- Modify: `app/Admin/src/shared/api/__tests__/paged.spec.ts`
- Modify: `app/Admin/src/features/catalog/services/variantApi.ts:61-68`
- Modify: `app/Admin/src/features/catalog/services/variantImageApi.ts:10-15`
- Modify: `app/Admin/src/features/catalog/__tests__/services/variantApi.spec.ts:81-93`
- Modify: `app/Admin/src/features/catalog/__tests__/services/variantImageApi.spec.ts`

**Interfaces:**
- Consumes: nothing from earlier tasks.
- Produces: (a) `getPaged(url, {})` now emits NO query string — the full-set mechanism every later task relies on; (b) `VariantApi.getOptionValues(variantId)` and `VariantImageApi.listImages(variantId)` request the full set. `usePagedQuery` always passes real `pageNumber`/`pageSize`, so it is unaffected.

- [ ] **Step 1: Write the failing test for `getPaged` empty params**

Add to `app/Admin/src/shared/api/__tests__/paged.spec.ts`:

```ts
  it('emits no paging params when the param object is empty', async () => {
    mockGet.mockResolvedValue(okResponse())

    await getPaged<unknown>('/api/variants?productId=x', {})

    expect(mockGet).toHaveBeenCalledWith(
      '/api/variants?productId=x',
      undefined,
    )
  })
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd app/Admin && pnpm exec vitest run src/shared/api/__tests__/paged.spec.ts`
Expected: FAIL — `getPaged` currently calls `mockGet` with `'/api/variants?productId=x&page=1&pageSize=20'`.

- [ ] **Step 3: Fix shared `getPaged`**

In `app/Admin/src/shared/api/paged.ts`, change the import to include `queryingParamsToModel` and route empty params through it:

```ts
import { queryingModelToParams, queryingParamsToModel } from '@/shared/types/querying'
```

Then replace the `parseAll(...)` call inside `getPaged` with `queryingParamsToModel(...)`:

```ts
  const parsed = queryingParamsToModel(
    params,
    options?.allowedFilterFields ?? null,
    options?.allowedSortFields ?? null,
    options?.allowedSearchFields ?? null,
  )
```

`queryingParamsToModel` short-circuits an all-empty param object to `emptyQueryingModel` (`isEmpty: true`, parsers/mappers handle it), and otherwise delegates to `parseAll` with identical behavior — so every existing caller that passes real params is unchanged.

- [ ] **Step 4: Run paged tests to verify the fix**

Run: `cd app/Admin && pnpm exec vitest run src/shared/api/__tests__/paged.spec.ts`
Expected: PASS (new empty-params test + all existing paged tests).

- [ ] **Step 5: Update the failing service test**

Edit `variantApi.spec.ts` — the `VariantApi.getOptionValues` test currently asserts `{ pageNumber: 1, pageSize: 100 }`. Change it to assert the empty param object:

```ts
describe('VariantApi.getOptionValues', () => {
  it('calls getPaged with option-values URL and no paging params', async () => {
    mockGetPaged.mockResolvedValue({
      items: [], page: 1, pageSize: 0, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })
    await VariantApi.getOptionValues('abc-123')
    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/variant-option-values?variantId=abc-123',
      {},
    )
  })
})
```

Also update `variantImageApi.spec.ts` the same way — its `listImages` test currently asserts `{ pageNumber: 1, pageSize: 100 }`; change the expected second argument to `{}`.

- [ ] **Step 6: Run service test to verify it fails**

Run: `cd app/Admin && pnpm exec vitest run src/features/catalog/__tests__/services/variantApi.spec.ts`
Expected: FAIL — assertion for the params argument does not match `{ pageNumber: 1, pageSize: 100 }`.

- [ ] **Step 7: Implement minimal service changes**

In `variantApi.ts`, change `getOptionValues` to send empty params:

```ts
  static getOptionValues(
    variantId: string,
  ): Promise<PagedResult<OptionValueAssignment>> {
    return getPaged<OptionValueAssignment>(
      `${CATALOG}/variant-option-values?variantId=${variantId}`,
      {},
    )
  }
```

In `variantImageApi.ts`, change `listImages` to send empty params:

```ts
  static listImages(variantId: string): Promise<PagedResult<VariantImage>> {
    return getPaged<VariantImage>(`${BASE}?variantId=${variantId}`, {})
  }
```

- [ ] **Step 8: Run tests to verify they pass**

Run: `cd app/Admin && pnpm exec vitest run src/shared/api/__tests__/paged.spec.ts src/features/catalog/__tests__/services/variantApi.spec.ts src/features/catalog/__tests__/services/variantImageApi.spec.ts`
Expected: PASS.

- [ ] **Step 9: Commit**

```bash
git add app/Admin/src/shared/api/paged.ts app/Admin/src/shared/api/__tests__/paged.spec.ts app/Admin/src/features/catalog/services/variantApi.ts app/Admin/src/features/catalog/services/variantImageApi.ts app/Admin/src/features/catalog/__tests__/services/variantApi.spec.ts app/Admin/src/features/catalog/__tests__/services/variantImageApi.spec.ts
git commit -m "fix(catalog): full-set paging via empty params and aligned variant services"
```

---

### Task 2: ProductsList — status action buttons + server paging

**Files:**
- Modify: `app/Admin/src/features/catalog/views/ProductsList.vue` (script lines 1-98, template lines 100-176)

**Interfaces:**
- Consumes: `ProductApi.activateProduct(id)`, `ProductApi.discontinueProduct(id)` (both already exist), `usePagedQuery` paging state, `useConfirm`, `useNotify`.
- Produces: none consumed by later tasks.

- [ ] **Step 1: Write a failing test for a new pure helper**

Views are not unit-tested in this repo; to keep TDD, extract the status-decision logic into a testable helper. Create `app/Admin/src/features/catalog/utils/productStatus.ts`:

```ts
import type { ProductListItem } from '../types/product'

export type ProductStatusAction =
  | { kind: 'activate' }
  | { kind: 'discontinue' }
  | { kind: 'none' }

export function statusAction(status: ProductListItem['status']): ProductStatusAction {
  if (status === 'Active') return { kind: 'discontinue' }
  if (status === 'Draft' || status === 'Archived') return { kind: 'activate' }
  return { kind: 'none' }
}
```

Create the test `app/Admin/src/features/catalog/__tests__/utils/productStatus.spec.ts`:

```ts
import { describe, it, expect } from 'vitest'
import { statusAction } from '../../utils/productStatus'

describe('statusAction', () => {
  it('returns discontinue for Active', () => {
    expect(statusAction('Active')).toEqual({ kind: 'discontinue' })
  })
  it('returns activate for Draft', () => {
    expect(statusAction('Draft')).toEqual({ kind: 'activate' })
  })
  it('returns activate for Archived', () => {
    expect(statusAction('Archived')).toEqual({ kind: 'activate' })
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd app/Admin && pnpm exec vitest run src/features/catalog/__tests__/utils/productStatus.spec.ts`
Expected: FAIL — `../../utils/productStatus` module not found.

- [ ] **Step 3: Implement the helper to make the test pass**

Create `app/Admin/src/features/catalog/utils/productStatus.ts` exactly as above. If `app/Admin/src/features/catalog/utils/` does not exist, create it.

- [ ] **Step 4: Run test to verify it passes**

Run: `cd app/Admin && pnpm exec vitest run src/features/catalog/__tests__/utils/productStatus.spec.ts`
Expected: PASS.

- [ ] **Step 5: Wire status action into ProductsList script**

Edit the `<script setup>` of `ProductsList.vue`:

1. Add imports: `import { statusAction } from '../utils/productStatus'` and `import type { ProductListItem } from '../types/product'` is already present.
2. Extend the `usePagedQuery` destructuring to include paging state:

```ts
const {
  items,
  loading,
  error,
  totalCount,
  page,
  pageSize,
  setPage,
  setPageSize,
  setSearch,
  setSort,
  refresh,
} = usePagedQuery<ProductListItem>('api/catalog/products', {
  allowedFilterFields: PRODUCT_FILTER_FIELDS,
  allowedSortFields: PRODUCT_SORT_FIELDS,
  allowedSearchFields,
  defaultSearchFields: allowedSearchFields,
  defaultSearchMode: 'any',
  defaultSort: ['name'],
  defaultPageSize: 25,
})

const first = computed(() => (page.value - 1) * pageSize.value)
```

3. Add `computed` to the vue import (currently `import { ref } from 'vue'`).
4. Add paging handlers mirroring VariantsList:

```ts
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
```

5. Add imports `import type { DataTablePageEvent, DataTableSortEvent } from 'primevue/datatable'`.
6. Add the status toggle handler:

```ts
function confirmStatusChange(product: ProductListItem) {
  const action = statusAction(product.status)
  if (action.kind === 'none') return
  const isActivate = action.kind === 'activate'

  confirm.require({
    message: isActivate
      ? `Are you sure you want to activate "${product.name}"?`
      : `Are you sure you want to discontinue "${product.name}"?`,
    header: isActivate ? 'Confirm Activate' : 'Confirm Discontinue',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: isActivate ? 'Activate' : 'Discontinue',
    acceptClass: isActivate ? 'p-button-success' : 'p-button-warning',
    accept: async () => {
      const result = isActivate
        ? await ProductApi.activateProduct(product.id)
        : await ProductApi.discontinueProduct(product.id)
      if (result.isSuccess) {
        notify.success(
          isActivate ? 'Product activated' : 'Product discontinued',
          `${product.name} is now ${isActivate ? 'Active' : 'Discontinued'}.`,
        )
        refresh()
      } else {
        notify.error(
          isActivate ? 'Activate failed' : 'Discontinue failed',
          result.errors?.[0]?.message ?? 'Could not update status.',
        )
      }
    },
  })
}
```

- [ ] **Step 6: Update the ProductsList template**

1. Replace `:rows="20"` with `:rows="pageSize"`.
2. Add `:total-records="totalCount"`, `:first="first"`, `@page="onPage"`, `@update:rows="onRows"`, `@sort="onSort"` to the DataTable.
3. Add the server-error Message block above the DataTable (`v-else-if="error"`) — import `Message` from `primevue/message` and replicate the block from `VariantsList.vue:153-160`.
4. In the actions column, add the status button before the Edit button:

```html
<Button
  v-if="statusAction(data.status).kind === 'activate'"
  icon="pi pi-check-circle"
  severity="success"
  text
  rounded
  aria-label="Activate"
  @click="confirmStatusChange(data)"
/>
<Button
  v-else-if="statusAction(data.status).kind === 'discontinue'"
  icon="pi pi-pause-circle"
  severity="warning"
  text
  rounded
  aria-label="Discontinue"
  @click="confirmStatusChange(data)"
/>
```

5. Widen the actions column to fit the extra button (e.g. `width: 12rem`).

- [ ] **Step 7: Verify lint + typecheck + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run test:unit`
Expected: lint 0 errors, all tests pass (existing products spec still passes — `productApi.spec.ts` already covers activate/discontinue URLs).

- [ ] **Step 8: Commit**

```bash
git add app/Admin/src/features/catalog/views/ProductsList.vue app/Admin/src/features/catalog/utils/productStatus.ts app/Admin/src/features/catalog/__tests__/utils/productStatus.spec.ts
git commit -m "feat(catalog): add product status actions and server paging to list"
```

---

### Task 3: OptionTypesList — server paging

**Files:**
- Modify: `app/Admin/src/features/catalog/views/OptionTypesList.vue`

**Interfaces:**
- Consumes: `usePagedQuery` paging state.
- Produces: none consumed by later tasks.

- [ ] **Step 1: Write a failing test for the page-count helper**

Create `app/Admin/src/features/catalog/utils/tablePaging.ts`:

```ts
export function tableFirst(page: number, pageSize: number): number {
  return (page - 1) * pageSize
}
```

Create test `app/Admin/src/features/catalog/__tests__/utils/tablePaging.spec.ts`:

```ts
import { describe, it, expect } from 'vitest'
import { tableFirst } from '../../utils/tablePaging'

describe('tableFirst', () => {
  it('computes first row index', () => {
    expect(tableFirst(1, 25)).toBe(0)
    expect(tableFirst(2, 25)).toBe(25)
    expect(tableFirst(3, 10)).toBe(20)
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd app/Admin && pnpm exec vitest run src/features/catalog/__tests__/utils/tablePaging.spec.ts`
Expected: FAIL — module not found.

- [ ] **Step 3: Implement the helper**

Create `app/Admin/src/features/catalog/utils/tablePaging.ts` exactly as above.

- [ ] **Step 4: Run test to verify it passes**

Run: `cd app/Admin && pnpm exec vitest run src/features/catalog/__tests__/utils/tablePaging.spec.ts`
Expected: PASS.

- [ ] **Step 5: Wire server paging into OptionTypesList script**

Edit `<script setup>` of `OptionTypesList.vue`:

1. Change vue import to `import { ref, computed } from 'vue'`.
2. Import the helpers and types:

```ts
import type { DataTablePageEvent, DataTableSortEvent } from 'primevue/datatable'
import { tableFirst } from '../utils/tablePaging'
```

3. Extend `usePagedQuery` destructuring:

```ts
const {
  items,
  loading,
  error,
  totalCount,
  page,
  pageSize,
  setPage,
  setPageSize,
  setSearch,
  setSort,
  refresh,
} = usePagedQuery<OptionTypeListItem>('api/catalog/option-types', {
  allowedFilterFields: OPTION_TYPE_FILTER_FIELDS,
  allowedSortFields: OPTION_TYPE_SORT_FIELDS,
  allowedSearchFields,
  defaultSearchFields: allowedSearchFields,
  defaultSearchMode: 'any',
  defaultSort: ['name'],
  defaultPageSize: 25,
})

const first = computed(() => tableFirst(page.value, pageSize.value))
```

4. Add handlers:

```ts
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
```

- [ ] **Step 6: Update the OptionTypesList template**

1. Add `:total-records="totalCount"`, `:first="first"`, `@page="onPage"`, `@update:rows="onRows"`, `@sort="onSort"` to the DataTable.
2. Add the `v-else-if="error"` Message block (import `Message`) as in VariantsList.

- [ ] **Step 7: Verify lint + typecheck + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run test:unit`
Expected: lint 0 errors, all tests pass.

- [ ] **Step 8: Commit**

```bash
git add app/Admin/src/features/catalog/views/OptionTypesList.vue app/Admin/src/features/catalog/utils/tablePaging.ts app/Admin/src/features/catalog/__tests__/utils/tablePaging.spec.ts
git commit -m "feat(catalog): wire option types list to server-side paging"
```

---

### Task 4: ProductDetail — fix classification and option-type tabs

**Files:**
- Modify: `app/Admin/src/features/catalog/views/ProductDetail.vue` (script lines 79-158, 409-453)

**Interfaces:**
- Consumes: `ProductClassificationApi`, `ProductOptionTypeApi` (already imported).
- Produces: none consumed by later tasks.

- [ ] **Step 1: Write a failing test for the reset helper**

Create `app/Admin/src/features/catalog/utils/assignmentState.ts`:

```ts
export interface AssignmentLists {
  unassigned: unknown[]
  assigned: unknown[]
}

export function makeEmptyAssignments(): { unassigned: []; assigned: [] } {
  return { unassigned: [], assigned: [] }
}
```

Create test `app/Admin/src/features/catalog/__tests__/utils/assignmentState.spec.ts`:

```ts
import { describe, it, expect } from 'vitest'
import { makeEmptyAssignments } from '../../utils/assignmentState'

describe('makeEmptyAssignments', () => {
  it('returns empty unassigned and assigned lists', () => {
    const { unassigned, assigned } = makeEmptyAssignments()
    expect(unassigned).toEqual([])
    expect(assigned).toEqual([])
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd app/Admin && pnpm exec vitest run src/features/catalog/__tests__/utils/assignmentState.spec.ts`
Expected: FAIL — module not found.

- [ ] **Step 3: Implement the helper**

Create `app/Admin/src/features/catalog/utils/assignmentState.ts` exactly as above.

- [ ] **Step 4: Run test to verify it passes**

Run: `cd app/Admin && pnpm exec vitest run src/features/catalog/__tests__/utils/assignmentState.spec.ts`
Expected: PASS.

- [ ] **Step 5: Fix reset-on-product-switch in ProductDetail script**

In `<script setup>`:

1. Add reload-flag refs next to the existing assignment refs:

```ts
const optionTypesLoaded = ref(false)
const classificationsLoaded = ref(false)
```

2. Add a reset function (uses `makeEmptyAssignments` from the new util):

```ts
import { makeEmptyAssignments } from '../utils/assignmentState'

function resetAssignments() {
  const ot = makeEmptyAssignments()
  const cl = makeEmptyAssignments()
  unassignedOptionTypes.value = ot.unassigned
  assignedOptionTypes.value = ot.assigned
  unassignedClassifications.value = cl.unassigned
  assignedClassifications.value = cl.assigned
  optionTypesLoaded.value = false
  classificationsLoaded.value = false
}
```

3. In `initEditMode`, after `formLoaded.value = true`, call `resetAssignments()` and reset the active tab:

```ts
    formLoaded.value = true
    resetAssignments()
    activeTab.value = '0'
```

4. Replace the `watch(activeTab)` guards (currently array-length based, `ProductDetail.vue:131-138`) with flag-based guards:

```ts
watch(activeTab, (tab) => {
  if (isEdit.value && tab === '4' && !optionTypesLoaded.value) {
    loadOptionTypes()
  }
  if (isEdit.value && tab === '5' && !classificationsLoaded.value) {
    loadClassifications()
  }
})
```

5. In `loadOptionTypes` and `loadClassifications`, set the flag on success:

```ts
async function loadOptionTypes() {
  optionTypesLoading.value = true
  const result = await ProductOptionTypeApi.getOptionTypes(route.params.id as string)
  if (result.isSuccess && result.items) {
    unassignedOptionTypes.value = result.items.filter(i => !i.isAssigned)
    assignedOptionTypes.value = result.items.filter(i => i.isAssigned)
    optionTypesLoaded.value = true
  }
  optionTypesLoading.value = false
}

async function loadClassifications() {
  classificationsLoading.value = true
  const result = await ProductClassificationApi.getClassifications(route.params.id as string)
  if (result.isSuccess && result.items) {
    unassignedClassifications.value = result.items.filter(i => !i.isAssigned)
    assignedClassifications.value = result.items.filter(i => i.isAssigned)
    classificationsLoaded.value = true
  }
  classificationsLoading.value = false
}
```

6. In `saveOptionTypes`/`saveClassifications`, reset the flag so a post-save reload always happens:

```ts
  if (result.isSuccess) {
    notify.success('Option types saved')
    optionTypesLoaded.value = false
    await loadOptionTypes()
  }
```

and

```ts
  if (result.isSuccess) {
    notify.success('Classifications saved')
    classificationsLoaded.value = false
    await loadClassifications()
  }
```

- [ ] **Step 6: Fix PickList two-way source binding in template**

> **Plan correction (review-driven):** The original Step 6 told the implementer to change `:source="unassignedOptionTypes"` to `v-model="unassignedOptionTypes"` while keeping `v-model:target`. This is infeasible with the installed PrimeVue 5.0.0 PickList: it has NO `source`/`target` props, consumes a single `modelValue: any[][]` (`modelValue[0]` = source, `modelValue[1]` = target), only emits `update:modelValue`, and sets `inheritAttrs: false` — so the pre-existing `:source`/`v-model:target` bindings were inert fallthrough attrs and the component always rendered the default `[[], []]` (the real root cause of the blank-list/snap-back symptom). The verified approach below implements the two-way-binding intent with writable computeds typed `[][]`; use it verbatim.

1. Add writable computed models next to the assignment refs in the script (they map the single PickList `modelValue` array to the two refs that `saveOptionTypes`/`saveClassifications` and the empty-state guards consume):

```ts
const optionTypesModel = computed<OptionTypeAssignment[][]>({
  get: () => [unassignedOptionTypes.value, assignedOptionTypes.value],
  set: (value) => {
    unassignedOptionTypes.value = value[0] ?? []
    assignedOptionTypes.value = value[1] ?? []
  },
})

const classificationsModel = computed<ClassificationAssignment[][]>({
  get: () => [unassignedClassifications.value, assignedClassifications.value],
  set: (value) => {
    unassignedClassifications.value = value[0] ?? []
    assignedClassifications.value = value[1] ?? []
  },
})
```

2. Option Types tab (`ProductDetail.vue:409-426`): replace the `v-model:target="assignedOptionTypes"` + `:source="unassignedOptionTypes"` props with `v-model="optionTypesModel"`.
3. Classifications tab (`ProductDetail.vue:433-449`): replace the `v-model:target="assignedClassifications"` + `:source="unassignedClassifications"` props with `v-model="classificationsModel"`.
4. Keep the `source-header`/`target-header`/`list-style`/filter-placeholder props unchanged.

- [ ] **Step 7: Add empty states to the tabs**

In each PickList tab, wrap in a conditional that shows a message when a loaded catalog is empty:

```html
<TabPanel v-if="isEdit" value="5">
  <div v-if="classificationsLoading" class="text-center py-8 text-muted-color">Loading classifications...</div>
  <div v-else-if="unassignedClassifications.length === 0 && assignedClassifications.length === 0" class="text-center py-8 text-muted-color">
    No classifications available.
  </div>
  <template v-else>
    <PickList ... />
    <div class="mt-3">
      <Button label="Save Classifications" severity="primary" @click="saveClassifications" />
    </div>
  </template>
</TabPanel>
```

Apply the same pattern to the Option Types tab (value="4").

- [ ] **Step 8: Verify lint + typecheck + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run test:unit`
Expected: lint 0 errors, all tests pass.

- [ ] **Step 9: Commit**

```bash
git add app/Admin/src/features/catalog/views/ProductDetail.vue app/Admin/src/features/catalog/utils/assignmentState.ts app/Admin/src/features/catalog/__tests__/utils/assignmentState.spec.ts
git commit -m "fix(catalog): repair product classification and option type tabs"
```

---

### Task 5: New `useProductOptions` composable

**Files:**
- Create: `app/Admin/src/features/catalog/composables/useProductOptions.ts`
- Modify: `app/Admin/src/features/catalog/composables/index.ts`
- Create: `app/Admin/src/features/catalog/__tests__/composables/useProductOptions.spec.ts`

**Interfaces:**
- Consumes: `ProductApi.getProducts(query)`, `debounce` from `@/shared/utils/debounce`.
- Produces: `useProductOptions()` returning:
  - `options: Ref<ProductListItem[]>`
  - `loading: Ref<boolean>`
  - `search: Ref<string>`
  - `selectedId: Ref<string | null>`
  - `searchProducts(term: string): void`
  - `loadInitial(): Promise<void>`

- [ ] **Step 1: Write the failing composable test**

Create `app/Admin/src/features/catalog/__tests__/composables/useProductOptions.spec.ts`:

```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockGetProducts } = vi.hoisted(() => ({
  mockGetProducts: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('../../services/productApi', () => ({
  ProductApi: { getProducts: mockGetProducts },
}))

import { useProductOptions } from '../../composables/useProductOptions'

function pagedResult(items: unknown[] = []) {
  return {
    isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    items, page: 1, pageSize: 25, totalCount: items.length, totalPages: 0,
  }
}

beforeEach(() => {
  vi.clearAllMocks()
  vi.useRealTimers()
})

describe('useProductOptions', () => {
  it('loadInitial fetches first page without search', async () => {
    mockGetProducts.mockResolvedValue(pagedResult([{ id: 'p1', name: 'Shirt' }]))
    const { options, loadInitial } = useProductOptions()
    await loadInitial()
    expect(mockGetProducts).toHaveBeenCalledWith(
      expect.objectContaining({ search: '', page: 1, pageSize: 25, sortBy: 'name' }),
    )
    expect(options.value).toHaveLength(1)
  })

  it('searchProducts is debounced and fetches with search term', async () => {
    vi.useFakeTimers()
    mockGetProducts.mockResolvedValue(pagedResult([{ id: 'p1', name: 'Shirt' }]))
    const { options, searchProducts } = useProductOptions()
    searchProducts('shirt')
    vi.advanceTimersByTime(300)
    await vi.advanceTimersByTimeAsync(0)
    expect(mockGetProducts).toHaveBeenCalledWith(
      expect.objectContaining({ search: 'shirt', page: 1, pageSize: 25 }),
    )
    expect(options.value).toHaveLength(1)
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd app/Admin && pnpm exec vitest run src/features/catalog/__tests__/composables/useProductOptions.spec.ts`
Expected: FAIL — `useProductOptions` module not found.

- [ ] **Step 3: Implement the composable**

Create `app/Admin/src/features/catalog/composables/useProductOptions.ts`:

```ts
import { ref } from 'vue'
import { debounce } from '@/shared/utils/debounce'
import { ProductApi } from '../services/productApi'
import type { ProductListItem } from '../types/product'

const PAGE_SIZE = 25

export function useProductOptions() {
  const options = ref<ProductListItem[]>([])
  const loading = ref(false)
  const search = ref('')
  const selectedId = ref<string | null>(null)
  const loadedFor = ref<string | null>(null)

  async function fetchOptions(term: string): Promise<void> {
    loading.value = true
    const result = await ProductApi.getProducts({
      search: term,
      page: 1,
      pageSize: PAGE_SIZE,
      sortBy: 'name',
    })
    if (result.isSuccess) {
      options.value = result.items
      loadedFor.value = term
    }
    loading.value = false
  }

  const searchProducts = debounce(async (term: string) => {
    search.value = term
    if (loadedFor.value === term) return
    await fetchOptions(term)
  }, 300)

  async function loadInitial(): Promise<void> {
    await fetchOptions('')
  }

  return { options, loading, search, selectedId, searchProducts, loadInitial }
}
```

- [ ] **Step 4: Export from the composables index**

Append to `app/Admin/src/features/catalog/composables/index.ts`:

```ts
export { useProductOptions } from './useProductOptions'
```

- [ ] **Step 5: Run test to verify it passes**

Run: `cd app/Admin && pnpm exec vitest run src/features/catalog/__tests__/composables/useProductOptions.spec.ts`
Expected: PASS.

- [ ] **Step 6: Verify lint + typecheck**

Run: `cd app/Admin && pnpm run lint`
Expected: lint 0 errors.

- [ ] **Step 7: Commit**

```bash
git add app/Admin/src/features/catalog/composables/useProductOptions.ts app/Admin/src/features/catalog/composables/index.ts app/Admin/src/features/catalog/__tests__/composables/useProductOptions.spec.ts
git commit -m "feat(catalog): add lazy searchable product options composable"
```

---

### Task 6: VariantsList — list + product select refactor

**Files:**
- Modify: `app/Admin/src/features/catalog/views/VariantsList.vue` (whole file)

**Interfaces:**
- Consumes: `useProductOptions` (Task 5), existing `usePagedQuery` state, `ProductApi` via the composable.
- Produces: none consumed by later tasks.

- [ ] **Step 1: Write a failing test for the URL builder**

Create `app/Admin/src/features/catalog/utils/variantListUrl.ts`:

```ts
export function variantsListUrl(productId: string | null | undefined): string {
  return productId
    ? `api/catalog/variants?productId=${productId}`
    : 'api/catalog/variants'
}
```

Create test `app/Admin/src/features/catalog/__tests__/utils/variantListUrl.spec.ts`:

```ts
import { describe, it, expect } from 'vitest'
import { variantsListUrl } from '../../utils/variantListUrl'

describe('variantsListUrl', () => {
  it('includes productId when present', () => {
    expect(variantsListUrl('abc')).toBe('api/catalog/variants?productId=abc')
  })
  it('omits productId when absent', () => {
    expect(variantsListUrl(null)).toBe('api/catalog/variants')
    expect(variantsListUrl(undefined)).toBe('api/catalog/variants')
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd app/Admin && pnpm exec vitest run src/features/catalog/__tests__/utils/variantListUrl.spec.ts`
Expected: FAIL — module not found.

- [ ] **Step 3: Implement the helper**

Create `app/Admin/src/features/catalog/utils/variantListUrl.ts` exactly as above.

- [ ] **Step 4: Run test to verify it passes**

Run: `cd app/Admin && pnpm exec vitest run src/features/catalog/__tests__/utils/variantListUrl.spec.ts`
Expected: PASS.

- [ ] **Step 5: Rework VariantsList script**

Edit `<script setup>` of `VariantsList.vue`:

1. Import `Select` from `primevue/select` and the composable + helper:

```ts
import { useProductOptions } from '../composables/useProductOptions'
import { variantsListUrl } from '../utils/variantListUrl'
```

2. Replace the `productId` computed with a two-way-backed ref synced to the route:

```ts
const productId = ref<string | null>(null)

function syncFromRoute() {
  const qp = route.query.productId as string | undefined
  productId.value = qp ?? null
  selectedProductId.value = qp ?? null
}

function onProductChange(id: string | null) {
  productId.value = id ?? null
  selectedProductId.value = id ?? null
  router.replace({
    query: { ...route.query, productId: id ?? undefined },
  })
  setSearch('')
  refresh()
}
```

3. Create the product options state from the composable:

```ts
const {
  options: productOptions,
  loading: productOptionsLoading,
  selectedId: selectedProductId,
  loadInitial,
  searchProducts,
} = useProductOptions()
```

4. Change the `usePagedQuery` URL to the helper and `immediate: false` stays:

```ts
const {
  items,
  loading,
  error,
  totalCount,
  page,
  pageSize,
  setPage,
  setPageSize,
  setSearch,
  setSort,
  refresh,
} = usePagedQuery<VariantListItem>(
  () => variantsListUrl(productId.value),
  {
    allowedFilterFields: VARIANT_FILTER_FIELDS,
    allowedSortFields: VARIANT_SORT_FIELDS,
    allowedSearchFields: VARIANT_SEARCH_FIELDS,
    defaultSearchFields: VARIANT_SEARCH_FIELDS,
    defaultSearchMode: 'any',
    defaultSort: ['position'],
    defaultPageSize: 25,
    immediate: false,
  },
)
```

5. Replace the `watch(productId, ...)` block with:

```ts
watch(productId, (id) => {
  setSearch('')
  setPage(1)
  refresh()
})

onMounted(() => {
  syncFromRoute()
  loadInitial()
  refresh()
})

watch(() => route.query.productId, () => {
  syncFromRoute()
  refresh()
})
```

Note: remove `setPage` from the paging destructuring duplication — `setPage` is already destructured; the watch calls `setPage(1)` which triggers a fetch, then `refresh()` is redundant — drop the redundant `refresh()`:

```ts
watch(productId, (id) => {
  setSearch('')
  setPage(1)
})
```

6. Update `navigateToNew` to require a product and disable otherwise:

```ts
function navigateToNew() {
  if (!productId.value) return
  router.push(`/catalog/variants/new?productId=${productId.value}`)
}
```

Add a computed for the disabled state:

```ts
const newDisabled = computed(() => !productId.value)
```

(Add `computed` to the vue import; the file already imports `ref, computed, watch`.)

- [ ] **Step 6: Update VariantsList template**

1. Remove the dead "Select a product to view its variants" block (`VariantsList.vue:145-151`) and its `v-if`/`v-else` gating on the DataTable — the DataTable should always render (loading/empty handled by `loading` + `#empty`).
2. Add the product Select in the header next to the search box:

```html
<Select
  :model-value="selectedProductId"
  :options="productOptions"
  option-label="name"
  option-value="id"
  placeholder="All products"
  show-clear
  filter
  :loading="productOptionsLoading"
  class="w-72"
  @update:model-value="onProductChange"
  @filter="(e: { value: string }) => searchProducts(e.value)"
/>
```

3. Update the "New Variant" button to disable without a product:

```html
<Button
  label="New Variant"
  icon="pi pi-plus"
  severity="primary"
  :disabled="newDisabled"
  @click="navigateToNew"
/>
```

Add a hint next to it when disabled: `<span v-if="newDisabled" class="text-sm text-muted-color">Select a product first</span>`.

4. Update the empty-state message:

```html
<template #empty>
  <div class="text-center py-8 text-muted-color">
    {{ productId ? 'No variants found for this product.' : 'No variants found.' }}
  </div>
</template>
```

5. Keep the "Back to Product" button only when a product is selected:

```html
<Button
  v-if="productId"
  label="Back to Product"
  icon="pi pi-arrow-left"
  severity="secondary"
  outlined
  @click="navigateToProduct"
/>
```

- [ ] **Step 7: Verify lint + typecheck + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run test:unit`
Expected: lint 0 errors, all tests pass.

- [ ] **Step 8: Commit**

```bash
git add app/Admin/src/features/catalog/views/VariantsList.vue app/Admin/src/features/catalog/utils/variantListUrl.ts app/Admin/src/features/catalog/__tests__/utils/variantListUrl.spec.ts
git commit -m "feat(catalog): refactor variants list with product selector"
```

---

## Final Verification

Run the full admin SPA verification:

```bash
cd app/Admin && pnpm run lint && pnpm run test:unit
```

Backend untouched — no `.NET` build needed, but optionally confirm nothing else regressed:

```bash
dotnet build service/Api/src/Module
```

## Self-Review Notes

- **Spec coverage:** ProductsList status button (Task 2), ProductsList paging (Task 2), ProductDetail classification tab (Task 4), ProductDetail option types tab (Task 4), OptionTypesList paging (Task 3), VariantsList product select + show-all (Tasks 5-6), service alignment (Task 1), VariantDetail create flow (verified in Task 6 Step 5 — route still carries `productId`).
- **No placeholders:** every step contains concrete code.
- **Type consistency:** `statusAction` returns `{ kind: 'activate' | 'discontinue' | 'none' }`; `tableFirst(page, pageSize)` used consistently; `variantsListUrl(productId)` signature matches the composable's `productId: Ref<string | null>`; `useProductOptions` return shape matches Task 6 consumption (`options/loading/selectedId/loadInitial/searchProducts`).
- **Plan corrections applied during execution (review-driven):** Task 1's "pass `{}`" mechanism was defective — `getPaged` normalizes `{}` to `page=1&pageSize=20` — fixed by routing empty params through `queryingParamsToModel`. Task 4's PickList Step 6 was infeasible with PrimeVue 5 (no `source`/`target` props; single `modelValue: any[][]`; inert fallthrough attrs) — fixed with writable `[][]` computeds. Both updated in-place; the corresponding spec (`docs/superpowers/specs/2026-08-02-admin-catalog-views-corrections-design.md`) should be reconciled before the next plan references it.
