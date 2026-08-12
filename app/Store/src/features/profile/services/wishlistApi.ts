import { get, post, patch, del } from '@/shared/api/client'
import { WishlistListItemSchema, WishlistDetailSchema } from '../validations/wishlist'
import { PagedResultSchema } from '@/shared/validations/result'
import type { Result, PagedResult } from '@/shared/types'
import type { WishlistListItem, WishlistDetail, CreateWishlistRequest, UpdateWishlistRequest, AddWishlistItemRequest } from '../types'

// Validate: Paged list schema for wishlist collection endpoint
const wishlistList = PagedResultSchema(WishlistListItemSchema)

export class WishlistApi {
  // Call: Fetch all wishlists for the authenticated user
  static async getWishlists(): Promise<PagedResult<WishlistListItem>> {
    const result = await get<PagedResult<WishlistListItem>>('/api/storefront/customer/wishlists')
    if (!result.isSuccess) return result
    // Transform: Parse paged result with wishlist item schema
    const parsed = wishlistList.parse({ ...result, items: result.items })
    return parsed as PagedResult<WishlistListItem>
  }

  // Call: Fetch full detail of a single wishlist by id
  static async getWishlist(id: string): Promise<Result<WishlistDetail>> {
    const result = await get<Result<WishlistDetail>>(`/api/storefront/customer/wishlists/${id}`)
    if (!result.isSuccess) return result
    result.value = WishlistDetailSchema.parse(result.value)
    return result
  }

  // Call: Create a new wishlist
  static async createWishlist(req: CreateWishlistRequest): Promise<Result<WishlistListItem>> {
    const result = await post<Result<WishlistListItem>>('/api/storefront/customer/wishlists', req)
    if (!result.isSuccess) return result
    result.value = WishlistListItemSchema.parse(result.value)
    return result
  }

  // Call: Update wishlist metadata (name, privacy, default flag)
  static async updateWishlist(id: string, req: UpdateWishlistRequest): Promise<Result<WishlistListItem>> {
    const result = await patch<Result<WishlistListItem>>(`/api/storefront/customer/wishlists/${id}`, req)
    if (!result.isSuccess) return result
    result.value = WishlistListItemSchema.parse(result.value)
    return result
  }

  // Call: Delete a wishlist and all its items
  static async deleteWishlist(id: string): Promise<Result<void>> {
    return await del<Result<void>>(`/api/storefront/customer/wishlists/${id}`)
  }

  // Call: Add a product variant to a wishlist
  static async addWishlistItem(listId: string, req: AddWishlistItemRequest): Promise<Result<WishlistDetail>> {
    const result = await post<Result<WishlistDetail>>(`/api/storefront/customer/wishlists/${listId}/items`, req)
    if (!result.isSuccess) return result
    result.value = WishlistDetailSchema.parse(result.value)
    return result
  }

  // Call: Remove a single item from a wishlist
  static async removeWishlistItem(listId: string, itemId: string): Promise<Result<WishlistDetail>> {
    const result = await del<Result<WishlistDetail>>(`/api/storefront/customer/wishlists/${listId}/items/${itemId}`)
    if (!result.isSuccess) return result
    result.value = WishlistDetailSchema.parse(result.value)
    return result
  }
}
