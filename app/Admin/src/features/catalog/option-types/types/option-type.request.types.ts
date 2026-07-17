import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { OptionTypeFormData } from '../schemas/option-type.schema'

export type CreateOptionTypeRequest = OptionTypeFormData
export type UpdateOptionTypeRequest = OptionTypeFormData
export type OptionTypeQuery = ServerQueryingParameters
