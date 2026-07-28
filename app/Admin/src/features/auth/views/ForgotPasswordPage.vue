<script setup lang="ts">
import { ref } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { forgotPasswordSchema } from '../validations/auth'
import { forgotPassword } from '../services/authApi'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import FloatLabel from 'primevue/floatlabel'
import Message from 'primevue/message'
import Envelope from '@primeicons/vue/envelope'

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

  <form v-else @submit="onSubmit" class="flex flex-col gap-4 w-full md:w-120">
    <div class="flex flex-col gap-1">
      <FloatLabel variant="on">
        <IconField>
          <InputIcon> <Envelope /> </InputIcon>
          <InputText
            id="email"
            v-model="email"
            v-bind="emailAttrs"
            fluid
            size="large"
            type="email"
            placeholder="Email address"
            autocomplete="email"
            :invalid="!!errors.email"
          />
        </IconField>
        <label for="email">Email</label>
      </FloatLabel>
      <small v-if="errors.email" class="text-red-500">{{ errors.email }}</small>
    </div>

    <Message v-if="submitError" severity="error" :closable="false">{{ submitError }}</Message>

    <Button type="submit" label="Send Reset Link" fluid size="large" :loading="isSubmitting" />
  </form>

  <router-link to="/auth/login" class="text-sm text-primary hover:underline text-center block mt-4">
    &larr; Back to login
  </router-link>
</template>
