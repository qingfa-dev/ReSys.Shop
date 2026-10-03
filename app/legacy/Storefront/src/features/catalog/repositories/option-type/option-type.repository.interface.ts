import type { PagedResult } from '@/core/models/result'
import type { StoreOptionTypeResponse } from '../../types/response'

export interface IOptionTypeRepository {
  getFilterable(): Promise<PagedResult<StoreOptionTypeResponse>>
}
