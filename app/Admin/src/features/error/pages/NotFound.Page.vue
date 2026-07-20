<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()
</script>

<template>
  <div class="bg-surface-50 dark:bg-surface-950 flex items-center justify-center min-h-screen min-w-[100vw] overflow-hidden">
    <div class="flex flex-col items-center justify-center">
      <div style="border-radius: 56px; padding: 0.3rem; background: linear-gradient(180deg, var(--primary-color) 10%, rgba(33, 150, 243, 0) 30%)">
        <div class="w-full bg-surface-0 dark:bg-surface-900 py-20 px-8 sm:px-20 flex flex-col items-center gap-6" style="border-radius: 53px">
          <div class="text-surface-900 dark:text-surface-0 text-6xl font-bold">404</div>
          <div class="text-surface-600 dark:text-surface-400 text-xl">Page not found</div>
          <Button label="Go to Dashboard" @click="router.push('/')" />
        </div>
      </div>
    </div>
  </div>
</template>
