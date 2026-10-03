<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import FloatLabel from 'primevue/floatlabel'
import Message from 'primevue/message'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import Eye from '@primeicons/vue/eye'
import EyeSlash from '@primeicons/vue/eye-slash'
import { useNotify } from '@/shared/composables/useNotify'
import { resetPasswordSchema } from '../validations/auth'
import { resetPassword } from '../services/authApi'

const route = useRoute()
const router = useRouter()
const notify = useNotify()

const form = ref({
  email: '',
  userId: '',
  token: '',
  newPassword: '',
})
const resetResolver = zodResolver(resetPasswordSchema)
const isSubmitting = ref(false)
const formError = ref<string | null>(null)
const mask = ref(true)

onMounted(() => {
  // Transform: Pre-fill the disabled fields from the emailed reset-link query params.
  const q = route.query as Record<string, string>
  form.value = {
    email: q.email ?? '',
    userId: q.userId ?? '',
    token: q.token ?? '',
    newPassword: '',
  }
})

async function onSubmit(event: FormSubmitEvent) {
  // Validate: Return early when zod form validation fails.
  if (!event.valid) return
  isSubmitting.value = true
  formError.value = null
  try {
    const data = event.values as { email: string; userId: string; token: string; newPassword: string }
    // Call: Confirm the reset token is valid and swap in the new password.
    const result = await resetPassword({
      email: data.email,
      userId: data.userId,
      token: data.token,
      newPassword: data.newPassword,
    })
    if (result.isSuccess) {
      notify.success('Password reset successful')
      router.push('/auth/login')
    } else {
      formError.value = result.message ?? 'Invalid or expired reset link'
    }
  } catch {
    formError.value = 'Invalid or expired reset link'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <!-- Section: Reset Form — email, user ID, token, and new password fields with validation -->
  <Form :resolver="resetResolver" :initial-values="form" class="flex flex-col gap-4 w-full md:w-120" @submit="onSubmit">
    <FormField name="email" class="flex flex-col gap-1">
      <label class="text-surface-900 dark:text-surface-0 font-medium">Email</label>
      <InputText type="email" fluid size="large" disabled />
    </FormField>

    <FormField name="userId" class="flex flex-col gap-1">
      <label class="text-surface-900 dark:text-surface-0 font-medium">User ID</label>
      <InputText fluid size="large" disabled />
    </FormField>

    <FormField v-slot="$field" name="token" class="flex flex-col gap-1">
      <label class="text-surface-900 dark:text-surface-0 font-medium">Reset Token</label>
      <InputText fluid size="large" />
      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
    </FormField>

    <FormField v-slot="$field" name="newPassword" class="flex flex-col gap-1">
      <FloatLabel variant="on">
        <IconField>
          <InputPassword id="newPassword" :mask="mask" fluid size="large" :feedback="false" />
          <InputIcon class="cursor-pointer" @click="mask = !mask">
            <Eye v-if="mask" />
            <EyeSlash v-else />
          </InputIcon>
        </IconField>
        <label for="newPassword">New Password</label>
      </FloatLabel>
      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
    </FormField>

    <!-- Section: Submit Action — reset-password button with inline error display -->
    <Message v-if="formError" severity="error" :closable="false">{{ formError }}</Message>

    <Button type="submit" label="Reset Password" fluid size="large" :loading="isSubmitting" />
  </Form>

  <router-link to="/auth/login" class="text-base text-primary hover:underline text-center block mt-4">
    &larr; Back to login
  </router-link>
</template>
