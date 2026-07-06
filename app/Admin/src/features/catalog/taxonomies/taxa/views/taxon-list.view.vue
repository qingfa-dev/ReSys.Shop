<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTaxonStore } from '../stores/taxon.store'
import { useTaxonomyStore } from '../../stores/taxonomy.store'
import { storeToRefs } from 'pinia'
import { taxonLocales } from '../locales/taxon.locales'
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use'
import AppBreadcrumb from '@/shared/components/breadcrumb.component.vue'
import { useToast } from '@/shared/composables/toast.use'
import { useConfirm } from 'primevue/useconfirm'
import { FilterMatchMode } from '@primevue/core/api'
import type { DataTablePageEvent, DataTableSortEvent, DataTableFilterMeta } from 'primevue/datatable'
import { QueryBuilder } from '@/shared/utils/query-builder.utils'
import type { TaxonListItem } from '../types/taxon.types'

const t = taxonLocales
const route = useRoute()
const router = useRouter()
const store = useTaxonStore()
const taxonomyStore = useTaxonomyStore()
const { currentTaxons: items, loading, totalRecords } = storeToRefs(store)
const { showToast } = useToast()
const { handleApiResult } = useApiErrorHandler()
const confirm = useConfirm()

const taxonomies = ref<{label: string, value: string}[]>([])

const filters = ref<DataTableFilterMeta>({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  taxonomy_id: { value: null, matchMode: FilterMatchMode.EQUALS }
})

const lazyParams = ref({
    page: 1,
    rows: 10,
    search: '',
    taxonomy_id: undefined as string | undefined
})

const loadItems = async () => {
  // Load taxonomies for filter
  const taxResult = await taxonomyStore.fetchTaxonomies({ page_size: 100 })
  if (taxResult.success && taxResult.data) {
      taxonomies.value = taxResult.data.map(tx => ({ label: tx.presentation || tx.name, value: tx.id }))
  }

  await fetchPagedData()
}

const fetchPagedData = async () => {
    const taxId = lazyParams.value.taxonomy_id || undefined
    await store.fetchTaxons(taxId || '', {
        page: lazyParams.value.page,
        page_size: lazyParams.value.rows,
        search: lazyParams.value.search || undefined
    })
}

const onPage = (event: DataTablePageEvent) => {
  lazyParams.value.page = (event.page || 0) + 1
  lazyParams.value.rows = event.rows
  fetchPagedData()
}

const onFilter = () => {
  lazyParams.value.page = 1
  lazyParams.value.search = (filters.value.global as any).value
  lazyParams.value.taxonomy_id = (filters.value.taxonomy_id as any).value
  fetchPagedData()
}

const clearFilters = () => {
  filters.value = {
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    taxonomy_id: { value: null, matchMode: FilterMatchMode.EQUALS }
  }
  onFilter()
}

const confirmDelete = (item: TaxonListItem) => {
  confirm.require({
    message: (t.confirm?.delete_message as string || 'Delete "{name}"?').replace('{name}', item.presentation),
    header: t.confirm?.delete_header as string || 'Warning',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: t.actions?.cancel,
    acceptLabel: t.actions?.delete_taxon,
    acceptProps: { severity: 'danger' },
    accept: async () => {
      const result = await store.deleteTaxon(item.taxonomy_id, item.id)
      if (result.success) {
        showToast('success', 'Deleted', t.messages?.delete_success || 'Category deleted')
      }
    }
  })
}

onMounted(() => {
  loadItems()
})
</script>

<template>
  <div class="p-6">
    <AppBreadcrumb :locales="t" />
    
    <div class="flex flex-col items-start justify-between gap-4 mb-8 md:flex-row md:items-center">
      <div>
        <h2 class="text-3xl font-black tracking-tight text-surface-900 dark:text-surface-50">
          {{ t.titles?.manager || 'Category List' }}
        </h2>
        <div class="flex items-center gap-2 mt-1">
          <span class="text-surface-500 dark:text-surface-400">
            {{ t.descriptions?.manager }}
          </span>
          <Badge :value="totalRecords" severity="info" class="ml-2" />
        </div>
      </div>
      <Button 
        :label="t.actions?.add_taxon" 
        icon="pi pi-plus" 
        @click="router.push({ name: 'catalog.taxa.create', params: { taxonomyId: taxonomies[0]?.value || 'root' } })"
        class="px-4 shadow-lg rounded-xl"
        :disabled="taxonomies.length === 0"
      />
    </div>

    <div class="overflow-hidden border shadow-sm bg-surface-0 dark:bg-surface-900 rounded-2xl border-surface-100 dark:border-surface-800">
      <DataTable
        v-model:filters="filters"
        :value="items"
        :loading="loading"
        :totalRecords="totalRecords"
        lazy
        paginator
        :rows="lazyParams.rows"
        :first="(lazyParams.page - 1) * lazyParams.rows"
        @page="onPage"
        filterDisplay="menu"
        scrollable
        rowHover
      >
        <template #header>
          <div class="flex flex-col items-center justify-between gap-4 md:flex-row">
            <div class="flex gap-2 w-full md:w-auto">
                <IconField iconPosition="left" class="w-full md:w-64">
                <InputIcon class="pi pi-search" />
                <InputText 
                    v-model="(filters.global as any).value" 
                    :placeholder="t.placeholders?.search || 'Search...'" 
                    @keyup.enter="onFilter" 
                    class="w-full rounded-xl"
                />
                </IconField>
                <Select 
                    v-model="(filters.taxonomy_id as any).value" 
                    :options="taxonomies" 
                    optionLabel="label" 
                    optionValue="value" 
                    placeholder="All Taxonomies" 
                    showClear
                    @change="onFilter"
                    class="w-full md:w-48 rounded-xl"
                />
            </div>
            
            <Button 
              type="button" 
              icon="pi pi-filter-slash" 
              label="Clear" 
              outlined 
              @click="clearFilters" 
              class="w-full rounded-xl md:w-auto"
            />
          </div>
        </template>

        <template #empty>
          <div class="flex flex-col items-center justify-center py-20 text-surface-400">
            <i class="mb-4 text-6xl pi pi-tags opacity-20"></i>
            <p class="text-xl font-medium">{{ t.messages?.empty_tree }}</p>
          </div>
        </template>

        <Column field="presentation" :header="t.labels?.presentation">
          <template #body="{ data }">
            <div class="flex flex-col">
                <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.presentation }}</span>
                <small class="text-surface-400 font-mono">{{ data.permalink }}</small>
            </div>
          </template>
        </Column>

        <Column field="name" :header="t.labels?.name">
            <template #body="{ data }">
                <span class="font-mono text-xs">{{ data.name }}</span>
            </template>
        </Column>

        <Column field="product_count" header="Products" class="text-center">
            <template #body="{ data }">
                <Badge :value="data.product_count" severity="secondary" />
            </template>
        </Column>

        <Column class="w-32 text-right">
          <template #body="{ data }">
            <div class="flex justify-end gap-1">
              <Button icon="pi pi-pencil" severity="secondary" text rounded @click="router.push({ name: 'catalog.taxa.edit', params: { taxonomyId: data.taxonomy_id, id: data.id } })" />
              <Button icon="pi pi-trash" severity="danger" text rounded @click="confirmDelete(data)" />
            </div>
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>
