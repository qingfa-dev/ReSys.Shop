<template>
  <AutoComplete
    :model-value="modelValue"
    :suggestions="suggestions"
    :option-label="optionLabel"
    :placeholder="placeholder"
    :invalid="invalid"
    :loading="loading"
    dropdown
    class="w-full"
    @complete="onComplete"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <template #option="{ option }">
      <slot name="option" :option="option">
        <span>{{ option[optionLabel] }}</span>
      </slot>
    </template>
  </AutoComplete>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import AutoComplete from 'primevue/autocomplete';
import { useDebounce } from '@/shared/composables/useDebounce';

const props = withDefaults(
  defineProps<{
    modelValue: unknown;
    optionLabel?: string;
    placeholder?: string;
    invalid?: boolean;
    search: (query: string) => Promise<unknown[]>;
  }>(),
  { optionLabel: 'name', placeholder: 'Search…' },
);

const emit = defineEmits<{ 'update:modelValue': [unknown] }>();

const suggestions = ref<unknown[]>([]);
const loading = ref(false);

const { debounced: onComplete } = useDebounce(async (e: { query: string }) => {
  loading.value = true;
  try {
    suggestions.value = await props.search(e.query);
  } finally {
    loading.value = false;
  }
}, 300);
</script>
