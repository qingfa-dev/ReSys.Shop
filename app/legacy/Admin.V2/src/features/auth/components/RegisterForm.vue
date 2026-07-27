<script setup lang="ts">
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import { AuthForms } from '../schemas'
import { useAuth } from '../composables/useAuth'
import { AuthRequestMapper } from '../mappers/auth.request.mapper'
import PasswordStrength from './PasswordStrength.vue'

const { t } = useI18n()
const schemas = new AuthForms(t)
const { register, isLoading, serverErrors, fieldErrors } = useAuth()

const { handleSubmit, defineField, errors } = useForm({
  validationSchema: toTypedSchema(schemas.register()),
  initialValues: {
    email: '',
    userName: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
    phone: '',
    acceptTerm: false,
  },
})

const [email] = defineField('email')
const [userName] = defineField('userName')
const [password] = defineField('password')
const [confirmPassword] = defineField('confirmPassword')
const [firstName] = defineField('firstName')
const [lastName] = defineField('lastName')
const [phone] = defineField('phone')
const [acceptTerm] = defineField('acceptTerm')

const onSubmit = handleSubmit((vals) => {
  register(AuthRequestMapper.toRegister(vals))
})
</script>

<template>
  <div>
    <div class="text-center mb-8">
      <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium mb-4">
        {{ t('auth.titles.register') }}
      </div>
      <span class="text-muted-color font-medium">{{ t('auth.titles.createAccount') }}</span>
    </div>

    <form @submit="onSubmit" class="flex flex-col" novalidate>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label for="firstName" class="block text-surface-900 dark:text-surface-0 font-medium mb-2">
            {{ t('auth.labels.firstName') }}
          </label>
          <InputText id="firstName" v-model="firstName" class="w-full" :invalid="!!errors.firstName" />
          <small v-if="errors.firstName" class="text-red-500">{{ errors.firstName }}</small>
          <small v-if="fieldErrors.firstName?.length" class="text-red-500">{{ fieldErrors.firstName[0] }}</small>
        </div>

        <div>
          <label for="lastName" class="block text-surface-900 dark:text-surface-0 font-medium mb-2">
            {{ t('auth.labels.lastName') }}
          </label>
          <InputText id="lastName" v-model="lastName" class="w-full" />
        </div>
      </div>

      <label for="email" class="block text-surface-900 dark:text-surface-0 font-medium mb-2 mt-4">
        {{ t('auth.labels.email') }}
      </label>
      <InputText id="email" v-model="email" type="email" class="w-full" :invalid="!!errors.email" />
      <small v-if="errors.email" class="text-red-500">{{ errors.email }}</small>
      <small v-if="fieldErrors.email?.length" class="text-red-500">{{ fieldErrors.email[0] }}</small>

      <label for="userName" class="block text-surface-900 dark:text-surface-0 font-medium mb-2 mt-4">
        {{ t('auth.labels.userName') }}
      </label>
      <InputText id="userName" v-model="userName" class="w-full" :invalid="!!errors.userName" />
      <small v-if="errors.userName" class="text-red-500">{{ errors.userName }}</small>
      <small v-if="fieldErrors.userName?.length" class="text-red-500">{{ fieldErrors.userName[0] }}</small>

      <label for="password" class="block text-surface-900 dark:text-surface-0 font-medium mb-2 mt-4">
        {{ t('auth.labels.password') }}
      </label>
      <Password id="password" v-model="password" :toggleMask="true" :feedback="false"
        class="w-full" fluid :invalid="!!errors.password" />
      <small v-if="errors.password" class="text-red-500">{{ errors.password }}</small>
      <small v-if="fieldErrors.password?.length" class="text-red-500">{{ fieldErrors.password[0] }}</small>
      <PasswordStrength :password="password" />

      <label for="confirmPassword" class="block text-surface-900 dark:text-surface-0 font-medium mb-2 mt-4">
        {{ t('auth.labels.confirmPassword') }}
      </label>
      <Password id="confirmPassword" v-model="confirmPassword" :toggleMask="true"
        :feedback="false" class="w-full" fluid :invalid="!!errors.confirmPassword" />
      <small v-if="errors.confirmPassword" class="text-red-500">{{ errors.confirmPassword }}</small>
      <small v-if="fieldErrors.confirmPassword?.length" class="text-red-500">{{ fieldErrors.confirmPassword[0] }}</small>

      <label for="phone" class="block text-surface-900 dark:text-surface-0 font-medium mb-2 mt-4">
        {{ t('auth.labels.phone') }}
      </label>
      <InputText id="phone" v-model="phone" class="w-full" />

      <div class="flex items-center mt-4 gap-2">
        <Checkbox id="acceptTerm" v-model="acceptTerm" binary />
        <label for="acceptTerm" class="text-surface-900 dark:text-surface-0">
          {{ t('auth.labels.acceptTerms') }}
        </label>
      </div>
      <small v-if="errors.acceptTerm" class="text-red-500">{{ errors.acceptTerm }}</small>
      <small v-if="fieldErrors.acceptTerm?.length" class="text-red-500">{{ fieldErrors.acceptTerm[0] }}</small>

      <Button type="submit" :label="t('auth.actions.register')" class="w-full mt-6" :loading="isLoading"
        :disabled="isLoading" />

      <div v-if="serverErrors.length" class="mt-4">
        <small v-for="err in serverErrors" :key="err.code" class="text-red-500 block">{{ err.message }}</small>
      </div>

      <p class="text-center text-muted-color mt-4 text-sm">
        {{ t('auth.messages.alreadyHaveAccount') }}
        <router-link to="/login" class="text-primary font-medium">{{ t('auth.actions.sign_in') }}</router-link>
      </p>
    </form>
  </div>
</template>
