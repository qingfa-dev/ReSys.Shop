import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { UserApi } from '../user.api'

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

describe('UserApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  describe('getMany', () => {
    it('calls GET /identity/users with serialized query params', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
      await UserApi.getMany(defaultQuery)
      expect(apiClient.get).toHaveBeenCalledWith('/identity/users', {
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
    it('calls GET /identity/users/:id', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', userName: 'john' }) })
      await UserApi.get('1')
      expect(apiClient.get).toHaveBeenCalledWith('/identity/users/1')
    })
  })

  describe('create', () => {
    it('calls POST /identity/users with body', async () => {
      const data = { userName: 'jane', email: 'jane@example.com', password: 'P@ssw0rd!' }
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '2', ...data }) })
      await UserApi.create(data)
      expect(apiClient.post).toHaveBeenCalledWith('/identity/users', data)
    })
  })

  describe('update', () => {
    it('calls PUT /identity/users/:id with body', async () => {
      vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1', userName: 'updated' }) })
      await UserApi.update('1', { userName: 'updated' })
      expect(apiClient.put).toHaveBeenCalledWith('/identity/users/1', { userName: 'updated' })
    })
  })

  describe('delete', () => {
    it('calls DELETE /identity/users/:id', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await UserApi.delete('1')
      expect(apiClient.delete).toHaveBeenCalledWith('/identity/users/1')
    })
  })

  describe('toggleStatus', () => {
    it('calls PATCH /identity/users/:id/status with body', async () => {
      const data = { isEnabled: false }
      vi.mocked(apiClient.patch).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await UserApi.toggleStatus('1', data)
      expect(apiClient.patch).toHaveBeenCalledWith('/identity/users/1/status', data)
    })
  })
})
