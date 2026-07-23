import type { CreateOptionTypeForm, UpdateOptionTypeForm } from '../schemas'

export type CreateOptionTypeRequest = CreateOptionTypeForm
export type UpdateOptionTypeRequest = UpdateOptionTypeForm

export interface OptionTypeListParams {
  page?: number
  pageSize?: number
  search?: string
}
