import { describe, it, expect, vi, beforeEach } from 'vitest'
import { orderRepository } from '../api/order.api'
import apiClient from '@/common/api/http/api.client'
import type { OrderQuery } from '../types/order.query'

// Mock apiClient
vi.mock('@/common/api/http/api.client', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
  },
}))

describe('OrderService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('list', () => {
    it('should call api.get with correct endpoint and params', async () => {
      const params: OrderQuery = { search: 'ORD-1', page: 1 }
      const serverResult = { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, items: [], totalCount: 0, page: 1, pageSize: 10 }

      vi.mocked(apiClient.get).mockResolvedValue({ data: serverResult })

      const result = await orderRepository.list(params)

      expect(apiClient.get).toHaveBeenCalledWith('ordering/orders', { params })
      expect(result).toEqual(serverResult)
    })
  })

  describe('getById', () => {
    it('should call api.get with correct endpoint', async () => {
      const id = 'order-id'
      const orderDetail = {
        id, number: 'R1', status: 0, checkoutState: 0, currency: 'USD',
        email: null, specialInstructions: null, billAddressId: null,
        shipAddressId: null, shippingMethodId: null, itemTotal: 0,
        adjustmentTotal: 0, shipmentTotal: 0, total: 0, paymentTotal: 0,
        outstandingBalance: 0, paymentState: null, shipmentState: null,
        userId: null, storeId: null, itemCount: 0,
        approvedById: null, approvedAtUtc: null, completedAtUtc: null,
        canceledAtUtc: null, createdAtUtc: '', modifiedAtUtc: null,
      }
      const serverResult = { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: orderDetail }

      vi.mocked(apiClient.get).mockResolvedValue({ data: serverResult })

      const result = await orderRepository.getById(id)

      expect(apiClient.get).toHaveBeenCalledWith('ordering/orders/order-id')
      expect(result.isSuccess).toBe(true)
      expect(result.value!.id).toBe(id)
    })
  })

  describe('updateStatus', () => {
    it('should call api.put to status endpoint', async () => {
      const id = 'order-id'
      const status = 'Processing'
      const serverResult = { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }

      vi.mocked(apiClient.put).mockResolvedValue({ data: serverResult })

      const result = await orderRepository.updateStatus(id, { status })

      expect(apiClient.put).toHaveBeenCalledWith('ordering/orders/order-id/status', { status })
      expect(result).toEqual(serverResult)
    })
  })

  describe('cancel', () => {
    it('should call api.post with reason', async () => {
      const id = 'order-id'
      const reason = 'Out of stock'
      const serverResult = { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }

      vi.mocked(apiClient.post).mockResolvedValue({ data: serverResult })

      const result = await orderRepository.cancel(id, { reason })

      expect(apiClient.post).toHaveBeenCalledWith('ordering/orders/order-id/cancel', { reason })
      expect(result).toEqual(serverResult)
    })
  })

  describe('addItem', () => {
    it('should call api.post to line-items endpoint', async () => {
      const id = 'order-id'
      const data = { variantId: 'v-1', quantity: 2 }
      const serverResult = { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }

      vi.mocked(apiClient.post).mockResolvedValue({ data: serverResult })

      const result = await orderRepository.addLineItem(id, data)

      expect(apiClient.post).toHaveBeenCalledWith('ordering/orders/order-id/line-items', data)
      expect(result).toEqual(serverResult)
    })
  })
})
