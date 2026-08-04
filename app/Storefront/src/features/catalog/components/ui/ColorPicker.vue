<script setup lang="ts">
import { ref, computed } from 'vue'
import type { ProductColor } from '../../types'

interface Props {
  colors: ProductColor[]
}

const props = defineProps<Props>()

const emit = defineEmits<{
  (e: 'update:selectedColor', value: string): void
  (e: 'select', colorId: string): void
}>()

const selectedColorId = ref<string>('')

const selectedColor = computed(() => {
  return props.colors.find(c => c.id === selectedColorId.value) || null
})

function selectColor(colorId: string) {
  selectedColorId.value = colorId
  emit('update:selectedColor', colorId)
  emit('select', colorId)
}
</script>

<template>
  <div class="color-picker">
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
</template>

<style scoped lang="scss">
.color-picker {
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
}
</style>
