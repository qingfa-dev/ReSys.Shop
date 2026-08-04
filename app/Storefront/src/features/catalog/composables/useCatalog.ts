import { computed } from 'vue'
import { useProductStore } from '../store/product'
import { useCategoryStore } from '../store/category'
import { productService } from '../services/product/product.service'

export function useCatalog() {
  const productStore = useProductStore()
  const categoryStore = useCategoryStore()

  const loading = computed(() => productStore.loading)
  const error = computed(() => productStore.error)
  const hasError = computed(() => !!productStore.error)
  const isLoading = loading
  const products = computed(() => productStore.products)
  const categories = computed(() => categoryStore.categories)
  const currentProduct = computed(() => productStore.currentProduct)
  const pagination = computed(() => productStore.pagination)
  const filter = computed(() => productStore.filter)

  async function loadProducts(page = 1) {
    await productStore.fetchProducts(undefined, page)
  }

  async function loadProduct(id: string) {
    await productStore.fetchProduct(id)
  }

  async function loadCategories() {
    await categoryStore.fetchCategories()
  }

  function search(query: string) {
    productStore.setFilter({ ...productStore.filter, tags: query ? [query] : [] })
    productStore.fetchProducts()
  }

  async function searchProducts(query: string, limit = 10) {
    const result = await productService.searchProducts(query, limit)
    if (result.isSuccess && result.data) {
      return result.data
    }
    return []
  }

  function sortBy(sort: 'newest' | 'price-asc' | 'price-desc' | 'popular') {
    productStore.setFilter({ ...productStore.filter, sortBy: sort })
    productStore.fetchProducts()
  }

  function setTaxon(taxonId: string) {
    productStore.setFilter({ ...productStore.filter, taxonId: [taxonId] })
    productStore.fetchProducts()
  }

  function setPriceRange(min: number, max: number) {
    productStore.setFilter({ ...productStore.filter, priceMin: min, priceMax: max })
    productStore.fetchProducts()
  }

  function setOptionValues(optionValueIds: string[]) {
    productStore.setFilter({ ...productStore.filter, optionTypeId: optionValueIds })
    productStore.fetchProducts()
  }

  function clearFilters() {
    productStore.clearFilter()
  }

  function goToPage(page: number) {
    productStore.setPage(page)
  }

  return {
    loading,
    error,
    hasError,
    isLoading,
    products,
    categories,
    currentProduct,
    pagination,
    filter,
    loadProducts,
    loadProduct,
    loadCategories,
    search,
    searchProducts,
    sortBy,
    setTaxon,
    setPriceRange,
    setOptionValues,
    clearFilters,
    goToPage,
  }
}
