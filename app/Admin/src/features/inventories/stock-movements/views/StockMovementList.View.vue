<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { movementService } from '../services/movement.service'
import { useFormatter } from '@/common/composables/formatter.use'
import { useI18n } from 'vue-i18n'
import PageShell from '@/shared/components/navigation/PageShell.vue'
import PageHeader from '@/shared/components/navigation/PageHeader.vue'
import DataTableShell from '@/shared/components/tables/DataTableShell.vue'
import type { DataTablePageEvent, DataTableSortEvent } from 'primevue/datatable'
import type { StockMovement } from '../types/stock-movement.response.type'
import type { ColumnDef } from '@/shared/components/tables/DataTableShell.vue'

const router = useRouter()
const { t } = useI18n()
const movements = ref<StockMovement[]>([])
const loading = ref(false)
const totalCount = ref(0)
const page = ref(1)
const pageSize = ref(10)
const sortField = ref<string>()
const sortOrder = ref<number>()
const { formatDate } = useFormatter()

const movementTypes: Record<number, string> = {
  1: 'Addition',
  2: 'Removal',
  3: 'Adjustment',
  4: 'Transfer',
}

const columns: ColumnDef[] = [
  { field: 'type', header: 'Type', body: (d) => movementTypes[d.type] ?? String(d.type) },
  { field: 'stockItemId', header: 'Stock Item', body: (d) => d.stockItemId },
  { field: 'quantity', header: 'Quantity', body: (d) => `${d.quantity > 0 ? '+' : ''}${d.quantity}` },
  { field: 'reference', header: 'Reference', body: (d) => d.reference ?? '-' },
  { field: 'reason', header: 'Reason', body: (d) => d.reason ?? '-' },
]

async function fetchMovements() {
  loading.value = true
  const result = await movementService.listMovements({
    page: page.value,
    pageSize: pageSize.value,
    sort: sortField.value ? [`${sortOrder.value === -1 ? '-' : ''}${sortField.value}`] : undefined,
  })
  if (result.isSuccess) {
    movements.value = result.items ?? []
    totalCount.value = result.totalCount ?? 0
  }
  loading.value = false
}

onMounted(() => fetchMovements())

function onPage(event: DataTablePageEvent) {
  page.value = event.page !== undefined ? event.page + 1 : 1
  pageSize.value = event.rows
  fetchMovements()
}

function onSort(event: DataTableSortEvent) {
  sortField.value = event.sortField as string | undefined ?? undefined
  sortOrder.value = event.sortOrder ?? undefined
  fetchMovements()
}

function onFilter() {
  fetchMovements()
}
</script>

<template>
  <PageShell maxWidth="7xl">
    <PageHeader :title="t('inventory.titles.stock_movement_history')" />

    <DataTableShell
      :columns="columns"
      :value="movements"
      :loading="loading"
      :totalRecords="totalCount"
      :rows="pageSize"
      dataKey="id"
      :showCreateButton="false"
      emptyIcon="pi pi-history"
      emptyTitle="No movements found"
      @page="onPage"
      @sort="onSort"
      @filter="onFilter"
      @refresh="fetchMovements"
    >
      <template #row-actions="{ data }">
        <Button
          icon="pi pi-eye"
          text
          rounded
          @click="router.push({ name: 'inventory.movements.detail', params: { id: data.id } })"
        />
      </template>
    </DataTableShell>
  </PageShell>
</template>
