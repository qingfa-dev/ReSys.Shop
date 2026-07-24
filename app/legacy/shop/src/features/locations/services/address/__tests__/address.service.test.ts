import { describe, it, expect } from 'vitest'
import { addressService } from '../address.service'

import type { Address, AddressSchemaType } from '../../../types'

describe('AddressService', () => {
  describe('getAddresses', () => {
    it('should return addresses', async () => {
      const result = await addressService.getAddresses()
      expect(result).toBeDefined()
    })
  })

  describe('getDefaultAddress', () => {
    it('should return default address', async () => {
      const result = await addressService.getDefaultAddress()
      expect(result).toBeDefined()
    })
  })

  describe('createAddress', () => {
    it('should create address', async () => {
      const result = await addressService.createAddress({ firstName: 'John', lastName: 'Doe', address1: '123 Main St', city: 'NYC', state: 'NY', postalCode: '10001', country: 'US' } as Omit<Address, 'id'>)
      expect(result).toBeDefined()
    })
  })

  describe('updateAddress', () => {
    it('should update address', async () => {
      const result = await addressService.updateAddress('addr-1', { firstName: 'Jane' } as Partial<Address>)
      expect(result).toBeDefined()
    })
  })

  describe('deleteAddress', () => {
    it('should delete address', async () => {
      const result = await addressService.deleteAddress('addr-1')
      expect(result).toBeDefined()
    })
  })

  describe('setDefaultAddress', () => {
    it('should set default address', async () => {
      const result = await addressService.setDefaultAddress('addr-1')
      expect(result).toBeDefined()
    })
  })
})