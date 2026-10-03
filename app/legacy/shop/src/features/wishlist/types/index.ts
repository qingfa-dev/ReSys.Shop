export interface WishlistItem {
    id: string
    userId: string
    productId: string
    dateAdded: string
    productName: string
    productImage: string
    productPrice: number
}

export interface Wishlist {
    id: string
    userId: string
    name: string
    items: WishlistItem[]
    createdAt: string
    updatedAt: string
    isPublic: boolean
    shareToken?: string
}
