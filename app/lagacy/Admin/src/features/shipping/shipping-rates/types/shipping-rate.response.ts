export interface ShippingRateListItem {
  id: string
  name: string
  cost: number
  currency: string
  shippingMethodId: string
  isActive: boolean
}

export type ShippingRateDetail = ShippingRateListItem
