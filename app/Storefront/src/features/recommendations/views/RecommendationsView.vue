<script setup lang="ts">
import { ref, computed } from "vue";
import { useRouter } from "vue-router";
import Button from "primevue/button";
import Card from "primevue/card";
import InputText from "primevue/inputtext";
import type { RecommendedProduct, RecommendationSet } from "../types";

const router = useRouter();

const isDragging = ref(false);
const uploadedImage = ref<string | null>(null);
const searchQuery = ref("");

const mockSimilarProducts: RecommendedProduct[] = [
  {
    id: "1",
    name: "Classic Cotton T-Shirt",
    brand: "ReSys",
    price: 29.99,
    image: "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=300",
    score: 0.95,
    reason: "Visually similar",
    badge: "trending",
  },
  {
    id: "2",
    name: "Slim Fit Jeans",
    brand: "ReSys",
    price: 79.99,
    image: "https://images.unsplash.com/photo-1542272604-787c3835535d?w=300",
    score: 0.88,
    reason: "Similar style",
  },
  {
    id: "3",
    name: "Wool Blend Coat",
    brand: "ReSys",
    price: 199.99,
    image: "https://images.unsplash.com/photo-1539533018447-63fcce2678e3?w=300",
    score: 0.82,
    reason: "Similar color",
    badge: "new",
  },
  {
    id: "4",
    name: "Leather Handbag",
    brand: "ReSys",
    price: 149.99,
    image: "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=300",
    score: 0.78,
    reason: "Similar material",
  },
  {
    id: "5",
    name: "Running Sneakers",
    brand: "ReSys",
    price: 119.99,
    image: "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=300",
    score: 0.75,
    reason: "Similar category",
  },
  {
    id: "6",
    name: "Summer Dress",
    brand: "ReSys",
    price: 89.99,
    image: "https://images.unsplash.com/photo-1572804013309-59a88b7e92f1?w=300",
    score: 0.72,
    reason: "Similar style",
  },
];

const mockYouMayLike: RecommendedProduct[] = [
  {
    id: "7",
    name: "Linen Blend Shirt",
    brand: "ReSys",
    price: 59.99,
    image: "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?w=300",
    score: 0.91,
    reason: "Based on your preferences",
    badge: "new",
  },
  {
    id: "8",
    name: "Tailored Trousers",
    brand: "ReSys",
    price: 89.99,
    image: "https://images.unsplash.com/photo-1594938298603-c8148c4dae35?w=300",
    score: 0.87,
    reason: "Popular in your area",
  },
  {
    id: "9",
    name: "Silk Scarf",
    brand: "ReSys",
    price: 49.99,
    image: "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=300",
    score: 0.84,
    reason: "Frequently bought together",
  },
  {
    id: "10",
    name: "Canvas Backpack",
    brand: "ReSys",
    price: 79.99,
    image: "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=300",
    score: 0.81,
    reason: "Trending now",
  },
];

const hasResults = computed(() => uploadedImage.value !== null);

function handleDragOver(event: DragEvent) {
  event.preventDefault();
  isDragging.value = true;
}

function handleDragLeave() {
  isDragging.value = false;
}

function handleDrop(event: DragEvent) {
  event.preventDefault();
  isDragging.value = false;
  
  const files = event.dataTransfer?.files;
  if (files && files.length > 0) {
    handleFile(files[0]!);
  }
}

function handleFileInput(event: Event) {
  const target = event.target as HTMLInputElement;
  const files = target.files;
  if (files && files.length > 0) {
    handleFile(files[0]!);
  }
}

function handleFile(file: File) {
  if (!file.type.startsWith("image/")) {
    return;
  }

  const reader = new FileReader();
  reader.onload = (e) => {
    uploadedImage.value = e.target?.result as string;
  };
  reader.readAsDataURL(file);
}

function clearImage() {
  uploadedImage.value = null;
}

function handleProductClick(productId: string) {
  router.push(`/product/${productId}`);
}

function handleSearch() {
  if (searchQuery.value.trim()) {
    router.push({
      path: "/shop",
      query: { q: searchQuery.value.trim() },
    });
  }
}
</script>

<template>
  <div class="recommendations-view">
    <div class="page-header">
      <h1>Image Search & Recommendations</h1>
      <p>Upload an image to find similar products or discover items you may like</p>
    </div>

    <div class="search-section">
      <div class="search-box">
        <i class="pi pi-search"></i>
        <InputText
          v-model="searchQuery"
          placeholder="Search for products..."
          @keyup.enter="handleSearch"
        />
      </div>
    </div>

    <div class="upload-section">
      <div
        class="drop-zone"
        :class="{ dragging: isDragging, 'has-image': uploadedImage }"
        @dragover="handleDragOver"
        @dragleave="handleDragLeave"
        @drop="handleDrop"
      >
        <template v-if="!uploadedImage">
          <div class="drop-content">
            <i class="pi pi-cloud-upload"></i>
            <h3>Drag & drop an image here</h3>
            <p>or</p>
            <label class="upload-btn">
              <Button label="Choose File" icon="pi pi-folder-open" />
              <input type="file" accept="image/*" @change="handleFileInput" hidden />
            </label>
            <span class="supported-formats">Supported: JPG, PNG, GIF, WebP</span>
          </div>
        </template>
        
        <template v-else>
          <div class="preview-container">
            <img :src="uploadedImage" alt="Uploaded image" class="preview-image" />
            <button class="clear-btn" @click="clearImage" aria-label="Clear image">
              <i class="pi pi-times"></i>
            </button>
          </div>
        </template>
      </div>
    </div>

    <div v-if="hasResults" class="results-section">
      <div class="similar-products">
        <h2>Similar Products</h2>
        <p class="section-subtitle">Products visually similar to your image</p>
        <div class="products-grid">
          <Card
            v-for="product in mockSimilarProducts"
            :key="product.id"
            class="product-card"
            @click="handleProductClick(product.id)"
          >
            <template #content>
              <div class="product-image">
                <img :src="product.image" :alt="product.name" />
                <span v-if="product.badge" class="badge" :class="product.badge">
                  {{ product.badge }}
                </span>
              </div>
              <div class="product-info">
                <span class="brand">{{ product.brand }}</span>
                <h4 class="name">{{ product.name }}</h4>
                <span class="price">${{ product.price.toFixed(2) }}</span>
                <span class="score">{{ Math.round(product.score * 100) }}% match</span>
              </div>
            </template>
          </Card>
        </div>
      </div>

      <div class="you-may-like">
        <h2>You May Also Like</h2>
        <p class="section-subtitle">Based on your browsing history</p>
        <div class="products-grid">
          <Card
            v-for="product in mockYouMayLike"
            :key="product.id"
            class="product-card"
            @click="handleProductClick(product.id)"
          >
            <template #content>
              <div class="product-image">
                <img :src="product.image" :alt="product.name" />
                <span v-if="product.badge" class="badge" :class="product.badge">
                  {{ product.badge }}
                </span>
              </div>
              <div class="product-info">
                <span class="brand">{{ product.brand }}</span>
                <h4 class="name">{{ product.name }}</h4>
                <span class="price">${{ product.price.toFixed(2) }}</span>
              </div>
            </template>
          </Card>
        </div>
      </div>
    </div>

    <div v-else class="empty-state">
      <i class="pi pi-image"></i>
      <h3>Try uploading an image</h3>
      <p>Upload a product image to find similar items or browse recommendations</p>
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
