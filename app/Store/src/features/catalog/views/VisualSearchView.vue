<script setup lang="ts">
import { useVisualSearch } from '../composables/useVisualSearch'
import VisualSearchDropzone from '../components/VisualSearchDropzone.vue'
import ProductCard from '../components/ProductCard.vue'

const vs = useVisualSearch()

function onFileSelected(file: File): void {
  vs.selectFile(file)
}
</script>
<template>
  <!-- Section: Visual Search Page -->
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <h1 class="text-2xl font-bold text-stone-900 mb-8">Visual Search</h1>

    <!-- State: Empty -->
    <VisualSearchDropzone v-if="vs.state.value === 'empty'" @file-selected="onFileSelected" />

    <!-- State: Upload (preview shown) -->
    <template v-if="vs.state.value === 'upload' && vs.previewUrl.value">
      <div class="flex flex-col md:flex-row gap-8">
        <div class="w-full md:w-1/3">
          <img :src="vs.previewUrl.value" alt="Query image" class="w-full rounded-xl shadow" />
          <p class="text-sm text-stone-500 mt-2">{{ vs.selectedFile.value?.name }} ({{ ((vs.selectedFile.value?.size ?? 0) / 1024 / 1024).toFixed(1) }} MB)</p>
        </div>
        <div class="w-full md:w-2/3 flex flex-col justify-center items-center">
          <Button label="Search Similar Products" icon="pi pi-search" size="large" @click="vs.search()" />
          <Button label="Change image" severity="secondary" text class="mt-4" @click="vs.reset()" />
        </div>
      </div>
    </template>

    <!-- State: Loading -->
    <div v-if="vs.state.value === 'loading'" class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
      <div v-for="i in 8" :key="i" class="bg-stone-100 rounded-xl animate-pulse">
        <div class="aspect-square bg-stone-200 rounded-t-xl" />
        <div class="p-4 space-y-2">
          <div class="h-4 bg-stone-200 rounded w-3/4" />
          <div class="h-5 bg-stone-200 rounded w-1/3" />
        </div>
      </div>
    </div>

    <!-- State: Results -->
    <template v-if="vs.state.value === 'results'">
      <div class="flex flex-col md:flex-row gap-8">
        <!-- Query image sidebar -->
        <div class="w-full md:w-1/4 shrink-0">
          <img v-if="vs.previewUrl.value" :src="vs.previewUrl.value" alt="Query image" class="w-full rounded-xl shadow" />
          <Button label="New Search" severity="secondary" class="w-full mt-4" @click="vs.reset()" />
        </div>

        <!-- Results grid -->
        <div class="flex-1">
          <!-- Empty results -->
          <div v-if="vs.results.value.length === 0" class="text-center py-16">
            <i class="pi pi-image text-4xl text-stone-300 mb-4" />
            <h3 class="text-lg font-medium text-stone-900">We couldn't find products similar to your image.</h3>
            <p class="text-stone-500 mt-2">Try a different image or angle.</p>
            <Button label="Try Again" severity="secondary" class="mt-4" @click="vs.reset()" />
          </div>

          <!-- Result cards -->
          <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            <div v-for="item in vs.results.value" :key="item.variantId" class="relative">
              <ProductCard
                :product="{
                  id: item.productId,
                  masterVariantId: item.variantId,
                  name: item.productName,
                  status: '',
                  description: null,
                  slug: item.productId,
                  minPrice: item.price,
                  currency: null,
                  thumbnailUrl: item.imageUrl,
                  thumbnailAlt: item.productName,
                  styleCode: null,
                  seasonName: null,
                  materialComposition: null,
                  careInstructions: null,
                  fitNotes: null,
                  department: null,
                  genderTarget: null,
                  variantsCount: 0,
                  availableOn: null,
                }"
              />
            </div>
          </div>
        </div>
      </div>
    </template>

    <!-- State: Validation Error -->
    <Message v-if="vs.validationError.value" severity="error" :closable="true" @close="vs.validationError.value = null">
      {{ vs.validationError.value.message }}
    </Message>

    <!-- State: Search Error -->
    <Message v-if="vs.error.value" severity="error" :closable="true" @close="vs.error.value = null">
      {{ vs.error.value }}
    </Message>
  </div>
</template>
