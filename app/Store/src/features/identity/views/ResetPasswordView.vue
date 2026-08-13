<script setup lang="ts">
import Label from 'primevue/label'
import { ref } from 'vue'
import { useRoute } from 'vue-router'
import { useForm } from 'vee-validate'
import { toFormValidator } from '@vee-validate/zod'
import FieldMessage from '@/shared/components/FieldMessage.vue'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { useAuthStore } from '../stores/authStore'
import { ResetPasswordFormSchema, type ResetPasswordForm } from '../validations'
import { usePasswordStrength } from '../composables/usePasswordStrength'

// Store: Auth store owns the reset request; route carries the emailed token.
const auth = useAuthStore()
const { applyFieldErrors } = useApiErrorHandler()
const route = useRoute()

const token = typeof route.query.token === 'string' ? route.query.token : ''
const { handleSubmit, isSubmitting, errors, defineField, setFieldError } = useForm<ResetPasswordForm>({
  validationSchema: toFormValidator(ResetPasswordFormSchema),
  initialValues: { token, newPassword: '', confirmPassword: '' },
})

// Fields: Two-way model refs with per-field attrs for the inputs.
const [newPassword, newPasswordAttrs] = defineField('newPassword')
const [confirmPassword, confirmPasswordAttrs] = defineField('confirmPassword')

// Meter: Live strength feedback for the new password field.
const strengthInfo = usePasswordStrength(newPassword)

// Feedback: Inline message state for API errors and success.
const apiError = ref<string | null>(null)
const resetSuccess = ref(false)

// Submit: Delegate to the store, then show the success state with a sign-in link.
const onSubmit = handleSubmit(async values => {
  apiError.value = null
  const ok = await auth.resetPassword(values.token, values.newPassword)
  if (ok) {
    resetSuccess.value = true
  } else {
    // Map: Push field-scoped backend errors into their inputs
    const remaining = applyFieldErrors(auth.errors, (f, m) => setFieldError(f, m))
    apiError.value = remaining.length > 0 ? remaining.map(e => e.message).join(' ') : (auth.error ?? 'Invalid or expired reset token')
  }
})
</script>

<template>
  <!-- Section: Success State — confirmation shown once the password is reset -->
  <div v-if="resetSuccess" class="flex flex-col gap-4">
    <Message severity="success" :closable="false">Password reset successfully.</Message>
    <Button as="router-link" to="/login" text size="small" label="Back to Sign In" icon="pi pi-arrow-left" />
  </div>

  <!-- Section: Reset Form — new password and confirm fields with validation -->
  <form v-else class="flex flex-col gap-4" novalidate @submit="onSubmit">
    <FloatLabel variant="on">
      <InputPassword
        id="newPassword"
        v-model="newPassword"
        v-bind="newPasswordAttrs"
        fluid
        autocomplete="new-password"
        :invalid="!!errors.newPassword"
      />
      <Label for="newPassword">New password</Label>
    </FloatLabel>
    <FieldMessage :error="errors.newPassword" />

    <!-- Section: Strength Meter — live feedback as the password improves -->
    <div v-if="strengthInfo" class="flex flex-col gap-1">
      <ProgressBar
        :value="strengthInfo.percent"
        :show-value="false"
        :pt="{ value: { style: { backgroundColor: strengthInfo.color } } }"
        style="height: 6px"
      />
      <div class="flex justify-end">
        <Tag :severity="strengthInfo.severity" :value="strengthInfo.label" />
      </div>
    </div>

    <FloatLabel variant="on">
      <InputPassword
        id="confirmPassword"
        v-model="confirmPassword"
        v-bind="confirmPasswordAttrs"
        fluid
        autocomplete="new-password"
        :invalid="!!errors.confirmPassword"
      />
      <Label for="confirmPassword">Confirm password</Label>
    </FloatLabel>
    <FieldMessage :error="errors.confirmPassword" />

    <!-- Section: Feedback — inline message for API errors -->
    <Message v-if="apiError" severity="error" :closable="false">{{ apiError }}</Message>

    <Button type="submit" label="Reset Password" fluid :loading="isSubmitting" />
  </form>
</template>
