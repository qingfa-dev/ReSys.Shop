<script setup lang="ts">
import { onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useCatalogStore } from '../stores/catalogStore'
import { useProductListStore } from '../stores/productListStore'
import ProductCard from '../components/ProductCard.vue'

// Assign: Page title for browser tab and SEO
usePageTitle('Home')
const catalog = useCatalogStore()
const productList = useProductListStore()

onMounted(() => {
  // Call: Load taxonomy groups for category browsing grid
  catalog.loadTaxonomyGroups()
  // Call: Initialize product list for new arrivals section
  productList.init()
})
</script>

<template>
  <!-- Section: Hero — editorial full-width banner with headline and CTA -->
  <section class="min-h-[60vh] bg-neutral-100 flex items-center justify-center">
    <div class="text-center px-4 sm:px-6 lg:px-8 max-w-3xl py-16">
      <h1 class="text-4xl sm:text-5xl lg:text-6xl font-['Newsreader'] italic font-semibold text-neutral-900 leading-tight">
        Curated fashion, intelligently found
      </h1>
      <p class="mt-4 text-neutral-500 text-lg sm:text-xl">
        Discover pieces matched to your style through AI-powered curation.
      </p>
      <router-link to="/shop">
        <Button
          class="mt-8"
          severity="info"
          style="background: #14b8a6; border-color: #14b8a6"
        >
          Shop New Arrivals
        </Button>
      </router-link>
    </div>
  </section>

  <!-- Section: Featured Categories — taxonomy group grid -->
  <section class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-16">
    <p class="text-xs font-semibold uppercase tracking-widest text-neutral-400 mb-2">
      Shop by Category
    </p>
    <!-- Section: Category Grid — 4-column layout of taxonomy cards -->
    <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6 mt-6">
      <router-link
        v-for="group in catalog.taxonomyGroups"
        :key="group.taxonomy.id"
        to="/shop"
        class="group block"
      >
        <div class="aspect-[3/4] bg-neutral-100 rounded-lg flex flex-col items-center justify-center p-6 text-center transition-shadow hover:shadow-md">
          <h3 class="text-lg font-semibold text-neutral-800">{{ group.taxonomy.name }}</h3>
          <p class="mt-2 text-sm text-neutral-400">Browse collection</p>
        </div>
      </router-link>
    </div>
  </section>

  <!-- Section: Featured Products — new arrivals product grid -->
  <section class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-16">
    <p class="text-xs font-semibold uppercase tracking-widest text-neutral-400 mb-2">
      New Arrivals
    </p>
    <!-- Section: Product Grid — 4-column grid with skeleton loading state -->
    <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6 mt-6">
      <!-- Loading state: 8 skeleton placeholders while products are fetched -->
      <template v-if="productList.isInitialLoad && productList.loading">
        <div v-for="n in 8" :key="n" class="space-y-3">
          <Skeleton width="100%" height="auto" class="aspect-[3/4] rounded-lg" />
          <Skeleton width="60%" height="0.75rem" />
          <Skeleton width="80%" height="1rem" />
          <Skeleton width="40%" height="1rem" />
        </div>
      </template>
      <!-- Loaded state: render product cards for first 8 items -->
      <ProductCard
        v-for="product in productList.items.slice(0, 8)"
        :key="product.id"
        :product="product"
      />
    </div>
  </section>

  <!-- Section: Bottom CTA — waitlist signup strip -->
  <section class="bg-neutral-100 py-16">
    <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 text-center">
      <h2 class="text-2xl sm:text-3xl font-semibold text-neutral-900 font-['Newsreader']">
        Join the waitlist for exclusive drops
      </h2>
      <p class="mt-2 text-neutral-500">
        Be the first to know about limited releases and member-only collections.
      </p>
      <div class="mt-6 flex flex-col sm:flex-row items-center justify-center gap-3">
        <InputText
          type="email"
          placeholder="Enter your email"
          class="w-full sm:w-80"
        />
        <Button
          severity="info"
          style="background: #14b8a6; border-color: #14b8a6"
        >
          Subscribe
        </Button>
      </div>
    </div>
  </section>
</template>
