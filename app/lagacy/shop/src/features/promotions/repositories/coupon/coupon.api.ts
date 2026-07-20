import { BaseRepository, type FilterParams } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { CouponResponse } from '../../types/response'
import type { ICouponRepository } from './coupon.repository.interface'

export class CouponApiRepository extends BaseRepository implements ICouponRepository {
  async validate(code: string, orderTotal?: number): Promise<Result<CouponResponse>> {
    const filter = orderTotal ? `code:${code},orderTotal:${orderTotal}` : `code:${code}`
    const params: FilterParams = { filter }
    return this.get<CouponResponse>('/promotions/coupons/validate', params)
  }
}

export const couponApiRepository = new CouponApiRepository()