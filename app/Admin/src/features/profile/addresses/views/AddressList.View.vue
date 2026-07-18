<script setup lang="ts">
import { onMounted } from 'vue'
import { useAddressStore } from '../stores/address.store'
import { storeToRefs } from 'pinia'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'

const store = useAddressStore()
const { items, loading } = storeToRefs(store)
const props = defineProps<{ userId: string }>()
onMounted(() => store.fetchAll(props.userId))
</script>

<template>
  <DataTable :value="items" :loading="loading" dataKey="id">
    <Column field="address1" header="Address" />
    <Column field="city" header="City" />
    <Column field="stateProvince" header="State" />
    <Column field="country" header="Country" />
    <Column field="isDefault" header="Default">
      <template #body="{ data }">
        <i :class="data.isDefault ? 'pi pi-check text-green-500' : 'pi pi-times text-red-500'" />
      </template>
    </Column>
  </DataTable>
</template>
