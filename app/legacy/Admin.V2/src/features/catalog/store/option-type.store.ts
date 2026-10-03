import { ref, readonly } from 'vue'
import { defineStore } from 'pinia'
import { defaultListQuery } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { OptionTypeResponse } from '../types'
import { OptionTypeApi } from '../api'
import type { FilterGroup, SortDirection } from '@/shared/models/querying'

export const useOptionTypeStore = defineStore('catalog-option-type', () => {
  const items = ref<OptionTypeResponse[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const totalRecords = ref(0)
  const query = ref<ListQuery>(defaultListQuery())

  async function fetchMany() {
    loading.value = true
    error.value = null
    try {
      const result = await OptionTypeApi.getMany(query.value)
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

  function setPage(page: number) { query.value.page = page; return fetchMany() }
  function setSearch(value: string) {
    query.value.search = { value, mode: 'Any' }
    query.value.page = 1
    return fetchMany()
  }
  function setSort(field: string, direction: SortDirection) {
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
