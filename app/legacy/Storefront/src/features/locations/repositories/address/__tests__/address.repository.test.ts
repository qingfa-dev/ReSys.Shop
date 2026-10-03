import { describe, it, expect, beforeEach } from 'vitest'
import { mockAddressRepository, MockAddressRepository } from '../address.mock.repository'

import type { AddressResponse } from '../../../types/response'
import type { Result } from '@/core/models/result'

describe('AddressRepository', () => {
  beforeEach(() => {
    MockAddressRepository.reset()
  })

  describe('getAddresses', () => {
    it('should return all addresses', async () => {
      const result = await mockAddressRepository.getAddresses()
      expect(result.isSuccess).toBe(true)
      expect(result.data).toHaveLength(2)
    })
  })

  describe('getById', () => {
    it('should return address by id', async () => {
      const result = await mockAddressRepository.getById('addr-1')
      expect(result.isSuccess).toBe(true)
      expect(result.data?.id).toBe('addr-1')
    })

    it('should return error for non-existent id', async () => {
      const result = await mockAddressRepository.getById('invalid-id')
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(404)
    })

    it('should support generic type', async () => {
      const result = await mockAddressRepository.getById('addr-1')
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('getDefault', () => {
    it('should return default address', async () => {
      const result = await mockAddressRepository.getDefault()
      expect(result.isSuccess).toBe(true)
      expect(result.data?.isDefault).toBe(true)
    })

    it('should return error when no default exists', async () => {
      const addresses = (mockAddressRepository as unknown as { getAddresses: () => Promise<Result<AddressResponse[]>> }).getAddresses()
      const defaultAddr = (await addresses).data?.find((a: AddressResponse) => a.isDefault)
      if (defaultAddr) defaultAddr.isDefault = false
      const result = await mockAddressRepository.getDefault()
      expect(result.isFailure).toBe(true)
    })
  })

  describe('create', () => {
    it('should create new address with generated id', async () => {
      const newAddress = { firstName: 'Jane', lastName: 'Smith', address1: '789 Pine St', city: 'Chicago', state: 'IL', postalCode: '60601', country: 'US', phone: '+1555123456', isDefault: false }
      const result = await mockAddressRepository.create(newAddress)
      expect(result.isSuccess).toBe(true)
      expect(result.data?.id).toBeDefined()
    })
  })

  describe('update', () => {
    it('should update existing address', async () => {
      const result = await mockAddressRepository.update('addr-1', { city: 'Boston', state: 'MA' })
      expect(result.isSuccess).toBe(true)
      expect(result.data?.city).toBe('Boston')
    })

    it('should return error for non-existent address', async () => {
      const result = await mockAddressRepository.update('invalid-id', { city: 'Boston' })
      expect(result.isFailure).toBe(true)
    })
  })

  describe('setDefault', () => {
    it('should set address as default', async () => {
      const result = await mockAddressRepository.setDefault('addr-2')
      expect(result.isSuccess).toBe(true)
    })
  })
})