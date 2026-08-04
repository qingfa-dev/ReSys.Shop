<script setup lang="ts">
import { ref, computed } from 'vue'

interface Category {
  id: string
  name: string
  slug: string
  children?: Category[]
}

interface Props {
  categories?: Category[]
  selectedId?: string | null
}

const props = withDefaults(defineProps<Props>(), {
  categories: () => [],
  selectedId: null,
})

const emit = defineEmits<{
  (e: 'select', id: string | null): void
}>()

const expandedCategories = ref<Set<string>>(new Set())

const hasChildren = (cat: Category) => cat.children && cat.children.length > 0

function toggleExpand(id: string) {
  if (expandedCategories.value.has(id)) {
    expandedCategories.value.delete(id)
  } else {
    expandedCategories.value.add(id)
  }
}

function selectCategory(id: string | null) {
  emit('select', id)
}

function isExpanded(id: string) {
  return expandedCategories.value.has(id)
}
</script>

<template>
  <div class="filter-category-tree">
    <div 
      class="category-item root"
      :class="{ active: selectedId === null }"
      @click="selectCategory(null)"
    >
      <span class="category-name">All Categories</span>
    </div>
    
    <div 
      v-for="category in categories" 
      :key="category.id"
      class="category-wrapper"
    >
      <div 
        class="category-item"
        :class="{ 
          active: selectedId === category.id,
          expandable: hasChildren(category)
        }"
      >
        <button 
          v-if="hasChildren(category)"
          class="expand-btn"
          :class="{ expanded: isExpanded(category.id) }"
          @click.stop="toggleExpand(category.id)"
        >
          <i class="pi pi-chevron-right"></i>
        </button>
        
        <span 
          class="category-name" 
          @click="selectCategory(category.id)"
        >
          {{ category.name }}
        </span>
        
        <span v-if="hasChildren(category)" class="count">
          {{ category.children?.length }}
        </span>
      </div>
      
      <div v-if="isExpanded(category.id)" class="children">
        <div
          v-for="child in category.children"
          :key="child.id"
          class="category-item child"
          :class="{ active: selectedId === child.id }"
          @click="selectCategory(child.id)"
        >
          <span class="category-name">{{ child.name }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.filter-category-tree {
  display: flex;
  flex-direction: column;
}

.category-wrapper {
  display: flex;
  flex-direction: column;
}

.category-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.625rem 0.5rem;
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: all var(--transition-fast);
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  
  &:hover {
    background: var(--color-surface-ground);
    color: var(--color-text);
  }
  
  &.active {
    background: var(--color-primary);
    color: white;
    
    .count {
      color: rgba(255, 255, 255, 0.8);
    }
  }
  
  &.root {
    font-weight: var(--font-weight-semibold);
    margin-bottom: 0.5rem;
    padding-left: 0;
    
    &:hover {
      background: transparent;
      color: var(--color-primary);
    }
    
    &.active {
      background: transparent;
      color: var(--color-primary);
    }
  }
  
  &.child {
    padding-left: 2rem;
    font-size: var(--font-size-sm);
  }
  
  &.expandable {
    cursor: pointer;
  }
}

.expand-btn {
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: none;
  border: none;
  cursor: pointer;
  padding: 0;
  color: inherit;
  transition: transform var(--transition-fast);
  
  &.expanded {
    transform: rotate(90deg);
  }
  
  i {
    font-size: 10px;
  }
}

.category-name {
  flex: 1;
}

.count {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
  background: var(--color-surface-ground);
  padding: 0.125rem 0.375rem;
  border-radius: var(--radius-full);
}

.children {
  display: flex;
  flex-direction: column;
}
</style>
