import type { Result } from '@/core/models/result'
import type { Promotion } from '../../types'

export interface IPromotionService {
  getActivePromotions(): Promise<Result<Promotion[]>>
  getPromotionByCode(code: string): Promise<Result<Promotion>>
  applyPromotion(code: string): Promise<Result<void>>
  getPromotionsHistory(): Promise<Result<Promotion[]>>
}