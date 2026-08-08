<script setup lang="ts">
import { useRouter } from 'vue-router'
import { toFormValidator } from '@vee-validate/zod'
import { useForm } from 'vee-validate'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useAuthStore } from '../stores/authStore'
import { useNotify } from '@/shared/composables/useNotify'
import { LoginFormSchema } from '../validations'

// Page: Set document title.
usePageTitle('Sign In')

// Stores: Access auth and notification services.
const auth = useAuthStore()
const notify = useNotify()
const router = useRouter()

// Form: Configure vee-validate with Zod schema.
const { handleSubmit, isSubmitting, errors, defineField } = useForm({
  validationSchema: toFormValidator(LoginFormSchema),
})

// Fields: Bind form inputs with validation.
const [credential, credentialAttrs] = defineField('credential')
const [password, passwordAttrs] = defineField('password')

// Submit: Handle form submission and navigate on success.
const onSubmit = handleSubmit(async (values) => {
  const ok = await auth.login(values.credential, values.password)
  if (ok) {
    notify.success('Signed in', 'Welcome back!')
    router.push('/')
  } else {
    notify.error('Sign in failed', auth.error ?? 'Invalid credentials')
  }
})

// Google: Redirect to Google OAuth provider.
async function handleGoogleLogin() {
  await auth.loginWithGoogle()
}
</script>

<template>
  <div class="max-w-md mx-auto py-16 px-4">
    <!-- Section: Page Header — breadcrumb and title -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Sign In' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Sign In</h1>

    <!-- Section: Content Card — login form and social login -->
    <Card>
      <template #content>
        <!-- Section: Form Fields — credential and password inputs -->
        <form @submit="onSubmit" class="space-y-4">
          <div>
            <label for="credential" class="block text-sm font-medium text-neutral-700 mb-1">
              Email or Username
            </label>
            <InputText
              id="credential"
              v-model="credential"
              v-bind="credentialAttrs"
              type="text"
              placeholder="Enter your email or username"
              class="w-full"
              :class="{ 'p-invalid': errors.credential }"
            />
            <small v-if="errors.credential" class="text-red-500">{{ errors.credential }}</small>
          </div>

          <div>
            <label for="password" class="block text-sm font-medium text-neutral-700 mb-1">
              Password
            </label>
            <Password
              id="password"
              v-model="password"
              v-bind="passwordAttrs"
              placeholder="Enter your password"
              :feedback="false"
              toggleMask
              class="w-full"
              inputClass="w-full"
              :class="{ 'p-invalid': errors.password }"
            />
            <small v-if="errors.password" class="text-red-500">{{ errors.password }}</small>
          </div>

          <!-- Section: Action Footer — submit button -->
          <div class="flex items-center justify-between">
            <router-link
              to="/forgot-password"
              class="text-sm text-neutral-600 hover:text-neutral-900"
            >
              Forgot password?
            </router-link>
            <Button
              type="submit"
              label="Sign In"
              :loading="isSubmitting"
              :disabled="isSubmitting"
            />
          </div>
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
            @click="handleGoogleLogin"
          />
        </div>

        <!-- Section: Register Link — create account prompt -->
        <p class="mt-6 text-center text-sm text-neutral-600">
          Don't have an account?
          <router-link to="/register" class="font-medium text-neutral-900 hover:underline">
            Create one
          </router-link>
        </p>
      </template>
    </Card>
  </div>
</template>
