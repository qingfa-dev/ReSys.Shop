<script setup lang="ts">
import { computed } from 'vue'
import type { StoreProductListItemResponse } from '../types/product'
import { formatVnd } from '@/shared/utils/currency'
import ProductBadge from './ProductBadge.vue'

const props = defineProps<{ product: StoreProductListItemResponse; loading?: boolean }>()
// The backend cart add endpoint requires a VARIANT id, not a product id — emit the master variant id.
const emit = defineEmits<{ addToCart: [variantId: string] }>()

// Map: Format price for display
function displayPrice(): string {
  const price = props.product.masterVariant?.price
  return price != null && price > 0 ? formatVnd(price) : 'Contact'
}

// Map: Mark products available within the last 14 days as new
const isNew = computed(() => {
  if (!props.product.availableOn) return false
  const diff = Date.now() - new Date(props.product.availableOn).getTime()
  return diff >= 0 && diff <= 14 * 24 * 60 * 60 * 1000
})

// Derive: Stock label from availableQuantity and backorderable
const stockLabel = computed(() => {
  const stock = props.product.masterVariant?.stock
  if (!stock) return null
  if (stock.availableQuantity > 5) return null
  if (stock.availableQuantity > 0) return `Only ${stock.availableQuantity} left`
  if (stock.backorderable) return 'Available for backorder'
  return 'Out of Stock'
})

// Derive: Tailwind color classes per stock level
const stockColor = computed(() => {
  const stock = props.product.masterVariant?.stock
  if (!stock) return 'bg-stone-100 text-stone-600'
  if (stock.availableQuantity > 5) return 'bg-emerald-100 text-emerald-700'
  if (stock.availableQuantity > 0) return 'bg-amber-100 text-amber-700'
  if (stock.backorderable) return 'bg-blue-100 text-blue-700'
  return 'bg-red-100 text-red-700'
})
</script>
<template>
  <!-- Section: Product Card -->
  <div class="group bg-white rounded-xl border border-stone-200 overflow-hidden shadow-sm hover:shadow-md transition-shadow">
    <!-- Section: Thumbnail -->
    <div class="relative">
      <ProductBadge v-if="isNew" variant="new" />
      <router-link :to="`/products/${product.slug}`" class="block aspect-square bg-stone-100 relative overflow-hidden">
          <img
            v-if="product.masterVariant?.images?.[0]?.url"
            :src="product.masterVariant.images[0].url"
            :alt="product.masterVariant.images[0].alt ?? product.name"
            class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-200"
          />
          <div v-else class="w-full h-full flex items-center justify-center text-stone-400">
            <i class="pi pi-image text-4xl" />
          </div>
          <!-- Section: Quick Add Overlay -->
          <div class="absolute inset-x-0 bottom-0 p-3 bg-gradient-to-t from-black/60 to-transparent opacity-0 group-hover:opacity-100 transition-opacity">
            <Button
              label="Quick Add"
              icon="pi pi-plus"
              size="small"
              class="w-full"
              :loading="loading"
              :disabled="loading"
              @click.prevent="emit('addToCart', product.masterVariantId)"
            />
          </div>
        </router-link>
      </div>
    <!-- Section: Product Info -->
    <div class="p-4">
      <router-link :to="`/products/${product.slug}`" class="text-sm font-medium text-stone-900 line-clamp-2 hover:text-stone-600">
        {{ product.name }}
      </router-link>
      <p class="mt-1 text-lg font-bold text-stone-900">{{ displayPrice() }}</p>
      <!-- Section: Stock Badge -->
      <span
        v-if="stockLabel"
        class="mt-2 inline-block text-xs font-medium px-2 py-0.5 rounded-full"
        :class="stockColor"
      >
        {{ stockLabel }}
      </span>
    </div>
  </div>
</template>
