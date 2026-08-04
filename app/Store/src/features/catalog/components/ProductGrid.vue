<script setup lang="ts">
import type { StoreProductListItemResponse } from '../types/product'
import ProductCard from './ProductCard.vue'
import SkeletonGrid from '@/shared/components/SkeletonGrid.vue'

defineProps<{
  products: StoreProductListItemResponse[]
  loading: boolean
  error: string | null
  /** Variant id currently being quick-added — drives that card's button loading state. */
  loadingVariantId?: string | null
}>()
const emit = defineEmits<{ addToCart: [variantId: string]; reload: [] }>()
</script>
<template>
  <!-- Section: Product Grid -->
  <div>
    <!-- Section: Error State -->
    <Message v-if="error" severity="error" :closable="false">
      {{ error }}
      <Button label="Reload" severity="secondary" size="small" class="ml-3" @click="emit('reload')" />
    </Message>

    <!-- Section: Loading State -->
    <SkeletonGrid v-if="loading" :count="8" />

    <!-- Section: Empty State -->
    <div v-else-if="!loading && products.length === 0" class="text-center py-16">
      <i class="pi pi-search text-4xl text-stone-300 mb-4" />
      <p class="text-stone-500">No products match your filters.</p>
    </div>

    <!-- Section: Grid -->
    <div v-else class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
      <ProductCard
        v-for="product in products"
        :key="product.id"
        :product="product"
        :loading="product.masterVariantId === loadingVariantId"
        @add-to-cart="(id) => emit('addToCart', id)"
      />
    </div>
  </div>
</template>
