<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useVisualSearch } from '../composables/useVisualSearch'
import VisualSearchDropzone from '../components/VisualSearchDropzone.vue'
import ProductCard from '../components/ProductCard.vue'
import { getVisualSearchModels } from '../services/searchByImageApi'
import type { VisualSearchModel } from '../types/searchByImage'

const vs = useVisualSearch()

const models = ref<VisualSearchModel[]>([])
const selectedModel = ref<string | null>(null)

// Load available visual search models on mount
onMounted(async () => {
  const res = await getVisualSearchModels()
  if (res.isSuccess) models.value = res.value
})

function onFileSelected(file: File): void {
  vs.selectFile(file)
}

function onSearch(): void {
  vs.search(20, selectedModel.value ?? undefined)
}
</script>
<template>
  <!-- Section: Visual Search Page -->
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <h1 class="text-2xl font-bold text-stone-900 mb-8">Visual Search</h1>

    <!-- Section: Model Selector -->
    <div v-if="models.length > 0" class="mb-4">
      <label class="block text-sm font-medium text-stone-700 mb-1">Model</label>
      <Select
        v-model="selectedModel"
        :options="models"
        option-label="name"
        option-value="id"
        placeholder="Select model"
        class="w-full md:w-64"
      />
    </div>

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
          <Button label="Search Similar Products" icon="pi pi-search" size="large" @click="onSearch" />
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
              <!-- Similarity score badge -->
              <span
                class="absolute top-2 right-2 z-10 text-xs font-bold px-2 py-0.5 rounded-full"
                :class="item.similarityScore > 0.85 ? 'bg-emerald-100 text-emerald-700' : item.similarityScore > 0.7 ? 'bg-amber-100 text-amber-700' : 'bg-red-100 text-red-700'"
              >
                {{ Math.round(item.similarityScore * 100) }}%
              </span>
              <ProductCard
                :product="{
                  id: item.productId,
                  masterVariantId: item.variantId,
                  name: item.productName,
                  status: '',
                  description: null,
                  slug: item.productId,
                  styleCode: null,
                  seasonName: null,
                  materialComposition: null,
                  careInstructions: null,
                  fitNotes: null,
                  department: null,
                  genderTarget: null,
                  variantsCount: 1,
                  availableOn: null,
                  masterVariant: {
                    id: item.variantId,
                    sku: item.sku,
                    isMaster: true,
                    price: item.price,
                    currency: null,
                    optionValues: [],
                    images: item.imageUrl ? [{ id: item.variantId, url: item.imageUrl, alt: item.productName, position: 0 }] : [],
                    prices: [],
                    stock: { availableQuantity: 0, backorderable: false },
                  },
                  classifications: [],
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
