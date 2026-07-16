import { describe, it, expect, vi, beforeEach } from 'vitest'
import { orderService } from '../services/order.service'
import apiClient from '@/shared/api/http/api.client'
import type { OrderSearchParams } from '../types/order.types'

// Mock apiClient
vi.mock('@/shared/api/http/api.client', () => ({
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
      const params: OrderSearchParams = { search: 'ORD-1', page: 1 }
      const mockResponse = { data: [], success: true }

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse)

      const result = await orderService.list(params)

      expect(apiClient.get).toHaveBeenCalledWith('api/ordering/orders', { params })
      expect(result).toEqual(mockResponse)
    })
  })

  describe('getById', () => {
    it('should call api.get with correct endpoint', async () => {
      const id = 'order-id'
      const mockResponse = { data: { id }, success: true }

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse)

      const result = await orderService.getById(id)

      expect(apiClient.get).toHaveBeenCalledWith('api/ordering/orders/order-id')
      expect(result).toEqual(mockResponse)
    })
  })

  describe('updateStatus', () => {
    it('should call api.put to status endpoint', async () => {
      const id = 'order-id'
      const status = 'Processing'
      const mockResponse = { data: {}, success: true }

      vi.mocked(apiClient.put).mockResolvedValue(mockResponse)

      const result = await orderService.updateStatus(id, status)

      expect(apiClient.put).toHaveBeenCalledWith('api/ordering/orders/order-id/status', { status })
      expect(result).toEqual(mockResponse)
    })
  })

  describe('cancel', () => {
    it('should call api.post with reason', async () => {
      const id = 'order-id'
      const reason = 'Out of stock'
      const mockResponse = { data: {}, success: true }

      vi.mocked(apiClient.post).mockResolvedValue(mockResponse)

      const result = await orderService.cancel(id, reason)

      expect(apiClient.post).toHaveBeenCalledWith('api/ordering/orders/order-id/cancel', { reason })
      expect(result).toEqual(mockResponse)
    })
  })

  describe('addItem', () => {
    it('should call api.post to line-items endpoint', async () => {
      const id = 'order-id'
      const data = { variantId: 'v-1', quantity: 2 }
      const mockResponse = { data: {}, success: true }

      vi.mocked(apiClient.post).mockResolvedValue(mockResponse)

      const result = await orderService.addItem(id, data)

      expect(apiClient.post).toHaveBeenCalledWith('api/ordering/orders/order-id/line-items', data)
      expect(result).toEqual(mockResponse)
    })
  })
})
