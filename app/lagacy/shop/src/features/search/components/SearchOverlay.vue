<script setup lang="ts">
import { ref, computed, watch } from "vue";
import { useRouter } from "vue-router";
import Dialog from "primevue/dialog";
import InputText from "primevue/inputtext";
import { useSearchStore } from "../store/search";
import type { SearchResult } from "../types";

const props = defineProps<{
  visible: boolean;
}>();

const emit = defineEmits<{
  (e: "update:visible", value: boolean): void;
}>();

const router = useRouter();
const searchStore = useSearchStore();

const searchQuery = ref("");
const mockResults = ref<SearchResult[]>([]);
const mockSuggestions = ref<string[]>([
  "Dresses",
  "Tops",
  "Trousers",
  "Jackets",
  "Shoes",
  "Bags",
]);

const showResults = computed(() => searchQuery.value.length >= 2);

watch(
  () => props.visible,
  (newVal) => {
    if (newVal) {
      searchQuery.value = "";
      mockResults.value = [];
    }
  }
);

function handleSearch() {
  if (searchQuery.value.trim()) {
    router.push({
      path: "/shop",
      query: { q: searchQuery.value.trim() },
    });
    closeOverlay();
  }
}

function handleSuggestionClick(suggestion: string) {
  searchQuery.value = suggestion;
  handleSearch();
}

function handleProductClick(productId: string) {
  router.push(`/product/${productId}`);
  closeOverlay();
}

function closeOverlay() {
  emit("update:visible", false);
}

function handleKeydown(event: KeyboardEvent) {
  if (event.key === "Escape") {
    closeOverlay();
  }
}

function getMockResults(query: string): SearchResult[] {
  const products: SearchResult[] = [
    {
      id: "1",
      name: "Classic Cotton T-Shirt",
      brand: "ReSys",
      price: 29.99,
      image: "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=300",
      rating: 4.5,
      reviews: 128,
      inStock: true,
    },
    {
      id: "2",
      name: "Slim Fit Jeans",
      brand: "ReSys",
      price: 79.99,
      image: "https://images.unsplash.com/photo-1542272604-787c3835535d?w=300",
      rating: 4.3,
      reviews: 89,
      inStock: true,
    },
    {
      id: "3",
      name: "Wool Blend Coat",
      brand: "ReSys",
      price: 199.99,
      image: "https://images.unsplash.com/photo-1539533018447-63fcce2678e3?w=300",
      rating: 4.8,
      reviews: 56,
      inStock: true,
    },
    {
      id: "4",
      name: "Leather Handbag",
      brand: "ReSys",
      price: 149.99,
      image: "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=300",
      rating: 4.6,
      reviews: 234,
      inStock: true,
    },
    {
      id: "5",
      name: "Running Sneakers",
      brand: "ReSys",
      price: 119.99,
      image: "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=300",
      rating: 4.4,
      reviews: 167,
      inStock: true,
    },
    {
      id: "6",
      name: "Summer Dress",
      brand: "ReSys",
      price: 89.99,
      image: "https://images.unsplash.com/photo-1572804013309-59a88b7e92f1?w=300",
      rating: 4.7,
      reviews: 92,
      inStock: true,
    },
  ];

  const lowerQuery = query.toLowerCase();
  return products.filter(
    (p) =>
      p.name.toLowerCase().includes(lowerQuery) ||
      p.brand.toLowerCase().includes(lowerQuery)
  );
}

watch(
  () => searchQuery.value,
  (newQuery) => {
    if (newQuery.length >= 2) {
      mockResults.value = getMockResults(newQuery);
    } else {
      mockResults.value = [];
    }
  }
);
</script>

<template>
  <Dialog
    :visible="visible"
    modal
    :closable="false"
    :showHeader="false"
    position="top"
    class="search-overlay-dialog"
    :style="{ width: '100%', maxWidth: '100%', margin: '0' }"
    @update:visible="closeOverlay"
  >
    <div class="search-overlay" @keydown="handleKeydown">
      <div class="search-header">
        <div class="search-input-wrapper">
          <i class="pi pi-search search-icon"></i>
          <InputText
            v-model="searchQuery"
            placeholder="Search for products, brands, categories..."
            class="search-input"
            @keyup.enter="handleSearch"
            autofocus
          />
          <button class="close-btn" @click="closeOverlay" aria-label="Close search">
            <i class="pi pi-times"></i>
          </button>
        </div>
      </div>

      <div class="search-content">
        <div v-if="!showResults" class="search-suggestions">
          <h3 class="section-title">Popular Searches</h3>
          <div class="suggestions-grid">
            <button
              v-for="suggestion in mockSuggestions"
              :key="suggestion"
              class="suggestion-chip"
              @click="handleSuggestionClick(suggestion)"
            >
              {{ suggestion }}
            </button>
          </div>
        </div>

        <div v-else class="search-results">
          <div v-if="mockResults.length > 0" class="results-grid">
            <div
              v-for="product in mockResults"
              :key="product.id"
              class="result-item"
              @click="handleProductClick(product.id)"
            >
              <div class="result-image">
                <img :src="product.image" :alt="product.name" />
              </div>
              <div class="result-info">
                <span class="result-brand">{{ product.brand }}</span>
                <h4 class="result-name">{{ product.name }}</h4>
                <span class="result-price">${{ product.price.toFixed(2) }}</span>
              </div>
            </div>
          </div>
          <div v-else class="no-results">
            <i class="pi pi-search"></i>
            <p>No results found for "{{ searchQuery }}"</p>
          </div>

          <div class="view-all-wrapper">
            <button class="view-all-btn" @click="handleSearch">
              View all results for "{{ searchQuery }}"
              <i class="pi pi-arrow-right"></i>
            </button>
          </div>
        </div>
      </div>
    </div>
  </Dialog>
