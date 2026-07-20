import { transferRepository } from '../api/transfer.api'
import type { ServerResult, ServerPagedResult } from '@/common/api/types/result.types'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'
import type { StockTransfer, StockTransferDetail } from '../types/stock-transfer.response.type'
import type { CreateStockTransferRequest } from '../types/stock-transfer.request.type'

export const transferService = {
  async listTransfers(params: ServerQueryingParameters): Promise<ServerPagedResult<StockTransfer>> {
    return transferRepository.list(params)
  },

  async getTransferDetail(id: string): Promise<ServerResult<StockTransferDetail>> {
    return transferRepository.getById(id)
  },

  async createTransfer(data: CreateStockTransferRequest): Promise<ServerResult<StockTransferDetail>> {
    return transferRepository.create(data)
  },

  async transferStock(id: string): Promise<ServerResult<void>> {
    return transferRepository.transfer(id)
  },

  async receiveTransfer(id: string): Promise<ServerResult<void>> {
    return transferRepository.receive(id)
  },

  async cancelTransfer(id: string): Promise<ServerResult<void>> {
    return transferRepository.cancel(id)
  },
}
