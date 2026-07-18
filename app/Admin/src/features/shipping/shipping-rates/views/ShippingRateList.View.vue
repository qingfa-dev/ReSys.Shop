<script setup lang="ts">
import { onMounted } from 'vue'
import { useShippingRateStore } from '../stores/shipping-rate.store'
import { storeToRefs } from 'pinia'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'

const store = useShippingRateStore()
const { items, loading, totalRecords } = storeToRefs(store)

onMounted(() => store.fetchItems())
</script>

<template>
  <DataTable :value="items" :loading="loading" :totalRecords="totalRecords" dataKey="id">
    <Column field="shippingMethodName" header="Shipping Method" />
    <Column field="name" header="Name" />
    <Column field="rate" header="Rate">
      <template #body="{ data }">${{ data.rate.toFixed(2) }}</template>
    </Column>
    <Column field="fromWeight" header="Min Weight" />
    <Column field="toWeight" header="Max Weight" />
  </DataTable>
</template>
