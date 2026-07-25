<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Column from 'primevue/column'
import DataTable from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import StatusTag from '@/shared/components/data/StatusTag.vue'
import { useI18n } from 'vue-i18n'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { useOrderStore } from '../store/order.store'
import { OrderApi } from '../api'
import { ROUTE } from '../routes'

const router = useRouter()
const { confirmDelete } = useConfirm()
const toast = useToast()
const { t } = useI18n()
const store = useOrderStore()

onMounted(() => store.fetchMany())

function goToCreate() { router.push({ name: ROUTE.ORDERS.CREATE }) }
function goToView(id: string) { router.push({ name: ROUTE.ORDERS.VIEW, params: { id } }) }
function goToEdit(id: string) { router.push({ name: ROUTE.ORDERS.EDIT, params: { id } }) }

async function onDelete(id: string) {
  confirmDelete({
    target: `order #${id}`,
    onAccept: async () => {
      const result = await OrderApi.delete(id)
      if (result.isSuccess) { toast.success(t('ordering.orders.messages.delete_success')); await store.fetchMany() }
      else { toast.error(result.message ?? t('ordering.orders.messages.delete_failed')) }
    },
  })
}

function onSearch(value: string) { store.setSearch(value) }
function onPageChange(e: { page: number; rows: number }) { store.setPage(e.page + 1) }

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount)
}
</script>

<template>
  <div>
    <TableToolbar
      :search-placeholder="t('ordering.orders.table.search_placeholder')"
      :create-label="t('ordering.orders.actions.create')"
      @search="onSearch"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="store.loading && store.items.length === 0" :rows="5" :columns="6" />
    <ErrorState v-else-if="store.error" :description="store.error" @retry="store.fetchMany" />
    <EmptyState v-else-if="store.items.length === 0" :title="t('ordering.orders.messages.empty_list')" :description="t('ordering.orders.messages.empty_description')" />
    <DataTable
      v-else
      :rows="[...store.items]"
      :loading="store.loading"
      :total-records="store.totalRecords"
      :page-size="store.query.pageSize"
      :first="(store.query.page - 1) * store.query.pageSize"
      @page="onPageChange"
    >
      <Column field="orderNumber" :header="t('ordering.orders.table.order_number')" sortable />
      <Column field="customerName" :header="t('ordering.orders.table.customer')" />
      <Column field="status" :header="t('ordering.orders.table.status')">
        <template #body="{ data }">
          <StatusTag :status="data.status" />
        </template>
      </Column>
      <Column field="total" :header="t('ordering.orders.table.total')">
        <template #body="{ data }">
          {{ formatCurrency(data.total) }}
        </template>
      </Column>
      <Column field="createdAt" :header="t('ordering.orders.table.date')" sortable />
      <template #rowActions="{ data }">
        <ActionMenu
          :items="[
            { label: t('ordering.orders.table.view'), icon: 'pi pi-eye', command: () => goToView(data.id) },
            { label: t('ordering.orders.table.edit'), icon: 'pi pi-pencil', command: () => goToEdit(data.id) },
            { label: t('ordering.orders.table.delete'), icon: 'pi pi-trash', command: () => onDelete(data.id) },
          ]"
        />
      </template>
    </DataTable>
  </div>
</template>
