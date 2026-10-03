import type { AddressResponse } from '../../types/response'
import type { IAddressRepository } from './address.repository.interface'
import type { Result } from '@/core/models/result'

const initialAddresses: AddressResponse[] = [
  { id: 'addr-1', firstName: 'John', lastName: 'Doe', address1: '123 Main Street', address2: 'Apt 4B', city: 'New York', state: 'NY', postalCode: '10001', country: 'US', phone: '+1234567890', isDefault: true },
  { id: 'addr-2', firstName: 'John', lastName: 'Doe', address1: '456 Oak Avenue', city: 'Los Angeles', state: 'CA', postalCode: '90001', country: 'US', phone: '+1987654321', isDefault: false },
]

const mockAddresses: AddressResponse[] = JSON.parse(JSON.stringify(initialAddresses))

export class MockAddressRepository implements IAddressRepository {
  static reset() {
    mockAddresses.length = 0
    initialAddresses.forEach(a => mockAddresses.push({ ...a }))
  }

  async getAddresses(): Promise<Result<AddressResponse[]>> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mockAddresses }
  }

  async getById<T = AddressResponse>(id: string): Promise<Result<T>> {
    const address = mockAddresses.find(a => a.id === id)
    if (!address) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Address not found' }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: address as T }
  }

  async getDefault(): Promise<Result<AddressResponse>> {
    const defaultAddr = mockAddresses.find(a => a.isDefault)
    if (!defaultAddr) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'No default address found' }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: defaultAddr }
  }

  async create(address: Omit<AddressResponse, 'id'>): Promise<Result<AddressResponse>> {
    const newAddress: AddressResponse = { ...address, id: `addr-${Date.now()}` }
    mockAddresses.push(newAddress)
    return { isSuccess: true, isFailure: false, statusCode: 201, data: newAddress }
  }

  async update(id: string, address: Partial<AddressResponse>): Promise<Result<AddressResponse>> {
    const index = mockAddresses.findIndex(a => a.id === id)
    if (index >= 0) {
      const existingAddress = mockAddresses[index]
      if (existingAddress) {
        const updated: AddressResponse = { ...existingAddress, ...address }
        mockAddresses[index] = updated
        return { isSuccess: true, isFailure: false, statusCode: 200, data: updated }
      }
    }
    return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Address not found' }
  }

  async setDefault(id: string): Promise<Result<void>> {
    mockAddresses.forEach(a => { a.isDefault = a.id === id })
    return { isSuccess: true, isFailure: false, statusCode: 200, data: undefined }
  }

  async delete(id: string): Promise<Result<void>> {
    const index = mockAddresses.findIndex(a => a.id === id)
    if (index >= 0) {
      mockAddresses.splice(index, 1)
      return { isSuccess: true, isFailure: false, statusCode: 200, data: undefined }
    }
    return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Address not found' }
  }
}

export const mockAddressRepository = new MockAddressRepository()