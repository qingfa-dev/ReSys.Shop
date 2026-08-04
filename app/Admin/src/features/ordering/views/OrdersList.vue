<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import Select from 'primevue/select'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { formatDateTimeUtc } from '@/shared/utils/date'
import { useOrderList } from '../composables/useOrderList'
import type { OrderListItem, OrderStatus, CheckoutState, ShipmentState } from '../types/order'
import { ORDER_SEARCH_FIELDS } from '../types/order'

const router = useRouter()
const { dt, exportCSV } = useDataTableExport()
const selectedItems = ref<OrderListItem[]>([])
const search = ref('')
const statusFilter = ref<OrderStatus | null>(null)
const checkoutStateFilter = ref<CheckoutState | null>(null)

const { items, loading, setFilter, setSearch, refresh } = useOrderList({
  defaultSearchFields: ORDER_SEARCH_FIELDS,
  defaultSort: ['-createdAtUtc'],
})

const STATUS_OPTIONS: OrderStatus[] = ['Draft', 'Placed', 'Canceled', 'Expired']
const CHECKOUT_STATE_OPTIONS: CheckoutState[] = ['Address', 'Delivery', 'Payment', 'Confirm', 'Complete']

const STATUS_SEVERITY: Record<OrderStatus, string> = {
  Draft: 'warn',
  Placed: 'success',
  Canceled: 'danger',
  Expired: 'secondary',
}

const SHIPMENT_SEVERITY: Record<ShipmentState, string> = {
  pending: 'warn',
  delivered: 'success',
  partial: 'info',
  ready: 'info',
  backorder: 'warn',
  canceled: 'danger',
}

function onSearch(value: string) {
  search.value = value
  setSearch(value)
}

function clearSearch() {
  search.value = ''
  setSearch('')
}

function applyFilters() {
  // Filter: Combine status and checkout-state clauses for the server request.
  const clauses: string[] = []
  if (statusFilter.value) clauses.push(`status=${statusFilter.value}`)
  if (checkoutStateFilter.value) clauses.push(`checkoutState=${checkoutStateFilter.value}`)
  setFilter(clauses.join(','))
}

function onStatusFilterChange(value: OrderStatus | null | undefined) {
  statusFilter.value = value ?? null
  applyFilters()
}

function onCheckoutStateFilterChange(value: CheckoutState | null | undefined) {
  checkoutStateFilter.value = value ?? null
  applyFilters()
}

function statusSeverity(status: OrderStatus): string {
  return STATUS_SEVERITY[status]
}

function shipmentSeverity(state: ShipmentState | null | undefined): string {
  return state ? SHIPMENT_SEVERITY[state] : 'secondary'
}

function formatShipmentState(state: string | null | undefined): string {
  return state ?? '—'
}

function formatCurrency(value: number | null | undefined): string {
  return (value ?? 0).toLocaleString('en-US', { style: 'currency', currency: 'USD' })
}

function navigateToDetail(id: string) {
  router.push(`/ordering/orders/${id}`)
}
</script>

<template>
  <div class="flex flex-col h-full">
    <!-- Section: Page Header — title and one-line description -->
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Orders</h1>
      <p class="text-muted-color">View and manage customer orders</p>
    </div>

    <!-- Section: Search & Filters — search box and status/checkout-state selects -->
    <div class="flex items-center gap-3 mb-4 flex-wrap">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText
          :model-value="search"
          placeholder="Search orders..."
          @update:model-value="onSearch($event ?? '')"
        />
      </IconField>
      <Button
        v-if="search"
        label="Clear"
        severity="secondary"
        icon="pi pi-times"
        @click="clearSearch"
      />
      <Select
        :model-value="statusFilter"
        :options="STATUS_OPTIONS"
        placeholder="All Statuses"
        show-clear
        class="w-40"
        @change="onStatusFilterChange($event.value)"
      />
      <Select
        :model-value="checkoutStateFilter"
        :options="CHECKOUT_STATE_OPTIONS"
        placeholder="All Checkout States"
        show-clear
        class="w-48"
        @change="onCheckoutStateFilterChange($event.value)"
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

    <!-- Section: Data Table — order grid with inline status and totals -->
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
      <Column selection-mode="multiple" header-style="width: 3rem" />
      <!-- Section: Table Columns — order identity, status, and totals -->
      <Column field="number" header="Order #" :sortable="true" />
      <Column field="email" header="Customer">
        <template #body="{ data }">
          {{ data.email ?? '—' }}
        </template>
      </Column>
      <Column field="status" header="Status" :sortable="true">
        <template #body="{ data }">
          <Tag :value="data.status" :severity="statusSeverity(data.status)" />
        </template>
      </Column>
      <Column field="shipmentState" header="Shipment">
        <template #body="{ data }">
          <Tag :value="formatShipmentState(data.shipmentState)" :severity="shipmentSeverity(data.shipmentState)" />
        </template>
      </Column>
      <Column field="total" header="Total" :sortable="true">
        <template #body="{ data }">
          {{ formatCurrency(data.total) }}
        </template>
      </Column>
      <Column field="paymentTotal" header="Paid">
        <template #body="{ data }">
          {{ formatCurrency(data.paymentTotal) }}
        </template>
      </Column>
      <Column field="createdAtUtc" header="Created" :sortable="true">
        <template #body="{ data }">
          {{ formatDateTimeUtc(data.createdAtUtc) }}
        </template>
      </Column>
      <!-- Section: Row Actions — view order detail -->
      <Column header="Actions" header-style="width:5rem">
        <template #body="{ data }">
          <Button icon="pi pi-eye" severity="secondary" text rounded @click="navigateToDetail(data.id)" />
        </template>
      </Column>
      <!-- Section: Empty State — shown when no orders match -->
      <template #empty>No orders found.</template>
    </DataTable>
  </div>
</template>
