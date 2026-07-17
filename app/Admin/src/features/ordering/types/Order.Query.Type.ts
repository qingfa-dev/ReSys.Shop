import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export interface OrderQuery extends ServerQueryingParameters {
  state?: string; storeId?: string; warehouseId?: string
  fromDate?: string; toDate?: string
}
