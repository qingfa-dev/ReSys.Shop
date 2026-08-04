import type { Result } from '@/core/models/result'
import type { StockStatus } from '../../types'

export interface IStockStatusService {
  getStockStatus(productId: string): Promise<Result<StockStatus>>
}