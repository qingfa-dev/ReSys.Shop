import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { mockGetRoles } = vi.hoisted(() => ({
  mockGetRoles: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('../../services/roleApi', () => ({
  RoleApi: {
    getRoles: mockGetRoles,
  },
}))

import { useRoleStore } from '../../stores/roleStore'

function rolesResult() {
  return {
    items: [{ id: 'r-1', name: 'Admin', description: null, presentation: null, isSystem: true }],
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

describe('useRoleStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('fetchActive fetches roles and caches them', async () => {
    mockGetRoles.mockResolvedValue(rolesResult())
    const store = useRoleStore()
    await store.fetchActive()
    expect(mockGetRoles).toHaveBeenCalledWith({ pageSize: 100, sortBy: 'name', sortDirection: 'asc' })
    expect(store.activeRoles).toHaveLength(1)
    expect(store.activeRoles[0]?.name).toBe('Admin')
    expect(store.loaded).toBe(true)
  })

  it('fetchActive does not refetch after loaded', async () => {
    mockGetRoles.mockResolvedValue(rolesResult())
    const store = useRoleStore()
    await store.fetchActive()
    await store.fetchActive()
    expect(mockGetRoles).toHaveBeenCalledTimes(1)
  })
})
