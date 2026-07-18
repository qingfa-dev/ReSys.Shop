export interface ShippingMethodListItem {
  id: string
  name: string
  description: string | null
  carrier: string
  isActive: boolean
  displayOrder: number
  createdAtUtc: string
  modifiedAtUtc: string | null
}

export interface ShippingMethodDetail extends ShippingMethodListItem {
  configuration: Record<string, unknown> | null
}
