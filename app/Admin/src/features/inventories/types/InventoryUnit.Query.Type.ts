import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
export interface InventoryUnitQuery extends ServerQueryingParameters {
  stockItemId?: string; orderId?: string; shipmentId?: string; state?: number
}
