import type { Wishlist, WishedItem } from '../../types/entity/wishlist.entity'
import type { CreateWishlistRequest, UpdateWishlistRequest, AddWishlistItemRequest } from '../../types/request/wishlist.request'
import type { Result } from '@/core/models/result'

export interface IWishlistService {
  getWishlists(): Promise<Result<Wishlist[]>>
  getWishlist(id: string): Promise<Result<Wishlist>>
  createWishlist(data: CreateWishlistRequest): Promise<Result<Wishlist>>
  updateWishlist(id: string, data: UpdateWishlistRequest): Promise<Result<Wishlist>>
  deleteWishlist(id: string): Promise<Result<void>>
  addItem(wishlistId: string, data: AddWishlistItemRequest): Promise<Result<WishedItem>>
  removeItem(wishlistId: string, itemId: string): Promise<Result<void>>
}
