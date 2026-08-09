<script setup lang="ts">
import { computed, onMounted } from 'vue'
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

// Benefits: Static trust strip copy
const benefits = [
  { icon: 'pi pi-truck', title: 'Free Shipping', text: 'Free standard delivery on orders over $100.' },
  { icon: 'pi pi-shield', title: 'Easy Returns', text: '30-day hassle-free returns on every order.' },
  { icon: 'pi pi-headset', title: 'Expert Support', text: 'Fashion specialists on hand seven days a week.' },
]

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
      <div class="mx-auto grid max-w-7xl items-center gap-10 px-4 py-16 sm:px-6 lg:grid-cols-2 lg:px-8 lg:py-24">
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
    <DeferredContent class="mx-auto max-w-7xl px-4 py-16 sm:px-6 lg:px-8">
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
      <Carousel
        v-else-if="featuredProducts.length > 0"
        :value="featuredProducts"
        :numVisible="4"
        :numScroll="1"
        :responsiveOptions="[
          { breakpoint: '1024px', numVisible: 3, numScroll: 1 },
          { breakpoint: '768px', numVisible: 2, numScroll: 1 },
          { breakpoint: '560px', numVisible: 1, numScroll: 1 },
        ]"
      >
        <template #item="{ data }">
          <div class="px-1">
            <ProductGridCard :product="data" />
          </div>
        </template>
      </Carousel>
    </DeferredContent>

    <!-- Section: Categories — root taxon tag row deferred until scrolled into view -->
    <DeferredContent class="mx-auto max-w-7xl px-4 pb-16 sm:px-6 lg:px-8">
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

    <!-- Section: Benefits — trust strip deferred until scrolled into view -->
    <DeferredContent class="border-t border-surface-200 bg-surface-50">
      <div class="mx-auto grid max-w-7xl gap-8 px-4 py-12 sm:grid-cols-3 sm:px-6 lg:px-8">
        <div
          v-for="benefit in benefits"
          :key="benefit.title"
          class="flex flex-col items-center gap-3 text-center"
        >
          <i :class="benefit.icon" class="text-3xl text-brand" />
          <h3 class="text-sm font-semibold uppercase tracking-wide text-heading">
            {{ benefit.title }}
          </h3>
          <p class="max-w-xs text-sm text-muted">
            {{ benefit.text }}
          </p>
        </div>
      </div>
    </DeferredContent>
  </div>
</template>
