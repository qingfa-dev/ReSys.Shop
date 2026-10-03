import { describe, it, expect, beforeEach } from 'vitest'
import { mockCartRepository } from '../cart.mock.repository'

describe('CartRepository', () => {
  beforeEach(async () => {
    await mockCartRepository.createCart()
  })

  describe('getCart', () => {
    it('should return cart', async () => {
      const result = await mockCartRepository.getCart()
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('addItem', () => {
    it('should add an item by variantId and quantity', async () => {
      const result = await mockCartRepository.addItem('prod-1', 2)
      expect(result.isSuccess).toBe(true)
      expect(result.data?.items).toHaveLength(1)
      expect(result.data?.items[0]?.variantId).toBe('prod-1')
      expect(result.data?.items[0]?.quantity).toBe(2)
    })
  })

  describe('updateItem', () => {
    it('should update cart item quantity', async () => {
      await mockCartRepository.addItem('prod-1', 2)
      const result = await mockCartRepository.updateItem('prod-1', 5)
      expect(result.isSuccess).toBe(true)
      expect(result.data?.items[0]?.quantity).toBe(5)
    })
  })

  describe('removeItem', () => {
    it('should remove item from cart', async () => {
      await mockCartRepository.addItem('prod-1', 2)
      const result = await mockCartRepository.removeItem('prod-1')
      expect(result.isSuccess).toBe(true)
      expect(result.data?.items).toHaveLength(0)
    })
  })

  describe('clearCart', () => {
    it('should clear cart', async () => {
      await mockCartRepository.addItem('prod-1', 2)
      const result = await mockCartRepository.clearCart()
      expect(result.isSuccess).toBe(true)
      expect(result.data?.items.length).toBe(0)
    })
  })
})
