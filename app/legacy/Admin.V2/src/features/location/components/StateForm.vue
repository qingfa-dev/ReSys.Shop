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
import Checkbox from 'primevue/checkbox'
import Button from 'primevue/button'
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
  if (mode.value === 'create') return t('location.states.form.create_title')
  if (mode.value === 'edit') return t('location.states.form.edit_title', { name: name.value || '' })
  return name.value || t('location.states.form.view_title')
})

async function loadCountries() {
  countriesLoading.value = true
  try {
    const result = await CountryApi.getMany({ page: 1, pageSize: 500 })
    if (result.isSuccess) {
      countries.value = result.items ?? []
    }
  } catch (err) {
    console.error(err)
  }
  countriesLoading.value = false
}

async function loadState() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  try {
    const result = await api.get(id.value)
    if (result.isSuccess) {
      setValues({
        name: result.value.name,
        isoCode: result.value.isoCode,
        countryId: result.value.countryId,
        isActive: result.value.isActive ?? undefined,
      })
    } else {
      loadError.value = result.message ?? t('location.states.messages.load_failed')
    }
  } catch (err) {
    console.error(err)
    loadError.value = t('location.states.messages.load_failed')
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = mode.value === 'create'
    ? StateFormMapper.toCreate(values)
    : StateFormMapper.toUpdate(values)
  try {
    const result = id.value
      ? await api.update(id.value, data)
      : await api.create(data)
    saving.value = false
    if (result.isSuccess) {
      toast.success(id.value
        ? t('location.states.messages.update_success')
        : t('location.states.messages.create_success'))
      router.replace({ name: ROUTE.STATES.VIEW, params: { id: result.value.id } })
    } else {
      toast.error(result.message ?? t('location.states.messages.save_failed'))
    }
  } catch (err) {
    console.error(err)
    saving.value = false
    toast.error(t('location.states.messages.save_failed'))
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
    <PageHeader :title="title" :subtitle="t('location.states.form.subtitle')" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <Button
          v-if="mode === 'view'"
          :label="t('location.states.actions.edit')"
          icon="pi pi-pencil"
          size="small"
          @click="toggleEdit"
        />
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadState" />
    <AppCard v-else>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('location.states.labels.name')" :error="errors.name" required>
            <input v-model="name" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-3">
          <FormField :label="t('location.states.labels.iso_code')" :error="errors.isoCode" required>
            <input v-model="isoCode" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-3">
          <FormField :label="t('location.states.labels.country')" :error="errors.countryId" required>
            <Select
              v-model="countryId"
              :options="countries"
              option-value="id"
              option-label="name"
              :loading="countriesLoading"
              :disabled="mode === 'view'"
              :placeholder="t('location.states.labels.select_country')"
              class="w-full"
            />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('location.states.labels.is_active')">
            <div class="flex items-center gap-2 mt-1">
              <Checkbox v-model="isActive" :binary="true" :disabled="mode === 'view'" input-id="isActive" />
              <label for="isActive">{{ t('location.states.labels.active_help') }}</label>
            </div>
          </FormField>
        </div>
      </div>

      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        :save-label="mode === 'create' ? t('location.states.actions.save_create') : t('location.states.actions.save_edit')"
        :cancel-label="t('location.states.actions.cancel')"
        @save="save"
        @cancel="cancel"
      />
    </AppCard>
  </div>
</template>
