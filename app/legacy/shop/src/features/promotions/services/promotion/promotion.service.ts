import { promotionApiRepository } from '../../repositories/promotion/promotion.api'
import { mockPromotionRepository } from '../../repositories/promotion/promotion.mock.repository'
import type { IPromotionService } from './promotion.service.interface'
import type { Promotion } from '../../types'
import type { Result } from '@/core/models/result'
import { toPromotion } from '../../mapping'
import { resultMap } from '@/core/utils/result-helpers'

const USE_MOCK = true

export class PromotionService implements IPromotionService {
  private readonly promotionRepository = USE_MOCK ? mockPromotionRepository : promotionApiRepository

  async getActivePromotions(): Promise<Result<Promotion[]>> {
    const response = await this.promotionRepository.getActive()
    if (response.isFailure) {
      return response as unknown as Result<Promotion[]>
    }
    return resultMap(response, (data) => data.map(toPromotion))
  }

  async getPromotionByCode(code: string): Promise<Result<Promotion>> {
    const response = await this.promotionRepository.getByCode(code)
    return resultMap(response, toPromotion)
  }

  async applyPromotion(code: string): Promise<Result<void>> {
    return (await this.promotionRepository.apply(code)) as unknown as Result<void>
  }

  async getPromotionsHistory(): Promise<Result<Promotion[]>> {
    const response = await this.promotionRepository.getHistory()
    if (response.isFailure) {
      return response as unknown as Result<Promotion[]>
    }
    return resultMap(response, (data) => data.map(toPromotion))
  }
}

export const promotionService = new PromotionService()