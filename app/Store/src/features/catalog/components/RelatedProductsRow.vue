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
    <Carousel
      :value="products"
      :num-visible="4"
      :num-scroll="1"
      :show-indicators="false"
      :responsive-options="[{ breakpoint: '768px', numVisible: 2, numScroll: 1 }, { breakpoint: '560px', numVisible: 1, numScroll: 1 }]"
    >
      <template #item="{ data }">
        <div class="px-2">
          <ProductCard :product="data" @add-to-cart="(id) => $emit('addToCart', id)" />
        </div>
      </template>
    </Carousel>
  </div>
</template>
