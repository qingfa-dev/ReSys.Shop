<script setup lang="ts">
import type { StoreProductImageResponse } from '../types/product'

defineProps<{ images: StoreProductImageResponse[]; alt: string }>()
</script>
<template>
  <!-- Section: Product Gallery -->
  <Galleria
    v-if="images.length > 0"
    :value="images"
    :num-visible="5"
    :show-thumbnails="images.length > 1"
    :show-item-navigators="images.length > 1"
    container-class="rounded-xl overflow-hidden"
  >
    <template #item="{ item }">
      <img :src="item.url" :alt="item.alt ?? alt" class="w-full object-cover" />
    </template>
    <template #thumbnail="{ item }">
      <img :src="item.url" :alt="item.alt ?? alt" class="w-20 h-20 object-cover rounded-lg" />
    </template>
  </Galleria>
  <div v-if="images.length === 0" class="aspect-square bg-stone-100 rounded-xl flex items-center justify-center text-stone-400">
    <i class="pi pi-image text-6xl" />
  </div>
</template>
