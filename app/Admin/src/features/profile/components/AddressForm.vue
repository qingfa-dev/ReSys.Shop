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
import { AppCard } from '@/shared/components'
import Button from 'primevue/button'
import InputSwitch from 'primevue/inputswitch'
import { useAddress } from '../composables/useAddress'
import { ROUTE } from '../routes'
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
  if (mode.value === 'create') return t('profile.addresses.form.create_title')
  if (mode.value === 'edit') return t('profile.addresses.form.edit_title', { name: address1.value || '' })
  return address1.value || t('profile.addresses.form.view_title')
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
    loadError.value = result.message ?? t('profile.addresses.messages.load_failed')
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
    toast.success(id.value
      ? t('profile.addresses.messages.update_success')
      : t('profile.addresses.messages.create_success'))
    router.push({ name: ROUTE.ADDRESSES.LIST })
  } else {
    toast.error(result.message ?? t('profile.addresses.messages.save_failed'))
  }
})

function cancel() {
  router.push({ name: ROUTE.ADDRESSES.LIST })
}

function toggleEdit() {
  router.push({ name: ROUTE.ADDRESSES.EDIT, params: { id: id.value } })
}

onMounted(loadAddress)
</script>

<template>
  <div>
    <PageHeader
      :title="title"
      :subtitle="t('profile.addresses.form.subtitle')"
      :icon="route.meta?.icon as string | undefined"
    >
      <template #actions>
        <Button
          v-if="mode === 'view'"
          :label="t('profile.addresses.actions.edit')"
          icon="pi pi-pencil"
          size="small"
          @click="toggleEdit"
        />
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadAddress" />
    <AppCard v-else>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('profile.addresses.labels.first_name')" :error="errors.firstName" required>
            <input v-model="firstName" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('profile.addresses.labels.last_name')" :error="errors.lastName" required>
            <input v-model="lastName" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('profile.addresses.labels.address1')" :error="errors.address1" required>
            <input v-model="address1" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('profile.addresses.labels.address2')" :error="errors.address2">
            <input v-model="address2" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6 lg:col-span-4">
          <FormField :label="t('profile.addresses.labels.city')" :error="errors.city" required>
            <input v-model="city" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6 lg:col-span-4">
          <FormField :label="t('profile.addresses.labels.state')" :error="errors.state">
            <input v-model="state" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6 lg:col-span-4">
          <FormField :label="t('profile.addresses.labels.postal_code')" :error="errors.postalCode" required>
            <input v-model="postalCode" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('profile.addresses.labels.country')" :error="errors.country" required>
            <input v-model="country" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('profile.addresses.labels.phone')" :error="errors.phone">
            <input v-model="phone" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('profile.addresses.labels.default')">
            <div class="flex items-center gap-2 mt-1">
              <InputSwitch v-model="isDefault" :disabled="mode === 'view'" input-id="isDefault" />
              <label for="isDefault">{{ isDefault ? t('profile.addresses.labels.yes') : t('profile.addresses.labels.no') }}</label>
            </div>
          </FormField>
        </div>
      </div>
      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        :save-label="mode === 'create' ? t('profile.addresses.actions.save_create') : t('profile.addresses.actions.save_edit')"
        :cancel-label="t('profile.addresses.actions.cancel')"
        @save="save"
        @cancel="cancel"
      />
    </AppCard>
  </div>
</template>
