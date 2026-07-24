import { describe, it, expect } from 'vitest'
import { mockOrderRepository } from '../order.mock.repository'

import type { CheckoutRequest } from '../../../types'

describe('OrderRepository', () => {
  describe('getAll', () => {
    it('should return orders', async () => {
      const result = await mockOrderRepository.getAll()
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('getById', () => {
    it('should return order by id', async () => {
      const result = await mockOrderRepository.getById('order-1')
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('checkout', () => {
    it('should create order', async () => {
      const result = await mockOrderRepository.checkout({} as CheckoutRequest)
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('cancelOrder', () => {
    it('should cancel order', async () => {
      const result = await mockOrderRepository.cancelOrder('order-1')
      expect(result.isSuccess).toBe(true)
    })
  })
})