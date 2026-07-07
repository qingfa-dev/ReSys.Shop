import type { ApiResult } from '@/shared/api/types/api.types'
import type { PaginationMeta } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'
import type { OptionTypeFormData } from '../schemas/option-type.schema'

export interface OptionTypeListItem {
  id: string
  name: string
  presentation: string
  position: number
  filterable: boolean
  optionValuesCount: number
  productsCount: number
  createdAtUtc: string
  modifiedAtUtc: string
}

export interface OptionTypeDetail extends OptionTypeListItem {}

export type CreateOptionTypeRequest = OptionTypeFormData

export type UpdateOptionTypeRequest = OptionTypeFormData

export type OptionTypeQuery = ServerQueryingParameters

export type { ApiResult, PaginationMeta }
