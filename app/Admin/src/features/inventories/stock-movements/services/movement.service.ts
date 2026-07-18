import { movementRepository } from '../api/movement.api'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { StockMovement } from '../types/stock-movement.response.type'

export const movementService = {
  async listMovements(params: ServerQueryingParameters): Promise<ServerPagedResult<StockMovement>> {
    return movementRepository.list(params)
  },

  async getMovementDetail(id: string): Promise<ServerResult<StockMovement>> {
    return movementRepository.getById(id)
  },
}
