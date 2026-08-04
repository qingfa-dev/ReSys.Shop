<script setup lang="ts">
import { ref, computed } from 'vue'
import type { StoreProductVariantResponse } from '../types/product'

const props = defineProps<{ variants: StoreProductVariantResponse[]; productName: string }>()

const visible = ref(false)

// Map: Extract unique option values for display.
const sizeOptions = computed(() => {
  const seen = new Set<string>()
  const sizes: string[] = []
  for (const v of props.variants) {
    const label = v.optionValue1?.presentation ?? v.optionValue1?.name
    if (label && !seen.has(label)) {
      seen.add(label)
      sizes.push(label)
    }
  }
  return sizes
})
</script>
<template>
  <button class="text-sm text-teal-600 hover:text-teal-700 font-medium flex items-center gap-1" @click="visible = true">
    <i class="pi pi-ruler" /> Size Guide
  </button>
  <Dialog v-model:visible="visible" modal :header="`Size Guide — ${productName}`" :style="{ width: '480px' }">
    <!-- Section: Size Table -->
    <div v-if="sizeOptions.length > 0" class="grid grid-cols-2 gap-3">
      <div v-for="size in sizeOptions" :key="size" class="text-center p-3 border border-stone-200 rounded-lg">
        <span class="text-sm font-semibold text-stone-900">{{ size }}</span>
      </div>
    </div>
    <!-- Section: No Data -->
    <p v-else class="text-stone-500 text-center py-4">Size information not available for this product.</p>
  </Dialog>
</template>