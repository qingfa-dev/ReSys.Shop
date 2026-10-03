import { describe, it, expect } from 'vitest'
import { orderService } from '../order.service'

import type { CheckoutRequest } from '../../../types'

describe('OrderService', () => {
  describe('getOrders', () => {
    it('should return orders', async () => {
      const result = await orderService.getOrders()
      expect(result).toBeDefined()
    })
  })

  describe('getOrder', () => {
    it('should return order by id', async () => {
      const result = await orderService.getOrder('order-1')
      expect(result).toBeDefined()
    })
  })

  describe('checkout', () => {
    it('should checkout', async () => {
      const result = await orderService.checkout({} as CheckoutRequest)
      expect(result).toBeDefined()
    })
  })

  describe('cancelOrder', () => {
    it('should cancel order', async () => {
      const result = await orderService.cancelOrder('order-1')
      expect(result).toBeDefined()
    })
  })
})