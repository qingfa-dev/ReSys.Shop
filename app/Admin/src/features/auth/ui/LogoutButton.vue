<template>
  <AppButton icon="pi pi-sign-out" label="Sign out" variant="ghost" @click="onClick" />
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { RouteName } from '@/shared/config/routes'
import { useAuthState } from '../composables/useAuthState'

const { logout, clear } = useAuthState()
const router = useRouter()

async function onClick() {
  await logout.mutateAsync()
  clear()
  await router.push({ name: RouteName.Login })
}
</script>
