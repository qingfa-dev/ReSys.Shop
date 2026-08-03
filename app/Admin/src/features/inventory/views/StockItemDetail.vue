<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Card from 'primevue/card'
import Message from 'primevue/message'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { useActiveStockLocations } from '../composables/useActiveStockLocations'
import { VariantApi } from '@/features/catalog/services/variantApi'
import type { VariantListItem } from '@/features/catalog/types/variant'
import { StockItemApi } from '../services/stockItemApi'
import {
  stockItemSchema,
  stockItemStockLocationId,
  stockItemVariantId,
  stockItemCountOnHand,
} from '../validations/stockItem'
import type { StockItemForm } from '../validations/stockItem'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()
const { items: activeStockLocations, load: loadActiveStockLocations } = useActiveStockLocations()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => (isEdit.value ? 'Edit Stock Item' : 'New Stock Item'))
const pageDescription = computed(() =>
  isEdit.value
    ? 'Edit the details of the stock item.'
    : 'Create a new stock item by filling out the form below.',
)

const form = ref<StockItemForm>({
  stockLocationId: '',
  variantId: '',
  countOnHand: 0,
  backorderable: false,
})

const stockItemResolver = zodResolver(stockItemSchema)
const stockLocationIdResolver = zodResolver(stockItemStockLocationId)
const variantIdResolver = zodResolver(stockItemVariantId)
const countOnHandResolver = zodResolver(stockItemCountOnHand)
const loading = ref(false)
const formLoaded = ref(!isEdit.value)
const variants = ref<VariantListItem[]>([])

async function loadVariants() {
  const result = await VariantApi.getVariants('', { pageSize: 100 })
  if (result.isSuccess) {
    variants.value = result.items
  } else {
    handleResult(result)
  }
}

async function loadStockItem(id: string) {
  const result = await StockItemApi.getStockItem(id)
  if (result.isSuccess) {
    const s = result.value
    form.value = {
      stockLocationId: s.stockLocationId,
      variantId: s.variantId,
      countOnHand: s.countOnHand,
      backorderable: s.backorderable,
    }
    formLoaded.value = true
    await ensureCurrentVariantPresent()
  } else {
    handleResult(result)
    router.push('/inventory/stock-items')
  }
}

async function ensureCurrentVariantPresent() {
  if (!form.value.variantId) return
  // Load: Fetch the variant when the 100-row dropdown omits the current one.
  const present = variants.value.some((v) => v.id === form.value.variantId)
  if (present) return

  const result = await VariantApi.getVariant(form.value.variantId)
  if (result.isSuccess && result.value) {
    variants.value = [...variants.value, result.value]
  } else {
    handleResult(result)
  }
}

onMounted(async () => {
  // Await: Stock location options for the form Select
  loadActiveStockLocations()
  await loadVariants()

  if (isEdit.value) {
    await loadStockItem(route.params.id as string)
  }
})

watch(
  () => route.params.id,
  (id) => {
    if (id && id !== 'new' && isEdit.value) {
      formLoaded.value = false
      loadStockItem(id as string)
    }
  },
)

async function onSubmit(event: FormSubmitEvent) {
  // Validate: Return early when zod form validation fails.
  if (!event.valid) return

  loading.value = true
  const data = event.values as StockItemForm
  const request = {
    stockLocationId: data.stockLocationId,
    variantId: data.variantId,
    countOnHand: data.countOnHand,
    backorderable: data.backorderable,
  }

  // Call: Persist the stock item, branching between update and create.
  const result = isEdit.value
    ? await StockItemApi.updateStockItem(route.params.id as string, request)
    : await StockItemApi.createStockItem(request)

  loading.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Stock item updated' : 'Stock item created')
    if (isEdit.value) {
      router.push('/inventory/stock-items')
    } else {
      router.replace(`/inventory/stock-items/${result.value.id}`)
    }
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/inventory/stock-items')
}
</script>

<template>
  <div class="flex flex-col h-full p-4">
    <!-- Section: Page Header — dynamic title with Save and Cancel actions -->
    <div class="flex-none flex justify-between items-start gap-4 mb-4">
      <div>
        <div class="font-semibold text-xl">{{ pageTitle }}</div>
        <p v-if="pageDescription" class="text-muted-color mt-1">{{ pageDescription }}</p>
      </div>
      <div class="flex items-center gap-2 shrink-0">
        <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="loading" form="stock-item-form" />
        <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel()" />
      </div>
    </div>

    <div class="flex-1 min-h-0 overflow-auto">
      <!-- Section: Content Card — form container for stock item details -->
      <Card>
        <template #content>
          <Form id="stock-item-form" :key="String(formLoaded)" :resolver="stockItemResolver" :initial-values="form" class="flex flex-col gap-4" @submit="onSubmit">
            <!-- Section: Form Fields — location, variant, and quantity fields -->
            <FormField v-slot="$field" name="stockLocationId" :resolver="stockLocationIdResolver" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Stock Location <span class="text-red-500">*</span></label>
              <Select :options="activeStockLocations" option-label="name" option-value="id" placeholder="Select a stock location" fluid />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField v-slot="$field" name="variantId" :resolver="variantIdResolver" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Variant <span class="text-red-500">*</span></label>
              <Select :options="variants" option-label="sku" option-value="id" placeholder="Select a variant" fluid />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField v-slot="$field" name="countOnHand" :resolver="countOnHandResolver" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Count On Hand <span class="text-red-500">*</span></label>
              <InputNumber fluid :min="0" />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField name="backorderable" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Backorderable</label>
              <ToggleSwitch />
            </FormField>
          </Form>
        </template>
      </Card>
    </div>
  </div>
</template>
