import apiClient from '@/common/api/http/api.client'
import { INVENTORY } from '@/common/api/constants'
import type { ServerResult, ServerPagedResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type { StockMovement } from '../types/stock-movement.response.type'
function path(sub?: string): string {
  return `${INVENTORY}/stock-movements${sub ? `/${sub}` : ''}`
}

export const movementRepository = {
  list(params: ServerQueryingParameters): Promise<ServerPagedResult<StockMovement>> {
    return apiClient.get(path(), { params }).then(res => res.data as ServerPagedResult<StockMovement>)
  },
  getById(id: string): Promise<ServerResult<StockMovement>> {
    return apiClient.get(path(id)).then(res => res.data as ServerResult<StockMovement>)
  },
}
