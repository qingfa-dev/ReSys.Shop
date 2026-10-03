export * from './schemas'
export * from './entity'
export * from './request'
export * from './response'

export interface ShippingRate {
  id: string
  name: string
  carrier: string
  price: number
  estimatedDays: number
  trackingEnabled: boolean
}

export interface Shipment {
  id: string
  orderId: string
  status: 'pending' | 'label_created' | 'in_transit' | 'delivered' | 'exception'
  trackingNumber?: string
  trackingUrl?: string
  carrier?: string
  estimatedDelivery?: string
}
