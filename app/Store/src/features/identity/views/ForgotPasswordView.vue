<script setup lang="ts">
import { ref } from 'vue'
import { useForm } from 'vee-validate'
import { toFormValidator } from '@vee-validate/zod'
import { useAuthStore } from '../stores/authStore'
import { ForgotPasswordSchema } from '../validations'

// Store: Auth store owns the reset-link request.
const auth = useAuthStore()

// Form: Wire vee-validate to the existing zod forgot-password schema.
const { handleSubmit, isSubmitting, errors, defineField } = useForm<{ email: string }>({
  validationSchema: toFormValidator(ForgotPasswordSchema),
  initialValues: { email: '' },
})

// Fields: Two-way model ref with per-field attrs for the input.
const [email, emailAttrs] = defineField('email')

// Feedback: Success state swaps the form for a confirmation message.
const submitted = ref(false)
const submittedEmail = ref('')
const apiError = ref<string | null>(null)

// Submit: Delegate to the store, then show the confirmation state.
const onSubmit = handleSubmit(async values => {
  apiError.value = null
  const ok = await auth.forgotPassword(values.email)
  if (ok) {
    submitted.value = true
    submittedEmail.value = values.email
  } else {
    apiError.value = auth.error ?? 'Could not send the reset link'
  }
})
</script>

<template>
  <!-- Section: Success State — confirmation shown once the reset link is sent -->
  <div v-if="submitted" class="flex flex-col gap-4">
    <Message severity="success" :closable="false">
      Reset link sent to {{ submittedEmail }}. Check your inbox.
    </Message>
    <Button as="router-link" to="/login" text size="small" label="Back to Sign In" icon="pi pi-arrow-left" />
  </div>

  <!-- Section: Forgot Form — email field with inline validation -->
  <form v-else class="flex flex-col gap-4" novalidate @submit="onSubmit">
    <p class="text-sm text-surface-500">
      Enter your email and we'll send you a link to reset your password.
    </p>

    <FloatLabel variant="on">
      <InputText
        id="email"
        v-model="email"
        v-bind="emailAttrs"
        type="email"
        fluid
        autocomplete="email"
        :invalid="!!errors.email"
      />
      <Label for="email">Email</Label>
    </FloatLabel>
    <Message v-if="errors.email" severity="error" size="small" variant="simple">
      {{ errors.email }}
    </Message>

    <!-- Section: Feedback — inline message for API errors -->
    <Message v-if="apiError" severity="error" :closable="false">{{ apiError }}</Message>

    <Button type="submit" label="Send Reset Link" fluid :loading="isSubmitting" />
  </form>
</template>
