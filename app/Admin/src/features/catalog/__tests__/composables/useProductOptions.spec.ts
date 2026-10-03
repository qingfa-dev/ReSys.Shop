import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockGetProducts } = vi.hoisted(() => ({
  mockGetProducts: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('../../services/productApi', () => ({
  ProductApi: { getProducts: mockGetProducts },
}))

import { useProductOptions } from '../../composables/useProductOptions'

function pagedResult(items: unknown[] = []) {
  return {
    isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    items, page: 1, pageSize: 25, totalCount: items.length, totalPages: 0,
  }
}

beforeEach(() => {
  vi.clearAllMocks()
  vi.useRealTimers()
})

describe('useProductOptions', () => {
  it('loadInitial fetches first page without search', async () => {
    mockGetProducts.mockResolvedValue(pagedResult([{ id: 'p1', name: 'Shirt' }]))
    const { options, loadInitial } = useProductOptions()
    await loadInitial()
    expect(mockGetProducts).toHaveBeenCalledWith(
      expect.objectContaining({ search: '', pageNumber: 1, pageSize: 25, sort: ['name:asc'] }),
    )
    expect(options.value).toHaveLength(1)
  })

  it('searchProducts is debounced and fetches with search term', async () => {
    vi.useFakeTimers()
    mockGetProducts.mockResolvedValue(pagedResult([{ id: 'p1', name: 'Shirt' }]))
    const { options, searchProducts } = useProductOptions()
    searchProducts('shirt')
    vi.advanceTimersByTime(300)
    await vi.advanceTimersByTimeAsync(0)
    expect(mockGetProducts).toHaveBeenCalledWith(
      expect.objectContaining({ search: 'shirt', pageNumber: 1, pageSize: 25 }),
    )
    expect(options.value).toHaveLength(1)
  })

  it('resets loading when the API rejects', async () => {
    mockGetProducts.mockRejectedValue(new Error('network'))
    const { loading, loadInitial } = useProductOptions()
    await expect(loadInitial()).rejects.toThrow('network')
    expect(loading.value).toBe(false)
  })

  it('skips refetch when the same term was already loaded', async () => {
    vi.useFakeTimers()
    mockGetProducts.mockResolvedValue(pagedResult([{ id: 'p1', name: 'Shirt' }]))
    const { searchProducts } = useProductOptions()
    searchProducts('shirt')
    vi.advanceTimersByTime(300)
    await vi.advanceTimersByTimeAsync(0)
    expect(mockGetProducts).toHaveBeenCalledTimes(1)

    searchProducts('shirt')
    vi.advanceTimersByTime(300)
    await vi.advanceTimersByTimeAsync(0)
    expect(mockGetProducts).toHaveBeenCalledTimes(1)
  })

  it('ignores stale responses when a newer search is in flight', async () => {
    vi.useFakeTimers()
    let resolveSh!: (value: unknown) => void
    mockGetProducts.mockImplementation((query: unknown) => {
      const q = query as { search: string }
      if (q.search === 'sh') return new Promise((resolve) => { resolveSh = resolve })
      return Promise.resolve(pagedResult([{ id: 'p1', name: 'Shirt' }]))
    })
    const { options, searchProducts } = useProductOptions()
    searchProducts('sh')
    vi.advanceTimersByTime(300)
    await vi.advanceTimersByTimeAsync(0)
    searchProducts('shirt')
    vi.advanceTimersByTime(300)
    await vi.advanceTimersByTimeAsync(0)
    expect(options.value.map((o) => o.id)).toEqual(['p1'])

    resolveSh(pagedResult([{ id: 'slow', name: 'Sh' }]))
    await vi.advanceTimersByTimeAsync(0)
    expect(options.value.map((o) => o.id)).toEqual(['p1'])
  })
})
