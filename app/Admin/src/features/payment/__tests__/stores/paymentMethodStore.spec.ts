import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { mockGetPaymentMethods } = vi.hoisted(() => ({
  mockGetPaymentMethods: vi.fn<any>(),
}))

vi.mock('../../services/paymentMethodApi', () => ({
  PaymentMethodApi: {
    getPaymentMethods: mockGetPaymentMethods,
  },
}))

import { usePaymentMethodStore } from '../../stores/paymentMethodStore'

function paymentMethodsResult() {
  return {
    items: [{
      id: 'pm-1',
      name: 'Card',
      providerKey: 'stripe',
      webhookEnabled: true,
      autoCapture: true,
      displayOn: 'Both',
      position: 1,
      active: true,
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

describe('usePaymentMethodStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('fetchActive fetches payment methods and caches them', async () => {
    mockGetPaymentMethods.mockResolvedValue(paymentMethodsResult())
    const store = usePaymentMethodStore()
    await store.fetchActive()
    expect(mockGetPaymentMethods).toHaveBeenCalledWith({ pageSize: 100, sortBy: 'name', sortDirection: 'asc' })
    expect(store.activePaymentMethods).toHaveLength(1)
    expect(store.activePaymentMethods[0]?.id).toBe('pm-1')
    expect(store.loaded).toBe(true)
  })

  it('fetchActive does not refetch after loaded', async () => {
    mockGetPaymentMethods.mockResolvedValue(paymentMethodsResult())
    const store = usePaymentMethodStore()
    await store.fetchActive()
    await store.fetchActive()
    expect(mockGetPaymentMethods).toHaveBeenCalledTimes(1)
  })
})
