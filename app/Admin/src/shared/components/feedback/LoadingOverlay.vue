<script setup lang="ts">
withDefaults(defineProps<{
  loading: boolean
  message?: string
}>(), {
  loading: false,
})

defineSlots<{
  default(): any
}>()
</script>

<template>
  <div class="relative">
    <slot />
    <Transition name="fade">
      <div
        v-if="loading"
        class="loading-overlay absolute inset-0 z-10 flex flex-col items-center justify-center rounded-xl"
        style="background: color-mix(in srgb, var(--p-surface-overlay) 70%, transparent)"
      >
        <i class="pi pi-spin pi-spinner text-3xl mb-3" style="color: var(--p-primary-color)" />
        <p v-if="message" class="text-sm font-medium" style="color: var(--p-text-muted-color)">{{ message }}</p>
      </div>
    </Transition>
  </div>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
