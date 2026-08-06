<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import { getProductBySlug, getSimilarProducts, getRelatedProducts } from '../services/productApi'
import { checkAvailability } from '@/features/inventory/services/availabilityApi'
import { useCartStore } from '@/features/ordering/stores/cartStore'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import ProductGallery from '../components/ProductGallery.vue'
import ProductOptions from '../components/ProductOptions.vue'
import SimilarProductsRow from '../components/SimilarProductsRow.vue'
import RelatedProductsRow from '../components/RelatedProductsRow.vue'
import SizeGuideModal from '../components/SizeGuideModal.vue'
import ProductDetailsInfo from '../components/ProductDetailsInfo.vue'
import { useRecentlyViewed } from '@/shared/composables/useRecentlyViewed'
import type { StoreProductDetailResponse, StoreProductListItemResponse } from '../types/product'
import type { AvailabilityEntry } from '@/features/inventory/types/availability'

const route = useRoute()
const cart = useCartStore()
const notify = useNotify()
const { handleError } = useApiErrorHandler()
const product = ref<StoreProductDetailResponse | null>(null)
const similar = ref<StoreProductListItemResponse[]>([])
const related = ref<StoreProductListItemResponse[]>([])
const relatedLoading = ref(true)
const loading = ref(true)
const error = ref<string | null>(null)
const adding = ref(false)
const selectedVariantId = ref<string | null>(null)
const quantity = ref(1)
const availability = ref<AvailabilityEntry[]>([])

// Map: Breadcrumb trail for the product detail page
const breadcrumbItems = computed(() => [
  { label: 'Home', to: '/' },
  { label: 'Shop', to: '/shop' },
  { label: product.value?.name ?? 'Product' },
])

// Derive: The currently selected variant object from the list
const selectedVariant = computed(() =>
  product.value?.variants.find((v) => v.id === selectedVariantId.value) ?? null,
)

// Derive: Stock label for the selected variant
const stockLabel = computed(() => {
  const stock = selectedVariant.value?.stock
  if (!stock) return null
  if (stock.availableQuantity > 5) return null
  if (stock.availableQuantity > 0) return `Only ${stock.availableQuantity} left!`
  if (stock.backorderable) return 'Available for backorder'
  return 'Out of stock'
})

// Derive: Tailwind text colour class for the stock badge
const stockColor = computed(() => {
  const stock = selectedVariant.value?.stock
  if (!stock) return null
  if (stock.availableQuantity > 5) return 'text-emerald-600'
  if (stock.availableQuantity > 0) return 'text-amber-600'
  if (stock.backorderable) return 'text-blue-600'
  return 'text-red-600'
})

// Trigger: Load product when slug changes
async function loadProduct(slug: string): Promise<void> {
  loading.value = true
  error.value = null
  similar.value = []
  related.value = []
  quantity.value = 1
  const result = await getProductBySlug(slug)
  if (result.isSuccess) {
    product.value = result.value
    // Track: Record the product in the recently-viewed history.
    useRecentlyViewed().add({
      productId: result.value.id,
      productName: result.value.name,
      slug: result.value.slug,
      thumbnailUrl: result.value.masterVariant?.images?.[0]?.url ?? null,
      minPrice: result.value.masterVariant?.price ?? null,
      viewedAt: Date.now(),
    })
    selectedVariantId.value = result.value.masterVariant?.id ?? null
    const simResult = await getSimilarProducts(result.value.id)
    if (simResult.isSuccess) similar.value = simResult.items
    relatedLoading.value = true
    const relResult = await getRelatedProducts(result.value.id, { pageNumber: 1, pageSize: 12 })
    if (relResult.isSuccess) related.value = relResult.items
    relatedLoading.value = false
  } else {
    error.value = result.message ?? 'Product not found'
  }
  loading.value = false
}

// Trigger: Add the selected variant to the cart.
async function addToCart(): Promise<void> {
  if (!product.value || !selectedVariantId.value) {
    handleError(new Error('Select a variant first'))
    return
  }
  adding.value = true
  try {
    const ok = await cart.addItem(selectedVariantId.value, quantity.value)
    if (ok) notify.success('Added to cart', product.value.name)
    else handleError(new Error(cart.error ?? 'Add to cart failed'))
  } catch {
    // A thrown rejection (network / non-Result 5xx) would otherwise be an
    // unhandled rejection — surface it as a toast.
    handleError(new Error(cart.error ?? 'Add to cart failed'))
  } finally {
    // Ensure the button never stays loading on a thrown rejection.
    adding.value = false
  }
}

