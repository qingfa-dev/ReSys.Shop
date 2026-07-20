<script setup lang="ts">
withDefaults(defineProps<{
  maxWidth?: string
  card?: boolean
}>(), {
  maxWidth: '1504px',
  card: true,
})

defineSlots<{
  default(): any
}>()
</script>

<template>
  <div
    class="page-container p-6 mx-auto"
    :style="{ maxWidth }"
  >
    <div v-if="card" class="card">
      <slot />
    </div>
    <slot v-else />
  </div>
</template>
