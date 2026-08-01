import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { mockGetStockTransfers } = vi.hoisted(() => ({
  mockGetStockTransfers: vi.fn<any>(),
}))

vi.mock('../../services/stockTransferApi', () => ({
  StockTransferApi: {
    getStockTransfers: mockGetStockTransfers,
  },
}))

import { useStockTransferStore } from '../../stores/stockTransferStore'

function stockTransfersResult() {
  return {
    items: [{
      id: 't-1',
      number: 'TR-1',
      reference: null,
      sourceLocationId: 'l-1',
      destinationLocationId: 'l-2',
      state: 'InTransit',
      totalItems: 1,
      createdAtUtc: '2026-01-01T00:00:00Z',
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

describe('useStockTransferStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('fetchActive fetches stock transfers and caches them', async () => {
    mockGetStockTransfers.mockResolvedValue(stockTransfersResult())
    const store = useStockTransferStore()
    await store.fetchActive()
    expect(mockGetStockTransfers).toHaveBeenCalledWith({ pageSize: 100, sortBy: 'createdAtUtc', sortDirection: 'desc' })
    expect(store.activeStockTransfers).toHaveLength(1)
    expect(store.activeStockTransfers[0]?.id).toBe('t-1')
    expect(store.loaded).toBe(true)
  })

  it('fetchActive does not refetch after loaded', async () => {
    mockGetStockTransfers.mockResolvedValue(stockTransfersResult())
    const store = useStockTransferStore()
    await store.fetchActive()
    await store.fetchActive()
    expect(mockGetStockTransfers).toHaveBeenCalledTimes(1)
  })
})
