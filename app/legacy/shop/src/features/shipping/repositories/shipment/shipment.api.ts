import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { ShipmentResponse } from '../../types/response'
import type { IShipmentRepository } from './shipment.repository.interface'

export class ShipmentApiRepository extends BaseRepository implements IShipmentRepository {
  async getByTrackingNumber(trackingNumber: string): Promise<Result<ShipmentResponse>> {
    return this.get<ShipmentResponse>('/shipping/shipments', { filter: `trackingNumber:${trackingNumber}` })
  }

  async getByOrderId(orderId: string): Promise<Result<ShipmentResponse[]>> {
    return this.get<ShipmentResponse[]>(`/shipping/orders/${orderId}/shipments`)
  }

  async getById<T = ShipmentResponse>(id: string): Promise<Result<T>> {
    return this.get<T>(`/shipping/shipments/${id}`)
  }
}

export const shipmentApiRepository = new ShipmentApiRepository()