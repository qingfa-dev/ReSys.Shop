<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { getProductBySlug, getSimilarProducts } from '../services/productApi'
import { useCartStore } from '@/features/ordering/stores/cartStore'
import { useNotify } from '@/shared/composables/useNotify'
import ProductGallery from '../components/ProductGallery.vue'
import ProductOptions from '../components/ProductOptions.vue'
import SimilarProductsRow from '../components/SimilarProductsRow.vue'
import type { StoreProductDetailResponse, StoreProductListItemResponse } from '../types/product'

const route = useRoute()
const cart = useCartStore()
const notify = useNotify()
const product = ref<StoreProductDetailResponse | null>(null)
const similar = ref<StoreProductListItemResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const adding = ref(false)
const selectedVariantId = ref<string | null>(null)
const quantity = ref(1)

// Trigger: Load product when slug changes
async function loadProduct(slug: string): Promise<void> {
  loading.value = true
  error.value = null
  similar.value = []
  quantity.value = 1
  const result = await getProductBySlug(slug)
  if (result.isSuccess) {
    product.value = result.value
    selectedVariantId.value = result.value.masterVariant?.id ?? null
    const simResult = await getSimilarProducts(result.value.id)
    if (simResult.isSuccess) similar.value = simResult.items
  } else {
    error.value = result.message ?? 'Product not found'
  }
  loading.value = false
}

// Trigger: Add the selected variant to the cart.
async function addToCart(): Promise<void> {
  if (!product.value || !selectedVariantId.value) {
    notify.error('Add to cart failed', 'Select a variant first')
    return
  }
  adding.value = true
  const ok = await cart.addItem(selectedVariantId.value, quantity.value)
  adding.value = false
  if (ok) notify.success('Added to cart', product.value.name)
  else notify.error('Add to cart failed', cart.error ?? undefined)
}

watch(() => route.params.slug, (slug) => {
  if (typeof slug === 'string') loadProduct(slug)
}, { immediate: true })
</script>
<template>
  <!-- Section: Product Detail Page -->
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Section: Error State -->
    <div v-if="error" class="text-center py-16">
      <i class="pi pi-exclamation-circle text-4xl text-gray-300 mb-4" />
      <h2 class="text-xl font-semibold text-gray-900">{{ error }}</h2>
      <router-link to="/shop" class="text-primary hover:underline mt-2 inline-block">Browse products</router-link>
    </div>

    <!-- Section: Loading State -->
    <div v-else-if="loading" class="animate-pulse space-y-8">
      <div class="flex flex-col md:flex-row gap-8">
        <div class="w-full md:w-1/2 aspect-square bg-gray-200 rounded-xl" />
        <div class="w-full md:w-1/2 space-y-4">
          <div class="h-8 bg-gray-200 rounded w-3/4" />
          <div class="h-6 bg-gray-200 rounded w-1/4" />
          <div class="h-4 bg-gray-200 rounded w-full" />
          <div class="h-12 bg-gray-200 rounded w-1/3" />
        </div>
      </div>
    </div>

    <!-- Section: Product Content -->
    <template v-else-if="product">
      <div class="flex flex-col md:flex-row gap-8">
        <!-- Section: Image Gallery -->
        <div class="w-full md:w-1/2">
          <ProductGallery :images="product.images" :alt="product.name" />
        </div>

        <!-- Section: Product Info -->
        <div class="w-full md:w-1/2 space-y-6">
          <!-- Section: Breadcrumb -->
          <nav class="flex items-center gap-2 text-sm text-gray-500">
            <router-link to="/" class="hover:text-gray-900">Home</router-link>
            <i class="pi pi-chevron-right text-xs" />
            <router-link to="/shop" class="hover:text-gray-900">Shop</router-link>
            <i class="pi pi-chevron-right text-xs" />
            <span class="text-gray-900">{{ product.name }}</span>
          </nav>

          <h1 class="text-2xl font-bold text-gray-900">{{ product.name }}</h1>

          <!-- Section: Price -->
          <p v-if="product.minPrice" class="text-3xl font-bold text-gray-900">
            {{ new Intl.NumberFormat('vi-VN', { style: 'currency', currency: product.currency ?? 'VND' }).format(product.minPrice) }}
          </p>

          <!-- Section: Fashion Metadata -->
          <div v-if="product.styleCode || product.materialComposition" class="flex flex-wrap gap-3 text-sm text-gray-500">
            <span v-if="product.styleCode" class="bg-gray-100 px-2 py-1 rounded">Style: {{ product.styleCode }}</span>
            <span v-if="product.seasonName" class="bg-gray-100 px-2 py-1 rounded">{{ product.seasonName }}</span>
            <span v-if="product.materialComposition" class="bg-gray-100 px-2 py-1 rounded">{{ product.materialComposition }}</span>
            <span v-if="product.department" class="bg-gray-100 px-2 py-1 rounded">{{ product.department }}</span>
            <span v-if="product.genderTarget" class="bg-gray-100 px-2 py-1 rounded">{{ product.genderTarget }}</span>
          </div>

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

          <!-- Section: Expandable Details -->
          <Accordion multiple class="space-y-2">
            <AccordionPanel v-if="product.description" value="description">
              <AccordionHeader>Description</AccordionHeader>
              <AccordionContent>
                <p class="text-gray-600">{{ product.description }}</p>
              </AccordionContent>
            </AccordionPanel>
            <AccordionPanel v-if="product.materialComposition" value="material">
              <AccordionHeader>Material &amp; Composition</AccordionHeader>
              <AccordionContent>
                <p class="text-gray-600">{{ product.materialComposition }}</p>
              </AccordionContent>
            </AccordionPanel>
            <AccordionPanel v-if="product.careInstructions" value="care">
              <AccordionHeader>Care Instructions</AccordionHeader>
              <AccordionContent>
                <p class="text-gray-600">{{ product.careInstructions }}</p>
              </AccordionContent>
            </AccordionPanel>
            <AccordionPanel v-if="product.fitNotes" value="fit">
              <AccordionHeader>Size &amp; Fit</AccordionHeader>
              <AccordionContent>
                <p class="text-gray-600">{{ product.fitNotes }}</p>
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
    </template>
  </div>
</template>
