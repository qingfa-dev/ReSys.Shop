<script setup lang="ts">
import { ref, computed } from 'vue'
import Select from 'primevue/select'
import type { ProductColor, ProductSize } from '../../types'

interface Props {
  colors?: ProductColor[]
  sizes?: ProductSize[]
  maxQuantity?: number
}

const props = withDefaults(defineProps<Props>(), {
  maxQuantity: 10
})

const emit = defineEmits<{
  (e: 'update:selectedColor', value: string): void
  (e: 'update:selectedSize', value: string): void
  (e: 'update:quantity', value: number): void
}>()

const selectedColor = ref('')
const selectedSize = ref('')
const quantity = ref(1)

const availableQuantities = computed(() => {
  const max = props.maxQuantity || 10
  return Array.from({ length: Math.min(max, 10) }, (_, i) => ({
    label: String(i + 1),
    value: i + 1,
  }))
})

const selectedSizeData = computed(() => {
  if (!props.sizes) return null
  return props.sizes.find(s => s.id === selectedSize.value) || null
})

function onColorClick(colorId: string) {
  selectedColor.value = colorId
  emit('update:selectedColor', colorId)
}

function onSizeChange(event: unknown) {
  const value = (event as { value?: string })?.value
  if (value) {
    emit('update:selectedSize', value)
  }
}

function onQuantityChange(event: unknown) {
  const value = (event as { value?: number })?.value
  if (value) {
    emit('update:quantity', value)
  }
}
</script>

<template>
  <div class="product-options">
    <div v-if="colors && colors.length > 0" class="option-group">
      <label class="option-label">Color</label>
      <div class="color-options">
        <button
          v-for="color in colors"
          :key="color.id"
          class="color-option"
          :class="{ active: selectedColor === color.id }"
          :style="{ borderColor: color.hex }"
          :title="color.name"
          @click="onColorClick(color.id)"
        >
          <span class="color-swatch" :style="{ backgroundColor: color.hex }" />
        </button>
      </div>
    </div>

    <div v-if="sizes && sizes.length > 0" class="option-group">
      <div class="size-header">
        <label class="option-label">Size</label>
        <button class="size-guide-btn">Size guide</button>
      </div>
      <Select
        v-model="selectedSize"
        :options="sizes"
        optionLabel="name"
        optionValue="id"
        placeholder="Select a size"
        class="size-dropdown"
        @change="onSizeChange"
      />
      <div v-if="selectedSizeData" class="size-info">
        <span v-if="selectedSizeData.stock > 0" class="stock-available">
          {{ selectedSizeData.stock }} available
        </span>
        <span v-else class="stock-unavailable">Out of stock</span>
      </div>
    </div>

    <div class="option-group">
      <label class="option-label">Quantity</label>
      <Select
        v-model="quantity"
        :options="availableQuantities"
        optionLabel="label"
        optionValue="value"
        class="quantity-dropdown"
        @change="onQuantityChange"
      />
    </div>
  </div>
</template>

<style scoped lang="scss">
.product-options {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  padding: 1.5rem 0;
  border-top: 1px solid var(--color-border-light);
  border-bottom: 1px solid var(--color-border-light);
}

.option-group {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.option-label {
  font-weight: var(--font-weight-semibold);
  font-size: var(--font-size-sm);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-text);
}

.size-header {
  display: flex;
  justify-content: space-between;
  align-items: center;

  .size-guide-btn {
    background: none;
    border: none;
    color: var(--color-primary);
    text-decoration: underline;
    cursor: pointer;
    font-size: var(--font-size-sm);
  }
}

.color-options {
  display: flex;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.color-option {
  width: 40px;
  height: 40px;
  border: 2px solid var(--color-border);
  border-radius: 50%;
  padding: 3px;
  background: white;
  cursor: pointer;
  transition: all var(--transition-fast);

  &:hover {
    border-color: var(--color-text);
  }

  &.active {
    border-color: var(--color-text);
    box-shadow:
      0 0 0 2px white,
      0 0 0 4px var(--color-text);
  }
}

.color-swatch {
  display: block;
  width: 100%;
  height: 100%;
  border-radius: 50%;
}

.size-info {
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);

  .stock-unavailable {
    color: var(--color-danger);
  }
}
</style>
