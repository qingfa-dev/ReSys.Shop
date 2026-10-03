<script setup lang="ts">
import { onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useVisualSearch } from '../composables/useVisualSearch'
import type { FileUploadSelectEvent } from 'primevue/fileupload'
import Label from 'primevue/label'
import ProductGridCard from '../components/ProductGridCard.vue'

usePageTitle('Visual Search')

const vs = useVisualSearch()

// Load: Fetch available ML models on mount
onMounted(() => { vs.loadModels() })

// Upload: Route the chosen file into the composable
function onSelect(event: FileUploadSelectEvent): void {
  const file = event.files[0] as File | undefined
  if (!file) return
  vs.selectFile(file)
  if (!vs.validationError) void vs.search(vs.topK)
}

// Search: Re-run search with current parameters
function onSearch(): void {
  if (!vs.selectedFile) return
  void vs.search(vs.topK)
}
</script>

<template>
  <div class="mx-auto max-w-screen-2xl px-4 py-8 sm:px-6 lg:px-8">
    <!-- Section: Page Header — breadcrumb, heading and subtitle -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Visual Search' }]" class="mb-6" />
    <h1 class="mb-2 text-2xl font-semibold tracking-tight text-heading">
      Visual Search
    </h1>
    <p class="mb-8 text-sm text-muted">
      Find visually similar products by uploading an image
    </p>

    <!-- Section: Upload Panel — file picker with image preview and clear action -->
    <Card class="mb-6">
      <template #content>
        <div class="flex flex-col items-center gap-4 py-4">
          <div v-if="vs.previewUrl" class="relative">
            <img
              :src="vs.previewUrl"
              alt="Preview"
              class="max-h-48 rounded-lg border border-border object-contain"
            />
            <Button
              icon="pi pi-times"
              rounded
              size="small"
              severity="danger"
              class="absolute -top-2 -right-2"
              @click="vs.reset()"
              aria-label="Clear image"
            />
          </div>
          <div v-else class="flex flex-col items-center gap-3">
            <FileUpload
              mode="basic"
              accept="image/*"
              chooseLabel="Choose an image"
              :auto="false"
              :customUpload="true"
              @select="onSelect"
            />
            <p class="text-sm text-muted">JPEG, PNG, WebP — max 10 MB</p>
          </div>
        </div>
      </template>
    </Card>

    <!-- Section: Search Parameters — model, result count, threshold, and weight controls -->
    <Card v-if="vs.selectedFile" class="mb-6">
      <template #content>
        <div class="grid gap-6 sm:grid-cols-2">
          <div class="flex flex-col gap-2">
            <Label for="model">Model</Label>
            <Select
              id="model"
              v-model="vs.selectedModelId"
              :options="vs.availableModels"
              optionLabel="name"
              optionValue="id"
              placeholder="Select model"
              class="w-full"
            />
            <p class="text-xs text-muted">Embedding model used to encode your image into a vector for similarity search.</p>
          </div>

          <div class="flex flex-col gap-2">
            <Label for="topK">Results</Label>
            <div class="flex items-center gap-3">
              <Slider
                id="topK"
                v-model="vs.topK"
                :min="1"
                :max="50"
                class="flex-1"
              />
              <InputNumber
                v-model="vs.topK"
                :min="1"
                :max="50"
                showButtons
                buttonLayout="horizontal"
                :inputStyle="{ width: '3rem', textAlign: 'center' }"
              />
            </div>
            <p class="text-xs text-muted">Maximum number of similar products to return (1-50).</p>
          </div>

          <div class="flex flex-col gap-2">
            <Label for="threshold">Min Match %</Label>
            <div class="flex items-center gap-3">
              <Slider
                id="threshold"
                v-model="vs.minSimilarity"
                :min="0"
                :max="100"
                class="flex-1"
              />
              <InputNumber
                v-model="vs.minSimilarity"
                :min="0"
                :max="100"
                suffix="%"
                :inputStyle="{ width: '4rem', textAlign: 'center' }"
              />
            </div>
            <p class="text-xs text-muted">Client-side filter: hide results below this similarity percentage.</p>
          </div>

          <div class="flex flex-col gap-2">
            <Label for="weight">Score Weight</Label>
            <div class="flex items-center gap-3">
              <Slider
                id="weight"
                v-model="vs.scoreWeight"
                :min="0.1"
                :max="3"
                :step="0.1"
                class="flex-1"
              />
              <InputNumber
                v-model="vs.scoreWeight"
                :min="0.1"
                :max="3"
                :step="0.1"
                :inputStyle="{ width: '4rem', textAlign: 'center' }"
              />
            </div>
            <p class="text-xs text-muted">Multiplier applied to similarity scores before threshold filtering (default 1.0).</p>
          </div>
        </div>

        <!-- Section: Parameter Actions — search and reset buttons -->
        <div class="mt-6 flex gap-3">
          <Button
            label="Search"
            icon="pi pi-search"
            :loading="vs.loading"
            @click="onSearch"
          />
          <Button
            label="Reset"
            icon="pi pi-refresh"
            severity="secondary"
            @click="vs.reset()"
          />
        </div>
      </template>
    </Card>

    <!-- Section: Validation Error — file rejected by the store -->
    <Message v-if="vs.validationError" severity="error" :closable="false" class="mb-6">
      {{ vs.validationError }}
    </Message>

    <!-- Section: Loading State — spinner while the image is embedded -->
    <div v-if="vs.state === 'loading'" class="flex flex-col items-center gap-4 py-16">
      <ProgressSpinner style="width: 3rem; height: 3rem" :strokeWidth="4" />
      <p class="text-sm text-muted">
        Embedding image and searching the catalog…
      </p>
    </div>

    <!-- Section: Results Grid — visually similar products with match tags -->
    <div v-if="vs.state === 'results'">
      <h2 class="mb-6 text-sm font-medium uppercase tracking-wide text-muted">
        Results ({{ vs.filteredResults.length }})
      </h2>
      <div class="grid grid-cols-2 gap-6 lg:grid-cols-4">
        <ProductGridCard
          v-for="item in vs.filteredResults"
          :key="item.id"
          :product="item"
          :show-similarity="true"
          :similarity-score="item.adjustedScore"
        />
      </div>
      <p v-if="vs.filteredResults.length === 0" class="py-12 text-center text-sm text-muted">
        No results above {{ vs.minSimilarity }}% similarity threshold.
      </p>
    </div>

    <!-- Section: Error State — search failure message -->
    <Message v-if="vs.error" severity="error" :closable="false" class="mt-6">
      {{ vs.error }}
    </Message>
  </div>
</template>
