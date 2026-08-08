<script setup lang="ts">
import { ref } from 'vue'
import { useForm } from 'vee-validate'
import { toFormValidator } from '@vee-validate/zod'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useAuthStore } from '../stores/authStore'
import { ForgotPasswordSchema } from '../validations/auth'
import { useNotify } from '@/shared/composables/useNotify'

usePageTitle('Reset Password')
const auth = useAuthStore()
const notify = useNotify()
const submitted = ref(false)
const submittedEmail = ref('')
const cooldown = ref(0)

const { handleSubmit, isSubmitting, meta } = useForm({
  validationSchema: toFormValidator(ForgotPasswordSchema),
  initialValues: { email: '' },
})

const onSubmit = handleSubmit(async (values) => {
  await auth.forgotPassword(values.email)
  submitted.value = true
  submittedEmail.value = values.email
  notify.success('Check your email')
})

async function onResend(): Promise<void> {
  if (cooldown.value > 0) return
  cooldown.value = 30
  await auth.forgotPassword(submittedEmail.value)
  notify.success('Email sent')
  const timer = setInterval(() => {
    cooldown.value--
    if (cooldown.value <= 0) clearInterval(timer)
  }, 1000)
}
</script>
<template>
  <!-- Section: Page Header -->
  <div class="w-full max-w-md mx-auto">
    <h1 class="text-lg font-semibold text-neutral-900 mb-3">Reset your password</h1>

    <!-- Section: Forgot Password Form -->
    <form v-if="!submitted" @submit="onSubmit" class="space-y-4">
      <p class="text-sm text-neutral-500">Enter your email and we'll send you a link to reset your password.</p>
      <div>
        <label class="block text-sm font-medium text-neutral-700 mb-1">Email</label>
        <Field name="email" v-slot="{ field, errorMessage }">
          <InputText v-bind="field" type="email" placeholder="you@example.com" class="w-full" :class="{ 'p-invalid': errorMessage }" />
          <ErrorMessage name="email" class="text-red-600 text-xs mt-1" />
        </Field>
      </div>
      <Button type="submit" label="Send Reset Link" severity="primary" class="w-full" :disabled="!meta.valid || isSubmitting" :loading="isSubmitting" />
    </form>

    <!-- Section: Success State -->
    <div v-else class="text-center py-8">
      <i class="pi pi-check-circle text-4xl text-green-500 mb-4 block" />
      <p class="text-sm font-medium text-neutral-900 mb-1">Check your email</p>
      <p class="text-sm text-neutral-500 mb-4">We sent a reset link to {{ submittedEmail }}</p>
      <button class="text-sm font-medium text-neutral-900 hover:underline" :disabled="cooldown > 0" @click="onResend">
        {{ cooldown > 0 ? `Resend in ${cooldown}s` : 'Resend' }}
      </button>
    </div>

    <!-- Section: Navigation Link -->
    <p class="text-center text-sm mt-6">
      <router-link to="/login" class="text-neutral-500 hover:text-neutral-900">&larr; Back to Sign In</router-link>
    </p>
  </div>
</template>
