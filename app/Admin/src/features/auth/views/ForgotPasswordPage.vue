<script setup lang="ts">
import { ref } from 'vue'
import FloatLabel from 'primevue/floatlabel'
import Message from 'primevue/message'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import { forgotPasswordSchema } from '../validations/auth'
import { forgotPassword } from '../services/authApi'

const form = ref({ email: '' })
const forgotResolver = zodResolver(forgotPasswordSchema)
const isSubmitting = ref(false)
const isSuccess = ref(false)
const submitError = ref<string | null>(null)

async function onSubmit(event: FormSubmitEvent) {
  if (!event.valid) return
  isSubmitting.value = true
  submitError.value = null
  try {
    const data = event.values as { email: string }
    await forgotPassword({ email: data.email })
    isSuccess.value = true
  } catch {
    submitError.value = 'Something went wrong. Please try again.'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <p v-if="isSuccess" class="text-green-600 font-medium text-center">
    If an account exists with that email, a reset link has been sent.
  </p>

  <div v-else>
    <Form :resolver="forgotResolver" :initial-values="form" class="flex flex-col gap-4 w-full md:w-120" @submit="onSubmit">
      <FormField v-slot="$field" name="email" class="flex flex-col gap-1">
        <FloatLabel variant="on">
          <InputText id="email" type="email" fluid size="large" autocomplete="email" />
          <label for="email">Email</label>
        </FloatLabel>
        <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
      </FormField>
      <Message v-if="submitError" severity="error" :closable="false">{{ submitError }}</Message>
      <Button type="submit" label="Send Reset Link" fluid size="large" :loading="isSubmitting" />
    </Form>
  </div>

  <router-link to="/auth/login" class="text-sm text-primary hover:underline text-center block mt-4">
    &larr; Back to login
  </router-link>
</template>
