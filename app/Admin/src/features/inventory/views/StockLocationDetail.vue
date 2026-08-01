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
import { StockLocationApi } from '../services/stockLocationApi'
import type { StockLocationDetail } from '../types/stockLocation'
import type { StockLocationRequest } from '../types/stockLocation'
import {
  stockLocationSchema,
  stockLocationName,
  stockLocationCode,
  stockLocationCity,
  stockLocationPostalCode,
  stockLocationPhone,
  stockLocationPosition,
} from '../validations/stockLocation'
import type { StockLocationForm } from '../validations/stockLocation'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => (isEdit.value ? 'Edit Stock Location' : 'New Stock Location'))
const pageDescription = computed(() =>
  isEdit.value
    ? 'Edit the details of the stock location.'
    : 'Create a new stock location by filling out the form below.',
)

const form = ref<StockLocationForm>({
  name: '',
  code: '',
  city: '',
  postalCode: '',
  phone: '',
  position: 0,
  active: true,
})

const stockLocationResolver = zodResolver(stockLocationSchema)
const nameResolver = zodResolver(stockLocationName)
const codeResolver = zodResolver(stockLocationCode)
const cityResolver = zodResolver(stockLocationCity)
const postalCodeResolver = zodResolver(stockLocationPostalCode)
const phoneResolver = zodResolver(stockLocationPhone)
const positionResolver = zodResolver(stockLocationPosition)
const loading = ref(false)
const formLoaded = ref(!isEdit.value)
const locationRecord = ref<StockLocationDetail | null>(null)

function buildRequest(data: StockLocationForm): StockLocationRequest {
  const record = locationRecord.value
  return {
    name: data.name,
    presentation: record?.presentation,
    code: data.code || undefined,
    address1: record?.address1,
    address2: record?.address2,
    city: data.city || undefined,
    postalCode: data.postalCode || undefined,
    phone: data.phone || undefined,
    active: data.active,
    default: record?.default ?? false,
    backorderableDefault: record?.backorderableDefault ?? false,
    propagateAllVariants: record?.propagateAllVariants ?? false,
    position: data.position,
  }
}

async function loadStockLocation(id: string) {
  const result = await StockLocationApi.getStockLocation(id)
  if (result.isSuccess) {
    const s = result.value
    locationRecord.value = s
    form.value = {
      name: s.name,
      code: s.code ?? '',
      city: s.city ?? '',
      postalCode: s.postalCode ?? '',
      phone: s.phone ?? '',
      position: s.position,
      active: s.active,
    }
    formLoaded.value = true
  } else {
    handleResult(result)
    router.push('/inventory/stock-locations')
  }
}

onMounted(async () => {
  if (isEdit.value) {
    await loadStockLocation(route.params.id as string)
  }
})

watch(
  () => route.params.id,
  (id) => {
    if (id && id !== 'new' && isEdit.value) {
      formLoaded.value = false
      loadStockLocation(id as string)
    }
  },
)

async function onSubmit(event: FormSubmitEvent) {
  if (!event.valid) return

  loading.value = true
  const data = event.values as StockLocationForm
  const request = buildRequest(data)

  const result = isEdit.value
    ? await StockLocationApi.updateStockLocation(route.params.id as string, request)
    : await StockLocationApi.createStockLocation(request)

  loading.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Stock location updated' : 'Stock location created')
    if (isEdit.value) {
      router.push('/inventory/stock-locations')
    } else {
      router.replace(`/inventory/stock-locations/${result.value.id}`)
    }
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/inventory/stock-locations')
}
</script>

<template>
  <div class="flex flex-col h-full p-4">
    <div class="flex-none flex justify-between items-start gap-4 mb-4">
      <div>
        <div class="font-semibold text-xl">{{ pageTitle }}</div>
        <p v-if="pageDescription" class="text-muted-color mt-1">{{ pageDescription }}</p>
      </div>
      <div class="flex items-center gap-2 shrink-0">
        <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="loading" form="stock-location-form" />
        <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel()" />
      </div>
    </div>

    <div class="flex-1 min-h-0 overflow-auto">
      <Card>
        <template #content>
          <Form id="stock-location-form" :key="String(formLoaded)" :resolver="stockLocationResolver" :initial-values="form" class="flex flex-col gap-4" @submit="onSubmit">
            <FormField v-slot="$field" name="name" :resolver="nameResolver" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Name <span class="text-red-500">*</span></label>
              <InputText fluid />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField v-slot="$field" name="code" :resolver="codeResolver" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Code</label>
              <InputText fluid />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField v-slot="$field" name="city" :resolver="cityResolver" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">City</label>
              <InputText fluid />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField v-slot="$field" name="postalCode" :resolver="postalCodeResolver" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Postal Code</label>
              <InputText fluid />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField v-slot="$field" name="phone" :resolver="phoneResolver" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Phone</label>
              <InputText fluid />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField v-slot="$field" name="position" :resolver="positionResolver" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Position <span class="text-red-500">*</span></label>
              <InputNumber fluid :min="0" />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField name="active" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Active</label>
              <ToggleSwitch />
            </FormField>
          </Form>
        </template>
      </Card>
    </div>
  </div>
</template>
