<script setup lang="ts">
import type { StoreProductListItemResponse } from '../types/product'
import ProductCard from './ProductCard.vue'

defineProps<{
  products: StoreProductListItemResponse[]
  loading?: boolean
}>()

defineEmits<{ addToCart: [variantId: string] }>()
</script>
<template>
  <!-- Section: Related Products -->
  <div>
    <h2 class="text-xl font-bold text-stone-900 mb-4">You Might Also Like</h2>
    <div class="flex gap-4 overflow-x-auto pb-2">
      <div
        v-for="item in products"
        :key="item.id"
        class="w-64 shrink-0"
      >
        <ProductCard :product="item" @add-to-cart="(id) => $emit('addToCart', id)" />
      </div>
    </div>
  </div>
</template>
