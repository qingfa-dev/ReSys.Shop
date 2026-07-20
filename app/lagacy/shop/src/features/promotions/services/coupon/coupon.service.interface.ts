import type { Result } from '@/core/models/result'
import type { Coupon } from '../../types'

export interface ICouponService {
  validateCoupon(code: string, orderTotal?: number): Promise<Result<Coupon>>
}