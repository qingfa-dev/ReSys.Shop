<template>
  <form class="flex flex-col gap-3" @submit.prevent="onSubmit">
    <AppFormField label="Email" :error="errors.email">
      <InputText v-model="email" :invalid="!!errors.email" type="email" autocomplete="email" />
    </AppFormField>
    <AppFormField label="Password" :error="errors.password">
      <Password v-model="password" :invalid="!!errors.password" :feedback="false" toggle-mask input-class="w-full" />
    </AppFormField>
    <AppButton type="submit" label="Sign in" :loading="login.isPending.value" />
  </form>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { loginSchema } from '../model/auth.schema'
import { useAuthState } from '../composables/useAuthState'

const email = ref('')
const password = ref('')
const errors = ref<{ email?: string; password?: string }>({})

const { login, setTokens } = useAuthState()

async function onSubmit() {
  const parsed = loginSchema.safeParse({ email: email.value, password: password.value })
  if (!parsed.success) {
    const flat = parsed.error.flatten().fieldErrors
    errors.value = { email: flat.email?.[0], password: flat.password?.[0] }
    return
  }
  errors.value = {}
  const tokens = await login.mutateAsync(parsed.data)
  await setTokens(tokens)
}
</script>
