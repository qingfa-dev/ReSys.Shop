<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import Button from 'primevue/button'
import FloatLabel from 'primevue/floatlabel'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import InputPassword from 'primevue/inputpassword'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import Eye from '@primeicons/vue/eye'
import EyeSlash from '@primeicons/vue/eye-slash'
import { useNotify } from '@/shared/composables/useNotify'
import * as authApi from '../services/authApi'
import { registerSchema } from '../validations/register'

const router = useRouter()
const notify = useNotify()

const form = ref({ fullName: '', email: '', password: '', confirmPassword: '' })
const registerResolver = zodResolver(registerSchema)
const isSubmitting = ref(false)
const formError = ref<string | null>(null)
const mask = ref(true)
const confirmMask = ref(true)

// Strength: Client-side password heuristic — length + mixed case + digits → 0-4 bar.
function strengthInfo(password: string): { width: number; label: string; colorClass: string } {
  let score = 0
  if (!password) return { width: 0, label: '', colorClass: 'bg-gray-200' }
  if (password.length >= 8) score++
  if (password.length >= 12) score++
  if (/[A-Z]/.test(password) && /[a-z]/.test(password)) score++
  if (/\d/.test(password)) score++
  const width = (score / 4) * 100
  const label = ['Too weak', 'Weak', 'Fair', 'Good', 'Strong'][score] ?? ''
  const colorClass = ['bg-red-400', 'bg-red-500', 'bg-yellow-400', 'bg-green-400', 'bg-green-600'][score] ?? 'bg-gray-200'
  return { width, label, colorClass }
}

async function onSubmit(event: FormSubmitEvent) {
  // Validate: Return early when zod form validation fails.
  if (!event.valid) return
  isSubmitting.value = true
  formError.value = null
  try {
    const data = event.values as { fullName: string; email: string; password: string; confirmPassword: string }
    const result = await authApi.register({ fullName: data.fullName, email: data.email, password: data.password })
    if (result.isSuccess) {
      notify.success('Account created', 'Check your email to verify')
      router.replace('/login')
    } else {
      formError.value = result.message ?? result.errors[0]?.message ?? 'Registration failed'
    }
  } catch {
    formError.value = 'Something went wrong. Please try again.'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div>
    <h2 class="text-2xl font-bold text-center text-gray-900 mb-6">Create your account</h2>
    <Form :resolver="registerResolver" :initial-values="form" class="flex flex-col gap-4" @submit="onSubmit">
      <FormField v-slot="$field" name="fullName" class="flex flex-col gap-1">
        <FloatLabel variant="on">
          <InputText id="fullName" type="text" fluid size="large" autocomplete="name" />
          <label for="fullName">Full Name</label>
        </FloatLabel>
        <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
      </FormField>

      <FormField v-slot="$field" name="email" class="flex flex-col gap-1">
        <FloatLabel variant="on">
          <InputText id="email" type="email" fluid size="large" autocomplete="email" />
          <label for="email">Email</label>
        </FloatLabel>
        <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
      </FormField>

      <FormField v-slot="$field" name="password" class="flex flex-col gap-1">
        <FloatLabel variant="on">
          <IconField>
            <InputPassword id="password" placeholder="Password" :mask="mask" fluid size="large" :feedback="false" autocomplete="new-password" />
            <InputIcon class="cursor-pointer" @click="mask = !mask">
              <Eye v-if="mask" />
              <EyeSlash v-else />
            </InputIcon>
          </IconField>
          <label for="password">Password</label>
        </FloatLabel>
        <!-- Section: Password Strength Bar -->
        <div v-if="$field.value" class="mt-1">
          <div class="h-1.5 w-full bg-gray-200 rounded-full overflow-hidden">
            <div
              class="h-full rounded-full transition-all duration-300"
              :class="strengthInfo($field.value).colorClass"
              :style="{ width: `${strengthInfo($field.value).width}%` }"
            />
          </div>
          <span class="text-xs text-gray-500">{{ strengthInfo($field.value).label }}</span>
        </div>
        <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
      </FormField>

      <FormField v-slot="$field" name="confirmPassword" class="flex flex-col gap-1">
        <FloatLabel variant="on">
          <IconField>
            <InputPassword id="confirmPassword" placeholder="Confirm password" :mask="confirmMask" fluid size="large" :feedback="false" autocomplete="new-password" />
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
      <Button type="submit" label="Create Account" fluid size="large" :loading="isSubmitting" />
    </Form>

    <p class="text-center text-sm text-gray-600 mt-6">
      Already have an account?
      <router-link to="/login" class="font-medium text-primary hover:underline">Sign in</router-link>
    </p>
  </div>
</template>
