import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { StockStatusResponse } from '../../types/response'
import type { IStockStatusRepository } from './stock-status.repository.interface'

export class StockStatusApiRepository extends BaseRepository implements IStockStatusRepository {
  async getByProductId(productId: string): Promise<Result<StockStatusResponse>> {
    return this.get<StockStatusResponse>(`/inventory/${productId}/stock-status`)
  }
}

export const stockStatusApiRepository = new StockStatusApiRepository()