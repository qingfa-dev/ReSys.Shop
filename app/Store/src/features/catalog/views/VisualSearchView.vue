<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useVisualSearchStore } from '../stores/visualSearchStore'

// Assign: Page title for browser tab and SEO
usePageTitle('Visual Search')
const vs = useVisualSearchStore()
const fileInput = ref<HTMLInputElement | null>(null)

// Call: Load available ML models for model selector on mount
onMounted(() => vs.loadModels())

// Route: File input change to store for validation and preview
function onFileChange(e: Event): void {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  if (file) vs.selectFile(file)
}

// Route: Drag-and-drop file to store for validation and preview
function onDrop(e: DragEvent): void {
  e.preventDefault()
  const file = e.dataTransfer?.files?.[0]
  if (file) vs.selectFile(file)
}

// Guard: Prevent default browser behavior on drag over
function onDragOver(e: DragEvent): void {
  e.preventDefault()
}

// Reset: Clear selection and return to empty state
function onChangeImage(): void {
  vs.reset()
  if (fileInput.value) fileInput.value.value = ''
}

// Action: Trigger visual search with selected image and model
function onSearch(): void {
  vs.search()
}
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Section: Page Header — breadcrumb, heading, and subtitle -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Visual Search' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-2">Visual Search</h1>
    <p class="text-sm text-neutral-500 mb-8">Find visually similar products by uploading an image</p>

    <!-- Section: Upload Drop Zone — drag-and-drop or click to select image -->
    <div
      v-if="vs.state === 'empty'"
      class="border-2 border-dashed border-neutral-300 rounded-xl py-16 text-center cursor-pointer hover:border-neutral-500 transition-colors"
      @click="fileInput?.click()"
      @drop="onDrop"
      @dragover="onDragOver"
    >
      <i class="pi pi-cloud-upload text-4xl text-neutral-300 mb-4 block" />
      <p class="text-lg font-medium text-neutral-900 mb-2">Upload an image</p>
      <p class="text-sm text-neutral-500">JPEG, PNG, WebP — Max 10 MB</p>
      <input ref="fileInput" type="file" accept="image/jpeg,image/png,image/webp" class="hidden" @change="onFileChange" />
    </div>

    <!-- Section: Upload Preview — selected image with model selector and search button -->
    <div v-if="vs.state === 'upload' || vs.state === 'loading'" class="flex items-start gap-6">
      <div class="relative shrink-0">
        <img v-if="vs.previewUrl" :src="vs.previewUrl" alt="Preview" class="w-40 h-40 object-cover rounded-lg" />
        <button
          class="absolute top-2 right-2 w-6 h-6 bg-white rounded-full shadow text-xs flex items-center justify-center hover:bg-neutral-100"
          @click="onChangeImage()"
        >
          &times;
        </button>
      </div>
      <div class="flex-1 min-w-0">
        <Select
          v-if="vs.availableModels.length > 0"
          v-model="vs.selectedModelId"
          :options="vs.availableModels"
          option-label="name"
          option-value="id"
          placeholder="Select model"
          class="w-full mb-3"
        />
        <p v-if="vs.validationError" class="text-sm text-red-600 mb-3">{{ vs.validationError }}</p>
        <Button
          label="Search"
          severity="primary"
          :loading="vs.state === 'loading'"
          :disabled="!vs.selectedFile"
          @click="onSearch()"
        />
      </div>
    </div>

    <!-- Section: Loading Skeleton — placeholder grid while search is in progress -->
    <div v-if="vs.state === 'loading'" class="grid grid-cols-2 md:grid-cols-4 gap-4 mt-8">
      <Skeleton v-for="i in 8" :key="i" class="aspect-[3/4] rounded-lg" />
    </div>

    <!-- Section: Results Grid — visually similar products via ProductCard -->
    <div v-if="vs.state === 'results'">
      <h2 class="text-sm font-medium text-neutral-500 uppercase tracking-wide mb-6 mt-8">
        Results ({{ vs.results.length }})
      </h2>
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <ProductCard
          v-for="item in vs.results"
          :key="item.id"
          :product="item"
          :show-similarity="true"
          :similarity-score="item.similarityScore"
        />
      </div>
    </div>

    <!-- Section: Error Toast — display API or validation errors -->
    <Message v-if="vs.error" severity="error" class="mt-4">{{ vs.error }}</Message>
    <Message v-if="vs.validationError && vs.state === 'empty'" severity="error" class="mt-4">{{ vs.validationError }}</Message>
  </div>
</template>
