<script setup lang="ts">
import ToggleSwitch from 'primevue/toggleswitch'

defineProps<{
  label: string
  description?: string
  value: boolean
  loading?: boolean
}>()
const emit = defineEmits<{ 'update:value': [value: boolean] }>()
</script>

<template>
  <div class="flex items-center justify-between gap-4 rounded-xl border border-stone-200 bg-white p-5">
    <div>
      <p class="font-medium text-stone-900">{{ label }}</p>
      <p v-if="description" class="text-sm text-stone-500 mt-0.5">{{ description }}</p>
    </div>
    <ToggleSwitch
      :model-value="value"
      :disabled="loading"
      @update:model-value="(v: boolean) => emit('update:value', v)"
    />
  </div>
</template>
