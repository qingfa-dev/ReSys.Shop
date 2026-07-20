<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  status: string | number
  statusMap: Record<string | number, { label: string; severity: string }>
  size?: 'small' | 'normal'
}>(), {
  size: 'normal',
})

const resolved = computed(() => props.statusMap[props.status] ?? { label: String(props.status), severity: 'secondary' })
</script>

<template>
  <Tag
    :value="resolved.label"
    :severity="resolved.severity as any"
    :class="size === 'normal' ? 'px-4 py-2 text-lg font-bold rounded-xl' : ''"
    rounded
  />
</template>
