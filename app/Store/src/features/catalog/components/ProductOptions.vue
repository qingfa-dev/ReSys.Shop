<script setup lang="ts">
import { computed } from 'vue'
import type { StoreProductVariantResponse } from '../types/product'

const props = defineProps<{
  variants: StoreProductVariantResponse[]
  modelValue: string | null
}>()
const emit = defineEmits<{ 'update:modelValue': [id: string] }>()

interface OptionDimension {
  key: 'optionValue1' | 'optionValue2'
  values: Array<{ id: string; name: string }>
}

// Map: Distinct option dimensions derived from the variant list
const dimensions = computed<OptionDimension[]>(() => {
  const dims: OptionDimension[] = []
  for (const key of ['optionValue1', 'optionValue2'] as const) {
    const entries = new Map<string, { id: string; name: string }>()
    for (const variant of props.variants) {
      const optionValue = variant[key]
      if (optionValue) {
        entries.set(optionValue.id, { id: optionValue.id, name: optionValue.presentation ?? optionValue.name })
      }
    }
    if (entries.size > 0) {
      dims.push({ key, values: [...entries.values()] })
    }
  }
  return dims
})

// Map: Resolve the currently selected variant's option values
const selectedValue1 = computed(() => props.variants.find(v => v.id === props.modelValue)?.optionValue1?.id ?? null)
const selectedValue2 = computed(() => props.variants.find(v => v.id === props.modelValue)?.optionValue2?.id ?? null)

// Map: Whether an option value is selected in a given dimension
function isSelected(key: 'optionValue1' | 'optionValue2', valueId: string): boolean {
  return (key === 'optionValue1' ? selectedValue1.value : selectedValue2.value) === valueId
}

// Trigger: Select an option value and resolve to the matching variant
function selectValue(key: 'optionValue1' | 'optionValue2', valueId: string): void {
  let variant: StoreProductVariantResponse | undefined
  if (key === 'optionValue1') {
    variant = props.variants.find(v => v.optionValue1?.id === valueId && v.optionValue2?.id === selectedValue2.value)
      ?? props.variants.find(v => v.optionValue1?.id === valueId)
  } else {
    variant = props.variants.find(v => v.optionValue2?.id === valueId && v.optionValue1?.id === selectedValue1.value)
      ?? props.variants.find(v => v.optionValue2?.id === valueId)
  }
  if (variant) emit('update:modelValue', variant.id)
}
</script>
<template>
  <!-- Section: Product Options -->
  <div class="space-y-4">
    <div v-for="dim in dimensions" :key="dim.key">
      <p class="text-sm font-medium text-gray-900 mb-2">{{ dim.key === 'optionValue1' ? 'Option 1' : 'Option 2' }}</p>
      <div class="flex flex-wrap gap-2">
        <button
          v-for="value in dim.values"
          :key="value.id"
          class="px-4 py-2 rounded-lg border text-sm transition-colors"
          :class="isSelected(dim.key, value.id)
            ? 'border-gray-900 bg-gray-900 text-white'
            : 'border-gray-300 text-gray-700 hover:border-gray-400'"
          @click="selectValue(dim.key, value.id)"
        >
          {{ value.name }}
        </button>
      </div>
    </div>
  </div>
</template>
