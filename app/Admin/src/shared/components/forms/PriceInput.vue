<template>
  <InputNumber :model-value="modelValue" mode="currency" :currency="currency" :locale="locale" :min="min"
    :invalid="invalid" class="w-full" @update:model-value="emit('update:modelValue', $event ?? 0)" />
</template>

<script setup lang="ts">
import InputNumber from 'primevue/inputnumber';

withDefaults(
  defineProps<{
    modelValue: number;
    currency?: string;
    locale?: string;
    min?: number;
    invalid?: boolean;
  }>(),
  { currency: 'USD', locale: 'en-US', min: 0 },
);

const emit = defineEmits<{ 'update:modelValue': [number] }>();
</script>
