<script setup lang="ts">
import { Field, ErrorMessage, useForm } from 'vee-validate'
import { toFormValidator } from '@vee-validate/zod'
import { z } from 'zod'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useAuthStore } from '@/features/identity/stores/authStore'
import { useNotify } from '@/shared/composables/useNotify'

usePageTitle('Change Password')
const authStore = useAuthStore()
const notify = useNotify()

// Schema: Extend ChangePasswordSchema with confirm-password refinement
const formSchema = z
  .object({
    currentPassword: z.string(),
    newPassword: z.string().min(8),
    confirmPassword: z.string(),
  })
  .refine((d) => d.newPassword === d.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  })

type ChangePasswordForm = z.infer<typeof formSchema>

// Form: Configure vee-validate with Zod schema
const { handleSubmit, isSubmitting, meta } = useForm<ChangePasswordForm>({
  validationSchema: toFormValidator(formSchema),
  initialValues: { currentPassword: '', newPassword: '', confirmPassword: '' },
})

// Submit: Call auth store and show outcome toast
const onSubmit = handleSubmit(async (values) => {
  const ok = await authStore.changePassword(values.currentPassword, values.newPassword)
  if (ok) {
    notify.success('Password changed successfully')
  } else {
    notify.error('Failed to change password')
  }
})
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Section: Page Header — breadcrumb navigation and title -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Change Password' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Change Password</h1>

    <!-- Section: Content Card — password form with validation -->
    <Card>
      <template #content>
        <form @submit="onSubmit" class="space-y-4">
          <div>
            <label for="currentPassword" class="block text-sm font-medium text-neutral-700 mb-1">
              Current password
            </label>
            <Field name="currentPassword" v-slot="{ field, errorMessage }">
              <Password
                id="currentPassword"
                v-bind="field"
                placeholder="Enter current password"
                :feedback="false"
                toggleMask
                class="w-full"
                inputClass="w-full"
                :class="{ 'p-invalid': errorMessage }"
              />
              <ErrorMessage name="currentPassword" class="text-red-500 text-xs mt-1" />
            </Field>
          </div>

          <div>
            <label for="newPassword" class="block text-sm font-medium text-neutral-700 mb-1">
              New password
            </label>
            <Field name="newPassword" v-slot="{ field, errorMessage }">
              <Password
                id="newPassword"
                v-bind="field"
                placeholder="Min 8 characters"
                :feedback="false"
                toggleMask
                class="w-full"
                inputClass="w-full"
                :class="{ 'p-invalid': errorMessage }"
              />
              <ErrorMessage name="newPassword" class="text-red-500 text-xs mt-1" />
            </Field>
          </div>

          <div>
            <label for="confirmPassword" class="block text-sm font-medium text-neutral-700 mb-1">
              Confirm password
            </label>
            <Field name="confirmPassword" v-slot="{ field, errorMessage }">
              <Password
                id="confirmPassword"
                v-bind="field"
                placeholder="Re-enter new password"
                :feedback="false"
                toggleMask
                class="w-full"
                inputClass="w-full"
                :class="{ 'p-invalid': errorMessage }"
              />
              <ErrorMessage name="confirmPassword" class="text-red-500 text-xs mt-1" />
            </Field>
          </div>

          <!-- Section: Action Footer — submit button -->
          <Button
            type="submit"
            label="Change Password"
            :disabled="!meta.valid || isSubmitting"
            :loading="isSubmitting"
          />
        </form>
      </template>
    </Card>
  </div>
</template>
