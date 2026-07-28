<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useNotify } from '@/shared/composables/useNotify'
import { loginSchema } from '../validations/auth'
import { useAuthStore } from '../stores/authStore'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import InputText from 'primevue/inputtext'
import InputPassword from 'primevue/inputpassword'
import Checkbox from 'primevue/checkbox'
import Button from 'primevue/button'
import User from '@primeicons/vue/user'
import Lock from '@primeicons/vue/lock'
import Eye from '@primeicons/vue/eye'
import EyeSlash from '@primeicons/vue/eye-slash'

const router = useRouter()
const store = useAuthStore()
const notify = useNotify()

const credential = ref('')
const password = ref('')
const remember = ref(false)
const mask = ref(true)

const isLoading = computed(() => store.status === 'loading')
const fieldErrors = ref<Record<string, string>>({})

async function onSubmit() {
  fieldErrors.value = {}
  const result = loginSchema.safeParse({ credential: credential.value, password: password.value })
  if (!result.success) {
    const errs: Record<string, string> = {}
    for (const issue of result.error.issues) {
      const field = issue.path[0] as string
      if (!errs[field]) errs[field] = issue.message
    }
    fieldErrors.value = errs
    return
  }
  await store.login(result.data.credential, result.data.password)
  if (store.isAuthenticated) {
    router.replace('/')
  } else if (store.error) {
    notify.error('Login failed', store.error)
  }
}
</script>

<template>
  <div>
    <label for="email1" class="block text-surface-900 dark:text-surface-0 text-2xl font-medium mb-2"
      >Email or Username</label
    >
    <IconField class="w-full md:w-[30rem] mb-8">
      <InputIcon> <User /> </InputIcon>
      <InputText
        id="email1"
        v-model="credential"
        type="text"
        placeholder="Email address"
        class="w-full p-4 text-lg"
        autocomplete="username"
        :invalid="!!fieldErrors.credential"
      />
    </IconField>
    <small v-if="fieldErrors.credential" class="text-red-500 block -mt-6 mb-2">{{
      fieldErrors.credential
    }}</small>

    <label
      for="password1"
      class="block text-surface-900 dark:text-surface-0 font-medium text-2xl mb-2"
      >Password</label
    >
    <IconField class="mb-4 w-full">
      <InputIcon> <Lock /> </InputIcon>
      <InputPassword
        id="password1"
        v-model="password"
        placeholder="Password"
        :mask="mask"
        class="w-full"
        fluid
        :feedback="false"
        autocomplete="current-password"
        :invalid="!!fieldErrors.password"
        :pt="{ input: { class: 'p-4 text-lg' } }"
      />
      <InputIcon class="cursor-pointer" @click="mask = !mask">
        <Eye v-if="mask" :size="16" />
        <EyeSlash v-else :size="16" />
      </InputIcon>
    </IconField>
    <small v-if="fieldErrors.password" class="text-red-500 block -mt-2 mb-2">{{
      fieldErrors.password
    }}</small>

    <div class="flex items-center justify-between mt-2 mb-8 gap-8">
      <div class="flex items-center">
        <Checkbox v-model="remember" inputId="rememberme1" binary class="mr-2" />
        <label for="rememberme1">Remember me</label>
      </div>
      <span
        class="font-medium no-underline ml-2 text-right cursor-pointer text-primary"
        @click="router.push('/auth/forgot-password')"
        >Forgot password?</span
      >
    </div>

    <Button label="Sign In" class="w-full p-4 text-lg" :loading="isLoading" @click="onSubmit" />
  </div>
</template>
