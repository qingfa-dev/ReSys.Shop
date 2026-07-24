<script setup lang="ts">
import { computed } from 'vue'
import Tag from 'primevue/tag'

interface Props {
  price: number
  compareAtPrice?: number
  inStock?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  inStock: true
})

const discountPercent = computed(() => {
  if (!props.compareAtPrice || props.compareAtPrice <= props.price) return null
  return Math.round(((props.compareAtPrice - props.price) / props.compareAtPrice) * 100)
})
</script>

<template>
  <div class="product-price">
    <div class="price-current">${{ price.toFixed(2) }}</div>
    <template v-if="compareAtPrice">
      <span class="price-original">${{ compareAtPrice.toFixed(2) }}</span>
      <span class="discount">-{{ discountPercent }}%</span>
    </template>
    <Tag v-if="inStock" value="In Stock" severity="success" />
    <Tag v-else value="Out of Stock" severity="danger" />
  </div>
</template>

<style scoped lang="scss">
.product-price {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.price-current {
  font-size: var(--font-size-2xl);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text);
}

.price-original {
  font-size: var(--font-size-base);
  color: var(--color-text-muted);
  text-decoration: line-through;
}

.discount {
  background: var(--color-danger);
  color: white;
  padding: 0.25rem 0.5rem;
  border-radius: var(--radius-md);
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-semibold);
}
</style>
