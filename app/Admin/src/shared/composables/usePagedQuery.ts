import { ref, computed } from 'vue'
import type { Ref, ComputedRef } from 'vue'
import type { QueryingParameters } from '@/shared/types/querying'
import type { PagedResult } from '@/shared/types/result'
import { getPaged } from '@/shared/api'

export interface UsePagedQueryOptions {
  defaultPageSize?: number
  defaultFilter?: string
  defaultSort?: string[]
  defaultSearch?: string
  allowedFilterFields?: string[]
  allowedSortFields?: string[]
  allowedSearchFields?: string[]
  immediate?: boolean
}

export interface PagedQueryState<T> {
  items: Ref<T[]>
  loading: Ref<boolean>
  error: Ref<string | null>
  page: Ref<number>
  pageSize: Ref<number>
  totalCount: Ref<number>
  totalPages: ComputedRef<number>
  filter: Ref<string>
  sort: Ref<string[]>
  search: Ref<string>
  fetch: () => Promise<PagedResult<T>>
  refresh: () => Promise<PagedResult<T>>
  setPage: (p: number) => void
  setPageSize: (s: number) => void
  setFilter: (f: string) => void
  setSort: (s: string[]) => void
  setSearch: (s: string) => void
  nextPage: () => void
  prevPage: () => void
  reset: () => void
}

export function usePagedQuery<T>(
  url: string | (() => string),
  options?: UsePagedQueryOptions,
): PagedQueryState<T> {
  const items = ref<T[]>([]) as Ref<T[]>
  const loading = ref(false)
  const error = ref<string | null>(null)
  const page = ref(1)
  const pageSize = ref(options?.defaultPageSize ?? 20)
  const totalCount = ref(0)
  const filter = ref(options?.defaultFilter ?? '')
  const sort = ref<string[]>(options?.defaultSort ?? [])
  const search = ref(options?.defaultSearch ?? '')

  const totalPages = computed(() => {
    if (pageSize.value <= 0) return 0
    return Math.ceil(totalCount.value / pageSize.value)
  })

  async function fetch(): Promise<PagedResult<T>> {
    loading.value = true
    error.value = null

    const resolvedUrl = typeof url === 'function' ? url() : url
    const params: QueryingParameters = {
      filter: filter.value || null,
      sort: sort.value.length > 0 ? sort.value : null,
      search: search.value || null,
      pageNumber: page.value,
      pageSize: pageSize.value,
    }

    const result = await getPaged<T>(resolvedUrl, params, {
      allowedFilterFields: options?.allowedFilterFields,
      allowedSortFields: options?.allowedSortFields,
      allowedSearchFields: options?.allowedSearchFields,
    })

    if (result.isSuccess) {
      items.value = result.items
      totalCount.value = result.totalCount
      page.value = result.page
      pageSize.value = result.pageSize
    } else {
      error.value = result.message ?? result.errors[0]?.message ?? 'Request failed.'
    }

    loading.value = false
    return result
  }

  function refresh(): Promise<PagedResult<T>> {
    return fetch()
  }

  function setPage(p: number) {
    page.value = Math.max(1, p)
    fetch()
  }

  function setPageSize(s: number) {
    pageSize.value = Math.max(1, s)
    page.value = 1
    fetch()
  }

  function setFilter(f: string) {
    filter.value = f
    page.value = 1
    fetch()
  }

  function setSort(s: string[]) {
    sort.value = s
    page.value = 1
    fetch()
  }

  function setSearch(s: string) {
    search.value = s
    page.value = 1
    fetch()
  }

  function nextPage() {
    if (page.value < totalPages.value) {
      page.value++
      fetch()
    }
  }

  function prevPage() {
    if (page.value > 1) {
      page.value--
      fetch()
    }
  }

  function reset() {
    filter.value = ''
    sort.value = []
    search.value = ''
    page.value = 1
    pageSize.value = options?.defaultPageSize ?? 20
    totalCount.value = 0
    items.value = []
    error.value = null
  }

  if (options?.immediate ?? true) {
    fetch()
  }

  return {
    items,
    loading,
    error,
    page,
    pageSize,
    totalCount,
    totalPages,
    filter,
    sort,
    search,
    fetch,
    refresh,
    setPage,
    setPageSize,
    setFilter,
    setSort,
    setSearch,
    nextPage,
    prevPage,
    reset,
  }
}
