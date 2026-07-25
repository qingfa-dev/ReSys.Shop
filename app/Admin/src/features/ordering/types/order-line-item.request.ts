export interface AddLineItemRequest {
  variantId: string
  quantity: number
  unitPrice: number
}

export interface UpdateLineItemRequest {
  quantity: number
  unitPrice: number
}
