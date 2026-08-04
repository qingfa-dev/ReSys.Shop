<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import FormField from '@/shared/components/forms/FormField.vue'
import FormActions from '@/shared/components/forms/FormActions.vue'
import { LoadingSkeleton, ErrorState, AppCard } from '@/shared/components'
import StatusTag from '@/shared/components/data/StatusTag.vue'
import Button from 'primevue/button'
import OrderLineItemManager from './OrderLineItemManager.vue'
import { useOrder } from '../composables/useOrder'
import { OrderForms } from '../schemas'
import { OrderFormMapper } from '../mappers/order.mapper'
import { OrderApi } from '../api'
import { ROUTE } from '../routes'
import type { OrderResponse, CreateOrderRequest } from '../types'

const { id, mode, route, router, toast } = useOrder()
const { t } = useI18n()

const schemas = new OrderForms(t)
const { handleSubmit, defineField, errors, setValues } = useForm({
  validationSchema: toTypedSchema(
    mode.value === 'create' ? schemas.create() : schemas.update(),
  ),
})

const [customerId] = defineField('customerId')
const [notes] = defineField('notes')

const order = ref<OrderResponse | null>(null)
const loading = ref(false)
const saving = ref(false)
const loadError = ref<string | null>(null)

const title = computed(() => {
  if (mode.value === 'create') return t('ordering.orders.titles.create')
  if (mode.value === 'edit') return `${t('ordering.orders.actions.edit')}: ${order.value?.orderNumber || ''}`
  return order.value?.orderNumber || t('ordering.orders.titles.view')
})

const subtitle = computed(() => {
  return t('ordering.orders.descriptions.general')
})

const canApprove = computed(() => order.value?.status === 'pending')
const canComplete = computed(() => order.value?.status === 'approved' || order.value?.status === 'processing')
const canCancel = computed(() => order.value?.status !== 'completed' && order.value?.status !== 'cancelled')
const canResume = computed(() => order.value?.status === 'cancelled' || order.value?.status === 'on_hold')

async function loadOrder() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  try {
    const result = await OrderApi.get(id.value)
    if (result.isSuccess) {
      order.value = result.value
      setValues({
        customerId: result.value.customerId,
        notes: result.value.notes ?? undefined,
      })
    } else {
      loadError.value = result.message ?? t('ordering.orders.messages.load_failed')
    }
  } catch (err) {
    console.error(err)
    loadError.value = t('ordering.orders.messages.load_failed')
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = mode.value === 'create'
    ? OrderFormMapper.toCreate(values)
    : OrderFormMapper.toUpdate(values)
  const result = id.value
    ? await OrderApi.update(id.value, data)
    : await OrderApi.create(data as CreateOrderRequest)
  saving.value = false
  if (result.isSuccess) {
    toast.success(id.value ? t('ordering.orders.messages.update_success') : t('ordering.orders.messages.create_success'))
    const newId = result.value.id
    router.replace({ name: ROUTE.ORDERS.VIEW, params: { id: newId } })
  } else {
    toast.error(result.message ?? t('ordering.orders.messages.save_failed'))
  }
})

async function lifecycleAction(action: 'approve' | 'complete' | 'cancel' | 'resume') {
  if (!id.value) return
  saving.value = true
  let result
  switch (action) {
    case 'approve': result = await OrderApi.approve(id.value); break
    case 'complete': result = await OrderApi.complete(id.value); break
    case 'cancel': result = await OrderApi.cancel(id.value); break
    case 'resume': result = await OrderApi.resume(id.value); break
    default: return
  }
  saving.value = false
  if (result!.isSuccess) {
    toast.success(t(`ordering.orders.messages.${action}_success`))
    await loadOrder()
  } else {
    toast.error(result!.message ?? t('ordering.orders.messages.save_failed'))
  }
}

function cancel() {
  if (id.value) router.push({ name: ROUTE.ORDERS.VIEW, params: { id: id.value } })
  else router.push({ name: ROUTE.ORDERS.LIST })
}

