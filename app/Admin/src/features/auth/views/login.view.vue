<script setup lang="ts">
import { computed, ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth.store';
import { storeToRefs } from 'pinia';
import { useForm } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/zod';
import { LoginSchema } from '../schemas/auth.schema';
import { authLocales } from '../locales/auth.locales';
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use';
import AppBrandMark from '@/shared/components/AppBrandMark.vue'

const router = useRouter();
const authStore = useAuthStore();
const { loading } = storeToRefs(authStore);
const { handleApiResult } = useApiErrorHandler();

const mounted = ref(false)

onMounted(() => {
  setTimeout(() => { mounted.value = true }, 50)
})

const { defineField, handleSubmit, errors, setErrors, values, setValues } = useForm({
  validationSchema: toTypedSchema(LoginSchema),
  initialValues: {
    credential: '',
    password: '',
    rememberMe: false,
  },
});

const [credential] = defineField('credential');
const [password] = defineField('password');
const [rememberMe] = defineField('rememberMe');

const onSubmit = handleSubmit(async (formValues) => {
  const result = await authStore.login(formValues);
  const handled = handleApiResult(result, {
    setErrors,
    fieldNames: Object.keys(values),
    successTitle: authLocales.titles?.welcome,
    successMessage: authLocales.messages?.login_success,
    errorTitle: authLocales.common?.error,
    genericError: authLocales.messages?.login_failed,
  });
  if (handled && result.success) {
    router.push('/');
  }
});

const showDevTools = ref(false)

const fillSeedCredentials = () => {
  setValues({
    credential: 'admin@resys.shop',
    password: 'Admin@1234!',
  });
};
</script>

<template>
  <div class="login-page">
    <div class="login-grid">
      <!-- Brand panel -->
      <div class="brand-panel">
        <div class="brand-content" :class="{ 'is-visible': mounted }">
          <div class="brand-mark">
            <AppBrandMark :size="64" />
          </div>
          <h1 class="brand-title">
            ReSys<span class="brand-dot">.</span><span class="brand-shop">Shop</span>
          </h1>
          <p class="brand-tagline">{{ authLocales.titles?.app_subtitle }}</p>
        </div>
      </div>

      <!-- Form panel -->
      <div class="form-panel">
        <div class="form-container" :class="{ 'is-visible': mounted }">
          <div class="form-card">
            <h2 class="form-heading">{{ authLocales.titles?.login }}</h2>

            <form @submit="onSubmit" class="login-form">
              <div class="field">
                <label for="credential">{{ authLocales.labels?.credential }}</label>
                <div class="input-wrapper" :class="{ 'has-error': !!errors.credential }">
                  <span class="input-icon pi pi-envelope"></span>
                  <InputText
                    id="credential"
                    v-model="credential"
                    type="text"
                    :placeholder="authLocales.placeholders?.credential"
                    :disabled="loading"
                    :invalid="!!errors.credential"
                  />
                </div>
                <Transition name="fade">
                  <small v-if="errors.credential" class="field-error">{{ errors.credential }}</small>
                </Transition>
              </div>

              <div class="field">
                <label for="password">{{ authLocales.labels?.password }}</label>
                <div class="input-wrapper" :class="{ 'has-error': !!errors.password }">
                  <span class="input-icon pi pi-lock"></span>
                  <Password
                    id="password"
                    v-model="password"
                    :feedback="false"
                    toggleMask
                    :placeholder="authLocales.placeholders?.password"
                    :disabled="loading"
                    :invalid="!!errors.password"
                  />
                </div>
                <Transition name="fade">
                  <small v-if="errors.password" class="field-error">{{ errors.password }}</small>
                </Transition>
              </div>

              <div class="form-options">
                <div class="checkbox-group">
                  <Checkbox id="rememberMe" v-model="rememberMe" :binary="true" :disabled="loading" />
                  <label for="rememberMe">{{ authLocales.labels?.remember_me }}</label>
                </div>
                <a href="#" class="forgot-link">{{ authLocales.labels?.forgot_password }}</a>
              </div>

              <Button
                type="submit"
                :label="authLocales.labels?.sign_in"
                icon="pi pi-arrow-right"
                iconPos="right"
                class="submit-btn"
                :loading="loading"
              />
            </form>
          </div>

          <!-- Dev tools toggle -->
          <div class="dev-tools">
            <button class="dev-toggle" @click="showDevTools = !showDevTools">
              <span class="pi pi-code"></span>
              {{ showDevTools ? 'Hide' : 'Dev' }}
            </button>
            <Transition name="fade">
              <div v-if="showDevTools" class="dev-panel">
                <Button
                  type="button"
                  label="Quick Login (Seed Admin)"
                  icon="pi pi-bolt"
                  severity="secondary"
                  outlined
                  class="w-full"
                  @click="fillSeedCredentials"
                  :disabled="loading"
                />
              </div>
            </Transition>
          </div>

          <p class="copyright">{{ authLocales.messages?.copyright?.replace('{year}', new Date().getFullYear().toString()) }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.login-page {
  min-height: 100vh;
  background: #F8F7F4;
  background-image:
    linear-gradient(rgba(0, 0, 0, 0.04) 1px, transparent 1px),
    linear-gradient(90deg, rgba(0, 0, 0, 0.04) 1px, transparent 1px);
  background-size: 48px 48px;
}

.login-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  min-height: 100vh;
}

/* ───── Brand panel ───── */
.brand-panel {
  display: flex;
  align-items: center;
  justify-content: center;
  background: #1C1917;
  position: relative;
  overflow: hidden;
}

.brand-panel::before {
  content: '';
  position: absolute;
  inset: 0;
  background:
    radial-gradient(ellipse at 30% 50%, rgba(180, 83, 9, 0.15) 0%, transparent 60%),
    radial-gradient(ellipse at 70% 50%, rgba(180, 83, 9, 0.08) 0%, transparent 50%);
}

.brand-content {
  position: relative;
  text-align: center;
  opacity: 0;
  transform: translateY(12px);
  transition: opacity 0.7s ease-out, transform 0.7s ease-out;
}

.brand-content.is-visible {
  opacity: 1;
  transform: translateY(0);
}

.brand-mark {
  display: flex;
  justify-content: center;
  margin-bottom: 24px;
}

.brand-title {
  font-family: 'Sora', ui-sans-serif, system-ui, sans-serif;
  font-size: 2.25rem;
  font-weight: 700;
  letter-spacing: -0.03em;
  color: #FAFAF9;
  margin: 0 0 8px 0;
  line-height: 1.2;
}

.brand-dot {
  color: #B45309;
}

.brand-shop {
  font-weight: 500;
  color: #A8A29E;
}

.brand-tagline {
  font-size: 0.875rem;
  color: #78716C;
  margin: 0;
  letter-spacing: 0.02em;
}

/* ───── Form panel ───── */
.form-panel {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 32px;
}

.form-container {
  width: 100%;
  max-width: 400px;
  opacity: 0;
  transform: translateY(16px);
  transition: opacity 0.5s ease-out 0.2s, transform 0.5s ease-out 0.2s;
}

.form-container.is-visible {
  opacity: 1;
  transform: translateY(0);
}

.form-card {
  background: #FFFFFF;
  border-radius: 16px;
  padding: 40px 36px;
  box-shadow:
    0 1px 3px rgba(0, 0, 0, 0.04),
    0 8px 24px rgba(0, 0, 0, 0.06);
}

.form-heading {
  font-family: 'Sora', ui-sans-serif, system-ui, sans-serif;
  font-size: 1.5rem;
  font-weight: 600;
  color: #1C1917;
  margin: 0 0 28px 0;
  line-height: 1.3;
}

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
  font-size: 0.8125rem;
  font-weight: 500;
  color: #44403C;
  letter-spacing: 0.01em;
}

