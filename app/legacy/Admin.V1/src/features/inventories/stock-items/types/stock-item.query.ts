import type { ServerQueryingParameters } from '@/common/api/types/query.types'
export interface StockItemQuery extends ServerQueryingParameters { lowStock?: boolean }
