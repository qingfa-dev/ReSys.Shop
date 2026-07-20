import { shippingRateApiRepository } from '../../repositories/shipping-rate/shipping-rate.api'
import { mockShippingRateRepository } from '../../repositories/shipping-rate/shipping-rate.mock.repository'
import type { IShippingRateService } from './shipping-rate.service.interface'
import type { ShippingRate } from '../../types'
import type { Result } from '@/core/models/result'
import { toShippingRate } from '../../mapping'
import { resultMap } from '@/core/utils/result-helpers'

const USE_MOCK = true

export class ShippingRateService implements IShippingRateService {
  private readonly shippingRateRepository = USE_MOCK ? mockShippingRateRepository : shippingRateApiRepository

  async getShippingRates(destination: string, weight: number): Promise<Result<ShippingRate[]>> {
    const response = await this.shippingRateRepository.getAll(destination, weight)
    if (response.isFailure) {
      return response as unknown as Result<ShippingRate[]>
    }
    return resultMap(response, (data) => data.map(toShippingRate))
  }

  async calculateShippingCost(rateId: string, distance?: number): Promise<Result<number>> {
    return (await this.shippingRateRepository.calculateCost(rateId, distance)) as unknown as Result<number>
  }

  async getEstimatedDelivery(rateId: string, destination: string): Promise<Result<string>> {
    return (await this.shippingRateRepository.getEstimatedDelivery(rateId, destination)) as unknown as Result<string>
  }
}

export const shippingRateService = new ShippingRateService()