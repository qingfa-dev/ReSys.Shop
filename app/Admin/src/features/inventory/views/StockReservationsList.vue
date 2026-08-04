<script setup lang="ts">
import { ref } from 'vue'
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
import { useStockReservationList } from '../composables/useStockReservationList'
import type { ReservationState } from '../types/stockReservation'
import type { StockReservationListItem } from '../types/stockReservation'

const { dt, exportCSV } = useDataTableExport()
const search = ref('')
const selectedState = ref<ReservationState | null>(null)
const selectedItems = ref<StockReservationListItem[]>([])

const { items, loading, setFilter, refresh } = useStockReservationList()

const STATE_OPTIONS: { label: string; value: ReservationState }[] = [
  { label: 'Reserved', value: 'Reserved' },
  { label: 'Fulfilled', value: 'Fulfilled' },
  { label: 'Released', value: 'Released' },
  { label: 'Expired', value: 'Expired' },
]

const STATE_SEVERITY: Record<ReservationState, string> = {
  Reserved: 'info',
  Fulfilled: 'success',
  Released: 'warn',
  Expired: 'danger',
}

function onStateFilterChange(value: ReservationState | null) {
  selectedState.value = value
  // Filter: Restrict the list to the selected reservation state.
  setFilter(value ? `state=${value}` : '')
}

function stateSeverity(state: ReservationState): string {
  return STATE_SEVERITY[state]
}
</script>

<template>
  <div class="flex flex-col h-full">
    <!-- Section: Page Header — title and one-line description -->
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Stock Reservations</h1>
      <p class="text-muted-color">System-managed stock reservations</p>
    </div>

    <!-- Section: Search & Filters — search, state filter, reload, and export -->
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
        class="w-48"
        @change="onStateFilterChange($event.value)"
      />
      <div class="flex-1" />
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

    <!-- Section: Data Table — read-only grid of stock reservations -->
    <DataTable
      ref="dt"
      :value="items"
      :loading="loading"
      v-model:selection="selectedItems"
      scrollable
      paginator
      :rows="20"
      :rows-per-page-options="[10, 20, 50]"
      data-key="id"
    >
      <Column selection-mode="multiple" header-style="width: 3rem" />
      <!-- Section: Table Columns — order, quantity, state, and expiry fields -->
      <Column field="variantId" header="Variant ID" />
      <Column field="orderId" header="Order ID">
        <template #body="{ data }">
          {{ data.orderId ?? '—' }}
        </template>
      </Column>
      <Column field="quantity" header="Quantity" />
      <Column field="state" header="State">
        <template #body="{ data }">
          <Tag :value="data.state" :severity="stateSeverity(data.state)" />
        </template>
      </Column>
      <Column field="expiresAtUtc" header="Expires" :sortable="true">
        <template #body="{ data }">
          {{ data.expiresAtUtc ? formatDateTimeUtc(data.expiresAtUtc) : '—' }}
        </template>
      </Column>
      <Column field="createdAtUtc" header="Created" :sortable="true">
        <template #body="{ data }">
          {{ formatDateTimeUtc(data.createdAtUtc) }}
        </template>
      </Column>
      <!-- Section: Empty State — shown when no stock reservations match -->
      <template #empty>No stock reservations found.</template>
    </DataTable>
  </div>
</template>
