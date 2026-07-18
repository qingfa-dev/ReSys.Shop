import { locationRepository } from '../api/location.api'
import { mapStockLocation } from '../mappers/stock-location.mapper'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { StockLocation, StockLocationDetail } from '../types/StockLocation.Response.Type'
import type { CreateStockLocationRequest } from '../types/StockLocation.Request.Type'

function applyMap<T, R>(data: T, mapper: (d: T) => R): R {
  return mapper(data) as R
}

function applyMapArray<T, R>(data: T[], mapper: (d: T) => R): R[] {
  return data.map(d => mapper(d) as R)
}

export const locationService = {
  async listLocations(params: ServerQueryingParameters): Promise<ServerPagedResult<StockLocation>> {
    const result = await locationRepository.list(params)
    return result.isSuccess ? { ...result, items: applyMapArray(result.items, mapStockLocation) } : result as unknown as ServerPagedResult<StockLocation>
  },

  async getLocationDetail(id: string): Promise<ServerResult<StockLocationDetail>> {
    const result = await locationRepository.getById(id)
    return result.isSuccess ? { ...result, value: applyMap(result.value, mapStockLocation) } as unknown as ServerResult<StockLocationDetail> : result as unknown as ServerResult<StockLocationDetail>
  },

  async createLocation(data: CreateStockLocationRequest): Promise<ServerResult<StockLocationDetail>> {
    const result = await locationRepository.create(data)
    return result.isSuccess ? { ...result, value: applyMap(result.value, mapStockLocation) } as unknown as ServerResult<StockLocationDetail> : result as unknown as ServerResult<StockLocationDetail>
  },

  async updateLocation(id: string, data: Partial<CreateStockLocationRequest>): Promise<ServerResult<StockLocationDetail>> {
    const result = await locationRepository.update(id, data)
    return result.isSuccess ? { ...result, value: applyMap(result.value, mapStockLocation) } as unknown as ServerResult<StockLocationDetail> : result as unknown as ServerResult<StockLocationDetail>
  },

  async deleteLocation(id: string): Promise<ServerResult<void>> {
    return locationRepository.delete(id) as unknown as Promise<ServerResult<void>>
  },

  async setDefaultLocation(id: string): Promise<ServerResult<void>> {
    return locationRepository.setDefault(id) as unknown as Promise<ServerResult<void>>
  },
}
