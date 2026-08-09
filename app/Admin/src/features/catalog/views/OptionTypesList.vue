<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import type { DataTablePageEvent, DataTableSortEvent } from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Message from 'primevue/message'

import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { OptionTypeApi } from '../services/optionTypeApi'
import type { OptionTypeListItem } from '../types/optionType'
import { OPTION_TYPE_FILTER_FIELDS, OPTION_TYPE_SORT_FIELDS } from '../types/optionType'
import { tableFirst } from '../utils/tablePaging'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()

const { dt, exportCSV } = useDataTableExport()
const selectedItems = ref<OptionTypeListItem[]>([])
const searchTerm = ref('')
const allowedSearchFields = ['name', 'presentation']

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
  setFilter,
  refresh,
} = usePagedQuery<OptionTypeListItem>('api/admin/catalog/option-types', {
  allowedFilterFields: OPTION_TYPE_FILTER_FIELDS,
  allowedSortFields: OPTION_TYPE_SORT_FIELDS,
  allowedSearchFields,
  defaultSearchFields: allowedSearchFields,
  defaultSearchMode: 'any',
  defaultSort: ['name'],
  defaultPageSize: 25,
})

// Map: Derive the zero-based PrimeVue row offset for lazy scrolling.
const first = computed(() => tableFirst(page.value, pageSize.value))

const filterable = ref<string | null>(null)
const filterableOptions = [
  { label: 'Yes', value: 'true' },
  { label: 'No', value: 'false' },
]

function onFilterableChange(value: string | null) {
  filterable.value = value ?? null
  // Filter: Scope the query to the selected filterable value.
  setFilter(value ? `filterable=${value}` : '')
}

function navigateToNew() {
  router.push('/catalog/option-types/new')
}

function navigateToEdit(id: string) {
  router.push(`/catalog/option-types/${id}`)
}

function onSearch(value: string) {
  searchTerm.value = value
  setSearch(value)
}

function clearSearch() {
  searchTerm.value = ''
  filterable.value = null
  setFilter('')
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

  // Trigger: Confirm before bulk-deleting the highlighted option types.
  confirm.require({
    message: `Are you sure you want to delete ${selectedItems.value.length > 1 ? 'these option types' : 'this option type'}?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const ids = selectedItems.value.map(i => i.id)
      const names = selectedItems.value.map(i => i.name)
      let failed = 0
      // Call: Delete each option type, tallying failures for the result toast.
      for (const id of ids) {
        const result = await OptionTypeApi.deleteOptionType(id)
        if (!result.isSuccess) failed++
      }
      selectedItems.value = []
      refresh()
      if (failed === 0) {
        notify.success(
          ids.length > 1 ? 'Option types deleted' : 'Option type deleted',
          ids.length > 1
            ? `${ids.length} option types have been removed.`
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
    <!-- Section: Page Header — title and one-line option-type description -->
    <div class="flex-none flex flex-col gap-4">
      <div>
        <div class="font-semibold text-xl">Option Types</div>
        <p class="text-muted-color mt-1">Manage product option types (Size, Color, Material, etc.)</p>
      </div>
    </div>

    <!-- Section: Scrollable Content — page body that grows and scrolls -->
    <div class="flex-1 min-h-0 mt-4">
      <!-- Section: Error State — full-area message with a reload action -->
      <div v-if="error" class="flex items-center justify-center h-full">
        <Message severity="error" :closable="false" class="w-full max-w-lg">
          <div class="flex flex-col gap-2">
            <span>{{ error }}</span>
            <Button label="Reload" icon="pi pi-sync" severity="secondary" size="small" @click="refresh" />
          </div>
        </Message>
      </div>

      <!-- Section: Data Table — lazy, scrollable option-type grid -->
      <DataTable v-else size="large"
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
        <!-- Section: Search & Filters — search box and filterable select plus bulk actions -->
        <template #header>
          <div class="flex justify-between items-center">
            <div class="flex items-center gap-2">
              <FloatLabel variant="on">
                <IconField>
                  <InputIcon class="pi pi-search" />
                  <InputText
                    :model-value="searchTerm"
                    placeholder="Search option types..."
                    @update:model-value="onSearch($event ?? '')"
                  />
                </IconField>
                <label>Search</label>
              </FloatLabel>
              <Select
                :model-value="filterable"
                :options="filterableOptions"
                option-label="label"
                option-value="value"
                placeholder="All (Filterable)"
                show-clear
                class="w-40"
                @update:model-value="onFilterableChange($event ?? null)"
              />
              <Button label="Clear" outlined @click="clearSearch" />
            </div>
            <div class="flex items-center gap-2">
              <Button label="New Option Type" icon="pi pi-plus" severity="primary" @click="navigateToNew" />
              <Button label="Reload" icon="pi pi-sync" severity="secondary" @click="refresh" />
              <Button label="Export" icon="pi pi-upload" severity="secondary" @click="exportCSV" />
            </div>
          </div>
        </template>
        <!-- Section: Table Columns — option-type descriptor and usage-count fields -->
        <Column field="name" header="Name" :sortable="true" />
        <Column field="presentation" header="Presentation" :sortable="true" />
        <Column field="position" header="Position" :sortable="true" />
        <Column field="filterable" header="Filterable" :sortable="true" body-style="text-align: center">
          <template #body="{ data }">
            <Tag :value="data.filterable ? 'Yes' : 'No'" :severity="data.filterable ? 'success' : 'secondary'" />
          </template>
        </Column>
        <Column field="optionValuesCount" header="Values" :sortable="true" />
        <Column field="productsCount" header="Products" :sortable="true" />
        <!-- Section: Row Actions — edit and delete per row -->
        <Column header="" body-style="text-align: right; width: 6rem">
          <template #body="{ data }">
            <div class="flex justify-end gap-2">
              <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
              <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [data]; confirmDelete()" />
            </div>
          </template>
        </Column>
        <!-- Section: Empty State — shown when the query returns no option types -->
        <template #empty>
          <div class="text-center py-8 text-muted-color">No option types found.</div>
        </template>
      </DataTable>
    </div>
  </div>
</template>
