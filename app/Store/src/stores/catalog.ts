import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '@/api'

export interface Product {
  id: string
  name: string
  slug: string
  description: string
  price: number
  images: string[]
  category: string
  rating: number
  inStock: boolean
}

export const useCatalogStore = defineStore('catalog', () => {
  const products = ref<Product[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const getProductById = computed(() => (id: string) =>
    products.value.find((p) => p.id === id),
  )

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
