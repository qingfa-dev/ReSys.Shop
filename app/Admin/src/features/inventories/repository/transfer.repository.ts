import apiClient from '@/shared/api/http/api.client'
import { INVENTORY } from '@/shared/api/constants'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { StockTransfer, StockTransferDetail } from '../types/StockTransfer.Response.Type'
import type { CreateStockTransferRequest } from '../types/StockTransfer.Request.Type'

function path(sub?: string): string {
  return `${INVENTORY}/transfers${sub ? `/${sub}` : ''}`
}

export const transferRepository = {
  list(params: ServerQueryingParameters): Promise<ServerPagedResult<StockTransfer>> {
    return apiClient.get(path(), { params }).then(res => res.data as ServerPagedResult<StockTransfer>)
  },
  getById(id: string): Promise<ServerResult<StockTransferDetail>> {
    return apiClient.get(path(id)).then(res => res.data as ServerResult<StockTransferDetail>)
  },
  create(data: CreateStockTransferRequest): Promise<ServerResult<StockTransferDetail>> {
    return apiClient.post(path(), data).then(res => res.data as ServerResult<StockTransferDetail>)
  },
  transfer(id: string): Promise<ServerResult<void>> {
    return apiClient.post(path(`${id}/transfer`)).then(res => res.data as ServerResult<void>)
  },
  receive(id: string): Promise<ServerResult<void>> {
    return apiClient.post(path(`${id}/receive`)).then(res => res.data as ServerResult<void>)
  },
  cancel(id: string): Promise<ServerResult<void>> {
    return apiClient.post(path(`${id}/cancel`)).then(res => res.data as ServerResult<void>)
  },
}
