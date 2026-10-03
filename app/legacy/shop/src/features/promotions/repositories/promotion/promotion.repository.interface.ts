import type { Result } from '@/core/models/result'
import type { PromotionResponse } from '../../types/response'

export interface IPromotionRepository {
  getActive(): Promise<Result<PromotionResponse[]>>
  getByCode(code: string): Promise<Result<PromotionResponse>>
  apply(code: string): Promise<Result<void>>
  getHistory(): Promise<Result<PromotionResponse[]>>
}