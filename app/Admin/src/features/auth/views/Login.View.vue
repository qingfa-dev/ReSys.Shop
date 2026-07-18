<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth.store'
import { storeToRefs } from 'pinia'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { createLoginSchema } from '../schemas/login.schema'
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
  validationSchema: toTypedSchema(createLoginSchema(t)),
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
  <div class="min-h-screen flex items-center justify-center p-8 bg-leather-50">
    <div
      :class="{
        'opacity-100 translate-y-0': mounted,
        'opacity-0 translate-y-3': !mounted,
      }"
      class="w-full max-w-md transition-all duration-600 ease-out"
    >
      <div class="text-center">
        <h1 class="login-brand-wordmark font-serif italic text-4xl/none leading-tight tracking-tight text-surface-900 m-0">
          ReSys<span class="text-leather-500">.</span><span class="text-leather-500 not-italic">Shop</span>
        </h1>
        <p class="text-xs text-leather-300 tracking-widest uppercase font-medium mt-2">{{ t('auth.titles.app_subtitle') }}</p>
      </div>

      <div class="flex items-center gap-3 my-9">
        <span class="flex-1 h-px bg-leather-200" />
        <span class="text-leather-500 opacity-60" style="font-size: 8px; line-height: 1">&#9670;</span>
        <span class="flex-1 h-px bg-leather-200" />
      </div>

      <form @submit="onSubmit" class="flex flex-col gap-5" novalidate>
        <div class="flex flex-col gap-1.5">
          <label for="credential" class="text-xs font-semibold text-leather-500 uppercase tracking-widest">{{
            t('auth.labels.credential')
          }}</label>
          <InputText
            id="credential"
            v-model="credential"
            :placeholder="t('auth.placeholders.credential')"
            :invalid="!!errors.credential"
            :disabled="loading"
            class="h-12"
            autocomplete="username"
          />
          <Message v-if="errors.credential" severity="error" variant="simple" size="small">{{ errors.credential }}</Message>
        </div>

        <div class="flex flex-col gap-1.5">
          <label for="password" class="text-xs font-semibold text-leather-500 uppercase tracking-widest">{{
            t('auth.labels.password')
          }}</label>
          <Password
            id="password"
            v-model="password"
            :placeholder="t('auth.placeholders.password')"
            :invalid="!!errors.password"
            :disabled="loading"
            :feedback="false"
            toggleMask
            class="h-12"
            autocomplete="current-password"
          />
          <Message v-if="errors.password" severity="error" variant="simple" size="small">{{ errors.password }}</Message>
        </div>

        <div class="flex items-center justify-between mt-1">
          <div class="flex items-center gap-2">
            <Checkbox inputId="remember" v-model="rememberMe" :disabled="loading" :binary="true" />
            <label for="remember" class="text-sm text-leather-500 cursor-pointer select-none">{{
              t('auth.labels.remember_me')
            }}</label>
          </div>
          <a href="#" class="text-sm text-leather-300 hover:text-leather-500 no-underline transition-colors">{{
            t('auth.labels.forgot_password')
          }}</a>
        </div>

        <Button
          type="submit"
          :label="t('auth.labels.sign_in')"
          :loading="loading"
          icon="pi pi-arrow-right"
          iconPos="right"
          class="mt-2 h-12"
        />
      </form>

      <div class="mt-6 text-center">
        <button
          class="inline-flex items-center gap-1.5 text-xs text-leather-200 hover:text-leather-400 hover:bg-leather-50 uppercase tracking-widest font-medium bg-transparent border-none cursor-pointer px-2.5 py-1.5 rounded transition-colors"
          @click="showDevTools = !showDevTools"
          type="button"
        >
          <i class="pi pi-code" />
          {{ showDevTools ? 'close' : 'dev' }}
        </button>
        <div v-if="showDevTools" class="mt-3">
          <Button
            type="button"
            label="Login as admin@resys.shop"
            icon="pi pi-plus"
            @click="fillSeedCredentials"
            :disabled="loading"
            size="small"
            severity="secondary"
            text
          />
        </div>
      </div>

      <p class="text-center text-xs text-leather-200 mt-7 tracking-wider">
        {{ t('auth.messages.copyright', { year: new Date().getFullYear().toString() }) }}
      </p>
    </div>
  </div>
</template>

<style scoped>
.login-brand-wordmark {
  font-family: 'DM Serif Display', ui-serif, Georgia, serif;
}
</style>
