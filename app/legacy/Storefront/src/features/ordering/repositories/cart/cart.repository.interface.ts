import type { Result } from '@/core/models/result'
import type { CartResponse, ShippingMethodResponse } from '../../types/response'

export interface ICartRepository {
  getCart(): Promise<Result<CartResponse>>
  createCart(): Promise<Result<CartResponse>>
  addItem(variantId: string, quantity: number): Promise<Result<CartResponse>>
  updateItem(itemId: string, quantity: number): Promise<Result<CartResponse>>
  removeItem(itemId: string): Promise<Result<CartResponse>>
  clearCart(): Promise<Result<CartResponse>>
  deleteCart(): Promise<Result<CartResponse>>
  updateCheckoutDetails(details: Record<string, unknown>): Promise<Result<CartResponse>>
  associateCart(): Promise<Result<CartResponse>>
  validateCart(): Promise<Result<CartResponse>>
  selectShippingRate(shippingMethodId: string): Promise<Result<CartResponse>>
}