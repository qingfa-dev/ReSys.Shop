import { BaseRepository } from '@/core/repositories'
import type { Wishlist, WishedItem } from '../../types/entity/wishlist.entity'
import type { CreateWishlistRequest, UpdateWishlistRequest, AddWishlistItemRequest } from '../../types/request/wishlist.request'
import type { Result } from '@/core/models/result'

const ENDPOINT = '/api/storefront/profiles/wishlists'

export class WishlistApiRepository extends BaseRepository {
  async getWishlists(): Promise<Result<Wishlist[]>> {
    return this.get<Wishlist[]>(ENDPOINT)
  }

  async getWishlist(id: string): Promise<Result<Wishlist>> {
    return this.get<Wishlist>(`${ENDPOINT}/${id}`)
  }

  async createWishlist(data: CreateWishlistRequest): Promise<Result<Wishlist>> {
    return this.post<Wishlist>(ENDPOINT, data)
  }

  async updateWishlist(id: string, data: UpdateWishlistRequest): Promise<Result<Wishlist>> {
    return this.put<Wishlist>(`${ENDPOINT}/${id}`, data)
  }

  async deleteWishlist(id: string): Promise<Result<void>> {
    return this.delete<void>(`${ENDPOINT}/${id}`)
  }

  async addItem(wishlistId: string, data: AddWishlistItemRequest): Promise<Result<WishedItem>> {
    return this.post<WishedItem>(`${ENDPOINT}/${wishlistId}/items`, data)
  }

  async removeItem(wishlistId: string, itemId: string): Promise<Result<void>> {
    return this.delete<void>(`${ENDPOINT}/${wishlistId}/items/${itemId}`)
  }
}

export const wishlistApiRepository = new WishlistApiRepository()
