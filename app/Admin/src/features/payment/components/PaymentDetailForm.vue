<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import Button from 'primevue/button'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import StatusTag from '@/shared/components/data/StatusTag.vue'
import FormField from '@/shared/components/forms/FormField.vue'
import { usePayment } from '../composables/usePayment'
import { ROUTE } from '../routes'
import type { PaymentResponse } from '../types'

const { id, route, router, toast, api } = usePayment()
const { t } = useI18n()
const { confirmDelete } = useConfirm()

const payment = ref<PaymentResponse | null>(null)
const loading = ref(false)
const loadError = ref<string | null>(null)
const actionLoading = ref(false)

const title = computed(() => payment.value?.orderNumber
  ? `Payment #${payment.value.orderNumber}`
  : 'Payment Detail')

async function loadPayment() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  const result = await api.get(id.value)
  if (result.isSuccess) {
    payment.value = result.value
  } else {
    loadError.value = result.message ?? 'Failed to load payment'
  }
  loading.value = false
}

const canCapture = computed(() => payment.value?.status === 'pending' || payment.value?.status === 'authorized')
const canVoid = computed(() => payment.value?.status === 'pending' || payment.value?.status === 'authorized')
const canRefund = computed(() => payment.value?.status === 'captured')

async function onCapture() {
  if (!id.value) return
  actionLoading.value = true
  const result = await api.capture(id.value)
  actionLoading.value = false
  if (result.isSuccess) {
    toast.success('Payment captured successfully')
    await loadPayment()
  } else {
    toast.error(result.message ?? 'Capture failed')
  }
}

async function onVoid() {
  if (!id.value) return
  actionLoading.value = true
  const result = await api.void(id.value)
  actionLoading.value = false
  if (result.isSuccess) {
    toast.success('Payment voided successfully')
    await loadPayment()
  } else {
    toast.error(result.message ?? 'Void failed')
  }
}

async function onRefund() {
  if (!id.value) return
  actionLoading.value = true
  const result = await api.refund(id.value)
  actionLoading.value = false
  if (result.isSuccess) {
    toast.success('Payment refunded successfully')
    await loadPayment()
  } else {
    toast.error(result.message ?? 'Refund failed')
  }
}

function goBack() { router.push({ name: ROUTE.PAYMENTS.LIST }) }

onMounted(loadPayment)
</script>

<template>
  <div>
    <PageHeader :title="title" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <button class="p-button p-component p-button-outlined" @click="goBack">Back</button>
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading" :rows="8" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadPayment" />
    <div v-else-if="payment" class="card">
      <div class="mb-4 flex items-center gap-4">
        <StatusTag :status="payment.status" />
        <span v-if="payment.authorizationCode" class="text-sm text-surface-500">Auth: {{ payment.authorizationCode }}</span>
      </div>
      <div class="grid">
        <div class="col-6">
          <FormField label="Order">
            <input :value="payment.orderNumber ?? '-'" type="text" class="p-inputtext p-component w-full" disabled />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Payment Method">
            <input :value="payment.paymentMethodName ?? '-'" type="text" class="p-inputtext p-component w-full" disabled />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-6">
          <FormField label="Amount">
            <input :value="`${payment.currency} ${payment.amount?.toFixed(2)}`" type="text" class="p-inputtext p-component w-full" disabled />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Currency">
            <input :value="payment.currency" type="text" class="p-inputtext p-component w-full" disabled />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-4">
          <FormField label="Captured At">
            <input :value="payment.capturedAt ?? '-'" type="text" class="p-inputtext p-component w-full" disabled />
          </FormField>
        </div>
        <div class="col-4">
          <FormField label="Voided At">
            <input :value="payment.voidedAt ?? '-'" type="text" class="p-inputtext p-component w-full" disabled />
          </FormField>
        </div>
        <div class="col-4">
          <FormField label="Refunded At">
            <input :value="payment.refundedAt ?? '-'" type="text" class="p-inputtext p-component w-full" disabled />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-12">
          <FormField label="Notes">
            <textarea :value="payment.notes ?? '-'" class="p-inputtext p-component w-full" disabled rows="3" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-6">
          <FormField label="Created At">
            <input :value="payment.createdAt" type="text" class="p-inputtext p-component w-full" disabled />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Updated At">
            <input :value="payment.updatedAt" type="text" class="p-inputtext p-component w-full" disabled />
          </FormField>
        </div>
      </div>

      <div v-if="canCapture || canVoid || canRefund" class="mt-6 flex items-center gap-3">
        <Button v-if="canCapture" label="Capture" icon="pi pi-credit-card" :loading="actionLoading" severity="success" @click="onCapture" />
        <Button v-if="canVoid" label="Void" icon="pi pi-ban" :loading="actionLoading" severity="warn" @click="onVoid" />
        <Button v-if="canRefund" label="Refund" icon="pi pi-money-bill" :loading="actionLoading" severity="info" @click="onRefund" />
      </div>
    </div>
  </div>
</template>

