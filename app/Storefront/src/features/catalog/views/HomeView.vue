<script setup lang="ts">
import { useRouter } from 'vue-router'
import HeroSection from '../components/ui/HeroSection.vue'
import FeaturesStrip from '../components/ui/FeaturesStrip.vue'
import CategoryGrid from '../components/category/CategoryGrid.vue'
import FeaturedProducts from '../components/recommendations/FeaturedProducts.vue'
import { useCartStore } from '@/features/ordering/store'
import type { Category, Product } from '../types'

const router = useRouter()
const cartStore = useCartStore()

function handleCategoryClick(category: Category) {
  // Navigate: Route to shop filtered by the selected category
  router.push({ path: '/shop', query: { category: category.id } })
}

async function handleAddToCart(product: Product) {
  // Call: Add the product's first variant to cart with quantity 1 (Task 2 changed the signature to variantId)
  try {
    const variantId = product.variants?.[0]?.id ?? product.id
    await cartStore.addItem(variantId, 1)
  } catch {
    // Error surfaced by cart store; view delegates
  }
}

function handleAddToWishlist(product: Product) {
  // Navigate: Redirect to wishlists — guest users will be prompted to log in
  router.push('/account/wishlists')
}

function handleProductClick(product: Product) {
  // Navigate: Go to product detail page
  router.push(`/products/${product.id}`)
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
