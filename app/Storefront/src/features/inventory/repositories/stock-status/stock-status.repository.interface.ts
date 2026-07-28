import type { Result } from '@/core/models/result'
import type { StockStatusResponse } from '../../types/response'

export interface IStockStatusRepository {
  getByProductId(productId: string): Promise<Result<StockStatusResponse>>
}