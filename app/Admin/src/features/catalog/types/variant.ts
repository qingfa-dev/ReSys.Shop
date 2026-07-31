export interface VariantParameters {
  sku: string
  position: number
  trackInventory: boolean
  weight?: number
  weightUnit?: string
  height?: number
  width?: number
  depth?: number
  dimensionsUnit?: string
  price?: number
  costPrice?: number
  costCurrency?: string
}

export interface VariantRequest extends VariantParameters {
  isMaster: boolean
  optionValueIds?: string[]
}

export interface Variant extends VariantParameters {
  id: string
  productId: string
  isMaster: boolean
  discontinuedOn?: string
  pricesCount: number
}

export interface VariantImage {
  id: string
  variantId: string
  url: string
  contentType: string
  fileName: string
  fileSize: number
  width?: number
  height?: number
  alt?: string
  position: number
  type: string
  createdAtUtc: string
}

export interface Price {
  id: string
  variantId: string
  amount?: number
  currency: string
  compareAtAmount?: number
  countryIso?: string
}

export interface OptionValueAssignment {
  optionValueId: string
  optionTypeId: string
  optionTypeName: string
  name: string
  presentation: string
  isAssigned: boolean
}

export const VARIANT_FILTER_FIELDS = [
  'sku',
  'position',
  'isMaster',
  'discontinuedOn',
]

export const VARIANT_SORT_FIELDS = [
  'sku',
  'position',
  'isMaster',
]
