import { describe, it, expect, vi } from 'vitest'
import apiClient from '@/shared/api/http/api.client'
import { identityApi } from '../api/identity.api'

vi.mock('@/shared/api/http/api.client', () => ({
  default: { get: vi.fn(), post: vi.fn(), put: vi.fn(), patch: vi.fn(), delete: vi.fn() }
}))

describe('identityApi.users', () => {
  it('list calls correct route', async () => {
    await identityApi.users.list({ page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('identity/users', expect.any(Object))
  })
  it('assignRole calls correct route', async () => {
    await identityApi.users.assignRole('uid-1', 'admin')
    expect(apiClient.post).toHaveBeenCalledWith('identity/users/uid-1/roles/assign', { roleName: 'admin' })
  })
  it('syncRoles calls correct route', async () => {
    await identityApi.users.syncRoles('uid-1', ['admin'])
    expect(apiClient.patch).toHaveBeenCalledWith('identity/users/uid-1/roles/sync', { roleNames: ['admin'] })
  })
  it('assignPermission calls correct route', async () => {
    await identityApi.users.assignPermission('uid-1', 'catalog.read')
    expect(apiClient.post).toHaveBeenCalledWith('identity/users/uid-1/permissions/assign', { permissionName: 'catalog.read' })
  })
  it('revokePermission calls correct route', async () => {
    await identityApi.users.revokePermission('uid-1', 'catalog.read')
    expect(apiClient.delete).toHaveBeenCalledWith('identity/users/uid-1/permissions/revoke', expect.any(Object))
  })
})
