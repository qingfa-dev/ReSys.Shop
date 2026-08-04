import type { Result } from '@/core/models/result'
import type { ShippingMethodResponse } from '../../types/response'

export interface IShippingMethodRepository {
  getAll(): Promise<Result<ShippingMethodResponse[]>>
  getById<T = ShippingMethodResponse>(id: string): Promise<Result<T>>
}