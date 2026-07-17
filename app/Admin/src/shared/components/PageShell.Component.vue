<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  maxWidth?: '2xl' | '4xl' | '6xl' | '7xl' | 'none'
  card?: boolean
  gap?: boolean
}>(), {
  maxWidth: 'none',
  card: true,
  gap: false,
})

const maxWidthClass = computed(() => {
  switch (props.maxWidth) {
    case '2xl': return 'max-w-2xl mx-auto'
    case '4xl': return 'max-w-4xl mx-auto'
    case '6xl': return 'max-w-6xl mx-auto'
    case '7xl': return 'max-w-7xl mx-auto'
    default: return ''
  }
})
</script>

<template>
  <div
    class="p-6"
    :class="[
      maxWidthClass,
      !card && gap ? 'flex flex-col gap-6' : '',
    ]"
  >
    <Card v-if="card">
      <slot />
    </Card>
    <slot v-else />
  </div>
</template>
