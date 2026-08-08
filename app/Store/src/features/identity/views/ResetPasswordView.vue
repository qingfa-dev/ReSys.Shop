<script setup lang="ts">
import { useForm } from 'vee-validate'
import { toFormValidator } from '@vee-validate/zod'
import { useRouter, useRoute } from 'vue-router'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useAuthStore } from '../stores/authStore'
import { ResetPasswordSchema } from '../validations/auth'
import { useNotify } from '@/shared/composables/useNotify'

usePageTitle('Set New Password')
const auth = useAuthStore()
const router = useRouter()
const route = useRoute()
const notify = useNotify()

const token = (route.query.token as string) || ''
const hasToken = token.length > 0

const { handleSubmit, isSubmitting, meta } = useForm({
  validationSchema: toFormValidator(ResetPasswordSchema),
  initialValues: { token, newPassword: '' },
})

const onSubmit = handleSubmit(async (values) => {
  const ok = await auth.resetPassword(values.token, values.newPassword)
  if (ok) {
    notify.success('Password reset successfully. Please sign in.')
    router.push('/login')
  } else {
    notify.error('Invalid or expired reset token')
  }
})
</script>
<template>
  <!-- Section: Page Header -->
  <div class="w-full max-w-md mx-auto">
    <h1 class="text-lg font-semibold text-neutral-900 mb-6">Set new password</h1>

    <!-- Section: Invalid Token State -->
    <div v-if="!hasToken" class="text-center py-8">
      <p class="text-sm text-neutral-500 mb-4">Invalid reset link.</p>
      <router-link to="/forgot-password" class="text-sm font-medium text-neutral-900 hover:underline">Request a new one</router-link>
    </div>

    <!-- Section: Reset Password Form -->
    <form v-else @submit="onSubmit" class="space-y-4">
      <div>
        <label class="block text-sm font-medium text-neutral-700 mb-1">New password</label>
        <Field name="newPassword" v-slot="{ field, errorMessage }">
          <Password v-bind="field" placeholder="Min 8 characters" class="w-full" :class="{ 'p-invalid': errorMessage }" :feedback="false" toggle-mask />
          <ErrorMessage name="newPassword" class="text-red-600 text-xs mt-1" />
        </Field>
      </div>
      <Button type="submit" label="Set New Password" severity="primary" class="w-full" :disabled="!meta.valid || isSubmitting" :loading="isSubmitting" />
    </form>
  </div>
</template>
