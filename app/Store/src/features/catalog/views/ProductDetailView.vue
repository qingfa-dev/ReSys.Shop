<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { formatCurrency } from '@/shared/utils/currency'
import { useProductDetailStore } from '../stores/productDetailStore'
import type { StoreProductVariantResponse, StoreVariantOptionValueResponse } from '../types/product'
import ProductCard from '../components/ProductCard.vue'

// Assign: Page title for browser tab and SEO
usePageTitle('Product')
const route = useRoute()
const detail = useProductDetailStore()

// Ref: DOM element for horizontal scroll of similar products
const similarScroll = ref<HTMLElement | null>(null)

// Derive: selected image index for gallery
const activeImageIndex = ref(0)

// Derive: gallery images from currently selected variant
const galleryImages = computed(() =>
  detail.selectedVariant?.images?.length
    ? detail.selectedVariant.images
    : detail.product?.masterVariant?.images ?? []
)

// Derive: active display price from selected variant
const displayPrice = computed(() => {
  const price = detail.selectedVariant?.price ?? detail.product?.masterVariant?.price
  return price != null ? formatCurrency(price) : null
})

// Derive: compare-at price for strikethrough display
const compareAtPrice = computed(() => {
  const prices = detail.selectedVariant?.prices ?? detail.product?.masterVariant?.prices
  const compareAt = prices?.[0]?.compareAtAmount
  return compareAt != null && compareAt > 0 ? formatCurrency(compareAt) : null
})

// Derive: option types grouped from all variants for selector UI
const optionGroups = computed(() => {
  if (!detail.product) return []
  const groups = new Map<string, { name: string; values: StoreVariantOptionValueResponse[] }>()
  for (const variant of detail.product.variants) {
    for (const ov of variant.optionValues) {
      if (!groups.has(ov.optionTypeId)) {
        groups.set(ov.optionTypeId, { name: ov.optionTypeName ?? ov.name, values: [] })
      }
      const group = groups.get(ov.optionTypeId)!
      if (!group.values.some(v => v.id === ov.id)) {
        group.values.push(ov)
      }
    }
  }
  return Array.from(groups.values())
})

// Derive: option values selected for current variant
const selectedOptionIds = computed(() => {
  if (!detail.selectedVariant) return new Set<string>()
  return new Set(detail.selectedVariant.optionValues.map(ov => ov.id))
})

// Derive: wishlist button state
const isWishlisted = ref(false)

// Assign: Update page title when product loads
watch(() => detail.product?.name, (name) => {
  if (name) usePageTitle(name)
}, { immediate: true })

// Assign: Reset image index when variant changes
watch(() => detail.selectedVariantId, () => {
  activeImageIndex.value = 0
})

// Action: Select a gallery image
function selectImage(index: number): void {
  activeImageIndex.value = index
}

// Action: Select an option value and find matching variant
function selectOptionValue(optionValueId: string): void {
  if (!detail.product || !detail.selectedVariant) return
  const currentOptionValues = detail.selectedVariant.optionValues
    .filter(ov => ov.id !== optionValueId)
    .map(ov => ov.id)

  const matchingVariant = detail.product.variants.find(v => {
    const vIds = v.optionValues.map(ov => ov.id)
    return vIds.length === currentOptionValues.length + 1
      && currentOptionValues.every(id => vIds.includes(id))
      && v.optionValues.some(ov => ov.id === optionValueId)
  })

  if (matchingVariant) {
    detail.selectVariant(matchingVariant.id)
  }
}

// Action: Scroll similar products by delta
function scrollSimilar(delta: number): void {
  if (similarScroll.value) {
    similarScroll.value.scrollBy({ left: delta, behavior: 'smooth' })
  }
}

// Action: Toggle wishlist
function toggleWishlist(): void {
  isWishlisted.value = !isWishlisted.value
}

// Action: Handle add to cart
async function handleAddToCart(): Promise<void> {
  await detail.addToCart()
}

// Watch: Reload product when slug changes
watch(() => route.params.slug, (slug) => {
  if (typeof slug === 'string') detail.load(slug)
}, { immediate: true })
</script>

