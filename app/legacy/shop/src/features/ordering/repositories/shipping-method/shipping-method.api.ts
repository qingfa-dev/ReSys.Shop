import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { ShippingMethodResponse } from '../../types/response'
import type { IShippingMethodRepository } from './shipping-method.repository.interface'

export class ShippingMethodApiRepository extends BaseRepository implements IShippingMethodRepository {
  async getAll(): Promise<Result<ShippingMethodResponse[]>> {
    return this.get<ShippingMethodResponse[]>('/ordering/shipping-methods')
  }

  getById<T = ShippingMethodResponse>(id: string): Promise<Result<T>> {
    return super.getById<T>('/ordering/shipping-methods', id)
  }
}

export const shippingMethodApiRepository = new ShippingMethodApiRepository()