import { describe, it, expect } from 'vitest'
import { couponService } from '../coupon.service'

describe('CouponService', () => {
  describe('validateCoupon', () => {
    it('should validate coupon', async () => {
      const result = await couponService.validateCoupon('SAVE20')
      expect(result).toBeDefined()
    })
  })
})