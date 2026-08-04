<script setup lang="ts">
import { ref } from 'vue'
import Button from 'primevue/button'
import FloatLabel from 'primevue/floatlabel'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import * as authApi from '../services/authApi'
import { forgotPasswordSchema } from '../validations/password'

const form = ref({ email: '' })
const forgotResolver = zodResolver(forgotPasswordSchema)
const isSubmitting = ref(false)
const isSuccess = ref(false)
const submitError = ref<string | null>(null)

async function onSubmit(event: FormSubmitEvent) {
  // Validate: Return early when zod form validation fails.
  if (!event.valid) return
  isSubmitting.value = true
  submitError.value = null
  try {
    const data = event.values as { email: string }
    await authApi.forgotPassword(data.email)
    // Security: Show the same confirmation regardless of whether the account exists.
    isSuccess.value = true
  } catch {
    submitError.value = 'Something went wrong. Please try again.'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div>
    <h2 class="text-2xl font-bold text-center text-gray-900 mb-6">Reset your password</h2>

    <!-- Section: Success State — confirmation shown once the reset link is sent -->
    <template v-if="isSuccess">
      <Message severity="success" :closable="false" class="w-full">
        If an account exists, a reset link has been sent.
      </Message>
      <router-link to="/login" class="text-sm text-primary hover:underline text-center block mt-4">
        &larr; Back to login
      </router-link>
    </template>

    <template v-else>
      <p class="text-sm text-gray-600 text-center mb-6">
        Enter your email and we'll send you a link to reset your password.
      </p>
      <Form :resolver="forgotResolver" :initial-values="form" class="flex flex-col gap-4" @submit="onSubmit">
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
      <router-link to="/login" class="text-sm text-primary hover:underline text-center block mt-4">
        &larr; Back to login
      </router-link>
    </template>
  </div>
</template>
