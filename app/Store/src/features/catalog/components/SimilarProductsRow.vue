<script setup lang="ts">
import type { StoreProductListItemResponse } from '../types/product'
import ProductCard from './ProductCard.vue'

defineProps<{ products: StoreProductListItemResponse[] }>()
</script>
<template>
  <!-- Section: Similar Products Row -->
  <section>
    <h2 class="text-2xl font-bold text-stone-900 mb-6">You may also like</h2>
    <Carousel
      :value="products"
      :num-visible="4"
      :num-scroll="1"
      :show-indicators="false"
      :responsive-options="[{ breakpoint: '768px', numVisible: 2 }, { breakpoint: '560px', numVisible: 1 }]"
    >
      <template #item="{ data }">
        <div class="px-2">
          <ProductCard :product="data" />
        </div>
      </template>
    </Carousel>
  </section>
</template>
