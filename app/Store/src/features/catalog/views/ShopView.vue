<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useCatalogStore } from '../stores/catalogStore'
import { useProductListStore } from '../stores/productListStore'
import ProductCard from '../components/ProductCard.vue'

usePageTitle('Shop')
const catalog = useCatalogStore()
const productList = useProductListStore()
const mobileFiltersOpen = ref(false)

onMounted(() => {
  catalog.loadTaxonomyGroups()
  catalog.loadOptionTypes()
  productList.init()
})

function onTaxonClick(id: string): void {
  catalog.toggleTaxon(id)
}

function onOptionValueClick(id: string): void {
  catalog.toggleOptionValue(id)
}

function onClearFilters(): void {
  catalog.clearFilters()
}
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Shop' }]" />

    <div class="flex gap-8 mt-4">
      <!-- Section: Filter Sidebar — taxonomy tree, price range, option values -->
      <aside class="hidden lg:block w-64 shrink-0">
        <div class="sticky top-20 space-y-6">
          <!-- Active Filters Header -->
          <div v-if="catalog.activeFilterCount > 0" class="flex items-center justify-between">
            <span class="text-sm font-medium text-neutral-900">{{ catalog.activeFilterCount }} active</span>
            <button class="text-xs text-neutral-500 hover:text-neutral-900" @click="onClearFilters()">Clear All</button>
          </div>

          <!-- Taxonomy Tree -->
          <div v-for="group in catalog.taxonomyGroups" :key="group.taxonomy.id">
            <h3 class="text-xs font-semibold text-neutral-900 uppercase tracking-wide mb-2">{{ group.taxonomy.name }}</h3>
            <div v-for="taxon in group.tree" :key="taxon.id" class="ml-2">
              <label class="flex items-center gap-2 py-1 cursor-pointer text-sm text-neutral-700 hover:text-neutral-900">
                <input
                  type="checkbox"
                  :checked="catalog.selectedTaxonIds.includes(taxon.id)"
                  class="rounded border-neutral-300"
                  @change="onTaxonClick(taxon.id)"
                />
                <span :class="{ 'font-semibold text-neutral-900': catalog.selectedTaxonIds.includes(taxon.id) }">
                  {{ taxon.name }}
                </span>
              </label>
              <!-- Nested children -->
              <div v-for="child in taxon.children" :key="child.id" class="ml-4">
                <label class="flex items-center gap-2 py-1 cursor-pointer text-sm text-neutral-700 hover:text-neutral-900">
                  <input
                    type="checkbox"
                    :checked="catalog.selectedTaxonIds.includes(child.id)"
                    class="rounded border-neutral-300"
                    @change="onTaxonClick(child.id)"
                  />
                  <span :class="{ 'font-semibold text-neutral-900': catalog.selectedTaxonIds.includes(child.id) }">
                    {{ child.name }}
                  </span>
                </label>
              </div>
            </div>
          </div>

          <!-- Price Range -->
          <div>
            <h3 class="text-xs font-semibold text-neutral-900 uppercase tracking-wide mb-2">Price</h3>
            <div class="flex items-center gap-2">
              <InputText
                type="number"
                placeholder="Min"
                :model-value="catalog.minPrice ?? ''"
                class="w-full text-sm"
                @update:model-value="(v: any) => catalog.setPriceRange(v ? Number(v) : null, catalog.maxPrice)"
              />
              <span class="text-neutral-300">&mdash;</span>
              <InputText
                type="number"
                placeholder="Max"
                :model-value="catalog.maxPrice ?? ''"
                class="w-full text-sm"
                @update:model-value="(v: any) => catalog.setPriceRange(catalog.minPrice, v ? Number(v) : null)"
              />
            </div>
          </div>

          <!-- Option Values -->
          <div v-for="opt in catalog.optionTypes" :key="opt.id">
            <h3 class="text-xs font-semibold text-neutral-900 uppercase tracking-wide mb-2">{{ opt.presentation || opt.name }}</h3>
            <div class="space-y-1">
              <label
                v-for="val in opt.values"
                :key="val.id"
                class="flex items-center gap-2 py-1 cursor-pointer text-sm text-neutral-700 hover:text-neutral-900"
              >
                <input
                  type="checkbox"
                  :checked="catalog.selectedOptionValueIds.includes(val.id)"
                  class="rounded border-neutral-300"
                  @change="onOptionValueClick(val.id)"
                />
                <span>{{ val.name }}</span>
              </label>
            </div>
          </div>
        </div>
      </aside>

      <!-- Section: Content Area — sort bar + product grid + pagination -->
      <div class="flex-1 min-w-0">
        <!-- Placeholder for Task 4 -->
        <div class="text-center py-24 text-neutral-500">
          Product grid content will be implemented in the next task.
        </div>
      </div>
    </div>
  </div>
</template>
