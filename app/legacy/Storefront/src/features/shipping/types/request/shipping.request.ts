export interface GetShippingRatesRequest {
  destination: string
  weight: number
  dimensions?: {
    length: number
    width: number
    height: number
  }
}

export interface GetShipmentRequest {
  trackingNumber: string
}

export interface GetShipmentsByOrderRequest {
  orderId: string
}

export interface CalculateShippingCostRequest {
  rateId: string
  distance?: number
  weight?: number
}

export interface GetEstimatedDeliveryRequest {
  rateId: string
  destination: string
}