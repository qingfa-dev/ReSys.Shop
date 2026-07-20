import type { ShipmentResponse } from '../../types/response'
import type { IShipmentRepository } from './shipment.repository.interface'
import type { Result } from '@/core/models/result'

const mockShipments: ShipmentResponse[] = [
  { id: 'ship-1', orderId: 'order-1', trackingNumber: 'TRK123456789', carrier: 'FedEx', status: 'delivered', estimatedDelivery: '2024-01-15T00:00:00Z', deliveredAt: '2024-01-14T00:00:00Z' },
  { id: 'ship-2', orderId: 'order-2', trackingNumber: 'TRK987654321', carrier: 'UPS', status: 'in_transit', estimatedDelivery: '2024-01-20T00:00:00Z' },
  { id: 'ship-3', orderId: 'order-3', trackingNumber: 'TRK456789123', carrier: 'DHL', status: 'pending', estimatedDelivery: '2024-01-25T00:00:00Z' },
]

export class MockShipmentRepository implements IShipmentRepository {
  static reset() {}

  async getByTrackingNumber(trackingNumber: string): Promise<Result<ShipmentResponse>> {
    const shipment = mockShipments.find(s => s.trackingNumber === trackingNumber)
    if (!shipment) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Shipment not found' }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: shipment }
  }

  async getByOrderId(orderId: string): Promise<Result<ShipmentResponse[]>> {
    const shipments = mockShipments.filter(s => s.orderId === orderId)
    return { isSuccess: true, isFailure: false, statusCode: 200, data: shipments }
  }

  async getById<T = ShipmentResponse>(id: string): Promise<Result<T>> {
    const shipment = mockShipments.find(s => s.id === id)
    if (!shipment) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Shipment not found' }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: shipment as T }
  }
}

export const mockShipmentRepository = new MockShipmentRepository()