<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Toolbar from 'primevue/toolbar'
import Plus from '@primeicons/vue/plus'
import Trash from '@primeicons/vue/trash'
import Upload from '@primeicons/vue/upload'

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
      <Toolbar>
        <template #start>
          <Button label="New" severity="secondary" class="mr-2" @click="navigateToNew">
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
    </div>

    <div class="flex-1 min-h-0 mt-4">
      <DataTable size="large"
        ref="dt"
        v-model:selection="selectedItems"
        :value="items"
        :loading="loading"
        scrollable
        :paginator="true"
        :rows="pageSize"
        filter-display="menu"
        data-key="id"
        :global-filter-fields="allowedSearchFields"
        paginator-template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :rows-per-page-options="[5, 10, 25]"
        current-page-report-template="Showing {first} to {last} of {totalRecords}"
        :pt="{ wrapper: { class: 'h-full' }, tableContainer: { class: 'h-full' } }"
      >
        <Column selection-mode="multiple" header-style="width: 3rem" />
        <template #header>
          <div class="flex justify-between items-center">
            <IconField>
              <InputIcon><i class="pi pi-search" /></InputIcon>
              <InputText
                :model-value="searchTerm"
                placeholder="Search taxonomies..."
                @update:model-value="onSearch($event ?? '')"
              />
            </IconField>
            <Button label="Clear" outlined @click="clearSearch" />
          </div>
        </template>
        <Column field="name" header="Name" :sortable="true" :filter="true" filter-field="name" />
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
