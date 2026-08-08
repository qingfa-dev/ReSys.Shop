// Context: Lightweight wishlist summary for list views
export interface WishlistListItem {
  id: string
  name: string
  isPrivate: boolean
  itemCount: number
}

// Context: Single product variant entry within a wishlist
export interface WishedItem {
  id: string
  variantId: string
  quantity: number
  addedAtUtc: string
}

// Context: Full wishlist detail including all wished items
export interface WishlistDetail {
  id: string
  name: string
  isPrivate: boolean
  itemCount: number
  token: string
  isDefault: boolean
  wishedItems: WishedItem[]
}

// Context: Request payload for creating a new wishlist
export interface CreateWishlistRequest {
  name: string
  isPrivate: boolean
}

// Context: Partial update payload for wishlist metadata
export interface UpdateWishlistRequest {
  name?: string
  isPrivate?: boolean
  isDefault?: boolean
}

// Context: Request payload for adding a product variant to a wishlist
export interface AddWishlistItemRequest {
  variantId: string
  quantity: number
}
