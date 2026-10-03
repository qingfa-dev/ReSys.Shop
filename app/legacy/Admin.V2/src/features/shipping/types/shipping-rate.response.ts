export interface ShippingRateResponse {
  id: string
  shippingMethodId: string
  shippingMethodName?: string | null
  name: string
  rate: number
  currency: string
  minOrderAmount?: number | null
  maxOrderAmount?: number | null
  minWeight?: number | null
  maxWeight?: number | null
  createdAt: string
  updatedAt: string
}
