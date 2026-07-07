import type { ApiResult } from '@/shared/api/types/api.types';
import type { PaginationMeta } from '@/shared/api/types/result.types';
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types';
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

export type PropertyTypeQuery = ServerQueryingParameters

export type { ApiResult, PaginationMeta };
