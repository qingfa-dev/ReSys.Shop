import { defineStore } from 'pinia'
import { ref } from 'vue'
import { ok, type Result } from '@/shared/types'
import type { TaxonTreeItem } from '../types/taxon'
import { TaxonApi } from '../services/taxonApi'

export const useTaxonTreeStore = defineStore('taxonTree', () => {
  const tree = ref<TaxonTreeItem[]>([])
  const treeLoading = ref(false)
  const treeTaxonomyId = ref<string | null>(null)

  async function fetchTree(taxonomyId: string): Promise<Result<{ tree: TaxonTreeItem[] }>> {
    if (treeTaxonomyId.value === taxonomyId) {
      return ok({ tree: tree.value })
    }

    treeLoading.value = true
    const result = await TaxonApi.getTree(taxonomyId)
    treeLoading.value = false

    if (result.isSuccess) {
      tree.value = result.value?.tree ?? []
      treeTaxonomyId.value = taxonomyId
    }

    return result
  }

  return { tree, treeLoading, treeTaxonomyId, fetchTree }
})
