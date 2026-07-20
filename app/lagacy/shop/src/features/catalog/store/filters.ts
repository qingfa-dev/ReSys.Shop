import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export interface FilterOption {
  id: string
  label: string
  count: number
  active: boolean
}

export interface ColorFilter extends FilterOption {
  hex: string
}

export interface SizeFilter extends FilterOption {
  value: string
}

export interface PriceRange {
  min: number
  max: number
  step: number
}

export interface FilterState {
  categories: FilterOption[]
  brands: FilterOption[]
  colors: ColorFilter[]
  sizes: SizeFilter[]
  priceRange: PriceRange
  activePriceRange: [number, number]
  conditions: FilterOption[]
  genders: FilterOption[]
  tags: FilterOption[]
  sortBy: SortOption
}

export type SortOption = 
  | 'recommended' 
  | 'newest' 
  | 'price-asc' 
  | 'price-desc' 
  | 'most-reviewed' 
  | 'top-rated' 
  | 'sale'

export interface ActiveFilter {
  key: keyof FilterState
  value: string
  label: string
}

export const useFiltersStore = defineStore('filters', () => {
  const state = ref<FilterState>({
    categories: [],
    brands: [],
    colors: [],
    sizes: [],
    priceRange: { min: 0, max: 10000, step: 50 },
    activePriceRange: [0, 10000],
    conditions: [],
    genders: [],
    tags: [],
    sortBy: 'recommended',
  })

  const activeFilters = computed<ActiveFilter[]>(() => {
    const active: ActiveFilter[] = []

    state.value.categories.filter(f => f.active).forEach(f => {
      active.push({ key: 'categories', value: f.id, label: f.label })
    })

    state.value.brands.filter(f => f.active).forEach(f => {
      active.push({ key: 'brands', value: f.id, label: f.label })
    })

    state.value.sizes.filter(f => f.active).forEach(f => {
      active.push({ key: 'sizes', value: f.id, label: f.label })
    })

    state.value.colors.filter(f => f.active).forEach(f => {
      active.push({ key: 'colors', value: f.id, label: f.label })
    })

    state.value.conditions.filter(f => f.active).forEach(f => {
      active.push({ key: 'conditions', value: f.id, label: f.label })
    })

    state.value.genders.filter(f => f.active).forEach(f => {
      active.push({ key: 'genders', value: f.id, label: f.label })
    })

    state.value.tags.filter(f => f.active).forEach(f => {
      active.push({ key: 'tags', value: f.id, label: f.label })
    })

    const [min, max] = state.value.activePriceRange
    const [rMin, rMax] = [state.value.priceRange.min, state.value.priceRange.max]
    if (min !== rMin || max !== rMax) {
      active.push({ key: 'activePriceRange', value: `${min}-${max}`, label: `$${min} – $${max}` })
    }

    return active
  })

  const hasActiveFilters = computed(() => activeFilters.value.length > 0)
  const activeCount = computed(() => activeFilters.value.length)

  function toggleFilter(key: 'categories' | 'brands' | 'sizes' | 'colors' | 'conditions' | 'genders' | 'tags', id: string) {
    const group = state.value[key] as FilterOption[]
    const item = group.find(f => f.id === id)
    if (item) {
      item.active = !item.active
    }
  }

  function setPriceRange(range: [number, number]) {
    state.value.activePriceRange = range
  }

  function setSortBy(sort: SortOption) {
    state.value.sortBy = sort
  }

  function removeFilter(filter: ActiveFilter) {
    if (filter.key === 'activePriceRange') {
      state.value.activePriceRange = [state.value.priceRange.min, state.value.priceRange.max]
      return
    }
    const group = state.value[filter.key] as FilterOption[]
    const item = group.find(f => f.id === filter.value)
    if (item) {
      item.active = false
    }
  }

  function clearAll() {
    ;(['categories', 'brands', 'sizes', 'colors', 'conditions', 'genders', 'tags'] as const).forEach(key => {
      const group = state.value[key] as FilterOption[]
      group.forEach(f => (f.active = false))
    })
    state.value.activePriceRange = [state.value.priceRange.min, state.value.priceRange.max]
    state.value.sortBy = 'recommended'
  }

  function setAvailableFilters(filters: Partial<FilterState>) {
    Object.assign(state.value, filters)
  }

  return {
    state,
    activeFilters,
    hasActiveFilters,
    activeCount,
    toggleFilter,
    setPriceRange,
    setSortBy,
    removeFilter,
    clearAll,
    setAvailableFilters,
  }
})
