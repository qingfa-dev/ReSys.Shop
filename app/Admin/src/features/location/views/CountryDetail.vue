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
import { CountryApi } from '../services/countryApi'
import { countrySchema, countryName, countryIsoCode, countryCallingCode } from '../validations/country'
import type { CountryForm } from '../validations/country'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => isEdit.value ? 'Edit Country' : 'New Country')
const pageDescription = computed(() =>
  isEdit.value
    ? 'Edit the details of the country.'
    : 'Create a new country by filling out the form below.',
)

const form = ref<CountryForm>({
  name: '',
  isoCode: '',
  callingCode: '',
  statesRequired: false,
  isActive: true,
})

const countryResolver = zodResolver(countrySchema)
const nameResolver = zodResolver(countryName)
const isoCodeResolver = zodResolver(countryIsoCode)
const callingCodeResolver = zodResolver(countryCallingCode)
const loading = ref(false)
const formLoaded = ref(!isEdit.value)

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
      formLoaded.value = true
    } else {
      handleResult(result)
      router.push('/location/countries')
    }
  }
})

async function onSubmit(event: FormSubmitEvent) {
  if (!event.valid) return

  loading.value = true
  // Map: Shape the resolved form values into the create/update request.
  const data = event.values as CountryForm
  const request = {
    name: data.name,
    isoCode: data.isoCode,
    callingCode: data.callingCode || null,
    statesRequired: data.statesRequired,
    isActive: data.isActive,
  }

  // Call: Create or update the country record.
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
  <div class="flex flex-col h-full p-4">
    <!-- Section: Page Header — title and save/cancel controls -->
    <div class="flex-none flex justify-between items-start gap-4 mb-4">
      <div>
        <div class="font-semibold text-xl">{{ pageTitle }}</div>
        <p v-if="pageDescription" class="text-muted-color mt-1">{{ pageDescription }}</p>
      </div>
      <div class="flex items-center gap-2 shrink-0">
        <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="loading" form="country-form" />
        <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel()" />
      </div>
    </div>

    <!-- Section: Content Card — scrolling area hosting the country form -->
    <div class="flex-1 min-h-0 overflow-auto">
      <Card>
        <!-- Section: Form Fields — country identity, state, and activation inputs -->
        <template #content>
          <Form id="country-form" :key="String(formLoaded)" :resolver="countryResolver" :initial-values="form" class="flex flex-col gap-4" @submit="onSubmit">
            <FormField v-slot="$field" name="name" :resolver="nameResolver" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Name <span class="text-red-500">*</span></label>
              <InputText fluid />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField v-slot="$field" name="isoCode" :resolver="isoCodeResolver" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">ISO Code <span class="text-red-500">*</span></label>
              <InputText fluid maxlength="3" />
              <small class="text-muted-color">2-3 uppercase letters (e.g. US, VN)</small>
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField v-slot="$field" name="callingCode" :resolver="callingCodeResolver" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">Calling Code</label>
              <InputText fluid maxlength="10" />
              <small class="text-muted-color">Optional (e.g. +1, +84)</small>
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField name="statesRequired" class="flex flex-col gap-1">
              <label class="text-surface-900 dark:text-surface-0 font-medium">States Required</label>
              <ToggleSwitch />
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
