import type { Result } from '@/core/models/result'
import type { StoreLocationResponse } from '../../types/response'

export interface IStoreLocationRepository {
  getAll(geoLocation?: { latitude: number; longitude: number }): Promise<Result<StoreLocationResponse[]>>
  getById<T = StoreLocationResponse>(id: string): Promise<Result<T>>
  findNearest(latitude: number, longitude: number): Promise<Result<StoreLocationResponse>>
}