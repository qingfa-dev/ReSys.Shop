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
  const similarProducts = ref<StoreProductListItemResponse[]>([])
  const relatedProducts = ref<StoreProductListItemResponse[]>([])
  const relatedLoading = ref(false)

  const selectedVariant = computed<StoreProductVariantResponse | null>(() =>
    product.value?.variants.find(v => v.id === selectedVariantId.value) ?? null
  )

  const stockLabel = computed(() => {
    const stock = selectedVariant.value?.stock
    if (!stock) return null
    if (stock.availableQuantity > 5) return null
    if (stock.availableQuantity > 0) return `Only ${stock.availableQuantity} left`
    if (stock.backorderable) return 'Available for backorder'
    return 'Out of stock'
  })

  const isInStock = computed(() => {
    const stock = selectedVariant.value?.stock
    return stock ? stock.availableQuantity > 0 || stock.backorderable : false
  })

  async function load(slug: string): Promise<void> {
    loading.value = true
    error.value = null
    const result = await ProductApi.getProductBySlug(slug)
    if (result.isSuccess) {
      product.value = result.value as any // ProductDetail
      selectedVariantId.value = product.value?.masterVariant?.id ?? null
      ProductApi.getSimilar(product.value!.id).then(r => {
        if (r.isSuccess) similarProducts.value = r.items
      })
      relatedLoading.value = true
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
    selectedVariantId.value = variantId
  }

  async function addToCart(): Promise<boolean> {
    if (!selectedVariantId.value) return false
    return true
  }

  function incrementQuantity(): void { if (quantity.value < 99) quantity.value++ }
  function decrementQuantity(): void { if (quantity.value > 1) quantity.value-- }
  function reset(): void {
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
