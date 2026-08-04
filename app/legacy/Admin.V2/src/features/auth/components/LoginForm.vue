<script setup lang="ts">
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import { AuthForms } from '../schemas'
import { useAuth } from '../composables/useAuth'
import { AuthRequestMapper } from '../mappers/auth.request.mapper'

const { t } = useI18n()
const schemas = new AuthForms(t)
const { login, isLoading, serverErrors, fieldErrors } = useAuth()

const { handleSubmit, defineField, errors } = useForm({
  validationSchema: toTypedSchema(schemas.login()),
})

const [credential] = defineField('credential')
const [password] = defineField('password')

const onSubmit = handleSubmit((values) => {
  const req = AuthRequestMapper.toLogin(values)
  login(req.credential, req.password)
})
</script>

<template>
  <div>
    <div class="text-center mb-8">
      <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium mb-4">
        {{ t('auth.titles.welcome') }}
      </div>
      <span class="text-muted-color font-medium">{{ t('auth.titles.login') }}</span>
    </div>

    <form @submit="onSubmit" class="flex flex-col" novalidate>
      <label for="credential" class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2">
        {{ t('auth.labels.credential') }}
      </label>
      <InputText
        id="credential"
        v-model="credential"
        :placeholder="t('auth.placeholders.credential')"
        class="w-full md:w-120 mb-4"
        :invalid="!!errors.credential"
      />
      <small v-if="errors.credential" class="text-red-500 -mt-3 mb-2">{{ errors.credential }}</small>
      <small v-if="fieldErrors.credential?.length" class="text-red-500 -mt-3 mb-2">{{ fieldErrors.credential[0] }}</small>

      <label for="password1" class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2">
        {{ t('auth.labels.password') }}
      </label>
      <Password
        id="password1"
        v-model="password"
        :placeholder="t('auth.placeholders.password')"
        :toggleMask="true"
        :feedback="false"
        class="mb-4"
        fluid
        :invalid="!!errors.password"
      />
      <small v-if="errors.password" class="text-red-500 -mt-3 mb-2">{{ errors.password }}</small>
      <small v-if="fieldErrors.password?.length" class="text-red-500 -mt-3 mb-2">{{ fieldErrors.password[0] }}</small>

      <div class="flex items-center justify-between mt-2 mb-8 gap-8">
        <div />
        <router-link to="/forgot-password" class="font-medium no-underline ml-2 text-right cursor-pointer text-primary">
          {{ t('auth.labels.forgot_password') }}
        </router-link>
      </div>

      <Button type="submit" :label="t('auth.actions.sign_in')" class="w-full" :loading="isLoading" :disabled="isLoading" />

      <div v-if="serverErrors.length" class="mt-4">
        <small v-for="err in serverErrors" :key="err.code" class="text-red-500 block">{{ err.message }}</small>
      </div>
    </form>
  </div>
</template>
