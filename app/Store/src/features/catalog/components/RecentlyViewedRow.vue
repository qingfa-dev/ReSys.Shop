<script setup lang="ts">
import { useRecentlyViewed } from '@/shared/composables/useRecentlyViewed'
import ProductCard from './ProductCard.vue'

const { items, clear } = useRecentlyViewed()
const emit = defineEmits<{ addToCart: [variantId: string] }>()
</script>
<template>
  <!-- Section: Recently Viewed -->
  <section v-if="items.length > 0" class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
    <div class="flex items-center justify-between mb-8">
      <h2 class="text-2xl font-bold text-stone-900">Recently Viewed</h2>
      <button class="text-sm text-stone-500 hover:text-stone-700 font-medium" @click="clear">Clear</button>
    </div>
    <!-- Section: Scrollable Row -->
    <div class="flex gap-4 overflow-x-auto pb-4">
      <div v-for="item in items" :key="item.productId" class="w-64 shrink-0">
        <ProductCard
          :product="{
            id: item.productId,
            masterVariantId: item.productId,
            name: item.productName,
            status: '',
            description: null,
            slug: item.slug,
            styleCode: null,
            seasonName: null,
            materialComposition: null,
            careInstructions: null,
            fitNotes: null,
            department: null,
            genderTarget: null,
            variantsCount: 0,
            availableOn: null,
            masterVariant: {
              id: item.productId,
              sku: null,
              isMaster: true,
              price: item.minPrice,
              currency: null,
              optionValues: [],
              images: item.thumbnailUrl ? [{ id: '', url: item.thumbnailUrl, alt: null, position: 0 }] : [],
              prices: [],
              stock: { availableQuantity: 0, backorderable: false },
            },
            classifications: [],
          }"
          @add-to-cart="(id: string) => emit('addToCart', id)"
        />
      </div>
    </div>
  </section>
</template>