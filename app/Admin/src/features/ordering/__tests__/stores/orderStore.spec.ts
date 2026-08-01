import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { mockGetOrders } = vi.hoisted(() => ({
  mockGetOrders: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('../../services/orderApi', () => ({
  OrderApi: {
    getOrders: mockGetOrders,
  },
}))

import { useOrderStore } from '../../stores/orderStore'

function ordersResult() {
  return {
    items: [{
      id: 'o-1',
      number: 'ORD-1',
      status: 'Placed',
      total: 100,
      paymentTotal: 100,
      createdAtUtc: '2026-01-01T00:00:00Z',
      currency: 'USD',
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

describe('useOrderStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('fetchActive fetches orders and caches them', async () => {
    mockGetOrders.mockResolvedValue(ordersResult())
    const store = useOrderStore()
    await store.fetchActive()
    expect(mockGetOrders).toHaveBeenCalledWith({ pageSize: 100, sortBy: 'createdAtUtc', sortDirection: 'desc' })
    expect(store.activeOrders).toHaveLength(1)
    expect(store.activeOrders[0]?.id).toBe('o-1')
    expect(store.loaded).toBe(true)
  })

  it('fetchActive does not refetch after loaded', async () => {
    mockGetOrders.mockResolvedValue(ordersResult())
    const store = useOrderStore()
    await store.fetchActive()
    await store.fetchActive()
    expect(mockGetOrders).toHaveBeenCalledTimes(1)
  })
})
