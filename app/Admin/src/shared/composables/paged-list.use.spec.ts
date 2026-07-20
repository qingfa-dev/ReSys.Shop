import { describe, it, expect, vi } from 'vitest'
import { usePagedList } from './paged-list.use'
import type { ServerPagedResult } from '@/shared/api/types/result.types'

type MockItem = { id: string; name: string }

describe('usePagedList', () => {
  const mockItems: MockItem[] = [{ id: '1', name: 'Item 1' }, { id: '2', name: 'Item 2' }]

  function createMockFetch(result: Partial<ServerPagedResult<MockItem>> = {}) {
    return vi.fn().mockResolvedValue({
      isSuccess: true,
      statusCode: 200,
      errors: [],
      message: null,
      metadata: null,
      items: mockItems,
      page: 1,
      pageSize: 10,
      totalCount: 10,
      ...result,
    } as ServerPagedResult<MockItem>)
  }

  it('should initialize with default state', () => {
    const { items, loading, error, totalRecords, params } = usePagedList(createMockFetch())
    expect(items.value).toEqual([])
    expect(loading.value).toBe(false)
    expect(error.value).toBeNull()
    expect(totalRecords.value).toBe(0)
    expect(params.value.page).toBe(1)
    expect(params.value.pageSize).toBe(10)
  })

  it('should initialize with custom default params', () => {
    const { params } = usePagedList(createMockFetch(), { pageSize: 25, sort: ['-createdAt'] })
    expect(params.value.pageSize).toBe(25)
    expect(params.value.sort).toEqual(['-createdAt'])
  })

  it('should fetch and populate items + totalRecords on success', async () => {
    const mockFetch = createMockFetch()
    const { items, totalRecords, loading, fetch } = usePagedList(mockFetch)

    await fetch()

    expect(mockFetch).toHaveBeenCalledOnce()
    expect(items.value).toEqual(mockItems)
    expect(totalRecords.value).toBe(10)
    expect(loading.value).toBe(false)
  })

  it('should set error on failed fetch', async () => {
    const mockFetch = createMockFetch({ isSuccess: false, errors: [{ code: 'NotFound', message: 'Not Found', type: 1, metadata: null }], items: [], totalCount: 0 })
    const { items, error, fetch } = usePagedList(mockFetch)

    await fetch()

    expect(items.value).toEqual([])
    expect(error.value).toBe('Not Found')
  })

  it('should merge overrides into params on fetch', async () => {
    const mockFetch = createMockFetch()
    const { params, fetch } = usePagedList(mockFetch, { pageSize: 10 })

    await fetch({ search: 'test', page: 2 })

    expect(params.value.search).toBe('test')
    expect(params.value.page).toBe(2)
    expect(params.value.pageSize).toBe(10)
  })

  it('setPage should call fetch with page override', async () => {
    const mockFetch = createMockFetch()
    const { setPage, params } = usePagedList(mockFetch)

    await setPage(3)

    expect(params.value.page).toBe(3)
    expect(mockFetch).toHaveBeenCalledOnce()
  })

  it('setSort should update sort param', async () => {
    const mockFetch = createMockFetch()
    const { setSort, params } = usePagedList(mockFetch)

    await setSort(['name', '-createdAt'])

    expect(params.value.sort).toEqual(['name', '-createdAt'])
  })

  it('setSearch should update search and searchFields', async () => {
    const mockFetch = createMockFetch()
    const { setSearch, params } = usePagedList(mockFetch)

    await setSearch('query', ['name', 'email'])

    expect(params.value.search).toBe('query')
    expect(params.value.searchFields).toEqual(['name', 'email'])
  })

  it('refresh should re-fetch with same params', async () => {
    const mockFetch = createMockFetch()
    const { refresh, fetch } = usePagedList(mockFetch)

    await fetch({ search: 'test' })
    expect(mockFetch).toHaveBeenCalledTimes(1)

    await refresh()
    expect(mockFetch).toHaveBeenCalledTimes(2)
  })

  it('should handle empty data response', async () => {
    const mockFetch = vi.fn().mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, items: [], page: 1, pageSize: 10, totalCount: 0 })
    const { items, totalRecords, fetch } = usePagedList(mockFetch)

    await fetch()

    expect(items.value).toEqual([])
    expect(totalRecords.value).toBe(0)
  })

  it('loading should be true during fetch', async () => {
    let resolvePromise!: (value: unknown) => void
    const mockFetch = vi.fn().mockImplementation(() => new Promise(resolve => {
      resolvePromise = resolve
    }))
    const { loading, fetch } = usePagedList(mockFetch)

    const promise = fetch()
    expect(loading.value).toBe(true)

    resolvePromise({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, items: [], page: 1, pageSize: 10, totalCount: 0 })
    await promise
    expect(loading.value).toBe(false)
  })
})
