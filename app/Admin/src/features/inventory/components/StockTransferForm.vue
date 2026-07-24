<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import FormField from '@/shared/components/forms/FormField.vue'
import FormActions from '@/shared/components/forms/FormActions.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import Tag from 'primevue/tag'
import { useStockTransfer } from '../composables/useStockTransfer'
import type { StockTransferResponse } from '../types'

const { id, mode, route, router, toast, api } = useStockTransfer()
const { t } = useI18n()

const item = ref<StockTransferResponse | null>(null)
const loading = ref(false)
const saving = ref(false)
const loadError = ref<string | null>(null)

const sourceLocationId = ref('')
const destinationLocationId = ref('')
const notes = ref('')
const lineInputs = ref<{ variantId: string; quantity: number }[]>([{ variantId: '', quantity: 1 }])

const title = computed(() => {
  if (mode.value === 'create') return 'Create Stock Transfer'
  if (item.value) return `Transfer: ${item.value.reference}`
  return 'Stock Transfer'
})

async function loadTransfer() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  const result = await api.get(id.value)
  if (result.isSuccess) {
    item.value = result.value
  } else {
    loadError.value = result.message ?? 'Failed to load transfer'
  }
  loading.value = false
}

async function onTransfer() {
  if (!id.value) return
  saving.value = true
  const result = await api.transfer(id.value)
  saving.value = false
  if (result.isSuccess) {
    item.value = result.value
    toast.success('Transfer completed')
  } else {
    toast.error(result.message ?? 'Transfer failed')
  }
}

async function onReceive() {
  if (!id.value) return
  saving.value = true
  const result = await api.receive(id.value)
  saving.value = false
  if (result.isSuccess) {
    item.value = result.value
    toast.success('Transfer received')
  } else {
    toast.error(result.message ?? 'Receive failed')
  }
}

async function onCancel() {
  if (!id.value) return
  saving.value = true
  const result = await api.cancel(id.value)
  saving.value = false
  if (result.isSuccess) {
    item.value = result.value
    toast.success('Transfer cancelled')
  } else {
    toast.error(result.message ?? 'Cancel failed')
  }
}

async function onCreate() {
  if (!sourceLocationId.value || !destinationLocationId.value) {
    toast.error('Source and destination locations are required')
    return
  }
  const validLines = lineInputs.value.filter(l => l.variantId && l.quantity > 0)
  if (validLines.length === 0) {
    toast.error('At least one line item is required')
    return
  }
  saving.value = true
  const result = await api.create({
    sourceLocationId: sourceLocationId.value,
    destinationLocationId: destinationLocationId.value,
    lineItems: validLines.map(l => ({ variantId: l.variantId, quantity: l.quantity })),
    notes: notes.value || null,
  })
  saving.value = false
  if (result.isSuccess) {
    toast.success('Transfer created')
    router.replace({ name: 'inventory.transfers.view', params: { id: result.value.id } })
  } else {
    toast.error(result.message ?? 'Create failed')
  }
}

function addLine() { lineInputs.value.push({ variantId: '', quantity: 1 }) }
function removeLine(i: number) { lineInputs.value.splice(i, 1) }

onMounted(async () => {
  await loadTransfer()
})
</script>

