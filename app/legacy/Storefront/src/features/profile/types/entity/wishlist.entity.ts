export interface Wishlist {
  id: string
  name: string
  isPublic: boolean
  items: WishedItem[]
  itemCount: number
  createdAtUtc: string
  modifiedAtUtc: string | null
}

export interface WishedItem {
  id: string
  productId: string
  variantId: string
  productName: string
  productImage: string | null
  price: number
  note: string | null
  addedAtUtc: string
}
