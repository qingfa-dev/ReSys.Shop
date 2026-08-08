<script setup lang="ts">
import { Field, ErrorMessage, useForm } from 'vee-validate'
import { toFormValidator } from '@vee-validate/zod'
import { useRouter } from 'vue-router'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useAuthStore } from '../stores/authStore'
import { RegisterFormSchema } from '../validations/auth'
import type { RegisterForm } from '../validations/auth'
import { useNotify } from '@/shared/composables/useNotify'

// Page: Set document title.
usePageTitle('Create Account')

// Stores: Access auth and notification services.
const auth = useAuthStore()
const notify = useNotify()
const router = useRouter()

// Form: Configure vee-validate with Zod schema.
const { handleSubmit, isSubmitting, meta } = useForm<RegisterForm>({
  validationSchema: toFormValidator(RegisterFormSchema),
  initialValues: { fullName: '', email: '', password: '', confirmPassword: '' },
})

// Submit: Handle form submission, register, auto-login, and redirect.
const onSubmit = handleSubmit(async (values) => {
  const ok = await auth.register({
    fullName: values.fullName,
    email: values.email,
    password: values.password,
  })
  if (ok) {
    await auth.login(values.email, values.password)
    router.push('/')
    notify.success('Account created')
  } else {
    notify.error(auth.error ?? 'Registration failed')
  }
})

// Google: Redirect to Google OAuth provider.
function onGoogleLogin(): void {
  auth.loginWithGoogle()
}
</script>

<template>
  <div class="max-w-md mx-auto py-16 px-4">
    <!-- Section: Page Header — breadcrumb and title -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Create Account' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Create Account</h1>

    <!-- Section: Content Card — registration form and social login -->
    <Card>
      <template #content>
        <!-- Section: Form Fields — full name, email, password, confirm password -->
        <form @submit="onSubmit" class="space-y-4">
          <div>
            <label for="fullName" class="block text-sm font-medium text-neutral-700 mb-1">
              Full name
            </label>
            <Field name="fullName" v-slot="{ field, errorMessage }">
              <InputText
                id="fullName"
                v-bind="field"
                placeholder="Jane Doe"
                class="w-full"
                :class="{ 'p-invalid': errorMessage }"
              />
              <ErrorMessage name="fullName" class="text-red-500 text-xs mt-1" />
            </Field>
          </div>

          <div>
            <label for="email" class="block text-sm font-medium text-neutral-700 mb-1">
              Email
            </label>
            <Field name="email" v-slot="{ field, errorMessage }">
              <InputText
                id="email"
                v-bind="field"
                type="email"
                placeholder="you@example.com"
                class="w-full"
                :class="{ 'p-invalid': errorMessage }"
              />
              <ErrorMessage name="email" class="text-red-500 text-xs mt-1" />
            </Field>
          </div>

          <div>
            <label for="password" class="block text-sm font-medium text-neutral-700 mb-1">
              Password
            </label>
            <Field name="password" v-slot="{ field, errorMessage }">
              <Password
                id="password"
                v-bind="field"
                placeholder="Min 8 characters"
                :feedback="false"
                toggleMask
                class="w-full"
                inputClass="w-full"
                :class="{ 'p-invalid': errorMessage }"
              />
              <ErrorMessage name="password" class="text-red-500 text-xs mt-1" />
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
                placeholder="Re-enter your password"
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
            label="Create Account"
            class="w-full"
            :disabled="!meta.valid || isSubmitting"
            :loading="isSubmitting"
          />
        </form>

        <!-- Section: Social Login — Google OAuth -->
        <div class="mt-6">
          <div class="relative">
            <div class="absolute inset-0 flex items-center">
              <div class="w-full border-t border-neutral-200" />
            </div>
            <div class="relative flex justify-center text-sm">
              <span class="bg-white px-2 text-neutral-500">or continue with</span>
            </div>
          </div>

          <Button
            type="button"
            label="Google"
            icon="pi pi-google"
            class="w-full mt-4"
            severity="secondary"
            outlined
            @click="onGoogleLogin"
          />
        </div>

        <!-- Section: Login Link — existing account prompt -->
        <p class="mt-6 text-center text-sm text-neutral-600">
          Already have an account?
          <router-link to="/login" class="font-medium text-neutral-900 hover:underline">
            Sign In
          </router-link>
        </p>
      </template>
    </Card>
  </div>
</template>
