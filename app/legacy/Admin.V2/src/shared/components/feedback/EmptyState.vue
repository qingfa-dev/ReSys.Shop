<template>
  <div class="flex flex-col items-center justify-center gap-3 py-14 text-center">
    <div class="flex h-14 w-14 items-center justify-center rounded-full bg-surface-100 dark:bg-surface-800">
      <i :class="icon" class="text-2xl text-surface-400" />
    </div>
    <div>
      <p class="font-medium text-surface-700 dark:text-surface-200">{{ title }}</p>
      <p v-if="description" class="mt-1 text-sm text-surface-400">{{ description }}</p>
    </div>
    <slot name="actions" />
  </div>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{ title: string; description?: string; icon?: string }>(),
  { icon: 'pi pi-inbox' },
);
</script>
