<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  title: string
  columns?: 1 | 2 | 3 | 4
}>(), {
  columns: 2,
})

defineSlots<{
  default(): any
}>()

const gridClass = computed(() => {
  const map: Record<number, string> = {
    1: 'grid-cols-1',
    2: 'grid-cols-1 md:grid-cols-2',
    3: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3',
    4: 'grid-cols-1 md:grid-cols-2 xl:grid-cols-4',
  }
  return map[props.columns] ?? map[2]
})
</script>

<template>
  <div class="mb-8">
    <h3 class="text-lg font-semibold mb-4 pb-3 border-b m-0" style="color: var(--p-text-color); border-color: var(--p-surface-200)">
      {{ title }}
    </h3>
    <div class="grid gap-6" :class="gridClass">
      <slot />
    </div>
  </div>
</template>
