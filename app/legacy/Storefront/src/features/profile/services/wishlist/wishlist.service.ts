import { wishlistApiRepository } from '../../repositories/wishlist/wishlist.api'
import type { IWishlistService } from './wishlist.service.interface'
import type { Wishlist, WishedItem } from '../../types/entity/wishlist.entity'
import type { CreateWishlistRequest, UpdateWishlistRequest, AddWishlistItemRequest } from '../../types/request/wishlist.request'
import type { Result } from '@/core/models/result'

export class WishlistService implements IWishlistService {
  private readonly repo = wishlistApiRepository

  async getWishlists(): Promise<Result<Wishlist[]>> {
    return this.repo.getWishlists()
  }

  async getWishlist(id: string): Promise<Result<Wishlist>> {
    return this.repo.getWishlist(id)
  }

  async createWishlist(data: CreateWishlistRequest): Promise<Result<Wishlist>> {
    return this.repo.createWishlist(data)
  }

  async updateWishlist(id: string, data: UpdateWishlistRequest): Promise<Result<Wishlist>> {
    return this.repo.updateWishlist(id, data)
  }

  async deleteWishlist(id: string): Promise<Result<void>> {
    return this.repo.deleteWishlist(id)
  }

  async addItem(wishlistId: string, data: AddWishlistItemRequest): Promise<Result<WishedItem>> {
    return this.repo.addItem(wishlistId, data)
  }

  async removeItem(wishlistId: string, itemId: string): Promise<Result<void>> {
    return this.repo.removeItem(wishlistId, itemId)
  }
}

export const wishlistService = new WishlistService()
