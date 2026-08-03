<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import Button from 'primevue/button'
import Card from 'primevue/card'
import ProgressBar from 'primevue/progressbar'
import Skeleton from 'primevue/skeleton'
import { recommendationsService } from '../services/recommendations.service'
import type { Product } from '@/features/catalog/types'

const router = useRouter()

// Reactive state
const isDragging = ref(false)
const uploadedImage = ref<string | null>(null)   // data: URL for <img> preview
const selectedFile = ref<File | null>(null)       // raw File for upload
const isSearching = ref(false)
const errorMessage = ref<string | null>(null)
const searchCompleted = ref(false)

// The backend search response is typed as Product[], but the embedding
// similarity score is not part of the Product schema — surface it as optional.
type SearchResultProduct = Product & { similarityScore?: number }

const results = ref<SearchResultProduct[]>([])

// Computed state flags
const showEmptyState = computed(() => !uploadedImage.value && !isSearching.value)
const showUpload = computed(() => uploadedImage.value && !isSearching.value && results.value.length === 0 && !errorMessage.value)
const showLoading = computed(() => isSearching.value)
const hasResults = computed(() => results.value.length > 0 && !isSearching.value)
const showEmptyResults = computed(() => searchCompleted.value && !isSearching.value && results.value.length === 0)

// Client-side validation before any network request
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp']
const MAX_SIZE = 10 * 1024 * 1024 // 10 MB

function validateFile(file: File): string | null {
  if (!ALLOWED_TYPES.includes(file.type)) {
    return 'Please select a JPEG, PNG, or WebP image.'
  }
  if (file.size > MAX_SIZE) {
    return 'Image must be under 10 MB.'
  }
  return null
}

function handleFile(file: File) {
  errorMessage.value = null
  results.value = []
  searchCompleted.value = false
  const err = validateFile(file)
  if (err) { errorMessage.value = err; return }
  selectedFile.value = file
  const reader = new FileReader()
  reader.onload = (e) => { uploadedImage.value = e.target?.result as string }
  reader.readAsDataURL(file)
}

// Drag-and-drop handlers
function handleDragOver(e: DragEvent) { e.preventDefault(); isDragging.value = true }
function handleDragLeave() { isDragging.value = false }
function handleDrop(e: DragEvent) {
  e.preventDefault(); isDragging.value = false
  const files = e.dataTransfer?.files
  if (files?.length) handleFile(files[0]!)
}
function handleFileInput(e: Event) {
  const target = e.target as HTMLInputElement
  const files = target.files
  if (files?.length) handleFile(files[0]!)
}

// CBIR search
async function handleSearch() {
  if (!selectedFile.value) return
  isSearching.value = true
  errorMessage.value = null
  const result = await recommendationsService.searchByImage(selectedFile.value)
  isSearching.value = false
  if (result.isSuccess && result.data) {
    searchCompleted.value = true
    results.value = result.data
  } else {
    searchCompleted.value = false
    errorMessage.value = result.message || 'Search failed. Please try again.'
  }
}

function clearImage() {
  uploadedImage.value = null
  selectedFile.value = null
  results.value = []
  errorMessage.value = null
  searchCompleted.value = false
}

function handleProductClick(productId: string) {
  router.push(`/products/${productId}`)
}

// Formatting helpers
function formatPrice(price: number): string {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price)
}

function scoreClass(score: number): string {
  if (score >= 0.9) return 'high'
  if (score >= 0.8) return 'medium'
  return 'low'
}

// Product schema stores images as an array of URL strings or { url, alt } objects.
function productThumbnail(product: Product): string {
  const firstImage = product.images?.[0]
  if (!firstImage) return ''
  return typeof firstImage === 'string' ? firstImage : firstImage.url || ''
}
</script>

