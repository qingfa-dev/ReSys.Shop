import { describe, it, expect } from 'vitest'
import { cartService } from '../cart.service'

describe('CartService', () => {
  describe('getCart', () => {
    it('should return cart', async () => {
      const result = await cartService.getCart()
      expect(result).toBeDefined()
    })
  })

  describe('addToCart', () => {
    it('should add item to cart', async () => {
      const result = await cartService.addToCart('prod-1', 'Test Product', 'http://image.jpg', 1, 99.99)
      expect(result).toBeDefined()
    })
  })

  describe('updateCartItem', () => {
    it('should update cart item', async () => {
      const result = await cartService.updateCartItem('item-1', 2)
      expect(result).toBeDefined()
    })
  })

  describe('removeCartItem', () => {
    it('should remove cart item', async () => {
      const result = await cartService.removeCartItem('item-1')
      expect(result).toBeDefined()
    })
  })

  describe('clearCart', () => {
    it('should clear cart', async () => {
      const result = await cartService.clearCart()
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('applyCoupon', () => {
    it('should apply coupon', async () => {
      const result = await cartService.applyCoupon('SAVE10')
      expect(result).toBeDefined()
    })
  })

  describe('removeCoupon', () => {
    it('should remove coupon', async () => {
      const result = await cartService.removeCoupon()
      expect(result).toBeDefined()
    })
  })
})