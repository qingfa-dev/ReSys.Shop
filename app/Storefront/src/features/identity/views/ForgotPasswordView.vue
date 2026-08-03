<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { IDENTITY_ENDPOINTS } from '../types/constants/identity.constants'
import { httpClient } from '@/core/http'

const route = useRoute()
const router = useRouter()

// Step discrimination: if token query param exists, show reset form
const tokenFromQuery = (route.query.token as string) || ''
const isResetStep = computed(() => !!tokenFromQuery)

// Step 1: Email entry
const email = ref('')
const isSubmitting = ref(false)
const emailSent = ref(false)
const emailError = ref<string | null>(null)

// Step 2: New password
const token = ref(tokenFromQuery)
const newPassword = ref('')
const confirmPassword = ref('')
const resetSuccess = ref(false)
const resetError = ref<string | null>(null)
const passwordMismatch = computed(() =>
  confirmPassword.value.length > 0 && newPassword.value !== confirmPassword.value)

async function handleSendResetLink() {
  if (!email.value.trim()) return
  isSubmitting.value = true
  emailError.value = null
  try {
    await httpClient.post(IDENTITY_ENDPOINTS.FORGOT_PASSWORD, { email: email.value.trim() })
    emailSent.value = true
  } catch {
    emailError.value = 'Something went wrong. Please try again.'
  } finally {
    isSubmitting.value = false
  }
}

async function handleResetPassword() {
  if (!newPassword.value || passwordMismatch.value) return
  isSubmitting.value = true
  resetError.value = null
  try {
    const response = await httpClient.post(IDENTITY_ENDPOINTS.RESET_PASSWORD, {
      email: (route.query.email as string) || '',
      token: token.value,
      newPassword: newPassword.value,
    })
    if (response.data?.isSuccess === false) {
      const msg = response.data?.message || ''
      resetError.value = msg.includes('expired')
        ? 'This reset link has expired. Please request a new one.'
        : (msg || 'Reset failed. Please try again.')
      return
    }
    resetSuccess.value = true
    setTimeout(() => router.push({ path: '/login', query: { reset: 'success' } }), 2000)
  } catch (e: any) {
    resetError.value = e?.response?.data?.message || 'Reset failed. Please try again.'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="forgot-password-view">
    <div class="auth-card">
      <!-- Step 1: Email entry -->
      <template v-if="!isResetStep">
        <h1>Forgot Password</h1>
        <p class="subtitle">Enter your email and we'll send you a reset link.</p>

        <div v-if="emailSent" class="success-state">
          <i class="pi pi-check-circle"></i>
          <p>If an account exists for this email, a reset link has been sent.</p>
          <Button label="Back to Login" class="p-button-text" @click="router.push('/login')" />
        </div>

        <template v-else>
          <div class="form-field">
            <label for="email">Email</label>
            <InputText id="email" v-model="email" type="email" placeholder="you@example.com"
                       :disabled="isSubmitting" class="full-width" />
          </div>

          <Message v-if="emailError" severity="error" :closable="false">{{ emailError }}</Message>

          <Button label="Send Reset Link" icon="pi pi-envelope" :loading="isSubmitting"
                  :disabled="!email.trim()" @click="handleSendResetLink" class="full-width" />

          <p class="back-link">
            <router-link to="/login">Back to Login</router-link>
          </p>
        </template>
      </template>

      <!-- Step 2: New password -->
      <template v-else>
        <h1>Reset Password</h1>
        <p class="subtitle">Enter your new password.</p>

        <div v-if="resetSuccess" class="success-state">
          <i class="pi pi-check-circle"></i>
          <p>Password reset successfully. Redirecting to login...</p>
        </div>

        <template v-else>
          <div class="form-field">
            <label for="new-password">New Password</label>
            <Password id="new-password" v-model="newPassword" toggle-mask
                      :disabled="isSubmitting" class="full-width" :feedback="false" />
          </div>

          <div class="form-field">
            <label for="confirm-password">Confirm Password</label>
            <Password id="confirm-password" v-model="confirmPassword" toggle-mask
                      :disabled="isSubmitting" class="full-width" :feedback="false" />
            <small v-if="passwordMismatch" class="field-error">Passwords do not match</small>
          </div>

          <Message v-if="resetError" severity="error" :closable="false">{{ resetError }}</Message>

          <Button label="Reset Password" icon="pi pi-lock" :loading="isSubmitting"
                  :disabled="!newPassword || passwordMismatch"
                  @click="handleResetPassword" class="full-width" />
        </template>
      </template>
    </div>
  </div>
</template>

<style scoped lang="scss">
.forgot-password-view {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 60vh;
  padding: 2rem;
}

.auth-card {
  width: 100%;
  max-width: 420px;
  padding: 2.5rem;
  background: var(--color-surface);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-md);

  h1 {
    font-family: var(--font-display);
    font-size: var(--font-size-2xl);
    margin-bottom: 0.5rem;
    text-align: center;
  }

  .subtitle {
    text-align: center;
    color: var(--color-text-secondary);
    margin-bottom: 2rem;
  }
}

.form-field {
  margin-bottom: 1.25rem;

  label {
    display: block;
    margin-bottom: 0.375rem;
    font-weight: var(--font-weight-medium);
    font-size: var(--font-size-sm);
  }

  .field-error {
    color: var(--color-danger);
    font-size: var(--font-size-xs);
  }
}

.full-width { width: 100%; }

.success-state {
  text-align: center;
  padding: 1.5rem 0;

  i { font-size: 3rem; color: #22c55e; margin-bottom: 1rem; }
  p { margin-bottom: 1rem; }
}

.back-link {
  text-align: center;
  margin-top: 1.5rem;

  a {
    color: var(--color-primary);
    text-decoration: none;
    font-size: var(--font-size-sm);
    &:hover { text-decoration: underline; }
  }
}
</style>
