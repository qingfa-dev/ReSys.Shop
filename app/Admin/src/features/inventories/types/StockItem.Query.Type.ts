import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
export interface StockItemQuery extends ServerQueryingParameters { lowStock?: boolean }
