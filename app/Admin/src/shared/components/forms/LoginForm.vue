<script setup lang="ts">
import { ref, computed } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import type { ZodObject } from 'zod'
import InputText from 'primevue/inputtext'
import InputPassword from 'primevue/inputpassword'
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
  <form @submit="onSubmit" class="w-full md:w-[30rem]">
    <div class="mb-8">
      <label for="credential" class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2">{{ credentialLabel }}</label>
      <InputText id="credential" v-model="credential" class="w-full" type="text" :placeholder="credentialPlaceholder" autocomplete="username" :invalid="!!credentialError" />
      <small v-if="credentialError" class="text-red-500">{{ credentialError }}</small>
    </div>

    <div class="mb-4">
      <label for="password" class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2">Password</label>
      <InputPassword id="password" v-model="password" class="w-full" fluid :toggleMask="true" :feedback="false" placeholder="Password" autocomplete="current-password" :invalid="!!passwordError" />
      <small v-if="passwordError" class="text-red-500">{{ passwordError }}</small>
    </div>

    <div class="flex items-center justify-between mt-2 mb-8 gap-8">
      <div class="flex items-center">
        <Checkbox v-model="remember" inputId="remember" binary class="mr-2" />
        <label for="remember" class="text-surface-600 dark:text-surface-300">Remember me</label>
      </div>
      <span class="font-medium no-underline ml-2 text-right cursor-pointer text-primary hover:underline" @click="emit('forgotPassword')">Forgot password?</span>
    </div>

    <Message v-if="serverError" severity="error" :closable="false" class="mb-4" />

    <Button type="submit" :label="submitLabel" class="w-full" :loading="loading" />
  </form>
</template>
