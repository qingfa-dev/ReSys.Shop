import { reservationRepository } from '../api/reservation.api'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { InventoryUnit } from '../types/inventory-unit.response.type'

export const reservationService = {
  async listReservations(params: ServerQueryingParameters): Promise<ServerPagedResult<InventoryUnit>> {
    return reservationRepository.list(params)
  },

  async getReservationDetail(id: string): Promise<ServerResult<InventoryUnit>> {
    return reservationRepository.getById(id)
  },

  async cancelReservation(id: string): Promise<ServerResult<void>> {
    return reservationRepository.cancel(id)
  },
}
