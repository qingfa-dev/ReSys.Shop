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
// Map: Derive the zero-based PrimeVue row offset for lazy scrolling.
const first = computed(() => (page.value - 1) * pageSize.value)

onMounted(async () => {
  // Await: Taxonomy options and the first taxon page load in parallel
  await Promise.all([loadActiveTaxonomies(), fetchTaxons()])
})

function onTaxonomyChange(id: string | null) {
  // Filter: Scope the taxon list to the selected taxonomy and reset paging.
  taxonomyId.value = id || null
  setPage(1)
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
  taxonomyId.value = null
  setPage(1)
  setSearch('')
  router.replace({ query: { ...route.query, taxonomyId: undefined } })
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

  // Trigger: Confirm before bulk-deleting the highlighted taxons.
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
      // Call: Delete each taxon, tallying failures for the result toast.
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

<template>
  <div class="flex flex-col h-full p-4">
    <!-- Section: Page Header — title and one-line taxon description -->
    <div class="flex-none flex flex-col gap-4">
      <div>
        <div class="font-semibold text-xl">Taxons</div>
        <p class="text-muted-color mt-1">Manage product classification taxons</p>
      </div>
    </div>

    <!-- Section: Scrollable Content — page body that grows and scrolls -->
    <div class="flex-1 min-h-0 mt-4">
      <!-- Section: Data Table — lazy, scrollable taxon grid -->
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
        <!-- Section: Search & Filters — search box, taxonomy scope select, and bulk actions -->
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
                :model-value="taxonomyId"
                :options="activeTaxonomies"
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
              <Button label="Reload" icon="pi pi-sync" severity="secondary" @click="refresh" />
              <Button label="Export" icon="pi pi-upload" severity="secondary" @click="exportCSV" />
            </div>
          </div>
        </template>
        <!-- Section: Table Columns — taxon descriptor, hierarchy, and count fields -->
        <Column field="name" header="Name" :sortable="true" />
        <Column field="slug" header="Slug" :sortable="true" />
        <Column field="taxonomyName" header="Taxonomy" />
        <Column field="parentName" header="Parent" />
        <Column field="depth" header="Depth" :sortable="true" body-style="text-align: center" />
        <Column field="position" header="Position" :sortable="true" />
        <Column field="taxonRuleCount" header="Rules" body-style="text-align: center" />
        <Column field="productCount" header="Products" body-style="text-align: center" />
        <!-- Section: Row Actions — edit and delete per row -->
        <Column header="" body-style="text-align: right; width: 6rem">
          <template #body="{ data }">
            <div class="flex justify-end gap-2">
              <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
              <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [data]; confirmDelete()" />
            </div>
          </template>
        </Column>
        <!-- Section: Empty State — shown when the query returns no taxons -->
        <template #empty>
          <div class="text-center py-8 text-muted-color">No taxons found.</div>
        </template>
      </DataTable>
    </div>
  </div>
</template>
