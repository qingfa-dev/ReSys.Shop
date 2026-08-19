<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import Card from 'primevue/card'
import Message from 'primevue/message'
import Tag from 'primevue/tag'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import { z } from 'zod'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { formatDateTimeUtc } from '@/shared/utils/date'
import { useActiveStockLocations } from '../composables/useActiveStockLocations'
import { StockTransferApi } from '../services/stockTransferApi'
import { VariantApi } from '@/features/catalog/services/variantApi'
import type { VariantListItem } from '@/features/catalog/types/variant'
import type {
  StockTransferDetail,
  StockTransferItemRequest,
  StockTransferState,
} from '../types/stockTransfer'
import {
  stockTransferItems,
  stockTransferSourceLocationId,
  stockTransferDestinationLocationId,
} from '../validations/stockTransfer'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const confirm = useConfirm()
const { handleResult } = useApiErrorHandler()
const { items: activeStockLocations, load: loadActiveStockLocations } = useActiveStockLocations()

const isCreate = computed(() => route.params.id === 'new')
const pageTitle = computed(() => (isCreate.value ? 'New Stock Transfer' : 'Stock Transfer Detail'))
const pageDescription = computed(() =>
  isCreate.value
    ? 'Create a stock transfer between locations by adding line items.'
    : 'Review stock transfer details and manage its state.',
)

const stockTransferHeaderSchema = z.object({
  sourceLocationId: stockTransferSourceLocationId,
  destinationLocationId: stockTransferDestinationLocationId,
  reference: z.string().optional(),
}).refine((d) => d.destinationLocationId !== d.sourceLocationId, {
  message: 'Source and destination locations must differ.',
  path: ['destinationLocationId'],
})

const resolver = zodResolver(stockTransferHeaderSchema)
const sourceResolver = zodResolver(stockTransferSourceLocationId)
const destinationResolver = zodResolver(stockTransferDestinationLocationId)

const form = ref({
  sourceLocationId: '',
  destinationLocationId: '',
  reference: '',
})

const items = ref<StockTransferItemRequest[]>([{ variantId: '', quantity: 1 }])
const itemsError = ref('')
const loading = ref(false)

const variants = ref<VariantListItem[]>([])

async function loadVariants() {
  // Load: Fetch the first 100 variants for the line-item dropdowns.
  const result = await VariantApi.getVariants('', { pageSize: 100 })
  if (result.isSuccess) {
    variants.value = result.items
  } else {
    handleResult(result)
  }
}

function addItem() {
  items.value.push({ variantId: '', quantity: 1 })
}

function removeItem(index: number) {
  items.value.splice(index, 1)
}

