import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockGetPaged } = vi.hoisted(() => ({
  mockGetPaged: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { PermissionApi } from '../../services/permissionApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('PermissionApi.getPermissions', () => {
  it('calls getPaged with default page values', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 100,
      totalCount: 0,
      totalPages: 0,
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
    })

    await PermissionApi.getPermissions()

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/identity/permissions',
      { pageNumber: 1, pageSize: 100 },
    )
  })
})
