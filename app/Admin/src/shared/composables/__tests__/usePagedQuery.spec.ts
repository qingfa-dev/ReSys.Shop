import { describe, it, expect, vi, beforeEach } from 'vitest'
import { usePagedQuery } from '../usePagedQuery'
import type { PagedResult } from '@/shared/types/result'

const { mockGetPaged } = vi.hoisted(() => ({ mockGetPaged: vi.fn<(...args: unknown[]) => unknown>() }))

vi.mock('../../api', () => ({
  getPaged: mockGetPaged,
}))

function okResult(overrides: Partial<PagedResult<{ id: string; name: string }>> = {}): PagedResult<{ id: string; name: string }> {
  return {
    isSuccess: true,
    statusCode: 200,
    items: [{ id: '1', name: 'Test' }],
    page: 1,
    pageSize: 20,
    totalCount: 1,
    totalPages: 1,
    errors: [],
    message: null,
    metadata: null,
    ...overrides,
  }
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('usePagedQuery', () => {
  it('fetches on mount by default', async () => {
    mockGetPaged.mockResolvedValue(okResult())
    const { items } = usePagedQuery<{ id: string; name: string }>('/api/products')

    await vi.waitFor(() => {
      expect(items.value).toHaveLength(1)
      expect(items.value[0]!.name).toBe('Test')
    })
  })

  it('does not fetch on mount when immediate is false', () => {
    const { items } = usePagedQuery<{ id: string; name: string }>('/api/products', { immediate: false })

    expect(items.value).toHaveLength(0)
    expect(mockGetPaged).not.toHaveBeenCalled()
  })

  it('setPage fetches with new page number', async () => {
    mockGetPaged.mockResolvedValue(okResult({ page: 2 }))
    const { page, setPage } = usePagedQuery<{ id: string; name: string }>('/api/products', { immediate: false })

    setPage(2)
    await vi.waitFor(() => {
      expect(page.value).toBe(2)
    })

    expect(mockGetPaged).toHaveBeenCalledWith(
      '/api/products',
      expect.objectContaining({ pageNumber: 2 }),
      expect.any(Object),
    )
  })

  it('setPage does not go below 1', () => {
    const { page, setPage } = usePagedQuery<{ id: string; name: string }>('/api/products', { immediate: false })

    setPage(0)
    expect(page.value).toBe(1)
  })

  it('setFilter resets page to 1 and refetches', async () => {
    mockGetPaged.mockResolvedValue(okResult())
    const { filter, page, setFilter } = usePagedQuery<{ id: string; name: string }>('/api/products', { immediate: false })

    setFilter('name=bolt')
    await vi.waitFor(() => {
      expect(filter.value).toBe('name=bolt')
      expect(page.value).toBe(1)
    })
  })

  it('nextPage increments page', async () => {
    mockGetPaged.mockResolvedValue(okResult({ totalCount: 50, pageSize: 10, totalPages: 5 }))
    const { nextPage, page, fetch } = usePagedQuery<{ id: string; name: string }>('/api/products', { immediate: false })

    await fetch()
    nextPage()
    expect(page.value).toBe(2)
  })

  it('prevPage decrements page', () => {
    const { prevPage, page } = usePagedQuery<{ id: string; name: string }>('/api/products', { immediate: false })

    page.value = 3
    prevPage()
    expect(page.value).toBe(2)
  })

  it('prevPage does not go below 1', () => {
    const { prevPage, page } = usePagedQuery<{ id: string; name: string }>('/api/products', { immediate: false })

    page.value = 1
    prevPage()
    expect(page.value).toBe(1)
  })

  it('reset clears all state', async () => {
    mockGetPaged.mockResolvedValue(okResult())
    const { items, filter, sort, search, page, pageSize, error: _error, reset } = usePagedQuery<{ id: string; name: string }>('/api/products', { immediate: false })

    filter.value = 'name=bolt'
    sort.value = ['-createdAt']
    search.value = 'test'
    page.value = 3
    pageSize.value = 50

    reset()

    expect(filter.value).toBe('')
    expect(sort.value).toEqual([])
    expect(search.value).toBe('')
    expect(page.value).toBe(1)
    expect(pageSize.value).toBe(20)
    expect(items.value).toEqual([])
  })

  it('tracks error on failure', async () => {
    mockGetPaged.mockResolvedValue({
      isSuccess: false,
      statusCode: 422,
      items: [],
      page: 1,
      pageSize: 20,
      totalCount: 0,
      totalPages: 0,
      errors: [{ code: 'Validation', message: 'Invalid filter' }],
      message: 'Invalid filter',
      metadata: null,
    })

    const { error } = usePagedQuery<{ id: string }>('/api/products')

    await vi.waitFor(() => {
      expect(error.value).toBe('Invalid filter')
    })
  })

  it('accepts a function returning URL', async () => {
    mockGetPaged.mockResolvedValue(okResult())
    const basePath = '/api/v1'
    const { items } = usePagedQuery<{ id: string }>(() => `${basePath}/products`)

    await vi.waitFor(() => {
      expect(items.value).toHaveLength(1)
    })
  })

  it('passes allowedSearchFields to getPaged', async () => {
    mockGetPaged.mockResolvedValue(okResult())

    const { fetch } = usePagedQuery<{ id: string }>('/api/products', {
      allowedFilterFields: ['name'],
      allowedSortFields: ['name', 'createdAt'],
      allowedSearchFields: ['name', 'description'],
      defaultSearchFields: ['name', 'description'],
      defaultSearchMode: 'any',
      immediate: false,
    })

    await fetch()

    expect(mockGetPaged).toHaveBeenCalledWith(
      '/api/products',
      expect.objectContaining({
        pageNumber: 1,
        pageSize: 20,
        searchFields: ['name', 'description'],
        searchMode: 'any',
      }),
      {
        allowedFilterFields: ['name'],
        allowedSortFields: ['name', 'createdAt'],
        allowedSearchFields: ['name', 'description'],
      },
    )
  })
})
