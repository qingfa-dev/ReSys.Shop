import { describe, it, expect, vi } from 'vitest'
import { usePagedList } from '../usePagedList'
import type { PagedResult, QueryingModel } from '@/shared/models'

type MockItem = { id: string; name: string }

describe('usePagedList', () => {
  const mockItems: MockItem[] = [{ id: '1', name: 'Item 1' }, { id: '2', name: 'Item 2' }]

  function createMockFetch(result: Partial<PagedResult<MockItem>> = {}) {
    return vi.fn<(params: QueryingModel) => Promise<PagedResult<MockItem>>>().mockResolvedValue({
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
    } as PagedResult<MockItem>)
  }

  it('should initialize with default state', () => {
    const { items, loading, error, totalRecords } = usePagedList(createMockFetch())
    expect(items.value).toEqual([])
    expect(loading.value).toBe(false)
    expect(error.value).toBeNull()
    expect(totalRecords.value).toBe(0)
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

  it('should handle empty data response', async () => {
    const mockFetch = vi.fn<(params: QueryingModel) => Promise<PagedResult<MockItem>>>().mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, items: [], page: 1, pageSize: 10, totalCount: 0 })
    const { items, totalRecords, fetch } = usePagedList(mockFetch)

    await fetch()

    expect(items.value).toEqual([])
    expect(totalRecords.value).toBe(0)
  })

  it('loading should be true during fetch', async () => {
    let resolvePromise!: (value: PagedResult<MockItem> | PromiseLike<PagedResult<MockItem>>) => void
    const mockFetch = vi.fn<(params: QueryingModel) => Promise<PagedResult<MockItem>>>().mockImplementation(() => new Promise(resolve => {
      resolvePromise = resolve
    }))
    const { loading, fetch } = usePagedList(mockFetch)

    const promise = fetch()
    expect(loading.value).toBe(true)

    resolvePromise({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, items: [], page: 1, pageSize: 10, totalCount: 0 })
    await promise
    expect(loading.value).toBe(false)
  })

  it('refresh should re-fetch with same params', async () => {
    const mockFetch = createMockFetch()
    const { refresh, fetch } = usePagedList(mockFetch)

    await fetch()
    expect(mockFetch).toHaveBeenCalledTimes(1)

    await refresh()
    expect(mockFetch).toHaveBeenCalledTimes(2)
  })

  it('params returns default QueryingModel', () => {
    const { params } = usePagedList(vi.fn())
    expect(params).toBeDefined()
    expect(typeof params.value.page.page).toBe('number')
  })

  it('handles Result<T[]> instead of PagedResult', async () => {
    const fetchFn = vi.fn().mockResolvedValue({ isSuccess: true, value: [{ id: '1' }] })
    const { items, totalRecords, fetch } = usePagedList(fetchFn)
    await fetch()
    expect(items.value).toHaveLength(1)
    expect(totalRecords.value).toBe(1)
  })

  it('sets error when API returns isSuccess: false with errors', async () => {
    const fetchFn = vi.fn().mockResolvedValue({ isSuccess: false, errors: [{ message: 'Not found' }] })
    const { error, fetch } = usePagedList(fetchFn)
    await fetch()
    expect(error.value).toBe('Not found')
  })

  it('sets generic error on catch', async () => {
    const fetchFn = vi.fn().mockRejectedValue(new Error('Network'))
    const { error, fetch } = usePagedList(fetchFn)
    await fetch()
    expect(error.value).toBe('An unexpected error occurred')
  })
})
