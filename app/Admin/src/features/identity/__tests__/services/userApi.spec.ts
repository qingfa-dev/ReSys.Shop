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

import { UserApi } from '../../services/userApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('UserApi.getUsers', () => {
  it('calls getPaged with user query params and allowed fields', async () => {
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

    await UserApi.getUsers({ filter: 'isActive=true', pageNumber: 1, pageSize: 10 })

    expect(mockGetPaged).toHaveBeenCalledWith(
      '/api/admin/identity/users',
      { filter: 'isActive=true', pageNumber: 1, pageSize: 10 },
      expect.objectContaining({ allowedFilterFields: expect.any(Array) }),
    )
  })
})

describe('UserApi.getUser', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: 'u-1', email: 'a@b.com', userName: 'admin' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await UserApi.getUser('u-1')
    expect(mockGet).toHaveBeenCalledWith('/api/admin/identity/users/u-1')
  })
})

describe('UserApi.createUser', () => {
  it('calls POST with request body', async () => {
    const req = {
      email: 'a@b.com',
      userName: 'admin',
      firstName: 'A',
      lastName: 'B',
      phoneNumber: '123',
      emailConfirmed: true,
      phoneNumberConfirmed: false,
    }
    mockPost.mockResolvedValue({ value: { id: 'u-1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await UserApi.createUser(req)
    expect(mockPost).toHaveBeenCalledWith('/api/admin/identity/users', req)
  })
})

describe('UserApi.updateUser', () => {
  it('calls PUT with request body', async () => {
    const req = {
      email: 'a@b.com',
      userName: 'admin',
      firstName: 'A',
      lastName: 'B',
      phoneNumber: '123',
      emailConfirmed: true,
      phoneNumberConfirmed: false,
    }
    mockPut.mockResolvedValue({ value: { id: 'u-1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await UserApi.updateUser('u-1', req)
    expect(mockPut).toHaveBeenCalledWith('/api/admin/identity/users/u-1', req)
  })
})

describe('UserApi.deleteUser', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: null, isSuccess: true, statusCode: 204, message: null, errors: [], metadata: null })
    await UserApi.deleteUser('u-1')
    expect(mockDel).toHaveBeenCalledWith('/api/admin/identity/users/u-1')
  })
})

describe('UserApi.toggleStatus', () => {
  it('calls PATCH with status URL and no body', async () => {
    mockPatch.mockResolvedValue({ value: null, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await UserApi.toggleStatus('u-1')
    expect(mockPatch).toHaveBeenCalledWith('/api/admin/identity/users/u-1/status')
  })
})

describe('UserApi.getRoles', () => {
  it('calls getPaged with roles URL and default page values', async () => {
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
    await UserApi.getRoles('u-1')
    expect(mockGetPaged).toHaveBeenCalledWith(
      '/api/admin/identity/users/u-1/roles',
      { pageNumber: 1, pageSize: 100 },
    )
  })
})

describe('UserApi.assignRoles', () => {
  it('calls POST with assign URL and roles body', async () => {
    mockPost.mockResolvedValue({ value: null, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await UserApi.assignRoles('u-1', ['Admin'])
    expect(mockPost).toHaveBeenCalledWith('/api/admin/identity/users/u-1/roles/assign', { roles: ['Admin'] })
  })
})

describe('UserApi.revokeRoles', () => {
  it('calls POST with revoke URL and roles body', async () => {
    mockPost.mockResolvedValue({ value: null, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await UserApi.revokeRoles('u-1', ['Admin'])
    expect(mockPost).toHaveBeenCalledWith('/api/admin/identity/users/u-1/roles/revoke', { roles: ['Admin'] })
  })
})

describe('UserApi.syncRoles', () => {
  it('calls PATCH with sync URL and roles body', async () => {
    mockPatch.mockResolvedValue({ value: null, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await UserApi.syncRoles('u-1', ['Admin'])
    expect(mockPatch).toHaveBeenCalledWith('/api/admin/identity/users/u-1/roles/sync', { roles: ['Admin'] })
  })
})

describe('UserApi.getPermissions', () => {
  it('calls GET with permissions URL', async () => {
    mockGet.mockResolvedValue({ value: { categories: [] }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await UserApi.getPermissions('u-1')
    expect(mockGet).toHaveBeenCalledWith('/api/admin/identity/users/u-1/permissions')
  })
})

describe('UserApi.assignPermissions', () => {
  it('calls POST with assign URL and permissions body', async () => {
    mockPost.mockResolvedValue({ value: null, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await UserApi.assignPermissions('u-1', ['a.b.c.view'])
    expect(mockPost).toHaveBeenCalledWith('/api/admin/identity/users/u-1/permissions/assign', { permissions: ['a.b.c.view'] })
  })
})

describe('UserApi.revokePermissions', () => {
  it('calls delWithBody with revoke URL and permissions body', async () => {
    mockDelWithBody.mockResolvedValue({ value: null, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await UserApi.revokePermissions('u-1', ['a.b.c.view'])
    expect(mockDelWithBody).toHaveBeenCalledWith('/api/admin/identity/users/u-1/permissions/revoke', { permissions: ['a.b.c.view'] })
    expect(mockDel).not.toHaveBeenCalled()
  })
})

describe('UserApi.syncPermissions', () => {
  it('calls PUT with sync URL and permissions body', async () => {
    mockPut.mockResolvedValue({ value: null, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await UserApi.syncPermissions('u-1', ['a.b.c.view'])
    expect(mockPut).toHaveBeenCalledWith('/api/admin/identity/users/u-1/permissions/sync', { permissions: ['a.b.c.view'] })
  })
})