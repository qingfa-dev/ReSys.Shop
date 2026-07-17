import apiClient from '@/shared/api/http/api.client'
import { INVENTORY } from '@/shared/api/constants'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { StockMovement } from '../types/StockMovement.Response.Type'

function path(sub?: string): string {
  return `${INVENTORY}/movements${sub ? `/${sub}` : ''}`
}

export const movementRepository = {
  list(params: ServerQueryingParameters): Promise<ServerPagedResult<StockMovement>> {
    return apiClient.get(path(), { params }).then(res => res.data as ServerPagedResult<StockMovement>)
  },
  getById(id: string): Promise<ServerResult<StockMovement>> {
    return apiClient.get(path(id)).then(res => res.data as ServerResult<StockMovement>)
  },
}
