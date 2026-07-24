<script setup lang="ts">
import { onMounted, ref } from 'vue'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import DataTable from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useI18n } from 'vue-i18n'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { StockReservationApi } from '../api'

const { confirmDelete } = useConfirm()
const toast = useToast()
const { t } = useI18n()

const items = ref<import('../types').StockReservationResponse[]>([])
const loading = ref(false)
const error = ref<string | null>(null)
const totalRecords = ref(0)

async function fetchMany() {
  loading.value = true
  error.value = null
  try {
    const result = await StockReservationApi.getMany({ page: 1, pageSize: 20 })
    if (result.isSuccess) {
      items.value = result.items ?? []
      totalRecords.value = result.totalCount ?? 0
    } else {
      error.value = result.message ?? 'Failed to load'
      items.value = []
      totalRecords.value = 0
    }
  } catch {
    error.value = 'Failed to load'
    items.value = []
    totalRecords.value = 0
  }
  loading.value = false
}

onMounted(() => fetchMany())

async function onCancel(id: string) {
  confirmDelete({
    target: 'this reservation',
    onAccept: async () => {
      const result = await StockReservationApi.cancel(id)
      if (result.isSuccess) { toast.success('Reservation cancelled'); await fetchMany() }
      else { toast.error(result.message ?? 'Failed to cancel') }
    },
  })
}
</script>

<template>
  <div>
    <TableToolbar search-placeholder="Search reservations..." @search="() => {}" />
    <LoadingSkeleton v-if="loading && items.length === 0" :rows="5" :columns="5" />
    <ErrorState v-else-if="error" :description="error" @retry="fetchMany" />
    <EmptyState v-else-if="items.length === 0" title="No reservations found" />
    <DataTable
      v-else
      :rows="[...items]"
      :loading="loading"
      :total-records="totalRecords"
      @page="() => {}"
    >
      <Column field="orderNumber" header="Order" sortable />
      <Column field="variantSku" header="SKU" sortable />
      <Column field="quantity" header="Quantity" sortable />
      <Column field="status" header="Status">
        <template #body="{ data }">
          <Tag :severity="data.status === 'Active' ? 'info' : data.status === 'Released' ? 'success' : 'warn'" :value="data.status" />
        </template>
      </Column>
      <Column field="expiresAt" header="Expires" />
      <Column field="createdAt" header="Created" sortable />
      <template #rowActions="{ data }">
        <ActionMenu
          v-if="data.status === 'Active'"
          :items="[
            { label: 'Cancel', icon: 'pi pi-times', command: () => onCancel(data.id) },
          ]"
        />
      </template>
    </DataTable>
  </div>
</template>
