<script setup lang="ts">
import FloatingConfigurator from '@/app/layout/components/FloatingConfigurator.vue'
</script>

<template>
  <div
    class="bg-surface-50 dark:bg-surface-950 flex items-center justify-center min-h-screen min-w-screen overflow-hidden">
    <FloatingConfigurator />

    <div style="border-radius: 56px; padding: 0.3rem;
             background: linear-gradient(180deg, var(--p-primary-color) 10%, rgba(33, 150, 243, 0) 30%)">
      <div class="w-full bg-surface-0 dark:bg-surface-900 py-20 px-8 sm:px-20" style="border-radius: 53px">
        <slot />
      </div>
    </div>
  </div>
</template>
