<script setup lang="ts">
import { onMounted } from 'vue'
import { usePaymentStore } from '../stores/payment.store'
import { storeToRefs } from 'pinia'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'

const store = usePaymentStore()
const { items, loading, totalRecords } = storeToRefs(store)

onMounted(() => store.fetchItems())
</script>

<template>
  <DataTable :value="items" :loading="loading" :totalRecords="totalRecords" dataKey="id">
    <Column field="orderId" header="Order" />
    <Column field="amount" header="Amount">
      <template #body="{ data }">{{ data.amountDisplay }}</template>
    </Column>
    <Column field="status" header="Status">
      <template #body="{ data }">{{ data.statusLabel }}</template>
    </Column>
    <Column field="methodName" header="Method" />
    <Column field="createdAtUtc" header="Date" />
  </DataTable>
</template>
