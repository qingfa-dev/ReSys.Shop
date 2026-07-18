export interface ShippingRateListItem {
  id: string
  shippingMethodId: string
  shippingMethodName: string
  name: string
  rate: number
  fromWeight: number | null
  toWeight: number | null
  fromTotal: number | null
  toTotal: number | null
  createdAtUtc: string
}

export type ShippingRateDetail = ShippingRateListItem
