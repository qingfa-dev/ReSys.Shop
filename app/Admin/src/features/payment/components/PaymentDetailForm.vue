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
import { AppCard } from '@/shared/components'
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
  ? `${t('payment.payments.detail.actions.back')} #${payment.value.orderNumber}`
  : t('payment.payments.title'))

async function loadPayment() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  try {
    const result = await api.get(id.value)
    if (result.isSuccess) {
      payment.value = result.value
    } else {
      loadError.value = result.message ?? t('payment.payments.detail.messages.load_failed')
    }
  } catch (err) {
    console.error(err)
    loadError.value = t('payment.payments.detail.messages.load_failed')
  }
  loading.value = false
}

const canCapture = computed(() => payment.value?.status === 'pending' || payment.value?.status === 'authorized')
const canVoid = computed(() => payment.value?.status === 'pending' || payment.value?.status === 'authorized')
const canRefund = computed(() => payment.value?.status === 'captured')

async function onCapture() {
  if (!id.value) return
  actionLoading.value = true
  try {
    const result = await api.capture(id.value)
    actionLoading.value = false
    if (result.isSuccess) {
      toast.success(t('payment.payments.detail.messages.capture_success'))
      await loadPayment()
    } else {
      toast.error(result.message ?? t('payment.payments.detail.messages.capture_failed'))
    }
  } catch (err) {
    console.error(err)
    actionLoading.value = false
    toast.error(t('payment.payments.detail.messages.capture_failed'))
  }
}

async function onVoid() {
  if (!id.value) return
  actionLoading.value = true
  try {
    const result = await api.void(id.value)
    actionLoading.value = false
    if (result.isSuccess) {
      toast.success(t('payment.payments.detail.messages.void_success'))
      await loadPayment()
    } else {
      toast.error(result.message ?? t('payment.payments.detail.messages.void_failed'))
    }
  } catch (err) {
    console.error(err)
    actionLoading.value = false
    toast.error(t('payment.payments.detail.messages.void_failed'))
  }
}

async function onRefund() {
  if (!id.value) return
  actionLoading.value = true
  try {
    const result = await api.refund(id.value)
    actionLoading.value = false
    if (result.isSuccess) {
      toast.success(t('payment.payments.detail.messages.refund_success'))
      await loadPayment()
    } else {
      toast.error(result.message ?? t('payment.payments.detail.messages.refund_failed'))
    }
  } catch (err) {
    console.error(err)
    actionLoading.value = false
    toast.error(t('payment.payments.detail.messages.refund_failed'))
  }
}

function goBack() { router.push({ name: ROUTE.PAYMENTS.LIST }) }

onMounted(async () => {
  await loadPayment()
})
</script>

<template>
  <div>
    <PageHeader :title="title" :subtitle="t('payment.payments.detail.subtitle')" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <Button
          :label="t('payment.payments.detail.actions.back')"
          icon="pi pi-arrow-left"
          severity="secondary"
          size="small"
          @click="goBack"
        />
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading" :rows="8" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadPayment" />
    <AppCard v-else-if="payment">
      <div class="mb-4 flex items-center gap-4">
        <StatusTag :status="payment.status" />
        <span v-if="payment.authorizationCode" class="text-sm text-surface-500">{{ t('payment.payments.detail.labels.authorization_code') }}: {{ payment.authorizationCode }}</span>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('payment.payments.detail.labels.order')">
            <input :value="payment.orderNumber ?? '-'" type="text" class="p-inputtext p-component w-full" disabled />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('payment.payments.detail.labels.payment_method')">
            <input :value="payment.paymentMethodName ?? '-'" type="text" class="p-inputtext p-component w-full" disabled />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('payment.payments.detail.labels.amount')">
            <input :value="`${payment.currency} ${payment.amount?.toFixed(2)}`" type="text" class="p-inputtext p-component w-full" disabled />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('payment.payments.detail.labels.currency')">
            <input :value="payment.currency" type="text" class="p-inputtext p-component w-full" disabled />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('payment.payments.detail.labels.captured_at')">
            <input :value="payment.capturedAt ?? '-'" type="text" class="p-inputtext p-component w-full" disabled />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('payment.payments.detail.labels.voided_at')">
            <input :value="payment.voidedAt ?? '-'" type="text" class="p-inputtext p-component w-full" disabled />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('payment.payments.detail.labels.refunded_at')">
            <input :value="payment.refundedAt ?? '-'" type="text" class="p-inputtext p-component w-full" disabled />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full">
          <FormField :label="t('payment.payments.detail.labels.notes')">
            <textarea :value="payment.notes ?? '-'" class="p-inputtext p-component w-full" disabled rows="3" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('payment.payments.detail.labels.created_at')">
            <input :value="payment.createdAt" type="text" class="p-inputtext p-component w-full" disabled />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('payment.payments.detail.labels.updated_at')">
            <input :value="payment.updatedAt" type="text" class="p-inputtext p-component w-full" disabled />
          </FormField>
        </div>
      </div>

      <div v-if="canCapture || canVoid || canRefund" class="mt-6 flex items-center gap-3">
        <Button v-if="canCapture" :label="t('payment.payments.detail.actions.capture')" icon="pi pi-credit-card" :loading="actionLoading" severity="success" @click="onCapture" />
        <Button v-if="canVoid" :label="t('payment.payments.detail.actions.void')" icon="pi pi-ban" :loading="actionLoading" severity="warn" @click="onVoid" />
        <Button v-if="canRefund" :label="t('payment.payments.detail.actions.refund')" icon="pi pi-money-bill" :loading="actionLoading" severity="info" @click="onRefund" />
      </div>
    </AppCard>
  </div>
</template>
