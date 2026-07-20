import type { Result } from '@/core/models/result'
import type { CartResponse } from '../../types/response'

export interface ICartRepository {
  getCart(): Promise<Result<CartResponse>>
  addItem(productId: string, productName: string, productImage: string, quantity: number, price: number): Promise<Result<CartResponse>>
  updateItem(itemId: string, quantity: number): Promise<Result<CartResponse>>
  removeItem(itemId: string): Promise<Result<CartResponse>>
  applyCoupon(code: string): Promise<Result<CartResponse>>
  removeCoupon(): Promise<Result<CartResponse>>
  clearCart(): Promise<Result<CartResponse>>
}