<template>
  <div class="recommendations-view">
    <div class="page-header">
      <h1>Image Search & Recommendations</h1>
      <p>Upload an image to find visually similar products</p>
    </div>

    <!-- State 1: Empty — drop zone -->
    <div v-if="showEmptyState" class="upload-section">
      <div
        class="drop-zone"
        :class="{ dragging: isDragging }"
        @dragover="handleDragOver"
        @dragleave="handleDragLeave"
        @drop="handleDrop"
      >
        <div class="drop-content">
          <i class="pi pi-cloud-upload"></i>
          <h3>Drag & drop an image here</h3>
          <p>or</p>
          <label class="upload-btn">
            <Button label="Choose an image" icon="pi pi-folder-open" />
            <input type="file" accept="image/jpeg,image/png,image/webp" @change="handleFileInput" hidden />
          </label>
          <span class="supported-formats">JPEG, PNG, or WebP up to 10 MB</span>
        </div>
      </div>
      <p v-if="errorMessage" class="error-message"><i class="pi pi-exclamation-circle"></i> {{ errorMessage }}</p>
    </div>

    <!-- State 2: Upload — image preview + Search button -->
    <div v-if="showUpload" class="upload-section">
      <div class="preview-container">
        <img :src="uploadedImage!" alt="Uploaded image" class="preview-image" />
        <p class="file-info">{{ selectedFile?.name }} ({{ ((selectedFile?.size ?? 0) / 1024).toFixed(1) }} KB)</p>
        <div class="preview-actions">
          <Button label="Search Similar Products" icon="pi pi-search" @click="handleSearch" />
          <Button label="Change image" icon="pi pi-refresh" class="p-button-outlined" @click="clearImage" />
        </div>
      </div>
    </div>

    <!-- State 3: Loading — skeleton grid -->
    <div v-if="showLoading" class="loading-section">
      <ProgressBar mode="indeterminate" />
      <div class="skeleton-grid">
        <Skeleton v-for="i in 8" :key="i" width="100%" height="320px" />
      </div>
    </div>

    <!-- State 4a: Results — product grid with similarity sidebar -->
    <div v-if="hasResults" class="results-layout">
      <aside class="query-sidebar">
        <img :src="uploadedImage!" alt="Query image" class="query-thumb" />
        <Button label="New Search" icon="pi pi-refresh" class="p-button-outlined" @click="clearImage" />
      </aside>
      <div class="results-grid">
        <Card
          v-for="product in results"
          :key="product.id"
          class="product-card"
          @click="handleProductClick(product.id)"
        >
          <template #content>
            <div class="product-image">
              <img :src="productThumbnail(product)" :alt="product.name" />
              <span
                v-if="product.similarityScore !== undefined"
                class="similarity-badge"
                :class="scoreClass(product.similarityScore)"
              >
                {{ Math.round(product.similarityScore * 100) }}% match
              </span>
            </div>
            <div class="product-info">
              <h4 class="name">{{ product.name }}</h4>
              <span class="price">{{ formatPrice(product.price) }}</span>
            </div>
          </template>
        </Card>
      </div>
    </div>

    <!-- State 4b: Empty results -->
    <div v-if="showEmptyResults" class="empty-results">
      <i class="pi pi-search-minus"></i>
      <h3>No similar products found</h3>
      <p>We couldn't find products visually similar to your image. Try a different image or browse the catalog.</p>
      <Button label="Try Again" icon="pi pi-refresh" @click="clearImage" />
    </div>

    <!-- MVP: dropped — backend /api/storefront/recommendations/personalized returns 501 -->
    <div v-if="false">
      <!-- You May Also Like section removed -->
    </div>
  </div>
</template>

<style scoped lang="scss">
.recommendations-view {
  max-width: 1400px;
  margin: 0 auto;
  padding: 2rem;
}

.page-header {
  text-align: center;
  padding: 3rem 0;

  h1 {
    font-size: var(--font-size-4xl);
    margin-bottom: 0.5rem;
  }

  p {
    color: var(--color-text-muted);
    font-size: var(--font-size-lg);
  }
}

.search-section {
  max-width: 600px;
  margin: 0 auto 2rem;

  .search-box {
    display: flex;
    align-items: center;
    gap: 1rem;
    background: var(--color-surface-ground);
    border: 2px solid var(--color-border-light);
    border-radius: var(--radius-full);
    padding: 0.75rem 1.5rem;
    transition: border-color var(--transition-fast);

    &:focus-within {
      border-color: var(--color-primary);
    }

    i {
      color: var(--color-text-secondary);
    }

    :deep(.p-inputtext) {
      flex: 1;
      border: none;
      background: transparent;
      font-size: var(--font-size-base);
      color: var(--color-text);

      &::placeholder {
        color: var(--color-text-secondary);
      }

      &:focus {
        box-shadow: none;
      }
    }
  }
}

.upload-section {
  max-width: 800px;
  margin: 0 auto 3rem;
}

.drop-zone {
  border: 2px dashed var(--color-border-light);
  border-radius: var(--radius-lg);
  padding: 3rem;
  text-align: center;
  transition: all var(--transition-fast);
  background: var(--color-surface-ground);

  &.dragging {
    border-color: var(--color-primary);
    background: rgba(var(--color-primary-rgb), 0.05);
  }

  &.has-image {
    padding: 2rem;
  }
}

.drop-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;

  i {
    font-size: 4rem;
    color: var(--color-text-secondary);
  }

  h3 {
    font-family: var(--font-body);
    font-size: var(--font-size-xl);
    color: var(--color-text);
    margin: 0;
  }

  p {
    color: var(--color-text-secondary);
    margin: 0;
  }
}

.upload-btn {
  cursor: pointer;

  :deep(.p-button) {
    background: var(--color-primary);
    border-color: var(--color-primary);
  }
}

.supported-formats {
  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
}

.preview-container {
  position: relative;
  display: inline-block;
}

.preview-image {
  max-width: 300px;
  max-height: 300px;
  border-radius: var(--radius-md);
  object-fit: contain;
}

.clear-btn {
  position: absolute;
  top: -10px;
  right: -10px;
  width: 32px;
  height: 32px;
  border: none;
  background: var(--color-surface);
  border-radius: var(--radius-full);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  box-shadow: var(--shadow-md);
  transition: all var(--transition-fast);

  &:hover {
    background: var(--color-primary);
    color: white;
  }
}

