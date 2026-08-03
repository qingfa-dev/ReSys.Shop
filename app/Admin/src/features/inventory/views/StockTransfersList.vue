<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Select from 'primevue/select'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { formatDateTimeUtc } from '@/shared/utils/date'
import { useStockTransferList } from '../composables/useStockTransferList'
import { useActiveStockLocations } from '../composables/useActiveStockLocations'
import type { StockTransferState } from '../types/stockTransfer'

const router = useRouter()
const { dt, exportCSV } = useDataTableExport()
const { items: activeStockLocations, load: loadActiveStockLocations } = useActiveStockLocations()
const search = ref('')
const selectedState = ref<StockTransferState | null>(null)
const selectedSourceLocation = ref<string | null>(null)
const selectedDestinationLocation = ref<string | null>(null)

const { items, loading, setFilter, refresh } = useStockTransferList()

// Load: Fetch stock locations once for the filter dropdowns.
loadActiveStockLocations()

const STATE_OPTIONS: { label: string; value: StockTransferState }[] = [
  { label: 'Draft', value: 'Draft' },
  { label: 'In Transit', value: 'InTransit' },
  { label: 'Received', value: 'Received' },
  { label: 'Canceled', value: 'Canceled' },
]

const STATE_SEVERITY: Record<StockTransferState, string> = {
  Draft: 'warn',
  InTransit: 'info',
  Received: 'success',
  Canceled: 'danger',
}

function applyFilters() {
  // Filter: Combine the selected state, source, and destination clauses.
  const clauses: string[] = []
  if (selectedState.value) clauses.push(`state=${selectedState.value}`)
  if (selectedSourceLocation.value) clauses.push(`sourceLocationId=${selectedSourceLocation.value}`)
  if (selectedDestinationLocation.value) clauses.push(`destinationLocationId=${selectedDestinationLocation.value}`)
  setFilter(clauses.join(','))
}

function onStateFilterChange(value: StockTransferState | null) {
  selectedState.value = value
  applyFilters()
}

function onSourceLocationChange(value: string | null) {
  selectedSourceLocation.value = value ?? null
  applyFilters()
}

function onDestinationLocationChange(value: string | null) {
  selectedDestinationLocation.value = value ?? null
  applyFilters()
}

function stateSeverity(state: StockTransferState): string {
  return STATE_SEVERITY[state]
}

function navigateToNew() {
  router.push('/inventory/stock-transfers/new')
}

function navigateToEdit(id: string) {
  router.push(`/inventory/stock-transfers/${id}`)
}
</script>

<template>
  <div class="flex flex-col h-full">
    <!-- Section: Page Header — title and one-line description -->
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Stock Transfers</h1>
      <p class="text-muted-color">Transfer stock between locations</p>
    </div>

    <!-- Section: Search & Filters — search, state, and location filters -->
    <div class="flex items-center gap-3 mb-4">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText v-model="search" placeholder="Search..." />
      </IconField>
      <Select
        v-model="selectedState"
        :options="STATE_OPTIONS"
        option-label="label"
        option-value="value"
        placeholder="All States"
        show-clear
        class="w-40"
        @change="onStateFilterChange($event.value)"
      />
      <Select
        :model-value="selectedSourceLocation"
        :options="activeStockLocations"
        option-label="name"
        option-value="id"
        placeholder="All sources"
        show-clear
        class="w-40"
        @update:model-value="onSourceLocationChange($event ?? null)"
      />
      <Select
        :model-value="selectedDestinationLocation"
        :options="activeStockLocations"
        option-label="name"
        option-value="id"
        placeholder="All destinations"
        show-clear
        class="w-44"
        @update:model-value="onDestinationLocationChange($event ?? null)"
      />
      <div class="flex-1" />
      <Button
        label="New Transfer"
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

    <!-- Section: Data Table — scrollable transfer grid -->
    <DataTable
      ref="dt"
      :value="items"
      :loading="loading"
      scrollable
      paginator
      :rows="20"
      :rows-per-page-options="[10, 20, 50]"
      data-key="id"
    >
      <!-- Section: Table Columns — transfer descriptor and state fields -->
      <Column field="number" header="Number" :sortable="true" />
      <Column field="reference" header="Reference">
        <template #body="{ data }">
          {{ data.reference ?? '—' }}
        </template>
      </Column>
      <Column field="sourceLocationId" header="Source" />
      <Column field="destinationLocationId" header="Destination" />
      <Column field="state" header="State" :sortable="true">
        <template #body="{ data }">
          <Tag :value="data.state" :severity="stateSeverity(data.state)" />
        </template>
      </Column>
      <Column field="totalItems" header="Items" />
      <Column field="createdAtUtc" header="Created" :sortable="true">
        <template #body="{ data }">
          {{ formatDateTimeUtc(data.createdAtUtc) }}
        </template>
      </Column>
      <!-- Section: Row Actions — edit per transfer -->
      <Column header="Actions" header-style="width:5rem">
        <template #body="{ data }">
          <Button icon="pi pi-pencil" severity="secondary" text rounded @click="navigateToEdit(data.id)" />
        </template>
      </Column>
      <!-- Section: Empty State — shown when no stock transfers match -->
      <template #empty>No stock transfers found.</template>
    </DataTable>
  </div>
</template>
