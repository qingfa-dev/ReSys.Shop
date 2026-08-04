import { describe, it, expect } from 'vitest'
import { mockCouponRepository } from '../coupon.mock.repository'

describe('CouponRepository', () => {
  describe('validate', () => {
    it('should validate valid coupon', async () => {
      const result = await mockCouponRepository.validate('SAVE20')
      expect(result.isSuccess).toBe(true)
      expect(result.data?.code).toBe('SAVE20')
    })

    it('should return error for invalid coupon', async () => {
      const result = await mockCouponRepository.validate('INVALID')
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(404)
    })

    it('should validate coupon with minimum purchase', async () => {
      const result = await mockCouponRepository.validate('SAVE20', 150)
      expect(result.isSuccess).toBe(true)
    })

    it('should fail when order total below minimum', async () => {
      const result = await mockCouponRepository.validate('SAVE20', 50)
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(400)
    })
  })
})