import { ref, readonly, watch } from 'vue'
import { defineStore } from 'pinia'
import { defaultListQuery } from '@/shared/models'
import type { ListQuery } from '@/shared/models'
import type { ProductResponse } from '../types'
import { ProductApi } from '../api'
import type { FilterGroup, SortDirection, FilterCondition, FilterOperator } from '@/shared/models/querying'
import type { FilterConfig } from '@/shared/components/layout/FilterPanel.vue'

export const useProductStore = defineStore('catalog-product', () => {
  const items = ref<ProductResponse[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const totalRecords = ref(0)
  const listQuery = ref<ListQuery>(defaultListQuery())

  const searchQuery = ref('')
  const activeFilters = ref<FilterConfig[]>([])
  let skipSearchWatch = false

  watch(searchQuery, (val) => {
    if (skipSearchWatch) {
      skipSearchWatch = false
      return
    }
    listQuery.value = {
      ...listQuery.value,
      search: val ? { value: val, mode: 'Any' } : undefined,
      page: 1,
    }
    fetchMany()
  })

  async function fetchMany() {
    loading.value = true
    error.value = null
    try {
      const result = await ProductApi.getMany(listQuery.value)
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

  function setPage(page: number) { listQuery.value.page = page; return fetchMany() }
  function setSort(field: string, direction: SortDirection) {
    listQuery.value.sort = [{ field, direction }]
    return fetchMany()
  }
  function resetQuery() { listQuery.value = defaultListQuery(); return fetchMany() }

  function buildFilterGroup(filters: FilterConfig[]): FilterGroup {
    const conditions: FilterCondition[] = filters.map(f => ({
      field: f.field,
      operator: f.operator as FilterOperator,
      value: String(f.value),
    }))
    return { logic: 'And', conditions, groups: [] }
  }

  function setFilters(f: FilterConfig[]) {
    activeFilters.value = f
    listQuery.value = {
      ...listQuery.value,
      filters: f.length > 0 ? buildFilterGroup(f) : undefined,
      page: 1,
    }
    return fetchMany()
  }

  function setFilter(group: FilterGroup) {
    listQuery.value = { ...listQuery.value, filters: group, page: 1 }
    activeFilters.value = []
    return fetchMany()
  }

  function setSearchQuery(q: string) {
    searchQuery.value = q
  }

  function setSearch(value: string) {
    skipSearchWatch = true
    searchQuery.value = value
    listQuery.value = {
      ...listQuery.value,
      search: value ? { value, mode: 'Any' } : undefined,
      page: 1,
    }
    return fetchMany()
  }

  return {
    items: readonly(items), loading: readonly(loading),
    error: readonly(error), totalRecords: readonly(totalRecords),
    query: readonly(listQuery),
    searchQuery: readonly(searchQuery),
    activeFilters: readonly(activeFilters),
    fetchMany, setPage, setSort, setFilters, setFilter, setSearchQuery, setSearch, resetQuery,
  }
})
