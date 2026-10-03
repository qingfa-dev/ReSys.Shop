import { stockStatusApiRepository } from '../../repositories/stock-status/stock-status.api'
import type { IStockStatusService } from './stock-status.service.interface'
import type { StockStatus } from '../../types'
import type { Result } from '@/core/models/result'
import { toStockStatus } from '../../mapping'
import { resultMap } from '@/core/utils/result-helpers'

export class StockStatusService implements IStockStatusService {
  private readonly stockStatusRepository = stockStatusApiRepository

  async getStockStatus(productId: string): Promise<Result<StockStatus>> {
    const response = await this.stockStatusRepository.getByProductId(productId)
    return resultMap(response, toStockStatus)
  }
}

export const stockStatusService = new StockStatusService()