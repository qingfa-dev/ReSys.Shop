<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import FormField from '@/shared/components/forms/FormField.vue'
import FormActions from '@/shared/components/forms/FormActions.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { AppCard } from '@/shared/components'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import { useStockTransfer } from '../composables/useStockTransfer'
import { ROUTE } from '../routes'
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
  if (mode.value === 'create') return t('inventory.transfers.titles.create')
  if (item.value) return `${t('inventory.transfers.titles.detail')}: ${item.value.reference}`
  return t('inventory.transfers.titles.detail')
})

async function loadTransfer() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  try {
    const result = await api.get(id.value)
    if (result.isSuccess) {
      item.value = result.value
    } else {
      loadError.value = result.message ?? 'Failed to load transfer'
    }
  } catch (err) {
    console.error(err)
    loadError.value = 'Failed to load transfer'
  }
  loading.value = false
}

async function onTransfer() {
  if (!id.value) return
  saving.value = true
  try {
    const result = await api.transfer(id.value)
    saving.value = false
    if (result.isSuccess) {
      item.value = result.value
      toast.success(t('inventory.transfers.messages.transfer_success'))
    } else {
      toast.error(result.message ?? 'Transfer failed')
    }
  } catch (err) {
    console.error(err)
    saving.value = false
    toast.error('Transfer failed')
  }
}

async function onReceive() {
  if (!id.value) return
  saving.value = true
  try {
    const result = await api.receive(id.value)
    saving.value = false
    if (result.isSuccess) {
      item.value = result.value
      toast.success(t('inventory.transfers.messages.receive_success'))
    } else {
      toast.error(result.message ?? 'Receive failed')
    }
  } catch (err) {
    console.error(err)
    saving.value = false
    toast.error('Receive failed')
  }
}

async function onCancelTransfer() {
  if (!id.value) return
  saving.value = true
  try {
    const result = await api.cancel(id.value)
    saving.value = false
    if (result.isSuccess) {
      item.value = result.value
      toast.success(t('inventory.transfers.messages.cancel_success'))
    } else {
      toast.error(result.message ?? 'Cancel failed')
    }
  } catch (err) {
    console.error(err)
    saving.value = false
    toast.error('Cancel failed')
  }
}

