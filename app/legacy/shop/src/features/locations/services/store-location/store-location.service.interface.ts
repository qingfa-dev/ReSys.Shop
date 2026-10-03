import type { Result } from '@/core/models/result'
import type { StoreLocation } from '../../types'

export interface IStoreLocationService {
  getStoreLocations(geoLocation?: { latitude: number; longitude: number }): Promise<Result<StoreLocation[]>>
  findNearestStore(latitude: number, longitude: number): Promise<Result<StoreLocation>>
}