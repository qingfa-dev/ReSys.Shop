<script setup lang="ts">
import { computed } from 'vue'
import type { Product } from '@/features/catalog/types'
import ProductCard from './ProductCard.vue'
import Skeleton from 'primevue/skeleton'

interface Props {
  products: Product[]
  loading?: boolean
  columns?: 2 | 3 | 4 | 5
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  columns: 4,
})

const emit = defineEmits<{
  (e: 'addToCart', product: Product): void
  (e: 'addToWishlist', product: Product): void
  (e: 'productClick', product: Product): void
}>()

const gridColumns = computed(() => {
  const cols = {
    2: 'grid-cols-1 sm:grid-cols-2',
    3: 'grid-cols-1 sm:grid-cols-2 lg:grid-cols-3',
    4: 'grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4',
    5: 'grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5',
  }
  return cols[props.columns]
})

function handleAddToCart(product: Product) {
  emit('addToCart', product)
}

function handleAddToWishlist(product: Product) {
  emit('addToWishlist', product)
}

function handleProductClick(product: Product) {
  emit('productClick', product)
}
</script>

<template>
  <div class="product-grid" :class="gridColumns">
    <div v-if="loading" class="loading-grid" :class="gridColumns">
      <div v-for="n in 8" :key="n" class="skeleton-card">
        <Skeleton width="100%" height="300px" borderRadius="8px" />
        <div class="skeleton-content">
          <Skeleton width="30%" height="1rem" class="mb-2" />
          <Skeleton width="100%" height="1rem" class="mb-2" />
          <Skeleton width="60%" height="1rem" />
        </div>
      </div>
    </div>
    
    <template v-else>
      <ProductCard
        v-for="product in products"
        :key="product.id"
        :product="product"
        @add-to-cart="handleAddToCart"
        @add-to-wishlist="handleAddToWishlist"
        @click="handleProductClick"
      />
    </template>
    
    <div v-if="!loading && products.length === 0" class="empty-state">
      <i class="pi pi-inbox"></i>
      <p>No products found</p>
    </div>
  </div>
</template>

<style scoped lang="scss">
.product-grid {
  display: grid;
  gap: 1.5rem;
}

.loading-grid {
  display: grid;
  gap: 1.5rem;
}

.skeleton-card {
  :deep(.p-skeleton) {
    margin-bottom: 1rem;
  }
}

.skeleton-content {
  padding-top: 0.5rem;
}

.mb-2 {
  margin-bottom: 0.5rem;
}

.empty-state {
  grid-column: 1 / -1;
  text-align: center;
  padding: 4rem 2rem;
  color: var(--color-text-muted);
  
  i {
    font-size: 4rem;
    margin-bottom: 1rem;
    display: block;
  }
  
  p {
    font-size: var(--font-size-lg);
  }
}
</style>
