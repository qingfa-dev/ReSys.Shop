import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { RolePermissionApi } from '../role-permission.api'

vi.mock('@/shared/api/client', () => ({
  default: {
    get: vi.fn<(...args: unknown[]) => unknown>(),
    post: vi.fn<(...args: unknown[]) => unknown>(),
    put: vi.fn<(...args: unknown[]) => unknown>(),
    delete: vi.fn<(...args: unknown[]) => unknown>(),
    patch: vi.fn<(...args: unknown[]) => unknown>(),
  },
}))

const singleOk = (value: unknown) => ({ isSuccess: true, value, statusCode: 200 })

describe('RolePermissionApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  describe('get', () => {
    it('calls GET /identity/roles/:id/permissions', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ roleId: '1', permissions: [{ id: 'p1', name: 'users.read' }] }) })
      await RolePermissionApi.get('1')
      expect(apiClient.get).toHaveBeenCalledWith('/identity/roles/1/permissions')
    })
  })

  describe('assign', () => {
    it('calls PUT /identity/roles/:id/permissions/assign with body', async () => {
      const data = { permissionIds: ['p1', 'p2'] }
      vi.mocked(apiClient.put).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await RolePermissionApi.assign('1', data)
      expect(apiClient.put).toHaveBeenCalledWith('/identity/roles/1/permissions/assign', data)
    })
  })

  describe('revoke', () => {
    it('calls DELETE /identity/roles/:id/permissions/revoke with data in config', async () => {
      const data = { permissionIds: ['p1'] }
      vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await RolePermissionApi.revoke('1', data)
      expect(apiClient.delete).toHaveBeenCalledWith('/identity/roles/1/permissions/revoke', { data })
    })
  })

  describe('sync', () => {
    it('calls PATCH /identity/roles/:id/permissions/sync with body', async () => {
      const data = { permissionIds: ['p1'] }
      vi.mocked(apiClient.patch).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await RolePermissionApi.sync('1', data)
      expect(apiClient.patch).toHaveBeenCalledWith('/identity/roles/1/permissions/sync', data)
    })
  })
})
