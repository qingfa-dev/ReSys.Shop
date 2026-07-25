<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import FormField from '@/shared/components/forms/FormField.vue'
import FormActions from '@/shared/components/forms/FormActions.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import StatusTag from '@/shared/components/data/StatusTag.vue'
import Button from 'primevue/button'
import OrderLineItemManager from './OrderLineItemManager.vue'
import { useOrder } from '../composables/useOrder'
import { OrderForms } from '../schemas'
import { OrderFormMapper } from '../mappers/order.mapper'
import { OrderApi } from '../api'
import { ROUTE } from '../routes'
import type { OrderResponse } from '../types'

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
  if (mode.value === 'create') return 'Create Order'
  if (mode.value === 'edit') return `Edit Order: ${order.value?.orderNumber || ''}`
  return order.value?.orderNumber || 'Order Details'
})

const canApprove = computed(() => order.value?.status === 'pending')
const canComplete = computed(() => order.value?.status === 'approved' || order.value?.status === 'processing')
const canCancel = computed(() => order.value?.status !== 'completed' && order.value?.status !== 'cancelled')
const canResume = computed(() => order.value?.status === 'cancelled' || order.value?.status === 'on_hold')

async function loadOrder() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  const result = await OrderApi.get(id.value)
  if (result.isSuccess) {
    order.value = result.value
    setValues({
      customerId: result.value.customerId,
      notes: result.value.notes ?? undefined,
    })
  } else {
    loadError.value = result.message ?? 'Failed to load order'
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
    : await OrderApi.create(data)
  saving.value = false
  if (result.isSuccess) {
    toast.success(id.value ? 'Order updated' : 'Order created')
    const newId = result.value.id
    router.replace({ name: ROUTE.ORDERS.VIEW, params: { id: newId } })
  } else {
    toast.error(result.message ?? 'Save failed')
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
    toast.success(`Order ${action}d`)
    await loadOrder()
  } else {
    toast.error(result!.message ?? `Failed to ${action} order`)
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
    <PageHeader :title="title" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <template v-if="mode === 'view' && order">
          <Button v-if="canApprove" label="Approve" icon="pi pi-check" severity="success" :loading="saving" @click="lifecycleAction('approve')" />
          <Button v-if="canComplete" label="Complete" icon="pi pi-check-circle" severity="success" :loading="saving" @click="lifecycleAction('complete')" />
          <Button v-if="canCancel" label="Cancel" icon="pi pi-times" severity="danger" :loading="saving" @click="lifecycleAction('cancel')" />
          <Button v-if="canResume" label="Resume" icon="pi pi-refresh" severity="info" :loading="saving" @click="lifecycleAction('resume')" />
          <button class="p-button p-component" @click="toggleEdit">Edit</button>
        </template>
      </template>
    </PageHeader>

    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadOrder" />

    <div v-else-if="order && mode === 'view'" class="card">
      <div class="grid mb-4">
        <div class="col-6">
          <div class="text-sm text-surface-500">Status</div>
          <StatusTag :status="order.status" />
        </div>
        <div class="col-6">
          <div class="text-sm text-surface-500">Customer</div>
          <div>{{ order.customerName || order.customerEmail || order.customerId }}</div>
        </div>
      </div>
      <div class="grid mb-4">
        <div class="col-4">
          <div class="text-sm text-surface-500">Subtotal</div>
          <div>{{ formatCurrency(order.subtotal) }}</div>
        </div>
        <div class="col-4">
          <div class="text-sm text-surface-500">Tax</div>
          <div>{{ formatCurrency(order.taxTotal) }}</div>
        </div>
        <div class="col-4">
          <div class="text-sm text-surface-500">Shipping</div>
          <div>{{ formatCurrency(order.shippingTotal) }}</div>
        </div>
      </div>
      <div class="mb-4">
        <div class="text-lg font-semibold">Total: {{ formatCurrency(order.total) }}</div>
      </div>
      <div v-if="order.notes" class="mb-4">
        <div class="text-sm text-surface-500">Notes</div>
        <div>{{ order.notes }}</div>
      </div>

      <OrderLineItemManager
        :order-id="order.id"
        :line-items="order.lineItems"
        :readonly="true"
      />
    </div>

    <div v-else class="card">
      <div v-if="mode === 'create'" class="grid">
        <div class="col-12">
          <FormField label="Customer ID" :error="errors.customerId" required>
            <input v-model="customerId" type="text" class="p-inputtext p-component w-full" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-12">
          <FormField label="Notes" :error="errors.notes">
            <textarea v-model="notes" class="p-inputtext p-component w-full" rows="3" />
          </FormField>
        </div>
      </div>

      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        :save-label="mode === 'create' ? 'Create Order' : 'Save Changes'"
        :cancel-label="'Cancel'"
        @save="save"
        @cancel="cancel"
      />
    </div>
  </div>
</template>
