<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import LoginForm from '@forms/LoginForm.vue'
import { loginSchema } from '../validations/auth'
import { useAuthStore } from '../stores/authStore'

const router = useRouter()
const store = useAuthStore()

const isLoading = computed(() => store.status === 'loading')
const authError = computed(() => store.error)

async function onLogin(data: { credential: string; password: string; remember: boolean }) {
  await store.login(data.credential, data.password)
  if (store.isAuthenticated) {
    router.replace('/')
  }
}

function onForgotPassword() {
  router.push('/auth/forgot-password')
}
</script>

<template>
  <LoginForm
    :validation-schema="loginSchema"
    :loading="isLoading"
    :server-error="authError"
    credential-label="Email or Username"
    credential-placeholder="Email address"
    @submit="onLogin"
    @forgot-password="onForgotPassword"
  />
</template>
