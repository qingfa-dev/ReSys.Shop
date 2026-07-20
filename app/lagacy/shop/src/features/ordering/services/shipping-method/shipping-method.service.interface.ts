import type { Result } from '@/core/models/result'
import type { ShippingMethod } from '../../types'

export interface IShippingMethodService {
  getShippingMethods(): Promise<Result<ShippingMethod[]>>
}