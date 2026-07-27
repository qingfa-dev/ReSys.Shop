import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useUserStore } from '../user.store'
import { UserApi, UserRoleApi, UserPermissionApi } from '../../api'
import type { UserResponse, CreateUserRequest, UpdateUserRequest } from '../../types'
import type { Result } from '@/shared/models'

const mockGetMany = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockGet = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockCreate = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockUpdate = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockDelete = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockToggleStatus = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockAssignRole = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockRevokeRole = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockAssignPermission = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockRevokePermission = vi.hoisted(() => vi.fn<(...args: any[]) => any>())

vi.mock('../../api', () => ({
  UserApi: {
    getMany: mockGetMany,
    get: mockGet,
    create: mockCreate,
    update: mockUpdate,
    delete: mockDelete,
    toggleStatus: mockToggleStatus,
  },
  UserRoleApi: {
    assign: mockAssignRole,
    revoke: mockRevokeRole,
  },
  UserPermissionApi: {
    assign: mockAssignPermission,
    revoke: mockRevokePermission,
  },
}))

const mockUser: UserResponse = {
  id: '1',
  email: 'john@example.com',
  userName: 'john',
  firstName: 'John',
  lastName: 'Doe',
  phone: null,
  isActive: true,
  roles: [],
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z',
}

function pagedResult(overrides: Partial<{ items: UserResponse[]; totalCount: number }> = {}) {
  return {
    isSuccess: true,
    statusCode: 200,
    items: overrides.items ?? [],
    page: 1,
    pageSize: 20,
    totalCount: overrides.totalCount ?? 0,
    errors: [],
    message: null,
    metadata: null,
  }
}

function successResult<T>(value: T): Result<T> {
  return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value }
}

function errorResult(message = 'Something went wrong'): Result<any> {
  return { isSuccess: false, statusCode: 400, errors: [], message, metadata: null, value: null }
}

const createPayload: CreateUserRequest = {
  email: 'john@example.com',
  userName: 'john',
  password: 'Pass123!',
  firstName: 'John',
  lastName: 'Doe',
}

const updatePayload: UpdateUserRequest = {
  email: 'john.updated@example.com',
  firstName: 'John',
  lastName: 'Doe Updated',
}

