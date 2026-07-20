import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { CartResponse } from '../../types/response'
import type { ICartRepository } from './cart.repository.interface'

export class CartApiRepository extends BaseRepository implements ICartRepository {
  async getCart(): Promise<Result<CartResponse>> {
    return this.get<CartResponse>('/ordering/cart')
  }

  async addItem(productId: string, productName: string, productImage: string, quantity: number, price: number): Promise<Result<CartResponse>> {
    return this.post<CartResponse>('/ordering/cart/items', { productId, productName, productImage, quantity, price })
  }

  async updateItem(itemId: string, quantity: number): Promise<Result<CartResponse>> {
    return this.patch<CartResponse>(`/ordering/cart/items/${itemId}`, { quantity })
  }

  async removeItem(itemId: string): Promise<Result<CartResponse>> {
    return this.delete<CartResponse>(`/ordering/cart/items/${itemId}`)
  }

  async applyCoupon(code: string): Promise<Result<CartResponse>> {
    return this.post<CartResponse>('/ordering/cart/coupon', { code })
  }

  async removeCoupon(): Promise<Result<CartResponse>> {
    return this.delete<CartResponse>('/ordering/cart/coupon')
  }

  async clearCart(): Promise<Result<CartResponse>> {
    return this.delete<CartResponse>('/ordering/cart')
  }
}

export const cartApiRepository = new CartApiRepository()