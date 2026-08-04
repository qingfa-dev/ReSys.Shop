import { describe, it, expect, beforeEach } from 'vitest'
import { mockPromotionRepository, MockPromotionRepository } from '../promotion.mock.repository'

describe('PromotionRepository', () => {
  beforeEach(() => {
    MockPromotionRepository.reset()
  })

  describe('getActive', () => {
    it('should return active promotions', async () => {
      const result = await mockPromotionRepository.getActive()
      expect(result.isSuccess).toBe(true)
      expect(result.data).toHaveLength(2)
      expect(result.data?.every(p => p.isActive)).toBe(true)
    })
  })

  describe('getByCode', () => {
    it('should return promotion by code', async () => {
      const result = await mockPromotionRepository.getByCode('SUMMER25')
      expect(result.isSuccess).toBe(true)
      expect(result.data?.code).toBe('SUMMER25')
    })

    it('should return error for non-existent code', async () => {
      const result = await mockPromotionRepository.getByCode('INVALID')
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(404)
    })
  })

  describe('apply', () => {
    it('should apply valid promotion', async () => {
      const result = await mockPromotionRepository.apply('SUMMER25')
      expect(result.isSuccess).toBe(true)
    })

    it('should fail for invalid promotion', async () => {
      const result = await mockPromotionRepository.apply('INVALID')
      expect(result.isFailure).toBe(true)
    })
  })

  describe('getHistory', () => {
    it('should return promotion history', async () => {
      const result = await mockPromotionRepository.getHistory()
      expect(result.isSuccess).toBe(true)
      expect(result.data).toHaveLength(2)
    })
  })
})