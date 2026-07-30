<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { PageShell, PageHeading } from '@panel'
import { FormSection, FormField } from '@form'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { CountryApi } from '../services/countryApi'
import { countrySchema } from '../validations/country'
import type { CountryForm } from '../validations/country'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => isEdit.value ? 'Edit Country' : 'New Country')

const form = ref<CountryForm>({
  name: '',
  isoCode: '',
  callingCode: '',
  statesRequired: false,
  isActive: true,
})

const fieldErrors = ref<Record<string, string>>({})
const loading = ref(false)

onMounted(async () => {
  if (isEdit.value) {
    const result = await CountryApi.getCountry(route.params.id as string)
    if (result.isSuccess) {
      const c = result.value
      form.value = {
        name: c.name,
        isoCode: c.isoCode,
        callingCode: c.callingCode ?? '',
        statesRequired: c.statesRequired,
        isActive: c.isActive,
      }
    } else {
      handleResult(result)
      router.push('/location/countries')
    }
  }
})

function onIsoCodeInput(value: string | undefined) {
  form.value.isoCode = (value ?? '').toUpperCase()
}

async function onSave() {
  fieldErrors.value = {}
  const parsed = countrySchema.safeParse(form.value)

  if (!parsed.success) {
    for (const issue of parsed.error.issues) {
      const field = String(issue.path[0])
      if (!fieldErrors.value[field]) {
        fieldErrors.value[field] = issue.message
      }
    }
    return
  }

  loading.value = true
  const data = parsed.data
  const request = {
    name: data.name,
    isoCode: data.isoCode,
    callingCode: data.callingCode || null,
    statesRequired: data.statesRequired,
    isActive: data.isActive,
  }

  const result = isEdit.value
    ? await CountryApi.updateCountry(route.params.id as string, request)
    : await CountryApi.createCountry(request)

  loading.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Country updated' : 'Country created')
    router.push('/location/countries')
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/location/countries')
}
</script>

<template>
  <PageShell :title="pageTitle">
    <PageHeading
      title=""
      :breadcrumbs="[
        { label: 'Home', to: '/' },
        { label: 'Countries', to: '/location/countries' },
        { label: pageTitle },
      ]"
      :actions="[
        { label: 'Save', icon: 'pi pi-check', severity: 'primary' },
        { label: 'Cancel', icon: 'pi pi-times' },
      ]"
      @action="(i: number) => i === 0 ? onSave() : onCancel()"
    />
    <FormSection title="Country Details">
      <FormField label="Name" :required="true" :invalid="!!fieldErrors.name">
        <InputText v-model="form.name" fluid class="w-full" />
        <small v-if="fieldErrors.name" class="text-red-500">{{ fieldErrors.name }}</small>
      </FormField>
      <FormField label="ISO Code" :required="true" :invalid="!!fieldErrors.isoCode" help-text="2-3 uppercase letters (e.g. US, VN)">
        <InputText v-model="form.isoCode" fluid maxlength="3" class="w-full" @update:model-value="(v: string | undefined) => onIsoCodeInput(v)" />
        <small v-if="fieldErrors.isoCode" class="text-red-500">{{ fieldErrors.isoCode }}</small>
      </FormField>
      <FormField label="Calling Code" :invalid="!!fieldErrors.callingCode" help-text="Optional (e.g. +1, +84)">
        <InputText v-model="form.callingCode" fluid maxlength="10" class="w-full" />
        <small v-if="fieldErrors.callingCode" class="text-red-500">{{ fieldErrors.callingCode }}</small>
      </FormField>
      <FormField label="States Required">
        <ToggleSwitch v-model="form.statesRequired" />
      </FormField>
      <FormField label="Active">
        <ToggleSwitch v-model="form.isActive" />
      </FormField>
    </FormSection>
  </PageShell>
</template>
