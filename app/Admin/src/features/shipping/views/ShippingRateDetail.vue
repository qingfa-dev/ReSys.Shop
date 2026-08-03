<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Card from 'primevue/card'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Select from 'primevue/select'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormResolverOptions, FormSubmitEvent } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { ShippingRateApi } from '../services/shippingRateApi'
import { ShippingMethodApi } from '../services/shippingMethodApi'
import { shippingRateSchema } from '../validations/shippingRate'
import type { ShippingRateForm } from '../validations/shippingRate'
import type { ShippingRateRequest } from '../types/shippingRate'
import type { ShippingMethodListItem } from '../types/shippingMethod'
import { useShippingRateDetail } from '../composables/useShippingRateDetail'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => (isEdit.value ? 'Edit Shipping Rate' : 'New Shipping Rate'))
const pageDescription = computed(() =>
  isEdit.value
    ? 'Edit the details of the shipping rate.'
    : 'Create a new shipping rate by filling out the form below.',
)

const resolver = (options: FormResolverOptions) => {
  const values: ShippingRateForm = {
    name: options.values.name,
    cost: options.values.cost,
    shippingMethodId: options.values.shippingMethodId,
    deliveryRange: options.values.deliveryRange || undefined,
    minWeight: options.values.minWeight ?? undefined,
    maxWeight: options.values.maxWeight ?? undefined,
    freeShippingThreshold: options.values.freeShippingThreshold ?? undefined,
  }
  return zodResolver(shippingRateSchema)({ ...options, values })
}

const form = ref<ShippingRateForm>({
  name: '',
  shippingMethodId: '',
  cost: 0,
  deliveryRange: undefined,
  minWeight: undefined,
  maxWeight: undefined,
  freeShippingThreshold: undefined,
})
const formLoaded = ref(!isEdit.value)
const submitting = ref(false)
const shippingMethods = ref<ShippingMethodListItem[]>([])

const { shippingRate, loading, error, fetchShippingRate } = useShippingRateDetail()

async function loadShippingMethods() {
  // Call: Fetch shipping methods for the rate's method Select.
  const result = await ShippingMethodApi.getShippingMethods({ pageSize: 100 })
  if (result.isSuccess) {
    shippingMethods.value = result.items
  } else {
    handleResult(result)
  }
}

async function initEditMode(id: string) {
  // Load: Fetch the rate to seed the editable form.
  const result = await fetchShippingRate(id)
  if (!result.isSuccess) {
    handleResult(result)
    router.push('/shipping/shipping-rates')
    return
  }
  const r = shippingRate.value
  if (r) {
    form.value = {
      name: r.name,
      shippingMethodId: r.shippingMethodId,
      cost: r.cost,
      deliveryRange: r.deliveryRange ?? undefined,
      minWeight: r.minWeight ?? undefined,
      maxWeight: r.maxWeight ?? undefined,
      freeShippingThreshold: r.freeShippingThreshold ?? undefined,
    }
  }
  formLoaded.value = true
}

onMounted(async () => {
  await loadShippingMethods()
  if (isEdit.value) initEditMode(route.params.id as string)
})

async function onSubmit(event: FormSubmitEvent) {
  if (!event.valid) return

  submitting.value = true
  const data = event.values as ShippingRateForm
  const request: ShippingRateRequest = {
    name: data.name,
    cost: data.cost,
    shippingMethodId: data.shippingMethodId,
    deliveryRange: data.deliveryRange || undefined,
    minWeight: data.minWeight ?? undefined,
    maxWeight: data.maxWeight ?? undefined,
    freeShippingThreshold: data.freeShippingThreshold ?? undefined,
  }

  const result = isEdit.value
    ? await ShippingRateApi.updateShippingRate(route.params.id as string, request)
    : await ShippingRateApi.createShippingRate(request)

  submitting.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Shipping rate updated' : 'Shipping rate created')
    if (isEdit.value) {
      router.push('/shipping/shipping-rates')
    } else {
      router.replace(`/shipping/shipping-rates/${result.value.id}`)
    }
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/shipping/shipping-rates')
}
</script>

<template>
  <div class="flex flex-col h-full p-4">
    <!-- Section: Page Header — title and save/cancel controls -->
    <div class="flex-none flex justify-between items-start gap-4 mb-4">
      <div>
        <div class="font-semibold text-xl">{{ pageTitle }}</div>
        <p v-if="pageDescription" class="text-muted-color mt-1">{{ pageDescription }}</p>
      </div>
      <div class="flex items-center gap-2 shrink-0">
        <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="submitting" :disabled="loading" form="shipping-rate-form" />
        <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel()" />
      </div>
    </div>

    <!-- Section: Content Card — scrolling area with loading, error, and form states -->
    <div class="flex-1 min-h-0 overflow-auto">
      <Card>
        <!-- Section: Form Fields — method, pricing, and weight-range inputs -->
        <template #content>
          <div v-if="loading" class="flex items-center gap-2 text-muted-color">
            <i class="pi pi-spin pi-spinner" />
            Loading shipping rate...
          </div>
          <Message v-else-if="error" severity="error">{{ error }}</Message>
          <Form
            v-else
            id="shipping-rate-form"
            :key="String(formLoaded)"
            :resolver="resolver"
            :initial-values="form"
            class="flex flex-col gap-4"
            @submit="onSubmit"
          >
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <FormField v-slot="$field" name="shippingMethodId" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Shipping Method <span class="text-red-500">*</span></label>
                <Select :options="shippingMethods" option-label="name" option-value="id" placeholder="Select a shipping method" fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="name" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Name <span class="text-red-500">*</span></label>
                <InputText fluid maxlength="255" />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
            </div>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <FormField v-slot="$field" name="cost" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Cost <span class="text-red-500">*</span></label>
                <InputNumber fluid :min="0" :step="0.01" />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="deliveryRange" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Delivery Range</label>
                <InputText fluid maxlength="100" />
                <small class="text-muted-color">Optional, e.g. "1-3 business days"</small>
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
            </div>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <FormField v-slot="$field" name="minWeight" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Min Weight</label>
                <InputNumber fluid :min="0" show-clear />
                <small class="text-muted-color">Optional</small>
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="maxWeight" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Max Weight</label>
                <InputNumber fluid :min="0" show-clear />
                <small class="text-muted-color">Optional</small>
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="freeShippingThreshold" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Free Shipping Threshold</label>
                <InputNumber fluid :min="0" show-clear />
                <small class="text-muted-color">Optional</small>
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
            </div>
          </Form>
        </template>
      </Card>
    </div>
  </div>
</template>