<template>
  <!-- Section: Page Header — breadcrumb navigation -->
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Shop', to: '/shop' }, { label: 'Product' }]" />

    <!-- Section: Loading State — skeleton while fetching product detail -->
    <div v-if="detail.loading" class="mt-4 grid grid-cols-1 lg:grid-cols-2 gap-8">
      <Skeleton width="100%" height="32rem" class="rounded-lg" />
      <div class="space-y-4">
        <Skeleton width="40%" height="1rem" />
        <Skeleton width="70%" height="2rem" />
        <Skeleton width="30%" height="1.5rem" />
        <Skeleton width="100%" height="3rem" />
        <Skeleton width="100%" height="6rem" />
      </div>
    </div>

    <!-- Section: Error State — display API or network errors -->
    <Message severity="error" v-else-if="detail.error" class="mt-4">{{ detail.error }}</Message>

    <!-- Section: Product Detail — gallery, info, and variant selector -->
    <div v-else-if="detail.product" class="mt-4">
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 lg:gap-12">
        <!-- Section: Gallery — main image with thumbnail strip -->
        <div class="flex flex-col-reverse sm:flex-row gap-4">
          <!-- Thumbnail strip -->
          <div class="flex sm:flex-col gap-2 shrink-0 overflow-x-auto sm:overflow-y-auto sm:h-[32rem]">
            <button
              v-for="(img, idx) in galleryImages"
              :key="img.id"
              class="shrink-0 w-16 h-20 sm:w-20 sm:h-24 rounded-md overflow-hidden border-2 transition-colors"
              :class="activeImageIndex === idx ? 'border-teal-500' : 'border-transparent hover:border-neutral-300'"
              @click="selectImage(idx)"
            >
              <img
                :src="img.url"
                :alt="img.alt ?? detail.product!.name"
                class="w-full h-full object-cover"
              />
            </button>
          </div>

          <!-- Main image display -->
          <div class="relative flex-1 aspect-[3/4] rounded-lg overflow-hidden bg-neutral-100">
            <img
              v-if="galleryImages.length > 0"
              :src="galleryImages[activeImageIndex]?.url"
              :alt="galleryImages[activeImageIndex]?.alt ?? detail.product!.name"
              class="w-full h-full object-cover"
            />
            <!-- Fallback: product icon when no images -->
            <div
              v-else
              class="flex h-full w-full items-center justify-center text-neutral-300"
            >
              <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
              </svg>
            </div>
          </div>
        </div>

        <!-- Section: Product Info — name, price, variant selector, add-to-cart -->
        <div class="flex flex-col">
          <!-- Brand label -->
          <p
            v-if="detail.product.department"
            class="text-xs font-medium uppercase tracking-wide text-neutral-400"
          >
            {{ detail.product.department }}
          </p>

          <!-- Product name -->
          <h1 class="mt-1 text-2xl sm:text-3xl font-semibold text-neutral-900">
            {{ detail.product.name }}
          </h1>

          <!-- Price display -->
          <div class="mt-3 flex items-baseline gap-3">
            <span
              v-if="displayPrice"
              class="text-xl font-semibold text-neutral-900"
              style="font-family: 'JetBrains Mono', monospace"
            >
              {{ displayPrice }}
            </span>
            <span
              v-if="compareAtPrice"
              class="text-base text-neutral-400 line-through"
              style="font-family: 'JetBrains Mono', monospace"
            >
              {{ compareAtPrice }}
            </span>
          </div>

          <!-- Stock label -->
          <p
            v-if="detail.stockLabel"
            class="mt-2 text-sm font-medium"
            :class="detail.isInStock ? 'text-amber-600' : 'text-red-600'"
          >
            {{ detail.stockLabel }}
          </p>

          <!-- Section: Variant Selector — option type groups with value buttons -->
          <div v-if="optionGroups.length > 0" class="mt-6 space-y-4">
            <div v-for="group in optionGroups" :key="group.name">
              <p class="text-sm font-medium text-neutral-700 mb-2">{{ group.name }}</p>
              <div class="flex flex-wrap gap-2">
                <button
                  v-for="val in group.values"
                  :key="val.id"
                  class="px-3 py-1.5 text-sm rounded-md border transition-colors"
                  :class="selectedOptionIds.has(val.id)
                    ? 'border-teal-500 bg-teal-50 text-teal-700 font-medium'
                    : 'border-neutral-200 text-neutral-700 hover:border-neutral-400'"
                  @click="selectOptionValue(val.id)"
                >
                  {{ val.presentation ?? val.name }}
                </button>
              </div>
            </div>
          </div>

          <!-- Section: Quantity Selector — increment/decrement controls -->
          <div class="mt-6 flex items-center gap-3">
            <p class="text-sm font-medium text-neutral-700">Quantity</p>
            <div class="flex items-center border border-neutral-200 rounded-md">
              <button
                class="w-9 h-9 flex items-center justify-center text-neutral-500 hover:text-neutral-900 disabled:opacity-40"
                :disabled="detail.quantity <= 1"
                @click="detail.decrementQuantity()"
              >
                &minus;
              </button>
              <span
                class="w-10 text-center text-sm font-medium"
                style="font-family: 'JetBrains Mono', monospace"
              >
                {{ detail.quantity }}
              </span>
              <button
                class="w-9 h-9 flex items-center justify-center text-neutral-500 hover:text-neutral-900 disabled:opacity-40"
                :disabled="detail.quantity >= 99"
                @click="detail.incrementQuantity()"
              >
                +
              </button>
            </div>
          </div>

          <!-- Section: Add to Cart + Wishlist — primary and secondary actions -->
          <div class="mt-6 flex items-center gap-3">
            <Button
              class="flex-1"
              severity="info"
              :disabled="!detail.isInStock"
              style="background: #14b8a6; border-color: #14b8a6"
              @click="handleAddToCart()"
            >
              {{ detail.isInStock ? 'Add to Cart' : 'Out of Stock' }}
            </Button>
            <Button
              :icon="isWishlisted ? 'pi pi-heart-fill' : 'pi pi-heart'"
              severity="secondary"
              aria-label="Add to wishlist"
              @click="toggleWishlist()"
            />
          </div>

          <!-- Section: Trust Signals — shipping, returns, security -->
          <div class="mt-8 border-t border-neutral-100 pt-6 space-y-3">
            <div class="flex items-center gap-3 text-sm text-neutral-500">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 shrink-0 text-neutral-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M8.25 18.75a1.5 1.5 0 01-3 0m3 0a1.5 1.5 0 00-3 0m3 0h6m-9 0H3.375a1.125 1.125 0 01-1.125-1.125V14.25m17.25 4.5a1.5 1.5 0 01-3 0m3 0a1.5 1.5 0 00-3 0m3 0h1.125c.621 0 1.129-.504 1.09-1.124a17.902 17.902 0 00-3.213-9.193 2.056 2.056 0 00-1.58-.86H14.25M16.5 18.75h-2.25m0-11.177v-.958c0-.568-.422-1.048-.987-1.106a48.554 48.554 0 00-10.026 0 1.106 1.106 0 00-.987 1.106v7.635m12-6.677v6.677m0 4.5v-4.5m0 0h-12" />
              </svg>
              Free shipping on orders over $50
            </div>
            <div class="flex items-center gap-3 text-sm text-neutral-500">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 shrink-0 text-neutral-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M16.023 9.348h4.992v-.001M2.985 19.644v-4.992m0 0h4.992m-4.993 0l3.181 3.183a8.25 8.25 0 0013.803-3.7M4.031 9.865a8.25 8.25 0 0113.803-3.7l3.181 3.182m0-4.991v4.99" />
              </svg>
              30-day free returns
            </div>
            <div class="flex items-center gap-3 text-sm text-neutral-500">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 shrink-0 text-neutral-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75m-3-7.036A11.959 11.959 0 013.598 6 11.99 11.99 0 003 9.749c0 5.592 3.824 10.29 9 11.623 5.176-1.332 9-6.03 9-11.622 0-1.31-.21-2.571-.598-3.751h-.152c-3.196 0-6.1-1.248-8.25-3.285z" />
              </svg>
              Secure checkout
            </div>
          </div>
        </div>
      </div>

      <!-- Section: Tabs — description, specifications, care instructions -->
      <div class="mt-12">
        <TabView>
          <TabList>
            <Tab header="Description" value="0" />
            <Tab header="Specifications" value="1" />
            <Tab header="Care" value="2" />
          </TabList>
          <TabPanels>
            <TabPanel value="0">
              <div class="prose prose-sm max-w-none text-neutral-700">
                <p v-if="detail.product.description">{{ detail.product.description }}</p>
                <p v-else class="text-neutral-400 italic">No description available.</p>
              </div>
            </TabPanel>
            <TabPanel value="1">
              <div class="space-y-3 text-sm">
                <div v-if="detail.product.styleCode" class="flex">
                  <span class="w-36 shrink-0 font-medium text-neutral-500">Style Code</span>
                  <span class="text-neutral-900">{{ detail.product.styleCode }}</span>
                </div>
                <div v-if="detail.product.seasonName" class="flex">
                  <span class="w-36 shrink-0 font-medium text-neutral-500">Season</span>
                  <span class="text-neutral-900">{{ detail.product.seasonName }}</span>
                </div>
                <div v-if="detail.product.materialComposition" class="flex">
                  <span class="w-36 shrink-0 font-medium text-neutral-500">Composition</span>
                  <span class="text-neutral-900">{{ detail.product.materialComposition }}</span>
                </div>
                <div v-if="detail.product.fitNotes" class="flex">
                  <span class="w-36 shrink-0 font-medium text-neutral-500">Fit</span>
                  <span class="text-neutral-900">{{ detail.product.fitNotes }}</span>
                </div>
                <div v-if="detail.selectedVariant?.sku" class="flex">
                  <span class="w-36 shrink-0 font-medium text-neutral-500">SKU</span>
                  <span class="text-neutral-900" style="font-family: 'JetBrains Mono', monospace">
                    {{ detail.selectedVariant.sku }}
                  </span>
                </div>
                <p v-if="!detail.product.styleCode && !detail.product.seasonName && !detail.product.materialComposition && !detail.product.fitNotes" class="text-neutral-400 italic">
                  No specifications available.
                </p>
              </div>
            </TabPanel>
            <TabPanel value="2">
              <div class="text-sm text-neutral-700">
                <p v-if="detail.product.careInstructions">{{ detail.product.careInstructions }}</p>
                <p v-else class="text-neutral-400 italic">No care instructions available.</p>
              </div>
            </TabPanel>
          </TabPanels>
        </TabView>
      </div>

      <!-- Section: Visually Similar — horizontal scroll carousel of AI-matched products -->
      <section v-if="detail.similarProducts.length > 0" class="mt-12">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-lg font-semibold text-neutral-900">Visually Similar</h2>
          <div class="flex gap-2">
            <button
              class="p-1.5 rounded-md border border-neutral-200 text-neutral-500 hover:text-neutral-900 hover:border-neutral-400"
              @click="scrollSimilar(-300)"
            >
              <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5L8.25 12l7.5-7.5" />
              </svg>
            </button>
            <button
              class="p-1.5 rounded-md border border-neutral-200 text-neutral-500 hover:text-neutral-900 hover:border-neutral-400"
              @click="scrollSimilar(300)"
            >
              <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M8.25 4.5l7.5 7.5-7.5 7.5" />
              </svg>
            </button>
          </div>
        </div>
        <div
          ref="similarScroll"
          class="flex gap-4 overflow-x-auto pb-4 snap-x snap-mandatory scrollbar-hide"
        >
          <div
            v-for="item in detail.similarProducts"
            :key="item.id"
            class="shrink-0 w-48 snap-start"
          >
            <ProductCard
              :product="item"
              :show-similarity="true"
              :similarity-score="item.similarityScore"
            />
          </div>
        </div>
      </section>

      <!-- Section: Related Products — grid of recommended items -->
      <section v-if="detail.relatedProducts.length > 0" class="mt-12 mb-8">
        <h2 class="text-lg font-semibold text-neutral-900 mb-4">You May Also Like</h2>
        <!-- Loading: skeleton grid -->
        <div v-if="detail.relatedLoading" class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
          <div v-for="i in 4" :key="i" class="space-y-3 animate-pulse">
            <div class="aspect-[3/4] rounded-lg bg-neutral-100" />
            <div class="h-3 w-16 rounded bg-neutral-100" />
            <div class="h-4 w-3/4 rounded bg-neutral-100" />
          </div>
        </div>
        <!-- Loaded: product grid -->
        <div v-else class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
          <ProductCard
            v-for="item in detail.relatedProducts"
            :key="item.id"
            :product="item"
          />
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped>
.scrollbar-hide::-webkit-scrollbar { display: none; }
.scrollbar-hide { -ms-overflow-style: none; scrollbar-width: none; }
</style>
