import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { mockGetProfiles } = vi.hoisted(() => ({
  mockGetProfiles: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('../../services/profileApi', () => ({
  ProfileApi: {
    getProfiles: mockGetProfiles,
  },
}))

import { useProfileStore } from '../../stores/profileStore'

function profilesResult() {
  return {
    items: [{
      id: 'p-1',
      userId: 'u-1',
      firstName: 'A',
      lastName: 'B',
      email: 'a@b.com',
      fullName: 'A B',
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

describe('useProfileStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('fetchActive fetches active profiles and caches them', async () => {
    mockGetProfiles.mockResolvedValue(profilesResult())
    const store = useProfileStore()
    await store.fetchActive()
    expect(mockGetProfiles).toHaveBeenCalledWith({ pageSize: 100, sortBy: 'firstName', sortDirection: 'asc' })
    expect(store.activeProfiles).toHaveLength(1)
    expect(store.activeProfiles[0]?.id).toBe('p-1')
    expect(store.loaded).toBe(true)
  })

  it('fetchActive does not refetch after loaded', async () => {
    mockGetProfiles.mockResolvedValue(profilesResult())
    const store = useProfileStore()
    await store.fetchActive()
    await store.fetchActive()
    expect(mockGetProfiles).toHaveBeenCalledTimes(1)
  })
})
