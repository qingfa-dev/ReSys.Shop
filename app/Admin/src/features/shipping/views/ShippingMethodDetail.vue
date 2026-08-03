<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Card from 'primevue/card'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import ToggleSwitch from 'primevue/toggleswitch'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { ShippingMethodApi } from '../services/shippingMethodApi'
import { shippingMethodSchema } from '../validations/shippingMethod'
import type { ShippingMethodForm } from '../validations/shippingMethod'
import type { ShippingMethodRequest } from '../types/shippingMethod'
import { useShippingMethodDetail } from '../composables/useShippingMethodDetail'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => (isEdit.value ? 'Edit Shipping Method' : 'New Shipping Method'))
const pageDescription = computed(() =>
  isEdit.value
    ? 'Edit the details of the shipping method.'
    : 'Create a new shipping method by filling out the form below.',
)

const resolver = zodResolver(shippingMethodSchema)

const form = ref<ShippingMethodForm>({
  name: '',
  code: '',
  trackingUrl: '',
  adminName: '',
  calculatorType: '',
  taxCategoryId: '',
  position: 0,
  availableToUsers: true,
})
const formLoaded = ref(!isEdit.value)
const submitting = ref(false)

const { shippingMethod, loading, error, fetchShippingMethod } = useShippingMethodDetail()

async function initEditMode(id: string) {
  // Load: Fetch the method to seed the editable form.
  const result = await fetchShippingMethod(id)
  if (!result.isSuccess) {
    handleResult(result)
    router.push('/shipping/shipping-methods')
    return
  }
  const m = shippingMethod.value
  if (m) {
    form.value = {
      name: m.name,
      code: m.code ?? '',
      trackingUrl: m.trackingUrl ?? '',
      adminName: m.adminName ?? '',
      calculatorType: m.calculatorType,
      taxCategoryId: m.taxCategoryId ?? '',
      position: m.position,
      availableToUsers: m.availableToUsers,
    }
  }
  formLoaded.value = true
}

onMounted(() => {
  if (isEdit.value) initEditMode(route.params.id as string)
})

async function onSubmit(event: FormSubmitEvent) {
  if (!event.valid) return

  submitting.value = true
  const data = event.values as ShippingMethodForm
  const request: ShippingMethodRequest = {
    name: data.name,
    code: data.code || undefined,
    trackingUrl: data.trackingUrl || undefined,
    adminName: data.adminName || undefined,
    calculatorType: data.calculatorType,
    taxCategoryId: data.taxCategoryId || undefined,
    position: data.position,
    availableToUsers: data.availableToUsers,
  }

  const result = isEdit.value
    ? await ShippingMethodApi.updateShippingMethod(route.params.id as string, request)
    : await ShippingMethodApi.createShippingMethod(request)

  submitting.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Shipping method updated' : 'Shipping method created')
    if (isEdit.value) {
      router.push('/shipping/shipping-methods')
    } else {
      router.replace(`/shipping/shipping-methods/${result.value?.id}`)
    }
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/shipping/shipping-methods')
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
        <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="submitting" :disabled="loading" form="shipping-method-form" />
        <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel()" />
      </div>
    </div>

    <!-- Section: Content Card — scrolling area with loading, error, and form states -->
    <div class="flex-1 min-h-0 overflow-auto">
      <Card>
        <!-- Section: Form Fields — method identity, calculator, and availability inputs -->
        <template #content>
          <div v-if="loading" class="flex items-center gap-2 text-muted-color">
            <i class="pi pi-spin pi-spinner" />
            Loading shipping method...
          </div>
          <Message v-else-if="error" severity="error">{{ error }}</Message>
          <Form
            v-else
            id="shipping-method-form"
            :key="String(formLoaded)"
            :resolver="resolver"
            :initial-values="form"
            class="flex flex-col gap-4"
            @submit="onSubmit"
          >
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <FormField v-slot="$field" name="name" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Name <span class="text-red-500">*</span></label>
                <InputText fluid maxlength="255" />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="code" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Code</label>
                <InputText fluid maxlength="50" />
                <small class="text-muted-color">Optional internal code</small>
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
            </div>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <FormField v-slot="$field" name="adminName" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Admin Name</label>
                <InputText fluid />
                <small class="text-muted-color">Optional name shown to administrators</small>
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="calculatorType" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Calculator Type <span class="text-red-500">*</span></label>
                <InputText fluid maxlength="100" />
                <small class="text-muted-color">Free-form calculator key</small>
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
            </div>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <FormField v-slot="$field" name="trackingUrl" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Tracking URL</label>
                <InputText fluid />
                <small class="text-muted-color">Optional URL template with {trackingNumber} placeholder</small>
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="taxCategoryId" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Tax Category ID</label>
                <InputText fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
            </div>
            <FormField v-slot="$field" name="position" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Position <span class="text-red-500">*</span></label>
              <InputNumber fluid :min="0" />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField name="availableToUsers" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Available to Users</label>
              <small class="text-muted-color">Whether customers can select this method at checkout</small>
              <ToggleSwitch />
            </FormField>
          </Form>
        </template>
      </Card>
    </div>
  </div>
</template>
