import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockPut, mockPatch, mockDel, mockDelWithBody, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<(...args: unknown[]) => unknown>(),
  mockGet: vi.fn<(...args: unknown[]) => unknown>(),
  mockPut: vi.fn<(...args: unknown[]) => unknown>(),
  mockPatch: vi.fn<(...args: unknown[]) => unknown>(),
  mockDel: vi.fn<(...args: unknown[]) => unknown>(),
  mockDelWithBody: vi.fn<(...args: unknown[]) => unknown>(),
  mockGetPaged: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  get: mockGet,
  put: mockPut,
  patch: mockPatch,
  del: mockDel,
  delWithBody: mockDelWithBody,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { RoleApi } from '../../services/roleApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('RoleApi.getRoles', () => {
  it('calls getPaged with role query params and allowed fields', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 10,
      totalCount: 0,
      totalPages: 0,
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
    })

    await RoleApi.getRoles({ search: 'Admin', searchFields: ['name'], pageNumber: 1, pageSize: 10 })

    expect(mockGetPaged).toHaveBeenCalledWith(
      '/api/admin/identity/roles',
      { search: 'Admin', searchFields: ['name'], pageNumber: 1, pageSize: 10 },
      expect.objectContaining({ allowedFilterFields: expect.any(Array) }),
    )
  })
})

describe('RoleApi.getRole', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: 'r-1', name: 'Admin', isSystem: true }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await RoleApi.getRole('r-1')
    expect(mockGet).toHaveBeenCalledWith('/api/admin/identity/roles/r-1')
  })
})

describe('RoleApi.createRole', () => {
  it('calls POST with request body', async () => {
    const req = { name: 'Admin', description: 'Administrator role', presentation: 'Admin' }
    mockPost.mockResolvedValue({ value: { id: 'r-1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await RoleApi.createRole(req)
    expect(mockPost).toHaveBeenCalledWith('/api/admin/identity/roles', req)
  })
})

describe('RoleApi.updateRole', () => {
  it('calls PUT with request body', async () => {
    const req = { name: 'Admin', description: 'Administrator role', presentation: 'Admin' }
    mockPut.mockResolvedValue({ value: { id: 'r-1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await RoleApi.updateRole('r-1', req)
    expect(mockPut).toHaveBeenCalledWith('/api/admin/identity/roles/r-1', req)
  })
})

describe('RoleApi.deleteRole', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: null, isSuccess: true, statusCode: 204, message: null, errors: [], metadata: null })
    await RoleApi.deleteRole('r-1')
    expect(mockDel).toHaveBeenCalledWith('/api/admin/identity/roles/r-1')
  })
})

describe('RoleApi.assignPermissions', () => {
  it('calls PUT with assign URL and permissions body', async () => {
    mockPut.mockResolvedValue({ value: null, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await RoleApi.assignPermissions('r-1', ['a.b.c.view'])
    expect(mockPut).toHaveBeenCalledWith('/api/admin/identity/roles/r-1/permissions/assign', { permissions: ['a.b.c.view'] })
  })
})

describe('RoleApi.getPermissions', () => {
  it('calls GET with permissions URL', async () => {
    mockGet.mockResolvedValue({ value: { categories: [] }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await RoleApi.getPermissions('r-1')
    expect(mockGet).toHaveBeenCalledWith('/api/admin/identity/roles/r-1/permissions')
  })
})

describe('RoleApi.revokePermissions', () => {
  it('calls delWithBody with revoke URL and permissions body', async () => {
    mockDelWithBody.mockResolvedValue({ value: null, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await RoleApi.revokePermissions('r-1', ['a.b.c.view'])
    expect(mockDelWithBody).toHaveBeenCalledWith('/api/admin/identity/roles/r-1/permissions/revoke', { permissions: ['a.b.c.view'] })
    expect(mockDel).not.toHaveBeenCalled()
  })
})

describe('RoleApi.syncPermissions', () => {
  it('calls PATCH with sync URL and permissions body', async () => {
    mockPatch.mockResolvedValue({ value: null, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await RoleApi.syncPermissions('r-1', ['a.b.c.view'])
    expect(mockPatch).toHaveBeenCalledWith('/api/admin/identity/roles/r-1/permissions/sync', { permissions: ['a.b.c.view'] })
  })
})