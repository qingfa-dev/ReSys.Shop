import type { Result } from '@/core/models/result'
import type { Shipment } from '../../types'

export interface IShipmentService {
  getShipment(trackingNumber: string): Promise<Result<Shipment>>
  getShipmentsByOrder(orderId: string): Promise<Result<Shipment[]>>
}