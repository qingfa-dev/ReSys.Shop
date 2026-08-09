<script setup lang="ts">
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useVisualSearch } from '../composables/useVisualSearch'
import type { FileUploadSelectEvent } from 'primevue/fileupload'
import ProductGridCard from '../components/ProductGridCard.vue'

// Title: Browser tab title for the visual search page
usePageTitle('Visual Search')

const vs = useVisualSearch()

// Upload: Route the chosen file into the composable, then embed and search
function onSelect(event: FileUploadSelectEvent): void {
  const file = event.files[0] as File | undefined
  if (!file) return
  vs.selectFile(file)
  // Guard: Skip search when the composable rejected the file
  if (!vs.validationError) void vs.search()
}
</script>

<template>
  <div class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
    <!-- Section: Page Header — breadcrumb, heading and subtitle -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Visual Search' }]" class="mb-6" />
    <h1 class="mb-2 text-2xl font-semibold tracking-tight text-heading">
      Visual Search
    </h1>
    <p class="mb-8 text-sm text-muted">
      Find visually similar products by uploading an image
    </p>

    <!-- Section: Upload Panel — basic FileUpload feeding the store -->
    <Card class="mb-8">
      <template #content>
        <div class="flex flex-col items-center gap-4 py-6 text-center">
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
        Results ({{ vs.results.length }})
      </h2>
      <div class="grid grid-cols-2 gap-6 lg:grid-cols-4">
        <ProductGridCard
          v-for="item in vs.results"
          :key="item.id"
          :product="item"
          :show-similarity="true"
          :similarity-score="item.similarityScore"
        />
      </div>
    </div>

    <!-- Section: Error State — search failure message -->
    <Message v-if="vs.error" severity="error" :closable="false" class="mt-6">
      {{ vs.error }}
    </Message>
  </div>
</template>
