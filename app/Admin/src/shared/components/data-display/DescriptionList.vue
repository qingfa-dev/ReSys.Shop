<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  items: { label: string; value: string | number; emptyText?: string }[]
  columns?: 1 | 2 | 3
}>(), {
  columns: 2,
})

const isEmpty = (val: string | number): boolean => {
  return val === '' || val === null || val === undefined
}

const gridClass = computed(() => {
  const map: Record<number, string> = {
    1: 'grid-cols-1',
    2: 'grid-cols-1 md:grid-cols-2',
    3: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3',
  }
  return map[props.columns] ?? map[2]
})
</script>

<template>
  <dl class="grid gap-4" :class="gridClass">
    <div v-for="(item, index) in items" :key="index" class="flex flex-col">
      <dt class="text-xs uppercase font-bold mb-1" style="color: var(--p-text-muted-color)">{{ item.label }}</dt>
      <dd class="text-sm font-medium m-0" style="color: var(--p-text-color)">
        {{ isEmpty(item.value) ? (item.emptyText ?? '\u2014') : item.value }}
      </dd>
    </div>
  </dl>
</template>
