import type { Result } from '@/core/models/result'
import type { CouponResponse } from '../../types/response'

export interface ICouponRepository {
  validate(code: string, orderTotal?: number): Promise<Result<CouponResponse>>
}