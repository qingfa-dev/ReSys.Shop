import type { StoreTaxonListItemResponse } from './taxon'

// StoreVariantStockInfo mirrors Module.Catalog StoreVariantStockInfo
export interface StoreVariantStockInfo {
  totalOnHand: number
  totalReserved: number
  totalAvailable: number
  backorderable: boolean
  locations: StoreStockLocationInfo[]
}

export interface StoreStockLocationInfo {
  stockLocationId: string
  stockLocationName: string | null
  countOnHand: number
  reservedCount: number
  availableCount: number
  backorderable: boolean
}

// Note: variantOptionValueId removed from this response — aligns with the
// backend StoreOptionValueListItemResponse change (VariantOptionValueId
// dropped). The SPA identifies option values by id + name only.
export interface StoreVariantOptionValueResponse {
  id: string
  name: string
  presentation: string | null
  position: number
  optionTypeId: string
  optionTypeName: string | null
}

export interface StoreProductImageResponse {
  id: string
  url: string
  alt: string | null
  position: number
}

export interface StoreVariantPriceResponse {
  id: string
  amount: number | null
  currency: string
  compareAtAmount: number | null
  countryIso: string | null
}

export interface StoreProductVariantResponse {
  id: string
  sku: string | null
  isMaster: boolean
  price: number | null
  currency: string | null
  optionValues: StoreVariantOptionValueResponse[]
  images: StoreProductImageResponse[]
  prices: StoreVariantPriceResponse[]
  stock: StoreVariantStockInfo
}

export interface StoreProductListItemResponse {
  id: string
  masterVariantId: string
  name: string
  status: string
  description: string | null
  slug: string
  styleCode: string | null
  seasonName: string | null
  materialComposition: string | null
  careInstructions: string | null
  fitNotes: string | null
  department: string | null
  genderTarget: string | null
  variantsCount: number
  availableOn: string | null
  masterVariant: StoreProductVariantResponse | null
  classifications: StoreTaxonListItemResponse[]
}

export interface StoreProductDetailResponse extends StoreProductListItemResponse {
  variants: StoreProductVariantResponse[]
}
