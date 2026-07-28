<script setup lang="ts">
import { ref, computed } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import type { ZodObject } from 'zod'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Checkbox from 'primevue/checkbox'
import Button from 'primevue/button'
import Message from 'primevue/message'

interface Props {
  validationSchema: ZodObject<any>
  credentialLabel?: string
  credentialPlaceholder?: string
  submitLabel?: string
  loading?: boolean
  serverError?: string | null
}

const props = withDefaults(defineProps<Props>(), {
  credentialLabel: 'Email or Username',
  credentialPlaceholder: 'Email address',
  submitLabel: 'Sign In',
  loading: false,
  serverError: null,
})

const emit = defineEmits<{
  submit: [data: { credential: string; password: string; remember: boolean }]
  forgotPassword: []
}>()

const credential = ref('')
const password = ref('')
const remember = ref(false)

const { errors: errorsRaw, handleSubmit } = useForm({
  validationSchema: toTypedSchema(props.validationSchema),
})

const allErrors = errorsRaw as unknown as Record<string, string | undefined>
const credentialError = computed(() => allErrors.credential)
const passwordError = computed(() => allErrors.password)

const onSubmit = handleSubmit(() => {
  emit('submit', { credential: credential.value, password: password.value, remember: remember.value })
})
</script>

<template>
  <form @submit="onSubmit" class="flex flex-col gap-4 w-full md:w-[30rem]">
    <div class="flex flex-col gap-1">
      <label for="credential" class="text-surface-900 dark:text-surface-0 font-medium text-xl">{{ credentialLabel }}</label>
      <InputText id="credential" v-model="credential" class="w-full" type="text" :placeholder="credentialPlaceholder" autocomplete="username" :invalid="!!credentialError" />
      <small v-if="credentialError" class="text-red-500">{{ credentialError }}</small>
    </div>

    <div class="flex flex-col gap-1">
      <label for="password" class="text-surface-900 dark:text-surface-0 font-medium text-xl">Password</label>
      <Password id="password" v-model="password" class="w-full" :toggleMask="true" :feedback="false" placeholder="Password" autocomplete="current-password" :invalid="!!passwordError" />
      <small v-if="passwordError" class="text-red-500">{{ passwordError }}</small>
    </div>

    <div class="flex items-center justify-between mt-2">
      <div class="flex items-center gap-2">
        <Checkbox v-model="remember" inputId="remember" binary />
        <label for="remember" class="text-surface-600 dark:text-surface-300">Remember me</label>
      </div>
      <a class="text-primary font-medium no-underline cursor-pointer hover:underline" @click="emit('forgotPassword')">Forgot password?</a>
    </div>

    <Message v-if="serverError" severity="error" :closable="false" class="mt-2">{{ serverError }}</Message>

    <Button type="submit" :label="submitLabel" class="w-full" :loading="loading" />
  </form>
</template>
