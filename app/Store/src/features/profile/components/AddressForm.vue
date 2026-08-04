<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import Checkbox from 'primevue/checkbox'
import { useLocationCascade } from '@/features/location/composables/useLocationCascade'
import type { Address, AddressInput, AddressType } from '../types/address'
import { addressSchema, type AddressFormValues } from '../validations/address'

const props = defineProps<{ initial?: Address | null; submitting?: boolean }>()
const emit = defineEmits<{ submit: [payload: AddressInput]; cancel: [] }>()

// Destructure the cascade refs so they auto-unwrap in the template (cascade.countries
// would otherwise pass a Ref object to Select.options).
const { countries, states, selectedCountryId, selectedStateId, loading, loadCountries } =
  useLocationCascade()
const resolver = zodResolver(addressSchema)
const countryError = ref<string | null>(null)
const countryInitialized = ref(false)
const stateInitialized = ref(false)

const addressTypeOptions: Array<{ label: string; value: AddressType }> = [
  { label: 'Shipping', value: 'Shipping' },
  { label: 'Billing', value: 'Billing' },
  { label: 'Other', value: 'Other' },
]

// Form field initial values are read once at mount; the parent mounts a fresh instance
// (v-if + :key) when switching between add/edit, so a computed is safe here.
const initialValues = {
  firstName: props.initial?.firstName ?? '',
  lastName: props.initial?.lastName ?? '',
  address1: props.initial?.address1 ?? '',
  address2: props.initial?.address2 ?? '',
  city: props.initial?.city ?? '',
  zipCode: props.initial?.zipCode ?? '',
  phone: props.initial?.phone ?? '',
  label: props.initial?.label ?? '',
  addressType: (props.initial?.addressType ?? 'Shipping') as AddressType,
  isDefault: props.initial?.isDefault ?? false,
}

watch(
  () => [countries.value, props.initial] as const,
  ([countryList]) => {
    if (countryInitialized.value || !props.initial || countryList.length === 0) return
    const match = countryList.find(
      (c) => c.isoCode === props.initial?.countryCode || c.name === props.initial?.countryName,
    )
    if (match) {
      selectedCountryId.value = match.id
      countryInitialized.value = true
    }
  },
  { immediate: true },
)

watch(
  () => [states.value, props.initial] as const,
  ([stateList]) => {
    if (stateInitialized.value || !props.initial || stateList.length === 0) return
    const match = stateList.find(
      (s) => s.abbreviation === props.initial?.stateCode || s.name === props.initial?.stateProvince,
    )
    if (match) {
      selectedStateId.value = match.id
      stateInitialized.value = true
    }
  },
  { immediate: true },
)

onMounted(() => {
  loadCountries()
})

// Clear the manual "select a country" error as soon as a country is chosen.
watch(selectedCountryId, () => {
  if (countryError.value) countryError.value = null
})

function onSubmit(event: FormSubmitEvent): void {
  countryError.value = null
  if (!event.valid) return
  const values = event.values as AddressFormValues
  const country = countries.value.find((c) => c.id === selectedCountryId.value)
  if (!country) {
    countryError.value = 'Please select a country'
    return
  }
  const state = states.value.find((s) => s.id === selectedStateId.value)
  emit('submit', {
    addressType: values.addressType,
    firstName: values.firstName,
    lastName: values.lastName || null,
    address1: values.address1,
    address2: values.address2 || null,
    city: values.city,
    zipCode: values.zipCode || null,
    phone: values.phone || null,
    label: values.label || null,
    isDefault: values.isDefault,
    countryName: country.name,
    stateProvince: state?.name ?? null,
    countryCode: country.isoCode,
    stateCode: state?.abbreviation ?? null,
  })
}
</script>

