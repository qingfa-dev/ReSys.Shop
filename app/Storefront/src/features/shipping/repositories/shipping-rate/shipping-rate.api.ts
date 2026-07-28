import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { ShippingRateResponse } from '../../types/response'
import type { IShippingRateRepository } from './shipping-rate.repository.interface'

export class ShippingRateApiRepository extends BaseRepository implements IShippingRateRepository {
  async getAll(destination: string, weight: number): Promise<Result<ShippingRateResponse[]>> {
    return this.get<ShippingRateResponse[]>('/api/storefront/shipping/rates', { filter: `destination:${destination},weight:${weight}` })
  }

  async getById<T = ShippingRateResponse>(id: string): Promise<Result<T>> {
    return this.get<T>(`/api/storefront/shipping/rates/${id}`)
  }

  async calculateCost(rateId: string, distance?: number): Promise<Result<number>> {
    return this.get<number>(`/api/storefront/shipping/rates/${rateId}/calculate`, distance ? { filter: `distance:${distance}` } : undefined)
  }

  async getEstimatedDelivery(rateId: string, destination: string): Promise<Result<string>> {
    return this.get<string>(`/api/storefront/shipping/rates/${rateId}/delivery`, { filter: `destination:${destination}` })
  }
}

export const shippingRateApiRepository = new ShippingRateApiRepository()