import { describe, it, expect } from 'vitest'
import { storeLocationService } from '../store-location.service'

describe('StoreLocationService', () => {
  describe('getStoreLocations', () => {
    it('should return store locations', async () => {
      const result = await storeLocationService.getStoreLocations()
      expect(result).toBeDefined()
    })
  })

  describe('findNearestStore', () => {
    it('should find nearest store', async () => {
      const result = await storeLocationService.findNearestStore(40.7128, -74.0060)
      expect(result).toBeDefined()
    })
  })
})