import { ref, readonly } from 'vue'
import { defineStore } from 'pinia'
import { defaultListQuery } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { TaxonomyResponse } from '../types'
import { TaxonomyApi } from '../api'
import type { FilterGroup } from '@/shared/models/querying'

export const useTaxonomyStore = defineStore('catalog-taxonomy', () => {
  const items = ref<TaxonomyResponse[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const totalRecords = ref(0)
  const query = ref<ListQuery>(defaultListQuery())

  async function fetchMany() {
    loading.value = true
    error.value = null
    try {
      const result = await TaxonomyApi.getMany(query.value)
      if (result.isSuccess) {
        items.value = result.items ?? []
        totalRecords.value = result.totalCount ?? 0
      } else {
        error.value = result.message ?? 'Failed to load'
        items.value = []
        totalRecords.value = 0
      }
    } catch {
      error.value = 'Failed to load'
      items.value = []
      totalRecords.value = 0
    }
    loading.value = false
  }

  function setPage(page: number) { query.value.page = page; return fetchMany() }
  function setSearch(value: string) {
    query.value.search = { value, mode: 'Any' }
    query.value.page = 1
    return fetchMany()
  }
  function setSort(field: string, direction: 'Asc' | 'Desc') {
    query.value.sort = [{ field, direction }]
    return fetchMany()
  }
  function setFilter(group: FilterGroup) {
    query.value.filters = group
    query.value.page = 1
    return fetchMany()
  }
  function resetQuery() { query.value = defaultListQuery(); return fetchMany() }

  return {
    items: readonly(items), loading: readonly(loading),
    error: readonly(error), totalRecords: readonly(totalRecords),
    query: readonly(query),
    fetchMany, setPage, setSearch, setSort, setFilter, resetQuery,
  }
})
