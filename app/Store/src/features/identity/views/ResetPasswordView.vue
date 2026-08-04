<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import FloatLabel from 'primevue/floatlabel'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import InputPassword from 'primevue/inputpassword'
import Message from 'primevue/message'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import Eye from '@primeicons/vue/eye'
import EyeSlash from '@primeicons/vue/eye-slash'
import { useNotify } from '@/shared/composables/useNotify'
import * as authApi from '../services/authApi'
import { resetPasswordSchema } from '../validations/password'

const route = useRoute()
const router = useRouter()
const notify = useNotify()

// Token: Read from the emailed reset link query parameter (not a form field).
const token = typeof route.query.token === 'string' ? route.query.token : ''

const form = ref({ newPassword: '', confirmPassword: '' })
const resetResolver = zodResolver(resetPasswordSchema)
const isSubmitting = ref(false)
const formError = ref<string | null>(null)
const mask = ref(true)
const confirmMask = ref(true)

async function onSubmit(event: FormSubmitEvent) {
  // Validate: Return early when zod form validation fails.
  if (!event.valid) return
  if (!token) {
    formError.value = 'Invalid or expired reset link'
    return
  }
  isSubmitting.value = true
  formError.value = null
  try {
    const data = event.values as { newPassword: string; confirmPassword: string }
    const result = await authApi.resetPassword(token, data.newPassword)
    if (result.isSuccess) {
      notify.success('Password reset', 'You can now sign in with your new password')
      router.replace('/login')
    } else {
      formError.value = 'Invalid or expired reset link'
    }
  } catch {
    formError.value = 'Invalid or expired reset link'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div>
    <h2 class="text-2xl font-bold text-center text-gray-900 mb-6">Set a new password</h2>
    <p class="text-sm text-gray-600 text-center mb-6">Choose a new password for your account.</p>

    <Form :resolver="resetResolver" :initial-values="form" class="flex flex-col gap-4" @submit="onSubmit">
      <FormField v-slot="$field" name="newPassword" class="flex flex-col gap-1">
        <FloatLabel variant="on">
          <IconField>
            <InputPassword id="newPassword" placeholder="New password" :mask="mask" fluid size="large" :feedback="false" autocomplete="new-password" />
            <InputIcon class="cursor-pointer" @click="mask = !mask">
              <Eye v-if="mask" />
              <EyeSlash v-else />
            </InputIcon>
          </IconField>
          <label for="newPassword">New Password</label>
        </FloatLabel>
        <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
      </FormField>

      <FormField v-slot="$field" name="confirmPassword" class="flex flex-col gap-1">
        <FloatLabel variant="on">
          <IconField>
            <InputPassword id="confirmPassword" placeholder="Confirm new password" :mask="confirmMask" fluid size="large" :feedback="false" autocomplete="new-password" />
            <InputIcon class="cursor-pointer" @click="confirmMask = !confirmMask">
              <Eye v-if="confirmMask" />
              <EyeSlash v-else />
            </InputIcon>
          </IconField>
          <label for="confirmPassword">Confirm Password</label>
        </FloatLabel>
        <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
      </FormField>

      <Message v-if="formError" severity="error" :closable="false">{{ formError }}</Message>
      <Button type="submit" label="Reset Password" fluid size="large" :loading="isSubmitting" />
    </Form>

    <router-link to="/login" class="text-sm text-primary hover:underline text-center block mt-4">
      &larr; Back to login
    </router-link>
  </div>
</template>
