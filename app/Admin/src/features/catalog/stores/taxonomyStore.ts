import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { TaxonomyListItem } from '../types/taxonomy'
import { TaxonomyApi } from '../services/taxonomyApi'

export const useTaxonomyStore = defineStore('taxonomies', () => {
  const activeTaxonomies = ref<TaxonomyListItem[]>([])
  const loaded = ref(false)

  async function fetchActive(): Promise<void> {
    if (loaded.value) return

    const result = await TaxonomyApi.getTaxonomies({
      pageSize: 100,
      sortBy: 'name',
      sortDirection: 'asc',
    })

    if (result.isSuccess) {
      activeTaxonomies.value = result.items
      loaded.value = true
    }
  }

  return { activeTaxonomies, loaded, fetchActive }
})
