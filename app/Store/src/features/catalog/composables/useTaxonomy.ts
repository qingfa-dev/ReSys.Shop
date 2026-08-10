import { ref, reactive } from 'vue'
import { TaxonomyApi } from '../services/taxonApi'
import { OptionTypeApi } from '../services/optionTypeApi'
import type { TaxonomyGroup, StoreOptionTypeListItem, StoreOptionValueListItemResponse, StoreTaxonListItemResponse, TaxonTreeNode } from '../types'

// Module-level singleton state
const taxonomyGroups = ref<TaxonomyGroup[]>([])
const optionTypes = ref<(StoreOptionTypeListItem & { values: StoreOptionValueListItemResponse[] })[]>([])
const collections = ref<StoreTaxonListItemResponse[]>([])
const taxonsLoading = ref(false)
const optionsLoading = ref(false)

// Map: Convert flat taxon list into nested tree structure grouped by taxonomy
function buildTree(items: StoreTaxonListItemResponse[], taxonomyId: string, parentId: string | null = null): TaxonTreeNode[] {
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

export function useTaxonomy() {
  async function loadTaxonomyGroups(): Promise<void> {
    if (taxonomyGroups.value.length > 0) return
    taxonsLoading.value = true
    const [taxonomiesResult, taxonsResult] = await Promise.all([
      TaxonomyApi.getTaxonomies({ pageNumber: 1, pageSize: 50 }),
      TaxonomyApi.getTaxons({ pageNumber: 1, pageSize: 500 }),
    ])
    if (taxonomiesResult.isSuccess && taxonsResult.isSuccess) {
      taxonomyGroups.value = taxonomiesResult.items.map(t => ({
        taxonomy: { id: t.id, name: t.name, presentation: t.presentation },
        tree: buildTree(taxonsResult.items, t.id),
      }))
      const roots: StoreTaxonListItemResponse[] = []
      const seen = new Set<string>()
      for (const t of taxonsResult.items) {
        if (t.depth === 0 && !seen.has(t.id)) {
          seen.add(t.id)
          roots.push(t)
        }
      }
      collections.value = roots
    }
    taxonsLoading.value = false
  }

  async function loadOptionTypes(): Promise<void> {
    if (optionTypes.value.length > 0) return
    optionsLoading.value = true
    const [typesResult, valuesResult] = await Promise.all([
      OptionTypeApi.getOptionTypes({ pageNumber: 1, pageSize: 50 }),
      OptionTypeApi.getOptionValues({ pageNumber: 1, pageSize: 500 }),
    ])
    if (typesResult.isSuccess && valuesResult.isSuccess) {
      optionTypes.value = typesResult.items
        .filter(t => t.filterable)
        .map(t => ({
          ...t,
          values: valuesResult.items.filter(v => v.optionTypeId === t.id),
        }))
    }
    optionsLoading.value = false
  }

  return reactive({
    taxonomyGroups, optionTypes, collections, taxonsLoading, optionsLoading,
    loadTaxonomyGroups, loadOptionTypes,
  })
}
