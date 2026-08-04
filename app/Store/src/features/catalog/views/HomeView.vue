<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getPaged } from '@/shared/api/paged'
import { ENDPOINTS } from '@/shared/constants/api'
import type { StoreProductListItemResponse } from '../types/product'
import ProductGrid from '../components/ProductGrid.vue'

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
    <!-- Section: Hero Banner -->
    <section class="bg-gray-900 text-white">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-24 text-center">
        <h1 class="text-4xl md:text-5xl font-bold mb-4">Discover Your Style</h1>
        <p class="text-lg text-gray-300 mb-8 max-w-xl mx-auto">
          Shop the latest fashion trends with visual search. Upload an image, find your look.
        </p>
        <div class="flex justify-center gap-4">
          <router-link to="/shop">
            <Button label="Shop All" size="large" />
          </router-link>
          <router-link to="/recommendations">
            <Button label="Visual Search" severity="secondary" size="large" />
          </router-link>
        </div>
      </div>
    </section>

    <!-- Section: New Arrivals -->
    <section class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
      <h2 class="text-2xl font-bold text-gray-900 mb-8">New Arrivals</h2>
      <ProductGrid
        :products="newArrivals"
        :loading="loading"
        :error="error"
        @reload="() => {}"
      />
      <div class="text-center mt-8">
        <router-link to="/shop">
          <Button label="View All Products" severity="secondary" />
        </router-link>
      </div>
    </section>
  </div>
</template>
