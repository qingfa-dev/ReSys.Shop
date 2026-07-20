import { defineStore } from 'pinia'
import { ref, computed, shallowRef } from 'vue'
import type { Product, ProductFilter } from '../types'
import { productService } from '../services/product/product.service'

export const useProductStore = defineStore('product', () => {
  const products = shallowRef<Product[]>([])
  const currentProduct = ref<Product | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const pagination = ref({ page: 1, pageSize: 12, total: 0, totalPages: 0 })
  const filter = ref<ProductFilter>({})

  const hasProducts = computed(() => products.value.length > 0)
  const productCount = computed(() => pagination.value.total)

  async function fetchProducts(newFilter?: ProductFilter, page = 1) {
    loading.value = true
    error.value = null
    try {
      if (newFilter) {
        filter.value = { ...filter.value, ...newFilter }
      }
      const result = await productService.getProducts(filter.value, page, pagination.value.pageSize)
      if (result.isSuccess) {
        products.value = result.items
        pagination.value = {
          page: result.page,
          pageSize: result.pageSize,
          total: result.totalCount,
          totalPages: result.totalPages,
        }
      } else {
        error.value = result.message || 'Failed to fetch products'
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to fetch products'
    } finally {
      loading.value = false
    }
  }

  async function fetchProduct(id: string) {
    loading.value = true
    error.value = null
    try {
      const result = await productService.getProduct(id)
      if (result.isSuccess && result.data) {
        currentProduct.value = result.data
      } else {
        error.value = result.message || 'Failed to fetch product'
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to fetch product'
    } finally {
      loading.value = false
    }
  }

  async function getFeaturedProducts(limit = 4) {
    loading.value = true
    error.value = null
    try {
      const result = await productService.getFeaturedProducts(limit)
      if (result.isSuccess && result.data) {
        products.value = result.data
      } else {
        error.value = result.message || 'Failed to fetch featured products'
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to fetch featured products'
    } finally {
      loading.value = false
    }
  }

  async function getNewArrivals(limit = 8) {
    loading.value = true
    error.value = null
    try {
      const result = await productService.getNewArrivals(limit)
      if (result.isSuccess && result.data) {
        products.value = result.data
      } else {
        error.value = result.message || 'Failed to fetch new arrivals'
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to fetch new arrivals'
    } finally {
      loading.value = false
    }
  }

  function setPage(page: number) {
    fetchProducts(undefined, page)
  }

  function setFilter(newFilter: ProductFilter) {
    filter.value = newFilter
    fetchProducts()
  }

  function clearFilter() {
    filter.value = {}
    fetchProducts()
  }

  function clearCurrentProduct() {
    currentProduct.value = null
  }

  return {
    products,
    currentProduct,
    loading,
    error,
    pagination,
    filter,
    hasProducts,
    productCount,
    fetchProducts,
    fetchProduct,
    getFeaturedProducts,
    getNewArrivals,
    setPage,
    setFilter,
    clearFilter,
    clearCurrentProduct,
  }
})
