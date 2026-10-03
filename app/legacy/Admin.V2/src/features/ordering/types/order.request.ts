export interface CreateOrderRequest {
  customerId: string
  notes?: string | null
  lineItems: { variantId: string; quantity: number; unitPrice: number }[]
}

export interface UpdateOrderStatusRequest {
  status: string
}

export interface UpdateAddressRequest {
  firstName: string
  lastName: string
  address1: string
  address2?: string | null
  city: string
  state?: string | null
  postalCode: string
  country: string
  phone?: string | null
}
