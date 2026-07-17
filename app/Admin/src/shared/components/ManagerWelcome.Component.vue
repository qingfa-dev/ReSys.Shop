<script setup lang="ts">
defineProps<{
  title: string;
  description: string;
  icon: string;
}>();
</script>

<template>
  <div
    class="flex-1 flex flex-col items-center justify-center bg-surface-50/50 dark:bg-surface-950/20 rounded-3xl border-2 border-dashed border-surface-200 dark:border-surface-800">
    <div class="w-20 h-20 rounded-full bg-surface-100 dark:bg-surface-800 flex items-center justify-center mb-6">
      <i :class="['pi', icon, 'text-4xl text-surface-300']"></i>
    </div>
    <h3 class="text-xl font-bold text-surface-700 dark:text-surface-200">{{ title }}</h3>
    <p class="text-surface-500 text-center max-w-xs px-4 mt-2">
      {{ description }}
    </p>
  </div>
</template>
