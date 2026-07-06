<template>
  <Button
    :label="label"
    :icon="icon"
    :loading="loading"
    :disabled="disabled"
    :severity="severity"
    :outlined="variant === 'secondary'"
    :text="variant === 'ghost'"
    :size="size"
    @click="$emit('click', $event)"
  />
</template>

<script setup lang="ts">
import { computed } from 'vue'

type Variant = 'primary' | 'secondary' | 'danger' | 'ghost'
type Severity = 'primary' | 'secondary' | 'danger' | 'success' | 'info' | 'warn' | 'help' | 'contrast'
type Size = 'small' | 'large' | undefined

const props = withDefaults(
  defineProps<{
    label?: string
    icon?: string
    loading?: boolean
    disabled?: boolean
    variant?: Variant
    size?: Size
  }>(),
  { variant: 'primary' },
)

defineEmits<{ click: [event: MouseEvent] }>()

const severity = computed<Severity>(() => {
  if (props.variant === 'danger') return 'danger'
  if (props.variant === 'ghost') return 'secondary'
  return 'primary'
})
</script>