.results-section {
  margin-top: 2rem;
}

.similar-products,
.you-may-like {
  margin-bottom: 3rem;

  h2 {
    font-family: var(--font-display);
    font-size: var(--font-size-2xl);
    color: var(--color-text);
    margin-bottom: 0.25rem;
  }

  .section-subtitle {
    color: var(--color-text-secondary);
    margin-bottom: 1.5rem;
  }
}

.products-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 1.5rem;
}

.product-card {
  cursor: pointer;
  transition: transform var(--transition-fast);

  &:hover {
    transform: translateY(-4px);
  }

  :deep(.p-card-body) {
    padding: 0;
  }

  :deep(.p-card-content) {
    padding: 0;
  }
}

.product-image {
  position: relative;
  aspect-ratio: 3/4;
  overflow: hidden;
  background: var(--color-surface-ground);

  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  .badge {
    position: absolute;
    top: 0.75rem;
    left: 0.75rem;
    padding: 0.25rem 0.75rem;
    font-size: var(--font-size-xs);
    font-weight: var(--font-weight-semibold);
    text-transform: uppercase;
    border-radius: var(--radius-sm);

    &.trending {
      background: var(--color-primary);
      color: white;
    }

    &.new {
      background: var(--color-success);
      color: white;
    }

    &.sale {
      background: var(--color-danger);
      color: white;
    }
  }
}

.product-info {
  padding: 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;

  .brand {
    font-size: var(--font-size-xs);
    color: var(--color-text-secondary);
    text-transform: uppercase;
    letter-spacing: 0.05em;
  }

  .name {
    font-family: var(--font-body);
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-medium);
    color: var(--color-text);
    margin: 0;
  }

  .price {
    font-family: var(--font-body);
    font-size: var(--font-size-base);
    font-weight: var(--font-weight-semibold);
    color: var(--color-text);
  }

  .score {
    font-size: var(--font-size-xs);
    color: var(--color-primary);
  }
}

.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  color: var(--color-text-secondary);

  i {
    font-size: 4rem;
    margin-bottom: 1rem;
    opacity: 0.5;
  }

  h3 {
    font-family: var(--font-display);
    font-size: var(--font-size-xl);
    color: var(--color-text);
    margin-bottom: 0.5rem;
  }

  p {
    max-width: 400px;
    margin: 0 auto;
  }
}

@media (max-width: 768px) {
.recommendations-view {
    padding: 1rem;
    padding-top: calc(var(--header-height) + 1rem);
  }

  .products-grid {
    grid-template-columns: repeat(2, 1fr);
    gap: 1rem;
  }

  .drop-zone {
    padding: 2rem 1rem;
  }

  .preview-image {
    max-width: 200px;
    max-height: 200px;
  }
}
</style>

<style scoped lang="scss">
// New: Loading state
.loading-section {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

.skeleton-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1.5rem;
  margin-top: 2rem;

  @media (max-width: 768px) {
    grid-template-columns: repeat(2, 1fr);
  }
}

// New: Results layout with sidebar
.results-layout {
  display: flex;
  gap: 2rem;
  max-width: 1400px;
  margin: 2rem auto;
  padding: 0 2rem;

  @media (max-width: 768px) {
    flex-direction: column;
  }
}

.query-sidebar {
  flex: 0 0 240px;
  text-align: center;

  .query-thumb {
    width: 100%;
    border-radius: var(--radius-md);
    margin-bottom: 1rem;
    border: 2px solid var(--color-primary);
  }
}

.results-grid {
  flex: 1;
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 1.5rem;
}

// New: Similarity badge
.similarity-badge {
  position: absolute;
  top: 0.5rem;
  right: 0.5rem;
  padding: 0.25rem 0.5rem;
  border-radius: var(--radius-sm);
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-semibold);
  color: white;

  &.high   { background: #22c55e; }   // >= 90%
  &.medium { background: #f59e0b; }   // >= 80%
  &.low    { background: #6b7280; }   // < 80%
}

// New: Empty results state
.empty-results {
  text-align: center;
  padding: 4rem 2rem;

  i { font-size: 4rem; color: var(--color-text-secondary); margin-bottom: 1rem; }
  h3 { font-family: var(--font-display); font-size: var(--font-size-xl); margin-bottom: 0.5rem; }
  p { max-width: 480px; margin: 0 auto 1.5rem; color: var(--color-text-secondary); }
}

// New: Error message
.error-message {
  color: var(--color-danger);
  text-align: center;
  margin-top: 0.75rem;

  i { margin-right: 0.25rem; }
}

// New: File info in preview
.file-info {
  color: var(--color-text-secondary);
  font-size: var(--font-size-sm);
  margin: 0.5rem 0 1rem;
}

.preview-actions {
  display: flex;
  gap: 0.75rem;
  justify-content: center;
}
</style>
