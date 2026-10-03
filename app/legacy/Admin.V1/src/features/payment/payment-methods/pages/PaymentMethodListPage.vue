<script setup lang="ts">
import { onMounted } from 'vue'
import { usePaymentMethodStore } from '../store/payment-method.store'
import { storeToRefs } from 'pinia'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'

const store = usePaymentMethodStore()
const { items, loading, totalRecords } = storeToRefs(store)

onMounted(() => store.fetchItems())
</script>

<template>
  <DataTable :value="items" :loading="loading" :totalRecords="totalRecords" dataKey="id">
    <Column field="name" header="Name" />
    <Column field="provider" header="Provider" />
    <Column field="isActive" header="Active">
      <template #body="{ data }">
        <i :class="data.isActive ? 'pi pi-check text-green-500' : 'pi pi-times text-red-500'" />
      </template>
    </Column>
    <Column field="displayOrder" header="Order" />
  </DataTable>
</template>