async function onSubmit(event: FormSubmitEvent) {
  // Validate: Return early when zod form validation fails.
  if (!event.valid) return

  // Validate: Reject line items that fail the transfer items schema.
  const parsed = stockTransferItems.safeParse(items.value)
  if (!parsed.success) {
    itemsError.value = parsed.error.issues[0]?.message ?? 'Invalid transfer items.'
    return
  }
  itemsError.value = ''

  const data = event.values as {
    sourceLocationId: string
    destinationLocationId: string
    reference?: string
  }

  loading.value = true
  // Call: Persist the new transfer with its validated line items.
  const result = await StockTransferApi.createStockTransfer({
    sourceLocationId: data.sourceLocationId,
    destinationLocationId: data.destinationLocationId,
    reference: data.reference || undefined,
    items: parsed.data,
  })
  loading.value = false

  if (result.isSuccess) {
    notify.success('Stock transfer created')
    router.replace(`/inventory/stock-transfers/${result.value.id}`)
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/inventory/stock-transfers')
}

const detail = ref<StockTransferDetail | null>(null)
const detailLoading = ref(false)
const actionLoading = ref(false)

async function loadDetail(id: string) {
  detailLoading.value = true
  // Load: Fetch the transfer's detail and its item variants.
  const result = await StockTransferApi.getStockTransfer(id)
  detailLoading.value = false
  if (result.isSuccess) {
    detail.value = result.value
    await ensureVariantsPresent(result.value.items.map((i) => i.variantId))
  } else {
    handleResult(result)
    router.push('/inventory/stock-transfers')
  }
}

async function ensureVariantsPresent(variantIds: string[]) {
  // Load: Fetch variants omitted by the 100-row dropdown so names render.
  for (const variantId of variantIds) {
    if (variants.value.some((v) => v.id === variantId)) continue
    const result = await VariantApi.getVariant(variantId)
    if (result.isSuccess && result.value) {
      variants.value = [...variants.value, result.value]
    }
  }
}

const STATE_SEVERITY: Record<StockTransferState, string> = {
  Draft: 'warn',
  InTransit: 'info',
  Received: 'success',
  Canceled: 'danger',
}

function stateSeverity(state: StockTransferState | undefined): string {
  return state ? STATE_SEVERITY[state] : 'secondary'
}

function locationName(locationId: string): string {
  // Filter: Resolve the location name from the active locations list; fall back to the raw ID
  return activeStockLocations.value.find((l) => l.id === locationId)?.name ?? locationId
}

function variantSku(variantId: string): string {
  return variants.value.find((v) => v.id === variantId)?.sku ?? variantId
}

function confirmSend() {
  if (!detail.value) return
  // Trigger: Confirm before sending the transfer into transit.
  confirm.require({
    message: 'Send this stock transfer? It will be marked as In Transit.',
    header: 'Confirm Send',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Send',
    accept: async () => {
      const id = route.params.id as string
      actionLoading.value = true
      // Call: Flush the transfer to In Transit via the transfer API.
      const result = await StockTransferApi.transferStockTransfer(id)
      actionLoading.value = false
      if (result.isSuccess) {
        if (detail.value) detail.value.state = 'InTransit'
        notify.success('Stock transfer sent')
      } else {
        handleResult(result)
      }
    },
  })
}

function confirmReceive() {
  const d = detail.value
  if (!d) return
  // Trigger: Confirm before receiving the full transfer quantity.
  confirm.require({
    message: 'Receive all quantities for this stock transfer?',
    header: 'Confirm Receive',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Receive',
    accept: async () => {
      const id = route.params.id as string
      actionLoading.value = true
      // Call: Receive all quantities via the transfer API, then update local state.
      const result = await StockTransferApi.receiveStockTransfer(id, {
        items: d.items.map((i) => ({ variantId: i.variantId, quantity: i.quantity })),
      })
      actionLoading.value = false
      if (result.isSuccess) {
        d.state = 'Received'
        d.items = d.items.map((i) => ({ ...i, receivedQuantity: i.quantity }))
        notify.success('Stock transfer received')
      } else {
        handleResult(result)
      }
    },
  })
}

function confirmCancel() {
  if (!detail.value) return
  // Trigger: Confirm before canceling the transfer.
  confirm.require({
    message: 'Cancel this stock transfer?',
    header: 'Confirm Cancel',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'No',
    acceptLabel: 'Cancel Transfer',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const id = route.params.id as string
      actionLoading.value = true
      // Call: Cancel the transfer and reflect the Canceled state locally.
      const result = await StockTransferApi.cancelStockTransfer(id)
      actionLoading.value = false
      if (result.isSuccess) {
        if (detail.value) detail.value.state = 'Canceled'
        notify.success('Stock transfer canceled')
      } else {
        handleResult(result)
      }
    },
  })
}

onMounted(async () => {
  loadActiveStockLocations()
  await loadVariants()

  if (!isCreate.value) {
    await loadDetail(route.params.id as string)
  }
})

watch(
  () => route.params.id,
  (id) => {
    if (id && id !== 'new') {
      detail.value = null
      loadDetail(id as string)
    }
  },
)
</script>

