import { describe, it, expect } from 'vitest'
import { promotionService } from '../promotion.service'

describe('PromotionService', () => {
  describe('getActivePromotions', () => {
    it('should return active promotions', async () => {
      const result = await promotionService.getActivePromotions()
      expect(result).toBeDefined()
    })
  })

  describe('getPromotionByCode', () => {
    it('should return promotion by code', async () => {
      const result = await promotionService.getPromotionByCode('SUMMER2026')
      expect(result).toBeDefined()
    })
  })

  describe('applyPromotion', () => {
    it('should apply promotion', async () => {
      const result = await promotionService.applyPromotion('SUMMER2026')
      expect(result).toBeDefined()
    })
  })

  describe('getPromotionsHistory', () => {
    it('should return promotions history', async () => {
      const result = await promotionService.getPromotionsHistory()
      expect(result).toBeDefined()
    })
  })
})