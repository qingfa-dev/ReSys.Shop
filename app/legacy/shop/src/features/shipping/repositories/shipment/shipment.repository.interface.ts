import type { Result } from '@/core/models/result'
import type { ShipmentResponse } from '../../types/response'

export interface IShipmentRepository {
  getByTrackingNumber(trackingNumber: string): Promise<Result<ShipmentResponse>>
  getByOrderId(orderId: string): Promise<Result<ShipmentResponse[]>>
  getById<T = ShipmentResponse>(id: string): Promise<Result<T>>
}