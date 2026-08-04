<script setup lang="ts">
import type { StoreProductListItemResponse } from '../types/product'
import { formatVnd } from '@/shared/utils/currency'

const props = defineProps<{ product: StoreProductListItemResponse }>()
// The backend cart add endpoint requires a VARIANT id, not a product id — emit the master variant id.
const emit = defineEmits<{ addToCart: [variantId: string] }>()

// Map: Format price for display
function displayPrice(): string {
  return props.product.minPrice != null ? formatVnd(props.product.minPrice) : 'Contact'
}
</script>
<template>
  <!-- Section: Product Card -->
  <div class="group bg-white rounded-xl border border-gray-200 overflow-hidden shadow-sm hover:shadow-md transition-shadow">
    <!-- Section: Thumbnail -->
    <router-link :to="`/products/${product.slug}`" class="block aspect-square bg-gray-100 relative overflow-hidden">
      <img
        v-if="product.thumbnailUrl"
        :src="product.thumbnailUrl"
        :alt="product.thumbnailAlt ?? product.name"
        class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-200"
      />
      <div v-else class="w-full h-full flex items-center justify-center text-gray-400">
        <i class="pi pi-image text-4xl" />
      </div>
      <!-- Section: Quick Add Overlay -->
      <div class="absolute inset-x-0 bottom-0 p-3 bg-gradient-to-t from-black/60 to-transparent opacity-0 group-hover:opacity-100 transition-opacity">
        <Button
          label="Quick Add"
          icon="pi pi-plus"
          size="small"
          class="w-full"
          @click.prevent="emit('addToCart', product.masterVariantId)"
        />
      </div>
    </router-link>
    <!-- Section: Product Info -->
    <div class="p-4">
      <router-link :to="`/products/${product.slug}`" class="text-sm font-medium text-gray-900 line-clamp-2 hover:text-gray-600">
        {{ product.name }}
      </router-link>
      <p class="mt-1 text-lg font-bold text-gray-900">{{ displayPrice() }}</p>
    </div>
  </div>
</template>
