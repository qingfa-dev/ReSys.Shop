<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { z } from 'zod'
import Card from 'primevue/card'
import Message from 'primevue/message'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { StateApi } from '../services/stateApi'
import { useCountryStore } from '../stores/countryStore'
import { stateSchema, stateName, stateAbbreviation, stateCountryId } from '../validations/state'
import type { StateForm } from '../validations/state'

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

const stateResolver = zodResolver(stateSchema)
const nameResolver = zodResolver(z.object({ name: stateName }))
const abbreviationResolver = zodResolver(z.object({ abbreviation: stateAbbreviation }))
const countryIdResolver = zodResolver(z.object({ countryId: stateCountryId }))
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
  <Card>
    <template #content>
      <div class="font-semibold text-xl mb-4">{{ pageTitle }}</div>
      <p v-if="pageDescription" class="text-muted-color mb-4">{{ pageDescription }}</p>

    <Card>
      <template #content>
        <div class="flex flex-col gap-6">
          <div class="font-semibold text-xl">State Details</div>
          <Form v-slot="$form" :resolver="stateResolver" :initial-values="form" class="flex flex-col gap-4" @submit="onSubmit">
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
              <Select :options="countryStore.activeCountries" option-label="name" option-value="id" placeholder="Select a country" fluid />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField v-slot="$field" name="isActive" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Active</label>
              <ToggleSwitch />
            </FormField>
            <div class="flex justify-end gap-2 pt-4 border-t border-surface">
              <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="loading" />
              <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel()" />
            </div>
          </Form>
        </div>
      </template>
    </Card>
    </template>
  </Card>
</template>
