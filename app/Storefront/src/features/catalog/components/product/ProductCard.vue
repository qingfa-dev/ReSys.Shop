<script setup lang="ts">
import { computed } from 'vue'
import type { Product, ProductColor, ProductSize } from '@/features/catalog/types'
import ProductCardGrid from './ProductCardGrid.vue'
import ProductCardList from './ProductCardList.vue'

interface Props {
  product: Product
  variant?: 'grid' | 'list'
  showActions?: boolean
  colors?: ProductColor[]
  sizes?: ProductSize[]
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'grid',
  showActions: true,
})

const emit = defineEmits<{
  (e: 'addToCart', product: Product, colorId?: string, sizeId?: string): void
  (e: 'addToWishlist', product: Product): void
  (e: 'click', product: Product): void
}>()

const isGridMode = computed(() => props.variant === 'grid')

function handleAddToCart(product: Product, colorId?: string, sizeId?: string) {
  emit('addToCart', product, colorId, sizeId)
}

function handleAddToWishlist(product: Product) {
  emit('addToWishlist', product)
}

function handleClick(product: Product) {
  emit('click', product)
}
</script>

<template>
  <ProductCardGrid
    v-if="isGridMode"
    :product="product"
    :show-actions="showActions"
    :colors="colors"
    :sizes="sizes"
    @add-to-cart="(p, c, s) => handleAddToCart(p, c, s)"
    @add-to-wishlist="(p) => handleAddToWishlist(p)"
    @click="(p) => handleClick(p)"
  />
  <ProductCardList
    v-else
    :product="product"
    :show-actions="showActions"
    :colors="colors"
    :sizes="sizes"
    @add-to-cart="(p, c, s) => handleAddToCart(p, c, s)"
    @add-to-wishlist="(p) => handleAddToWishlist(p)"
    @click="(p) => handleClick(p)"
  />
</template>
