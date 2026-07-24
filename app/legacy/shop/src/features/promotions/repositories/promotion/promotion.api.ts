import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { PromotionResponse } from '../../types/response'
import type { IPromotionRepository } from './promotion.repository.interface'

export class PromotionApiRepository extends BaseRepository implements IPromotionRepository {
  async getActive(): Promise<Result<PromotionResponse[]>> {
    return this.get<PromotionResponse[]>('/promotions/active')
  }

  async getByCode(code: string): Promise<Result<PromotionResponse>> {
    return this.get<PromotionResponse>(`/promotions/code/${code}`)
  }

  async apply(code: string): Promise<Result<void>> {
    return this.post<void>('/promotions/apply', { code })
  }

  async getHistory(): Promise<Result<PromotionResponse[]>> {
    return this.get<PromotionResponse[]>('/promotions/history')
  }
}

export const promotionApiRepository = new PromotionApiRepository()