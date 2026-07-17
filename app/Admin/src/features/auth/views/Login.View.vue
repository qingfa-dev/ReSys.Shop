<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth.store'
import { storeToRefs } from 'pinia'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { LoginSchema } from '../schemas/auth.schema'
import { useI18n } from 'vue-i18n'
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use'

const { t } = useI18n()

const router = useRouter()
const authStore = useAuthStore()
const { loading } = storeToRefs(authStore)
const { handleApiResult } = useApiErrorHandler()

const mounted = ref(false)
const showDevTools = ref(false)

onMounted(() => {
  requestAnimationFrame(() => {
    mounted.value = true
  })
})

const { defineField, handleSubmit, errors, setErrors, values, setValues } = useForm({
  validationSchema: toTypedSchema(LoginSchema),
  initialValues: {
    credential: '',
    password: '',
    rememberMe: false,
  },
})

const [credential] = defineField('credential')
const [password] = defineField('password')
const [rememberMe] = defineField('rememberMe')

const onSubmit = handleSubmit(async (formValues) => {
  const result = await authStore.login(formValues)
  const handled = handleApiResult(result, {
    setErrors,
    fieldNames: Object.keys(values),
    successTitle: t('auth.titles.welcome'),
    successMessage: t('auth.messages.login_success'),
    errorTitle: t('common.error'),
    genericError: t('auth.messages.login_failed'),
  })
  if (handled && result.isSuccess) {
    router.push('/')
  }
})

const fillSeedCredentials = () => {
  setValues({
    credential: 'admin@resys.shop',
    password: 'Admin@1234!',
  })
}
</script>

<template>
  <div class="login-root">
    <div class="login-container" :class="{ 'is-visible': mounted }">
      <!-- Brand -->
      <div class="brand">
        <h1 class="brand-wordmark">
          <span class="brand-resys">ReSys</span><span class="brand-dot">.</span><span class="brand-shop">Shop</span>
        </h1>
        <p class="brand-subtitle">{{ t('auth.titles.app_subtitle') }}</p>
      </div>

      <!-- Divider -->
      <div class="divider">
        <span class="divider-line"></span>
        <span class="divider-mark">&#9670;</span>
        <span class="divider-line"></span>
      </div>

      <!-- Form -->
      <form @submit="onSubmit" class="login-form" novalidate>
        <div class="field">
          <label for="credential">{{ t('auth.labels.credential') }}</label>
          <input
            id="credential"
            v-model="credential"
            type="text"
            :placeholder="t('auth.placeholders.credential')"
            class="text-input"
            :class="{ 'has-value': values.credential, 'has-error': !!errors.credential }"
            :disabled="loading"
            autocomplete="username"
            spellcheck="false"
          />
          <Transition name="fade">
            <p v-if="errors.credential" class="field-msg error">{{ errors.credential }}</p>
          </Transition>
        </div>

        <div class="field">
          <label for="password">{{ t('auth.labels.password') }}</label>
          <div class="password-wrapper">
            <input
              id="password"
              v-model="password"
              type="password"
              :placeholder="t('auth.placeholders.password')"
              class="text-input"
              :class="{ 'has-value': values.password, 'has-error': !!errors.password }"
              :disabled="loading"
              autocomplete="current-password"
            />
          </div>
          <Transition name="fade">
            <p v-if="errors.password" class="field-msg error">{{ errors.password }}</p>
          </Transition>
        </div>

        <div class="form-footer">
          <label class="checkbox-label">
            <input type="checkbox" v-model="rememberMe" :disabled="loading" class="checkbox" />
            <span class="checkbox-faux"></span>
            <span class="checkbox-text">{{ t('auth.labels.remember_me') }}</span>
          </label>
          <a href="#" class="forgot-link">{{ t('auth.labels.forgot_password') }}</a>
        </div>

        <button
          type="submit"
          class="submit-btn"
          :disabled="loading"
        >
          <span v-if="!loading" class="submit-text">{{ t('auth.labels.sign_in') }}</span>
          <span v-if="!loading" class="submit-arrow">
            <svg width="18" height="18" viewBox="0 0 18 18" fill="none">
              <path d="M3 9h12M10 4l5 5-5 5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
          </span>
          <span v-else class="submit-loading">
            <svg class="spinner" width="18" height="18" viewBox="0 0 18 18" fill="none">
              <circle cx="9" cy="9" r="7" stroke="currentColor" stroke-width="2" stroke-dasharray="32" stroke-linecap="round" opacity="0.3"/>
              <circle cx="9" cy="9" r="7" stroke="currentColor" stroke-width="2" stroke-dasharray="32" stroke-linecap="round" stroke-dashoffset="24"/>
            </svg>
          </span>
        </button>
      </form>

      <!-- Dev toggle -->
      <div class="dev-section">
        <button class="dev-toggle" @click="showDevTools = !showDevTools" type="button">
          <svg width="14" height="14" viewBox="0 0 14 14" fill="none">
            <path d="M4 1L1 7l3 6M10 1l3 6-3 6" stroke="currentColor" stroke-width="1.2" stroke-linecap="round"/>
          </svg>
          {{ showDevTools ? 'close' : 'dev' }}
        </button>
        <Transition name="fade">
          <div v-if="showDevTools" class="dev-panel">
            <button type="button" class="dev-quick-btn" @click="fillSeedCredentials" :disabled="loading">
              <svg width="12" height="12" viewBox="0 0 12 12" fill="none">
                <path d="M6 1v10M1 6h10" stroke="currentColor" stroke-width="1.2" stroke-linecap="round"/>
              </svg>
              Login as admin@resys.shop
            </button>
          </div>
        </Transition>
      </div>

      <p class="copyright">{{ t('auth.messages.copyright', { year: new Date().getFullYear().toString() }) }}</p>
    </div>
  </div>