describe('useUserStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('initial state', () => {
    it('has initial state', () => {
      const store = useUserStore()
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
      expect(store.query.page).toBe(1)
    })
  })

  describe('fetchMany', () => {
    it('success', async () => {
      mockGetMany.mockResolvedValue(pagedResult({ items: [mockUser], totalCount: 1 }))
      const store = useUserStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.items).toHaveLength(1)
      expect(store.totalRecords).toBe(1)
      expect(store.error).toBeNull()
    })

    it('failure', async () => {
      mockGetMany.mockResolvedValue({ ...pagedResult(), isSuccess: false, message: 'Server error' })
      const store = useUserStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.error).toBe('Server error')
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
    })

    it('network error', async () => {
      mockGetMany.mockRejectedValue(new Error('Network'))
      const store = useUserStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.error).toBe('Failed to load')
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
    })
  })

  describe('getById', () => {
    it('success', async () => {
      mockGet.mockResolvedValue(successResult(mockUser))
      const store = useUserStore()
      const result = await store.getById('1')
      expect(result.isSuccess).toBe(true)
      expect(result.value).toEqual(mockUser)
      expect(UserApi.get).toHaveBeenCalledWith('1')
    })

    it('failure', async () => {
      mockGet.mockResolvedValue(errorResult('Not found'))
      const store = useUserStore()
      const result = await store.getById('2')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Not found')
      expect(store.error).toBe('Not found')
    })

    it('network error', async () => {
      mockGet.mockRejectedValue(new Error('Network'))
      const store = useUserStore()
      const result = await store.getById('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to load')
      expect(store.error).toBe('Failed to load')
    })
  })

  describe('create', () => {
    it('success', async () => {
      mockCreate.mockResolvedValue(successResult(mockUser))
      const store = useUserStore()
      const result = await store.create(createPayload)
      expect(result.isSuccess).toBe(true)
      expect(result.value).toEqual(mockUser)
      expect(UserApi.create).toHaveBeenCalledWith(createPayload)
    })

    it('failure', async () => {
      mockCreate.mockResolvedValue(errorResult('Validation failed'))
      const store = useUserStore()
      const result = await store.create(createPayload)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Validation failed')
      expect(store.error).toBe('Validation failed')
    })

    it('network error', async () => {
      mockCreate.mockRejectedValue(new Error('Network'))
      const store = useUserStore()
      const result = await store.create(createPayload)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to create')
      expect(store.error).toBe('Failed to create')
    })
  })

  describe('update', () => {
    it('success', async () => {
      mockUpdate.mockResolvedValue(successResult(mockUser))
      const store = useUserStore()
      const result = await store.update('1', updatePayload)
      expect(result.isSuccess).toBe(true)
      expect(UserApi.update).toHaveBeenCalledWith('1', updatePayload)
    })

    it('failure', async () => {
      mockUpdate.mockResolvedValue(errorResult('Not found'))
      const store = useUserStore()
      const result = await store.update('1', updatePayload)
      expect(result.isSuccess).toBe(false)
      expect(store.error).toBe('Not found')
    })

    it('network error', async () => {
      mockUpdate.mockRejectedValue(new Error('Network'))
      const store = useUserStore()
      const result = await store.update('1', updatePayload)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to update')
    })
  })

  describe('delete', () => {
    it('success', async () => {
      mockDelete.mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null })
      const store = useUserStore()
      const result = await store.delete('1')
      expect(result.isSuccess).toBe(true)
      expect(UserApi.delete).toHaveBeenCalledWith('1')
    })

    it('failure', async () => {
      mockDelete.mockResolvedValue(errorResult('Cannot delete'))
      const store = useUserStore()
      const result = await store.delete('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Cannot delete')
    })

    it('network error', async () => {
      mockDelete.mockRejectedValue(new Error('Network'))
      const store = useUserStore()
      const result = await store.delete('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to delete')
    })
  })

  describe('toggleStatus', () => {
    it('success', async () => {
      mockToggleStatus.mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null })
      const store = useUserStore()
      const result = await store.toggleStatus('1', false)
      expect(result.isSuccess).toBe(true)
      expect(UserApi.toggleStatus).toHaveBeenCalledWith('1', { isActive: false })
    })

    it('failure', async () => {
      mockToggleStatus.mockResolvedValue(errorResult('Cannot toggle'))
      const store = useUserStore()
      const result = await store.toggleStatus('1', false)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Cannot toggle')
    })

    it('network error', async () => {
      mockToggleStatus.mockRejectedValue(new Error('Network'))
      const store = useUserStore()
      const result = await store.toggleStatus('1', false)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to toggle status')
    })
  })

  describe('assignRole', () => {
    it('success', async () => {
      mockAssignRole.mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null })
      const store = useUserStore()
      const result = await store.assignRole('user-1', 'role-admin')
      expect(result.isSuccess).toBe(true)
      expect(UserRoleApi.assign).toHaveBeenCalledWith('user-1', { items: [{ roleId: 'role-admin' }] })
    })

    it('failure', async () => {
      mockAssignRole.mockResolvedValue(errorResult('Assign failed'))
      const store = useUserStore()
      const result = await store.assignRole('user-1', 'role-admin')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Assign failed')
    })

    it('network error', async () => {
      mockAssignRole.mockRejectedValue(new Error('Network'))
      const store = useUserStore()
      const result = await store.assignRole('user-1', 'role-admin')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to assign role')
    })
  })

  describe('revokeRole', () => {
    it('success', async () => {
      mockRevokeRole.mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null })
      const store = useUserStore()
      const result = await store.revokeRole('user-1', 'role-admin')
      expect(result.isSuccess).toBe(true)
      expect(UserRoleApi.revoke).toHaveBeenCalledWith('user-1', { items: [{ roleId: 'role-admin' }] })
    })

    it('failure', async () => {
      mockRevokeRole.mockResolvedValue(errorResult('Revoke failed'))
      const store = useUserStore()
      const result = await store.revokeRole('user-1', 'role-admin')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Revoke failed')
    })

    it('network error', async () => {
      mockRevokeRole.mockRejectedValue(new Error('Network'))
      const store = useUserStore()
      const result = await store.revokeRole('user-1', 'role-admin')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to revoke role')
    })
  })

  describe('assignPermission', () => {
    it('success', async () => {
      mockAssignPermission.mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null })
      const store = useUserStore()
      const result = await store.assignPermission('user-1', 'perm-read')
      expect(result.isSuccess).toBe(true)
      expect(UserPermissionApi.assign).toHaveBeenCalledWith('user-1', { items: [{ permissionId: 'perm-read' }] })
    })

    it('failure', async () => {
      mockAssignPermission.mockResolvedValue(errorResult('Assign failed'))
      const store = useUserStore()
      const result = await store.assignPermission('user-1', 'perm-read')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Assign failed')
    })

    it('network error', async () => {
      mockAssignPermission.mockRejectedValue(new Error('Network'))
      const store = useUserStore()
      const result = await store.assignPermission('user-1', 'perm-read')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to assign permission')
    })
  })

  describe('revokePermission', () => {
    it('success', async () => {
      mockRevokePermission.mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null })
      const store = useUserStore()
      const result = await store.revokePermission('user-1', 'perm-read')
      expect(result.isSuccess).toBe(true)
      expect(UserPermissionApi.revoke).toHaveBeenCalledWith('user-1', { items: [{ permissionId: 'perm-read' }] })
    })

    it('failure', async () => {
      mockRevokePermission.mockResolvedValue(errorResult('Revoke failed'))
      const store = useUserStore()
      const result = await store.revokePermission('user-1', 'perm-read')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Revoke failed')
    })

    it('network error', async () => {
      mockRevokePermission.mockRejectedValue(new Error('Network'))
      const store = useUserStore()
      const result = await store.revokePermission('user-1', 'perm-read')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to revoke permission')
    })
  })

  describe('listing helpers', () => {
    it('setPage updates query and re-fetches', async () => {
      mockGetMany.mockResolvedValue(pagedResult({ totalCount: 1 }))
      const store = useUserStore()
      await store.setPage(3)
      expect(store.query.page).toBe(3)
      expect(mockGetMany).toHaveBeenCalled()
    })

    it('setSort updates sort clause', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useUserStore()
      await store.setSort('email', 'Ascending')
      expect(store.query.sort).toEqual([{ field: 'email', direction: 'Ascending' }])
    })

    it('setSearch sets search and resets page', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useUserStore()
      await store.setPage(3)
      await store.setSearch('john')
      expect(store.query.search).toEqual({ value: 'john', mode: 'Any' })
      expect(store.query.page).toBe(1)
    })

    it('setFilter sets filter group and resets page', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useUserStore()
      await store.setFilter({ logic: 'And', conditions: [{ field: 'isActive', operator: 'Equal', value: 'true' }], groups: [] })
      expect(mockGetMany).toHaveBeenCalled()
      expect(store.query.filters).toEqual({ logic: 'And', conditions: [{ field: 'isActive', operator: 'Equal', value: 'true' }], groups: [] })
      expect(store.query.page).toBe(1)
    })

    it('resetQuery restores defaults', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useUserStore()
      await store.setPage(5)
      await store.resetQuery()
      expect(store.query.page).toBe(1)
      expect(store.query.search).toBeUndefined()
      expect(store.query.sort).toEqual([{ field: 'createdAt', direction: 'Descending' }])
      expect(mockGetMany).toHaveBeenCalled()
    })
  })

  describe('loading state', () => {
    it('loading is true during fetchMany', async () => {
      let resolver!: (value: unknown) => void
      mockGetMany.mockImplementation(() => new Promise(resolve => { resolver = resolve }))
      const store = useUserStore()
      const promise = store.fetchMany()
      expect(store.loading).toBe(true)
      resolver(pagedResult())
      await promise
    })

    it('loading is true during getById', async () => {
      let resolver!: (value: unknown) => void
      mockGet.mockImplementation(() => new Promise(resolve => { resolver = resolve }))
      const store = useUserStore()
      const promise = store.getById('1')
      expect(store.loading).toBe(true)
      resolver(successResult(mockUser))
      await promise
    })

    it('loading is true during toggleStatus', async () => {
      let resolver!: (value: unknown) => void
      mockToggleStatus.mockImplementation(() => new Promise(resolve => { resolver = resolve }))
      const store = useUserStore()
      const promise = store.toggleStatus('1', false)
      expect(store.loading).toBe(true)
      resolver({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null })
      await promise
    })
  })
})
