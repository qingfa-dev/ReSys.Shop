<script setup lang="ts">
import { ref, watch } from 'vue'
import type { StoreProductImageResponse } from '../types/product'

const props = defineProps<{ images: StoreProductImageResponse[]; alt: string }>()
const activeIndex = ref(0)

// Trigger: Reset active image when the product changes
watch(() => props.images, () => {
  activeIndex.value = 0
})
</script>
<template>
  <!-- Section: Product Gallery -->
  <div class="space-y-4">
    <!-- Section: Main Image -->
    <div class="aspect-square bg-gray-100 rounded-xl overflow-hidden">
      <img
        v-if="images.length > 0"
        :src="images[activeIndex]?.url"
        :alt="images[activeIndex]?.alt ?? alt"
        class="w-full h-full object-cover"
      />
      <div v-else class="w-full h-full flex items-center justify-center text-gray-400">
        <i class="pi pi-image text-6xl" />
      </div>
    </div>

    <!-- Section: Thumbnails -->
    <div v-if="images.length > 1" class="flex gap-2 overflow-x-auto">
      <button
        v-for="(image, index) in images"
        :key="image.id"
        class="w-20 h-20 rounded-lg overflow-hidden border-2 shrink-0 transition-colors"
        :class="index === activeIndex ? 'border-gray-900' : 'border-transparent hover:border-gray-300'"
        @click="activeIndex = index"
      >
        <img :src="image.url" :alt="image.alt ?? alt" class="w-full h-full object-cover" />
      </button>
    </div>
  </div>
</template>
