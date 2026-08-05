# List View Refactor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove CrudToolbar.vue and FilterableDataTable.vue wrapper components, refactor CountriesList.vue and StatesList.vue to use PrimeVue DataTable, Toolbar, Card, and Column directly.

**Architecture:** Inline PrimeVue DataTable + Column into each list view with explicit column tags. Extract a small `useDataTableExport` composable for the CSV export `dt` ref. Search input moves from CrudToolbar right side to DataTable header left side. Country filter in StatesList moves to DataTable header right side.

**Tech Stack:** Vue 3 + TypeScript + PrimeVue v5, Vitest

## Global Constraints

- Must use `<script setup lang="ts">` and Vue 3 Composition API
- PrimeVue v5 Card requires `<template #content>` wrapper (named slot only)
- Must preserve all existing lint (`pnpm run lint`), build (`pnpm run build-only`), and test (`pnpm run test:unit -- run`) commands — all must pass with zero errors
- `usePagedQuery` composable interface must remain unchanged
- `ColumnDef` interface in both list views must be removed (no longer needed with explicit Column tags)
- No new imports from deleted wrapper components or their barrel

## Spec Reference

`docs/superpowers/specs/2026-07-28-list-view-refactor-design.md`

---

### Task 1: Create useDataTableExport composable

**Files:**
- Create: `app/Admin/src/shared/composables/useDataTableExport.ts`
- Modify: `app/Admin/src/shared/composables/index.ts`

**Interfaces:**
- Consumes: nothing
- Produces: `useDataTableExport()` returning `{ dt: Ref<DataTable>, exportCSV: () => void }`

- [ ] **Step 1: Write the composable**

Create `app/Admin/src/shared/composables/useDataTableExport.ts`:

```typescript
import { ref } from 'vue'
import type DataTable from 'primevue/datatable'

export function useDataTableExport() {
  const dt = ref<InstanceType<typeof DataTable>>()

  function exportCSV() {
    dt.value?.exportCSV()
  }

  return { dt, exportCSV }
}
```

- [ ] **Step 2: Add barrel re-export**

In `app/Admin/src/shared/composables/index.ts`, add after the last export line:

```typescript
export { useDataTableExport } from './useDataTableExport'
```

- [ ] **Step 3: Verify build**

```bash
pnpm run build-only
```

Expected: builds successfully.

- [ ] **Step 4: Run tests**

```bash
pnpm run test:unit -- run
```

Expected: 413/413 tests pass.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/shared/composables/useDataTableExport.ts app/Admin/src/shared/composables/index.ts
git commit -m "feat(admin): add useDataTableExport composable for CSV export"
```

---

### Task 2: Refactor CountriesList with inline PrimeVue components

**Files:**
- Modify: `app/Admin/src/features/location/views/CountriesList.vue`

**Interfaces:**
- Consumes: `useDataTableExport` from `@/shared/composables/useDataTableExport`, PrimeVue `DataTable` + `Column` + `Card` + `Toolbar` + `Tag`, `@panel/PageShell`
- Produces: Same public behavior — country list with search, pagination, new/delete/edit/export actions

- [ ] **Step 1: Rewrite CountriesList.vue**

Replace entire file contents with:

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Toolbar from 'primevue/toolbar'
import Tag from 'primevue/tag'
import Plus from '@primeicons/vue/plus'
import Trash from '@primeicons/vue/trash'
import Upload from '@primeicons/vue/upload'
import { PageShell } from '@panel'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { CountryApi } from '../services/countryApi'
import type { CountryListItem } from '../types/country'
import { COUNTRY_FILTER_FIELDS, COUNTRY_SORT_FIELDS } from '../types/country'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()

const { dt, exportCSV } = useDataTableExport()
const selectedItems = ref<CountryListItem[]>([])
const searchTerm = ref('')

const {
  items,
  loading,
  totalCount,
  page,
  pageSize,
  setSearch,
  refresh,
} = usePagedQuery<CountryListItem>('api/locations/countries', {
  allowedFilterFields: COUNTRY_FILTER_FIELDS,
  allowedSortFields: COUNTRY_SORT_FIELDS,
  defaultSort: ['name'],
  defaultPageSize: 20,
})

function navigateToNew() {
  router.push('/location/countries/new')
}

function navigateToEdit(id: string) {
  router.push(`/location/countries/${id}`)
}

function onSearch(value: string) {
  searchTerm.value = value
  setSearch(value)
}

function clearSearch() {
  searchTerm.value = ''
  setSearch('')
}

function confirmDelete() {
  if (selectedItems.value.length === 0) return

  confirm.require({
    message: `Are you sure you want to delete ${selectedItems.value.length > 1 ? 'these countries' : 'this country'}?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const target = selectedItems.value[0]!
      const result = await CountryApi.deleteCountry(target.id)
      if (result.isSuccess) {
        notify.success('Country deleted', `${target.name} has been removed.`)
      } else {
        notify.error('Delete failed', result.errors?.[0]?.message ?? 'Could not delete country.')
      }
      selectedItems.value = []
      refresh()
    },
  })
}
</script>