<template>
  <div class="flex flex-col h-full p-4">
    <!-- Section: Page Header — title plus create or state-management actions -->
    <div class="flex-none flex justify-between items-start gap-4 mb-4">
      <div>
        <div class="font-semibold text-xl">{{ pageTitle }}</div>
        <p v-if="pageDescription" class="text-muted-color mt-1">{{ pageDescription }}</p>
      </div>
      <div class="flex items-center gap-2 shrink-0">
        <template v-if="isCreate">
          <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="loading" form="stock-transfer-form" />
          <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel()" />
        </template>
        <template v-else>
          <Button v-if="detail?.state === 'Draft'" label="Send" icon="pi pi-send" severity="primary" :loading="actionLoading" @click="confirmSend()" />
          <Button v-if="detail?.state === 'InTransit'" label="Receive" icon="pi pi-check-circle" severity="primary" :loading="actionLoading" @click="confirmReceive()" />
          <Button v-if="detail?.state === 'Draft' || detail?.state === 'InTransit'" label="Cancel" icon="pi pi-times" severity="secondary" :loading="actionLoading" @click="confirmCancel()" />
          <Button label="Back" type="button" icon="pi pi-arrow-left" severity="secondary" @click="onCancel()" />
        </template>
      </div>
    </div>

    <div class="flex-1 min-h-0 overflow-auto">
      <!-- Section: Content Card — create form, detail readout, or loading fallback -->
      <Card v-if="isCreate">
        <template #content>
          <Form id="stock-transfer-form" :resolver="resolver" :initial-values="form" class="flex flex-col gap-4" @submit="onSubmit">
            <!-- Section: Form Fields — source, destination, and reference fields -->
            <FormField v-slot="$field" name="sourceLocationId" :resolver="sourceResolver" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Source Location <span class="text-red-500">*</span></label>
              <Select :options="activeStockLocations" option-label="name" option-value="id" placeholder="Select a source location" fluid />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField v-slot="$field" name="destinationLocationId" :resolver="destinationResolver" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Destination Location <span class="text-red-500">*</span></label>
              <Select :options="activeStockLocations" option-label="name" option-value="id" placeholder="Select a destination location" fluid />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField name="reference" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Reference</label>
              <InputText fluid />
            </FormField>
          </Form>

          <!-- Section: Transfer Line Items — add and manage item rows -->
          <div class="mt-6 flex flex-col gap-3">
            <div class="flex items-center justify-between">
              <span class="font-medium text-surface-900 dark:text-surface-0">Items <span class="text-red-500">*</span></span>
              <Button label="Add Item" icon="pi pi-plus" severity="secondary" type="button" @click="addItem()" />
            </div>
            <div v-for="(item, index) in items" :key="index" class="flex items-center gap-2">
              <Select v-model="item.variantId" :options="variants" option-label="sku" option-value="id" placeholder="Select a variant" class="flex-1" />
              <InputNumber v-model="item.quantity" :min="1" placeholder="Quantity" class="w-40" />
              <Button icon="pi pi-trash" severity="danger" text rounded aria-label="Remove item" type="button" @click="removeItem(index)" />
            </div>
            <Message v-if="itemsError" severity="error" size="small" variant="simple">{{ itemsError }}</Message>
          </div>
        </template>
      </Card>

      <Card v-else-if="detail">
        <template #content>
          <!-- Section: Detail Readout — read-only transfer summary and items -->
          <div class="flex flex-col gap-4">
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
              <div>
                <div class="text-sm text-muted-color">Number</div>
                <div class="font-medium">{{ detail.number }}</div>
              </div>
              <div>
                <div class="text-sm text-muted-color">Reference</div>
                <div class="font-medium">{{ detail.reference || '—' }}</div>
              </div>
              <div>
                <div class="text-sm text-muted-color">State</div>
                <Tag :value="detail.state" :severity="stateSeverity(detail.state)" />
              </div>
              <div>
                <div class="text-sm text-muted-color">Source Location</div>
                <div class="font-medium">{{ locationName(detail.sourceLocationId) }}</div>
              </div>
              <div>
                <div class="text-sm text-muted-color">Destination Location</div>
                <div class="font-medium">{{ locationName(detail.destinationLocationId) }}</div>
              </div>
              <div>
                <div class="text-sm text-muted-color">Created</div>
                <div class="font-medium">{{ formatDateTimeUtc(detail.createdAtUtc) }}</div>
              </div>
              <div>
                <div class="text-sm text-muted-color">Modified</div>
                <div class="font-medium">{{ detail.modifiedAtUtc ? formatDateTimeUtc(detail.modifiedAtUtc) : '—' }}</div>
              </div>
            </div>

            <div class="mt-2">
              <div class="font-semibold mb-2">Items</div>
              <DataTable :value="detail.items" data-key="id" striped-rows>
                <Column header="Variant">
                  <template #body="{ data }">{{ variantSku(data.variantId) }}</template>
                </Column>
                <Column field="quantity" header="Quantity" />
                <Column field="receivedQuantity" header="Received" />
                <template #empty>No items.</template>
              </DataTable>
            </div>
          </div>
        </template>
      </Card>

      <Card v-else>
        <template #content>
          <p class="text-muted-color">{{ detailLoading ? 'Loading stock transfer...' : 'Stock transfer not found.' }}</p>
        </template>
      </Card>
    </div>
  </div>
</template>
