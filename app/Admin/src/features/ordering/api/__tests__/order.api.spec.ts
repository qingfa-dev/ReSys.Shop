import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { OrderApi } from '../order.api'

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

describe('OrderApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  describe('getMany', () => {
    it('calls GET /ordering/orders with serialized query params', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
      await OrderApi.getMany(defaultQuery)
      expect(apiClient.get).toHaveBeenCalledWith('/ordering/orders', {
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
    it('calls GET /ordering/orders/:id', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', orderNumber: 'ORD-001' }) })
      await OrderApi.get('1')
      expect(apiClient.get).toHaveBeenCalledWith('/ordering/orders/1')
    })
  })

  describe('create', () => {
    it('calls POST /ordering/orders with body', async () => {
      const data = { customerId: 'cust-1', lineItems: [{ variantId: 'p1', quantity: 2, unitPrice: 10 }] }
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '1', orderNumber: 'ORD-001' }) })
      await OrderApi.create(data)
      expect(apiClient.post).toHaveBeenCalledWith('/ordering/orders', data)
    })
  })

  describe('update', () => {
    it('calls PUT /ordering/orders/:id with body', async () => {
      vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1', notes: 'updated notes' }) })
      await OrderApi.update('1', { notes: 'updated notes' })
      expect(apiClient.put).toHaveBeenCalledWith('/ordering/orders/1', { notes: 'updated notes' })
    })
  })

  describe('delete', () => {
    it('calls DELETE /ordering/orders/:id', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await OrderApi.delete('1')
      expect(apiClient.delete).toHaveBeenCalledWith('/ordering/orders/1')
    })
  })

  describe('cancel', () => {
    it('calls POST /ordering/orders/:id/cancel', async () => {
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '1', status: 'Cancelled' }) })
      await OrderApi.cancel('1')
      expect(apiClient.post).toHaveBeenCalledWith('/ordering/orders/1/cancel')
    })
  })

  describe('complete', () => {
    it('calls POST /ordering/orders/:id/complete', async () => {
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '1', status: 'Completed' }) })
      await OrderApi.complete('1')
      expect(apiClient.post).toHaveBeenCalledWith('/ordering/orders/1/complete')
    })
  })

  describe('approve', () => {
    it('calls POST /ordering/orders/:id/approve', async () => {
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '1', status: 'Approved' }) })
      await OrderApi.approve('1')
      expect(apiClient.post).toHaveBeenCalledWith('/ordering/orders/1/approve')
    })
  })

  describe('resume', () => {
    it('calls POST /ordering/orders/:id/resume', async () => {
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '1', status: 'Processing' }) })
      await OrderApi.resume('1')
      expect(apiClient.post).toHaveBeenCalledWith('/ordering/orders/1/resume')
    })
  })

  describe('updateStatus', () => {
    it('calls PUT /ordering/orders/:id/status with body', async () => {
      const data = { status: 'Shipped' }
      vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1', status: 'Shipped' }) })
      await OrderApi.updateStatus('1', data)
      expect(apiClient.put).toHaveBeenCalledWith('/ordering/orders/1/status', data)
    })
  })

  describe('updateShipAddress', () => {
    it('calls PUT /ordering/orders/:id/ship-address with body', async () => {
      const data = { firstName: 'John', lastName: 'Doe', address1: '123 Main St', city: 'NYC', postalCode: '10001', country: 'US', stateId: 's1', countryId: 'c1' }
      vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1' }) })
      await OrderApi.updateShipAddress('1', data)
      expect(apiClient.put).toHaveBeenCalledWith('/ordering/orders/1/ship-address', data)
    })
  })

  describe('updateBillAddress', () => {
    it('calls PUT /ordering/orders/:id/bill-address with body', async () => {
      const data = { firstName: 'Jane', lastName: 'Doe', address1: '456 Oak Ave', city: 'LA', postalCode: '90001', country: 'US', stateId: 's2', countryId: 'c1' }
      vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1' }) })
      await OrderApi.updateBillAddress('1', data)
      expect(apiClient.put).toHaveBeenCalledWith('/ordering/orders/1/bill-address', data)
    })
  })

  describe('updateShippingMethod', () => {
    it('calls PUT /ordering/orders/:id/shipping-method with body', async () => {
      const data = { shippingMethod: 'express' }
      vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1' }) })
      await OrderApi.updateShippingMethod('1', data)
      expect(apiClient.put).toHaveBeenCalledWith('/ordering/orders/1/shipping-method', data)
    })
  })
})
