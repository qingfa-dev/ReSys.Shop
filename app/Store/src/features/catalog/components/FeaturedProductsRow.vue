<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getPagedProducts } from '../services/productApi'
import ProductCard from './ProductCard.vue'
import type { StoreProductListItemResponse } from '../types/product'
import SkeletonCard from '@/shared/components/SkeletonCard.vue'

const products = ref<StoreProductListItemResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const emit = defineEmits<{ addToCart: [variantId: string] }>()

// Trigger: Fetch featured products on mount.
onMounted(async () => {
  const result = await getPagedProducts({ pageNumber: 1, pageSize: 12, sort: ['-createdAtUtc'] })
  if (result.isSuccess) products.value = result.items
  else error.value = result.message ?? 'Failed to load featured products'
  loading.value = false
})
</script>
<template>
  <!-- Section: Featured Products -->
  <div v-if="loading || error || products.length > 0" class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
    <h2 class="text-2xl font-bold text-stone-900 mb-8">Featured</h2>
    <!-- Section: Loading -->
    <div v-if="loading" class="flex gap-4 overflow-x-auto pb-4">
      <SkeletonCard v-for="i in 4" :key="i" />
    </div>
    <!-- Section: Error -->
    <Message v-else-if="error" severity="error" class="mb-4">{{ error }}</Message>
    <!-- Section: Carousel -->
    <Carousel
      v-else
      :value="products"
      :num-visible="4"
      :num-scroll="1"
      :show-indicators="false"
      :responsive-options="[{ breakpoint: '768px', numVisible: 2 }, { breakpoint: '560px', numVisible: 1 }]"
    >
      <template #item="{ data }">
        <div class="px-2">
          <ProductCard :product="data" @add-to-cart="(id: string) => emit('addToCart', id)" />
        </div>
      </template>
    </Carousel>
  </div>
</template>
