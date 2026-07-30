<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Message from 'primevue/message'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { resetPasswordSchema } from '../validations/auth'
import { resetPassword } from '../services/authApi'

const route = useRoute()
const router = useRouter()
const notify = useNotify()

const form = ref({
  email: '',
  userId: '',
  token: '',
  newPassword: '',
})
const resetResolver = zodResolver(resetPasswordSchema)
const isSubmitting = ref(false)
const formError = ref<string | null>(null)

onMounted(() => {
  const q = route.query as Record<string, string>
  form.value = {
    email: q.email ?? '',
    userId: q.userId ?? '',
    token: q.token ?? '',
    newPassword: '',
  }
})

async function onSubmit(event: FormSubmitEvent) {
  if (!event.valid) return
  isSubmitting.value = true
  formError.value = null
  try {
    const data = event.values as { email: string; userId: string; token: string; newPassword: string }
    const result = await resetPassword({
      email: data.email,
      userId: data.userId,
      token: data.token,
      newPassword: data.newPassword,
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
}
</script>

<template>
  <Form v-slot="$form" :resolver="resetResolver" :initial-values="form" class="flex flex-col gap-4 w-full md:w-120" @submit="onSubmit">
    <FormField v-slot="$field" name="email" class="flex flex-col gap-1">
      <label class="text-surface-900 dark:text-surface-0 font-medium">Email</label>
      <InputText type="email" fluid size="large" disabled />
    </FormField>

    <FormField v-slot="$field" name="userId" class="flex flex-col gap-1">
      <label class="text-surface-900 dark:text-surface-0 font-medium">User ID</label>
      <InputText fluid size="large" disabled />
    </FormField>

    <FormField v-slot="$field" name="token" class="flex flex-col gap-1">
      <label class="text-surface-900 dark:text-surface-0 font-medium">Reset Token</label>
      <InputText fluid size="large" />
      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
    </FormField>

    <FormField v-slot="$field" name="newPassword" class="flex flex-col gap-1">
      <label class="text-surface-900 dark:text-surface-0 font-medium">New Password</label>
      <InputPassword fluid size="large" :feedback="false" toggleMask />
      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
    </FormField>

    <Message v-if="formError" severity="error" :closable="false">{{ formError }}</Message>

    <Button type="submit" label="Reset Password" fluid size="large" :loading="isSubmitting" />
  </Form>

  <router-link to="/auth/login" class="text-base text-primary hover:underline text-center block mt-4">
    &larr; Back to login
  </router-link>
</template>
