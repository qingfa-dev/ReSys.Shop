import { ref, computed, reactive } from 'vue'
import { ProductApi } from '../services/productApi'
import type { StoreProductDetailResponse, StoreProductListItemResponse, StoreProductVariantResponse } from '../types'

// Module-level singleton state
const product = ref<StoreProductDetailResponse | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)
const selectedVariantId = ref<string | null>(null)
const quantity = ref(1)
const similarProducts = ref<(StoreProductListItemResponse & { similarityScore: number })[]>([])
const relatedProducts = ref<StoreProductListItemResponse[]>([])
const relatedLoading = ref(false)

// Seq: Monotonic load counter — stale loads discard their responses after rapid navigation
let _loadSeq = 0

const selectedVariant = computed<StoreProductVariantResponse | null>(() =>
  product.value?.variants.find(v => v.id === selectedVariantId.value) ?? null
)

const stockLabel = computed(() => {
  const stock = selectedVariant.value?.stock
  if (!stock) return null
  if (stock.totalAvailable > 5) return null
  if (stock.totalAvailable > 0) return `Only ${stock.totalAvailable} left`
  if (stock.backorderable) return 'Available for backorder'
  return 'Out of stock'
})

const isInStock = computed(() => {
  const stock = selectedVariant.value?.stock
  return stock ? stock.totalAvailable > 0 || stock.backorderable : false
})

async function load(id: string): Promise<void> {
  const seq = ++_loadSeq
  loading.value = true
  error.value = null
  try {
    const result = await ProductApi.getProductById(id)
    if (seq !== _loadSeq) return
    if (result.isSuccess) {
      product.value = result.value
      selectedVariantId.value = product.value?.masterVariant?.id ?? null
      ProductApi.getSimilar(product.value!.id).then(r => {
        if (seq !== _loadSeq) return
        if (r.isSuccess) similarProducts.value = r.items
      }).catch(() => {
        // Guard: Leave the similar rail empty when the recommendation call fails.
      })
      relatedLoading.value = true
      ProductApi.getRelated(product.value!.id, { pageNumber: 1, pageSize: 12 }).then(r => {
        if (seq !== _loadSeq) return
        if (r.isSuccess) relatedProducts.value = r.items
        relatedLoading.value = false
      }).catch(() => {
        if (seq === _loadSeq) relatedLoading.value = false
      })
    } else {
      error.value = result.message ?? 'Product not found'
    }
  } catch (e) {
    if (seq === _loadSeq) {
      error.value = e instanceof Error ? e.message : 'Failed to load product'
    }
  } finally {
    if (seq === _loadSeq) loading.value = false
  }
}

function selectVariant(variantId: string): void {
  selectedVariantId.value = variantId
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

export function useProductDetail() {
  return reactive({
    product, loading, error, selectedVariantId, quantity, similarProducts, relatedProducts, relatedLoading,
    selectedVariant, stockLabel, isInStock,
    load, selectVariant, incrementQuantity, decrementQuantity, reset,
  })
}
