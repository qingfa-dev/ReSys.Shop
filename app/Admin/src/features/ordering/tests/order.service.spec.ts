import { describe, it, expect, vi, beforeEach } from 'vitest'
import { orderService } from '../services/order.service'
import apiClient from '@/shared/api/http/api.client'
import type { OrderSearchParams } from '../types/order.request.types'

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
      const serverResult = { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: [] }

      vi.mocked(apiClient.get).mockResolvedValue({ data: serverResult })

      const result = await orderService.list(params)

      expect(apiClient.get).toHaveBeenCalledWith('api/ordering/orders', { params })
      expect(result).toEqual(serverResult)
    })
  })

  describe('getById', () => {
    it('should call api.get with correct endpoint', async () => {
      const id = 'order-id'
      const serverResult = { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: { id } }

      vi.mocked(apiClient.get).mockResolvedValue({ data: serverResult })

      const result = await orderService.getById(id)

      expect(apiClient.get).toHaveBeenCalledWith('api/ordering/orders/order-id')
      expect(result).toEqual(serverResult)
    })
  })

  describe('updateStatus', () => {
    it('should call api.put to status endpoint', async () => {
      const id = 'order-id'
      const status = 'Processing'
      const serverResult = { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }

      vi.mocked(apiClient.put).mockResolvedValue({ data: serverResult })

      const result = await orderService.updateStatus(id, status)

      expect(apiClient.put).toHaveBeenCalledWith('api/ordering/orders/order-id/status', { status })
      expect(result).toEqual(serverResult)
    })
  })

  describe('cancel', () => {
    it('should call api.post with reason', async () => {
      const id = 'order-id'
      const reason = 'Out of stock'
      const serverResult = { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }

      vi.mocked(apiClient.post).mockResolvedValue({ data: serverResult })

      const result = await orderService.cancel(id, reason)

      expect(apiClient.post).toHaveBeenCalledWith('api/ordering/orders/order-id/cancel', { reason })
      expect(result).toEqual(serverResult)
    })
  })

  describe('addItem', () => {
    it('should call api.post to line-items endpoint', async () => {
      const id = 'order-id'
      const data = { variantId: 'v-1', quantity: 2 }
      const serverResult = { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }

      vi.mocked(apiClient.post).mockResolvedValue({ data: serverResult })

      const result = await orderService.addItem(id, data)

      expect(apiClient.post).toHaveBeenCalledWith('api/ordering/orders/order-id/line-items', data)
      expect(result).toEqual(serverResult)
    })
  })
})
