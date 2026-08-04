<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import Button from 'primevue/button'
import Checkbox from 'primevue/checkbox'
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
import Google from '@primeicons/vue/google'
import { useAuthStore } from '../stores/authStore'
import { loginSchema } from '../validations/login'
import { validateRedirect } from '@/shared/utils/postLoginRedirect'

const router = useRouter()
const route = useRoute()
const store = useAuthStore()

const form = ref({ credential: '', password: '' })
const loginResolver = zodResolver(loginSchema)
const mask = ref(true)
const rememberMe = ref(false)
const googleLoading = ref(false)
const loginError = ref<string | null>(null)

const isLoading = computed(() => store.status === 'loading')
const redirectTarget = typeof route.query.redirect === 'string' ? route.query.redirect : null

async function onSubmit(event: FormSubmitEvent) {
  // Validate: Return early when zod form validation fails.
  if (!event.valid) return
  loginError.value = null
  const data = event.values as { credential: string; password: string }
  try {
    const ok = await store.login(data.credential, data.password)
    if (ok) {
      // Redirect: Resume the protected page the user originally requested.
      router.replace(validateRedirect(redirectTarget))
    } else {
      // Security: Show a generic message without revealing whether the account exists.
      loginError.value = 'Invalid email or password'
    }
  } catch {
    // Defensive: login() resolves false on all failures, but guard against a
    // rethrown rejection so the button never stays loading.
    loginError.value = store.error ?? 'Unable to sign in. Please try again.'
  }
}

async function onGoogleLogin(): Promise<void> {
  googleLoading.value = true
  try {
    await store.loginWithGoogle()
  } finally {
    googleLoading.value = false
  }
}
</script>

<template>
  <div>
    <h2 class="text-2xl font-bold text-center text-gray-900 mb-6">Sign in to your account</h2>
    <Form :resolver="loginResolver" :initial-values="form" class="flex flex-col gap-4" @submit="onSubmit">
      <FormField v-slot="$field" name="credential" class="flex flex-col gap-1">
        <FloatLabel variant="on">
          <InputText id="credential" type="email" fluid size="large" autocomplete="email" />
          <label for="credential">Email</label>
        </FloatLabel>
        <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
      </FormField>

      <FormField v-slot="$field" name="password" class="flex flex-col gap-1">
        <FloatLabel variant="on">
          <IconField>
            <InputPassword id="password" placeholder="Password" :mask="mask" fluid size="large" :feedback="false" autocomplete="current-password" />
            <InputIcon class="cursor-pointer" @click="mask = !mask">
              <Eye v-if="mask" />
              <EyeSlash v-else />
            </InputIcon>
          </IconField>
          <label for="password">Password</label>
        </FloatLabel>
        <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
      </FormField>

      <!-- Section: Remember me + forgot-password link -->
      <div class="flex items-center justify-between mt-2 mb-2">
        <div class="flex items-center gap-2">
          <Checkbox inputId="rememberme" binary v-model="rememberMe" />
          <label for="rememberme" class="text-sm text-gray-600">Remember me</label>
        </div>
        <router-link to="/forgot-password" class="text-sm font-medium text-primary hover:underline">Forgot password?</router-link>
      </div>

      <Message v-if="loginError" severity="error" :closable="false">{{ loginError }}</Message>
      <Button type="submit" label="Sign In" fluid size="large" :loading="isLoading" />
    </Form>

    <div class="flex items-center gap-4 my-6">
      <Divider />
      <span class="text-sm text-gray-400">or</span>
      <Divider />
    </div>

    <Button type="button" variant="outlined" severity="secondary" fluid size="large" :loading="googleLoading" @click="onGoogleLogin">
      <Google class="w-4 h-4 mr-2 shrink-0" />
      Continue with Google
    </Button>

    <p class="text-center text-sm text-gray-600 mt-6">
      Don't have an account?
      <router-link to="/register" class="font-medium text-primary hover:underline">Create account</router-link>
    </p>
  </div>
</template>
