import type { CartResponse, CartItemResponse, CartSingleResponse } from '../../types/response'

interface CartData {
  items: CartItemResponse[]
  subtotal: number
  discount: number
  total: number
  tax: number
  shipping: number
}

let mockCart: CartData = {
  items: [],
  subtotal: 0,
  discount: 0,
  total: 0,
  tax: 0,
  shipping: 0,
}

function mapToCartResponse(cart: CartData): CartResponse {
  return {
    id: 'cart-1',
    items: cart.items,
    subtotal: cart.subtotal,
    tax: cart.tax,
    shipping: cart.shipping,
    discount: cart.discount,
    total: cart.total,
    currency: 'USD',
  }
}

function recalculateCart(): void {
  mockCart.subtotal = mockCart.items.reduce((sum, item) => sum + item.price * item.quantity, 0)
  mockCart.tax = mockCart.subtotal * 0.09
  mockCart.shipping = mockCart.subtotal > 50 ? 0 : 5.99
  mockCart.total = mockCart.subtotal + mockCart.tax + (mockCart.shipping ?? 0) - mockCart.discount
}

export class MockCartRepository {
  async getCart(): Promise<CartSingleResponse> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToCartResponse(mockCart) }
  }

  async addItem(productId: string, productName: string, productImage: string, quantity: number, price: number): Promise<CartSingleResponse> {
    const newItem: CartItemResponse = { id: `item-${Date.now()}`, productId, productName, productImage, quantity, price }
    mockCart = { ...mockCart, items: [...mockCart.items, newItem] }
    recalculateCart()
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToCartResponse(mockCart) }
  }

  async updateItem(itemId: string, quantity: number): Promise<CartSingleResponse> {
    mockCart = { ...mockCart, items: mockCart.items.map(item => item.id === itemId ? { ...item, quantity } : item) }
    recalculateCart()
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToCartResponse(mockCart) }
  }

  async removeItem(itemId: string): Promise<CartSingleResponse> {
    mockCart = { ...mockCart, items: mockCart.items.filter(item => item.id !== itemId) }
    recalculateCart()
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToCartResponse(mockCart) }
  }

  async clearCart(): Promise<CartSingleResponse> {
    mockCart = { items: [], subtotal: 0, discount: 0, total: 0, tax: 0, shipping: 0 }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToCartResponse(mockCart) }
  }
}

export const mockCartRepository = new MockCartRepository()