async function onCreate() {
  if (!sourceLocationId.value || !destinationLocationId.value) {
    toast.error(t('inventory.transfers.messages.source_dest_required'))
    return
  }
  const validLines = lineInputs.value.filter(l => l.variantId && l.quantity > 0)
  if (validLines.length === 0) {
    toast.error(t('inventory.transfers.messages.lines_required'))
    return
  }
  saving.value = true
  try {
    const result = await api.create({
      sourceLocationId: sourceLocationId.value,
      destinationLocationId: destinationLocationId.value,
      lineItems: validLines.map(l => ({ variantId: l.variantId, quantity: l.quantity })),
      notes: notes.value || null,
    })
    saving.value = false
    if (result.isSuccess) {
      toast.success(t('inventory.transfers.messages.create_success'))
      router.replace({ name: ROUTE.TRANSFERS.VIEW, params: { id: result.value.id } })
    } else {
      toast.error(result.message ?? 'Create failed')
    }
  } catch (err) {
    console.error(err)
    saving.value = false
    toast.error('Create failed')
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
        <Button
          v-if="mode === 'view' && item?.status === 'Pending'"
          :label="t('inventory.transfers.actions.transfer')"
          icon="pi pi-send"
          severity="success"
          size="small"
          :disabled="saving"
          class="mr-2"
          @click="onTransfer"
        />
        <Button
          v-if="mode === 'view' && item?.status === 'InTransit'"
          :label="t('inventory.transfers.actions.receive')"
          icon="pi pi-check"
          severity="info"
          size="small"
          :disabled="saving"
          class="mr-2"
          @click="onReceive"
        />
        <Button
          v-if="mode === 'view' && (item?.status === 'Pending' || item?.status === 'InTransit')"
          :label="t('inventory.transfers.actions.cancel')"
          icon="pi pi-times"
          severity="danger"
          size="small"
          :disabled="saving"
          @click="onCancelTransfer"
        />
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="5" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadTransfer" />

    <AppCard v-else-if="mode === 'create'">
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('inventory.transfers.labels.source_location')" required>
            <input v-model="sourceLocationId" type="text" class="p-inputtext p-component w-full" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('inventory.transfers.labels.destination_location')" required>
            <input v-model="destinationLocationId" type="text" class="p-inputtext p-component w-full" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full">
          <FormField :label="t('inventory.transfers.labels.notes')">
            <textarea v-model="notes" class="p-inputtext p-component w-full" rows="2" />
          </FormField>
        </div>
      </div>
      <h3 class="mb-2 text-lg font-semibold">{{ t('inventory.transfers.titles.line_items') }}</h3>
      <div v-for="(line, i) in lineInputs" :key="i" class="mb-3 grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-5">
          <FormField :label="t('inventory.transfers.labels.variant_id')">
            <input v-model="line.variantId" type="text" class="p-inputtext p-component w-full" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-5">
          <FormField :label="t('inventory.transfers.labels.quantity')">
            <input v-model="line.quantity" type="number" class="p-inputtext p-component w-full" min="1" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-2 flex items-end">
          <Button
            v-if="lineInputs.length > 1"
            :label="t('inventory.transfers.actions.remove')"
            icon="pi pi-times"
            severity="danger"
            size="small"
            @click="removeLine(i)"
          />
        </div>
      </div>
      <Button
        :label="t('inventory.transfers.actions.add_line')"
        icon="pi pi-plus"
        outlined
        size="small"
        class="mb-3"
        @click="addLine"
      />
      <FormActions
        :loading="saving"
        :save-label="t('inventory.transfers.actions.create_transfer')"
        :cancel-label="t('inventory.transfers.actions.cancel')"
        @save="onCreate"
        @cancel="() => router.push({ name: ROUTE.TRANSFERS.LIST })"
      />
    </AppCard>

    <AppCard v-else-if="item">
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('inventory.transfers.labels.reference')">
            <span class="block w-full rounded bg-surface-50 px-3 py-2 text-sm dark:bg-surface-700">{{ item.reference }}</span>
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('inventory.transfers.labels.from')">
            <span class="block w-full rounded bg-surface-50 px-3 py-2 text-sm dark:bg-surface-700">{{ item.sourceLocationName || item.sourceLocationId }}</span>
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('inventory.transfers.labels.to')">
            <span class="block w-full rounded bg-surface-50 px-3 py-2 text-sm dark:bg-surface-700">{{ item.destinationLocationName || item.destinationLocationId }}</span>
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('inventory.transfers.labels.status')">
            <Tag :severity="item.status === 'Completed' ? 'success' : item.status === 'Cancelled' ? 'danger' : 'warn'" :value="item.status" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('inventory.transfers.labels.created')">
            <span class="block w-full rounded bg-surface-50 px-3 py-2 text-sm dark:bg-surface-700">{{ item.createdAt }}</span>
          </FormField>
        </div>
      </div>
      <div v-if="item.notes" class="grid grid-cols-12 gap-4">
        <div class="col-span-full">
          <FormField :label="t('inventory.transfers.labels.notes')">
            <span class="block w-full rounded bg-surface-50 px-3 py-2 text-sm dark:bg-surface-700">{{ item.notes }}</span>
          </FormField>
        </div>
      </div>
      <h3 class="mt-4 mb-2 text-lg font-semibold">{{ t('inventory.transfers.titles.line_items') }}</h3>
      <div v-for="(line, i) in item.lineItems" :key="i" class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('inventory.transfers.labels.sku')">
            <span class="block w-full rounded bg-surface-50 px-3 py-2 text-sm dark:bg-surface-700">{{ line.variantSku || line.variantId }}</span>
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('inventory.transfers.labels.quantity')">
            <span class="block w-full rounded bg-surface-50 px-3 py-2 text-sm dark:bg-surface-700">{{ line.quantity }}</span>
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('inventory.transfers.labels.received')">
            <span class="block w-full rounded bg-surface-50 px-3 py-2 text-sm dark:bg-surface-700">{{ line.receivedQuantity }}</span>
          </FormField>
        </div>
      </div>
    </AppCard>
  </div>
</template>
