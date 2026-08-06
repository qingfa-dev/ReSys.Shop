<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  min?: number
  max?: number
  modelValue: { min: number | null; max: number | null }
}>(), {
  min: 0,
  max: 1_000_000,
})

const emit = defineEmits<{ 'update:modelValue': [value: { min: number | null; max: number | null }] }>()

const range = computed({
  get: () => [props.modelValue.min ?? props.min, props.modelValue.max ?? props.max] as [number, number],
  set: ([lo, hi]: [number, number]) => {
    emit('update:modelValue', {
      min: lo > props.min ? lo : null,
      max: hi < props.max ? hi : null,
    })
  },
})

const localMin = computed({
  get: () => props.modelValue.min ?? props.min,
  set: (v: number | null) => {
    const hi = props.modelValue.max ?? props.max
    emit('update:modelValue', { min: v && v > props.min ? v : null, max: hi })
  },
})

const localMax = computed({
  get: () => props.modelValue.max ?? props.max,
  set: (v: number | null) => {
    const lo = props.modelValue.min ?? props.min
    emit('update:modelValue', { min: lo, max: v && v < props.max ? v : null })
  },
})
</script>
<template>
  <!-- Section: Price Range Filter -->
  <section class="space-y-3">
    <h3 class="text-sm font-semibold text-stone-900">Price Range</h3>
    <Slider v-model="range" :min="min" :max="max" :step="10_000" range class="w-full" />
    <div class="flex gap-2">
      <InputNumber
        v-model="localMin"
        :min="min"
        :max="max"
        :step="10_000"
        mode="currency"
        currency="VND"
        locale="vi-VN"
        fluid
        size="small"
      />
      <InputNumber
        v-model="localMax"
        :min="min"
        :max="max"
        :step="10_000"
        mode="currency"
        currency="VND"
        locale="vi-VN"
        fluid
        size="small"
      />
    </div>
  </section>
</template>
