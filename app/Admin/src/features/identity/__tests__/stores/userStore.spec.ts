import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { mockGetUsers } = vi.hoisted(() => ({
  mockGetUsers: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('../../services/userApi', () => ({
  UserApi: {
    getUsers: mockGetUsers,
  },
}))

import { useUserStore } from '../../stores/userStore'

function usersResult() {
  return {
    items: [{ id: 'u-1', email: 'a@b.com', userName: 'admin', firstName: 'A', lastName: 'B', fullName: 'A B', isActive: true }],
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

describe('useUserStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('fetchActive fetches users and caches them', async () => {
    mockGetUsers.mockResolvedValue(usersResult())
    const store = useUserStore()
    await store.fetchActive()
    expect(mockGetUsers).toHaveBeenCalledWith({ pageSize: 100, sortBy: 'userName', sortDirection: 'asc' })
    expect(store.activeUsers).toHaveLength(1)
    expect(store.activeUsers[0]?.userName).toBe('admin')
    expect(store.loaded).toBe(true)
  })

  it('fetchActive does not refetch after loaded', async () => {
    mockGetUsers.mockResolvedValue(usersResult())
    const store = useUserStore()
    await store.fetchActive()
    await store.fetchActive()
    expect(mockGetUsers).toHaveBeenCalledTimes(1)
  })
})
