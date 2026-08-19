import { ref, computed, reactive } from 'vue'
import { emit } from '@/shared/composables/useStoreEvents'

// Module-level singleton state
const searchQuery = ref('')
const selectedTaxonIds = ref<string[]>([])
const selectedOptionValueIds = ref<string[]>([])
const minPrice = ref<number | null>(null)
const maxPrice = ref<number | null>(null)
const sortField = ref('-CreatedAtUtc')

// Compute: Count all active filters for badge display on filter button
const activeFilterCount = computed(() => {
  let count = 0
  if (searchQuery.value) count++
  count += selectedTaxonIds.value.length
  count += selectedOptionValueIds.value.length
  if (minPrice.value != null) count++
  if (maxPrice.value != null) count++
  return count
})

function emitFilterChanged(): void {
  emit({ type: 'filter:changed' })
}

export function useFilters() {
  function setSearch(query: string): void {
    searchQuery.value = query
    emitFilterChanged()
  }

  function toggleTaxon(id: string): void {
    const idx = selectedTaxonIds.value.indexOf(id)
    if (idx === -1) selectedTaxonIds.value.push(id)
    else selectedTaxonIds.value.splice(idx, 1)
    emitFilterChanged()
  }

  function toggleOptionValue(id: string): void {
    const idx = selectedOptionValueIds.value.indexOf(id)
    if (idx === -1) selectedOptionValueIds.value.push(id)
    else selectedOptionValueIds.value.splice(idx, 1)
    emitFilterChanged()
  }

  function setPriceRange(min: number | null, max: number | null): void {
    minPrice.value = min
    maxPrice.value = max
    emitFilterChanged()
  }

  function setSort(field: string): void {
    sortField.value = field
    emitFilterChanged()
  }

  function clearFilters(): void {
    searchQuery.value = ''
    selectedTaxonIds.value = []
    selectedOptionValueIds.value = []
    minPrice.value = null
    maxPrice.value = null
    emit({ type: 'filter:changed' })
  }

  return reactive({
    searchQuery, selectedTaxonIds, selectedOptionValueIds, minPrice, maxPrice, sortField,
    activeFilterCount,
    setSearch, toggleTaxon, toggleOptionValue, setPriceRange, setSort, clearFilters,
  })
}
