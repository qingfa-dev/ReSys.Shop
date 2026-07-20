import type { StoreLocationResponse } from '../../types/response'
import type { IStoreLocationRepository } from './store-location.repository.interface'
import type { Result } from '@/core/models/result'

const initialStoreLocations: StoreLocationResponse[] = [
  { id: 'store-1', name: 'New York Flagship', address: '123 5th Avenue, New York, NY 10001', phone: '+12125551000', hours: 'Mon-Sat 10AM-9PM, Sun 11AM-7PM', latitude: 40.7128, longitude: -74.006 },
  { id: 'store-2', name: 'Los Angeles', address: '456 Rodeo Drive, Los Angeles, CA 90001', phone: '+13105551000', hours: 'Mon-Sat 10AM-8PM, Sun 11AM-6PM', latitude: 34.0522, longitude: -118.2437 },
  { id: 'store-3', name: 'Chicago', address: '789 Michigan Ave, Chicago, IL 60601', phone: '+13125551000', hours: 'Mon-Sat 10AM-8PM, Sun 11AM-6PM', latitude: 41.8781, longitude: -87.6298 },
]

const mockStoreLocations: StoreLocationResponse[] = JSON.parse(JSON.stringify(initialStoreLocations))

export class MockStoreLocationRepository implements IStoreLocationRepository {
  static reset() {
    mockStoreLocations.length = 0
    initialStoreLocations.forEach(s => mockStoreLocations.push({ ...s }))
  }

  async getAll(_geoLocation?: { latitude: number; longitude: number }): Promise<Result<StoreLocationResponse[]>> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mockStoreLocations }
  }

  async getById<T = StoreLocationResponse>(id: string): Promise<Result<T>> {
    const store = mockStoreLocations.find(s => s.id === id)
    if (!store) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Store not found' }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: store as T }
  }

  async findNearest(_latitude: number, _longitude: number): Promise<Result<StoreLocationResponse>> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mockStoreLocations[0] }
  }
}

export const mockStoreLocationRepository = new MockStoreLocationRepository()