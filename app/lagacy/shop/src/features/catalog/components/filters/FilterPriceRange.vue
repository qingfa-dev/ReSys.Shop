<script setup lang="ts">
import { ref, watch } from 'vue'
import Slider from 'primevue/slider'
import InputNumber from 'primevue/inputnumber'

interface Props {
  min?: number
  max?: number
  minValue?: number
  maxValue?: number
}

const props = withDefaults(defineProps<Props>(), {
  min: 0,
  max: 1000,
  minValue: 0,
  maxValue: 1000,
})

const emit = defineEmits<{
  (e: 'rangeChange', range: { min: number; max: number }): void
}>()

const range = ref<[number, number]>([props.minValue, props.maxValue])

watch(range, (newRange) => {
  emit('rangeChange', { min: newRange[0], max: newRange[1] })
}, { deep: true })

function handleMinChange(value: number | null) {
  const newMin = value ?? props.min
  const minVal = Math.min(newMin, range.value[1])
  range.value = [minVal, range.value[1]]
  emit('rangeChange', { min: minVal, max: range.value[1] })
}

function handleMaxChange(value: number | null) {
  const newMax = value ?? props.max
  const maxVal = Math.max(newMax, range.value[0])
  range.value = [range.value[0], maxVal]
  emit('rangeChange', { min: range.value[0], max: maxVal })
}
</script>

<template>
  <div class="filter-price-range">
    <div class="price-display">
      <span class="price-label">${{ range[0].toLocaleString() }}</span>
      <span class="price-label">${{ range[1].toLocaleString() }}</span>
    </div>
    
    <Slider 
      v-model="range" 
      :range="true" 
      :min="min" 
      :max="max"
      :step="10"
      class="price-slider"
    />
    
    <div class="price-inputs">
      <div class="input-group">
        <label>Min</label>
        <InputNumber
          :model-value="range[0]"
          :min="min"
          :max="range[1]"
          mode="currency"
          currency="USD"
          locale="en-US"
          placeholder="Min"
          @update:model-value="handleMinChange"
        />
      </div>
      <span class="separator">—</span>
      <div class="input-group">
        <label>Max</label>
        <InputNumber
          :model-value="range[1]"
          :min="range[0]"
          :max="max"
          mode="currency"
          currency="USD"
          locale="en-US"
          placeholder="Max"
          @update:model-value="handleMaxChange"
        />
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.filter-price-range {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.price-display {
  display: flex;
  justify-content: space-between;
  
  .price-label {
    font-size: var(--font-size-base);
    font-weight: var(--font-weight-semibold);
    color: var(--color-primary);
  }
}

.price-slider {
  margin: 0.5rem 0;
  
  :deep(.p-slider-range) {
    background: var(--color-primary);
  }
  
  :deep(.p-slider-handle) {
    background: var(--color-primary);
    border-color: var(--color-primary);
    
    &:hover {
      background: var(--color-primary-hover);
      border-color: var(--color-primary-hover);
    }
  }
}

.price-inputs {
  display: flex;
  align-items: flex-end;
  gap: 0.75rem;
  
  .input-group {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
    
    label {
      font-size: var(--font-size-xs);
      color: var(--color-text-muted);
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    
    :deep(.p-inputnumber) {
      width: 100%;
    }
    
    :deep(.p-inputnumber-input) {
      width: 100%;
      padding: 0.5rem;
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      font-size: var(--font-size-sm);
      
      &:focus {
        border-color: var(--color-primary);
        box-shadow: 0 0 0 2px rgba(var(--color-primary-rgb), 0.1);
      }
    }
  }
  
  .separator {
    color: var(--color-text-muted);
    padding-bottom: 0.5rem;
  }
}
</style>
