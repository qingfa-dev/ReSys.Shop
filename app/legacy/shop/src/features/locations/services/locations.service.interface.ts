import type { Result } from '@/core/models/result'
import type { Address, StoreLocation } from '../types'

export interface ILocationsService {
  getAddresses(): Promise<Result<Address[]>>
  getDefaultAddress(): Promise<Result<Address>>
  createAddress(address: Omit<Address, 'id'>): Promise<Result<Address>>
  updateAddress(id: string, address: Partial<Address>): Promise<Result<Address>>
  deleteAddress(id: string): Promise<Result<void>>
  setDefaultAddress(id: string): Promise<Result<void>>
  getStoreLocations(geoLocation?: { latitude: number; longitude: number }): Promise<Result<StoreLocation[]>>
  findNearestStore(latitude: number, longitude: number): Promise<Result<StoreLocation>>
}