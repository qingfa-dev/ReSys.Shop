<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Card from 'primevue/card'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import ToggleSwitch from 'primevue/toggleswitch'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { useAuthStore } from '@/features/auth/stores/authStore'
import { useAddressDetail } from '../composables/useAddressDetail'
import { AddressApi } from '../services/addressApi'
import { addressSchema } from '../validations/address'
import type { AddressForm } from '../validations/address'
import type { AddressRequest, AddressType } from '../types/address'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()
const authStore = useAuthStore()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => (isEdit.value ? 'Edit Address' : 'New Address'))
const pageDescription = computed(() =>
  isEdit.value
    ? 'Edit the details of the address.'
    : 'Create a new address by filling out the form below.',
)

const addressTypeOptions: AddressType[] = ['Shipping', 'Billing', 'Other']

// The address API needs the owner userId up front (both for get and list).
// Resolution order: route query `?userId=` (set by the Add/Edit links) ->
// the signed-in user's id -> empty string.
const initialUserId = (() => {
  const requested = route.query.userId
  return (typeof requested === 'string' ? requested : undefined) ?? authStore.currentUser?.userId ?? ''
})()

const resolver = zodResolver(addressSchema)
const form = ref<AddressForm>({
  userId: initialUserId,
  addressType: 'Shipping',
  firstName: '',
  lastName: '',
  address1: '',
  address2: '',
  city: '',
  zipCode: '',
  phone: '',
  label: '',
  isDefault: false,
  countryName: '',
})
const formLoaded = ref(!isEdit.value)
const submitting = ref(false)

const { address, loading, error, fetchAddress } = useAddressDetail()

async function initEditMode(id: string) {
  const result = await fetchAddress(form.value.userId, id)
  if (!result.isSuccess) {
    handleResult(result)
    router.push('/profile/addresses')
    return
  }
  const a = address.value
  if (a) {
    form.value = {
      userId: a.userId,
      addressType: a.addressType,
      firstName: a.firstName,
      lastName: a.lastName ?? '',
      address1: a.address1,
      address2: a.address2 ?? '',
      city: a.city,
      zipCode: a.zipCode ?? '',
      phone: a.phone ?? '',
      label: a.label ?? '',
      isDefault: a.isDefault,
      countryName: a.countryName,
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
  const data = event.values as AddressForm
  const request: AddressRequest = {
    userId: data.userId,
    addressType: data.addressType,
    firstName: data.firstName,
    lastName: data.lastName || undefined,
    address1: data.address1,
    address2: data.address2 || undefined,
    city: data.city,
    zipCode: data.zipCode || undefined,
    phone: data.phone || undefined,
    label: data.label || undefined,
    isDefault: data.isDefault,
    countryName: data.countryName,
  }

  const result = isEdit.value
    ? await AddressApi.updateAddress(route.params.id as string, request)
    : await AddressApi.createAddress(request)

  submitting.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Address updated' : 'Address created')
    router.push('/profile/addresses')
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/profile/addresses')
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
        <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="submitting" :disabled="loading" form="address-form" />
        <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel()" />
      </div>
    </div>

    <div class="flex-1 min-h-0 overflow-auto">
      <Card>
        <template #content>
          <div v-if="loading" class="flex items-center gap-2 text-muted-color">
            <i class="pi pi-spin pi-spinner" />
            Loading address...
          </div>
          <Message v-else-if="error" severity="error">{{ error }}</Message>
          <Form
            v-else
            id="address-form"
            :key="String(formLoaded)"
            :resolver="resolver"
            :initial-values="form"
            class="flex flex-col gap-4"
            @submit="onSubmit"
          >
            <Message v-if="!form.userId" severity="warn" variant="simple">
              No user is currently selected. Open this page with a userId query parameter or sign in to save an address.
            </Message>
            <FormField v-slot="$field" name="userId" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">User <span class="text-red-500">*</span></label>
              <InputText fluid readonly />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField v-slot="$field" name="addressType" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Address Type</label>
              <Select :options="addressTypeOptions" fluid />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <FormField v-slot="$field" name="firstName" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">First Name <span class="text-red-500">*</span></label>
                <InputText fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="lastName" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Last Name</label>
                <InputText fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
            </div>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <FormField v-slot="$field" name="address1" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Address Line 1 <span class="text-red-500">*</span></label>
                <InputText fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="address2" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Address Line 2</label>
                <InputText fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
            </div>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <FormField v-slot="$field" name="city" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">City <span class="text-red-500">*</span></label>
                <InputText fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="countryName" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Country <span class="text-red-500">*</span></label>
                <InputText fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
            </div>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <FormField v-slot="$field" name="zipCode" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Zip Code</label>
                <InputText fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="phone" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Phone</label>
                <InputText fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
            </div>
            <FormField v-slot="$field" name="label" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Label</label>
              <InputText fluid />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField name="isDefault" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Default Address</label>
              <ToggleSwitch />
            </FormField>
          </Form>
        </template>
      </Card>
    </div>
  </div>
</template>
