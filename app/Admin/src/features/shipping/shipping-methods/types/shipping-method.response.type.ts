export interface ShippingMethodListItem {
  id: string
  name: string
  serviceLevel: string
  isActive: boolean
  trackingUrl: string | null
}

export type ShippingMethodDetail = ShippingMethodListItem
