<script setup lang="ts">
import { onMounted, shallowRef, computed } from 'vue'
import { RouterLink } from 'vue-router'
import ProductCard from '../product/ProductCard.vue'
import { useProductStore } from '../../store/product'
import type { Product } from '../../types'

interface Props {
  viewAllRoute?: string
  limit?: number
}

const props = withDefaults(defineProps<Props>(), {
  viewAllRoute: '/shop',
  limit: 4,
})

const productStore = useProductStore()
const products = computed(() => productStore.products)
const loading = computed(() => productStore.loading)

onMounted(() => {
  productStore.getFeaturedProducts(props.limit)
})

const emit = defineEmits<{
  (e: 'addToCart', product: Product): void
  (e: 'addToWishlist', product: Product): void
  (e: 'productClick', product: Product): void
}>()

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
  <section class="featured-products">
    <div class="section-header">
      <h2>Featured Products</h2>
      <RouterLink v-if="viewAllRoute" :to="viewAllRoute" class="view-all">
        View All <i class="pi pi-arrow-right"></i>
      </RouterLink>
    </div>
    <div v-if="loading" class="loading-state">
      <i class="pi pi-spin pi-spinner"></i>
    </div>
    <div v-else class="product-grid">
      <ProductCard
        v-for="product in products"
        :key="product.id"
        :product="product"
        :show-actions="false"
        @add-to-cart="handleAddToCart"
        @add-to-wishlist="handleAddToWishlist"
        @click="handleProductClick"
      />
    </div>
  </section>
</template>

<style scoped lang="scss">
.featured-products {
  max-width: 1400px;
  margin: 0 auto;
  padding: 4rem 2rem;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;

  h2 {
    font-size: var(--font-size-3xl);
  }

  .view-all {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-medium);
    color: var(--color-primary);

    &:hover {
      text-decoration: underline;
    }
  }
}

.loading-state {
  display: flex;
  justify-content: center;
  padding: 4rem 0;

  i {
    font-size: 2rem;
    color: var(--color-primary);
  }
}

.product-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 2rem;

  @media (max-width: 1024px) {
    grid-template-columns: repeat(2, 1fr);
  }

  @media (max-width: 480px) {
    grid-template-columns: 1fr;
  }
}
</style>
