<script setup lang="ts">
import { ref } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { formatCurrency } from '@/shared/utils/currency'
import { usePaymentList } from '../composables/usePaymentList'
import type { PaymentListItem } from '../types/payment'
import { PAYMENT_SEARCH_FIELDS } from '../types/payment'

const { dt, exportCSV } = useDataTableExport()
const selectedItems = ref<PaymentListItem[]>([])
const search = ref('')

const { items, loading, setSearch, refresh } = usePaymentList({
  defaultSearchFields: PAYMENT_SEARCH_FIELDS,
})

function onSearch(value: string) {
  search.value = value
  setSearch(value)
}

function clearSearch() {
  search.value = ''
  setSearch('')
}
</script>

<template>
  <div class="flex flex-col h-full">
    <!-- Section: Page Header — title and one-line description -->
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Payments</h1>
      <p class="text-muted-color">System-managed payment records</p>
    </div>

    <!-- Section: Search & Filters — search box and list-level actions -->
    <div class="flex items-center gap-3 mb-4">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText
          :model-value="search"
          placeholder="Search payments..."
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

    <!-- Section: Data Table — read-only payment record grid -->
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
      <!-- Section: Table Columns — payment identity, amount, and state fields -->
      <Column field="id" header="Payment ID" />
      <Column field="amount" header="Amount" :sortable="true">
        <template #body="{ data }">
          {{ formatCurrency(data.amount, data.currency) }}
        </template>
      </Column>
      <Column field="orderId" header="Order ID" />
      <Column field="paymentMethodId" header="Method ID" />
      <Column field="state" header="State" :sortable="true">
        <template #body="{ data }">
          <Tag :value="data.state" />
        </template>
      </Column>
      <Column field="paymentStatus" header="Payment Status">
        <template #body="{ data }">
          {{ data.paymentStatus ?? '—' }}
        </template>
      </Column>
      <!-- Section: Empty State — shown when no payments match -->
      <template #empty>No payments found.</template>
    </DataTable>
  </div>
</template>
