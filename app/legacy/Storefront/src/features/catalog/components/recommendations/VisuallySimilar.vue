<script setup lang="ts">
import type { Product } from '../../types'
import ProductCard from '../product/ProductCard.vue'

interface Props {
  products: Product[]
}

defineProps<Props>()
</script>

<template>
  <div class="recommendation-section">
    <h2 class="section-title">Visually Similar</h2>
    <div class="products-grid">
      <ProductCard
        v-for="product in products"
        :key="product.id"
        :product="product"
        variant="grid"
      />
    </div>
  </div>
</template>

<style scoped lang="scss">
.recommendation-section {
  .section-title {
    font-size: var(--font-size-xl);
    font-weight: var(--font-weight-semibold);
    margin-bottom: 1.5rem;
    padding-bottom: 1rem;
    border-bottom: 1px solid var(--color-border-light);
  }
}

.products-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1.5rem;

  @media (max-width: 1200px) {
    grid-template-columns: repeat(3, 1fr);
  }

  @media (max-width: 768px) {
    grid-template-columns: repeat(2, 1fr);
  }

  @media (max-width: 480px) {
    grid-template-columns: 1fr;
  }
}
</style>
