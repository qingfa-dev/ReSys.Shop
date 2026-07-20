<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import InputNumber from 'primevue/inputnumber'
import type { ProductColor, ProductSize } from '../../types'

interface Props {
  colors?: ProductColor[]
  sizes?: ProductSize[]
  compact?: boolean
  showQuantity?: boolean
  maxQuantity?: number
}

const props = withDefaults(defineProps<Props>(), {
  compact: false,
  showQuantity: true,
  maxQuantity: 10
})

const emit = defineEmits<{
  (e: 'update:selectedColor', value: string): void
  (e: 'update:selectedSize', value: string): void
  (e: 'update:quantity', value: number): void
  (e: 'optionChange', type: 'color' | 'size', value: string): void
  (e: 'openSizeGuide'): void
}>()

const selectedColorId = ref<string>('')
const selectedSizeId = ref<string>('')
const quantity = ref(1)

const selectedColor = computed(() => {
  if (!props.colors) return null
  return props.colors.find(c => c.id === selectedColorId.value) || null
})

const selectedSize = computed(() => {
  if (!props.sizes) return null
  return props.sizes.find(s => s.id === selectedSizeId.value) || null
})

const hasColors = computed(() => props.colors && props.colors.length > 0)
const hasSizes = computed(() => props.sizes && props.sizes.length > 0)

const availableQuantities = computed(() => {
  const maxStock = selectedSize.value?.stock || props.maxQuantity
  const max = Math.min(maxStock, props.maxQuantity)
  return Array.from({ length: max }, (_, i) => i + 1)
})

function selectColor(colorId: string) {
  selectedColorId.value = colorId
  emit('update:selectedColor', colorId)
  emit('optionChange', 'color', colorId)
}

function selectSize(sizeId: string) {
  selectedSizeId.value = sizeId
  emit('update:selectedSize', sizeId)
  emit('optionChange', 'size', sizeId)
  if (selectedSize.value) {
    quantity.value = Math.min(quantity.value, selectedSize.value.stock)
  }
}

function setQuantity(value: number) {
  quantity.value = value
  emit('update:quantity', value)
}

function openSizeGuide() {
  emit('openSizeGuide')
}

function reset() {
  selectedColorId.value = ''
  selectedSizeId.value = ''
  quantity.value = 1
}

watch(quantity, (newValue) => {
  if (newValue) {
    emit('update:quantity', newValue)
  }
})

defineExpose({
  selectedColorId,
  selectedSizeId,
  quantity,
  reset
})
</script>

<template>
  <div class="product-option-picker" :class="{ compact }">
    <div v-if="hasColors" class="option-section">
      <div class="option-header">
        <label class="option-label">Color</label>
        <span v-if="selectedColor" class="selected-value">{{ selectedColor.name }}</span>
      </div>
      <div class="color-options">
        <button
          v-for="color in colors"
          :key="color.id"
          class="color-swatch"
          :class="{ active: selectedColorId === color.id }"
          :style="{ backgroundColor: color.hex }"
          :title="color.name"
          @click="selectColor(color.id)"
        >
          <i v-if="selectedColorId === color.id" class="pi pi-check"></i>
        </button>
      </div>
    </div>

    <div v-if="hasSizes" class="option-section">
      <div class="option-header">
        <label class="option-label">Size</label>
        <button class="size-guide-link" @click="openSizeGuide">Size guide</button>
      </div>
      <div class="size-options">
        <button
          v-for="size in sizes"
          :key="size.id"
          class="size-btn"
          :class="{ active: selectedSizeId === size.id, unavailable: size.stock === 0 }"
          :disabled="size.stock === 0"
          @click="selectSize(size.id)"
        >
          <span class="size-name">{{ size.name }}</span>
          <span v-if="size.stock > 0 && size.stock <= 3" class="low-stock">{{ size.stock }} left</span>
        </button>
      </div>
    </div>

    <div v-if="showQuantity" class="option-section">
      <label class="option-label">Quantity</label>
      <div class="quantity-selector">
        <InputNumber
          v-model="quantity"
          :min="1"
          :max="availableQuantities[availableQuantities.length - 1] || 10"
          :max-value="availableQuantities[availableQuantities.length - 1] || 10"
          showButtons
          buttonLayout="horizontal"
          :step="1"
          incrementButtonIcon="pi pi-plus"
          decrementButtonIcon="pi pi-minus"
          class="qty-input"
        />
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.product-option-picker {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;

  &.compact {
    gap: 0.75rem;
  }
}

.option-section {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.option-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.option-label {
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-semibold);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-text);
}

.selected-value {
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);
}

.size-guide-link {
  background: none;
  border: none;
  color: var(--color-primary);
  font-size: var(--font-size-sm);
  text-decoration: underline;
  cursor: pointer;
  padding: 0;
}

.color-options {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.color-swatch {
  width: 32px;
  height: 32px;
  border: 2px solid var(--color-border);
  border-radius: 50%;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all var(--transition-fast);
  padding: 0;

  &:hover {
    transform: scale(1.1);
  }

  &.active {
    border-color: var(--color-primary);
    box-shadow: 0 0 0 2px white, 0 0 0 4px var(--color-primary);
  }

  i {
    font-size: 12px;
    color: white;
    text-shadow: 0 1px 2px rgba(0, 0, 0, 0.5);
  }

  .compact & {
    width: 24px;
    height: 24px;
  }
}

.size-options {
  display: flex;
  gap: 0.375rem;
  flex-wrap: wrap;
}

.size-btn {
  min-width: 48px;
  height: 40px;
  padding: 0 0.75rem;
  border: 1px solid var(--color-border);
  background: var(--color-surface);
  border-radius: var(--radius-md);
  font-size: var(--font-size-sm);
  cursor: pointer;
  transition: all var(--transition-fast);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 2px;

  .size-name {
    font-weight: var(--font-weight-medium);
  }

  .low-stock {
    font-size: 9px;
    color: var(--color-danger);
  }

  &:hover:not(:disabled) {
    border-color: var(--color-primary);
    color: var(--color-primary);
  }

  &.active {
    background: var(--color-primary);
    border-color: var(--color-primary);
    color: white;

    .low-stock {
      color: white;
    }
  }

  &.unavailable {
    opacity: 0.4;
    cursor: not-allowed;
    text-decoration: line-through;
  }

  .compact & {
    min-width: 40px;
    height: 32px;
    font-size: 11px;
  }
}

.quantity-selector {
  display: inline-flex;
  align-items: center;
  width: fit-content;
}

.qty-input {
  width: 120px;
  
  :deep(.p-inputnumber-input) {
    width: 40px;
    text-align: center;
    font-weight: var(--font-weight-semibold);
  }
  
  :deep(.p-inputnumber-button) {
    width: 36px;
    height: 36px;
    border-radius: var(--radius-md);
  }
}
</style>
