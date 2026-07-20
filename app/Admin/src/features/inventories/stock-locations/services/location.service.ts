import { locationRepository } from '../api/location.api'
import type { ServerResult, ServerPagedResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type { StockLocation, StockLocationDetail } from '../types/stock-location.response.type'
import type { CreateStockLocationRequest } from '../types/stock-location.request.type'

export const locationService = {
  async listLocations(params: ServerQueryingParameters): Promise<ServerPagedResult<StockLocation>> {
    return locationRepository.list(params)
  },

  async getLocationDetail(id: string): Promise<ServerResult<StockLocationDetail>> {
    return locationRepository.getById(id)
  },

  async createLocation(data: CreateStockLocationRequest): Promise<ServerResult<StockLocationDetail>> {
    return locationRepository.create(data)
  },

  async updateLocation(id: string, data: Partial<CreateStockLocationRequest>): Promise<ServerResult<StockLocationDetail>> {
    return locationRepository.update(id, data)
  },

  async deleteLocation(id: string): Promise<ServerResult<void>> {
    return locationRepository.delete(id)
  },

  async setDefaultLocation(id: string): Promise<ServerResult<void>> {
    return locationRepository.setDefault(id)
  },
}
