<script setup lang="ts">
import type { StoreProductDetailResponse } from '../types/product'

defineProps<{ product: StoreProductDetailResponse }>()
</script>
<template>
  <!-- Section: Product Details Info -->
  <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
    <div v-if="product.styleCode" class="flex items-center gap-2 text-sm">
      <i class="pi pi-hashtag text-teal-600" />
      <span class="text-stone-500">Style:</span>
      <span class="text-stone-900 font-medium">{{ product.styleCode }}</span>
    </div>
    <div v-if="product.materialComposition" class="flex items-center gap-2 text-sm">
      <i class="pi pi-palette text-teal-600" />
      <span class="text-stone-500">Material:</span>
      <span class="text-stone-900 font-medium">{{ product.materialComposition }}</span>
    </div>
    <div v-if="product.careInstructions" class="flex items-center gap-2 text-sm">
      <i class="pi pi-droplet text-teal-600" />
      <span class="text-stone-500">Care:</span>
      <span class="text-stone-900 font-medium">{{ product.careInstructions }}</span>
    </div>
    <div v-if="product.fitNotes" class="flex items-center gap-2 text-sm">
      <i class="pi pi-ruler text-teal-600" />
      <span class="text-stone-500">Fit:</span>
      <span class="text-stone-900 font-medium">{{ product.fitNotes }}</span>
    </div>
    <div v-if="product.seasonName" class="flex items-center gap-2 text-sm">
      <i class="pi pi-calendar text-teal-600" />
      <span class="text-stone-500">Season:</span>
      <span class="text-stone-900 font-medium">{{ product.seasonName }}</span>
    </div>
    <div v-if="product.department" class="flex items-center gap-2 text-sm">
      <i class="pi pi-tag text-teal-600" />
      <span class="text-stone-500">Department:</span>
      <span class="text-stone-900 font-medium">{{ product.department }}</span>
    </div>
    <div v-if="product.genderTarget" class="flex items-center gap-2 text-sm">
      <i class="pi pi-users text-teal-600" />
      <span class="text-stone-500">Gender:</span>
      <span class="text-stone-900 font-medium">{{ product.genderTarget }}</span>
    </div>
  </div>
</template>