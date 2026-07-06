<template>
  <div class="flex flex-col items-center justify-center gap-2 p-8 text-center text-red-600">
    <i class="pi pi-exclamation-triangle text-3xl" />
    <p class="font-medium">{{ message }}</p>
    <Button v-if="onRetry" label="Retry" severity="secondary" outlined @click="onRetry()" />
  </div>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{ message: string; onRetry?: () => void }>(),
  { onRetry: undefined },
)
</script>
