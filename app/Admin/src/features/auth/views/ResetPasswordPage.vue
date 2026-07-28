<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useToast } from 'primevue/usetoast'
import { resetPasswordSchema } from '../validations/auth'
import { resetPassword } from '../services/authApi'
import PageShell from '@ui/PageShell.vue'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Message from 'primevue/message'

const route = useRoute()
const router = useRouter()
const toast = useToast()

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
      toast.add({ severity: 'success', summary: 'Password reset successful', life: 5000 })
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
  <PageShell title="Set New Password">
    <form @submit="onSubmit" class="flex flex-col gap-4 max-w-md mx-auto">
      <div class="flex flex-col gap-2">
        <label for="email" class="text-sm font-medium">Email</label>
        <InputText id="email" :modelValue="email" disabled class="w-full" />
      </div>

      <div class="flex flex-col gap-2">
        <label for="userId" class="text-sm font-medium">User ID</label>
        <InputText id="userId" :modelValue="userId" disabled class="w-full" />
      </div>

      <div class="flex flex-col gap-2">
        <label for="token" class="text-sm font-medium">Reset Token</label>
        <InputText id="token" v-model="token" v-bind="tokenAttrs" class="w-full" :invalid="!!errors.token" />
        <small v-if="errors.token" class="text-red-500">{{ errors.token }}</small>
      </div>

      <div class="flex flex-col gap-2">
        <label for="newPassword" class="text-sm font-medium">New Password</label>
        <Password id="newPassword" v-model="newPassword" v-bind="newPasswordAttrs" class="w-full" :feedback="false" :invalid="!!errors.newPassword" toggleMask />
        <small v-if="errors.newPassword" class="text-red-500">{{ errors.newPassword }}</small>
      </div>

      <Message v-if="formError" severity="error" :closable="false">{{ formError }}</Message>

      <Button type="submit" label="Reset Password" severity="primary" :loading="isSubmitting" />
    </form>
  </PageShell>
</template>
