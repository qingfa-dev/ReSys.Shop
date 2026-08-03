import { shippingMethodApiRepository } from '../../repositories/shipping-method/shipping-method.api'
import type { IShippingMethodService } from './shipping-method.service.interface'
import type { ShippingMethod } from '../../types'
import type { Result } from '@/core/models/result'
import { succeed, fail } from '@/core/utils/result-helpers'

export class ShippingMethodService implements IShippingMethodService {
  private shippingRepo = shippingMethodApiRepository

  async getShippingMethods(): Promise<Result<ShippingMethod[]>> {
    const response = await this.shippingRepo.getAll()
    if (response.isFailure) {
      return fail(response.message ?? 'Failed to get shipping methods', response.statusCode, response.errors)
    }
    return succeed(response.data!.map(sm => ({
      ...sm,
      price: 0,                    // backend returns no price — 0 unless a rate is applied
      estimatedDays: undefined,
      carrier: undefined,
    })), response.statusCode)
  }
}

export const shippingMethodService = new ShippingMethodService()