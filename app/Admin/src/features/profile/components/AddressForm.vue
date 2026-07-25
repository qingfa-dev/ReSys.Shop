<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import FormField from '@/shared/components/forms/FormField.vue'
import FormActions from '@/shared/components/forms/FormActions.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import InputSwitch from 'primevue/inputswitch'
import { useAddress } from '../composables/useAddress'
import { AddressForms } from '../schemas'
import { AddressFormMapper } from '../mappers/address.mapper'

const { id, mode, route, router, toast, api } = useAddress()
const { t } = useI18n()

const schemas = new AddressForms(t)
const { handleSubmit, defineField, errors, setValues } = useForm({
  validationSchema: toTypedSchema(
    mode.value === 'create' ? schemas.create() : schemas.update(),
  ),
})

const [firstName] = defineField('firstName')
const [lastName] = defineField('lastName')
const [address1] = defineField('address1')
const [address2] = defineField('address2')
const [city] = defineField('city')
const [state] = defineField('state')
const [postalCode] = defineField('postalCode')
const [country] = defineField('country')
const [phone] = defineField('phone')
const [isDefault] = defineField('isDefault')

const loading = ref(false)
const saving = ref(false)
const loadError = ref<string | null>(null)

const title = computed(() => {
  if (mode.value === 'create') return 'Create Address'
  if (mode.value === 'edit') return `Edit Address: ${address1.value || ''}`
  return address1.value || 'Address Details'
})

async function loadAddress() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  const result = await api.get(id.value)
  if (result.isSuccess) {
    setValues({
      firstName: result.value.firstName,
      lastName: result.value.lastName,
      address1: result.value.address1,
      address2: result.value.address2 ?? undefined,
      city: result.value.city,
      state: result.value.state ?? undefined,
      postalCode: result.value.postalCode,
      country: result.value.country,
      phone: result.value.phone ?? undefined,
      isDefault: result.value.isDefault ?? undefined,
    })
  } else {
    loadError.value = result.message ?? 'Failed to load address'
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = mode.value === 'create'
    ? AddressFormMapper.toCreate(values)
    : AddressFormMapper.toUpdate(values)
  const result = id.value
    ? await api.update(id.value, data)
    : await api.create(data)
  saving.value = false
  if (result.isSuccess) {
    toast.success(id.value ? 'Address updated successfully' : 'Address created successfully')
    router.push({ name: 'profile.addresses' })
  } else {
    toast.error(result.message ?? 'Save failed')
  }
})

function cancel() {
  router.push({ name: 'profile.addresses' })
}

function toggleEdit() {
  router.push({ name: 'profile.addresses.edit', params: { id: id.value } })
}

onMounted(loadAddress)
</script>

<template>
  <div>
    <PageHeader :title="title" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <button v-if="mode === 'view'" class="p-button p-component" @click="toggleEdit">Edit</button>
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="6" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadAddress" />
    <div v-else class="card">
      <div class="grid">
        <div class="col-6">
          <FormField label="First Name" :error="errors.firstName" required>
            <input v-model="firstName" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Last Name" :error="errors.lastName" required>
            <input v-model="lastName" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-6">
          <FormField label="Address Line 1" :error="errors.address1" required>
            <input v-model="address1" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Address Line 2" :error="errors.address2">
            <input v-model="address2" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-4">
          <FormField label="City" :error="errors.city" required>
            <input v-model="city" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-4">
          <FormField label="State / Province" :error="errors.state">
            <input v-model="state" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-4">
          <FormField label="Postal Code" :error="errors.postalCode" required>
            <input v-model="postalCode" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-6">
          <FormField label="Country" :error="errors.country" required>
            <input v-model="country" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Phone" :error="errors.phone">
            <input v-model="phone" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-6">
          <FormField label="Default Address">
            <div class="flex align-items-center gap-2 mt-1">
              <InputSwitch v-model="isDefault" :disabled="mode === 'view'" input-id="isDefault" />
              <label for="isDefault">{{ isDefault ? 'Yes' : 'No' }}</label>
            </div>
          </FormField>
        </div>
      </div>
      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        save-label="Save Address"
        cancel-label="Cancel"
        @save="save"
        @cancel="cancel"
      />
    </div>
  </div>
</template>