</template>

<style scoped>
/* ───── Reset ───── */
.login-root {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 32px 24px;
  background: #FCFCFA;
}

.login-container {
  width: 100%;
  max-width: 400px;
  opacity: 0;
  transform: translateY(12px);
  transition: opacity 0.6s ease-out, transform 0.6s ease-out;
}
.login-container.is-visible {
  opacity: 1;
  transform: translateY(0);
}

/* ───── Brand ───── */
.brand {
  text-align: center;
}

.brand-wordmark {
  font-family: 'DM Serif Display', ui-serif, Georgia, serif;
  font-size: 2.5rem;
  font-weight: 400;
  font-style: italic;
  color: #1A1A1A;
  margin: 0;
  line-height: 1.15;
  letter-spacing: -0.01em;
}

.brand-resys {
  font-style: italic;
}

.brand-dot {
  color: #6B4F3A;
  font-style: italic;
}

.brand-shop {
  font-style: normal;
  font-weight: 400;
  color: #6B4F3A;
}

.brand-subtitle {
  font-size: 0.8125rem;
  color: #A69282;
  margin: 8px 0 0 0;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  font-weight: 500;
}

/* ───── Divider ───── */
.divider {
  display: flex;
  align-items: center;
  gap: 12px;
  margin: 36px 0;
}

.divider-line {
  flex: 1;
  height: 1px;
  background: #E8E2DA;
}

.divider-mark {
  color: #6B4F3A;
  font-size: 8px;
  line-height: 1;
  opacity: 0.6;
}

/* ───── Form ───── */
.login-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.field label {
  font-size: 0.75rem;
  font-weight: 600;
  color: #6B4F3A;
  text-transform: uppercase;
  letter-spacing: 0.06em;
}

/* ───── Text input ───── */
.text-input {
  width: 100%;
  height: 48px;
  padding: 0 16px;
  font-size: 0.9375rem;
  font-family: 'Inter', ui-sans-serif, system-ui, sans-serif;
  color: #1A1A1A;
  background: transparent;
  border: none;
  border-bottom: 1.5px solid #E8E2DA;
  border-radius: 0;
  outline: none;
  transition: border-color 0.2s ease, background 0.2s ease;
  -webkit-appearance: none;
}

.text-input::placeholder {
  color: #C4B8AC;
}

.text-input:hover {
  border-color: #C4B8AC;
}

.text-input:focus {
  border-color: #6B4F3A;
  background: #F5F2EE;
}

.text-input.has-value {
  border-color: #6B4F3A;
}

