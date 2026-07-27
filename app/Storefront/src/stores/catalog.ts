import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '@/api'

/** A product in the catalog. */
export interface Product {
  /** Unique product identifier. */
  id: string
  /** Display name. */
  name: string
  /** URL-friendly slug. */
  slug: string
  /** Long-form product description. */
  description: string
  /** Price in cents or base currency unit. */
  price: number
  /** Image URLs. */
  images: string[]
  /** Category slug or ID. */
  category: string
  /** Average customer rating (0–5). */
  rating: number
  /** Whether the product is currently in stock. */
  inStock: boolean
}

/** Manage the product catalog: list, detail, and loading state. */
export const useCatalogStore = defineStore('catalog', () => {
  /** All loaded products. */
  const products = ref<Product[]>([])
  /** Whether a fetch is in progress. */
  const isLoading = ref(false)
  /** Last error message, or null. */
  const error = ref<string | null>(null)

  /** Look up a product by ID from the loaded list. */
  const getProductById = computed(() => (id: string) =>
    products.value.find((p) => p.id === id),
  )

  /** Fetch the product list from the API. */
  async function fetchProducts() {
    isLoading.value = true
    error.value = null
    try {
      products.value = await api.get<Product[]>('/api/products')
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Unknown error'
    } finally {
      isLoading.value = false
    }
  }

  /** Fetch a single product by ID and upsert it into the list. */
  async function fetchProduct(id: string) {
    isLoading.value = true
    error.value = null
    try {
      const product = await api.get<Product>(`/api/products/${id}`)
      const index = products.value.findIndex((p) => p.id === id)
      if (index >= 0) {
        products.value[index] = product
      } else {
        products.value.push(product)
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Unknown error'
    } finally {
      isLoading.value = false
    }
  }

  return {
    products,
    isLoading,
    error,
    getProductById,
    fetchProducts,
    fetchProduct,
  }
})