<template>
  <PageShell title="Countries" description="Manage supported countries">
    <Card>
      <template #content>
        <Toolbar>
          <template #start>
            <Button label="New Country" severity="secondary" class="mr-2" @click="navigateToNew">
              <Plus />
            </Button>
            <Button label="Delete" severity="secondary" :disabled="selectedItems.length === 0" @click="confirmDelete">
              <Trash />
            </Button>
          </template>
          <template #end>
            <Button label="Export" severity="secondary" @click="exportCSV">
              <Upload />
            </Button>
          </template>
        </Toolbar>
      </template>
    </Card>

    <DataTable
      ref="dt"
      :value="items"
      :loading="loading"
      :paginator="true"
      :rows="pageSize"
      filter-display="menu"
      data-key="id"
      :global-filter-fields="['name', 'isoCode', 'callingCode']"
      paginator-template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
      :rows-per-page-options="[5, 10, 25]"
      current-page-report-template="Showing {first} to {last} of {totalRecords}"
    >
      <template #header>
        <div class="flex justify-between items-center">
          <IconField>
            <InputIcon><i class="pi pi-search" /></InputIcon>
            <InputText
              :model-value="searchTerm"
              placeholder="Search countries..."
              @update:model-value="onSearch($event ?? '')"
            />
          </IconField>
          <Button label="Clear" outlined @click="clearSearch" />
        </div>
      </template>
      <Column field="name" header="Name" :sortable="true" :filter="true" filter-field="name" />
      <Column field="isoCode" header="ISO Code" :sortable="true" :filter="true" filter-field="isoCode" />
      <Column field="callingCode" header="Calling Code" :sortable="true" />
      <Column field="statesRequired" header="States Required" :sortable="true" body-style="text-align: center">
        <template #body="{ data }">
          <Tag :value="data.statesRequired ? 'Yes' : 'No'" :severity="data.statesRequired ? 'info' : 'secondary'" />
        </template>
      </Column>
      <Column field="isActive" header="Active" :sortable="true" :filter="true" filter-field="isActive" body-style="text-align: center">
        <template #body="{ data }">
          <Tag :value="data.isActive ? 'Active' : 'Inactive'" :severity="data.isActive ? 'success' : 'danger'" />
        </template>
      </Column>
      <Column header="" body-style="text-align: right; width: 6rem">
        <template #body="{ data }">
          <div class="flex justify-end gap-2">
            <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
            <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [data]; confirmDelete()" />
          </div>
        </template>
      </Column>
      <template #empty>
        <div class="text-center py-8 text-muted-color">No countries found.</div>
      </template>
    </DataTable>
  </PageShell>
