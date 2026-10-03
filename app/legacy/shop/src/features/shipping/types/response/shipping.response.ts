import type { Result } from '@/core/models/result'
import type { ShippingRateSchemaType, ShipmentSchemaType } from '../schemas'

export interface ShippingRateResponse extends ShippingRateSchemaType {}
export interface ShipmentResponse extends ShipmentSchemaType {}

export interface GetShippingRatesResponse {
  rates: ShippingRateResponse[]
  totalCount: number
}

export interface GetShipmentResponse {
  shipment: ShipmentResponse
}

export interface GetShipmentsByOrderResponse {
  shipments: ShipmentResponse[]
  totalCount: number
}

export interface CalculateShippingCostResponse {
  cost: number
  currency: string
  breakdown?: {
    baseCost: number
    distanceCost: number
    weightCost: number
  }
}

export interface GetEstimatedDeliveryResponse {
  estimatedDelivery: string
  guaranteed: boolean
}

export type ShippingRateListResponse = Result<ShippingRateResponse[]>
export type ShipmentSingleResponse = Result<ShipmentResponse>
export type ShipmentListResponse = Result<ShipmentResponse[]>