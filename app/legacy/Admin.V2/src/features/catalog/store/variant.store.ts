import { ref, readonly } from 'vue'
import { defineStore } from 'pinia'
import { defaultListQuery } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { VariantListItemResponse } from '../types'
import { VariantApi } from '../api'
import type { FilterGroup, SortDirection } from '@/shared/models/querying'

export const useVariantStore = defineStore('catalog-variant', () => {
  const items = ref<VariantListItemResponse[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const totalRecords = ref(0)
  const query = ref<ListQuery>(defaultListQuery())

  async function fetchMany(productId: string) {
    loading.value = true
    error.value = null
    try {
      const result = await VariantApi.getMany(productId, query.value)
      if (result.isSuccess) {
        items.value = result.items ?? []
        totalRecords.value = result.totalCount ?? 0
      } else {
        error.value = result.message ?? 'Failed to load'
        items.value = []
        totalRecords.value = 0
      }
    } catch (err) {
      console.error(err)
      error.value = 'Failed to load'
      items.value = []
      totalRecords.value = 0
    }
    loading.value = false
  }

  function setPage(page: number, productId: string) { query.value.page = page; return fetchMany(productId) }
  function setSearch(value: string, productId: string) {
    query.value.search = { value, mode: 'Any' }
    query.value.page = 1
    return fetchMany(productId)
  }
  function setSort(field: string, direction: SortDirection, productId: string) {
    query.value.sort = [{ field, direction }]
    return fetchMany(productId)
  }
  function setFilter(group: FilterGroup, productId: string) {
    query.value.filters = group
    query.value.page = 1
    return fetchMany(productId)
  }
  function resetQuery(productId: string) { query.value = defaultListQuery(); return fetchMany(productId) }

  return {
    items: readonly(items), loading: readonly(loading),
    error: readonly(error), totalRecords: readonly(totalRecords),
    query: readonly(query),
    fetchMany, setPage, setSearch, setSort, setFilter, resetQuery,
  }
})
