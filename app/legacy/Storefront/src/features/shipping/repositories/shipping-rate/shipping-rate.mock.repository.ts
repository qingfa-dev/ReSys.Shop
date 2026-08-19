import type { ShippingRateResponse } from '../../types/response'
import type { IShippingRateRepository } from './shipping-rate.repository.interface'
import type { Result } from '@/core/models/result'

const mockShippingRates: ShippingRateResponse[] = [
  { id: 'rate-1', carrier: 'FedEx', name: 'Ground', price: 9.99, estimatedDays: 5, trackingEnabled: true },
  { id: 'rate-2', carrier: 'FedEx', name: 'Express', price: 24.99, estimatedDays: 2, trackingEnabled: true },
  { id: 'rate-3', carrier: 'UPS', name: 'Standard', price: 7.99, estimatedDays: 7, trackingEnabled: true },
  { id: 'rate-4', carrier: 'UPS', name: 'Next Day', price: 49.99, estimatedDays: 1, trackingEnabled: true },
]

export class MockShippingRateRepository implements IShippingRateRepository {
  static reset() {}

  async getAll(_destination: string, _weight: number): Promise<Result<ShippingRateResponse[]>> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mockShippingRates }
  }

  async getById<T = ShippingRateResponse>(id: string): Promise<Result<T>> {
    const rate = mockShippingRates.find(r => r.id === id)
    if (!rate) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Shipping rate not found' }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: rate as T }
  }

  async calculateCost(rateId: string, distance?: number): Promise<Result<number>> {
    const rate = mockShippingRates.find(r => r.id === rateId)
    if (!rate) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Shipping rate not found' }
    }
    const distanceCost = distance ? distance * 0.1 : 0
    const totalCost = rate.price + distanceCost
    return { isSuccess: true, isFailure: false, statusCode: 200, data: totalCost }
  }

  async getEstimatedDelivery(rateId: string, _destination: string): Promise<Result<string>> {
    const rate = mockShippingRates.find(r => r.id === rateId)
    if (!rate) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Shipping rate not found' }
    }
    const deliveryDate = new Date()
    deliveryDate.setDate(deliveryDate.getDate() + rate.estimatedDays)
    return { isSuccess: true, isFailure: false, statusCode: 200, data: deliveryDate.toISOString() }
  }
}

export const mockShippingRateRepository = new MockShippingRateRepository()