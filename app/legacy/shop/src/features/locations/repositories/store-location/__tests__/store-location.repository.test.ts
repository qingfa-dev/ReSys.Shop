import { describe, it, expect, beforeEach } from 'vitest'
import { mockStoreLocationRepository, MockStoreLocationRepository } from '../store-location.mock.repository'

describe('StoreLocationRepository', () => {
  beforeEach(() => {
    MockStoreLocationRepository.reset()
  })

  describe('getAll', () => {
    it('should return all store locations', async () => {
      const result = await mockStoreLocationRepository.getAll()
      expect(result.isSuccess).toBe(true)
      expect(result.data).toHaveLength(3)
    })

    it('should accept geoLocation parameter', async () => {
      const result = await mockStoreLocationRepository.getAll({ latitude: 40.7128, longitude: -74.006 })
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('getById', () => {
    it('should return store by id', async () => {
      const result = await mockStoreLocationRepository.getById('store-1')
      expect(result.isSuccess).toBe(true)
      expect(result.data?.id).toBe('store-1')
    })

    it('should return error for non-existent id', async () => {
      const result = await mockStoreLocationRepository.getById('invalid-id')
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(404)
    })

    it('should support generic type', async () => {
      const result = await mockStoreLocationRepository.getById('store-1')
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('findNearest', () => {
    it('should return nearest store', async () => {
      const result = await mockStoreLocationRepository.findNearest(40.7128, -74.006)
      expect(result.isSuccess).toBe(true)
      expect(result.data).toBeDefined()
    })
  })
})