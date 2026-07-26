import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { AddressApi } from '../address.api'

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

describe('AddressApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  describe('getMany', () => {
    it('calls GET /profiles/addresses with serialized query params', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
      await AddressApi.getMany(defaultQuery)
      expect(apiClient.get).toHaveBeenCalledWith('/profiles/addresses', {
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
    it('calls GET /profiles/addresses/:id', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', addressLine1: '123 Main St' }) })
      await AddressApi.get('1')
      expect(apiClient.get).toHaveBeenCalledWith('/profiles/addresses/1')
    })
  })

  describe('create', () => {
    it('calls POST /profiles/addresses with body', async () => {
      const data = { addressLine1: '456 Oak Ave', city: 'LA' }
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '2', ...data }) })
      await AddressApi.create(data)
      expect(apiClient.post).toHaveBeenCalledWith('/profiles/addresses', data)
    })
  })

  describe('update', () => {
    it('calls PUT /profiles/addresses/:id with body', async () => {
      vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1', addressLine1: 'Updated' }) })
      await AddressApi.update('1', { addressLine1: 'Updated' })
      expect(apiClient.put).toHaveBeenCalledWith('/profiles/addresses/1', { addressLine1: 'Updated' })
    })
  })

  describe('delete', () => {
    it('calls DELETE /profiles/addresses/:id', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await AddressApi.delete('1')
      expect(apiClient.delete).toHaveBeenCalledWith('/profiles/addresses/1')
    })
  })
})
