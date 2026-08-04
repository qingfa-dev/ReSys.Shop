<script setup lang="ts">
withDefaults(defineProps<{
  label: string
  value?: string | number | null
  emptyText?: string
}>(), {
  emptyText: '\u2014',
})
</script>

<template>
  <div class="flex flex-col">
    <span class="text-xs text-muted-color uppercase font-bold mb-1">{{ label }}</span>
    <span v-if="value !== null && value !== undefined && value !== ''" class="text-lg font-medium text-color dark:text-surface-0">
      {{ value }}
    </span>
    <span v-else class="text-lg text-muted-color">{{ emptyText }}</span>
  </div>
</template>