<template>
  <div class="bg-white rounded-xl border border-gray-200 p-6">
    <h3 class="text-lg font-semibold text-gray-900 mb-4">
      {{ initial ? 'Edit Address' : 'Add New Address' }}
    </h3>
    <Form :resolver="resolver" :initial-values="initialValues" class="space-y-4" @submit="onSubmit">
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <FormField v-slot="$field" name="firstName" class="flex flex-col gap-1">
          <label class="text-sm font-medium text-gray-700">First name *</label>
          <InputText type="text" fluid :invalid="$field?.invalid" placeholder="Jane" />
          <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">
            {{ $field.error?.message }}
          </Message>
        </FormField>
        <FormField v-slot="$field" name="lastName" class="flex flex-col gap-1">
          <label class="text-sm font-medium text-gray-700">Last name</label>
          <InputText type="text" fluid :invalid="$field?.invalid" placeholder="Smith" />
          <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">
            {{ $field.error?.message }}
          </Message>
        </FormField>
      </div>

      <FormField v-slot="$field" name="address1" class="flex flex-col gap-1">
        <label class="text-sm font-medium text-gray-700">Street address *</label>
        <InputText type="text" fluid :invalid="$field?.invalid" placeholder="123 Main Street" />
        <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">
          {{ $field.error?.message }}
        </Message>
      </FormField>

      <FormField v-slot="$field" name="address2" class="flex flex-col gap-1">
        <label class="text-sm font-medium text-gray-700">Apartment, suite, etc.</label>
        <InputText type="text" fluid :invalid="$field?.invalid" placeholder="Suite 400" />
        <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">
          {{ $field.error?.message }}
        </Message>
      </FormField>

      <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <FormField v-slot="$field" name="city" class="flex flex-col gap-1">
          <label class="text-sm font-medium text-gray-700">City *</label>
          <InputText type="text" fluid :invalid="$field?.invalid" placeholder="New York" />
          <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">
            {{ $field.error?.message }}
          </Message>
        </FormField>
        <FormField v-slot="$field" name="zipCode" class="flex flex-col gap-1">
          <label class="text-sm font-medium text-gray-700">ZIP / Postal code</label>
          <InputText type="text" fluid :invalid="$field?.invalid" placeholder="10001" />
          <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">
            {{ $field.error?.message }}
          </Message>
        </FormField>
        <FormField v-slot="$field" name="phone" class="flex flex-col gap-1">
          <label class="text-sm font-medium text-gray-700">Phone</label>
          <InputText type="tel" fluid :invalid="$field?.invalid" placeholder="+1-555-0100" />
          <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">
            {{ $field.error?.message }}
          </Message>
        </FormField>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <!-- Country cascade select (managed by useLocationCascade; validated manually). -->
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-gray-700">Country *</label>
          <Select
            v-model="selectedCountryId"
            :options="countries"
            option-label="name"
            option-value="id"
            :loading="loading"
            placeholder="Select country"
            fluid
            :invalid="!!countryError"
          />
        </div>
        <!-- State/province cascade select (filtered client-side from cached states). -->
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-gray-700">State / Province</label>
          <Select
            v-model="selectedStateId"
            :options="states"
            option-label="name"
            option-value="id"
            :loading="loading"
            placeholder="Select state"
            fluid
          />
        </div>
      </div>
      <Message v-if="countryError" severity="error" size="small" variant="simple">{{ countryError }}</Message>

      <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <FormField v-slot="$field" name="addressType" class="flex flex-col gap-1">
          <label class="text-sm font-medium text-gray-700">Type</label>
          <Select
            :options="addressTypeOptions"
            option-label="label"
            option-value="value"
            placeholder="Type"
            fluid
            :invalid="$field?.invalid"
          />
          <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">
            {{ $field.error?.message }}
          </Message>
        </FormField>
        <FormField v-slot="$field" name="label" class="flex flex-col gap-1">
          <label class="text-sm font-medium text-gray-700">Label</label>
          <InputText type="text" fluid :invalid="$field?.invalid" placeholder="Home" />
          <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">
            {{ $field.error?.message }}
          </Message>
        </FormField>
      </div>

      <FormField v-slot="$field" name="isDefault" class="flex items-center gap-2">
        <Checkbox binary :invalid="$field?.invalid" />
        <label class="text-sm text-gray-700">Set as default address</label>
      </FormField>

      <div class="flex justify-end gap-3 pt-2">
        <Button label="Cancel" severity="secondary" outlined type="button" @click="emit('cancel')" />
        <Button label="Save Address" type="submit" icon="pi pi-check" :loading="submitting" />
      </div>
    </Form>
  </div>
</template>
