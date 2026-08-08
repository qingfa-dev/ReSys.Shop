import { get, post, put, del } from '@/shared/api/client'
import { PROFILES } from '@/shared/constants/api'
import { WishlistListItemSchema, WishlistDetailSchema } from '../validations/wishlist'
import { PagedResultSchema } from '@/shared/validations/result'
import type { Result, PagedResult } from '@/shared/types'
import type { WishlistListItem, WishlistDetail, CreateWishlistRequest, UpdateWishlistRequest, AddWishlistItemRequest } from '../types'

const wishlistList = PagedResultSchema(WishlistListItemSchema)

export class WishlistApi {
  private static readonly BASE = `${PROFILES}/wishlists`

  static async getWishlists(): Promise<PagedResult<WishlistListItem>> {
    const result = await get<PagedResult<WishlistListItem>>(this.BASE)
    if (!result.isSuccess) return result
    const parsed = wishlistList.parse({ ...result, items: result.items })
    return parsed as PagedResult<WishlistListItem>
  }

  static async getWishlist(id: string): Promise<Result<WishlistDetail>> {
    const result = await get<Result<WishlistDetail>>(`${this.BASE}/${id}`)
    if (!result.isSuccess) return result
    result.value = WishlistDetailSchema.parse(result.value)
    return result
  }

  static async createWishlist(req: CreateWishlistRequest): Promise<Result<WishlistListItem>> {
    const result = await post<Result<WishlistListItem>>(this.BASE, req)
    if (!result.isSuccess) return result
    result.value = WishlistListItemSchema.parse(result.value)
    return result
  }

  static async updateWishlist(id: string, req: UpdateWishlistRequest): Promise<Result<WishlistListItem>> {
    const result = await put<Result<WishlistListItem>>(`${this.BASE}/${id}`, req)
    if (!result.isSuccess) return result
    result.value = WishlistListItemSchema.parse(result.value)
    return result
  }

  static async deleteWishlist(id: string): Promise<Result<void>> {
    return await del<Result<void>>(`${this.BASE}/${id}`)
  }

  static async addWishlistItem(listId: string, req: AddWishlistItemRequest): Promise<Result<WishlistDetail>> {
    const result = await post<Result<WishlistDetail>>(`${this.BASE}/${listId}/items`, req)
    if (!result.isSuccess) return result
    result.value = WishlistDetailSchema.parse(result.value)
    return result
  }

  static async removeWishlistItem(listId: string, itemId: string): Promise<Result<WishlistDetail>> {
    const result = await del<Result<WishlistDetail>>(`${this.BASE}/${listId}/items/${itemId}`)
    if (!result.isSuccess) return result
    result.value = WishlistDetailSchema.parse(result.value)
    return result
  }
}
