<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import type { DataTablePageEvent, DataTableSortEvent } from 'primevue/datatable'
import Column from 'primevue/column'
import Select from 'primevue/select'
import Tag from 'primevue/tag'

import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useActiveList } from '@/shared/composables'
import { useNotify } from '@/shared/composables/useNotify'
import { StateApi } from '../services/stateApi'
import { CountryApi } from '../services/countryApi'
import type { StateListItem } from '../types/state'
import type { CountryListItem } from '../types/country'
import { toCountryQueryParams } from '../types/country'
import { STATE_FILTER_FIELDS, STATE_SORT_FIELDS } from '../types/state'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const { items: activeCountries, load: loadActiveCountries } = useActiveList<CountryListItem>(() => CountryApi.getCountries(toCountryQueryParams({ isActive: true })))

const { dt, exportCSV } = useDataTableExport()
const selectedCountryId = ref<string | null>(null)
const selectedItems = ref<StateListItem[]>([])
const searchTerm = ref('')

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
  setFilter,
  refresh,
} =
  usePagedQuery<StateListItem>((params) => StateApi.getStates(params), {
    allowedFilterFields: STATE_FILTER_FIELDS,
    allowedSortFields: STATE_SORT_FIELDS,
    allowedSearchFields: STATE_FILTER_FIELDS,
    defaultSearchFields: STATE_FILTER_FIELDS,
    defaultSearchMode: 'any',
    defaultSort: ['name'],
    defaultPageSize: 20,
  })

// Map: Derive the zero-based PrimeVue row offset for lazy scrolling.
const first = computed(() => (page.value - 1) * pageSize.value)

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

onMounted(() => {
  // Await: Country options for the country filter Select
  loadActiveCountries()
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
  selectedCountryId.value = null
  setFilter('')
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

  // Trigger: Confirm before bulk-deleting the highlighted states.
  confirm.require({
    message: `Are you sure you want to delete ${selectedItems.value.length > 1 ? 'these states' : 'this state'}?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const ids = selectedItems.value.map(i => i.id)
      const names = selectedItems.value.map(i => i.name)
      let failed = 0
      // Call: Delete each selected state, tallying failures for the toast.
      for (const id of ids) {
        const result = await StateApi.deleteState(id)
        if (!result.isSuccess) failed++
      }
      selectedItems.value = []
      refresh()
      if (failed === 0) {
        notify.success(
          ids.length > 1 ? 'States deleted' : 'State deleted',
          ids.length > 1
            ? `${ids.length} states have been removed.`
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

<template>
  <div class="flex flex-col h-full p-4">
    <!-- Section: Page Header — title and one-line description -->
    <div class="flex-none flex flex-col gap-4">
      <div>
        <div class="font-semibold text-xl">States</div>
        <p class="text-muted-color mt-1">Manage states and provinces for countries</p>
      </div>
    </div>

    <!-- Section: Scrollable Content — page body that grows and scrolls -->
    <div class="flex-1 min-h-0 mt-4">
      <!-- Section: Data Table — lazy, selectable state grid -->
      <DataTable size="large"
        ref="dt"
        v-model:selection="selectedItems"
        :value="items"
        :loading="loading"
        :total-records="totalCount"
        :first="first"
        :rows="pageSize"
        scrollable
        :paginator="true"
        lazy
        data-key="id"
        :global-filter-fields="STATE_FILTER_FIELDS"
        paginator-template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :rows-per-page-options="[5, 10, 25]"
        current-page-report-template="Showing {first} to {last} of {totalRecords}"
        @page="onPage"
        @update:rows="onRows"
        @sort="onSort"
        :pt="{ wrapper: { class: 'h-full' }, tableContainer: { class: 'h-full' } }"
      >
        <Column selection-mode="multiple" header-style="width: 3rem" />
        <!-- Section: Search & Filters — search box, country filter, and bulk actions -->
        <template #header>
          <div class="flex justify-between items-center">
            <div class="flex items-center gap-2">
              <FloatLabel variant="on">
                <IconField>
                  <InputIcon class="pi pi-search" />
                  <InputText
                    :model-value="searchTerm"
                    placeholder="Search states..."
                    @update:model-value="onSearch($event ?? '')"
                  />
                </IconField>
                <label>Search</label>
              </FloatLabel>
              <Button label="Clear" outlined @click="clearSearch" />
              <label class="text-sm text-muted-color whitespace-nowrap ml-2">Country:</label>
              <Select
                v-model="selectedCountryId"
                :options="activeCountries"
                option-label="name"
                option-value="id"
                placeholder="All Countries"
                show-clear
                class="w-48"
                @change="onCountryFilterChange($event.value)"
              />
            </div>
            <div class="flex items-center gap-2">
              <Button label="New State" icon="pi pi-plus" severity="primary" @click="navigateToNew" />
              <Button label="Reload" icon="pi pi-sync" severity="secondary" @click="refresh" />
              <Button label="Export" icon="pi pi-upload" severity="secondary" @click="exportCSV" />
            </div>
          </div>
        </template>
        <!-- Section: Table Columns — state identity and status fields -->
        <Column field="name" header="Name" :sortable="true" />
        <Column field="abbreviation" header="Abbreviation" :sortable="true" />
        <Column field="countryName" header="Country" :sortable="true" />
        <Column field="isActive" header="Active" :sortable="true" body-style="text-align: center">
          <template #body="{ data }">
            <Tag :value="data.isActive ? 'Active' : 'Inactive'" :severity="data.isActive ? 'success' : 'danger'" />
          </template>
        </Column>
        <!-- Section: Row Actions — edit and delete per state -->
        <Column header="" body-style="text-align: right; width: 6rem">
          <template #body="{ data }">
            <div class="flex justify-end gap-2">
              <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
              <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [data]; confirmDelete()" />
            </div>
          </template>
        </Column>
        <!-- Section: Empty State — shown when no states match -->
        <template #empty>
          <div class="text-center py-8 text-muted-color">No states found.</div>
        </template>
      </DataTable>
    </div>
  </div>
</template>
