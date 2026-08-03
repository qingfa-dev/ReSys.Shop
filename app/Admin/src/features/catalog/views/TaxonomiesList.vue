<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import type { DataTablePageEvent, DataTableSortEvent } from 'primevue/datatable'
import Column from 'primevue/column'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { TaxonomyApi } from '../services/taxonomyApi'
import type { TaxonomyListItem } from '../types/taxonomy'
import { TAXONOMY_FILTER_FIELDS, TAXONOMY_SORT_FIELDS } from '../types/taxonomy'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()

const { dt, exportCSV } = useDataTableExport()
const selectedItems = ref<TaxonomyListItem[]>([])
const searchTerm = ref('')
const allowedSearchFields = ['name', 'presentation']

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
  refresh,
} = usePagedQuery<TaxonomyListItem>('api/catalog/taxonomies', {
  allowedFilterFields: TAXONOMY_FILTER_FIELDS,
  allowedSortFields: TAXONOMY_SORT_FIELDS,
  allowedSearchFields,
  defaultSearchFields: allowedSearchFields,
  defaultSearchMode: 'any',
  defaultSort: ['name'],
  defaultPageSize: 25,
})

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

function navigateToNew() {
  router.push('/catalog/taxonomies/new')
}

function navigateToEdit(id: string) {
  router.push(`/catalog/taxonomies/${id}`)
}

function navigateToTaxons(id: string) {
  router.push(`/catalog/taxons?taxonomyId=${id}`)
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
    message: `Are you sure you want to delete ${selectedItems.value.length > 1 ? 'these taxonomies' : 'this taxonomy'}?`,
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
        const result = await TaxonomyApi.deleteTaxonomy(id)
        if (!result.isSuccess) failed++
      }
      selectedItems.value = []
      refresh()
      if (failed === 0) {
        notify.success(
          ids.length > 1 ? 'Taxonomies deleted' : 'Taxonomy deleted',
          ids.length > 1
            ? `${ids.length} taxonomies have been removed.`
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
    <div class="flex-none flex flex-col gap-4">
      <div>
        <div class="font-semibold text-xl">Taxonomies</div>
        <p class="text-muted-color mt-1">Manage product classification taxonomies</p>
      </div>
    </div>

    <div class="flex-1 min-h-0 mt-4">
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
                    placeholder="Search taxonomies..."
                    @update:model-value="onSearch($event ?? '')"
                  />
                </IconField>
                <label>Search</label>
              </FloatLabel>
              <Button label="Clear" outlined @click="clearSearch" />
            </div>
            <div class="flex items-center gap-2">
              <Button label="New Taxonomy" icon="pi pi-plus" severity="primary" @click="navigateToNew" />
              <Button label="Reload" icon="pi pi-sync" severity="secondary" @click="refresh" />
              <Button label="Export" icon="pi pi-upload" severity="secondary" @click="exportCSV" />
            </div>
          </div>
        </template>
        <Column field="name" header="Name" :sortable="true" />
        <Column field="presentation" header="Presentation" :sortable="true" />
        <Column field="position" header="Position" :sortable="true" />
        <Column field="taxonsCount" header="Taxons" :sortable="true" />
        <Column header="" body-style="text-align: right; width: 10rem">
          <template #body="{ data }">
            <div class="flex justify-end gap-2">
              <Button icon="pi pi-sitemap" severity="secondary" text rounded aria-label="Taxons" @click="navigateToTaxons(data.id)" />
              <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
              <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [data]; confirmDelete()" />
            </div>
          </template>
        </Column>
        <template #empty>
          <div class="text-center py-8 text-muted-color">No taxonomies found.</div>
        </template>
      </DataTable>
    </div>
  </div>
</template>
