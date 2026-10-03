import { shipmentApiRepository } from '../../repositories/shipment/shipment.api'
import { mockShipmentRepository } from '../../repositories/shipment/shipment.mock.repository'
import type { IShipmentService } from './shipment.service.interface'
import type { Shipment } from '../../types'
import type { Result } from '@/core/models/result'
import { toShipment } from '../../mapping'
import { resultMap } from '@/core/utils/result-helpers'

const USE_MOCK = true

export class ShipmentService implements IShipmentService {
  private readonly shipmentRepository = USE_MOCK ? mockShipmentRepository : shipmentApiRepository

  async getShipment(trackingNumber: string): Promise<Result<Shipment>> {
    const response = await this.shipmentRepository.getByTrackingNumber(trackingNumber)
    return resultMap(response, toShipment)
  }

  async getShipmentsByOrder(orderId: string): Promise<Result<Shipment[]>> {
    const response = await this.shipmentRepository.getByOrderId(orderId)
    if (response.isFailure) {
      return response as unknown as Result<Shipment[]>
    }
    return resultMap(response, (data) => data.map(toShipment))
  }
}

export const shipmentService = new ShipmentService()