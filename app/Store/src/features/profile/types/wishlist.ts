// Types mirror the storefront wishlist DTOs exactly (camelCase JSON).
// Contracts pinned from Module.Profile.Features.Store.Wishlists (service/Api):
// - GET api/store/profiles/wishlists → PagedResult<WishlistListItemResponse>
// - GET api/store/profiles/wishlists/{id} → WishlistDetailResponse
// - POST {id}/items and DELETE {id}/items/{itemId} → WishlistDetailResponse
//
// NOTE: WishedItem carries only variantId + quantity (+ addedAtUtc). The backend does
// NOT embed product name/thumbnail/price, so the UI cannot render product thumbnails
// without a separate catalog lookup (out of this contract).

export interface WishlistListItem {
  id: string
  name: string
  isPrivate: boolean
  itemCount: number
}

export interface WishedItem {
  id: string
  variantId: string
  quantity: number
  addedAtUtc: string
}

export interface WishlistDetail {
  id: string
  name: string
  isPrivate: boolean
  itemCount: number
  token: string
  isDefault: boolean
  wishedItems: WishedItem[]
}

export interface CreateWishlistRequest {
  name: string
  isPrivate: boolean
}

export interface UpdateWishlistRequest {
  name?: string | null
  isPrivate?: boolean | null
  isDefault?: boolean | null
}

export interface AddWishlistItemRequest {
  variantId: string
  quantity: number
}
