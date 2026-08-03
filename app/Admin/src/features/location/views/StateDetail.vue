<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Card from 'primevue/card'
import Message from 'primevue/message'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { StateApi } from '../services/stateApi'
import { useActiveCountries } from '../composables/useActiveCountries'
import { stateSchema, stateName, stateAbbreviation, stateCountryId } from '../validations/state'
import type { StateForm } from '../validations/state'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()
const { items: activeCountries, load: loadActiveCountries } = useActiveCountries()

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

const stateResolver = zodResolver(stateSchema)
const nameResolver = zodResolver(stateName)
const abbreviationResolver = zodResolver(stateAbbreviation)
const countryIdResolver = zodResolver(stateCountryId)
const loading = ref(false)
const formLoaded = ref(!isEdit.value)

onMounted(async () => {
  // Await: Country options for the country field Select
  loadActiveCountries()

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
      formLoaded.value = true
    } else {
      handleResult(result)
      router.push('/location/states')
    }
  }
})

async function onSubmit(event: FormSubmitEvent) {
  if (!event.valid) return

  loading.value = true
  const data = event.values as StateForm
  const request = {
    name: data.name,
    abbreviation: data.abbreviation,
    countryId: data.countryId,
    isActive: data.isActive,
  }

  // Call: Create or update; branching saves one round trip on edit.
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
  <div class="flex flex-col h-full p-4">
    <!-- Section: Page Header — title and save/cancel controls -->
    <div class="flex-none flex justify-between items-start gap-4 mb-4">
      <div>
        <div class="font-semibold text-xl">{{ pageTitle }}</div>
        <p v-if="pageDescription" class="text-muted-color mt-1">{{ pageDescription }}</p>
      </div>
      <div class="flex items-center gap-2 shrink-0">
        <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="loading" form="state-form" />
        <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel()" />
      </div>
    </div>

    <!-- Section: Content Card — scrolling area hosting the state form -->
    <div class="flex-1 min-h-0 overflow-auto">
      <Card>
        <!-- Section: Form Fields — state identity, country, and activation inputs -->
        <template #content>
          <Form id="state-form" :key="String(formLoaded)" :resolver="stateResolver" :initial-values="form" class="flex flex-col gap-4" @submit="onSubmit">
              <FormField v-slot="$field" name="name" :resolver="nameResolver" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Name <span class="text-red-500">*</span></label>
                <InputText fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="abbreviation" :resolver="abbreviationResolver" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Abbreviation <span class="text-red-500">*</span></label>
                <InputText fluid maxlength="10" />
                <small class="text-muted-color">Short code (e.g. CA, NY, TX)</small>
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="countryId" :resolver="countryIdResolver" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Country <span class="text-red-500">*</span></label>
                <Select :options="activeCountries" option-label="name" option-value="id" placeholder="Select a country" fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField name="isActive" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Active</label>
                <ToggleSwitch />
              </FormField>
            </Form>
        </template>
      </Card>
    </div>
  </div>
</template>
