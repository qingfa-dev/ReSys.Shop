<script setup lang="ts">
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import AuthLayout from '@/shared/components/layout/AuthLayout.vue'
import { AuthForms } from '../schemas'
import { useAuth } from '../composables/useAuth'
import { AuthRequestMapper } from '../mappers/auth.request.mapper'
import PasswordStrength from '../components/PasswordStrength.vue'

const { t } = useI18n()
const route = useRoute()
const schemas = new AuthForms(t)
const { resetPassword, isLoading, serverErrors, fieldErrors } = useAuth()

const { handleSubmit, defineField, errors } = useForm({
  validationSchema: toTypedSchema(schemas.resetPassword()),
})

const [password] = defineField('password')
const [confirmPassword] = defineField('confirmPassword')

const onSubmit = handleSubmit((vals) => {
  resetPassword(AuthRequestMapper.toResetPassword(vals, {
    email: (route.query.email as string) ?? '',
    userId: (route.query.userId as string) ?? '',
    token: (route.query.token as string) ?? '',
  }))
})
</script>

<template>
  <AuthLayout>
    <div class="text-center mb-8">
      <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium mb-4">
        {{ t('auth.titles.resetPassword') }}
      </div>
      <span class="text-muted-color font-medium">{{ t('auth.titles.resetPasswordSubtitle') }}</span>
    </div>

    <form @submit="onSubmit" class="flex flex-col" novalidate>
      <label for="rspassword" class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2">
        {{ t('auth.labels.newPassword') }}
      </label>
      <Password id="rspassword" v-model="password" :toggleMask="true" :feedback="false" class="w-full md:w-120" fluid
        :invalid="!!errors.password" />
      <small v-if="errors.password" class="text-red-500 mt-1">{{ errors.password }}</small>
      <small v-if="fieldErrors.password?.length" class="text-red-500">{{ fieldErrors.password[0] }}</small>
      <PasswordStrength :password="password" />

      <label for="rsconfirmPassword" class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2 mt-4">
        {{ t('auth.labels.confirmPassword') }}
      </label>
      <Password id="rsconfirmPassword" v-model="confirmPassword" :toggleMask="true" :feedback="false"
        class="w-full md:w-120" fluid :invalid="!!errors.confirmPassword" />
      <small v-if="errors.confirmPassword" class="text-red-500 mt-1">{{ errors.confirmPassword }}</small>
      <small v-if="fieldErrors.confirmPassword?.length" class="text-red-500">{{ fieldErrors.confirmPassword[0]
      }}</small>

      <Button type="submit" :label="t('auth.actions.resetPassword')" class="w-full mt-6" :loading="isLoading"
        :disabled="isLoading" />

      <div v-if="serverErrors.length" class="mt-4">
        <small v-for="err in serverErrors" :key="err.code" class="text-red-500 block">{{ err.message }}</small>
      </div>
    </form>
  </AuthLayout>
</template>
