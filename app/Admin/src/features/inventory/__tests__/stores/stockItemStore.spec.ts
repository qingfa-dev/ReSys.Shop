import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { mockGetStockItems } = vi.hoisted(() => ({
  mockGetStockItems: vi.fn<any>(),
}))

vi.mock('../../services/stockItemApi', () => ({
  StockItemApi: {
    getStockItems: mockGetStockItems,
  },
}))

import { useStockItemStore } from '../../stores/stockItemStore'

function stockItemsResult() {
  return {
    items: [{
      id: 's-1',
      stockLocationId: 'l-1',
      variantId: 'v-1',
      countOnHand: 10,
      backorderable: true,
      createdAtUtc: '2026-01-01T00:00:00Z',
      modifiedAtUtc: null,
      createdBy: null,
      modifiedBy: null,
    }],
    page: 1,
    pageSize: 100,
    totalCount: 1,
    totalPages: 1,
    isSuccess: true,
    statusCode: 200,
    message: null,
    errors: [],
    metadata: null,
  }
}

describe('useStockItemStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('fetchActive fetches stock items and caches them', async () => {
    mockGetStockItems.mockResolvedValue(stockItemsResult())
    const store = useStockItemStore()
    await store.fetchActive()
    expect(mockGetStockItems).toHaveBeenCalledWith({ pageSize: 100 })
    expect(store.activeStockItems).toHaveLength(1)
    expect(store.activeStockItems[0]?.id).toBe('s-1')
    expect(store.loaded).toBe(true)
  })

  it('fetchActive does not refetch after loaded', async () => {
    mockGetStockItems.mockResolvedValue(stockItemsResult())
    const store = useStockItemStore()
    await store.fetchActive()
    await store.fetchActive()
    expect(mockGetStockItems).toHaveBeenCalledTimes(1)
  })
})