</template>
```

- [ ] **Step 2: Verify build**

```bash
cd app/Admin && pnpm run build-only
```

Expected: builds successfully with no warnings.

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/location/views/CountriesList.vue
git commit -m "refactor(admin): inline PrimeVue DataTable in CountriesList"
```

---

### Task 3: Refactor StatesList with inline PrimeVue components

**Files:**
- Modify: `app/Admin/src/features/location/views/StatesList.vue`

**Interfaces:**
- Consumes: `useDataTableExport`, PrimeVue `DataTable` + `Column` + `Card` + `Toolbar` + `Select` + `Tag`, `@panel/PageShell`, `useCountryStore`
- Produces: Same public behavior — state list with search, country filter dropdown in DataTable header, pagination, new/delete/edit/export actions

- [ ] **Step 1: Rewrite StatesList.vue**

Replace entire file contents with:

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Toolbar from 'primevue/toolbar'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import Plus from '@primeicons/vue/plus'
import Trash from '@primeicons/vue/trash'
import Upload from '@primeicons/vue/upload'
import { PageShell } from '@panel'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { StateApi } from '../services/stateApi'
import { useCountryStore } from '../stores/countryStore'
import type { StateListItem } from '../types/state'
import { STATE_FILTER_FIELDS, STATE_SORT_FIELDS } from '../types/state'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const countryStore = useCountryStore()

const { dt, exportCSV } = useDataTableExport()
const selectedCountryId = ref<string | null>(null)
const selectedItems = ref<StateListItem[]>([])
const searchTerm = ref('')

const { items, loading, totalCount, page, pageSize, setSearch, setFilter, refresh } =
  usePagedQuery<StateListItem>('api/locations/states', {
    allowedFilterFields: STATE_FILTER_FIELDS,
    allowedSortFields: STATE_SORT_FIELDS,
    defaultSort: ['name'],
    defaultPageSize: 20,
  })

onMounted(() => {
  countryStore.fetchActive()
})

function navigateToNew() {
  router.push('/location/states/new')
}

function navigateToEdit(id: string) {
  router.push(`/location/states/${id}`)
}

function onSearch(value: string) {
  searchTerm.value = value
  setSearch(value)
}

function clearSearch() {
  searchTerm.value = ''
  setSearch('')
}

function onCountryFilterChange(countryId: string | null) {
  selectedCountryId.value = countryId
  if (countryId) {
    setFilter(`countryId=${countryId}`)
  } else {
    setFilter('')
  }
}

