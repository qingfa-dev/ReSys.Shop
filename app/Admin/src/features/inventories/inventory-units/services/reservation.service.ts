import { reservationRepository } from '../api/reservation.api'
import { mapInventoryUnit } from '../mappers/inventory-unit.mapper'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { InventoryUnit } from '../types/InventoryUnit.Response.Type'

export const reservationService = {
  async listReservations(params: ServerQueryingParameters): Promise<ServerPagedResult<InventoryUnit>> {
    const result = await reservationRepository.list(params)
    if (result.isSuccess) {
      return { ...result, items: result.items.map(mapInventoryUnit) }
    }
    return result as unknown as ServerPagedResult<InventoryUnit>
  },

  async getReservationDetail(id: string): Promise<ServerResult<InventoryUnit>> {
    const result = await reservationRepository.getById(id)
    if (result.isSuccess) {
      return { ...result, value: mapInventoryUnit(result.value) } as unknown as ServerResult<InventoryUnit>
    }
    return result as unknown as ServerResult<InventoryUnit>
  },

  async cancelReservation(id: string): Promise<ServerResult<void>> {
    return reservationRepository.cancel(id) as unknown as Promise<ServerResult<void>>
  },
}
