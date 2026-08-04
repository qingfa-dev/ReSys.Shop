import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { StoreLocationResponse } from '../../types/response'
import type { IStoreLocationRepository } from './store-location.repository.interface'

export class StoreLocationApiRepository extends BaseRepository implements IStoreLocationRepository {
  async getAll(geoLocation?: { latitude: number; longitude: number }): Promise<Result<StoreLocationResponse[]>> {
    return this.get<StoreLocationResponse[]>('/locations/stores', geoLocation ? { filter: `lat:${geoLocation.latitude},lng:${geoLocation.longitude}` } : undefined)
  }

  async getById<T = StoreLocationResponse>(id: string): Promise<Result<T>> {
    return this.get<T>(`/locations/stores/${id}`)
  }

  async findNearest(latitude: number, longitude: number): Promise<Result<StoreLocationResponse>> {
    return this.get<StoreLocationResponse>('/locations/stores/nearest', { filter: `lat:${latitude},lng:${longitude}` })
  }
}

export const storeLocationApiRepository = new StoreLocationApiRepository()