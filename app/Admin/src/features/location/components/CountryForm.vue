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
import { useCountry } from '../composables/useCountry'
import { CountryForms } from '../schemas'
import { CountryFormMapper } from '../mappers/country.mapper'
import { ROUTE } from '../routes'

const { id, mode, route, router, toast, api } = useCountry()
const { t } = useI18n()

const schemas = new CountryForms(t)
const { handleSubmit, defineField, errors, setValues } = useForm({
  validationSchema: toTypedSchema(
    mode.value === 'create' ? schemas.create() : schemas.update(),
  ),
})

const [name] = defineField('name')
const [isoCode] = defineField('isoCode')
const [iso3Code] = defineField('iso3Code')
const [numericCode] = defineField('numericCode')
const [phoneCode] = defineField('phoneCode')
const [isActive] = defineField('isActive')

const loading = ref(false)
const saving = ref(false)
const loadError = ref<string | null>(null)

const title = computed(() => {
  if (mode.value === 'create') return 'Create Country'
  if (mode.value === 'edit') return `Edit: ${name.value || ''}`
  return name.value || 'Country Details'
})

async function loadCountry() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  const result = await api.get(id.value)
  if (result.isSuccess) {
    setValues({
      name: result.value.name,
      isoCode: result.value.isoCode,
      iso3Code: result.value.iso3Code ?? undefined,
      numericCode: result.value.numericCode ?? undefined,
      phoneCode: result.value.phoneCode ?? undefined,
      isActive: result.value.isActive ?? undefined,
    })
  } else {
    loadError.value = result.message ?? 'Failed to load country'
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = mode.value === 'create'
    ? CountryFormMapper.toCreate(values)
    : CountryFormMapper.toUpdate(values)
  const result = id.value
    ? await api.update(id.value, data)
    : await api.create(data)
  saving.value = false
  if (result.isSuccess) {
    toast.success(id.value ? 'Country updated successfully' : 'Country created successfully')
    router.replace({ name: ROUTE.COUNTRIES.VIEW, params: { id: result.value.id } })
  } else {
    toast.error(result.message ?? 'Save failed')
  }
})

function cancel() {
  if (id.value) router.push({ name: ROUTE.COUNTRIES.VIEW, params: { id: id.value } })
  else router.push({ name: ROUTE.COUNTRIES.LIST })
}

function toggleEdit() {
  router.push({ name: ROUTE.COUNTRIES.EDIT, params: { id: id.value } })
}

onMounted(loadCountry)
</script>

<template>
  <div>
    <PageHeader :title="title" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <button v-if="mode === 'view'" class="p-button p-component" @click="toggleEdit">Edit</button>
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="5" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadCountry" />
    <div v-else class="card">
      <div class="grid">
        <div class="col-6">
          <FormField label="Name" :error="errors.name" required>
            <input v-model="name" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-3">
          <FormField label="ISO Code" :error="errors.isoCode" required>
            <input v-model="isoCode" type="text" class="p-inputtext p-component w-full" maxlength="2" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-3">
          <FormField label="ISO 3 Code" :error="errors.iso3Code">
            <input v-model="iso3Code" type="text" class="p-inputtext p-component w-full" maxlength="3" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-4">
          <FormField label="Numeric Code" :error="errors.numericCode">
            <input v-model="numericCode" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-4">
          <FormField label="Phone Code" :error="errors.phoneCode">
            <input v-model="phoneCode" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-4">
          <FormField label="Active">
            <div class="flex align-items-center gap-2 mt-1">
              <InputSwitch v-model="isActive" :disabled="mode === 'view'" input-id="isActive" />
              <label for="isActive">{{ isActive ? 'Active' : 'Inactive' }}</label>
            </div>
          </FormField>
        </div>
      </div>
      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        save-label="Save Country"
        cancel-label="Cancel"
        @save="save"
        @cancel="cancel"
      />
    </div>
  </div>
</template>
