import { ref, readonly, watch } from 'vue'
import { defineStore } from 'pinia'
import { defaultListQuery } from '@/shared/models'
import type { ListQuery, Result } from '@/shared/models'
import type { ShippingRateResponse, CreateShippingRateRequest, UpdateShippingRateRequest } from '../types'
import { ShippingRateApi } from '../api'
import type { FilterGroup, SortDirection, FilterCondition, FilterOperator } from '@/shared/models/querying'
import type { FilterConfig } from '@/shared/components/layout/FilterPanel.vue'

export const useShippingRateStore = defineStore('shipping-rate', () => {
  const items = ref<ShippingRateResponse[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const totalRecords = ref(0)
  const query = ref<ListQuery>(defaultListQuery())

  const searchQuery = ref('')
  const activeFilters = ref<FilterConfig[]>([])
  let skipSearchWatch = false

  watch(searchQuery, (val) => {
    if (skipSearchWatch) {
      skipSearchWatch = false
      return
    }
    query.value = {
      ...query.value,
      search: val ? { value: val, mode: 'Any' } : undefined,
      page: 1,
    }
    fetchMany()
  })

  async function fetchMany() {
    loading.value = true
    error.value = null
    try {
      const result = await ShippingRateApi.getMany(query.value)
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
  function setSort(field: string, direction: SortDirection) {
    query.value.sort = [{ field, direction }]
    return fetchMany()
  }
  function resetQuery() { query.value = defaultListQuery(); return fetchMany() }

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
    query.value = {
      ...query.value,
      filters: f.length > 0 ? buildFilterGroup(f) : undefined,
      page: 1,
    }
    return fetchMany()
  }

  function setFilter(group: FilterGroup) {
    query.value = { ...query.value, filters: group, page: 1 }
    activeFilters.value = []
    return fetchMany()
  }

  function setSearchQuery(q: string) {
    searchQuery.value = q
  }

  function setSearch(value: string) {
    skipSearchWatch = true
    searchQuery.value = value
    query.value = {
      ...query.value,
      search: value ? { value, mode: 'Any' } : undefined,
      page: 1,
    }
    return fetchMany()
  }

  async function getById(id: string): Promise<Result<ShippingRateResponse>> {
    loading.value = true
    error.value = null
    try {
      const result = await ShippingRateApi.get(id)
      if (!result.isSuccess) {
        error.value = result.message ?? 'Failed to load'
      }
      return result
    } catch (err) {
      console.error(err)
      error.value = 'Failed to load'
      return { isSuccess: false, statusCode: 0, errors: [], message: 'Failed to load', metadata: null, value: null as unknown as ShippingRateResponse }
    } finally {
      loading.value = false
    }
  }

  async function create(data: CreateShippingRateRequest): Promise<Result<ShippingRateResponse>> {
    loading.value = true
    error.value = null
    try {
      const result = await ShippingRateApi.create(data)
      if (!result.isSuccess) {
        error.value = result.message ?? 'Failed to create'
      }
      return result
    } catch (err) {
      console.error(err)
      error.value = 'Failed to create'
      return { isSuccess: false, statusCode: 0, errors: [], message: 'Failed to create', metadata: null, value: null as unknown as ShippingRateResponse }
    } finally {
      loading.value = false
    }
  }

  async function update(id: string, data: UpdateShippingRateRequest): Promise<Result<ShippingRateResponse>> {
    loading.value = true
    error.value = null
    try {
      const result = await ShippingRateApi.update(id, data)
      if (!result.isSuccess) {
        error.value = result.message ?? 'Failed to update'
      }
      return result
    } catch (err) {
      console.error(err)
      error.value = 'Failed to update'
      return { isSuccess: false, statusCode: 0, errors: [], message: 'Failed to update', metadata: null, value: null as unknown as ShippingRateResponse }
    } finally {
      loading.value = false
    }
  }

  async function deleteRate(id: string): Promise<Result<void>> {
    loading.value = true
    error.value = null
    try {
      const result = await ShippingRateApi.delete(id)
      if (!result.isSuccess) {
        error.value = result.message ?? 'Failed to delete'
      }
      return result
    } catch (err) {
      console.error(err)
      error.value = 'Failed to delete'
      return { isSuccess: false, statusCode: 0, errors: [], message: 'Failed to delete', metadata: null, value: null as unknown as void }
    } finally {
      loading.value = false
    }
  }

  return {
    items: readonly(items), loading: readonly(loading),
    error: readonly(error), totalRecords: readonly(totalRecords),
    query: readonly(query),
    searchQuery: readonly(searchQuery),
    activeFilters: readonly(activeFilters),
    fetchMany, setPage, setSort, setFilters, setFilter, setSearchQuery, setSearch, resetQuery,
    getById, create, update, delete: deleteRate,
  }
})
