import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { OptionValueFormData } from '../schemas/option-value.schema'

export type CreateOptionValueRequest = OptionValueFormData & {
  optionTypeId: string;
};

export type UpdateOptionValueRequest = OptionValueFormData & {
  optionTypeId?: string;
};

export interface UpdateOptionValuePositionsRequest {
  optionTypeId: string;
  positions: { id: string; position: number }[];
}

export interface OptionValueQuery extends ServerQueryingParameters {
  optionTypeId?: string
}
