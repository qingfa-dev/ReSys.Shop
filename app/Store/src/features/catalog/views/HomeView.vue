<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getPaged } from '@/shared/api/paged'
import { ENDPOINTS } from '@/shared/constants/api'
import { useCartStore } from '@/features/ordering/stores/cartStore'
import type { StoreProductListItemResponse } from '../types/product'
import HeroSection from '../components/HeroSection.vue'
import FeaturesStrip from '../components/FeaturesStrip.vue'
import CategoryGrid from '../components/CategoryGrid.vue'
import FeaturedProductsRow from '../components/FeaturedProductsRow.vue'
import RecentlyViewedRow from '../components/RecentlyViewedRow.vue'
import ProductGrid from '../components/ProductGrid.vue'

const cart = useCartStore()
const newArrivals = ref<StoreProductListItemResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  const result = await getPaged<StoreProductListItemResponse>(ENDPOINTS.products, {
    pageNumber: 1,
    pageSize: 8,
    sort: ['-createdAtUtc'],
  })
  if (result.isSuccess) newArrivals.value = result.items
  else error.value = result.message
  loading.value = false
})
</script>
<template>
  <!-- Section: Home Page -->
  <div>
    <HeroSection />
    <FeaturesStrip />
    <CategoryGrid />
    <FeaturedProductsRow @add-to-cart="(id: string) => cart.addItem(id)" />
    <RecentlyViewedRow @add-to-cart="(id: string) => cart.addItem(id)" />
    <!-- Section: New Arrivals -->
    <section class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
      <h2 class="text-2xl font-bold text-stone-900 mb-8">New Arrivals</h2>
      <ProductGrid :products="newArrivals" :loading="loading" :error="error" @reload="() => {}" />
      <div class="text-center mt-8">
        <router-link to="/shop">
          <Button label="View All Products" severity="secondary" />
        </router-link>
      </div>
    </section>
  </div>
</template>