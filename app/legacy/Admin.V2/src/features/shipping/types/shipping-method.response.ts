export interface ShippingMethodResponse {
  id: string
  name: string
  code: string
  description?: string | null
  isActive: boolean
  displayOrder: number
  estimatedDeliveryMin?: number | null
  estimatedDeliveryMax?: number | null
  createdAt: string
  updatedAt: string
}
