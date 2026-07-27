import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { ShippingRateApi } from '../shipping-rate.api'

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

describe('ShippingRateApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  describe('getMany', () => {
    it('calls GET /shipping/shipping-rates with serialized query params', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
      await ShippingRateApi.getMany(defaultQuery)
      expect(apiClient.get).toHaveBeenCalledWith('/shipping/shipping-rates', {
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
    it('calls GET /shipping/shipping-rates/:id', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', rate: 9.99 }) })
      await ShippingRateApi.get('1')
      expect(apiClient.get).toHaveBeenCalledWith('/shipping/shipping-rates/1')
    })
  })

  describe('create', () => {
    it('calls POST /shipping/shipping-rates with body', async () => {
      const data = { name: 'Standard Rate', rate: 5.99, currency: 'USD', shippingMethodId: 'sm1' }
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '2', ...data }) })
      await ShippingRateApi.create(data)
      expect(apiClient.post).toHaveBeenCalledWith('/shipping/shipping-rates', data)
    })
  })

  describe('update', () => {
    it('calls PUT /shipping/shipping-rates/:id with body', async () => {
      vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1', rate: 7.99 }) })
      await ShippingRateApi.update('1', { name: 'Express Rate', rate: 7.99, currency: 'USD', shippingMethodId: 'sm2' })
      expect(apiClient.put).toHaveBeenCalledWith('/shipping/shipping-rates/1', { name: 'Express Rate', rate: 7.99, currency: 'USD', shippingMethodId: 'sm2' })
    })
  })

  describe('delete', () => {
    it('calls DELETE /shipping/shipping-rates/:id', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await ShippingRateApi.delete('1')
      expect(apiClient.delete).toHaveBeenCalledWith('/shipping/shipping-rates/1')
    })
  })
})
