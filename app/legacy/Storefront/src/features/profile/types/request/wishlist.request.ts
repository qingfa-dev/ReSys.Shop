export interface CreateWishlistRequest {
  name: string
  isPublic?: boolean
}

export interface UpdateWishlistRequest {
  name?: string
  isPublic?: boolean
}

export interface AddWishlistItemRequest {
  productId: string
  variantId: string
  note?: string
}
