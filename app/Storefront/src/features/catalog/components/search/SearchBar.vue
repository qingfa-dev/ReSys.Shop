<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import AutoComplete from 'primevue/autocomplete'
import type { Product } from '@/features/catalog/types'
import { productService } from '@/features/catalog/services/product/product.service'

const router = useRouter()

const searchQuery = ref('')
const suggestions = ref<Product[]>([])
const selectedItem = ref<Product | null>(null)
const loading = ref(false)

let debounceTimeout: ReturnType<typeof setTimeout> | null = null

async function search(event: { query: string }) {
  if (debounceTimeout) clearTimeout(debounceTimeout)
  
  if (event.query.length < 2) {
    suggestions.value = []
    return
  }
  
  debounceTimeout = setTimeout(async () => {
    loading.value = true
    try {
      const result = await productService.searchProducts(event.query, 8)
      suggestions.value = result.data ?? []
    } catch {
      suggestions.value = []
    } finally {
      loading.value = false
    }
  }, 300)
}

function onSelect(event: { value: Product | null }) {
  if (event.value) {
    router.push(`/product/${event.value.slug}`)
    searchQuery.value = ''
    suggestions.value = []
  }
}

function handleSearch() {
  if (searchQuery.value.trim()) {
    router.push(`/shop?q=${encodeURIComponent(searchQuery.value)}`)
    searchQuery.value = ''
  }
}
</script>

<template>
  <div class="search-bar">
    <AutoComplete
      v-model="selectedItem"
      v-model:searchQuery="searchQuery"
      :suggestions="suggestions"
      optionLabel="name"
      placeholder="Search products..."
      :loading="loading"
      @complete="search"
      @item-select="onSelect"
      @keyup.enter="handleSearch"
      class="search-input"
    >
      <template #option="{ option }">
        <div class="search-suggestion">
          <div class="suggestion-image" :style="option.images?.[0] ? { backgroundImage: `url(${option.images[0]})` } : {}"></div>
          <div class="suggestion-info">
            <span class="suggestion-name">{{ option.name }}</span>
            <span class="suggestion-price">${{ option.price }}</span>
          </div>
        </div>
      </template>
      
      <template #empty>
        <div class="no-results">No products found</div>
      </template>
    </AutoComplete>
  </div>
</template>

<style scoped lang="scss">
.search-bar {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  width: 100%;
  max-width: 280px;
}

.search-input {
  flex: 1;
  
  :deep(.p-autocomplete-input) {
    width: 100%;
    padding: 0.5rem 0.75rem;
    padding-right: 2rem;
    border: 1px solid var(--color-border);
    border-radius: var(--radius-md);
    background: var(--color-surface-ground);
    font-size: var(--font-size-sm);
    
    &:focus {
      border-color: var(--color-primary);
      box-shadow: 0 0 0 2px rgba(15, 118, 110, 0.1);
    }
  }
}

.search-suggestion {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.5rem 0;
}

.suggestion-image {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-md);
  background-color: var(--color-surface-ground);
  background-size: cover;
  background-position: center;
}

.suggestion-info {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.suggestion-name {
  font-weight: var(--font-weight-medium);
  color: var(--color-text);
}

.suggestion-price {
  font-size: var(--font-size-sm);
  color: var(--color-primary);
}

.no-results {
  padding: 1rem;
  text-align: center;
  color: var(--color-text-muted);
}
</style>
