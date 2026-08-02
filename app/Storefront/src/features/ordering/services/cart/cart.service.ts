import { cartApiRepository } from '../../repositories/cart/cart.api'
import { mockCartRepository } from '../../repositories/cart/cart.mock.repository'
import type { ICartService } from './cart.service.interface'
import type { Cart } from '../../types'
import type { Result } from '@/core/models/result'
import { mapCartResponseToEntity } from '../../mapping'
import { resultMap } from '@/core/utils/result-helpers'

const USE_MOCK = true

export class CartService implements ICartService {
  private cartRepo = USE_MOCK ? mockCartRepository : cartApiRepository

  async getCart(): Promise<Result<Cart>> {
    const response = await this.cartRepo.getCart()
    return resultMap(response, mapCartResponseToEntity)
  }

  async addToCart(productId: string, productName: string, productImage: string, quantity: number, price: number): Promise<Result<Cart>> {
    const response = await this.cartRepo.addItem(productId, productName, productImage, quantity, price)
    return resultMap(response, mapCartResponseToEntity)
  }

  async updateCartItem(itemId: string, quantity: number): Promise<Result<Cart>> {
    const response = await this.cartRepo.updateItem(itemId, quantity)
    return resultMap(response, mapCartResponseToEntity)
  }

  async removeCartItem(itemId: string): Promise<Result<Cart>> {
    const response = await this.cartRepo.removeItem(itemId)
    return resultMap(response, mapCartResponseToEntity)
  }

  async clearCart(): Promise<Result<Cart>> {
    const response = await this.cartRepo.clearCart()
    return resultMap(response, mapCartResponseToEntity)
  }
}

export const cartService = new CartService()