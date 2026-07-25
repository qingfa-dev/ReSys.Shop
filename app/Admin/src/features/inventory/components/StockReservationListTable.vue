<script setup lang="ts">
import { onMounted, computed } from 'vue'
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
import { useStockReservationStore } from '../store/stock-reservation.store'
import { StockReservationApi } from '../api'

const { confirmDelete } = useConfirm()
const toast = useToast()
const { t } = useI18n()
const store = useStockReservationStore()

onMounted(() => store.fetchMany())

async function onCancel(id: string) {
  confirmDelete({
    target: 'this reservation',
    onAccept: async () => {
      const result = await StockReservationApi.cancel(id)
      if (result.isSuccess) { toast.success('Reservation cancelled'); await store.fetchMany() }
      else { toast.error(result.message ?? 'Failed to cancel') }
    },
  })
}

function onPageChange(e: { page: number; rows: number }) { store.setPage(e.page + 1) }
</script>

<template>
  <div>
    <TableToolbar v-model:query="store.searchQuery" search-placeholder="Search reservations..." />
    <LoadingSkeleton v-if="store.loading && store.items.length === 0" :rows="5" :columns="5" />
    <ErrorState v-else-if="store.error" :description="store.error" @retry="store.fetchMany" />
    <EmptyState v-else-if="store.items.length === 0" title="No reservations found" />
    <DataTable
      v-else
      :rows="[...store.items]"
      :loading="store.loading"
      :total-records="store.totalRecords"
      :page-size="store.query.pageSize"
      :first="(store.query.page - 1) * store.query.pageSize"
      @page="onPageChange"
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
