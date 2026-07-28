<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { loginSchema } from '../validations/auth'
import { useAuthStore } from '../stores/authStore'
import PageShell from '@ui/PageShell.vue'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Message from 'primevue/message'

const router = useRouter()
const store = useAuthStore()

const { defineField, errors, handleSubmit } = useForm({
  validationSchema: toTypedSchema(loginSchema),
})

const [credential, credentialAttrs] = defineField('credential', { validateOnModelUpdate: false })
const [password, passwordAttrs] = defineField('password', { validateOnModelUpdate: false })

const isLoading = computed(() => store.status === 'loading')
const authError = computed(() => store.error)

const onSubmit = handleSubmit(async (values) => {
  await store.login(values.credential, values.password)
  if (store.isAuthenticated) {
    router.replace('/')
  }
})
</script>

<template>
  <PageShell title="Sign In">
    <form @submit="onSubmit" class="flex flex-col gap-4 max-w-md mx-auto">
      <div class="flex flex-col gap-2">
        <label for="credential" class="text-sm font-medium">Email or Username</label>
        <InputText id="credential" v-model="credential" v-bind="credentialAttrs" autocomplete="username" class="w-full" :invalid="!!errors.credential" />
        <small v-if="errors.credential" class="text-red-500">{{ errors.credential }}</small>
      </div>

      <div class="flex flex-col gap-2">
        <label for="password" class="text-sm font-medium">Password</label>
        <Password id="password" v-model="password" v-bind="passwordAttrs" autocomplete="current-password" class="w-full" :feedback="false" :invalid="!!errors.password" toggleMask />
        <small v-if="errors.password" class="text-red-500">{{ errors.password }}</small>
      </div>

      <Message v-if="authError" severity="error" :closable="false">{{ authError }}</Message>

      <Button type="submit" label="Sign In" severity="primary" :loading="isLoading" />

      <router-link to="/auth/forgot-password" class="text-sm text-primary hover:underline text-center">
        Forgot password?
      </router-link>
    </form>
  </PageShell>
</template>
