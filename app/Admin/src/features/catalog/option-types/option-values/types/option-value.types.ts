import type { ApiResult } from '@/shared/api/types/api.types';
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types';
import type { OptionValueFormData } from '../schemas/option-value.schema';

export interface OptionValueListItem {
  id: string;
  optionTypeId: string;
  name: string;
  presentation: string;
  position: number;
}

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

export type { ApiResult };