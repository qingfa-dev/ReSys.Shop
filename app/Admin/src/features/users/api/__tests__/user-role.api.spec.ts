import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { UserRoleApi } from '../user-role.api'

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

describe('UserRoleApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  describe('get', () => {
    it('calls GET /identity/users/:id/roles', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ userId: '1', roles: [{ id: 'r1', name: 'Admin' }] }) })
      await UserRoleApi.get('1')
      expect(apiClient.get).toHaveBeenCalledWith('/identity/users/1/roles')
    })
  })

  describe('assign', () => {
    it('calls POST /identity/users/:id/roles/assign with body', async () => {
      const data = { roleIds: ['r1', 'r2'] }
      vi.mocked(apiClient.post).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await UserRoleApi.assign('1', data)
      expect(apiClient.post).toHaveBeenCalledWith('/identity/users/1/roles/assign', data)
    })
  })

  describe('revoke', () => {
    it('calls POST /identity/users/:id/roles/revoke with body', async () => {
      const data = { roleIds: ['r1'] }
      vi.mocked(apiClient.post).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await UserRoleApi.revoke('1', data)
      expect(apiClient.post).toHaveBeenCalledWith('/identity/users/1/roles/revoke', data)
    })
  })

  describe('sync', () => {
    it('calls PATCH /identity/users/:id/roles/sync with body', async () => {
      const data = { roleIds: ['r1'] }
      vi.mocked(apiClient.patch).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await UserRoleApi.sync('1', data)
      expect(apiClient.patch).toHaveBeenCalledWith('/identity/users/1/roles/sync', data)
    })
  })
})
