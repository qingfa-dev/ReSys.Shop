<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useNotify } from '@/shared/composables/useNotify'
import { resetPasswordSchema } from '../validations/auth'
import { resetPassword } from '../services/authApi'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import InputPassword from 'primevue/inputpassword'
import Message from 'primevue/message'
import Envelope from '@primeicons/vue/envelope'
import User from '@primeicons/vue/user'
import Key from '@primeicons/vue/key'
import Lock from '@primeicons/vue/lock'
import Eye from '@primeicons/vue/eye'
import EyeSlash from '@primeicons/vue/eye-slash'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const mask = ref(true)

const { defineField, errors, handleSubmit, setFieldValue } = useForm({
  validationSchema: toTypedSchema(resetPasswordSchema),
})

const [email] = defineField('email', { validateOnModelUpdate: false })
const [userId] = defineField('userId', { validateOnModelUpdate: false })
const [token, tokenAttrs] = defineField('token', { validateOnModelUpdate: false })
const [newPassword, newPasswordAttrs] = defineField('newPassword', { validateOnModelUpdate: false })

const isSubmitting = ref(false)
const formError = ref<string | null>(null)

onMounted(() => {
  const q = route.query as Record<string, string>
  setFieldValue('email', q.email ?? '')
  setFieldValue('userId', q.userId ?? '')
  setFieldValue('token', q.token ?? '')
})

const onSubmit = handleSubmit(async (values) => {
  isSubmitting.value = true
  formError.value = null
  try {
    const result = await resetPassword({
      email: values.email,
      userId: values.userId,
      token: values.token,
      newPassword: values.newPassword,
    })
    if (result.isSuccess) {
      notify.success('Password reset successful')
      router.push('/auth/login')
    } else {
      formError.value = result.message ?? 'Invalid or expired reset link'
    }
  } catch {
    formError.value = 'Invalid or expired reset link'
  } finally {
    isSubmitting.value = false
  }
})
</script>

<template>
  <form @submit="onSubmit" class="flex flex-col gap-4 w-full md:w-120">
    <div class="flex flex-col gap-1">
      <label for="email" class="text-surface-900 dark:text-surface-0 font-medium text-2xl">Email</label>
      <IconField class="w-full">
        <InputIcon> <Envelope /> </InputIcon>
        <InputText id="email" :modelValue="email" disabled class="w-full p-4 text-lg" />
      </IconField>
    </div>

    <div class="flex flex-col gap-1">
      <label for="userId" class="text-surface-900 dark:text-surface-0 font-medium text-2xl">User ID</label>
      <IconField class="w-full">
        <InputIcon> <User /> </InputIcon>
        <InputText id="userId" :modelValue="userId" disabled class="w-full p-4 text-lg" />
      </IconField>
    </div>

    <div class="flex flex-col gap-1">
      <label for="token" class="text-surface-900 dark:text-surface-0 font-medium text-2xl">Reset Token</label>
      <IconField class="w-full">
        <InputIcon> <Key /> </InputIcon>
        <InputText id="token" v-model="token" v-bind="tokenAttrs" class="w-full p-4 text-lg" :invalid="!!errors.token" />
      </IconField>
      <small v-if="errors.token" class="text-red-500">{{ errors.token }}</small>
    </div>

    <div class="flex flex-col gap-1">
      <label for="newPassword" class="text-surface-900 dark:text-surface-0 font-medium text-2xl">New Password</label>
      <IconField class="w-full">
        <InputIcon> <Lock /> </InputIcon>
        <InputPassword id="newPassword" v-model="newPassword" v-bind="newPasswordAttrs" class="w-full" :mask="mask" :feedback="false" :invalid="!!errors.newPassword" fluid :pt="{ input: { class: 'p-4 text-lg' } }" />
        <InputIcon class="cursor-pointer" @click="mask = !mask">
          <Eye v-if="mask" :size="16" />
          <EyeSlash v-else :size="16" />
        </InputIcon>
      </IconField>
      <small v-if="errors.newPassword" class="text-red-500">{{ errors.newPassword }}</small>
    </div>

    <Message v-if="formError" severity="error" :closable="false">{{ formError }}</Message>

    <Button type="submit" label="Reset Password" class="w-full p-4 text-lg" :loading="isSubmitting" />
  </form>

  <router-link to="/auth/login" class="text-base text-primary hover:underline text-center block mt-4">
    &larr; Back to login
  </router-link>
</template>
