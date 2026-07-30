<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { PageShell, PageHeading } from '@panel'
import { FormSection, FormField } from '@form'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { StateApi } from '../services/stateApi'
import { useCountryStore } from '../stores/countryStore'
import { stateSchema } from '../validations/state'
import type { StateForm } from '../validations/state'
import Select from 'primevue/select'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()
const countryStore = useCountryStore()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => isEdit.value ? 'Edit State' : 'New State')

const form = ref<StateForm>({
  name: '',
  abbreviation: '',
  countryId: '',
  isActive: true,
})

const fieldErrors = ref<Record<string, string>>({})
const loading = ref(false)

onMounted(async () => {
  countryStore.fetchActive()

  if (isEdit.value) {
    const result = await StateApi.getState(route.params.id as string)
    if (result.isSuccess) {
      const s = result.value
      form.value = {
        name: s.name,
        abbreviation: s.abbreviation,
        countryId: s.countryId,
        isActive: s.isActive,
      }
    } else {
      handleResult(result)
      router.push('/location/states')
    }
  }
})

async function onSave() {
  fieldErrors.value = {}
  const parsed = stateSchema.safeParse(form.value)

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
    abbreviation: data.abbreviation,
    countryId: data.countryId,
    isActive: data.isActive,
  }

  const result = isEdit.value
    ? await StateApi.updateState(route.params.id as string, request)
    : await StateApi.createState(request)

  loading.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'State updated' : 'State created')
    router.push('/location/states')
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/location/states')
}
</script>

<template>
  <PageShell :title="pageTitle">
    <PageHeading
      title=""
      :breadcrumbs="[
        { label: 'Home', to: '/' },
        { label: 'States', to: '/location/states' },
        { label: pageTitle },
      ]"
      :actions="[
        { label: 'Save', icon: 'pi pi-check', severity: 'primary' },
        { label: 'Cancel', icon: 'pi pi-times' },
      ]"
      @action="(i: number) => i === 0 ? onSave() : onCancel()"
    />
    <FormSection title="State Details">
      <FormField label="Name" :required="true" :invalid="!!fieldErrors.name">
        <InputText v-model="form.name" fluid class="w-full" />
        <small v-if="fieldErrors.name" class="text-red-500">{{ fieldErrors.name }}</small>
      </FormField>
      <FormField label="Abbreviation" :required="true" :invalid="!!fieldErrors.abbreviation" help-text="Short code (e.g. CA, NY, TX)">
        <InputText v-model="form.abbreviation" fluid maxlength="10" class="w-full" />
        <small v-if="fieldErrors.abbreviation" class="text-red-500">{{ fieldErrors.abbreviation }}</small>
      </FormField>
      <FormField label="Country" :required="true" :invalid="!!fieldErrors.countryId">
        <Select
          v-model="form.countryId"
          :options="countryStore.activeCountries"
          option-label="name"
          option-value="id"
          placeholder="Select a country"
          class="w-full"
        />
        <small v-if="fieldErrors.countryId" class="text-red-500">{{ fieldErrors.countryId }}</small>
      </FormField>
      <FormField label="Active">
        <ToggleSwitch v-model="form.isActive" />
      </FormField>
    </FormSection>
  </PageShell>
</template>
