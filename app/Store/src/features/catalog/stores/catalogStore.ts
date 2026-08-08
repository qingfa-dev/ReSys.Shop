import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { TaxonApi } from '../services/taxonApi'
import { OptionTypeApi } from '../services/optionTypeApi'
import { emit } from '@/shared/composables/useStoreEvents'
import type { TaxonomyGroup, StoreOptionTypeListItem, StoreOptionValueListItemResponse } from '../types'

export const useCatalogStore = defineStore('catalog', () => {
  const searchQuery = ref('')
  const selectedTaxonIds = ref<string[]>([])
  const selectedOptionValueIds = ref<string[]>([])
  const minPrice = ref<number | null>(null)
  const maxPrice = ref<number | null>(null)
  const sortField = ref('-CreatedAtUtc')

  const taxonomyGroups = ref<TaxonomyGroup[]>([])
  const optionTypes = ref<(StoreOptionTypeListItem & { values: StoreOptionValueListItemResponse[] })[]>([])
  const taxonsLoading = ref(false)
  const optionsLoading = ref(false)

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

  function setSearch(query: string): void {
    searchQuery.value = query
    emitFilterChanged()
  }

  function toggleTaxon(id: string): void {
    // Filter: Add or remove taxon ID from active category filters
    const idx = selectedTaxonIds.value.indexOf(id)
    if (idx === -1) selectedTaxonIds.value.push(id)
    else selectedTaxonIds.value.splice(idx, 1)
    emitFilterChanged()
  }

  function toggleOptionValue(id: string): void {
    // Filter: Add or remove option value ID from active attribute filters
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

  function clearFilters(): void {
    // Reset: Clear all active filters and notify product list
    searchQuery.value = ''
    selectedTaxonIds.value = []
    selectedOptionValueIds.value = []
    minPrice.value = null
    maxPrice.value = null
    emit({ type: 'filter:changed' })
  }

  async function loadTaxonomyGroups(): Promise<void> {
    // Guard: Skip fetch if taxonomy groups already loaded (singleton cache)
    if (taxonomyGroups.value.length > 0) return
    taxonsLoading.value = true
    // Call: Catalog API — fetch taxonomies and taxons in parallel for category tree
    const [taxonomiesResult, taxonsResult] = await Promise.all([
      TaxonApi.getTaxonomies({ pageNumber: 1, pageSize: 50 }),
      TaxonApi.getTaxons({ pageNumber: 1, pageSize: 500 }),
    ])
    if (taxonomiesResult.isSuccess && taxonsResult.isSuccess) {
      // Map: Build taxonomy groups with nested tree structure from flat taxon list
      taxonomyGroups.value = taxonomiesResult.items.map(t => ({
        taxonomy: { id: t.id, name: t.name, presentation: t.presentation },
        tree: buildTree(taxonsResult.items, t.id),
      }))
    }
    taxonsLoading.value = false
  }

  async function loadOptionTypes(): Promise<void> {
    // Guard: Skip fetch if option types already loaded (singleton cache)
    if (optionTypes.value.length > 0) return
    optionsLoading.value = true
    // Call: Catalog API — fetch option types and values in parallel for filter sidebar
    const [typesResult, valuesResult] = await Promise.all([
      OptionTypeApi.getOptionTypes({ pageNumber: 1, pageSize: 50 }),
      OptionTypeApi.getOptionValues({ pageNumber: 1, pageSize: 500 }),
    ])
    if (typesResult.isSuccess && valuesResult.isSuccess) {
      // Filter: Only include filterable option types (e.g. exclude internal-only types)
      // Map: Attach option values to their parent type by ID
      optionTypes.value = typesResult.items
        .filter(t => t.filterable)
        .map(t => ({
          ...t,
          values: valuesResult.items.filter(v => v.optionTypeId === t.id),
        }))
    }
    optionsLoading.value = false
  }

  function setSort(field: string): void {
    sortField.value = field
    emitFilterChanged()
  }

  function emitFilterChanged(): void {
    // Raise: Emit filter:changed event for product list store to refetch
    emit({ type: 'filter:changed' })
  }

  return {
    searchQuery, selectedTaxonIds, selectedOptionValueIds, minPrice, maxPrice, sortField,
    taxonomyGroups, optionTypes, taxonsLoading, optionsLoading,
    activeFilterCount,
    setSearch, toggleTaxon, toggleOptionValue, setPriceRange, setSort, clearFilters,
    loadTaxonomyGroups, loadOptionTypes,
  }
})

// Map: Convert flat taxon list into nested tree structure grouped by taxonomy
function buildTree(items: any[], taxonomyId: string, parentId: string | null = null): any[] {
  return items
    .filter(i => i.taxonomyId === taxonomyId && i.parentId === parentId)
    .map(i => ({
      id: i.id,
      name: i.name,
      presentation: i.presentation,
      permalink: i.permalink,
      depth: i.depth,
      hasChildren: items.some(c => c.parentId === i.id),
      children: buildTree(items, taxonomyId, i.id),
    }))
}
