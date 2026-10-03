import type { ApiResult } from '@/shared/api/api.types';
import type { OptionValueFormData } from '../schemas/option-value.schema';

export interface OptionValueListItem {
  id: string;
  option_type_id: string;
  name: string;
  presentation: string;
  position: number;
}

export type CreateOptionValueRequest = OptionValueFormData & {
  option_type_id: string;
};

export type UpdateOptionValueRequest = OptionValueFormData & {
  option_type_id?: string;
};

export interface UpdateOptionValuePositionsRequest {
  option_type_id: string;
  positions: { id: string; position: number }[];
}

export interface OptionValueQuery {
  page?: number;
  page_size?: number;
  sort?: string;
  option_type_id?: string | string[];
  search?: string;
  search_field?: string[];
  filter?: string;
}

export type { ApiResult };