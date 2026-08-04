import type { AddressResponse, AddressSingleResponse, AddressListResponse } from '../../types/response'

export class MockAddressRepository {
  private addresses: AddressResponse[] = [
    { id: 'addr-1', firstName: 'John', lastName: 'Doe', address1: '123 Main St', city: 'New York', stateProvince: 'NY', zipCode: '10001', countryName: 'United States', countryCode: 'US', phone: '555-1234', isDefault: true },
    { id: 'addr-2', firstName: 'Jane', lastName: 'Smith', address1: '456 Oak Ave', city: 'Los Angeles', stateProvince: 'CA', zipCode: '90001', countryName: 'United States', countryCode: 'US', phone: '555-5678', isDefault: false },
  ]

  async getAll(): Promise<AddressListResponse> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: this.addresses }
  }

  async getById(id: string): Promise<AddressSingleResponse> {
    const address = this.addresses.find(a => a.id === id)
    if (!address) return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Address not found' }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: address }
  }

  async create(address: Omit<AddressResponse, 'id'>): Promise<AddressSingleResponse> {
    const newAddress: AddressResponse = { ...address, id: `addr-${Date.now()}` }
    this.addresses.push(newAddress)
    return { isSuccess: true, isFailure: false, statusCode: 201, data: newAddress }
  }

  async update(id: string, updates: Partial<AddressResponse>): Promise<AddressSingleResponse> {
    const index = this.addresses.findIndex(a => a.id === id)
    if (index === -1) return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Address not found' }
    const updated: AddressResponse = { ...this.addresses[index]!, ...updates }
    this.addresses[index] = updated
    return { isSuccess: true, isFailure: false, statusCode: 200, data: updated }
  }

  async delete(id: string): Promise<{ isSuccess: boolean; isFailure: boolean; statusCode: number }> {
    const index = this.addresses.findIndex(a => a.id === id)
    if (index === -1) return { isSuccess: false, isFailure: true, statusCode: 404 }
    this.addresses.splice(index, 1)
    return { isSuccess: true, isFailure: false, statusCode: 204 }
  }

  async setDefault(id: string): Promise<AddressSingleResponse> {
    this.addresses = this.addresses.map(a => ({ ...a, isDefault: a.id === id }))
    const address = this.addresses.find(a => a.id === id)
    if (!address) return { isSuccess: false, isFailure: true, statusCode: 404 }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: address }
  }
}

export const mockAddressRepository = new MockAddressRepository()