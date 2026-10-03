import { shippingMethodApiRepository } from '../../repositories/shipping-method/shipping-method.api'
import { mockShippingMethodRepository } from '../../repositories/shipping-method/shipping-method.mock.repository'
import type { IShippingMethodService } from './shipping-method.service.interface'
import type { ShippingMethod } from '../../types'
import type { Result } from '@/core/models/result'
import { succeed, fail } from '@/core/utils/result-helpers'

const USE_MOCK = true

export class ShippingMethodService implements IShippingMethodService {
  private shippingRepo = USE_MOCK ? mockShippingMethodRepository : shippingMethodApiRepository

  async getShippingMethods(): Promise<Result<ShippingMethod[]>> {
    const response = await this.shippingRepo.getAll() as Result<ShippingMethod[]>
    if (response.isFailure) {
      return fail(response.message ?? 'Failed to get shipping methods', response.statusCode, response.errors)
    }
    return succeed(response.data!.map(sm => ({
      id: sm.id,
      name: sm.name,
      description: sm.carrier ?? 'Standard shipping',
      price: sm.price,
      estimatedDays: sm.estimatedDays ?? 3,
    })), response.statusCode)
  }
}

export const shippingMethodService = new ShippingMethodService()