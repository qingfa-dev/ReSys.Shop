<template>
  <div
    class="sticky bottom-0 -mx-6 mt-6 flex items-center justify-end gap-2 border-t border-surface-200 bg-white/95 px-6 py-4 backdrop-blur dark:border-surface-700 dark:bg-surface-900/95">
    <slot name="left" />
    <div class="flex items-center gap-2 ml-auto">
      <Button :label="cancelLabel" severity="secondary" text :disabled="loading" @click="emit('cancel')" />
      <Button :label="saveLabel" icon="pi pi-check" :loading="loading" @click="emit('save')" />
    </div>
  </div>
</template>

<script setup lang="ts">
import Button from 'primevue/button';

withDefaults(
  defineProps<{ saveLabel?: string; cancelLabel?: string; loading?: boolean }>(),
  { saveLabel: 'Save', cancelLabel: 'Cancel', loading: false },
);

const emit = defineEmits<{ save: []; cancel: [] }>();
</script>
