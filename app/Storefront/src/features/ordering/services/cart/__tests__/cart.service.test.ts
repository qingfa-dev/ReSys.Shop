import { describe, it, expect, vi, beforeEach } from 'vitest'
import { cartService } from '../cart.service'
import { cartApiRepository } from '../../../repositories/cart/cart.api'
import type { CartResponse } from '../../../types/response'

vi.mock('../../../repositories/cart/cart.api', () => ({
  cartApiRepository: {
    getCart: vi.fn(),
    addItem: vi.fn(),
    updateItem: vi.fn(),
    removeItem: vi.fn(),
    clearCart: vi.fn(),
  },
}))

function mockCartResponse(): CartResponse {
  return {
    id: 'cart-1',
    items: [
      { variantId: 'var-1', variantName: 'Variant', sku: 'SKU-1', quantity: 2, price: 19.99, productName: 'Product' },
    ],
    itemTotal: 39.98,
    total: 39.98,
    currency: 'USD',
    itemCount: 2,
    checkoutState: 'cart',
    tax: 0,
    shipping: 0,
    discount: 0,
  }
}

describe('CartService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('getCart', () => {
    it('should return the mapped cart', async () => {
      vi.mocked(cartApiRepository.getCart).mockResolvedValue({ isSuccess: true, isFailure: false, statusCode: 200, data: mockCartResponse() })
      const result = await cartService.getCart()
      expect(result.isSuccess).toBe(true)
      expect(result.data?.id).toBe('cart-1')
      expect(result.data?.subtotal).toBe(39.98) // maps from backend itemTotal
      expect(result.data?.items).toHaveLength(1)
      expect(result.data?.items[0]?.variantId).toBe('var-1')
    })
  })

  describe('addToCart', () => {
    it('should add an item by variantId and quantity', async () => {
      vi.mocked(cartApiRepository.addItem).mockResolvedValue({ isSuccess: true, isFailure: false, statusCode: 200, data: mockCartResponse() })
      const result = await cartService.addToCart('prod-1', 2)
      expect(cartApiRepository.addItem).toHaveBeenCalledWith('prod-1', 2)
      expect(result.isSuccess).toBe(true)
      expect(result.data?.items[0]?.variantId).toBe('var-1')
    })
  })

  describe('updateCartItem', () => {
    it('should update cart item', async () => {
      vi.mocked(cartApiRepository.updateItem).mockResolvedValue({ isSuccess: true, isFailure: false, statusCode: 200, data: mockCartResponse() })
      const result = await cartService.updateCartItem('item-1', 2)
      expect(cartApiRepository.updateItem).toHaveBeenCalledWith('item-1', 2)
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('removeCartItem', () => {
    it('should remove cart item', async () => {
      vi.mocked(cartApiRepository.removeItem).mockResolvedValue({ isSuccess: true, isFailure: false, statusCode: 200, data: mockCartResponse() })
      const result = await cartService.removeCartItem('item-1')
      expect(cartApiRepository.removeItem).toHaveBeenCalledWith('item-1')
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('clearCart', () => {
    it('should clear cart', async () => {
      vi.mocked(cartApiRepository.clearCart).mockResolvedValue({ isSuccess: true, isFailure: false, statusCode: 200, data: mockCartResponse() })
      const result = await cartService.clearCart()
      expect(cartApiRepository.clearCart).toHaveBeenCalled()
      expect(result.isSuccess).toBe(true)
    })
  })
})
