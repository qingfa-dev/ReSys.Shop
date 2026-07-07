import { describe, it, expect, vi, beforeEach } from 'vitest'
import { orderService } from '../services/order.service'
import apiClient from '@/shared/api/http/api.client'
import type { OrderSearchParams } from '../types/order.types'

// Mock apiClient
vi.mock('@/shared/api/http/api.client', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
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

      expect(apiClient.get).toHaveBeenCalledWith('/admin/orders', { params })
      expect(result).toEqual(mockResponse)
    })
  })

  describe('getById', () => {
    it('should call api.get with correct endpoint', async () => {
      const id = 'order-id'
      const mockResponse = { data: { id }, success: true }

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse)

      const result = await orderService.getById(id)

      expect(apiClient.get).toHaveBeenCalledWith(`/admin/orders/${id}`)
      expect(result).toEqual(mockResponse)
    })
  })

  describe('updateState (advance)', () => {
    it('should call api.post to advance endpoint', async () => {
      const id = 'order-id'
      const mockResponse = { data: {}, success: true }

      vi.mocked(apiClient.post).mockResolvedValue(mockResponse)

      const result = await orderService.updateState(id)

      expect(apiClient.post).toHaveBeenCalledWith(`/admin/orders/${id}/advance`)
      expect(result).toEqual(mockResponse)
    })
  })

  describe('cancelOrder', () => {
    it('should call api.post with reason', async () => {
      const id = 'order-id'
      const reason = 'Out of stock'
      const mockResponse = { data: {}, success: true }

      vi.mocked(apiClient.post).mockResolvedValue(mockResponse)

      const result = await orderService.cancelOrder(id, reason)

      expect(apiClient.post).toHaveBeenCalledWith(`/admin/orders/${id}/cancel`, { reason })
      expect(result).toEqual(mockResponse)
    })
  })
})