<template>
  <div>
    <PageHeader :title="title" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <button v-if="mode === 'view' && item?.status === 'Pending'" class="p-button p-component p-button-success mr-2" :disabled="saving" @click="onTransfer">Transfer</button>
        <button v-if="mode === 'view' && item?.status === 'InTransit'" class="p-button p-component p-button-info mr-2" :disabled="saving" @click="onReceive">Receive</button>
        <button v-if="mode === 'view' && (item?.status === 'Pending' || item?.status === 'InTransit')" class="p-button p-component p-button-danger" :disabled="saving" @click="onCancel">Cancel</button>
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="5" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadTransfer" />
    <div v-else-if="mode === 'create'" class="card">
      <div class="grid">
        <div class="col-6">
          <FormField label="Source Location ID" required>
            <input v-model="sourceLocationId" type="text" class="p-inputtext p-component w-full" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Destination Location ID" required>
            <input v-model="destinationLocationId" type="text" class="p-inputtext p-component w-full" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-12">
          <FormField label="Notes">
            <textarea v-model="notes" class="p-inputtext p-component w-full" rows="2" />
          </FormField>
        </div>
      </div>
      <h3 class="text-lg font-semibold mb-2">Line Items</h3>
      <div v-for="(line, i) in lineInputs" :key="i" class="grid mb-3">
        <div class="col-5">
          <FormField :label="'Variant ID'">
            <input v-model="line.variantId" type="text" class="p-inputtext p-component w-full" />
          </FormField>
        </div>
        <div class="col-5">
          <FormField :label="'Quantity'">
            <input v-model="line.quantity" type="number" class="p-inputtext p-component w-full" min="1" />
          </FormField>
        </div>
        <div class="col-2 flex align-items-end">
          <button v-if="lineInputs.length > 1" class="p-button p-component p-button-danger p-button-sm" @click="removeLine(i)">Remove</button>
        </div>
      </div>
      <button class="p-button p-component p-button-outlined mb-3" @click="addLine">Add Line</button>
      <FormActions
        :loading="saving"
        save-label="Create Transfer"
        cancel-label="Cancel"
        @save="onCreate"
        @cancel="() => router.push({ name: 'inventory.transfers.list' })"
      />
    </div>
    <div v-else-if="item" class="card">
      <div class="grid">
        <div class="col-4">
          <FormField label="Reference">
            <span class="p-inputtext p-component w-full" style="display:block; background:var(--p-surface-50)">{{ item.reference }}</span>
          </FormField>
        </div>
        <div class="col-4">
          <FormField label="From">
            <span class="p-inputtext p-component w-full" style="display:block; background:var(--p-surface-50)">{{ item.sourceLocationName || item.sourceLocationId }}</span>
          </FormField>
        </div>
        <div class="col-4">
          <FormField label="To">
            <span class="p-inputtext p-component w-full" style="display:block; background:var(--p-surface-50)">{{ item.destinationLocationName || item.destinationLocationId }}</span>
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-4">
          <FormField label="Status">
            <Tag :severity="item.status === 'Completed' ? 'success' : item.status === 'Cancelled' ? 'danger' : 'warn'" :value="item.status" />
          </FormField>
        </div>
        <div class="col-4">
          <FormField label="Created">
            <span class="p-inputtext p-component w-full" style="display:block; background:var(--p-surface-50)">{{ item.createdAt }}</span>
          </FormField>
        </div>
      </div>
      <div v-if="item.notes" class="grid">
        <div class="col-12">
          <FormField label="Notes">
            <span class="p-inputtext p-component w-full" style="display:block; background:var(--p-surface-50)">{{ item.notes }}</span>
          </FormField>
        </div>
      </div>
      <h3 class="text-lg font-semibold mt-4 mb-2">Line Items</h3>
      <div v-for="(line, i) in item.lineItems" :key="i" class="grid">
        <div class="col-4">
          <FormField label="SKU">
            <span class="p-inputtext p-component w-full" style="display:block; background:var(--p-surface-50)">{{ line.variantSku || line.variantId }}</span>
          </FormField>
        </div>
        <div class="col-4">
          <FormField label="Quantity">
            <span class="p-inputtext p-component w-full" style="display:block; background:var(--p-surface-50)">{{ line.quantity }}</span>
          </FormField>
        </div>
        <div class="col-4">
          <FormField label="Received">
            <span class="p-inputtext p-component w-full" style="display:block; background:var(--p-surface-50)">{{ line.receivedQuantity }}</span>
          </FormField>
        </div>
      </div>
    </div>
  </div>
</template>
