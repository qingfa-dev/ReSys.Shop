import { get, post, put, del, getPaged } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types/result'
import type {
  WishlistListItem,
  WishlistDetail,
  CreateWishlistRequest,
  UpdateWishlistRequest,
  AddWishlistItemRequest,
} from '../types/wishlist'

// GET api/store/profiles/wishlists — PagedResult envelope; no paging params → all rows.
export function getWishlists(): Promise<PagedResult<WishlistListItem>> {
  return getPaged<WishlistListItem>(ENDPOINTS.wishlists, {})
}

export function getWishlist(id: string): Promise<Result<WishlistDetail>> {
  return get<Result<WishlistDetail>>(ENDPOINTS.wishlistById(id))
}

export function createWishlist(req: CreateWishlistRequest): Promise<Result<WishlistDetail>> {
  return post<Result<WishlistDetail>>(ENDPOINTS.wishlists, req)
}

export function updateWishlist(id: string, req: UpdateWishlistRequest): Promise<Result<WishlistDetail>> {
  return put<Result<WishlistDetail>>(ENDPOINTS.wishlistById(id), req)
}

// DELETE api/store/profiles/wishlists/{id} — returns the soft-deleted detail.
export function deleteWishlist(id: string): Promise<Result<WishlistDetail>> {
  return del<Result<WishlistDetail>>(ENDPOINTS.wishlistById(id))
}

// POST api/store/profiles/wishlists/{id}/items — 201 with the updated detail.
export function addWishlistItem(id: string, req: AddWishlistItemRequest): Promise<Result<WishlistDetail>> {
  return post<Result<WishlistDetail>>(ENDPOINTS.wishlistItems(id), req)
}

// DELETE api/store/profiles/wishlists/{id}/items/{itemId} — returns the updated detail.
export function removeWishlistItem(listId: string, itemId: string): Promise<Result<WishlistDetail>> {
  return del<Result<WishlistDetail>>(ENDPOINTS.wishlistItem(listId, itemId))
}
