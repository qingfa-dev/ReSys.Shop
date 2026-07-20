<script setup lang="ts">
import HeroSection from '../components/ui/HeroSection.vue'
import FeaturesStrip from '../components/ui/FeaturesStrip.vue'
import CategoryGrid from '../components/category/CategoryGrid.vue'
import FeaturedProducts from '../components/recommendations/FeaturedProducts.vue'
import type { Category, Product } from '../types'

function handleCategoryClick(category: Category) {
  console.log('Category clicked:', category.name)
}

function handleAddToCart(product: Product) {
  console.log('Add to cart:', product.name)
}

function handleAddToWishlist(product: Product) {
  console.log('Add to wishlist:', product.name)
}

function handleProductClick(product: Product) {
  console.log('Product clicked:', product.name)
}
</script>

<template>
  <div class="home-view">
    <HeroSection
      badge="New Collection 2026"
      title="Redefine Your<br>Style"
      subtitle="Discover curated fashion that speaks to your unique personality."
      cta-primary-label="Shop Now"
      cta-primary-route="/shop"
      cta-secondary-label="View Collections"
      cta-secondary-route="/collections"
    />

    <FeaturesStrip />

    <CategoryGrid
      view-all-route="/shop"
      @category-click="handleCategoryClick"
    />

    <FeaturedProducts
      view-all-route="/shop"
      @add-to-cart="handleAddToCart"
      @add-to-wishlist="handleAddToWishlist"
      @product-click="handleProductClick"
    />
  </div>
</template>

<style scoped lang="scss">
.home-view {
  animation: fadeIn var(--transition-normal) ease-out;
}
</style>
