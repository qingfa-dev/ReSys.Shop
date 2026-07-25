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
  if (mode.value === 'create') return t('location.countries.form.create_title')
  if (mode.value === 'edit') return t('location.countries.form.edit_title', { name: name.value || '' })
  return name.value || t('location.countries.form.view_title')
})

async function loadCountry() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  try {
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
      loadError.value = result.message ?? t('location.countries.messages.load_failed')
    }
  } catch (err) {
    console.error(err)
    loadError.value = t('location.countries.messages.load_failed')
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = mode.value === 'create'
    ? CountryFormMapper.toCreate(values)
    : CountryFormMapper.toUpdate(values)
  try {
    const result = id.value
      ? await api.update(id.value, data)
      : await api.create(data)
    saving.value = false
    if (result.isSuccess) {
      toast.success(id.value
        ? t('location.countries.messages.update_success')
        : t('location.countries.messages.create_success'))
      router.replace({ name: ROUTE.COUNTRIES.VIEW, params: { id: result.value.id } })
    } else {
      toast.error(result.message ?? t('location.countries.messages.save_failed'))
    }
  } catch (err) {
    console.error(err)
    saving.value = false
    toast.error(t('location.countries.messages.save_failed'))
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
    <PageHeader :title="title" :subtitle="t('location.countries.form.subtitle')" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <Button
          v-if="mode === 'view'"
          :label="t('location.countries.actions.edit')"
          icon="pi pi-pencil"
          size="small"
          @click="toggleEdit"
        />
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadCountry" />
    <AppCard v-else>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('location.countries.labels.name')" :error="errors.name" required>
            <input v-model="name" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-3">
          <FormField :label="t('location.countries.labels.iso_code')" :error="errors.isoCode" required>
            <input v-model="isoCode" type="text" class="p-inputtext p-component w-full" maxlength="2" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-3">
          <FormField :label="t('location.countries.labels.iso3_code')" :error="errors.iso3Code">
            <input v-model="iso3Code" type="text" class="p-inputtext p-component w-full" maxlength="3" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('location.countries.labels.numeric_code')" :error="errors.numericCode">
            <input v-model="numericCode" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('location.countries.labels.phone_code')" :error="errors.phoneCode">
            <input v-model="phoneCode" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('location.countries.labels.is_active')">
            <div class="flex items-center gap-2 mt-1">
              <Checkbox v-model="isActive" :binary="true" :disabled="mode === 'view'" input-id="isActive" />
              <label for="isActive">{{ t('location.countries.labels.active_help') }}</label>
            </div>
          </FormField>
        </div>
      </div>

      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        :save-label="mode === 'create' ? t('location.countries.actions.save_create') : t('location.countries.actions.save_edit')"
        :cancel-label="t('location.countries.actions.cancel')"
        @save="save"
        @cancel="cancel"
      />
    </AppCard>
  </div>
</template>
