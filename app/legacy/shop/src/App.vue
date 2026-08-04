<script setup lang="ts">
import { onMounted } from 'vue'
import DefaultLayout from '@/app/layouts/DefaultLayout.vue'
import Toast from 'primevue/toast'
import { useAuthStore } from '@/features/identity/store/auth'

const authStore = useAuthStore()

onMounted(() => {
  authStore.initialize()
})
</script>

<template>
  <DefaultLayout>
    <RouterView />
  </DefaultLayout>
  <Toast position="top-right" />
</template>
