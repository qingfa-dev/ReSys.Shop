<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { forgotPasswordSchema } from '../validations/auth'
import { forgotPassword } from '../services/authApi'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'

const router = useRouter()

const { defineField, errors, handleSubmit } = useForm({
  validationSchema: toTypedSchema(forgotPasswordSchema),
})

const [email, emailAttrs] = defineField('email', { validateOnModelUpdate: false })

const isSubmitting = ref(false)
const isSuccess = ref(false)
const submitError = ref<string | null>(null)

const onSubmit = handleSubmit(async (values) => {
  isSubmitting.value = true
  submitError.value = null
  try {
    await forgotPassword({ email: values.email })
    isSuccess.value = true
  } catch {
    submitError.value = 'Something went wrong. Please try again.'
  } finally {
    isSubmitting.value = false
  }
})
</script>

<template>
  <p v-if="isSuccess" class="text-green-600 font-medium text-center">
    If an account exists with that email, a reset link has been sent.
  </p>

  <form v-else @submit="onSubmit" class="flex flex-col gap-4 w-full md:w-[30rem]">
    <div class="flex flex-col gap-1">
      <label for="email" class="text-surface-900 dark:text-surface-0 font-medium text-xl">Email</label>
      <InputText id="email" v-model="email" v-bind="emailAttrs" class="w-full" type="email" placeholder="Email address" autocomplete="email" :invalid="!!errors.email" />
      <small v-if="errors.email" class="text-red-500">{{ errors.email }}</small>
    </div>

    <Message v-if="submitError" severity="error" :closable="false">{{ submitError }}</Message>

    <Button type="submit" label="Send Reset Link" class="w-full" :loading="isSubmitting" />
  </form>

  <router-link to="/auth/login" class="text-sm text-primary hover:underline text-center block mt-4">
    &larr; Back to login
  </router-link>
</template>
