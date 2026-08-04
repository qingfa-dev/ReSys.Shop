export interface CartLineItem {
  lineItemId: string
  variantId: string
  productId: string
  productName: string
  productSlug: string
  sku: string | null
  quantity: number
  unitPrice: number
  currency: string
  thumbnailUrl: string | null
  optionDescription: string | null
  maxQuantity: number
}

export interface CartResponse {
  id: string
  items: CartLineItem[]
  itemCount: number
  subtotal: number
  currency: string
}

export interface AddCartItemRequest {
  variantId: string
  quantity: number
}

export interface UpdateCartItemRequest {
  quantity: number
}
