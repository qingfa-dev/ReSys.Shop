<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { StoreProductVariantResponse } from '../types/product'

const props = defineProps<{
  variants: StoreProductVariantResponse[]
  modelValue: string | null
}>()
const emit = defineEmits<{ 'update:modelValue': [id: string] }>()

interface OptionDimension {
  optionTypeId: string
  optionTypeName: string | null
  values: Array<{
    optionValueId: string
    name: string
    presentation: string | null
  }>
}

// Derive: Distinct option dimensions grouped by optionTypeId from all variants
const dimensions = computed<OptionDimension[]>(() => {
  const map = new Map<string, OptionDimension>()

  for (const variant of props.variants) {
    for (const ov of variant.optionValues) {
      if (!map.has(ov.optionTypeId)) {
        map.set(ov.optionTypeId, {
          optionTypeId: ov.optionTypeId,
          optionTypeName: ov.optionTypeName,
          values: [],
        })
      }
      const dim = map.get(ov.optionTypeId)!
      if (!dim.values.some(v => v.optionValueId === ov.optionValueId)) {
        dim.values.push({
          optionValueId: ov.optionValueId,
          name: ov.name,
          presentation: ov.presentation,
        })
      }
    }
  }

  return [...map.values()]
})

// State: Track selected optionValueId per optionTypeId
const selectedByOptionType = ref<Map<string, string>>(new Map())

// Sync: When parent changes modelValue, update internal selection
watch(() => props.modelValue, (newId) => {
  const variant = props.variants.find(v => v.id === newId)
  if (variant) {
    const next = new Map<string, string>()
    for (const ov of variant.optionValues) {
      next.set(ov.optionTypeId, ov.optionValueId)
    }
    selectedByOptionType.value = next
  }
}, { immediate: true })

// Derive: Whether an option value is out of stock
function isOptionValueOutOfStock(optionTypeId: string, optionValueId: string): boolean {
  const matchingVariant = props.variants.find(v => {
    return v.optionValues.some(ov =>
      ov.optionTypeId === optionTypeId && ov.optionValueId === optionValueId
    )
  })
  if (!matchingVariant) return true
  return matchingVariant.stock.availableQuantity === 0 && !matchingVariant.stock.backorderable
}

// Trigger: Select an option value and resolve to the matching variant
function selectValue(optionTypeId: string, optionValueId: string): void {
  if (isOptionValueOutOfStock(optionTypeId, optionValueId)) return

  const next = new Map(selectedByOptionType.value)
  next.set(optionTypeId, optionValueId)
  selectedByOptionType.value = next

  const variant = props.variants.find(v => {
    return v.optionValues.every(ov => {
      const selected = next.get(ov.optionTypeId)
      return !selected || selected === ov.optionValueId
    })
  })
  if (variant) emit('update:modelValue', variant.id)
}

// Derive: Display name for an option value
function displayValue(value: { name: string; presentation: string | null }): string {
  return value.presentation ?? value.name
}
</script>
<template>
  <!-- Section: Product Options -->
  <div class="space-y-4">
    <div v-for="dim in dimensions" :key="dim.optionTypeId">
      <p class="text-sm font-medium text-stone-900 mb-2">
        {{ dim.optionTypeName ?? 'Option' }}
      </p>
      <div class="flex flex-wrap gap-2">
        <button
          v-for="value in dim.values"
          :key="value.optionValueId"
          class="px-4 py-2 rounded-lg border text-sm transition-colors"
          :class="[
            selectedByOptionType.get(dim.optionTypeId) === value.optionValueId
              ? 'border-stone-900 bg-stone-900 text-white'
              : 'border-stone-300 text-stone-700 hover:border-stone-400',
            isOptionValueOutOfStock(dim.optionTypeId, value.optionValueId)
              ? 'line-through opacity-50 cursor-not-allowed'
              : 'cursor-pointer',
          ]"
          :disabled="isOptionValueOutOfStock(dim.optionTypeId, value.optionValueId)"
          @click="selectValue(dim.optionTypeId, value.optionValueId)"
        >
          {{ displayValue(value) }}
        </button>
      </div>
    </div>
  </div>
</template>
