<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { usePaymentStore } from '../store/payment.store'
import { storeToRefs } from 'pinia'
import { useToast } from '@/common/composables/toast.use'
import { useFormatter } from '@/common/composables/formatter.use'
import { PaymentStateMap } from '@/shared/utils/enums'
import PageShell from '@/shared/components/navigation/PageShell.vue'
import PageHeader from '@/shared/components/navigation/PageHeader.vue'
import DetailField from '@/shared/components/data-display/DetailField.vue'
import StatusBadge from '@/shared/components/feedback/StatusBadge.vue'
import ConfirmDialog from '@/shared/components/overlays/ConfirmDialog.vue'
import CompactTable from '@/shared/components/tables/CompactTable.vue'
import type { ColumnDef } from '@/shared/components/tables/DataTableShell.vue'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const store = usePaymentStore()
const { showToast } = useToast()
const { formatDate } = useFormatter()
const { current, loading } = storeToRefs(store)

const paymentId = route.params.id as string

const transactionColumns: ColumnDef[] = [
  { field: 'type', header: t('payment.table.type') || 'Type' },
  { field: 'amount', header: t('payment.table.amount') || 'Amount' },
  { field: 'status', header: t('payment.table.status') || 'Status' },
  { field: 'gatewayTransactionId', header: t('payment.table.gateway_id') || 'Gateway ID' },
  { field: 'createdAtUtc', header: t('payment.table.date') || 'Date', body: (data: any) => formatDate(data.createdAtUtc) },
]

onMounted(async () => {
  const result = await store.fetchById(paymentId)
  if (!result.isSuccess) {
    showToast('error', 'Error', result.message || 'Failed to load payment')
    router.push({ name: 'payment.payments.list' })
  }
})

const paymentStatusMap = computed(() => ({
  0: { label: 'Pending', severity: 'warn' as const },
  1: { label: 'Completed', severity: 'success' as const },
  2: { label: 'Failed', severity: 'danger' as const },
  4: { label: 'Refunded', severity: 'info' as const },
  8: { label: 'Voided', severity: 'secondary' as const },
}))

async function onCapture() {
  const result = await store.capture(current.value!.id)
  if (result.isSuccess) {
    showToast('success', t('common.success') || 'Success', t('payment.messages.captured') || 'Payment captured')
    store.fetchById(paymentId)
  } else {
    showToast('error', t('common.error') || 'Error', result.message || 'Capture failed')
  }
}

async function onVoid() {
  const result = await store.void(current.value!.id)
  if (result.isSuccess) {
    showToast('success', t('common.success') || 'Success', t('payment.messages.voided') || 'Payment voided')
    store.fetchById(paymentId)
  } else {
    showToast('error', t('common.error') || 'Error', result.message || 'Void failed')
  }
}

async function onRefund() {
  const result = await store.refund(current.value!.id, current.value!.amount)
  if (result.isSuccess) {
    showToast('success', t('common.success') || 'Success', t('payment.messages.refunded') || 'Payment refunded')
    store.fetchById(paymentId)
  } else {
    showToast('error', t('common.error') || 'Error', result.message || 'Refund failed')
  }
}
</script>

<template>
  <PageShell :card="false" gap max-width="7xl">
    <template v-if="current">
      <PageHeader
        back
        :title="t('payment.titles.detail') || 'Payment Detail'"
        :description="`#${current.id}`"
      >
        <template #badge>
          <StatusBadge :status="current.status" :status-map="paymentStatusMap" />
        </template>
        <template #actions>
          <ConfirmDialog
            :header="t('payment.titles.capture') || 'Capture Payment'"
            :message="t('payment.messages.confirm_capture') || 'Are you sure you want to capture this payment?'"
            icon="pi pi-check-circle"
            severity="success"
            @confirm="onCapture"
            v-if="current.status === 0"
          >
            {{ t('payment.actions.capture') || 'Capture' }}
          </ConfirmDialog>
          <ConfirmDialog
            :header="t('payment.titles.void') || 'Void Payment'"
            :message="t('payment.messages.confirm_void') || 'Are you sure you want to void this payment?'"
            icon="pi pi-times-circle"
            severity="warn"
            @confirm="onVoid"
            v-if="current.status === 0"
          >
            {{ t('payment.actions.void') || 'Void' }}
          </ConfirmDialog>
          <ConfirmDialog
            :header="t('payment.titles.refund') || 'Refund Payment'"
            :message="t('payment.messages.confirm_refund') || 'Are you sure you want to refund this payment?'"
            icon="pi pi-undo"
            severity="danger"
            @confirm="onRefund"
            v-if="current.status === 1"
          >
            {{ t('payment.actions.refund') || 'Refund' }}
          </ConfirmDialog>
        </template>
      </PageHeader>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <!-- Payment Info -->
        <div class="lg:col-span-2 flex flex-col gap-6">
          <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900 overflow-hidden">
            <template #title>
              <span class="text-xl font-black uppercase tracking-tight p-4 block">
                {{ t('payment.titles.payment_info') || 'Payment Information' }}
              </span>
            </template>
            <template #content>
              <div class="grid grid-cols-1 md:grid-cols-2 gap-8 p-6">
                <DetailField :label="t('payment.table.id') || 'ID'" :value="current.id" />
                <DetailField :label="t('payment.table.order') || 'Order'" :value="current.orderId" />
                <DetailField :label="t('payment.table.amount') || 'Amount'" :value="current.amountDisplay" />
                <DetailField :label="t('payment.table.method') || 'Method'" :value="current.methodName" />
                <DetailField :label="t('payment.labels.currency') || 'Currency'" :value="current.currency" />
                <DetailField :label="t('payment.labels.created_at') || 'Created At'" :value="formatDate(current.createdAtUtc)" />
              </div>
            </template>
          </Card>

          <!-- Transactions -->
          <Card v-if="current.transactions?.length" class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900 overflow-hidden">
            <template #title>
              <span class="text-xl font-black uppercase tracking-tight p-4 block">
                {{ t('payment.titles.transactions') || 'Transactions' }}
              </span>
            </template>
            <template #content>
              <CompactTable :value="current.transactions" :columns="transactionColumns" stripedRows />
            </template>
          </Card>
        </div>

        <!-- Gateway Response -->
        <div class="flex flex-col gap-6">
          <Card v-if="current.gatewayResponse" class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900 overflow-hidden">
            <template #title>
              <span class="text-sm font-black uppercase tracking-widest text-surface-400 p-4 block">
                {{ t('payment.titles.gateway_response') || 'Gateway Response' }}
              </span>
            </template>
            <template #content>
              <pre class="text-xs font-mono whitespace-pre-wrap break-all p-4 m-0 bg-surface-50 dark:bg-surface-800 rounded-xl text-surface-700 dark:text-surface-300">{{ JSON.stringify(current.gatewayResponse, null, 2) }}</pre>
            </template>
          </Card>
        </div>
      </div>
    </template>

    <div v-else-if="loading" class="flex justify-center py-20">
      <ProgressSpinner />
    </div>
  </PageShell>
</template>

<style scoped>
:deep(.p-card-body) {
  padding: 0;
}
:deep(.p-card-content) {
  padding: 0;
}
</style>
