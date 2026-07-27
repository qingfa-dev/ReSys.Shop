import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { PaymentApi } from '../payment.api'

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

describe('PaymentApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  describe('getMany', () => {
    it('calls GET /payment/payments with serialized query params', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
      await PaymentApi.getMany(defaultQuery)
      expect(apiClient.get).toHaveBeenCalledWith('/payment/payments', {
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
    it('calls GET /payment/payments/:id', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', amount: 100 }) })
      await PaymentApi.get('1')
      expect(apiClient.get).toHaveBeenCalledWith('/payment/payments/1')
    })
  })

  describe('capture', () => {
    it('calls POST /payment/payments/:id/capture', async () => {
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '1', status: 'Captured' }) })
      await PaymentApi.capture('1')
      expect(apiClient.post).toHaveBeenCalledWith('/payment/payments/1/capture', undefined)
    })

    it('sends body when data is provided', async () => {
      const data = { amount: 50 }
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '1', status: 'Captured' }) })
      await PaymentApi.capture('1', data)
      expect(apiClient.post).toHaveBeenCalledWith('/payment/payments/1/capture', data)
    })
  })

  describe('void', () => {
    it('calls POST /payment/payments/:id/void', async () => {
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '1', status: 'Voided' }) })
      await PaymentApi.void('1')
      expect(apiClient.post).toHaveBeenCalledWith('/payment/payments/1/void', undefined)
    })

    it('sends body when data is provided', async () => {
      const data = { reason: 'fraud' }
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '1', status: 'Voided' }) })
      await PaymentApi.void('1', data)
      expect(apiClient.post).toHaveBeenCalledWith('/payment/payments/1/void', data)
    })
  })

  describe('refund', () => {
    it('calls POST /payment/payments/:id/refund', async () => {
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '1', status: 'Refunded' }) })
      await PaymentApi.refund('1')
      expect(apiClient.post).toHaveBeenCalledWith('/payment/payments/1/refund', undefined)
    })

    it('sends body when data is provided', async () => {
      const data = { amount: 25 }
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '1', status: 'Refunded' }) })
      await PaymentApi.refund('1', data)
      expect(apiClient.post).toHaveBeenCalledWith('/payment/payments/1/refund', data)
    })
  })
})