</template>

<style scoped lang="scss">
.search-overlay {
  background: var(--color-surface);
  min-height: 100vh;
  padding: 1.5rem 2rem 2rem;
}

.search-header {
  max-width: 800px;
  margin: 0 auto 2rem;
}

.search-input-wrapper {
  display: flex;
  align-items: center;
  gap: 1rem;
  background: var(--color-surface-ground);
  border-radius: var(--radius-lg);
  padding: 0.75rem 1rem;
  border: 2px solid var(--color-border-light);
  transition: border-color var(--transition-fast);

  &:focus-within {
    border-color: var(--color-primary);
  }
}

.search-icon {
  color: var(--color-text-secondary);
  font-size: 1.25rem;
}

.search-input {
  flex: 1;
  border: none;
  background: transparent;
  font-size: 1.25rem;
  font-family: var(--font-body);
  color: var(--color-text);
  outline: none;

  &::placeholder {
    color: var(--color-text-secondary);
  }
}

.close-btn {
  width: 40px;
  height: 40px;
  border: none;
  background: transparent;
  border-radius: var(--radius-full);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text-secondary);
  cursor: pointer;
  transition: all var(--transition-fast);

  &:hover {
    background: var(--color-surface-ground);
    color: var(--color-text);
  }

  i {
    font-size: 1.25rem;
  }
}

.search-content {
  max-width: 1200px;
  margin: 0 auto;
}

.section-title {
  font-family: var(--font-body);
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-semibold);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-secondary);
  margin-bottom: 1rem;
}

.suggestions-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
}

.suggestion-chip {
  padding: 0.75rem 1.25rem;
  background: var(--color-surface-ground);
  border: 1px solid var(--color-border-light);
  border-radius: var(--radius-full);
  font-family: var(--font-body);
  font-size: var(--font-size-sm);
  color: var(--color-text);
  cursor: pointer;
  transition: all var(--transition-fast);

  &:hover {
    background: var(--color-primary);
    border-color: var(--color-primary);
    color: white;
  }
}

.results-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 1.5rem;
}

.result-item {
  cursor: pointer;
  transition: transform var(--transition-fast);

  &:hover {
    transform: translateY(-4px);
  }
}

.result-image {
  aspect-ratio: 3/4;
  border-radius: var(--radius-md);
  overflow: hidden;
  background: var(--color-surface-ground);
  margin-bottom: 0.75rem;

  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }
}

.result-info {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.result-brand {
  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.result-name {
  font-family: var(--font-body);
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);
  color: var(--color-text);
  margin: 0;
}

.result-price {
  font-family: var(--font-body);
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text);
}

.no-results {
  text-align: center;
  padding: 4rem 2rem;
  color: var(--color-text-secondary);

  i {
    font-size: 3rem;
    margin-bottom: 1rem;
  }

  p {
    font-size: var(--font-size-lg);
  }
}

.view-all-wrapper {
  margin-top: 2rem;
  text-align: center;
}

.view-all-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 1rem 2rem;
  background: var(--color-text);
  color: var(--color-surface);
  border: none;
  border-radius: var(--radius-full);
  font-family: var(--font-body);
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-semibold);
  cursor: pointer;
  transition: all var(--transition-fast);

  &:hover {
    background: var(--color-primary);
  }
}

@media (max-width: 768px) {
  .search-overlay {
    padding: 1rem;
  }

  .search-input {
    font-size: 1rem;
  }

  .search-input-wrapper {
    padding: 0.5rem 0.75rem;
  }

  .results-grid {
    grid-template-columns: repeat(2, 1fr);
    gap: 1rem;
  }
}
</style>

<style lang="scss">
.search-overlay-dialog {
  .p-dialog-content {
    padding: 0;
    overflow: hidden;
  }

  .p-dialog-mask {
    background: rgba(0, 0, 0, 0.5);
    backdrop-filter: blur(4px);
  }
}
</style>
