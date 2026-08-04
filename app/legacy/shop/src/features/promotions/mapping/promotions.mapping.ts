import type { Promotion, Coupon, PromotionSchemaType, CouponSchemaType } from '../types'

export function toPromotion(schema: PromotionSchemaType): Promotion {
  return {
    id: schema.id,
    code: schema.code,
    type: schema.type,
    value: schema.value,
    minOrderAmount: schema.minOrderAmount,
    maxUses: schema.maxUses,
    usedCount: schema.usedCount,
    startsAt: schema.startsAt,
    expiresAt: schema.expiresAt,
    isActive: schema.isActive,
  }
}

export function fromPromotion(promotion: Promotion): PromotionSchemaType {
  return PromotionSchema.parse(promotion)
}

export function toCoupon(schema: CouponSchemaType): Coupon {
  return {
    code: schema.code,
    description: schema.description,
    discount: schema.discount,
    expiresAt: schema.expiresAt,
  }
}

export function fromCoupon(coupon: Coupon): CouponSchemaType {
  return CouponSchema.parse(coupon)
}

export function isPromotionExpired(promotion: Promotion): boolean {
  return new Date(promotion.expiresAt) < new Date()
}

export function isPromotionActive(promotion: Promotion): boolean {
  const now = new Date()
  const start = new Date(promotion.startsAt)
  const end = new Date(promotion.expiresAt)
  return promotion.isActive && now >= start && now <= end
}

export function formatDiscount(promotion: Promotion): string {
  switch (promotion.type) {
    case 'percentage':
      return `${promotion.value}% off`
    case 'fixed':
      return `$${promotion.value} off`
    case 'shipping':
      return 'Free shipping'
    case 'bogo':
      return 'Buy 2 get 1 free'
    default:
      return `${promotion.value}`
  }
}

import { PromotionSchema, CouponSchema } from '../types/schemas'