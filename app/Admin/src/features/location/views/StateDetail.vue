<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { PageShell } from '@panel'
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
const pageTitle = computed(() => (isEdit.value ? 'Edit State' : 'New State'))
const pageDescription = computed(() =>
  isEdit.value
    ? 'Edit the details of the state.'
    : 'Create a new state by filling out the form below.',
)

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
  <PageShell :title="pageTitle" :description="pageDescription">
    <!-- Page actions -->
    <div class="flex justify-end gap-2 mb-8">
      <Button label="Save" icon="pi pi-check" severity="primary" @click="onSave()" />
      <Button label="Cancel" icon="pi pi-times" severity="secondary" @click="onCancel()" />
    </div>

    <!-- Form section -->
    <FormSection title="State Details">
      <FormField label="Name" :required="true" :invalid="!!fieldErrors.name" class="mb-4">
        <InputText v-model="form.name" fluid />
        <small v-if="fieldErrors.name" class="text-red-500">{{ fieldErrors.name }}</small>
      </FormField>
      <FormField label="Abbreviation" :required="true" :invalid="!!fieldErrors.abbreviation" help-text="Short code (e.g. CA, NY, TX)" class="mb-4">
        <InputText v-model="form.abbreviation" fluid maxlength="10" />
        <small v-if="fieldErrors.abbreviation" class="text-red-500">{{ fieldErrors.abbreviation }}</small>
      </FormField>
      <FormField label="Country" :required="true" :invalid="!!fieldErrors.countryId" class="mb-4">
        <Select
          v-model="form.countryId"
          :options="countryStore.activeCountries"
          option-label="name"
          option-value="id"
          placeholder="Select a country"
          fluid
        />
        <small v-if="fieldErrors.countryId" class="text-red-500">{{ fieldErrors.countryId }}</small>
      </FormField>
      <FormField label="Active">
        <ToggleSwitch v-model="form.isActive" />
      </FormField>
    </FormSection>
  </PageShell>
</template>
