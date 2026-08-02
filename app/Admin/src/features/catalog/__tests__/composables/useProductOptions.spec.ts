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
      expect.objectContaining({ search: '', page: 1, pageSize: 25, sortBy: 'name' }),
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
      expect.objectContaining({ search: 'shirt', page: 1, pageSize: 25 }),
    )
    expect(options.value).toHaveLength(1)
  })
})
