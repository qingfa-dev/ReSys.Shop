import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { PropertyTypeFormData } from '../schemas/property-type.schema'

export type CreatePropertyTypeRequest = PropertyTypeFormData & {
  publicMetadata?: Record<string, any>;
  privateMetadata?: Record<string, any>;
};

export type UpdatePropertyTypeRequest = PropertyTypeFormData & {
  publicMetadata?: Record<string, any>;
  privateMetadata?: Record<string, any>;
};

export type PropertyTypeQuery = ServerQueryingParameters
