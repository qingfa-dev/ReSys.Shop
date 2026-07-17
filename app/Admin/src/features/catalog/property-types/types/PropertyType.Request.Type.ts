import type { PropertyTypeParameters } from '../schemas/PropertyType.Schema'

export type CreatePropertyTypeRequest = PropertyTypeParameters & {
  publicMetadata?: Record<string, unknown>
  privateMetadata?: Record<string, unknown>
}

export type UpdatePropertyTypeRequest = PropertyTypeParameters & {
  publicMetadata?: Record<string, unknown>
  privateMetadata?: Record<string, unknown>
}
