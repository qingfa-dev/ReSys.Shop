import apiClient from '@/common/api/http/api.client'
import { INVENTORY } from '@/common/api/constants'
import type { ServerResult, ServerPagedResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type { StockLocation, StockLocationDetail } from '../types/stock-location.response'
import type { CreateStockLocationRequest, UpdateStockLocationRequest } from '../types/stock-location.request'
function path(sub?: string): string {
  return `${INVENTORY}/stock-locations${sub ? `/${sub}` : ''}`
}

export const locationRepository = {
  list(params?: ServerQueryingParameters): Promise<ServerPagedResult<StockLocation>> {
    return apiClient.get(path(), { params }).then(res => res.data as ServerPagedResult<StockLocation>)
  },
  getById(id: string): Promise<ServerResult<StockLocationDetail>> {
    return apiClient.get(path(id)).then(res => res.data as ServerResult<StockLocationDetail>)
  },
  create(data: CreateStockLocationRequest): Promise<ServerResult<StockLocationDetail>> {
    return apiClient.post(path(), data).then(res => res.data as ServerResult<StockLocationDetail>)
  },
  update(id: string, data: UpdateStockLocationRequest): Promise<ServerResult<StockLocationDetail>> {
    return apiClient.put(path(id), data).then(res => res.data as ServerResult<StockLocationDetail>)
  },
  delete(id: string): Promise<ServerResult<void>> {
    return apiClient.delete(path(id)).then(res => res.data as ServerResult<void>)
  },
  setDefault(id: string): Promise<ServerResult<void>> {
    return apiClient.post(path(`${id}/set-default`)).then(res => res.data as ServerResult<void>)
  },
}
