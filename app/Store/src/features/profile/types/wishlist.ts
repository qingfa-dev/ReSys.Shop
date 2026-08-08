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
  name?: string
  isPrivate?: boolean
  isDefault?: boolean
}

export interface AddWishlistItemRequest {
  variantId: string
  quantity: number
}
