export interface StoreProductListItemResponse {
  id: string
  masterVariantId: string
  name: string
  status: string
  description: string | null
  slug: string
  minPrice: number | null
  currency: string | null
  thumbnailUrl: string | null
  thumbnailAlt: string | null
  styleCode: string | null
  seasonName: string | null
  materialComposition: string | null
  careInstructions: string | null
  fitNotes: string | null
  department: string | null
  genderTarget: string | null
  variantsCount: number
  availableOn: string | null
}

export interface StoreProductVariantResponse {
  id: string
  sku: string | null
  isMaster: boolean
  price: number | null
  currency: string | null
  optionValue1: { id: string; name: string; presentation: string | null } | null
  optionValue2: { id: string; name: string; presentation: string | null } | null
  images: StoreProductImageResponse[]
}

export interface StoreProductImageResponse {
  id: string
  url: string
  alt: string | null
  position: number
}

export interface StoreProductDetailResponse extends StoreProductListItemResponse {
  masterVariant: StoreProductVariantResponse | null
  variants: StoreProductVariantResponse[]
  images: StoreProductImageResponse[]
  taxons: StoreProductTaxonResponse[]
}

export interface StoreProductTaxonResponse {
  id: string
  name: string
  permalink: string
  depth: number
}

export interface AvailabilityAxisValue {
  id: string
  name: string
  presentation: string | null
}

export interface AvailabilityCell {
  variantId: string
  optionValue1Id: string
  optionValue2Id: string | null
  status: string
  price: number | null
  currency: string | null
}

export interface AvailabilityMatrixResponse {
  axes: Array<{
    name: string
    presentation: string | null
    values: AvailabilityAxisValue[]
  }>
  cells: AvailabilityCell[]
}
