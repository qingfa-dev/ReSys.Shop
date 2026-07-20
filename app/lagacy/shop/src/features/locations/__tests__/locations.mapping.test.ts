import { describe, it, expect } from 'vitest'
import {
  toAddress,
  fromAddress,
  toStoreLocation,
  fromStoreLocation,
  formatAddress,
  getFullName,
} from '../mapping/locations.mapping'
import { AddressSchema, StoreLocationSchema } from '../types/schemas'

describe('Locations Mapping', () => {
  describe('toAddress', () => {
    it('should convert schema to entity', () => {
      const schema = AddressSchema.parse({
        id: 'addr-1',
        firstName: 'John',
        lastName: 'Doe',
        address1: '123 Main St',
        city: 'NYC',
        state: 'NY',
        postalCode: '10001',
        country: 'US',
        isDefault: true,
      })
      const result = toAddress(schema)
      expect(result.firstName).toBe('John')
      expect(result.city).toBe('NYC')
    })
  })

  describe('formatAddress', () => {
    it('should format address correctly', () => {
      const address = {
        address1: '123 Main St',
        address2: 'Apt 4B',
        city: 'New York',
        state: 'NY',
        postalCode: '10001',
        country: 'US',
        firstName: 'John',
        lastName: 'Doe',
        id: '',
        isDefault: false,
      }
      const result = formatAddress(address)
      expect(result).toContain('123 Main St')
      expect(result).toContain('New York')
    })
  })

  describe('getFullName', () => {
    it('should return full name', () => {
      const address = { firstName: 'John', lastName: 'Doe', address1: '', city: '', state: '', postalCode: '', country: '', id: '', isDefault: false }
      expect(getFullName(address)).toBe('John Doe')
    })
  })

  describe('toStoreLocation', () => {
    it('should convert schema to entity', () => {
      const schema = StoreLocationSchema.parse({
        id: 'store-1',
        name: 'NYC Store',
        address: '123 5th Ave',
        phone: '+1234567890',
        hours: 'Mon-Fri 9-5',
        latitude: 40.7128,
        longitude: -74.006,
      })
      const result = toStoreLocation(schema)
      expect(result.name).toBe('NYC Store')
    })
  })
})