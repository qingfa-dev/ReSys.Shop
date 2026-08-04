import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useCatalogStore = defineStore('catalog', () => {
  const searchQuery = ref('')
  const selectedTaxonId = ref<string | null>(null)
  const selectedOptionValueIds = ref<string[]>([])
  const minPrice = ref<number | null>(null)
  const maxPrice = ref<number | null>(null)
  const sortField = ref<string | null>(null)
  const sortOrder = ref<number>(1)

  function setSearch(q: string): void {
    searchQuery.value = q
  }

  function setTaxon(id: string | null): void {
    selectedTaxonId.value = id
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
    selectedOptionValueIds.value = []
    minPrice.value = null
    maxPrice.value = null
    selectedTaxonId.value = null
    searchQuery.value = ''
  }

  return {
    searchQuery, selectedTaxonId, selectedOptionValueIds, minPrice, maxPrice, sortField, sortOrder,
    setSearch, setTaxon, toggleOptionValue, setPriceRange, clearFilters,
  }
})
