import { BaseRepository } from '@/core/repositories'
import type { PagedResult } from '@/core/models/result'
import type { StoreOptionTypeResponse } from '../../types/response'
import type { IOptionTypeRepository } from './option-type.repository.interface'

export class OptionTypeApiRepository extends BaseRepository implements IOptionTypeRepository {
  getFilterable(): Promise<PagedResult<StoreOptionTypeResponse>> {
    return super.getPaged<StoreOptionTypeResponse>('/api/storefront/option-types', { page: 1, pageSize: 100 })
  }
}

export const optionTypeApiRepository = new OptionTypeApiRepository()
