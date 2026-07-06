<template>
  <div class="mb-4 flex items-center justify-between">
    <div>
      <h1 class="text-2xl font-semibold">{{ title }}</h1>
      <p v-if="subtitle" class="text-color-secondary">{{ subtitle }}</p>
    </div>
    <div class="flex gap-2">
      <slot name="actions" />
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{ title: string; subtitle?: string }>()
</script>
