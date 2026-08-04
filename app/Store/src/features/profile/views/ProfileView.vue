<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import { useProfileStore } from '../stores/profileStore'
import { useNotify } from '@/shared/composables/useNotify'
import { profileSchema, type ProfileFormValues } from '../validations/profile'

const store = useProfileStore()
const notify = useNotify()
const resolver = zodResolver(profileSchema)

const initialValues = ref({
  firstName: '',
  lastName: '',
  email: '',
  phoneNumber: '',
})

async function onSubmit(event: FormSubmitEvent): Promise<void> {
  if (!event.valid) return
  const values = event.values as ProfileFormValues
  const ok = await store.updateProfile({
    firstName: values.firstName,
    lastName: values.lastName,
    email: values.email,
    phoneNumber: values.phoneNumber || null,
  })
  if (ok) notify.success('Profile updated', 'Your profile has been saved.')
  else notify.error('Update failed', store.error ?? 'Unable to update your profile.')
}

async function loadProfile(): Promise<void> {
  const ok = await store.fetchProfile()
  if (ok && store.profile) {
    initialValues.value = {
      firstName: store.profile.firstName,
      lastName: store.profile.lastName,
      email: store.profile.email,
      phoneNumber: store.profile.phoneNumber ?? '',
    }
  }
}

onMounted(loadProfile)
</script>

<template>
  <div>
    <!-- Section: Page Header -->
    <div class="mb-6">
      <h1 class="text-2xl font-bold text-stone-900">Profile</h1>
      <p class="text-sm text-stone-500 mt-1">Your basic account details.</p>
    </div>

    <!-- Section: Error -->
    <Message v-if="store.error" severity="error" :closable="false" class="mb-4">
      {{ store.error }}
    </Message>

    <!-- Section: Loading -->
    <div v-if="store.loading" class="space-y-4">
      <Skeleton v-for="i in 4" :key="i" height="3rem" class="rounded-lg" />
    </div>

    <!-- Section: Profile Form -->
    <div v-else class="max-w-xl bg-white rounded-xl border border-stone-200 p-6">
      <Form :resolver="resolver" :initial-values="initialValues" class="space-y-4" @submit="onSubmit">
        <FormField v-slot="$field" name="firstName" class="flex flex-col gap-1">
          <label class="text-sm font-medium text-stone-700">First name *</label>
          <InputText type="text" fluid :invalid="$field?.invalid" />
          <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">
            {{ $field.error?.message }}
          </Message>
        </FormField>

        <FormField v-slot="$field" name="lastName" class="flex flex-col gap-1">
          <label class="text-sm font-medium text-stone-700">Last name *</label>
          <InputText type="text" fluid :invalid="$field?.invalid" />
          <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">
            {{ $field.error?.message }}
          </Message>
        </FormField>

        <FormField v-slot="$field" name="email" class="flex flex-col gap-1">
          <label class="text-sm font-medium text-stone-700">Email *</label>
          <InputText type="email" fluid :invalid="$field?.invalid" autocomplete="email" />
          <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">
            {{ $field.error?.message }}
          </Message>
        </FormField>

        <FormField v-slot="$field" name="phoneNumber" class="flex flex-col gap-1">
          <label class="text-sm font-medium text-stone-700">Phone</label>
          <InputText type="tel" fluid :invalid="$field?.invalid" placeholder="+1-555-0100" />
          <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">
            {{ $field.error?.message }}
          </Message>
        </FormField>

        <div class="flex justify-end pt-2">
          <Button label="Save Changes" type="submit" icon="pi pi-check" :loading="store.saving" />
        </div>
      </Form>
    </div>
  </div>
</template>
