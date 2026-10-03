import { describe, it, expect } from 'vitest'
import { mockShippingMethodRepository } from '../shipping-method.mock.repository'

describe('ShippingMethodRepository', () => {
  describe('getAll', () => {
    it('should return shipping methods', async () => {
      const result = await mockShippingMethodRepository.getAll()
      expect(result.isSuccess).toBe(true)
      expect(result.data?.length).toBe(3)
    })
  })

  describe('getById', () => {
    it('should return shipping method by id', async () => {
      const result = await mockShippingMethodRepository.getById('ship-1')
      expect(result.isSuccess).toBe(true)
    })
  })
})