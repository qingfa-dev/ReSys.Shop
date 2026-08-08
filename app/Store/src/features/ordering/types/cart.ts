export interface CartLineItem {
  id: string
  variantId: string
  variantName: string
  sku: string
  productName: string | null
  productImageUrl: string | null
  quantity: number
  price: number
  total: number
}

export interface CartResponse {
  id: string
  itemTotal: number
  total: number
  currency: string
  itemCount: number
  checkoutState: string
  items: CartLineItem[]
}

export interface AddCartItemRequest {
  variantId: string
  quantity: number
}

export interface UpdateCartItemRequest {
  quantity: number
}
