<script setup lang="ts">
import { ref } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import { useConfirm } from 'primevue/useconfirm'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { formatCurrency } from '@/shared/utils/currency'
import { usePaymentList } from '../composables/usePaymentList'
import { PaymentApi } from '../services/paymentApi'
import { PAYMENT_STATE_SEVERITY, PAYMENT_SEARCH_FIELDS } from '../types/payment'
import type { PaymentListItem, PaymentRecordState } from '../types/payment'
import type { Result } from '@/shared/types'

const { dt, exportCSV } = useDataTableExport()
const selectedItems = ref<PaymentListItem[]>([])
const search = ref('')
const confirm = useConfirm()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()
const paymentActionId = ref<string | null>(null)

const { items, loading, setSearch, refresh } = usePaymentList({
  defaultSearchFields: PAYMENT_SEARCH_FIELDS,
})

function onSearch(value: string) {
  search.value = value
  setSearch(value)
}

function clearSearch() {
  search.value = ''
  setSearch('')
}

// Gate: Capture applies only to payments awaiting settlement.
function canCapturePayment(state: PaymentRecordState): boolean {
  return state === 'Pending' || state === 'Processing'
}

// Gate: Refund applies only to completed payments.
function canRefundPayment(state: PaymentRecordState): boolean {
  return state === 'Completed'
}

// Gate: Void applies only to payments that have not completed.
function canVoidPayment(state: PaymentRecordState): boolean {
  return state === 'Pending' || state === 'Processing'
}

// Trigger: Confirm before running a payment action on the row, then reload the list.
function confirmPaymentAction<T>(payment: PaymentListItem, label: string, message: string, run: () => Promise<Result<T>>) {
  confirm.require({
    message,
    header: `Confirm ${label}`,
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: label,
    accept: async () => {
      paymentActionId.value = payment.id
      const result = await run()
      paymentActionId.value = null
      if (result.isSuccess) {
        notify.success('Payment', `Payment ${label.toLowerCase()}d.`)
        await refresh()
      } else {
        handleResult(result)
      }
    },
  })
}

function capturePayment(payment: PaymentListItem) {
  confirmPaymentAction(payment, 'Capture', 'Capture this payment?', () => PaymentApi.capturePayment(payment.id))
}

function refundPayment(payment: PaymentListItem) {
  const amount = formatCurrency(payment.amount, payment.currency)
  confirmPaymentAction(payment, 'Refund', `Refund ${amount} for this payment?`, () =>
    PaymentApi.refundPayment(payment.id, { amount: payment.amount }),
  )
}

function voidPayment(payment: PaymentListItem) {
  confirmPaymentAction(payment, 'Void', 'Void this payment?', () => PaymentApi.voidPayment(payment.id))
}
</script>

<template>
  <div class="flex flex-col h-full">
    <!-- Section: Page Header — title and one-line description -->
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Payments</h1>
      <p class="text-muted-color">System-managed payment records</p>
    </div>

    <!-- Section: Search & Filters — search box and list-level actions -->
    <div class="flex items-center gap-3 mb-4">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText
          :model-value="search"
          placeholder="Search payments..."
          @update:model-value="onSearch($event ?? '')"
        />
      </IconField>
      <Button
        v-if="search"
        label="Clear"
        severity="secondary"
        icon="pi pi-times"
        @click="clearSearch"
      />
      <div class="flex-1" />
      <Button
        label="Reload"
        icon="pi pi-refresh"
        severity="secondary"
        @click="refresh"
      />
      <Button
        label="Export"
        icon="pi pi-download"
        severity="secondary"
        @click="exportCSV"
      />
    </div>

    <!-- Section: Data Table — read-only payment record grid -->
    <DataTable
      ref="dt"
      v-model:selection="selectedItems"
      :value="items"
      :loading="loading"
      scrollable
      paginator
      :rows="20"
      :rows-per-page-options="[10, 20, 50]"
      data-key="id"
    >
      <Column selection-mode="multiple" header-style="width: 3rem" />
      <!-- Section: Table Columns — payment identity, amount, and state fields -->
      <Column field="id" header="Payment ID" />
      <Column field="amount" header="Amount" :sortable="true">
        <template #body="{ data }">
          {{ formatCurrency(data.amount, data.currency) }}
        </template>
      </Column>
      <Column field="orderId" header="Order ID" />
      <Column field="paymentMethodId" header="Method ID" />
      <Column field="state" header="State" :sortable="true">
        <template #body="{ data }">
          <Tag :value="data.state" :severity="PAYMENT_STATE_SEVERITY[data.state as PaymentRecordState]" />
        </template>
      </Column>
      <Column field="paymentStatus" header="Payment Status">
        <template #body="{ data }">
          {{ data.paymentStatus ?? '—' }}
        </template>
      </Column>
      <!-- Section: Row Actions — per-row capture, refund, and void controls -->
      <Column header="Actions">
        <template #body="{ data }">
          <div class="flex items-center gap-2">
            <Button
              v-if="canCapturePayment(data.state)"
              icon="pi pi-check"
              label="Capture"
              size="small"
              severity="primary"
              :loading="paymentActionId === data.id"
              @click="capturePayment(data)"
            />
            <Button
              v-if="canRefundPayment(data.state)"
              icon="pi pi-refresh"
              label="Refund"
              size="small"
              severity="secondary"
              :loading="paymentActionId === data.id"
              @click="refundPayment(data)"
            />
            <Button
              v-if="canVoidPayment(data.state)"
              icon="pi pi-times"
              label="Void"
              size="small"
              severity="danger"
              :loading="paymentActionId === data.id"
              @click="voidPayment(data)"
            />
          </div>
        </template>
      </Column>
      <!-- Section: Empty State — shown when no payments match -->
      <template #empty>No payments found.</template>
    </DataTable>
  </div>
</template>
