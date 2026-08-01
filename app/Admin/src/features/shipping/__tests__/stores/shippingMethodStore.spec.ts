import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { mockGetShippingMethods } = vi.hoisted(() => ({
  mockGetShippingMethods: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('../../services/shippingMethodApi', () => ({
  ShippingMethodApi: {
    getShippingMethods: mockGetShippingMethods,
  },
}))

import { useShippingMethodStore } from '../../stores/shippingMethodStore'

function shippingMethodsResult() {
  return {
    items: [{
      id: 'sm-1',
      name: 'Express',
      position: 1,
      availableToUsers: true,
      calculatorType: 'FlatRate',
      createdAtUtc: '2026-01-01T00:00:00Z',
      isDeleted: false,
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

describe('useShippingMethodStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('fetchActive fetches shipping methods and caches them', async () => {
    mockGetShippingMethods.mockResolvedValue(shippingMethodsResult())
    const store = useShippingMethodStore()
    await store.fetchActive()
    expect(mockGetShippingMethods).toHaveBeenCalledWith({ pageSize: 100, sortBy: 'name', sortDirection: 'asc' })
    expect(store.activeShippingMethods).toHaveLength(1)
    expect(store.activeShippingMethods[0]?.id).toBe('sm-1')
    expect(store.loaded).toBe(true)
  })

  it('fetchActive does not refetch after loaded', async () => {
    mockGetShippingMethods.mockResolvedValue(shippingMethodsResult())
    const store = useShippingMethodStore()
    await store.fetchActive()
    await store.fetchActive()
    expect(mockGetShippingMethods).toHaveBeenCalledTimes(1)
  })
})
