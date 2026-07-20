import apiClient from '@/common/api/http/api.client'
import { INVENTORY } from '@/common/api/constants'
import type { ServerResult, ServerPagedResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type { InventoryUnit } from '../types/inventory-unit.response.type'
function path(sub?: string): string {
  return `${INVENTORY}/stock-reservations${sub ? `/${sub}` : ''}`
}

export const reservationRepository = {
  list(params: ServerQueryingParameters): Promise<ServerPagedResult<InventoryUnit>> {
    return apiClient.get(path(), { params }).then(res => res.data as ServerPagedResult<InventoryUnit>)
  },
  getById(id: string): Promise<ServerResult<InventoryUnit>> {
    return apiClient.get(path(id)).then(res => res.data as ServerResult<InventoryUnit>)
  },
  cancel(id: string): Promise<ServerResult<void>> {
    return apiClient.post(path(`${id}/cancel`)).then(res => res.data as ServerResult<void>)
  },
}
