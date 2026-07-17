import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
export interface StockMovementQuery extends ServerQueryingParameters {
  stockItemId?: string; type?: number
}
