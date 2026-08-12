<script setup lang="ts">
import Label from 'primevue/label'
import { ref } from 'vue'
import { useForm } from 'vee-validate'
import { toFormValidator } from '@vee-validate/zod'
import FieldMessage from '@/shared/components/FieldMessage.vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useAuthStore } from '@/features/identity/stores/authStore'
import { ChangePasswordFormSchema, type ChangePasswordForm } from '@/features/identity/validations'
import { usePasswordStrength } from '@/features/identity/composables/usePasswordStrength'

usePageTitle('Change Password')

// Store: Auth store owns the change-password request.
const auth = useAuthStore()

const { handleSubmit, isSubmitting, errors, defineField } = useForm<ChangePasswordForm>({
  validationSchema: toFormValidator(ChangePasswordFormSchema),
  initialValues: { currentPassword: '', newPassword: '', confirmPassword: '' },
})

// Fields: Two-way model refs with per-field attrs for the inputs.
const [currentPassword, currentPasswordAttrs] = defineField('currentPassword')
const [newPassword, newPasswordAttrs] = defineField('newPassword')
const [confirmPassword, confirmPasswordAttrs] = defineField('confirmPassword')

// Meter: Live strength feedback for the new password field.
const strengthInfo = usePasswordStrength(newPassword)

// Feedback: Inline message state for API errors and success.
const apiError = ref<string | null>(null)
const changed = ref(false)

// Submit: Delegate to the store, then show the success state with a profile link.
const onSubmit = handleSubmit(async values => {
  apiError.value = null
  const ok = await auth.changePassword(values.currentPassword, values.newPassword)
  if (ok) {
    changed.value = true
  } else {
    apiError.value = auth.error ?? 'Current password is incorrect'
  }
})
</script>

<template>
  <!-- Section: Success State — confirmation shown once the password is changed -->
  <div v-if="changed" class="flex flex-col gap-4">
    <Message severity="success" :closable="false">Password changed successfully.</Message>
    <Button as="router-link" to="/account/profile" text size="small" label="Back to Profile" icon="pi pi-arrow-left" />
  </div>

  <!-- Section: Content Card — current, new and confirm password fields -->
  <Card v-else class="max-w-xl">
    <template #title>Change Password</template>
    <template #content>
      <Fluid>
        <form class="flex flex-col gap-4" novalidate @submit="onSubmit">
          <!-- Section: Form Fields — password inputs with validation -->
          <FloatLabel variant="on">
            <InputPassword
              id="currentPassword"
              v-model="currentPassword"
              v-bind="currentPasswordAttrs"
              fluid
              autocomplete="current-password"
              :invalid="!!errors.currentPassword"
            />
            <Label for="currentPassword">Current password</Label>
          </FloatLabel>
          <FieldMessage :error="errors.currentPassword" />

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
            <Label for="confirmPassword">Confirm new password</Label>
          </FloatLabel>
          <FieldMessage :error="errors.confirmPassword" />

          <!-- Section: Feedback — inline message for API errors -->
          <Message v-if="apiError" severity="error" :closable="false">{{ apiError }}</Message>

          <!-- Section: Action Footer — submit persists the new password -->
          <Button type="submit" label="Change Password" :loading="isSubmitting" />
        </form>
      </Fluid>
    </template>
  </Card>
</template>