function confirmDelete() {
  if (selectedItems.value.length === 0) return

  confirm.require({
    message: `Are you sure you want to delete ${selectedItems.value.length > 1 ? 'these states' : 'this state'}?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const target = selectedItems.value[0]!
      const result = await StateApi.deleteState(target.id)
      if (result.isSuccess) {
        notify.success('State deleted', `${target.name} has been removed.`)
      } else {
        notify.error('Delete failed', result.errors?.[0]?.message ?? 'Could not delete state.')
      }
      selectedItems.value = []
      refresh()
    },
  })
}
</script>

<template>
  <PageShell title="States" description="Manage states and provinces for countries">
    <Card>
      <template #content>
        <Toolbar>
          <template #start>
            <Button label="New State" severity="secondary" class="mr-2" @click="navigateToNew">
              <Plus />
            </Button>
            <Button label="Delete" severity="secondary" :disabled="selectedItems.length === 0" @click="confirmDelete">
              <Trash />
            </Button>
          </template>
          <template #end>
            <Button label="Export" severity="secondary" @click="exportCSV">
              <Upload />
            </Button>
          </template>
        </Toolbar>
      </template>
    </Card>

    <DataTable
      ref="dt"
      :value="items"
      :loading="loading"
      :paginator="true"
      :rows="pageSize"
      filter-display="menu"
      data-key="id"
      :global-filter-fields="['name', 'abbreviation', 'countryName']"
      paginator-template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
      :rows-per-page-options="[5, 10, 25]"
      current-page-report-template="Showing {first} to {last} of {totalRecords}"
    >
      <template #header>
        <div class="flex justify-between items-center">
          <IconField>
            <InputIcon><i class="pi pi-search" /></InputIcon>
            <InputText
              :model-value="searchTerm"
              placeholder="Search states..."
              @update:model-value="onSearch($event ?? '')"
            />
          </IconField>
          <div class="flex items-center gap-2">
            <label class="text-sm text-muted-color whitespace-nowrap">Country:</label>
            <Select
              v-model="selectedCountryId"
              :options="countryStore.activeCountries"
              option-label="name"
              option-value="id"
              placeholder="All Countries"
              show-clear
              class="w-56"
              @change="onCountryFilterChange($event.value)"
            />
            <Button label="Clear" outlined @click="clearSearch" />
          </div>
        </div>
      </template>
      <Column field="name" header="Name" :sortable="true" :filter="true" filter-field="name" />
      <Column field="abbreviation" header="Abbreviation" :sortable="true" :filter="true" filter-field="abbreviation" />
      <Column field="countryName" header="Country" :sortable="true" :filter="true" filter-field="countryName" />
      <Column field="isActive" header="Active" :sortable="true" :filter="true" filter-field="isActive" body-style="text-align: center">
        <template #body="{ data }">
          <Tag :value="data.isActive ? 'Active' : 'Inactive'" :severity="data.isActive ? 'success' : 'danger'" />
        </template>
      </Column>
      <Column header="" body-style="text-align: right; width: 6rem">
        <template #body="{ data }">
          <div class="flex justify-end gap-2">
            <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
            <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [data]; confirmDelete()" />
          </div>
        </template>
      </Column>
      <template #empty>
        <div class="text-center py-8 text-muted-color">No states found.</div>
      </template>
    </DataTable>
  </PageShell>
</template>
```

- [ ] **Step 2: Verify build**

```bash
cd app/Admin && pnpm run build-only
```

Expected: builds successfully with no warnings.

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/location/views/StatesList.vue
git commit -m "refactor(admin): inline PrimeVue DataTable in StatesList"
```

---

### Task 4: Delete CrudToolbar and FilterableDataTable wrappers

**Files:**
- Delete: `app/Admin/src/shared/components/data/CrudToolbar.vue`
- Delete: `app/Admin/src/shared/components/data/FilterableDataTable.vue`
- Modify: `app/Admin/src/shared/components/data/index.ts`

**Interfaces:**
- Consumes: Neither component is imported anywhere (confirmed after Tasks 2-3)
- Produces: Clean barrel with only StatusTag and RatingBadge exports

- [ ] **Step 1: Remove barrel exports**

In `app/Admin/src/shared/components/data/index.ts`, remove the first two lines. The file should become:

```typescript
export { default as StatusTag } from './StatusTag.vue'
export { default as RatingBadge } from './RatingBadge.vue'
```

- [ ] **Step 2: Delete wrapper component files**

```bash
rm app/Admin/src/shared/components/data/CrudToolbar.vue
rm app/Admin/src/shared/components/data/FilterableDataTable.vue
```

- [ ] **Step 3: Run lint**

```bash
cd app/Admin && pnpm run lint
```

Expected: zero lint errors.

- [ ] **Step 4: Run build**

```bash
cd app/Admin && pnpm run build-only
```

Expected: builds successfully with no warnings.

- [ ] **Step 5: Run full test suite**

```bash
cd app/Admin && pnpm run test:unit -- run
```

Expected: 413/413 tests pass.

- [ ] **Step 6: Commit**

```bash
git rm app/Admin/src/shared/components/data/CrudToolbar.vue app/Admin/src/shared/components/data/FilterableDataTable.vue
git add app/Admin/src/shared/components/data/index.ts
git commit -m "refactor(admin): remove CrudToolbar and FilterableDataTable wrappers"
```
