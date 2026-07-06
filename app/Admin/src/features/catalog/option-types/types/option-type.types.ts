import type { ApiResult, PaginationMeta } from '@/shared/api/api.types'
import type { OptionTypeFormData } from '../schemas/option-type.schema'

export interface OptionTypeListItem {
  id: string
  name: string
  presentation: string
  description?: string
  position: number
  filterable: boolean
  publicMetadata?: Record<string, any>
  privateMetadata?: Record<string, any>
}

export interface OptionTypeDetail extends OptionTypeListItem {}

export type CreateOptionTypeRequest = OptionTypeFormData & {
  publicMetadata?: Record<string, any>
  privateMetadata?: Record<string, any>
}

export type UpdateOptionTypeRequest = OptionTypeFormData & {
  publicMetadata?: Record<string, any>
  privateMetadata?: Record<string, any>
}

export interface OptionTypeQuery {
  page?: number
  page_size?: number
  sort?: string
  search?: string
  search_field?: string[]
  filter?: string
}

export type { ApiResult, PaginationMeta }
