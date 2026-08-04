import type { Result } from '@/core/models/result'
import type { ShippingRate } from '../../types'

export interface IShippingRateService {
  getShippingRates(destination: string, weight: number): Promise<Result<ShippingRate[]>>
  calculateShippingCost(rateId: string, distance?: number): Promise<Result<number>>
  getEstimatedDelivery(rateId: string, destination: string): Promise<Result<string>>
}