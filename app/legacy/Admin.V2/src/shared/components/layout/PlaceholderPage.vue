<script setup lang="ts">
defineProps<{ title: string; description?: string }>()
</script>

<template>
  <div class="flex flex-col items-center justify-center min-h-[60vh] gap-4">
    <h1 class="text-2xl font-bold text-surface-900 dark:text-surface-0">{{ title }}</h1>
    <p v-if="description" class="text-surface-500">{{ description }}</p>
  </div>
</template>
