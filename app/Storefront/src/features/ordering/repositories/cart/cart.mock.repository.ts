import type { CartResponse, CartItemResponse, CartSingleResponse } from '../../types/response'

const MOCK_ITEM_PRICE = 19.99

interface CartData {
  items: CartItemResponse[]
  itemTotal: number
  discount: number
  total: number
  tax: number
  shipping: number
}

let mockCart: CartData = {
  items: [],
  itemTotal: 0,
  discount: 0,
  total: 0,
  tax: 0,
  shipping: 0,
}

function mapToCartResponse(cart: CartData): CartResponse {
  return {
    id: 'cart-1',
    items: cart.items,
    itemTotal: cart.itemTotal,
    tax: cart.tax,
    shipping: cart.shipping,
    discount: cart.discount,
    total: cart.total,
    currency: 'USD',
    itemCount: cart.items.reduce((sum, item) => sum + item.quantity, 0),
    checkoutState: 'cart',
  }
}

function recalculateCart(): void {
  mockCart.itemTotal = mockCart.items.reduce((sum, item) => sum + item.price * item.quantity, 0)
  mockCart.tax = mockCart.itemTotal * 0.09
  mockCart.shipping = mockCart.itemTotal > 50 ? 0 : 5.99
  mockCart.total = mockCart.itemTotal + mockCart.tax + (mockCart.shipping ?? 0) - mockCart.discount
}

export class MockCartRepository {
  async getCart(): Promise<CartSingleResponse> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToCartResponse(mockCart) }
  }

  async createCart(): Promise<CartSingleResponse> {
    mockCart = { items: [], itemTotal: 0, discount: 0, total: 0, tax: 0, shipping: 0 }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToCartResponse(mockCart) }
  }

  async addItem(variantId: string, quantity: number): Promise<CartSingleResponse> {
    const newItem: CartItemResponse = { variantId, quantity, price: MOCK_ITEM_PRICE }
    mockCart = { ...mockCart, items: [...mockCart.items, newItem] }
    recalculateCart()
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToCartResponse(mockCart) }
  }

  async updateItem(itemId: string, quantity: number): Promise<CartSingleResponse> {
    mockCart = { ...mockCart, items: mockCart.items.map(item => item.variantId === itemId ? { ...item, quantity } : item) }
    recalculateCart()
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToCartResponse(mockCart) }
  }

  async removeItem(itemId: string): Promise<CartSingleResponse> {
    mockCart = { ...mockCart, items: mockCart.items.filter(item => item.variantId !== itemId) }
    recalculateCart()
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToCartResponse(mockCart) }
  }

  async clearCart(): Promise<CartSingleResponse> {
    mockCart = { items: [], itemTotal: 0, discount: 0, total: 0, tax: 0, shipping: 0 }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToCartResponse(mockCart) }
  }

  async deleteCart(): Promise<CartSingleResponse> {
    mockCart = { items: [], itemTotal: 0, discount: 0, total: 0, tax: 0, shipping: 0 }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToCartResponse(mockCart) }
  }

  async updateCheckoutDetails(_details: Record<string, unknown>): Promise<CartSingleResponse> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToCartResponse(mockCart) }
  }

  async associateCart(): Promise<CartSingleResponse> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToCartResponse(mockCart) }
  }

  async validateCart(): Promise<CartSingleResponse> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToCartResponse(mockCart) }
  }

  async selectShippingRate(_shippingMethodId: string): Promise<CartSingleResponse> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToCartResponse(mockCart) }
  }
}

export const mockCartRepository = new MockCartRepository()