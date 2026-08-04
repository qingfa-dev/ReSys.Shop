<script setup lang="ts">
import type { StoreProductListItemResponse } from '../types/product'
import ProductCard from './ProductCard.vue'

defineProps<{ products: StoreProductListItemResponse[] }>()
</script>
<template>
  <!-- Section: Similar Products Row -->
  <section>
    <h2 class="text-2xl font-bold text-gray-900 mb-6">You may also like</h2>
    <div class="flex gap-4 overflow-x-auto pb-2 -mx-4 px-4">
      <div v-for="product in products" :key="product.id" class="w-64 shrink-0">
        <ProductCard :product="product" />
      </div>
    </div>
  </section>
</template>
