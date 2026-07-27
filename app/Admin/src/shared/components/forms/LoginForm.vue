<script setup lang="ts">
import { ref } from 'vue'

interface LoginData {
  email: string
  password: string
  remember: boolean
}

interface Props {
  submitLabel?: string
  loading?: boolean
}

withDefaults(defineProps<Props>(), {
  submitLabel: 'Sign In',
  loading: false,
})

const emit = defineEmits<{
  (e: 'submit', data: LoginData): void
  (e: 'forgotPassword'): void
}>()

const email = ref('')
const password = ref('')
const remember = ref(false)
</script>

<template>
  <form @submit.prevent="emit('submit', { email, password, remember })" class="flex flex-col gap-4 w-full md:w-[30rem]">
    <div class="flex flex-col gap-1">
      <label for="email" class="text-surface-900 dark:text-surface-0 font-medium">Email</label>
      <InputText id="email" v-model="email" class="w-full" type="email" placeholder="Email address" />
    </div>
    <div class="flex flex-col gap-1">
      <label for="password" class="text-surface-900 dark:text-surface-0 font-medium">Password</label>
      <Password id="password" v-model="password" class="w-full" :toggleMask="true" :feedback="false" placeholder="Password" />
    </div>
    <div class="flex items-center justify-between">
      <div class="flex items-center gap-2">
        <Checkbox v-model="remember" inputId="remember" binary />
        <label for="remember" class="text-surface-600 dark:text-surface-300">Remember me</label>
      </div>
      <a class="text-primary font-medium hover:underline cursor-pointer" @click="emit('forgotPassword')">Forgot password?</a>
    </div>
    <Button type="submit" :label="submitLabel" class="w-full" :loading="loading" />
  </form>
</template>
