import type { Result } from '@/core/models/result'
import type { ShippingRateResponse } from '../../types/response'

export interface IShippingRateRepository {
  getAll(destination: string, weight: number): Promise<Result<ShippingRateResponse[]>>
  getById<T = ShippingRateResponse>(id: string): Promise<Result<T>>
  calculateCost(rateId: string, distance?: number): Promise<Result<number>>
  getEstimatedDelivery(rateId: string, destination: string): Promise<Result<string>>
}