.text-input.has-error {
  border-color: #B91C1C;
}

.text-input:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.field-msg {
  font-size: 0.75rem;
  margin: 2px 0 0 0;
  line-height: 1.3;
}

.field-msg.error {
  color: #B91C1C;
}

/* ───── Password wrapper ───── */
.password-wrapper .text-input {
  /* Inherits from .text-input */
}

/* ───── Form footer ───── */
.form-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 4px;
}

/* Custom checkbox */
.checkbox-label {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  user-select: none;
}

.checkbox-label input[type="checkbox"] {
  position: absolute;
  opacity: 0;
  width: 0;
  height: 0;
  pointer-events: none;
}

.checkbox-faux {
  width: 16px;
  height: 16px;
  border: 1.5px solid #C4B8AC;
  border-radius: 3px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: border-color 0.2s, background 0.2s;
  flex-shrink: 0;
}

.checkbox-label input:checked + .checkbox-faux {
  border-color: #6B4F3A;
  background: #6B4F3A;
}

.checkbox-label input:checked + .checkbox-faux::after {
  content: '';
  width: 5px;
  height: 8px;
  border: solid #FFFFFF;
  border-width: 0 1.5px 1.5px 0;
  transform: rotate(45deg);
  margin-top: -1px;
}

.checkbox-label input:focus-visible + .checkbox-faux {
  outline: 2px solid #6B4F3A;
  outline-offset: 2px;
}

.checkbox-text {
  font-size: 0.8125rem;
  color: #6B4F3A;
}

.forgot-link {
  font-size: 0.8125rem;
  color: #A69282;
  text-decoration: none;
  transition: color 0.2s;
}

.forgot-link:hover {
  color: #6B4F3A;
}

/* ───── Submit button ───── */
.submit-btn {
  width: 100%;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  background: #6B4F3A;
  color: #FFFFFF;
  border: none;
  border-radius: 6px;
  font-family: 'Inter', ui-sans-serif, system-ui, sans-serif;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s ease, transform 0.15s ease;
  margin-top: 8px;
}

.submit-btn:hover:not(:disabled) {
  background: #5A4230;
  transform: translateY(-1px);
}

.submit-btn:active:not(:disabled) {
  transform: translateY(0);
}

.submit-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.submit-text {
  letter-spacing: 0.02em;
}

.submit-arrow {
  display: flex;
  align-items: center;
  transition: transform 0.2s ease;
}

.submit-btn:hover .submit-arrow {
  transform: translateX(3px);
}

.submit-loading {
  display: flex;
  align-items: center;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.spinner {
  animation: spin 0.8s linear infinite;
}

/* ───── Dev tools ───── */
.dev-section {
  margin-top: 24px;
  text-align: center;
}

.dev-toggle {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 0.6875rem;
  font-family: 'Inter', ui-sans-serif, system-ui, sans-serif;
  font-weight: 500;
  color: #C4B8AC;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  background: none;
  border: none;
  cursor: pointer;
  padding: 6px 10px;
  border-radius: 4px;
  transition: color 0.2s, background 0.2s;
}

.dev-toggle:hover {
  color: #A69282;
  background: #F5F2EE;
}

.dev-panel {
  margin-top: 12px;
}

.dev-quick-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 0.75rem;
  font-family: 'Inter', ui-sans-serif, system-ui, sans-serif;
  font-weight: 500;
  color: #6B4F3A;
  background: #F5F2EE;
  border: 1px solid #E8E2DA;
  border-radius: 6px;
  padding: 8px 16px;
  cursor: pointer;
  transition: background 0.2s, border-color 0.2s;
}

.dev-quick-btn:hover {
  background: #E8E2DA;
  border-color: #C4B8AC;
}

/* ───── Copyright ───── */
.copyright {
  text-align: center;
  font-size: 0.6875rem;
  color: #C4B8AC;
  margin-top: 28px;
  letter-spacing: 0.03em;
}

/* ───── Transitions ───── */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

/* ───── Responsive ───── */
@media (max-width: 480px) {
  .login-container {
    max-width: 100%;
  }

  .brand-wordmark {
    font-size: 2rem;
  }

  .divider {
    margin: 28px 0;
  }

  .form-footer {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }
}
</style>
