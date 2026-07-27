<template>
  <div class="flex flex-col items-center justify-center gap-3 py-14 text-center">
    <div class="flex h-14 w-14 items-center justify-center rounded-full bg-red-100 dark:bg-red-400/10">
      <i class="pi pi-exclamation-triangle text-2xl text-red-500" />
    </div>
    <div>
      <p class="font-medium text-surface-700 dark:text-surface-200">{{ title }}</p>
      <p v-if="description" class="mt-1 text-sm text-surface-400">{{ description }}</p>
    </div>
    <Button v-if="retryable" label="Try again" icon="pi pi-refresh" outlined @click="emit('retry')" />
  </div>
</template>

<script setup lang="ts">
import Button from 'primevue/button';

withDefaults(
  defineProps<{ title?: string; description?: string; retryable?: boolean }>(),
  {
    title: 'Something went wrong',
    description: "We couldn't load this data. Please try again.",
    retryable: true,
  },
);

const emit = defineEmits<{ retry: [] }>();
</script>
