<template>
  <div class="flex flex-col gap-1.5">
    <label v-if="label" :for="fieldId" class="text-sm font-medium text-surface-700 dark:text-surface-200">
      {{ label }}
      <span v-if="required" class="text-red-500">*</span>
    </label>
    <slot :field-id="fieldId" :invalid="!!error" />
    <small v-if="hint && !error" class="text-surface-400">{{ hint }}</small>
    <Message v-if="error" severity="error" size="small" variant="simple">{{ error }}</Message>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import Message from 'primevue/message';

const props = defineProps<{
  label?: string;
  hint?: string;
  error?: string;
  required?: boolean;
  name?: string;
}>();

const fieldId = computed(() => props.name ?? props.label?.toLowerCase().replace(/\s+/g, '-') ?? '');
</script>
