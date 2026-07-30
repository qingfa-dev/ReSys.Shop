<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import TreeTable from 'primevue/treetable'
import Column from 'primevue/column'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { TaxonApi } from '../services/taxonApi'
import type { TaxonListItem, TaxonTreeItem } from '../types/taxon'
import { TAXON_FILTER_FIELDS, TAXON_SORT_FIELDS } from '../types/taxon'

const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()

const { dt, exportCSV } = useDataTableExport()
const selectedItems = ref<TaxonListItem[]>([])
const searchTerm = ref('')
const viewMode = ref<'table' | 'tree'>('table')
const allowedSearchFields = ['name', 'slug']

const {
  items,
  loading,
  setSearch,
  setFilter,
  refresh,
} = usePagedQuery<TaxonListItem>('api/catalog/taxonomies/taxons', {
  allowedFilterFields: TAXON_FILTER_FIELDS,
  allowedSortFields: TAXON_SORT_FIELDS,
  allowedSearchFields,
  defaultSearchFields: allowedSearchFields,
  defaultSearchMode: 'any',
  defaultSort: ['lft'],
  defaultPageSize: 25,
})

const treeData = ref<any[]>([])
const treeLoading = ref(false)
const treeFilter = ref('')

onMounted(() => {
  const taxonomyId = route.query.taxonomyId as string | undefined
  if (taxonomyId) {
    setFilter(`taxonomyId=${taxonomyId}`)
  }
})

function addTreeNodeKeys(nodes: any[]): any[] {
  return nodes.map(n => ({
    ...n,
    key: n.id,
    children: n.children ? addTreeNodeKeys(n.children) : [],
  }))
}

async function loadTree() {
  const taxonomyId = route.query.taxonomyId as string | undefined
  if (!taxonomyId) return

  treeLoading.value = true
  const result = await TaxonApi.getTree(taxonomyId)
  if (result.isSuccess && result.value?.tree) {
    treeData.value = addTreeNodeKeys(result.value.tree) as any
  }
  treeLoading.value = false
}

function toggleViewMode() {
  if (viewMode.value === 'table') {
    viewMode.value = 'tree'
    if (treeData.value.length === 0) {
      loadTree()
    }
  } else {
    viewMode.value = 'table'
  }
}

function navigateToNew() {
  const taxonomyId = route.query.taxonomyId as string | undefined
  const query = taxonomyId ? `?taxonomyId=${taxonomyId}` : ''
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

function filterTree(name: string) {
  treeFilter.value = name ? name.toLowerCase() : ''
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
        scrollable
        :paginator="true"
        :rows="20"
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
              <Button label="Clear" outlined @click="clearSearch" />
            </div>
            <div class="flex items-center gap-2">
              <Button label="New Taxon" icon="pi pi-plus" severity="primary" @click="navigateToNew" />
              <Button label="Reload" icon="pi pi-sync" severity="secondary" @click="refresh" />
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
        <Column field="taxonomyName" header="Taxonomy" :sortable="true" />
        <Column field="parentName" header="Parent" :sortable="true" />
        <Column field="depth" header="Depth" :sortable="true" body-style="text-align: center" />
        <Column field="position" header="Position" :sortable="true" />
        <Column field="taxonRuleCount" header="Rules" :sortable="true" body-style="text-align: center" />
        <Column field="productCount" header="Products" :sortable="true" body-style="text-align: center" />
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
                <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [{ ...node.data, ...node.data }] as any; confirmDelete()" />
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
