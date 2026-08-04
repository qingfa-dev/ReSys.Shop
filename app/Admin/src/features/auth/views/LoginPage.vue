<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import FloatLabel from 'primevue/floatlabel'
import Message from 'primevue/message'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import Eye from '@primeicons/vue/eye'
import EyeSlash from '@primeicons/vue/eye-slash'
import { useNotify } from '@/shared/composables/useNotify'
import { loginSchema } from '../validations/auth'
import { useAuthStore } from '../stores/authStore'
import { resolvePostLoginRedirect } from '@/shared/utils/postLoginRedirect'

const router = useRouter()
const route = useRoute()
const store = useAuthStore()
const notify = useNotify()

const form = ref({ credential: '', password: '' })
const loginResolver = zodResolver(loginSchema)
const isLoading = computed(() => store.status === 'loading')
const mask = ref(true)

async function onSubmit(event: FormSubmitEvent) {
  // Validate: Return early when zod form validation fails.
  if (!event.valid) return
  const data = event.values as { credential: string; password: string }
  // Call: Delegate the login request to the auth store, which owns auth state.
  await store.login(data.credential, data.password)
  if (store.isAuthenticated) {
    // Redirect: Resume the protected page the user originally requested
    router.replace(resolvePostLoginRedirect(route.query.redirect))
  } else if (store.error) {
    notify.error('Login failed', store.error)
  }
}
</script>

<template>
  <div>
    <!-- Section: Login Form — credential & password fields with inline validation -->
    <Form :resolver="loginResolver" :initial-values="form" class="flex flex-col gap-4" @submit="onSubmit">
      <FormField v-slot="$field" name="credential" class="flex flex-col gap-1">
        <FloatLabel variant="on">
          <InputText id="credential" type="text" fluid size="large" autocomplete="username" />
          <label for="credential">Email or Username</label>
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
      <!-- Section: Form Actions — remember me toggle, forgot-password link, and submit -->
      <div class="flex items-center justify-between mt-2 mb-8 gap-8">
        <div class="flex items-center">
          <Checkbox inputId="rememberme1" binary class="mr-2" />
          <label for="rememberme1">Remember me</label>
        </div>
        <span
          class="font-medium no-underline ml-2 text-right cursor-pointer text-primary"
          @click="router.push('/auth/forgot-password')"
        >Forgot password?</span
        >
      </div>
      <Button label="Sign In" type="submit" fluid size="large" :loading="isLoading" />
    </Form>
  </div>
</template>
