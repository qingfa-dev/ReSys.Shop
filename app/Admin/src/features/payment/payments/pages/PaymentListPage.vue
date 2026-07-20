<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { usePaymentStore } from '../store/payment.store'
import { storeToRefs } from 'pinia'
import type { DataTablePageEvent, DataTableSortEvent } from 'primevue/datatable'
import PageShell from '@/shared/components/navigation/PageShell.vue'
import PageHeader from '@/shared/components/navigation/PageHeader.vue'
import DataTableShell from '@/shared/components/tables/DataTableShell.vue'
import type { ColumnDef } from '@/shared/components/tables/DataTableShell.vue'

const { t } = useI18n()
const router = useRouter()
const store = usePaymentStore()
const { items, loading, totalRecords } = storeToRefs(store)

const columns: ColumnDef[] = [
  { field: 'id', header: t('payment.table.id') || 'ID', sortable: true },
  { field: 'orderId', header: t('payment.table.order') || 'Order', sortable: true },
  { field: 'amountDisplay', header: t('payment.table.amount') || 'Amount', sortable: true },
  { field: 'statusLabel', header: t('payment.table.status') || 'Status', sortable: true },
  { field: 'methodName', header: t('payment.table.method') || 'Method', sortable: true },
]

function onPage(event: DataTablePageEvent) {
  store.fetchItems({ page: event.page + 1, pageSize: event.rows })
}
function onSort(event: DataTableSortEvent) {
  store.fetchItems({ sort: event.sortField ? [`${event.sortOrder === -1 ? '-' : ''}${event.sortField}`] : undefined })
}
function onFilter() { store.fetchItems({}) }
function refresh() { store.fetchItems({}) }

onMounted(() => store.fetchItems({}))
</script>

<template>
  <PageShell max-width="7xl">
    <PageHeader :title="t('payment.titles.list') || 'Payments'" :description="t('payment.descriptions.list') || 'View and manage payments'">
      <template #badge>
        <Badge :value="totalRecords" severity="info" />
      </template>
    </PageHeader>
    <DataTableShell
      :columns="columns"
      :value="items"
      :loading="loading"
      :total-records="totalRecords"
      :search-placeholder="t('payment.placeholders.search') || 'Search payments...'"
      :empty-title="t('payment.messages.empty') || 'No payments found'"
      :show-create-button="false"
      @page="onPage"
      @sort="onSort"
      @filter="onFilter"
      @refresh="refresh"
    >
      <template #row-actions="{ data }">
        <Button icon="pi pi-eye" severity="info" text rounded @click="router.push({ name: 'payment.payments.detail', params: { id: data.id } })" />
      </template>
    </DataTableShell>
  </PageShell>
</template>
