import { transferRepository } from '../api/transfer.api'
import { mapStockTransfer } from '../mappers/stock-transfer.mapper'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { StockTransfer, StockTransferDetail } from '../types/StockTransfer.Response.Type'
import type { CreateStockTransferRequest } from '../types/StockTransfer.Request.Type'

function applyMap<T, R>(data: T, mapper: (d: T) => R): R {
  return mapper(data) as R
}

function applyMapArray<T, R>(data: T[], mapper: (d: T) => R): R[] {
  return data.map(d => mapper(d) as R)
}

export const transferService = {
  async listTransfers(params: ServerQueryingParameters): Promise<ServerPagedResult<StockTransfer>> {
    const result = await transferRepository.list(params)
    return result.isSuccess ? { ...result, items: applyMapArray(result.items, mapStockTransfer) } : result as unknown as ServerPagedResult<StockTransfer>
  },

  async getTransferDetail(id: string): Promise<ServerResult<StockTransferDetail>> {
    const result = await transferRepository.getById(id)
    return result.isSuccess ? { ...result, value: applyMap(result.value, mapStockTransfer) } as unknown as ServerResult<StockTransferDetail> : result as unknown as ServerResult<StockTransferDetail>
  },

  async createTransfer(data: CreateStockTransferRequest): Promise<ServerResult<StockTransferDetail>> {
    const result = await transferRepository.create(data)
    return result.isSuccess ? { ...result, value: applyMap(result.value, mapStockTransfer) } as unknown as ServerResult<StockTransferDetail> : result as unknown as ServerResult<StockTransferDetail>
  },

  async transferStock(id: string): Promise<ServerResult<void>> {
    return transferRepository.transfer(id) as unknown as Promise<ServerResult<void>>
  },

  async receiveTransfer(id: string): Promise<ServerResult<void>> {
    return transferRepository.receive(id) as unknown as Promise<ServerResult<void>>
  },

  async cancelTransfer(id: string): Promise<ServerResult<void>> {
    return transferRepository.cancel(id) as unknown as Promise<ServerResult<void>>
  },
}
