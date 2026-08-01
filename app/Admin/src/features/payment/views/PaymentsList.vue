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
import { PAYMENT_SEARCH_FIELDS } from '../types/payment'

const { dt, exportCSV } = useDataTableExport()
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
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Payments</h1>
      <p class="text-muted-color">System-managed payment records</p>
    </div>

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
      <template #empty>No payments found.</template>
    </DataTable>
  </div>
</template>
