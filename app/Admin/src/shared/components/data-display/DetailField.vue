<script setup lang="ts">
withDefaults(defineProps<{
  label: string
  value?: string | number | null
  emptyText?: string
}>(), {
  emptyText: '\u2014',
})

defineSlots<{
  default?(): any
}>()
</script>

<template>
  <div class="flex flex-col">
    <span class="text-xs uppercase font-bold mb-1" style="color: var(--p-text-muted-color)">{{ label }}</span>
    <slot>
      <span v-if="value !== null && value !== undefined && value !== ''" class="text-lg font-medium" style="color: var(--p-text-color)">
        {{ value }}
      </span>
      <span v-else class="text-lg" style="color: var(--p-text-muted-color)">{{ emptyText }}</span>
    </slot>
  </div>
</template>
