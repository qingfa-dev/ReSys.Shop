import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { ProductApi } from '../services/productApi'
import type { StoreProductDetailResponse, StoreProductListItemResponse, StoreProductVariantResponse } from '../types'

export const useProductDetailStore = defineStore('productDetail', () => {
  const product = ref<StoreProductDetailResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const selectedVariantId = ref<string | null>(null)
  const quantity = ref(1)
  const similarProducts = ref<(StoreProductListItemResponse & { similarityScore: number })[]>([])
  const relatedProducts = ref<StoreProductListItemResponse[]>([])
  const relatedLoading = ref(false)

  const selectedVariant = computed<StoreProductVariantResponse | null>(() =>
    // Compute: Derive selected variant from current variant ID — null if not found
    product.value?.variants.find(v => v.id === selectedVariantId.value) ?? null
  )

  const stockLabel = computed(() => {
    const stock = selectedVariant.value?.stock
    if (!stock) return null
    // Guard: Skip label when stock is plentiful (over 5 units)
    if (stock.availableQuantity > 5) return null
    if (stock.availableQuantity > 0) return `Only ${stock.availableQuantity} left`
    if (stock.backorderable) return 'Available for backorder'
    return 'Out of stock'
  })

  const isInStock = computed(() => {
    const stock = selectedVariant.value?.stock
    // Compute: In-stock includes backorderable items as a valid purchase option
    return stock ? stock.availableQuantity > 0 || stock.backorderable : false
  })

  async function load(slug: string): Promise<void> {
    loading.value = true
    error.value = null
    // Call: Catalog API — fetch product detail by slug
    const result = await ProductApi.getProductBySlug(slug)
    if (result.isSuccess) {
      product.value = result.value
      // Assign: Default to master variant on initial load
      selectedVariantId.value = product.value?.masterVariant?.id ?? null
      // Call: Fetch similar products in background — non-blocking for faster page load
      ProductApi.getSimilar(product.value!.id).then(r => {
        if (r.isSuccess) similarProducts.value = r.items
      })
      relatedLoading.value = true
      // Call: Fetch related products in background — non-blocking for faster page load
      ProductApi.getRelated(product.value!.id, { pageNumber: 1, pageSize: 12 }).then(r => {
        if (r.isSuccess) relatedProducts.value = r.items
        relatedLoading.value = false
      })
    } else {
      error.value = result.message ?? 'Product not found'
    }
    loading.value = false
  }

  function selectVariant(variantId: string): void {
    // Assign: User-selected variant drives price and stock display
    selectedVariantId.value = variantId
  }

  async function addToCart(): Promise<boolean> {
    // Guard: Require a selected variant before adding to cart
    if (!selectedVariantId.value) return false
    return true
  }

  function incrementQuantity(): void { if (quantity.value < 99) quantity.value++ }
  function decrementQuantity(): void { if (quantity.value > 1) quantity.value-- }
  function reset(): void {
    // Reset: All state to initial values when navigating away from product detail
    product.value = null
    loading.value = false
    error.value = null
    selectedVariantId.value = null
    quantity.value = 1
    similarProducts.value = []
    relatedProducts.value = []
    relatedLoading.value = false
  }

  return {
    product, loading, error, selectedVariantId, quantity, similarProducts, relatedProducts, relatedLoading,
    selectedVariant, stockLabel, isInStock,
    load, selectVariant, addToCart, incrementQuantity, decrementQuantity, reset,
  }
})