// Trigger: Quick-add a related product variant to the cart.
async function quickAdd(variantId: string): Promise<void> {
  if (!variantId) {
    notify.warn('Unavailable', 'This product has no purchasable variant')
    return
  }
  try {
    const ok = await cart.addItem(variantId, 1)
    if (ok) notify.success('Added to cart')
    else handleError(new Error(cart.error ?? 'Could not add item'))
  } catch {
    handleError(new Error(cart.error ?? 'Could not add item'))
  }
}

watch(() => route.params.slug, (slug) => {
  if (typeof slug === 'string') loadProduct(slug)
}, { immediate: true })

// Trigger: Fetch per-location availability when the selected variant changes.
watch(selectedVariantId, async (variantId) => {
  availability.value = []
  if (!variantId) return
  const result = await checkAvailability(variantId)
  if (result.isSuccess) availability.value = result.items
})
</script>
<template>
  <!-- Section: Product Detail Page -->
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Section: Error State -->
    <div v-if="error" class="text-center py-16">
      <i class="pi pi-exclamation-circle text-4xl text-stone-300 mb-4" />
      <h2 class="text-xl font-semibold text-stone-900">{{ error }}</h2>
      <router-link to="/shop" class="text-primary hover:underline mt-2 inline-block">Browse products</router-link>
    </div>

    <!-- Section: Loading State -->
    <div v-else-if="loading" class="animate-pulse space-y-8">
      <div class="flex flex-col md:flex-row gap-8">
        <div class="w-full md:w-1/2 aspect-square bg-stone-200 rounded-xl" />
        <div class="w-full md:w-1/2 space-y-4">
          <div class="h-8 bg-stone-200 rounded w-3/4" />
          <div class="h-6 bg-stone-200 rounded w-1/4" />
          <div class="h-4 bg-stone-200 rounded w-full" />
          <div class="h-12 bg-stone-200 rounded w-1/3" />
        </div>
      </div>
    </div>

    <!-- Section: Product Content -->
    <template v-else-if="product">
      <div class="flex flex-col md:flex-row gap-8">
        <!-- Section: Image Gallery -->
        <div class="w-full md:w-1/2">
          <ProductGallery :images="product.masterVariant?.images ?? []" :alt="product.name" />
        </div>

        <!-- Section: Product Info -->
        <div class="w-full md:w-1/2 space-y-6">
          <!-- Section: Breadcrumb -->
          <Breadcrumb :model="breadcrumbItems" class="mb-4" />

          <h1 class="text-2xl font-bold text-stone-900">{{ product.name }}</h1>

          <!-- Section: Size Guide -->
          <SizeGuideModal v-if="product.variants.length > 0" :variants="product.variants" :product-name="product.name" />

          <!-- Section: Price -->
          <p v-if="product.masterVariant?.price" class="text-3xl font-bold text-stone-900">
            {{ new Intl.NumberFormat('vi-VN', { style: 'currency', currency: product.masterVariant?.currency ?? 'VND' }).format(product.masterVariant.price) }}
          </p>

          <!-- Section: Stock Status -->
          <p v-if="stockLabel" :class="stockColor" class="text-sm font-medium">
            {{ stockLabel }}
          </p>

          <!-- Section: Per-Location Stock -->
          <div v-if="availability.length > 0" class="mt-2 space-y-1">
            <p v-for="loc in availability" :key="loc.stockLocationId" class="text-xs text-stone-500">
              {{ loc.locationName }}: {{ loc.availableCount }} in stock
            </p>
          </div>

          <!-- Section: Product Details Info -->
          <ProductDetailsInfo :product="product" />

          <!-- Section: Variant Options -->
          <ProductOptions
            v-if="product.variants.length > 0"
            :variants="product.variants"
            :model-value="selectedVariantId"
            @update:model-value="(id: string) => selectedVariantId = id"
          />

          <!-- Section: Quantity + Add to Cart -->
          <div class="flex items-center gap-4">
            <InputNumber v-model="quantity" :min="1" :max="99" class="w-24" />
            <Button label="Add to Cart" icon="pi pi-shopping-cart" class="flex-1" :loading="adding" @click="addToCart" />
          </div>

          <!-- Section: Description -->
          <Accordion v-if="product.description" class="space-y-2">
            <AccordionPanel value="description">
              <AccordionHeader>Description</AccordionHeader>
              <AccordionContent>
                <p class="text-stone-600">{{ product.description }}</p>
              </AccordionContent>
            </AccordionPanel>
          </Accordion>
        </div>
      </div>

      <!-- Section: Similar Products -->
      <SimilarProductsRow
        v-if="similar.length > 0"
        :products="similar"
        class="mt-16"
      />

      <!-- Section: Related Products -->
      <RelatedProductsRow
        v-if="related.length > 0"
        :products="related"
        class="mt-16"
        @add-to-cart="quickAdd"
      />
    </template>
  </div>
</template>
