import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { mockGetShippingRates } = vi.hoisted(() => ({
  mockGetShippingRates: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('../../services/shippingRateApi', () => ({
  ShippingRateApi: {
    getShippingRates: mockGetShippingRates,
  },
}))

import { useShippingRateStore } from '../../stores/shippingRateStore'

function shippingRatesResult() {
  return {
    items: [{
      id: 'sr-1',
      name: 'Standard',
      cost: 5,
      shippingMethodId: 'sm-1',
      finalPrice: 5,
      selected: false,
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

describe('useShippingRateStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('fetchActive fetches shipping rates and caches them', async () => {
    mockGetShippingRates.mockResolvedValue(shippingRatesResult())
    const store = useShippingRateStore()
    await store.fetchActive()
    expect(mockGetShippingRates).toHaveBeenCalledWith({ pageSize: 100, sortBy: 'name', sortDirection: 'asc' })
    expect(store.activeShippingRates).toHaveLength(1)
    expect(store.activeShippingRates[0]?.id).toBe('sr-1')
    expect(store.loaded).toBe(true)
  })

  it('fetchActive does not refetch after loaded', async () => {
    mockGetShippingRates.mockResolvedValue(shippingRatesResult())
    const store = useShippingRateStore()
    await store.fetchActive()
    await store.fetchActive()
    expect(mockGetShippingRates).toHaveBeenCalledTimes(1)
  })
})