function toggleEdit() {
  router.push({ name: ROUTE.ORDERS.EDIT, params: { id: id.value } })
}

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount)
}

onMounted(async () => { await loadOrder() })
</script>

<template>
  <div>
    <PageHeader :title="title" :subtitle="subtitle" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <template v-if="mode === 'view' && order">
          <Button v-if="canApprove" :label="t('ordering.orders.actions.approve')" icon="pi pi-check" severity="success" :loading="saving" @click="lifecycleAction('approve')" />
          <Button v-if="canComplete" :label="t('ordering.orders.actions.complete')" icon="pi pi-check-circle" severity="success" :loading="saving" @click="lifecycleAction('complete')" />
          <Button v-if="canCancel" :label="t('ordering.orders.actions.cancel_action')" icon="pi pi-times" severity="danger" :loading="saving" @click="lifecycleAction('cancel')" />
          <Button v-if="canResume" :label="t('ordering.orders.actions.resume')" icon="pi pi-refresh" severity="info" :loading="saving" @click="lifecycleAction('resume')" />
          <Button :label="t('ordering.orders.actions.edit')" @click="toggleEdit" />
        </template>
      </template>
    </PageHeader>

    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadOrder" />

    <template v-else-if="order && mode === 'view'">
      <AppCard>
        <div class="grid grid-cols-2 gap-4 mb-4">
          <div>
            <div class="text-sm text-surface-500">{{ t('ordering.orders.labels.status') }}</div>
            <StatusTag :status="order.status" />
          </div>
          <div>
            <div class="text-sm text-surface-500">{{ t('ordering.orders.labels.customer') }}</div>
            <div>{{ order.customerName || order.customerEmail || order.customerId }}</div>
          </div>
        </div>
        <div class="grid grid-cols-3 gap-4 mb-4">
          <div>
            <div class="text-sm text-surface-500">{{ t('ordering.orders.labels.subtotal') }}</div>
            <div>{{ formatCurrency(order.subtotal) }}</div>
          </div>
          <div>
            <div class="text-sm text-surface-500">{{ t('ordering.orders.labels.tax') }}</div>
            <div>{{ formatCurrency(order.taxTotal) }}</div>
          </div>
          <div>
            <div class="text-sm text-surface-500">{{ t('ordering.orders.labels.shipping') }}</div>
            <div>{{ formatCurrency(order.shippingTotal) }}</div>
          </div>
        </div>
        <div class="mb-4">
          <div class="text-lg font-semibold">{{ t('ordering.orders.labels.total') }}: {{ formatCurrency(order.total) }}</div>
        </div>
        <div v-if="order.notes" class="mb-4">
          <div class="text-sm text-surface-500">{{ t('ordering.orders.labels.notes') }}</div>
          <div>{{ order.notes }}</div>
        </div>
      </AppCard>
      <OrderLineItemManager
        :order-id="order.id"
        :line-items="order.lineItems"
        :readonly="true"
      />
    </template>

    <template v-else>
      <AppCard>
        <div v-if="mode === 'create'" class="grid grid-cols-1 gap-4 mb-4">
          <FormField :label="t('ordering.orders.labels.customer_id')" :error="errors.customerId" required>
            <input v-model="customerId" type="text" class="p-inputtext p-component w-full" />
          </FormField>
        </div>
        <div class="grid grid-cols-1 gap-4">
          <FormField :label="t('ordering.orders.labels.notes')" :error="errors.notes">
            <textarea v-model="notes" class="p-inputtext p-component w-full" rows="3" />
          </FormField>
        </div>
      </AppCard>

      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        :save-label="mode === 'create' ? t('ordering.orders.actions.save_create') : t('ordering.orders.actions.save_edit')"
        :cancel-label="t('ordering.orders.actions.cancel')"
        @save="save"
        @cancel="cancel"
      />
    </template>
  </div>
</template>
