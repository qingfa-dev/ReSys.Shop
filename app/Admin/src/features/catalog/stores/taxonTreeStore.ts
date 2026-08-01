import { defineStore } from 'pinia'
import { ref } from 'vue'
import { type PagedResult } from '@/shared/types'
import type { TaxonTreeItem } from '../types/taxon'
import { TaxonApi } from '../services/taxonApi'

export const useTaxonTreeStore = defineStore('taxonTree', () => {
  const tree = ref<TaxonTreeItem[]>([])
  const treeLoading = ref(false)
  const treeTaxonomyId = ref<string | null>(null)

  async function fetchTree(taxonomyId: string): Promise<PagedResult<TaxonTreeItem>> {
    if (treeTaxonomyId.value === taxonomyId) {
      return {
        items: tree.value,
        page: 1,
        pageSize: tree.value.length,
        totalCount: tree.value.length,
        totalPages: tree.value.length > 0 ? 1 : 0,
        isSuccess: true,
        statusCode: 200,
        message: null,
        errors: [],
        metadata: null,
      }
    }

    treeLoading.value = true
    const result = await TaxonApi.getTree(taxonomyId)
    treeLoading.value = false

    if (result.isSuccess) {
      tree.value = result.items ?? []
      treeTaxonomyId.value = taxonomyId
    }

    return result
  }

  return { tree, treeLoading, treeTaxonomyId, fetchTree }
})
