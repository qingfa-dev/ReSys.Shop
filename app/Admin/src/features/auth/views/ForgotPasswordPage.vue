<script setup lang="ts">
import { ref } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { forgotPasswordSchema } from '../validations/auth'
import { forgotPassword } from '../services/authApi'
import PageShell from '@ui/PageShell.vue'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'

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
  <PageShell title="Forgot Password">
    <router-link to="/auth/login" class="text-sm text-primary hover:underline">&larr; Back to login</router-link>

    <p v-if="isSuccess" class="mt-4 text-green-600">
      If an account exists with that email, a reset link has been sent.
    </p>

    <form v-else @submit="onSubmit" class="flex flex-col gap-4 max-w-md mx-auto mt-4">
      <div class="flex flex-col gap-2">
        <label for="email" class="text-sm font-medium">Email</label>
        <InputText id="email" v-model="email" v-bind="emailAttrs" autocomplete="email" class="w-full" :invalid="!!errors.email" />
        <small v-if="errors.email" class="text-red-500">{{ errors.email }}</small>
      </div>

      <Message v-if="submitError" severity="error" :closable="false">{{ submitError }}</Message>

      <Button type="submit" label="Send Reset Link" severity="primary" :loading="isSubmitting" />
    </form>
  </PageShell>
</template>
