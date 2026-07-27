import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { UserPermissionApi } from '../user-permission.api'

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

describe('UserPermissionApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  describe('get', () => {
    it('calls GET /identity/users/:id/permissions', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ userId: '1', permissions: [{ id: 'p1', name: 'users.read' }] }) })
      await UserPermissionApi.get('1')
      expect(apiClient.get).toHaveBeenCalledWith('/identity/users/1/permissions')
    })
  })

  describe('assign', () => {
    it('calls POST /identity/users/:id/permissions/assign with body', async () => {
      const data = { items: [{ permissionId: 'p1' }, { permissionId: 'p2' }] }
      vi.mocked(apiClient.post).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await UserPermissionApi.assign('1', data)
      expect(apiClient.post).toHaveBeenCalledWith('/identity/users/1/permissions/assign', data)
    })
  })

  describe('revoke', () => {
    it('calls DELETE /identity/users/:id/permissions/revoke with data in config', async () => {
      const data = { items: [{ permissionId: 'p1' }] }
      vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await UserPermissionApi.revoke('1', data)
      expect(apiClient.delete).toHaveBeenCalledWith('/identity/users/1/permissions/revoke', { data })
    })
  })

  describe('sync', () => {
    it('calls PUT /identity/users/:id/permissions/sync with body', async () => {
      const data = { items: [{ permissionId: 'p1' }] }
      vi.mocked(apiClient.put).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await UserPermissionApi.sync('1', data)
      expect(apiClient.put).toHaveBeenCalledWith('/identity/users/1/permissions/sync', data)
    })
  })
})
