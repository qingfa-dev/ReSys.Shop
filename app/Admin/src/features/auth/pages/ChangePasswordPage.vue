<script setup lang="ts">
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import { AuthForms } from '../schemas'
import { useAuth } from '../composables/useAuth'
import { AuthRequestMapper } from '../mappers/auth.request.mapper'

const { t } = useI18n()
const schemas = new AuthForms(t)
const { changePassword, isLoading, serverErrors, fieldErrors, currentUser } = useAuth()

const { handleSubmit, defineField, errors } = useForm({
  validationSchema: toTypedSchema(schemas.changePassword()),
})

const [currentPassword] = defineField('currentPassword')
const [newPassword] = defineField('newPassword')
const [confirmPassword] = defineField('confirmPassword')

const onSubmit = handleSubmit((vals) => {
  changePassword(AuthRequestMapper.toChangePassword(vals, currentUser.value?.email ?? ''))
})
</script>

<template>
  <div class="max-w-lg mx-auto mt-8">
    <div class="card p-6">
      <h2 class="text-2xl font-medium text-surface-900 dark:text-surface-0 mb-6">
        {{ t('auth.titles.changePassword') }}
      </h2>

      <form @submit="onSubmit" class="flex flex-col gap-4" novalidate>
        <div>
          <label for="cpcurrent" class="block text-surface-900 dark:text-surface-0 font-medium mb-2">
            {{ t('auth.labels.currentPassword') }}
          </label>
          <Password id="cpcurrent" v-model="currentPassword" :toggleMask="true" :feedback="false" fluid :invalid="!!errors.currentPassword" />
          <small v-if="errors.currentPassword" class="text-red-500">{{ errors.currentPassword }}</small>
          <small v-if="fieldErrors.currentPassword?.length" class="text-red-500">{{ fieldErrors.currentPassword[0] }}</small>
        </div>

        <div>
          <label for="cpnew" class="block text-surface-900 dark:text-surface-0 font-medium mb-2">
            {{ t('auth.labels.newPassword') }}
          </label>
          <Password id="cpnew" v-model="newPassword" :toggleMask="true" :feedback="false" fluid :invalid="!!errors.newPassword" />
          <small v-if="errors.newPassword" class="text-red-500">{{ errors.newPassword }}</small>
          <small v-if="fieldErrors.newPassword?.length" class="text-red-500">{{ fieldErrors.newPassword[0] }}</small>
        </div>

        <div>
          <label for="cpconfirm" class="block text-surface-900 dark:text-surface-0 font-medium mb-2">
            {{ t('auth.labels.confirmPassword') }}
          </label>
          <Password id="cpconfirm" v-model="confirmPassword" :toggleMask="true" :feedback="false" fluid :invalid="!!errors.confirmPassword" />
          <small v-if="errors.confirmPassword" class="text-red-500">{{ errors.confirmPassword }}</small>
          <small v-if="fieldErrors.confirmPassword?.length" class="text-red-500">{{ fieldErrors.confirmPassword[0] }}</small>
        </div>

        <Button type="submit" :label="t('auth.actions.updatePassword')" class="w-full" :loading="isLoading" :disabled="isLoading" />

        <div v-if="serverErrors.length" class="mt-2">
          <small v-for="err in serverErrors" :key="err.code" class="text-red-500 block">{{ err.message }}</small>
        </div>
      </form>
    </div>
  </div>
</template>
