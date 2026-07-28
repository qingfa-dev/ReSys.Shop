import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { ProductFilter } from '../types'
import { useProductStore } from './product'

export const useCatalogStore = defineStore('catalog', () => {
  const filter = ref<ProductFilter>({})

  const productStore = useProductStore()

  const hasProducts = computed(() => productStore.hasProducts)
  const productCount = computed(() => productStore.productCount)
  const products = computed(() => productStore.products)
  const loading = computed(() => productStore.loading)
  const error = computed(() => productStore.error)
  const pagination = computed(() => productStore.pagination)

  function setPage(page: number) {
    productStore.fetchProducts(undefined, page)
  }

  function setFilter(newFilter: ProductFilter) {
    filter.value = newFilter
    productStore.fetchProducts()
  }

  function clearFilter() {
    filter.value = {}
    productStore.fetchProducts()
  }

  return {
    filter,
    hasProducts,
    productCount,
    products,
    loading,
    error,
    pagination,
    setPage,
    setFilter,
    clearFilter,
  }
})
