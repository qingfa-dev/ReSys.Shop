import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useCatalogStore = defineStore('catalog', () => {
  const searchQuery = ref('')
  const selectedTaxonIds = ref<string[]>([])
  const selectedOptionValueIds = ref<string[]>([])
  const minPrice = ref<number | null>(null)
  const maxPrice = ref<number | null>(null)
  const sortField = ref<string | null>(null)

  function setSearch(q: string): void {
    searchQuery.value = q
  }

  function toggleTaxon(id: string): void {
    const idx = selectedTaxonIds.value.indexOf(id)
    if (idx >= 0) {
      selectedTaxonIds.value.splice(idx, 1)
    } else {
      selectedTaxonIds.value.push(id)
    }
  }

  function toggleOptionValue(id: string): void {
    const idx = selectedOptionValueIds.value.indexOf(id)
    if (idx >= 0) {
      selectedOptionValueIds.value.splice(idx, 1)
    } else {
      selectedOptionValueIds.value.push(id)
    }
  }

  function setPriceRange(min: number | null, max: number | null): void {
    minPrice.value = min
    maxPrice.value = max
  }

  function clearFilters(): void {
    selectedTaxonIds.value = []
    selectedOptionValueIds.value = []
    minPrice.value = null
    maxPrice.value = null
    searchQuery.value = ''
  }

  return {
    searchQuery, selectedTaxonIds, selectedOptionValueIds, minPrice, maxPrice, sortField,
    setSearch, toggleTaxon, toggleOptionValue, setPriceRange, clearFilters,
  }
})
