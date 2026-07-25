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
import { usePaymentStore } from '../store/payment.store'
import { ROUTE } from '../routes'

const router = useRouter()
const { t } = useI18n()
const store = usePaymentStore()

onMounted(() => store.fetchMany())

function goToView(id: string) { router.push({ name: ROUTE.PAYMENTS.VIEW, params: { id } }) }

function onPageChange(e: { page: number; rows: number }) { store.setPage(e.page + 1) }
</script>

<template>
  <div>
    <TableToolbar
      v-model:query="store.searchQuery"
      :search-placeholder="t('payment.payments.table.search_placeholder')"
      :create-label="undefined"
    />
    <LoadingSkeleton v-if="store.loading && store.items.length === 0" :rows="5" :columns="6" />
    <ErrorState v-else-if="store.error" :description="store.error" @retry="store.fetchMany" />
    <EmptyState
      v-else-if="store.items.length === 0"
      :title="t('payment.payments.messages.empty_list')"
      :description="t('payment.payments.messages.empty_description')"
    />
    <DataTable
      v-else
      :rows="[...store.items]"
      :loading="store.loading"
      :total-records="store.totalRecords"
      :page-size="store.query.pageSize"
      :first="(store.query.page - 1) * store.query.pageSize"
      @page="onPageChange"
    >
      <Column :header="t('payment.payments.table.order')" field="orderNumber" sortable />
      <Column :header="t('payment.payments.table.method')" field="paymentMethodName" />
      <Column :header="t('payment.payments.table.amount')" field="amount">
        <template #body="{ data }">
          {{ data.currency }} {{ data.amount?.toFixed(2) }}
        </template>
      </Column>
      <Column :header="t('payment.payments.table.status')" field="status">
        <template #body="{ data }">
          <StatusTag :status="data.status" />
        </template>
      </Column>
      <Column :header="t('payment.payments.table.date')" field="createdAt" sortable />
      <template #rowActions="{ data }">
        <ActionMenu
          :items="[
            { label: t('payment.payments.table.view'), icon: 'pi pi-eye', command: () => goToView(data.id) },
          ]"
        />
      </template>
    </DataTable>
  </div>
</template>