.input-wrapper {
  position: relative;
}

.input-wrapper .input-icon {
  position: absolute;
  left: 12px;
  top: 50%;
  transform: translateY(-50%);
  color: #A8A29E;
  font-size: 0.875rem;
  z-index: 1;
  pointer-events: none;
}

.input-wrapper :deep(.p-inputtext),
.input-wrapper :deep(.p-password) {
  width: 100%;
}

.input-wrapper :deep(.p-inputtext) {
  padding-left: 36px;
  height: 44px;
  border-radius: 10px;
  border: 1.5px solid #E7E5E4;
  background: #FAFAF9;
  font-size: 0.875rem;
  transition: border-color 0.2s, box-shadow 0.2s, background 0.2s;
}

.input-wrapper :deep(.p-inputtext):focus {
  border-color: #B45309;
  box-shadow: 0 0 0 3px rgba(180, 83, 9, 0.1);
  background: #FFFFFF;
}

.input-wrapper :deep(.p-inputtext)::placeholder {
  color: #D6D3D1;
}

.input-wrapper :deep(.p-password-input) {
  padding-left: 36px !important;
}

.input-wrapper.has-error :deep(.p-inputtext) {
  border-color: #DC2626;
}

.field-error {
  color: #DC2626;
  font-size: 0.75rem;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.form-options {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.checkbox-group {
  display: flex;
  align-items: center;
  gap: 8px;
}

.checkbox-group label {
  font-size: 0.8125rem;
  color: #44403C;
  cursor: pointer;
}

.forgot-link {
  font-size: 0.8125rem;
  font-weight: 500;
  color: #B45309;
  text-decoration: none;
  transition: color 0.2s;
}

.forgot-link:hover {
  color: #92400E;
}

.submit-btn {
  width: 100%;
  height: 44px;
  border-radius: 10px;
  background: #B45309;
  border: none;
  font-weight: 600;
  font-size: 0.875rem;
  transition: background 0.2s, transform 0.15s, box-shadow 0.2s;
  margin-top: 4px;
}

.submit-btn:hover {
  background: #92400E;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(180, 83, 9, 0.25);
}

.submit-btn:active {
  transform: translateY(0);
}

/* ───── Dev tools ───── */
.dev-tools {
  margin-top: 16px;
  text-align: center;
}

.dev-toggle {
  font-size: 0.75rem;
  color: #A8A29E;
  background: none;
  border: none;
  cursor: pointer;
  padding: 6px 12px;
  border-radius: 6px;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  transition: color 0.2s, background 0.2s;
}

.dev-toggle:hover {
  color: #78716C;
  background: rgba(0, 0, 0, 0.04);
}

.dev-panel {
  margin-top: 12px;
}

/* ───── Copyright ───── */
.copyright {
  text-align: center;
  font-size: 0.75rem;
  color: #A8A29E;
  margin-top: 24px;
}

/* ───── Responsive ───── */
@media (max-width: 768px) {
  .login-grid {
    grid-template-columns: 1fr;
  }

  .brand-panel {
    min-height: 180px;
    padding: 32px 24px;
  }

  .brand-title {
    font-size: 1.75rem;
  }

  .form-panel {
    padding: 24px 16px;
  }

  .form-card {
    padding: 28px 24px;
  }
}
</style>
