import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { ShippingMethodApi } from '../shipping-method.api'

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

describe('ShippingMethodApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  describe('getMany', () => {
    it('calls GET /shipping/shipping-methods with serialized query params', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
      await ShippingMethodApi.getMany(defaultQuery)
      expect(apiClient.get).toHaveBeenCalledWith('/shipping/shipping-methods', {
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
    it('calls GET /shipping/shipping-methods/:id', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', name: 'Standard' }) })
      await ShippingMethodApi.get('1')
      expect(apiClient.get).toHaveBeenCalledWith('/shipping/shipping-methods/1')
    })
  })

  describe('create', () => {
    it('calls POST /shipping/shipping-methods with body', async () => {
      const data = { name: 'Express', code: 'express' }
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '2', ...data }) })
      await ShippingMethodApi.create(data)
      expect(apiClient.post).toHaveBeenCalledWith('/shipping/shipping-methods', data)
    })
  })

  describe('update', () => {
    it('calls PUT /shipping/shipping-methods/:id with body', async () => {
      vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1', name: 'Updated' }) })
      await ShippingMethodApi.update('1', { name: 'Updated', code: 'EXP' })
      expect(apiClient.put).toHaveBeenCalledWith('/shipping/shipping-methods/1', { name: 'Updated', code: 'EXP' })
    })
  })

  describe('delete', () => {
    it('calls DELETE /shipping/shipping-methods/:id', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await ShippingMethodApi.delete('1')
      expect(apiClient.delete).toHaveBeenCalledWith('/shipping/shipping-methods/1')
    })
  })

  describe('activate', () => {
    it('calls PATCH /shipping/shipping-methods/:id/activate', async () => {
      vi.mocked(apiClient.patch).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await ShippingMethodApi.activate('1')
      expect(apiClient.patch).toHaveBeenCalledWith('/shipping/shipping-methods/1/activate')
    })
  })

  describe('deactivate', () => {
    it('calls PATCH /shipping/shipping-methods/:id/deactivate', async () => {
      vi.mocked(apiClient.patch).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await ShippingMethodApi.deactivate('1')
      expect(apiClient.patch).toHaveBeenCalledWith('/shipping/shipping-methods/1/deactivate')
    })
  })
})
