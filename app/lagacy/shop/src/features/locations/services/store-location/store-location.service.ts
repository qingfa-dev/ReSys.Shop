import { storeLocationApiRepository } from '../../repositories/store-location/store-location.api'
import { mockStoreLocationRepository } from '../../repositories/store-location/store-location.mock.repository'
import type { IStoreLocationService } from './store-location.service.interface'
import type { StoreLocation } from '../../types'
import type { Result } from '@/core/models/result'
import { toStoreLocation } from '../../mapping'
import { resultMap } from '@/core/utils/result-helpers'

const USE_MOCK = true

export class StoreLocationService implements IStoreLocationService {
  private readonly storeLocationRepository = USE_MOCK ? mockStoreLocationRepository : storeLocationApiRepository

  async getStoreLocations(geoLocation?: { latitude: number; longitude: number }): Promise<Result<StoreLocation[]>> {
    const response = await this.storeLocationRepository.getAll(geoLocation)
    if (response.isFailure) {
      return response as unknown as Result<StoreLocation[]>
    }
    return resultMap(response, (data) => data.map(toStoreLocation))
  }

  async findNearestStore(latitude: number, longitude: number): Promise<Result<StoreLocation>> {
    const response = await this.storeLocationRepository.findNearest(latitude, longitude)
    return resultMap(response, toStoreLocation)
  }
}

export const storeLocationService = new StoreLocationService()