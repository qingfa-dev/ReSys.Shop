import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { mockGetStockLocations } = vi.hoisted(() => ({
  mockGetStockLocations: vi.fn<any>(),
}))

vi.mock('../../services/stockLocationApi', () => ({
  StockLocationApi: {
    getStockLocations: mockGetStockLocations,
  },
}))

import { useStockLocationStore } from '../../stores/stockLocationStore'

function stockLocationsResult() {
  return {
    items: [{
      id: 'l-1',
      name: 'Main',
      code: null,
      active: true,
      default: false,
      backorderableDefault: true,
      propagateAllVariants: true,
      position: 0,
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

describe('useStockLocationStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('fetchActive fetches stock locations and caches them', async () => {
    mockGetStockLocations.mockResolvedValue(stockLocationsResult())
    const store = useStockLocationStore()
    await store.fetchActive()
    expect(mockGetStockLocations).toHaveBeenCalledWith({ pageSize: 100, sortBy: 'name', sortDirection: 'asc' })
    expect(store.activeStockLocations).toHaveLength(1)
    expect(store.activeStockLocations[0]?.name).toBe('Main')
    expect(store.loaded).toBe(true)
  })

  it('fetchActive does not refetch after loaded', async () => {
    mockGetStockLocations.mockResolvedValue(stockLocationsResult())
    const store = useStockLocationStore()
    await store.fetchActive()
    await store.fetchActive()
    expect(mockGetStockLocations).toHaveBeenCalledTimes(1)
  })
})
