<script setup lang="ts">
import { useAuthStore } from '@/features/identity/store/auth'

defineProps<{
  title?: string
  subtitle?: string
}>()

const authStore = useAuthStore()
</script>

<template>
  <div class="account-header">
    <h1>{{ title || 'My Account' }}</h1>
    <p>{{ subtitle || `Welcome back, ${authStore.user?.firstName || 'Guest'}` }}</p>
  </div>
</template>

<style scoped lang="scss">
.account-header {
  margin-bottom: 2rem;

  h1 {
    font-size: var(--font-size-3xl);
    margin-bottom: 0.5rem;
  }

  p {
    color: var(--color-text-muted);
  }
}
</style>