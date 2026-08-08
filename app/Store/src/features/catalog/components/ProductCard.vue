<script setup lang="ts">
import type { StoreProductListItemResponse } from '../types/product'
import { formatCurrency } from '@/shared/utils/currency'

const props = withDefaults(
  defineProps<{
    product: StoreProductListItemResponse
    aspectRatio?: string
    showSimilarity?: boolean
    similarityScore?: number
  }>(),
  {
    aspectRatio: 'aspect-[3/4]',
    showSimilarity: false,
    similarityScore: 0,
  },
)

// Derive: primary image URL from product master variant.
const imageUrl = props.product.masterVariant?.images?.[0]?.url ?? null

// Derive: brand label from department field.
const brand = props.product.department

// Derive: formatted price from master variant.
const price = props.product.masterVariant?.price
const formattedPrice = price != null ? formatCurrency(price) : null

// Derive: similarity percentage badge text.
const similarityPercent = `${(props.similarityScore * 100).toFixed(1)}%`
</script>

<template>
  <!-- Section: Product Card — linked card with image, info, and optional similarity badge -->
  <router-link
    :to="`/products/${product.slug}`"
    class="group block"
  >
    <div :class="['relative overflow-hidden rounded-lg bg-neutral-100', aspectRatio]">
      <!-- Fallback: product icon when no image available -->
      <div
        v-if="!imageUrl"
        class="flex h-full w-full items-center justify-center text-neutral-300"
      >
        <svg
          xmlns="http://www.w3.org/2000/svg"
          class="h-12 w-12"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="1.5"
            d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4"
          />
        </svg>
      </div>

      <!-- Product image -->
      <img
        v-if="imageUrl"
        :src="imageUrl"
        :alt="product.name"
        class="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
      />

      <!-- Section: Similarity Badge — visual match score overlay -->
      <span
        v-if="showSimilarity"
        class="absolute right-2 top-2 bg-teal-500/90 text-white text-xs rounded px-1.5 py-0.5"
      >
        {{ similarityPercent }}
      </span>
    </div>

    <!-- Section: Product Info — brand, name, and price -->
    <div class="mt-2 space-y-0.5">
      <p
        v-if="brand"
        class="text-xs font-medium uppercase tracking-wide text-neutral-400"
      >
        {{ brand }}
      </p>
      <p class="text-sm leading-snug text-neutral-800 line-clamp-2">
        {{ product.name }}
      </p>
      <p
        v-if="formattedPrice"
        class="text-sm font-semibold text-neutral-900"
        style="font-family: 'JetBrains Mono', monospace"
      >
        {{ formattedPrice }}
      </p>
    </div>
  </router-link>
</template>
