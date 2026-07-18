import { movementRepository } from '../api/movement.api'
import { mapStockMovement } from '../mappers/stock-movement.mapper'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { StockMovement } from '../types/StockMovement.Response.Type'

export const movementService = {
  async listMovements(params: ServerQueryingParameters): Promise<ServerPagedResult<StockMovement>> {
    const result = await movementRepository.list(params)
    if (result.isSuccess) {
      const mapped = result.items.map(mapStockMovement)
      return { ...result, items: mapped }
    }
    return result as unknown as ServerPagedResult<StockMovement>
  },

  async getMovementDetail(id: string): Promise<ServerResult<StockMovement>> {
    const result = await movementRepository.getById(id)
    if (result.isSuccess) {
      return { ...result, value: mapStockMovement(result.value) } as unknown as ServerResult<StockMovement>
    }
    return result as unknown as ServerResult<StockMovement>
  },
}
