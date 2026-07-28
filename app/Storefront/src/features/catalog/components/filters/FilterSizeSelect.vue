<script setup lang="ts">
import { ref, computed } from 'vue'

interface Size {
  id: string
  name: string
}

interface Props {
  sizes?: Size[]
  selectedIds?: string[]
}

const props = withDefaults(defineProps<Props>(), {
  sizes: () => [],
  selectedIds: () => [],
})

const emit = defineEmits<{
  (e: 'update:selectedIds', ids: string[]): void
}>()

function toggleSize(sizeId: string) {
  const newSelected = props.selectedIds.includes(sizeId)
    ? props.selectedIds.filter(id => id !== sizeId)
    : [...props.selectedIds, sizeId]
  emit('update:selectedIds', newSelected)
}

function isSelected(sizeId: string) {
  return props.selectedIds.includes(sizeId)
}
</script>

<template>
  <div class="filter-size-select">
    <div class="size-grid">
      <button
        v-for="size in sizes"
        :key="size.id"
        class="size-btn"
        :class="{ active: isSelected(size.id) }"
        @click="toggleSize(size.id)"
      >
        {{ size.name }}
      </button>
    </div>
  </div>
</template>

<style scoped lang="scss">
.filter-size-select {
  display: flex;
  flex-direction: column;
}

.size-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.size-btn {
  min-width: 44px;
  height: 40px;
  padding: 0 0.75rem;
  border: 1px solid var(--color-border);
  background: var(--color-surface);
  border-radius: var(--radius-md);
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);
  cursor: pointer;
  transition: all var(--transition-fast);
  
  &:hover:not(.active) {
    border-color: var(--color-primary);
    color: var(--color-primary);
  }
  
  &.active {
    background: var(--color-primary);
    border-color: var(--color-primary);
    color: white;
  }
}
</style>
