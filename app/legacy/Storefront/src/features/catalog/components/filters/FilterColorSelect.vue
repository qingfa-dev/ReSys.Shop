<script setup lang="ts">
interface Color {
  id: string
  name: string
  hex: string
}

interface Props {
  colors?: Color[]
  selectedIds?: string[]
}

const props = withDefaults(defineProps<Props>(), {
  colors: () => [],
  selectedIds: () => [],
})

const emit = defineEmits<{
  (e: 'update:selectedIds', ids: string[]): void
}>()

function toggleColor(colorId: string) {
  const newSelected = props.selectedIds.includes(colorId)
    ? props.selectedIds.filter(id => id !== colorId)
    : [...props.selectedIds, colorId]
  emit('update:selectedIds', newSelected)
}

function isSelected(colorId: string) {
  return props.selectedIds.includes(colorId)
}
</script>

<template>
  <div class="filter-color-select">
    <div class="color-grid">
      <button
        v-for="color in colors"
        :key="color.id"
        class="color-btn"
        :class="{ active: isSelected(color.id) }"
        :style="{ backgroundColor: color.hex }"
        :title="color.name"
        @click="toggleColor(color.id)"
      >
        <i v-if="isSelected(color.id)" class="pi pi-check"></i>
      </button>
    </div>
    
    <div v-if="selectedIds.length > 0" class="selected-info">
      <span>{{ selectedIds.length }} selected</span>
    </div>
  </div>
</template>

<style scoped lang="scss">
.filter-color-select {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.color-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.color-btn {
  width: 36px;
  height: 36px;
  border: 2px solid var(--color-border);
  border-radius: var(--radius-full);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all var(--transition-fast);
  
  &:hover:not(.active) {
    transform: scale(1.1);
    box-shadow: var(--shadow-md);
  }
  
  &.active {
    border-color: var(--color-primary);
    box-shadow: 0 0 0 3px var(--color-primary);
  }
  
  i {
    font-size: 14px;
    color: white;
    text-shadow: 0 1px 2px rgba(0, 0, 0, 0.5);
  }
}

.selected-info {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
}
</style>
