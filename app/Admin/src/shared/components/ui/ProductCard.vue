<script setup lang="ts">
import RatingBadge from './RatingBadge.vue'
import StatusTag from './StatusTag.vue'

interface Product {
  id: string
  name: string
  category: string
  price: number
  rating: number
  inventoryStatus: string
  image: string
}

interface Props {
  product: Product
  layout?: 'list' | 'grid'
}

withDefaults(defineProps<Props>(), {
  layout: 'list',
})
</script>

<template>
  <div v-if="layout === 'list'" class="flex flex-col sm:flex-row items-center p-4 gap-4 border-b border-surface">
    <div class="relative">
      <img :src="product.image" :alt="product.name" class="w-48 rounded-border" />
      <StatusTag :status="product.inventoryStatus" domain="inventory" class="absolute top-3 left-3" />
    </div>
    <div class="flex flex-col gap-2 flex-1">
      <span class="text-muted-color text-sm">{{ product.category }}</span>
      <span class="text-surface-900 dark:text-surface-0 font-medium text-lg">{{ product.name }}</span>
      <RatingBadge :rating="product.rating" />
      <div class="flex items-center justify-between">
        <span class="text-surface-900 dark:text-surface-0 font-semibold text-xl">{{ product.price }}</span>
        <slot name="actions" />
      </div>
    </div>
  </div>
  <div v-else class="border border-surface rounded-border p-4 flex flex-col gap-4">
    <div class="relative">
      <img :src="product.image" :alt="product.name" class="w-full rounded-border" />
      <StatusTag :status="product.inventoryStatus" domain="inventory" class="absolute top-3 left-3" />
    </div>
    <div class="flex flex-col gap-2">
      <span class="text-muted-color text-sm">{{ product.category }}</span>
      <span class="text-surface-900 dark:text-surface-0 font-medium text-lg">{{ product.name }}</span>
      <RatingBadge :rating="product.rating" />
      <div class="flex items-center justify-between">
        <span class="text-surface-900 dark:text-surface-0 font-semibold text-xl">{{ product.price }}</span>
        <slot name="actions" />
      </div>
    </div>
  </div>
</template>
