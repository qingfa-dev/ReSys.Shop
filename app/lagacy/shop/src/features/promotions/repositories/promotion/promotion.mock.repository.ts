import type { PromotionResponse } from '../../types/response'
import type { IPromotionRepository } from './promotion.repository.interface'
import type { Result } from '@/core/models/result'

const initialPromotions: PromotionResponse[] = [
  { id: 'promo-1', code: 'SUMMER25', type: 'percentage', value: 25, usedCount: 0, startsAt: '2024-06-01', expiresAt: '2024-08-31', isActive: true },
  { id: 'promo-2', code: 'FREESHIP', type: 'shipping', value: 0, usedCount: 0, startsAt: '2024-01-01', expiresAt: '2024-12-31', isActive: true },
]

const mockPromotions: PromotionResponse[] = JSON.parse(JSON.stringify(initialPromotions))

let appliedPromoCode: string | null = null

export class MockPromotionRepository implements IPromotionRepository {
  static reset() {
    mockPromotions.length = 0
    initialPromotions.forEach(p => mockPromotions.push({ ...p }))
    appliedPromoCode = null
  }

  async getActive(): Promise<Result<PromotionResponse[]>> {
    const active = mockPromotions.filter(p => p.isActive)
    return { isSuccess: true, isFailure: false, statusCode: 200, data: active }
  }

  async getByCode(code: string): Promise<Result<PromotionResponse>> {
    const promo = mockPromotions.find(p => p.code === code)
    if (!promo) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Promotion not found' }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: promo }
  }

  async apply(code: string): Promise<Result<void>> {
    const promo = mockPromotions.find(p => p.code === code && p.isActive)
    if (!promo) {
      return { isSuccess: false, isFailure: true, statusCode: 400, message: 'Invalid or expired promotion' }
    }
    appliedPromoCode = code
    return { isSuccess: true, isFailure: false, statusCode: 200, data: undefined }
  }

  async getHistory(): Promise<Result<PromotionResponse[]>> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mockPromotions }
  }
}

export const mockPromotionRepository = new MockPromotionRepository()