import { describe, it, expect } from 'vitest'
import { mockAddressRepository } from '../address.mock.repository'

describe('AddressRepository', () => {
  describe('getAll', () => {
    it('should return addresses', async () => {
      const result = await mockAddressRepository.getAll()
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('getById', () => {
    it('should return address by id', async () => {
      const result = await mockAddressRepository.getById('addr-1')
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('create', () => {
    it('should create address', async () => {
      const result = await mockAddressRepository.create({ firstName: 'Test', lastName: 'User', address1: '123 St', city: 'City', state: 'ST', postalCode: '12345', country: 'US' })
      expect(result.isSuccess).toBe(true)
    })
  })
})