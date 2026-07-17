import apiClient from '@/shared/api/http/api.client'
import { INVENTORY } from '@/shared/api/constants'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { InventoryUnit } from '../types/InventoryUnit.Response.Type'

function path(sub?: string): string {
  return `${INVENTORY}/reservations${sub ? `/${sub}` : ''}`
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
