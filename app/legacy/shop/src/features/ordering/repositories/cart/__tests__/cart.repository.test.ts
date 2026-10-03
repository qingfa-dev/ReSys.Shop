import { describe, it, expect } from 'vitest'
import { mockCartRepository } from '../cart.mock.repository'

describe('CartRepository', () => {
  describe('getCart', () => {
    it('should return cart', async () => {
      const result = await mockCartRepository.getCart()
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('addItem', () => {
    it('should add item to cart', async () => {
      const result = await mockCartRepository.addItem('prod-1', 'Test Product', 'image.jpg', 2, 29.99)
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('updateItem', () => {
    it('should update cart item', async () => {
      await mockCartRepository.addItem('prod-1', 'Test', 'img', 1, 10)
      const result = await mockCartRepository.updateItem('item-1', 5)
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('removeItem', () => {
    it('should remove item from cart', async () => {
      const result = await mockCartRepository.removeItem('item-1')
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('clearCart', () => {
    it('should clear cart', async () => {
      const result = await mockCartRepository.clearCart()
      expect(result.isSuccess).toBe(true)
      expect(result.data?.items.length).toBe(0)
    })
  })
})