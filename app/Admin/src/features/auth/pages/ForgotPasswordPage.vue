<!-- features/auth/pages/ForgotPasswordPage.vue -->
<script setup lang="ts">
import { ref } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import AuthLayout from '@/shared/components/layout/AuthLayout.vue'
import { useAuth } from '../composables/useAuth'

defineOptions({ name: 'ForgotPasswordPage' })

const { t } = useI18n()
const { forgotPassword, forgotPasswordSchema, isLoading, serverErrors } = useAuth()

const { handleSubmit, defineField, errors } = useForm({
  validationSchema: toTypedSchema(forgotPasswordSchema),
})

const [email] = defineField('email')
const submitted = ref(false)

const onSubmit = handleSubmit((values) => {
  forgotPassword(values.email)
  submitted.value = true
})
</script>

<template>
  <AuthLayout>
    <div class="text-center mb-8">
      <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium mb-4">
        {{ t('auth.titles.forgotPassword') }}
      </div>
      <span class="text-muted-color font-medium">{{ t('auth.titles.forgotPasswordSubtitle') }}</span>
    </div>

    <div v-if="submitted && !serverErrors.length" class="text-center">
      <i class="pi pi-check-circle text-green-500 text-4xl mb-4" />
      <p class="text-surface-900 dark:text-surface-0 font-medium">{{ t('auth.messages.forgotPasswordSent') }}</p>
      <router-link to="/login" class="text-primary font-medium mt-4 inline-block">
        {{ t('auth.actions.backToLogin') }}
      </router-link>
    </div>

    <form v-else @submit="onSubmit" class="flex flex-col" novalidate>
      <label for="fpemail" class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2">
        {{ t('auth.labels.email') }}
      </label>
      <InputText id="fpemail" v-model="email" type="email" class="w-full md:w-[30rem] mb-4" :invalid="!!errors.email" />
      <small v-if="errors.email" class="text-red-500 -mt-3 mb-2">{{ errors.email }}</small>

      <Button type="submit" :label="t('auth.actions.sendResetLink')" class="w-full" :loading="isLoading" :disabled="isLoading" />

      <div v-if="serverErrors.length" class="mt-4">
        <small v-for="err in serverErrors" :key="err.code" class="text-red-500 block">{{ err.message }}</small>
      </div>

      <p class="text-center text-muted-color mt-4 text-sm">
        <router-link to="/login" class="text-primary font-medium">{{ t('auth.actions.backToLogin') }}</router-link>
      </p>
    </form>
  </AuthLayout>
</template>
