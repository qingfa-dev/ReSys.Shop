<script setup lang="ts">
import { ref, computed } from 'vue'
import Button from 'primevue/button'
import FilterPriceRange from '../filters/FilterPriceRange.vue'
import FilterCategoryTree from '../filters/FilterCategoryTree.vue'
import FilterBrandSelect from '../filters/FilterBrandSelect.vue'
import FilterSizeSelect from '../filters/FilterSizeSelect.vue'
import FilterColorSelect from '../filters/FilterColorSelect.vue'

const DEFAULT_PRICE_MIN = 0
const DEFAULT_PRICE_MAX = 1000

interface Category {
  id: string
  name: string
  slug: string
  children?: Category[]
}

interface Color {
  id: string
  name: string
  hex: string
}

interface Size {
  id: string
  name: string
}

interface Brand {
  name: string
  slug: string
}

interface Props {
  categories?: Category[]
  colors?: Color[]
  sizes?: Size[]
  brands?: Brand[]
}

const props = withDefaults(defineProps<Props>(), {
  categories: () => [],
  colors: () => [],
  sizes: () => [],
  brands: () => [],
})

const emit = defineEmits<{
  (e: 'filterChange', filters: FilterState): void
  (e: 'clear'): void
}>()

interface FilterState {
  category: string | null
  priceMin: number | null
  priceMax: number | null
  sizes: string[]
  colors: string[]
  brands: string[]
}

const selectedCategory = ref<string | null>(null)
const priceRange = ref({ min: DEFAULT_PRICE_MIN, max: DEFAULT_PRICE_MAX })
const selectedSizeIds = ref<string[]>([])
const selectedColorIds = ref<string[]>([])
const selectedBrandSlugs = ref<string[]>([])

const hasActiveFilters = computed(() => {
  return selectedCategory.value !== null ||
    priceRange.value.min > DEFAULT_PRICE_MIN ||
    priceRange.value.max < DEFAULT_PRICE_MAX ||
    selectedSizeIds.value.length > 0 ||
    selectedColorIds.value.length > 0 ||
    selectedBrandSlugs.value.length > 0
})

function handlePriceRangeChange(range: { min: number; max: number }) {
  priceRange.value = range
  emitFilters()
}

function handleCategorySelect(slug: string | null) {
  selectedCategory.value = slug
  emitFilters()
}

function handleSizeChange(ids: string[]) {
  selectedSizeIds.value = ids
  emitFilters()
}

function handleColorChange(ids: string[]) {
  selectedColorIds.value = ids
  emitFilters()
}

function handleBrandChange(slugs: string[]) {
  selectedBrandSlugs.value = slugs
  emitFilters()
}

function emitFilters() {
  emit('filterChange', {
    category: selectedCategory.value,
    priceMin: priceRange.value.min > DEFAULT_PRICE_MIN ? priceRange.value.min : null,
    priceMax: priceRange.value.max < DEFAULT_PRICE_MAX ? priceRange.value.max : null,
    sizes: selectedSizeIds.value,
    colors: selectedColorIds.value,
    brands: selectedBrandSlugs.value,
  })
}

function clearAllFilters() {
  selectedCategory.value = null
  priceRange.value = { min: DEFAULT_PRICE_MIN, max: DEFAULT_PRICE_MAX }
  selectedSizeIds.value = []
  selectedColorIds.value = []
  selectedBrandSlugs.value = []
  emit('clear')
}
</script>

<template>
  <aside class="shop-filters">
    <div class="filters-header">
      <h3>Filters</h3>
      <button 
        v-if="hasActiveFilters" 
        class="clear-all"
        aria-label="Clear all filters"
        @click="clearAllFilters"
      >
        Clear All
      </button>
    </div>

    <div class="filter-section">
      <h4>Category</h4>
      <FilterCategoryTree
        :categories="categories"
        :selected-slug="selectedCategory"
        @select="handleCategorySelect"
      />
    </div>

    <div class="filter-section">
      <h4>Price Range</h4>
      <FilterPriceRange
        :min-value="priceRange.min"
        :max-value="priceRange.max"
        :min="DEFAULT_PRICE_MIN"
        :max="DEFAULT_PRICE_MAX"
        @range-change="handlePriceRangeChange"
      />
    </div>

    <div class="filter-section">
      <h4>Size</h4>
      <FilterSizeSelect
        :sizes="sizes"
        :selected-ids="selectedSizeIds"
        @update:selected-ids="handleSizeChange"
      />
    </div>

    <div class="filter-section">
      <h4>Color</h4>
      <FilterColorSelect
        :colors="colors"
        :selected-ids="selectedColorIds"
        @update:selected-ids="handleColorChange"
      />
    </div>

    <div class="filter-section">
      <h4>Brand</h4>
      <FilterBrandSelect
        :brands="brands"
        :selected-slugs="selectedBrandSlugs"
        @update:selected-slugs="handleBrandChange"
      />
    </div>
  </aside>
</template>

<style scoped lang="scss">
.shop-filters {
  background: var(--color-surface);
  border-radius: var(--radius-lg);
  padding: 1.5rem;
  max-height: calc(100vh - 200px);
  overflow-y: auto;
}

.filters-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid var(--color-border-light);

  h3 {
    margin: 0;
    font-size: var(--font-size-lg);
    font-weight: var(--font-weight-semibold);
  }

  .clear-all {
    background: none;
    border: none;
    color: var(--color-primary);
    cursor: pointer;
    font-size: var(--font-size-sm);
    text-decoration: underline;

    &:hover {
      text-decoration: none;
    }
  }
}

.filter-section {
  margin-bottom: 1.75rem;
  padding-bottom: 1.5rem;
  border-bottom: 1px solid var(--color-border-light);

  &:last-child {
    border-bottom: none;
    margin-bottom: 0;
    padding-bottom: 0;
  }

  h4 {
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-semibold);
    text-transform: uppercase;
    letter-spacing: 0.05em;
    margin-bottom: 0.75rem;
    color: var(--color-text);
  }
}
</style>
