<script setup lang="ts">
import { onMounted } from 'vue'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import DataTable from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useI18n } from 'vue-i18n'
import { useStockMovementStore } from '../store/stock-movement.store'

const { t } = useI18n()
const store = useStockMovementStore()

onMounted(() => store.fetchMany())

function onPageChange(e: { page: number; rows: number }) { store.setPage(e.page + 1) }
</script>

<template>
  <div>
    <TableToolbar v-model:query="store.searchQuery" search-placeholder="Search movements..." />
    <LoadingSkeleton v-if="store.loading && store.items.length === 0" :rows="5" :columns="6" />
    <ErrorState v-else-if="store.error" :description="store.error" @retry="store.fetchMany" />
    <EmptyState v-else-if="store.items.length === 0" title="No movements found" />
    <DataTable
      v-else
      :rows="[...store.items]"
      :loading="store.loading"
      :total-records="store.totalRecords"
      :page-size="store.query.pageSize"
      :first="(store.query.page - 1) * store.query.pageSize"
      @page="onPageChange"
    >
      <Column field="variantSku" header="SKU" sortable />
      <Column field="locationName" header="Location" />
      <Column field="quantity" header="Quantity" sortable />
      <Column field="direction" header="Direction">
        <template #body="{ data }">
          <Tag :severity="data.direction === 'In' ? 'success' : 'danger'" :value="data.direction" />
        </template>
      </Column>
      <Column field="reason" header="Reason" />
      <Column field="reference" header="Reference" />
      <Column field="createdAt" header="Date" sortable />
    </DataTable>
  </div>
</template>
