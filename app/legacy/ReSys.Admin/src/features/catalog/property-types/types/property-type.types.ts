import type { ApiResult, PaginationMeta } from '@/shared/api/api.types';
import type { PropertyTypeFormData } from '../schemas/property-type.schema';
import type { PropertyKind } from './property-kind';

export interface PropertyTypeListItem {
  id: string;
  name: string;
  presentation: string;
  kind: PropertyKind;
  position: number;
  filterable: boolean;
  publicMetadata?: Record<string, any>;
  privateMetadata?: Record<string, any>;
}

export type PropertyTypeDetail = PropertyTypeListItem;

export type CreatePropertyTypeRequest = PropertyTypeFormData & {
  publicMetadata?: Record<string, any>;
  privateMetadata?: Record<string, any>;
};

export type UpdatePropertyTypeRequest = PropertyTypeFormData & {
  publicMetadata?: Record<string, any>;
  privateMetadata?: Record<string, any>;
};

export interface PropertyTypeQuery {
  page?: number;
  page_size?: number;
  sort?: string;
  search?: string;
  search_field?: string[];
  filter?: string;
}

export type { ApiResult, PaginationMeta };
