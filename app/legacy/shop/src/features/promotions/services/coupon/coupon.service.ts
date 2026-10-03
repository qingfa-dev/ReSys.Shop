import { couponApiRepository } from '../../repositories/coupon/coupon.api'
import { mockCouponRepository } from '../../repositories/coupon/coupon.mock.repository'
import type { ICouponService } from './coupon.service.interface'
import type { Coupon } from '../../types'
import type { Result } from '@/core/models/result'
import { toCoupon } from '../../mapping'
import { resultMap } from '@/core/utils/result-helpers'

const USE_MOCK = true

export class CouponService implements ICouponService {
  private readonly couponRepository = USE_MOCK ? mockCouponRepository : couponApiRepository

  async validateCoupon(code: string, orderTotal?: number): Promise<Result<Coupon>> {
    const response = await this.couponRepository.validate(code, orderTotal)
    return resultMap(response, toCoupon)
  }
}

export const couponService = new CouponService()