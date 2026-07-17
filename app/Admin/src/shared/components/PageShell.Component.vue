<script setup lang="ts">
withDefaults(defineProps<{
  maxWidth?: '2xl' | '4xl' | '6xl' | '7xl' | 'none'
  card?: boolean
  gap?: boolean
}>(), {
  maxWidth: 'none',
  card: true,
  gap: false,
})
</script>

<template>
  <div
    class="p-6"
    :class="[
      maxWidth !== 'none' ? `max-w-${maxWidth} mx-auto` : '',
      !card && gap ? 'flex flex-col gap-6' : '',
    ]"
  >
    <Card v-if="card">
      <slot />
    </Card>
    <slot v-else />
  </div>
</template>
