export interface AddressResponse {
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

export interface OrderLineItemResponse {
  id: string
  variantId: string
  variantSku?: string | null
  variantName?: string | null
  quantity: number
  unitPrice: number
  totalPrice: number
}

export interface OrderResponse {
  id: string
  orderNumber: string
  status: string
  customerId: string
  customerName?: string | null
  customerEmail?: string | null
  subtotal: number
  total: number
  taxTotal: number
  shippingTotal: number
  currency: string
  shipAddress?: AddressResponse | null
  billAddress?: AddressResponse | null
  lineItems: OrderLineItemResponse[]
  notes?: string | null
  createdAt: string
  updatedAt: string
}
