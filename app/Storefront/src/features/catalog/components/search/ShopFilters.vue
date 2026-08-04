<script setup lang="ts">
import { ref, computed } from 'vue'
import Button from 'primevue/button'
import FilterPriceRange from '../filters/FilterPriceRange.vue'
import FilterCategoryTree from '../filters/FilterCategoryTree.vue'
import type { StoreOptionTypeResponse } from '@/features/catalog/types'

const DEFAULT_PRICE_MIN = 0
const DEFAULT_PRICE_MAX = 1000

interface Category {
  id: string
  name: string
  slug: string
  children?: Category[]
}

interface Props {
  categories?: Category[]
  optionTypes?: StoreOptionTypeResponse[]
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  categories: () => [],
  optionTypes: () => [],
  loading: false,
})

const emit = defineEmits<{
  (e: 'filterChange', filters: FilterState): void
  (e: 'clear'): void
}>()

interface FilterState {
  category: string | null
  priceMin: number | null
  priceMax: number | null
  optionValues: string[]
}

const selectedCategory = ref<string | null>(null)
const priceRange = ref({ min: DEFAULT_PRICE_MIN, max: DEFAULT_PRICE_MAX })
const selectedOptionValueIds = ref<string[]>([])

const hasActiveFilters = computed(() => {
  return selectedCategory.value !== null ||
    priceRange.value.min > DEFAULT_PRICE_MIN ||
    priceRange.value.max < DEFAULT_PRICE_MAX ||
    selectedOptionValueIds.value.length > 0
})

function handlePriceRangeChange(range: { min: number; max: number }) {
  priceRange.value = range
  emitFilterChange()
}

function handleCategorySelect(slug: string | null) {
  selectedCategory.value = slug
  emitFilterChange()
}

function emitFilterChange() {
  emit('filterChange', {
    category: selectedCategory.value,
    priceMin: priceRange.value.min > DEFAULT_PRICE_MIN ? priceRange.value.min : null,
    priceMax: priceRange.value.max < DEFAULT_PRICE_MAX ? priceRange.value.max : null,
    optionValues: selectedOptionValueIds.value,
  })
}

function clearAllFilters() {
  selectedCategory.value = null
  priceRange.value = { min: DEFAULT_PRICE_MIN, max: DEFAULT_PRICE_MAX }
  selectedOptionValueIds.value = []
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

    <div v-if="loading" class="filter-loading">
      <i class="pi pi-spin pi-spinner"></i>
      <span>Loading filters...</span>
    </div>

    <template v-else>
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

      <div
        v-for="optionType in optionTypes"
        :key="optionType.id"
        class="filter-section"
      >
        <h4>{{ optionType.name }}</h4>
        <div class="filter-options">
          <label
            v-for="optionValue in optionType.values"
            :key="optionValue.id"
            class="filter-option"
          >
            <input
              type="checkbox"
              :value="optionValue.id"
              v-model="selectedOptionValueIds"
              @change="emitFilterChange"
            />
            <span>{{ optionValue.name }}</span>
          </label>
        </div>
      </div>
    </template>
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

.filter-loading {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 1rem 0;
  color: var(--color-text-secondary);
  font-size: var(--font-size-sm);
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

.filter-options {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.filter-option {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  font-size: var(--font-size-sm);
  color: var(--color-text);

  input[type='checkbox'] {
    cursor: pointer;
  }
}
</style>
