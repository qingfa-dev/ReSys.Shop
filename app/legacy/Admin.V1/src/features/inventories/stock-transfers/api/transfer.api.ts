import apiClient from '@/common/api/http/api.client'
import { INVENTORY } from '@/common/api/constants'
import type { ServerResult, ServerPagedResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type { StockTransfer, StockTransferDetail } from '../types/stock-transfer.response'
import type { CreateStockTransferRequest } from '../types/stock-transfer.request'
function path(sub?: string): string {
  return `${INVENTORY}/stock-transfers${sub ? `/${sub}` : ''}`
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
