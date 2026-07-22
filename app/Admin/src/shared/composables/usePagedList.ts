import { ref } from 'vue'
import type { Result, PagedResult, QueryingModel } from '@/shared/models'

type PagedFetchResult<T> = PagedResult<T> | Result<T[]>

function isPaged<T>(r: PagedFetchResult<T>): r is PagedResult<T> {
  return 'items' in r && 'totalCount' in r
}

export function usePagedList<TItem>(fetchFn: (params: QueryingModel) => Promise<PagedFetchResult<TItem>>) {
  const items = ref<TItem[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const totalRecords = ref(0)
  const params = ref<QueryingModel>({
    filter: { conditions: [], allowedFields: [], violations: [] },
    search: { term: { value: '', caseSensitive: false }, fields: [], mode: 'Any', allowedFields: [], violations: [] },
    sort: { clauses: [], allowedFields: [], violations: [] },
    page: { page: 1, pageSize: 10, isEmpty: false, bounds: { defaultPage: 1, defaultPageSize: 10, maxPageSize: 100 }, violations: [] },
  })

  async function fetch() {
    loading.value = true
    error.value = null
    try {
      const result = await fetchFn(params.value)
      if (result.isSuccess) {
        if (isPaged(result)) {
          items.value = result.items
          totalRecords.value = result.totalCount || 0
        } else if (result.value) {
          items.value = result.value
          totalRecords.value = result.value.length || 0
        }
      } else {
        error.value = result.errors?.[0]?.message || 'Failed to fetch'
      }
      return result
    } catch {
      error.value = 'An unexpected error occurred'
    } finally {
      loading.value = false
    }
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
    refresh,
  }
}
