<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { INVENTORY } from '@/shared/constants/api'
import { StockItemApi } from '../services/stockItemApi'
import { useActiveStockLocations } from '../composables/useActiveStockLocations'
import type { StockItemListItem } from '../types/stockItem'
import { STOCK_ITEM_FILTER_FIELDS, STOCK_ITEM_SORT_FIELDS, STOCK_ITEM_SEARCH_FIELDS } from '../types/stockItem'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const { dt, exportCSV } = useDataTableExport()
const { items: activeStockLocations, load: loadActiveStockLocations } = useActiveStockLocations()
const search = ref('')
const selectedLocation = ref<string | null>(null)
const selectedItems = ref<StockItemListItem[]>([])

// Load: Fetch stock locations once for the filter dropdown.
loadActiveStockLocations()

const { items, loading, setSearch, setFilter, refresh } = usePagedQuery<StockItemListItem>(
  `${INVENTORY}/stock-items`,
  {
    allowedFilterFields: STOCK_ITEM_FILTER_FIELDS,
    allowedSortFields: STOCK_ITEM_SORT_FIELDS,
    allowedSearchFields: STOCK_ITEM_SEARCH_FIELDS,
    defaultSearchFields: [],
  },
)

function onSearch(value: string) {
  search.value = value
  setSearch(value)
}

function clearSearch() {
  search.value = ''
  selectedLocation.value = null
  setFilter('')
  setSearch('')
}

function onLocationChange(value: string | null) {
  selectedLocation.value = value ?? null
  setFilter(value ? `stockLocationId=${value}` : '')
}

function navigateToNew() {
  router.push('/inventory/stock-items/new')
}

function navigateToEdit(id: string) {
  router.push(`/inventory/stock-items/${id}`)
}

function confirmDelete() {
  const ids = selectedItems.value.map((s) => s.variantId).join(', ')
  // Trigger: Confirm before bulk-deleting the selected stock items.
  confirm.require({
    message: `Delete stock item${selectedItems.value.length > 1 ? 's' : ''} ${ids}? This action cannot be undone.`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      // Call: Delete each selected stock item, notifying per-item outcome.
      for (const item of selectedItems.value) {
        const result = await StockItemApi.deleteStockItem(item.id)
        if (result.isSuccess) {
          notify.success('Deleted', item.variantId)
        } else {
          notify.error('Failed', `${item.variantId}: ${result.message}`)
        }
      }
      selectedItems.value = []
      refresh()
    },
  })
}
</script>

<template>
  <div class="flex flex-col h-full">
    <!-- Section: Page Header — title and one-line description -->
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Stock Items</h1>
      <p class="text-muted-color">Manage inventory stock items</p>
    </div>

    <!-- Section: Search & Filters — search, location filter, and bulk actions -->
    <div class="flex items-center gap-3 mb-4">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText
          :model-value="search"
          placeholder="Search stock items..."
          @update:model-value="onSearch($event ?? '')"
        />
      </IconField>
      <Select
        :model-value="selectedLocation"
        :options="activeStockLocations"
        option-label="name"
        option-value="id"
        placeholder="All locations"
        show-clear
        class="w-48"
        @update:model-value="onLocationChange($event ?? null)"
      />
      <Button
        v-if="search"
        label="Clear"
        severity="secondary"
        icon="pi pi-times"
        @click="clearSearch"
      />
      <div class="flex-1" />
      <Button
        label="New Stock Item"
        icon="pi pi-plus"
        @click="navigateToNew"
      />
      <Button
        label="Reload"
        icon="pi pi-refresh"
        severity="secondary"
        @click="refresh"
      />
      <Button
        label="Export"
        icon="pi pi-download"
        severity="secondary"
        @click="exportCSV"
      />
    </div>

    <!-- Section: Data Table — scrollable stock item grid -->
    <DataTable
      ref="dt"
      v-model:selection="selectedItems"
      :value="items"
      :loading="loading"
      scrollable
      paginator
      :rows="20"
      :rows-per-page-options="[10, 20, 50]"
      data-key="id"
    >
      <!-- Section: Table Columns — location, variant, and stock-level fields -->
      <Column selection-mode="multiple" header-style="width:3rem" />
      <Column field="stockLocationId" header="Stock Location ID" />
      <Column field="variantId" header="Variant ID" />
      <Column field="countOnHand" header="Count On Hand" :sortable="true" />
      <Column field="backorderable" header="Backorderable">
        <template #body="{ data }">
          <Tag :value="data.backorderable ? 'Yes' : 'No'" :severity="data.backorderable ? 'success' : 'warn'" />
        </template>
      </Column>
      <!-- Section: Row Actions — edit and delete per stock item -->
      <Column header="Actions" header-style="width:8rem">
        <template #body="{ data }">
          <Button icon="pi pi-pencil" severity="secondary" text rounded @click="navigateToEdit(data.id)" />
          <Button
            icon="pi pi-trash"
            severity="danger"
            text
            rounded
            @click="selectedItems = [data]; confirmDelete()"
          />
        </template>
      </Column>
      <!-- Section: Empty State — shown when no stock items match -->
      <template #empty>No stock items found.</template>
    </DataTable>
  </div>
</template>
