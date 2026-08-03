<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Card from 'primevue/card'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Select from 'primevue/select'
import InputNumber from 'primevue/inputnumber'
import ToggleSwitch from 'primevue/toggleswitch'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { PaymentMethodApi } from '../services/paymentMethodApi'
import { paymentMethodSchema } from '../validations/paymentMethod'
import type { PaymentMethodForm } from '../validations/paymentMethod'
import type { PaymentMethodRequest } from '../types/paymentMethod'
import { usePaymentMethodDetail } from '../composables/usePaymentMethodDetail'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => (isEdit.value ? 'Edit Payment Method' : 'New Payment Method'))
const pageDescription = computed(() =>
  isEdit.value
    ? 'Edit the details of the payment method.'
    : 'Create a new payment method by filling out the form below.',
)

const resolver = zodResolver(paymentMethodSchema)
const displayOnOptions = ['Both', 'Frontend', 'Backend']

const form = ref<PaymentMethodForm>({
  name: '',
  code: '',
  providerKey: '',
  description: '',
  displayOn: 'Both',
  position: 0,
  active: true,
  webhookEnabled: false,
  autoCapture: true,
})
const formLoaded = ref(!isEdit.value)
const submitting = ref(false)

const { paymentMethod, loading, error, fetchPaymentMethod } = usePaymentMethodDetail()

async function initEditMode(id: string) {
  // Load: Fetch the method's values to seed the editable form.
  const result = await fetchPaymentMethod(id)
  if (!result.isSuccess) {
    handleResult(result)
    router.push('/payment/payment-methods')
    return
  }
  const m = paymentMethod.value
  if (m) {
    form.value = {
      name: m.name,
      code: m.code ?? '',
      providerKey: m.providerKey,
      description: m.description ?? '',
      displayOn: m.displayOn,
      position: m.position,
      active: m.active,
      webhookEnabled: m.webhookEnabled,
      autoCapture: m.autoCapture,
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
  // Map: Shape the resolved method form into the create/update request.
  const data = event.values as PaymentMethodForm
  const request: PaymentMethodRequest = {
    name: data.name,
    code: data.code,
    description: data.description || undefined,
    providerKey: data.providerKey,
    displayOn: data.displayOn,
    position: data.position,
    active: data.active,
    webhookEnabled: data.webhookEnabled,
    autoCapture: data.autoCapture,
  }

  const result = isEdit.value
    ? await PaymentMethodApi.updatePaymentMethod(route.params.id as string, request)
    : await PaymentMethodApi.createPaymentMethod(request)

  submitting.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Payment method updated' : 'Payment method created')
    if (isEdit.value) {
      router.push('/payment/payment-methods')
    } else {
      router.replace(`/payment/payment-methods/${result.value?.id}`)
    }
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/payment/payment-methods')
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
        <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="submitting" :disabled="loading" form="payment-method-form" />
        <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel()" />
      </div>
    </div>

    <!-- Section: Content Card — scrolling area with loading, error, and form states -->
    <div class="flex-1 min-h-0 overflow-auto">
      <Card>
        <!-- Section: Form Fields — method identity, display, and capture settings -->
        <template #content>
          <div v-if="loading" class="flex items-center gap-2 text-muted-color">
            <i class="pi pi-spin pi-spinner" />
            Loading payment method...
          </div>
          <Message v-else-if="error" severity="error">{{ error }}</Message>
          <Form
            v-else
            id="payment-method-form"
            :key="String(formLoaded)"
            :resolver="resolver"
            :initial-values="form"
            class="flex flex-col gap-4"
            @submit="onSubmit"
          >
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <FormField v-slot="$field" name="name" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Name <span class="text-red-500">*</span></label>
                <InputText fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="code" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Code <span class="text-red-500">*</span></label>
                <InputText fluid />
                <small class="text-muted-color">Only letters, numbers, underscores, and hyphens</small>
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
            </div>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <FormField v-slot="$field" name="providerKey" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Provider Key <span class="text-red-500">*</span></label>
                <InputText fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="displayOn" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Display On</label>
                <Select :options="displayOnOptions" fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
            </div>
            <FormField v-slot="$field" name="position" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Position <span class="text-red-500">*</span></label>
              <InputNumber fluid :min="0" :max="9999" />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField v-slot="$field" name="description" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Description</label>
              <Textarea fluid rows="3" />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <FormField name="webhookEnabled" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Webhook Enabled</label>
                <ToggleSwitch />
              </FormField>
              <FormField name="autoCapture" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Auto Capture</label>
                <ToggleSwitch />
              </FormField>
              <FormField name="active" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Active</label>
                <ToggleSwitch />
              </FormField>
            </div>
          </Form>
        </template>
      </Card>
    </div>
  </div>
</template>
