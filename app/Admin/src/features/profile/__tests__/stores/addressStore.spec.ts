import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { mockGetAddresses } = vi.hoisted(() => ({
  mockGetAddresses: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('../../services/addressApi', () => ({
  AddressApi: {
    getAddresses: mockGetAddresses,
  },
}))

import { useAddressStore } from '../../stores/addressStore'

function addressesResult() {
  return {
    items: [{
      id: 'a-1',
      userId: 'u-1',
      addressType: 'Shipping',
      firstName: 'A',
      address1: '1 Main St',
      city: 'Hanoi',
      isDefault: true,
      countryName: 'Vietnam',
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

describe('useAddressStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('fetchActive fetches active addresses with userId in the query object', async () => {
    mockGetAddresses.mockResolvedValue(addressesResult())
    const store = useAddressStore()
    await store.fetchActive('u-1')
    expect(mockGetAddresses).toHaveBeenCalledWith('u-1', { userId: 'u-1', pageSize: 100, sortBy: 'firstName', sortDirection: 'asc' })
    expect(store.activeAddresses).toHaveLength(1)
    expect(store.activeAddresses[0]?.id).toBe('a-1')
    expect(store.loaded).toBe(true)
  })

  it('fetchActive does not refetch after loaded', async () => {
    mockGetAddresses.mockResolvedValue(addressesResult())
    const store = useAddressStore()
    await store.fetchActive('u-1')
    await store.fetchActive('u-1')
    expect(mockGetAddresses).toHaveBeenCalledTimes(1)
  })
})
