import type { PropertyKind } from '../schemas/PropertyType.Schema'

export const PropertyKindOptions: { label: string; value: string }[] = [
  { label: 'String', value: 'String' },
  { label: 'Integer', value: 'Integer' },
  { label: 'Float', value: 'Float' },
  { label: 'Boolean', value: 'Boolean' },
  { label: 'Date', value: 'Date' },
  { label: 'HTML', value: 'Html' },
]

export interface PropertyTypeListItem {
  id: string
  name: string
  presentation: string
  kind: PropertyKind
  position: number
  filterable: boolean
  publicMetadata?: Record<string, unknown>
  privateMetadata?: Record<string, unknown>
}

export type PropertyTypeDetail = PropertyTypeListItem
