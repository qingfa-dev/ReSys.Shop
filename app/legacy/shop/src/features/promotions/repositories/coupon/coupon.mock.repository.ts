import type { CouponResponse } from '../../types/response'
import type { ICouponRepository } from './coupon.repository.interface'
import type { Result } from '@/core/models/result'

const mockCoupons: Record<string, CouponResponse> = {
  'SAVE20': {
    code: 'SAVE20',
    description: 'Save $20 on orders over $100',
    discount: '20',
    minOrderAmount: 100,
    expiresAt: '2024-12-31',
  },
}

export class MockCouponRepository implements ICouponRepository {
  async validate(code: string, orderTotal?: number): Promise<Result<CouponResponse>> {
    const coupon = mockCoupons[code]
    if (!coupon) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Coupon not found' }
    }
    if (coupon.minOrderAmount && orderTotal !== undefined && orderTotal < coupon.minOrderAmount) {
      return { isSuccess: false, isFailure: true, statusCode: 400, message: `Minimum purchase of $${coupon.minOrderAmount} required` }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: coupon }
  }
}

export const mockCouponRepository = new MockCouponRepository()