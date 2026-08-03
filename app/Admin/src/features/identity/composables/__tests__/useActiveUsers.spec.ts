import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useActiveUsers } from '../useActiveUsers'
import { UserApi } from '../../services/userApi'
import type { PagedResult } from '@/shared/types/result'
import type { UserListItem } from '../../types/user'

vi.mock('../../services/userApi', () => ({
  UserApi: { getUsers: vi.fn<() => Promise<PagedResult<UserListItem>>>() },
}))

const mockGetUsers = vi.mocked(UserApi.getUsers)

function okResult(items: UserListItem[] = [{ id: 'u1', email: 'admin@shop.local', userName: 'admin', firstName: 'Admin', lastName: 'User', emailConfirmed: true, phoneNumberConfirmed: false, fullName: 'Admin User', isActive: true }]): PagedResult<UserListItem> {
  return { isSuccess: true, statusCode: 200, items, page: 1, pageSize: 20, totalCount: items.length, totalPages: 1, errors: [], message: null, metadata: null }
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useActiveUsers', () => {
  it('loads all users via the UserApi', async () => {
    mockGetUsers.mockResolvedValue(okResult())
    const { items, load } = useActiveUsers()

    await load()

    expect(mockGetUsers).toHaveBeenCalledWith({})
    expect(items.value).toHaveLength(1)
  })
})
