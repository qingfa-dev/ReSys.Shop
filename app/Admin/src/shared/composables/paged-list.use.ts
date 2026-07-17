import { ref } from 'vue'
import type { ApiResult } from '@/shared/api/types/api.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'

export function usePagedList<TItem, TParams extends ServerQueryingParameters = ServerQueryingParameters>(
  fetchFn: (params: TParams) => Promise<ApiResult<TItem[]>>,
  defaultParams?: Partial<TParams>,
) {
  const items = ref<TItem[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const totalRecords = ref(0)
  const params = ref<TParams>({
    page: 1,
    pageSize: 10,
    ...defaultParams,
  } as TParams)

  async function fetch(overrides?: Partial<TParams>) {
    loading.value = true
    error.value = null

    if (overrides) {
      params.value = { ...params.value, ...overrides }
    }

    try {
      const result = await fetchFn(params.value)
      if (result.success && result.data) {
        items.value = result.data
        totalRecords.value = result.meta?.totalCount || 0
      } else {
        error.value = result.error?.title || 'Failed to fetch'
      }
      return result
    } catch (err) {
      error.value = 'An unexpected error occurred'
      throw err
    } finally {
      loading.value = false
    }
  }

  function setPage(page: number) {
    return fetch({ page } as unknown as Partial<TParams>)
  }

  function setSort(sort: string[]) {
    return fetch({ sort } as unknown as Partial<TParams>)
  }

  function setSearch(search: string, searchFields?: string[]) {
    return fetch({ search, searchFields } as unknown as Partial<TParams>)
  }

  function refresh() {
    return fetch()
  }

  return {
    items,
    loading,
    error,
    totalRecords,
    params,
    fetch,
    setPage,
    setSort,
    setSearch,
    refresh,
  }
}
