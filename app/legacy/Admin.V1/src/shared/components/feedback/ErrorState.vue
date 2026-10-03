<script setup lang="ts">
const props = withDefaults(defineProps<{
  title?: string
  message?: string
  icon?: string
  retryLabel?: string
}>(), {
  title: 'Something went wrong',
  message: 'An unexpected error occurred. Please try again.',
  icon: 'pi-exclamation-triangle',
})

const emit = defineEmits<{
  retry: []
}>()
</script>

<template>
  <div class="flex flex-col items-center justify-center gap-4 py-16 text-center">
    <i :class="['pi', icon, 'text-6xl text-muted-color']" />
    <h3 class="text-xl font-semibold text-surface-700 dark:text-surface-300">{{ title }}</h3>
    <p class="max-w-md text-muted-color">{{ message }}</p>
    <Button v-if="retryLabel" :label="retryLabel" icon="pi-refresh" @click="emit('retry')" />
  </div>
</template>
