<script setup lang="ts">
import { computed, onMounted } from 'vue'
import ChevronLeft from '@primeicons/vue/chevron-left'
import ChevronRight from '@primeicons/vue/chevron-right'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useTaxonomy } from '../composables/useTaxonomy'
import { useProducts } from '../composables/useProducts'
import ProductGridCard from '../components/ProductGridCard.vue'

// Title: Browser tab title for the storefront home
usePageTitle('Home')

const taxonomy = useTaxonomy()
const productList = useProducts()

// Featured: First page of products doubles as the home rail (store has no featured getter)
const featuredProducts = computed(() => productList.items.slice(0, 8))
const featuredLoading = computed(() => productList.isInitialLoad && productList.loading)

// Categories: De-duplicated root taxons across taxonomy groups for the tag row
const rootTaxons = computed(() => {
  const seen = new Set<string>()
  const roots: { id: string; name: string }[] = []
  for (const group of taxonomy.taxonomyGroups) {
    for (const node of group.tree) {
      if (node.depth === 0 && !seen.has(node.id)) {
        seen.add(node.id)
        roots.push({ id: node.id, name: node.presentation ?? node.name })
      }
    }
  }
  return roots
})

onMounted(() => {
  // Load: Taxonomy groups for the category row — composable guards duplicate fetches
  void taxonomy.loadTaxonomyGroups()
  // Load: Featured rail only on first visit — products stay cached afterwards
  if (productList.isInitialLoad) void productList.fetch()
})
</script>

<template>
  <div>
    <!-- Section: Hero — animated headline, CTA and gradient banner -->
    <section
      v-animateonscroll.once="{ enterClass: 'animate-fadein' }"
      class="bg-gradient-to-br from-highlight via-surface-0 to-brand-subtle"
    >
      <div class="mx-auto grid max-w-screen-2xl items-center gap-10 px-4 py-16 sm:px-6 lg:grid-cols-2 lg:px-8 lg:py-24">
        <div>
          <h1 class="text-4xl font-semibold leading-tight tracking-tight text-heading sm:text-5xl lg:text-6xl">
            Curated fashion, intelligently found
          </h1>
          <p class="mt-4 max-w-xl text-lg text-muted">
            Discover pieces matched to your style through AI-powered curation and visual search.
          </p>
          <Button
            as="router-link"
            to="/shop"
            label="Shop New Arrivals"
            size="large"
            class="mt-8"
          />
        </div>
        <!-- Banner: Gradient panel stands in for the seasonal campaign image -->
        <!-- Decorative: dark gradient panel (intentional primary scale use, not a token violation) -->
        <div class="hidden min-h-80 items-center justify-center rounded-3xl bg-gradient-to-br from-primary-500 via-primary-700 to-primary-950 shadow-lg lg:flex">
          <i class="pi pi-sparkles text-7xl text-on-brand/80" />
        </div>
      </div>
    </section>

    <!-- Section: Featured — carousel rail deferred until scrolled into view -->
    <DeferredContent class="mx-auto max-w-screen-2xl px-4 py-16 sm:px-6 lg:px-8">
      <div class="mb-8 flex items-baseline justify-between gap-4">
        <h2 class="text-2xl font-semibold tracking-tight text-heading">Featured</h2>
        <Button as="router-link" to="/shop" label="View all" variant="text" />
      </div>
      <!-- Loading: Skeleton placeholders while the first page fetches -->
      <div v-if="featuredLoading" class="grid grid-cols-2 gap-6 lg:grid-cols-4">
        <div v-for="n in 4" :key="n" class="space-y-3">
          <Skeleton class="aspect-square w-full rounded-xl" />
          <Skeleton width="70%" height="1rem" />
          <Skeleton width="40%" height="1rem" />
        </div>
      </div>
      <!-- Loaded: Responsive carousel of product cards -->
      <Carousel v-else-if="featuredProducts.length > 0" align="center" autoSize :spacing="24" class="-mx-2">
        <CarouselContent>
          <CarouselItem
            v-for="product in featuredProducts"
            :key="product.id"
            class="basis-full! sm:basis-1/2! lg:basis-1/3! xl:basis-1/4! 2xl:basis-1/5!"
          >
            <div class="p-2">
              <ProductGridCard :product="product" />
            </div>
          </CarouselItem>
        </CarouselContent>
        <div class="mt-4 flex items-center justify-between gap-2">
          <CarouselIndicators />
          <div class="flex items-center justify-end gap-2">
          <CarouselPrev class="flex h-9 w-9 cursor-pointer items-center justify-center rounded-full border border-surface bg-surface-0 text-muted transition-opacity hover:opacity-75">
            <ChevronLeft class="text-lg" />
          </CarouselPrev>
          <CarouselNext class="flex h-9 w-9 cursor-pointer items-center justify-center rounded-full border border-surface bg-surface-0 text-muted transition-opacity hover:opacity-75">
            <ChevronRight class="text-lg" />
          </CarouselNext>
          </div>
        </div>
      </Carousel>
    </DeferredContent>

    <!-- Section: Categories — root taxon tag row deferred until scrolled into view -->
    <DeferredContent class="mx-auto max-w-screen-2xl px-4 pb-16 sm:px-6 lg:px-8">
      <h2 class="mb-6 text-2xl font-semibold tracking-tight text-heading">Shop by Category</h2>
      <div v-if="rootTaxons.length > 0" class="flex flex-wrap gap-2">
        <Button
          v-for="taxon in rootTaxons"
          :key="taxon.id"
          :label="taxon.name"
          variant="outlined"
          rounded
          as="router-link"
          :to="{ path: '/shop', query: { taxon: taxon.id } }"
        />
      </div>
    </DeferredContent>
  </div>
</template>
