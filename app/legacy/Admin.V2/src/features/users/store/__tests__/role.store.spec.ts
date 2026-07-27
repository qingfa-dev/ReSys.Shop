import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useRoleStore } from '../role.store'
import { RoleApi, RolePermissionApi } from '../../api'
import type { RoleResponse, CreateRoleRequest, UpdateRoleRequest } from '../../types'
import type { Result } from '@/shared/models'

const mockGetMany = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockGet = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockCreate = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockUpdate = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockDelete = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockAssignPermission = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockRevokePermission = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockSyncPermissions = vi.hoisted(() => vi.fn<(...args: any[]) => any>())

vi.mock('../../api', () => ({
  RoleApi: {
    getMany: mockGetMany,
    get: mockGet,
    create: mockCreate,
    update: mockUpdate,
    delete: mockDelete,
  },
  RolePermissionApi: {
    assign: mockAssignPermission,
    revoke: mockRevokePermission,
    sync: mockSyncPermissions,
  },
}))

const mockRole: RoleResponse = {
  id: '1',
  name: 'Admin',
  description: 'Administrator',
  isSystem: false,
  permissionCount: 5,
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z',
}

function pagedResult(overrides: Partial<{ items: RoleResponse[]; totalCount: number }> = {}) {
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

const createPayload: CreateRoleRequest = {
  name: 'Admin',
  description: 'Administrator',
}

const updatePayload: UpdateRoleRequest = {
  name: 'Admin Updated',
  description: 'Updated description',
}

describe('useRoleStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('initial state', () => {
    it('has initial state', () => {
      const store = useRoleStore()
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
      expect(store.query.page).toBe(1)
    })
  })

  describe('fetchMany', () => {
    it('success', async () => {
      mockGetMany.mockResolvedValue(pagedResult({ items: [mockRole], totalCount: 1 }))
      const store = useRoleStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.items).toHaveLength(1)
      expect(store.totalRecords).toBe(1)
      expect(store.error).toBeNull()
    })

    it('failure', async () => {
      mockGetMany.mockResolvedValue({ ...pagedResult(), isSuccess: false, message: 'Server error' })
      const store = useRoleStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.error).toBe('Server error')
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
    })

    it('network error', async () => {
      mockGetMany.mockRejectedValue(new Error('Network'))
      const store = useRoleStore()
      await store.fetchMany()
      expect(store.loading).toBe(false)
      expect(store.error).toBe('Failed to load')
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
    })
  })

  describe('getById', () => {
    it('success', async () => {
      mockGet.mockResolvedValue(successResult(mockRole))
      const store = useRoleStore()
      const result = await store.getById('1')
      expect(result.isSuccess).toBe(true)
      expect(result.value).toEqual(mockRole)
      expect(RoleApi.get).toHaveBeenCalledWith('1')
    })

    it('failure', async () => {
      mockGet.mockResolvedValue(errorResult('Not found'))
      const store = useRoleStore()
      const result = await store.getById('2')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Not found')
      expect(store.error).toBe('Not found')
    })

    it('network error', async () => {
      mockGet.mockRejectedValue(new Error('Network'))
      const store = useRoleStore()
      const result = await store.getById('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to load')
      expect(store.error).toBe('Failed to load')
    })
  })

  describe('create', () => {
    it('success', async () => {
      mockCreate.mockResolvedValue(successResult(mockRole))
      const store = useRoleStore()
      const result = await store.create(createPayload)
      expect(result.isSuccess).toBe(true)
      expect(result.value).toEqual(mockRole)
      expect(RoleApi.create).toHaveBeenCalledWith(createPayload)
    })

    it('failure', async () => {
      mockCreate.mockResolvedValue(errorResult('Validation failed'))
      const store = useRoleStore()
      const result = await store.create(createPayload)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Validation failed')
      expect(store.error).toBe('Validation failed')
    })

    it('network error', async () => {
      mockCreate.mockRejectedValue(new Error('Network'))
      const store = useRoleStore()
      const result = await store.create(createPayload)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to create')
      expect(store.error).toBe('Failed to create')
    })
  })

  describe('update', () => {
    it('success', async () => {
      mockUpdate.mockResolvedValue(successResult(mockRole))
      const store = useRoleStore()
      const result = await store.update('1', updatePayload)
      expect(result.isSuccess).toBe(true)
      expect(RoleApi.update).toHaveBeenCalledWith('1', updatePayload)
    })

    it('failure', async () => {
      mockUpdate.mockResolvedValue(errorResult('Not found'))
      const store = useRoleStore()
      const result = await store.update('1', updatePayload)
      expect(result.isSuccess).toBe(false)
      expect(store.error).toBe('Not found')
    })

    it('network error', async () => {
      mockUpdate.mockRejectedValue(new Error('Network'))
      const store = useRoleStore()
      const result = await store.update('1', updatePayload)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to update')
    })
  })

  describe('delete', () => {
    it('success', async () => {
      mockDelete.mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null })
      const store = useRoleStore()
      const result = await store.delete('1')
      expect(result.isSuccess).toBe(true)
      expect(RoleApi.delete).toHaveBeenCalledWith('1')
    })

    it('failure', async () => {
      mockDelete.mockResolvedValue(errorResult('Cannot delete'))
      const store = useRoleStore()
      const result = await store.delete('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Cannot delete')
    })

    it('network error', async () => {
      mockDelete.mockRejectedValue(new Error('Network'))
      const store = useRoleStore()
      const result = await store.delete('1')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to delete')
    })
  })

  describe('assignPermission', () => {
    it('success', async () => {
      mockAssignPermission.mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null })
      const store = useRoleStore()
      const result = await store.assignPermission('role-1', 'perm-read')
      expect(result.isSuccess).toBe(true)
      expect(RolePermissionApi.assign).toHaveBeenCalledWith('role-1', { items: [{ permissionId: 'perm-read' }] })
    })

    it('failure', async () => {
      mockAssignPermission.mockResolvedValue(errorResult('Assign failed'))
      const store = useRoleStore()
      const result = await store.assignPermission('role-1', 'perm-read')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Assign failed')
    })

    it('network error', async () => {
      mockAssignPermission.mockRejectedValue(new Error('Network'))
      const store = useRoleStore()
      const result = await store.assignPermission('role-1', 'perm-read')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to assign permission')
    })
  })

  describe('revokePermission', () => {
    it('success', async () => {
      mockRevokePermission.mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null })
      const store = useRoleStore()
      const result = await store.revokePermission('role-1', 'perm-read')
      expect(result.isSuccess).toBe(true)
      expect(RolePermissionApi.revoke).toHaveBeenCalledWith('role-1', { items: [{ permissionId: 'perm-read' }] })
    })

    it('failure', async () => {
      mockRevokePermission.mockResolvedValue(errorResult('Revoke failed'))
      const store = useRoleStore()
      const result = await store.revokePermission('role-1', 'perm-read')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Revoke failed')
    })

    it('network error', async () => {
      mockRevokePermission.mockRejectedValue(new Error('Network'))
      const store = useRoleStore()
      const result = await store.revokePermission('role-1', 'perm-read')
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to revoke permission')
    })
  })

  describe('syncPermissions', () => {
    it('success', async () => {
      mockSyncPermissions.mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null })
      const store = useRoleStore()
      const result = await store.syncPermissions('role-1', ['perm-read', 'perm-write'])
      expect(result.isSuccess).toBe(true)
      expect(RolePermissionApi.sync).toHaveBeenCalledWith('role-1', { items: [{ permissionId: 'perm-read' }, { permissionId: 'perm-write' }] })
    })

    it('failure', async () => {
      mockSyncPermissions.mockResolvedValue(errorResult('Sync failed'))
      const store = useRoleStore()
      const result = await store.syncPermissions('role-1', ['perm-read'])
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Sync failed')
    })

    it('network error', async () => {
      mockSyncPermissions.mockRejectedValue(new Error('Network'))
      const store = useRoleStore()
      const result = await store.syncPermissions('role-1', ['perm-read'])
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Failed to sync permissions')
    })
  })

  describe('listing helpers', () => {
    it('setPage updates query and re-fetches', async () => {
      mockGetMany.mockResolvedValue(pagedResult({ totalCount: 1 }))
      const store = useRoleStore()
      await store.setPage(3)
      expect(store.query.page).toBe(3)
      expect(mockGetMany).toHaveBeenCalled()
    })

    it('setSort updates sort clause', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useRoleStore()
      await store.setSort('name', 'Ascending')
      expect(store.query.sort).toEqual([{ field: 'name', direction: 'Ascending' }])
    })

    it('setSearch sets search and resets page', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useRoleStore()
      await store.setPage(3)
      await store.setSearch('admin')
      expect(store.query.search).toEqual({ value: 'admin', mode: 'Any' })
      expect(store.query.page).toBe(1)
    })

    it('setFilter sets filter group and resets page', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useRoleStore()
      await store.setFilter({ logic: 'And', conditions: [{ field: 'isSystem', operator: 'Equal', value: 'false' }], groups: [] })
      expect(mockGetMany).toHaveBeenCalled()
      expect(store.query.filters).toEqual({ logic: 'And', conditions: [{ field: 'isSystem', operator: 'Equal', value: 'false' }], groups: [] })
      expect(store.query.page).toBe(1)
    })

    it('resetQuery restores defaults', async () => {
      mockGetMany.mockResolvedValue(pagedResult())
      const store = useRoleStore()
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
      const store = useRoleStore()
      const promise = store.fetchMany()
      expect(store.loading).toBe(true)
      resolver(pagedResult())
      await promise
    })

    it('loading is true during getById', async () => {
      let resolver!: (value: unknown) => void
      mockGet.mockImplementation(() => new Promise(resolve => { resolver = resolve }))
      const store = useRoleStore()
      const promise = store.getById('1')
      expect(store.loading).toBe(true)
      resolver(successResult(mockRole))
      await promise
    })

    it('loading is true during assignPermission', async () => {
      let resolver!: (value: unknown) => void
      mockAssignPermission.mockImplementation(() => new Promise(resolve => { resolver = resolve }))
      const store = useRoleStore()
      const promise = store.assignPermission('role-1', 'perm-read')
      expect(store.loading).toBe(true)
      resolver({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null })
      await promise
    })
  })
})
