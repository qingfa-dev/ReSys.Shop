import { describe, it, expect } from 'vitest'
import {
  toPromotion,
  fromPromotion,
  toCoupon,
  fromCoupon,
  isPromotionExpired,
  isPromotionActive,
  formatDiscount,
} from '../mapping/promotions.mapping'
import { PromotionSchema, CouponSchema } from '../types/schemas'

describe('Promotions Mapping', () => {
  describe('toPromotion', () => {
    it('should convert schema to entity', () => {
      const schema = PromotionSchema.parse({
        id: 'promo-1',
        code: 'SAVE10',
        type: 'percentage',
        value: 10,
        usedCount: 0,
        startsAt: '2026-01-01T00:00:00Z',
        expiresAt: '2026-12-31T23:59:59Z',
        isActive: true,
      })
      const result = toPromotion(schema)
      expect(result.code).toBe('SAVE10')
      expect(result.value).toBe(10)
    })
  })

  describe('isPromotionExpired', () => {
    it('should return true for expired promotion', () => {
      const promotion = {
        id: 'promo-1',
        code: 'EXPIRED',
        type: 'percentage' as const,
        value: 10,
        usedCount: 0,
        startsAt: '2025-01-01T00:00:00Z',
        expiresAt: '2025-12-31T23:59:59Z',
        isActive: true,
      }
      expect(isPromotionExpired(promotion)).toBe(true)
    })

    it('should return false for active promotion', () => {
      const promotion = {
        id: 'promo-1',
        code: 'ACTIVE',
        type: 'percentage' as const,
        value: 10,
        usedCount: 0,
        startsAt: '2026-01-01T00:00:00Z',
        expiresAt: '2026-12-31T23:59:59Z',
        isActive: true,
      }
      expect(isPromotionExpired(promotion)).toBe(false)
    })
  })

  describe('isPromotionActive', () => {
    it('should return true for active promotion', () => {
      const promotion = {
        id: 'promo-1',
        code: 'ACTIVE',
        type: 'percentage' as const,
        value: 10,
        usedCount: 0,
        startsAt: '2026-01-01T00:00:00Z',
        expiresAt: '2026-12-31T23:59:59Z',
        isActive: true,
      }
      expect(isPromotionActive(promotion)).toBe(true)
    })

    it('should return false for inactive promotion', () => {
      const promotion = {
        id: 'promo-1',
        code: 'INACTIVE',
        type: 'percentage' as const,
        value: 10,
        usedCount: 0,
        startsAt: '2026-01-01T00:00:00Z',
        expiresAt: '2026-12-31T23:59:59Z',
        isActive: false,
      }
      expect(isPromotionActive(promotion)).toBe(false)
    })
  })

  describe('formatDiscount', () => {
    it('should format percentage discount', () => {
      const promotion = { type: 'percentage' as const, value: 10, id: '', code: '', usedCount: 0, startsAt: '', expiresAt: '', isActive: true }
      expect(formatDiscount(promotion)).toBe('10% off')
    })

    it('should format fixed discount', () => {
      const promotion = { type: 'fixed' as const, value: 20, id: '', code: '', usedCount: 0, startsAt: '', expiresAt: '', isActive: true }
      expect(formatDiscount(promotion)).toBe('$20 off')
    })

    it('should format shipping discount', () => {
      const promotion = { type: 'shipping' as const, value: 0, id: '', code: '', usedCount: 0, startsAt: '', expiresAt: '', isActive: true }
      expect(formatDiscount(promotion)).toBe('Free shipping')
    })

    it('should format bogo discount', () => {
      const promotion = { type: 'bogo' as const, value: 100, id: '', code: '', usedCount: 0, startsAt: '', expiresAt: '', isActive: true }
      expect(formatDiscount(promotion)).toBe('Buy 2 get 1 free')
    })
  })

  describe('toCoupon', () => {
    it('should convert schema to entity', () => {
      const schema = CouponSchema.parse({
        code: 'SAVE10',
        description: '10% off',
        discount: '10%',
      })
      const result = toCoupon(schema)
      expect(result.code).toBe('SAVE10')
    })
  })
})