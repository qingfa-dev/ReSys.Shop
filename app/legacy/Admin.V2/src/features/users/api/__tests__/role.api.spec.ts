import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { RoleApi } from '../role.api'

vi.mock('@/shared/api/client', () => ({
  default: {
    get: vi.fn<(...args: unknown[]) => unknown>(),
    post: vi.fn<(...args: unknown[]) => unknown>(),
    put: vi.fn<(...args: unknown[]) => unknown>(),
    delete: vi.fn<(...args: unknown[]) => unknown>(),
    patch: vi.fn<(...args: unknown[]) => unknown>(),
  },
}))

const pagedEmpty = { isSuccess: true, items: [], page: 1, pageSize: 20, totalCount: 0, statusCode: 200 }
const singleOk = (value: unknown) => ({ isSuccess: true, value, statusCode: 200 })
const defaultQuery = { page: 1, pageSize: 20, sort: [{ field: 'createdAt' as const, direction: 'Descending' as const }] }

describe('RoleApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  describe('getMany', () => {
    it('calls GET /identity/roles with serialized query params', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
      await RoleApi.getMany(defaultQuery)
      expect(apiClient.get).toHaveBeenCalledWith('/identity/roles', {
        params: {
          'page.page': 1,
          'page.pageSize': 20,
          'sort.clauses[0].field': 'createdAt',
          'sort.clauses[0].direction': 'Descending',
        },
      })
    })
  })

  describe('get', () => {
    it('calls GET /identity/roles/:id', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', name: 'Admin' }) })
      await RoleApi.get('1')
      expect(apiClient.get).toHaveBeenCalledWith('/identity/roles/1')
    })
  })

  describe('create', () => {
    it('calls POST /identity/roles with body', async () => {
      const data = { name: 'Manager', description: 'Store manager role' }
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '2', ...data }) })
      await RoleApi.create(data)
      expect(apiClient.post).toHaveBeenCalledWith('/identity/roles', data)
    })
  })

  describe('update', () => {
    it('calls PUT /identity/roles/:id with body', async () => {
      vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1', name: 'Updated' }) })
      await RoleApi.update('1', { name: 'Updated' })
      expect(apiClient.put).toHaveBeenCalledWith('/identity/roles/1', { name: 'Updated' })
    })
  })

  describe('delete', () => {
    it('calls DELETE /identity/roles/:id', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await RoleApi.delete('1')
      expect(apiClient.delete).toHaveBeenCalledWith('/identity/roles/1')
    })
  })
})
