import type { Result } from '@/core/models/result'
import type { Cart } from '../../types'

export interface ICartService {
  getCart(): Promise<Result<Cart>>
  addToCart(productId: string, productName: string, productImage: string, quantity: number, price: number): Promise<Result<Cart>>
  updateCartItem(itemId: string, quantity: number): Promise<Result<Cart>>
  removeCartItem(itemId: string): Promise<Result<Cart>>
  clearCart(): Promise<Result<Cart>>
}