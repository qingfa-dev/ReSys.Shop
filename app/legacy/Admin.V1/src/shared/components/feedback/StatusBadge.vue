<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  status?: string | number
  statusMap?: Record<string | number, { label: string; severity: string }>
  label?: string
  severity?: string
  size?: 'small' | 'normal'
}>(), {
  size: 'normal',
  severity: 'info',
})

const resolved = computed(() => {
  if (props.label) return { label: props.label, severity: props.severity }
  if (props.status !== undefined && props.statusMap) {
    return props.statusMap[props.status] ?? { label: String(props.status), severity: 'secondary' }
  }
  return { label: props.label ?? '', severity: props.severity }
})
</script>

<template>
  <Tag
    :value="resolved.label"
    :severity="resolved.severity"
    :class="size === 'normal' ? 'px-4 py-2 font-bold rounded-border' : ''"
    rounded
  />
</template>
