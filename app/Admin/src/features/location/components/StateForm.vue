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
import Select from 'primevue/select'
import { useState } from '../composables/useState'
import { StateForms } from '../schemas'
import { StateFormMapper } from '../mappers/state.mapper'
import { CountryApi } from '../api'
import { ROUTE } from '../routes'
import type { CountryResponse } from '../types'

const { id, mode, route, router, toast, api } = useState()
const { t } = useI18n()

const countries = ref<CountryResponse[]>([])
const countriesLoading = ref(false)

const schemas = new StateForms(t)
const { handleSubmit, defineField, errors, setValues } = useForm({
  validationSchema: toTypedSchema(
    mode.value === 'create' ? schemas.create() : schemas.update(),
  ),
})

const [name] = defineField('name')
const [isoCode] = defineField('isoCode')
const [countryId] = defineField('countryId')
const [isActive] = defineField('isActive')

const loading = ref(false)
const saving = ref(false)
const loadError = ref<string | null>(null)

const title = computed(() => {
  if (mode.value === 'create') return 'Create State/Province'
  if (mode.value === 'edit') return `Edit: ${name.value || ''}`
  return name.value || 'State/Province Details'
})

async function loadCountries() {
  countriesLoading.value = true
  const result = await CountryApi.getMany({ page: 1, pageSize: 500 })
  if (result.isSuccess) {
    countries.value = result.items ?? []
  }
  countriesLoading.value = false
}

async function loadState() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  const result = await api.get(id.value)
  if (result.isSuccess) {
    setValues({
      name: result.value.name,
      isoCode: result.value.isoCode,
      countryId: result.value.countryId,
      isActive: result.value.isActive ?? undefined,
    })
  } else {
    loadError.value = result.message ?? 'Failed to load state/province'
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = mode.value === 'create'
    ? StateFormMapper.toCreate(values)
    : StateFormMapper.toUpdate(values)
  const result = id.value
    ? await api.update(id.value, data)
    : await api.create(data)
  saving.value = false
  if (result.isSuccess) {
    toast.success(id.value ? 'State updated successfully' : 'State created successfully')
    router.replace({ name: ROUTE.STATES.VIEW, params: { id: result.value.id } })
  } else {
    toast.error(result.message ?? 'Save failed')
  }
})

function cancel() {
  if (id.value) router.push({ name: ROUTE.STATES.VIEW, params: { id: id.value } })
  else router.push({ name: ROUTE.STATES.LIST })
}

function toggleEdit() {
  router.push({ name: ROUTE.STATES.EDIT, params: { id: id.value } })
}

onMounted(async () => {
  await loadCountries()
  await loadState()
})
</script>

<template>
  <div>
    <PageHeader :title="title" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <button v-if="mode === 'view'" class="p-button p-component" @click="toggleEdit">Edit</button>
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="5" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadState" />
    <div v-else class="card">
      <div class="grid">
        <div class="col-6">
          <FormField label="Name" :error="errors.name" required>
            <input v-model="name" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-3">
          <FormField label="ISO Code" :error="errors.isoCode" required>
            <input v-model="isoCode" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-3">
          <FormField label="Country" :error="errors.countryId" required>
            <Select
              v-model="countryId"
              :options="countries"
              option-value="id"
              option-label="name"
              :loading="countriesLoading"
              :disabled="mode === 'view'"
              placeholder="Select country"
              class="w-full"
            />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-6">
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
        save-label="Save State/Province"
        cancel-label="Cancel"
        @save="save"
        @cancel="cancel"
      />
    </div>
  </div>
</template>
