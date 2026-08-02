import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { CartResponse } from '../../types/response'
import type { ICartRepository } from './cart.repository.interface'

export class CartApiRepository extends BaseRepository implements ICartRepository {
  async getCart(): Promise<Result<CartResponse>> {
    return this.get<CartResponse>('/api/storefront/cart')
  }

  async createCart(): Promise<Result<CartResponse>> {
    return this.post<CartResponse>('/api/storefront/cart')
  }

  async addItem(productId: string, productName: string, productImage: string, quantity: number, price: number): Promise<Result<CartResponse>> {
    return this.post<CartResponse>('/api/storefront/cart/items', { productId, productName, productImage, quantity, price })
  }

  async updateItem(itemId: string, quantity: number): Promise<Result<CartResponse>> {
    return this.put<CartResponse>(`/api/storefront/cart/items/${itemId}`, { quantity })
  }

  async removeItem(itemId: string): Promise<Result<CartResponse>> {
    return this.delete<CartResponse>(`/api/storefront/cart/items/${itemId}`)
  }

  async clearCart(): Promise<Result<CartResponse>> {
    return this.post<CartResponse>('/api/storefront/cart/empty')
  }

  async deleteCart(): Promise<Result<CartResponse>> {
    return this.delete<CartResponse>('/api/storefront/cart')
  }

  async updateCheckoutDetails(details: Record<string, unknown>): Promise<Result<CartResponse>> {
    return this.put<CartResponse>('/api/storefront/cart', details)
  }

  async associateCart(): Promise<Result<CartResponse>> {
    return this.post<CartResponse>('/api/storefront/cart/associate')
  }

  async validateCart(): Promise<Result<CartResponse>> {
    return this.post<CartResponse>('/api/storefront/cart/validate')
  }

  async selectShippingRate(rateId: string): Promise<Result<CartResponse>> {
    return this.post<CartResponse>('/api/storefront/cart/shipping-rate', { rateId })
  }
}

export const cartApiRepository = new CartApiRepository()