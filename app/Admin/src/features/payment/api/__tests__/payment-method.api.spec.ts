import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { PaymentMethodApi } from '../payment-method.api'

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

describe('PaymentMethodApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  describe('getMany', () => {
    it('calls GET /payment/payment-methods with serialized query params', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
      await PaymentMethodApi.getMany(defaultQuery)
      expect(apiClient.get).toHaveBeenCalledWith('/payment/payment-methods', {
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
    it('calls GET /payment/payment-methods/:id', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', name: 'Credit Card' }) })
      await PaymentMethodApi.get('1')
      expect(apiClient.get).toHaveBeenCalledWith('/payment/payment-methods/1')
    })
  })

  describe('create', () => {
    it('calls POST /payment/payment-methods with body', async () => {
      const data = { name: 'PayPal', code: 'paypal' }
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '2', ...data }) })
      await PaymentMethodApi.create(data)
      expect(apiClient.post).toHaveBeenCalledWith('/payment/payment-methods', data)
    })
  })

  describe('update', () => {
    it('calls PUT /payment/payment-methods/:id with body', async () => {
      vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1', name: 'Updated' }) })
      await PaymentMethodApi.update('1', { name: 'Updated' })
      expect(apiClient.put).toHaveBeenCalledWith('/payment/payment-methods/1', { name: 'Updated' })
    })
  })

  describe('delete', () => {
    it('calls DELETE /payment/payment-methods/:id', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await PaymentMethodApi.delete('1')
      expect(apiClient.delete).toHaveBeenCalledWith('/payment/payment-methods/1')
    })
  })

  describe('activate', () => {
    it('calls PATCH /payment/payment-methods/:id/activate', async () => {
      vi.mocked(apiClient.patch).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await PaymentMethodApi.activate('1')
      expect(apiClient.patch).toHaveBeenCalledWith('/payment/payment-methods/1/activate')
    })
  })

  describe('deactivate', () => {
    it('calls PATCH /payment/payment-methods/:id/deactivate', async () => {
      vi.mocked(apiClient.patch).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await PaymentMethodApi.deactivate('1')
      expect(apiClient.patch).toHaveBeenCalledWith('/payment/payment-methods/1/deactivate')
    })
  })
})
