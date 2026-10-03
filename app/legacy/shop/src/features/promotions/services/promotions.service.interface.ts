import type { Result } from '@/core/models/result'
import type { Promotion, Coupon } from '../types'

export interface IPromotionsService {
  getActivePromotions(): Promise<Result<Promotion[]>>
  getPromotionByCode(code: string): Promise<Result<Promotion>>
  validateCoupon(code: string, orderTotal?: number): Promise<Result<Coupon>>
  applyPromotion(code: string): Promise<Result<void>>
  getPromotionsHistory(): Promise<Result<Promotion[]>>
}