<script setup lang="ts">
interface Props {
  name: string
  image?: string
  productCount?: number
}

defineProps<Props>()

const emit = defineEmits<{
  (e: 'click'): void
}>()
</script>

<template>
  <article class="category-card" @click="emit('click')">
    <div 
      class="category-image" 
      :style="image ? { backgroundImage: `url(${image})` } : {}"
    >
      <div class="category-overlay"></div>
    </div>
    <div class="category-content">
      <h3 class="category-name">{{ name }}</h3>
      <span v-if="productCount" class="product-count">{{ productCount }} items</span>
    </div>
  </article>
</template>

<style scoped lang="scss">
.category-card {
  position: relative;
  aspect-ratio: 3/4;
  border-radius: var(--radius-xl);
  overflow: hidden;
  cursor: pointer;
  
  &:hover {
    .category-image {
      transform: scale(1.05);
    }
    
    .category-content {
      background: linear-gradient(to top, rgba(0,0,0,0.8) 0%, rgba(0,0,0,0.4) 60%, transparent 100%);
    }
  }
}

.category-image {
  position: absolute;
  inset: 0;
  background: linear-gradient(135deg, #e7e5e4 0%, #d6d3d1 100%);
  background-size: cover;
  background-position: center;
  transition: transform var(--transition-slow);
}

.category-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(to top, rgba(0,0,0,0.5), transparent 50%);
}

.category-content {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  padding: 1.5rem;
  background: linear-gradient(to top, rgba(0,0,0,0.6) 0%, transparent 100%);
  transition: background var(--transition-normal);
}

.category-name {
  font-size: var(--font-size-2xl);
  color: white;
  margin-bottom: 0.25rem;
}

.product-count {
  font-size: var(--font-size-sm);
  color: rgba(255, 255, 255, 0.8);
}
</style>
