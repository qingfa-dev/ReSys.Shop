export interface VariantPriceResponse {
  id: string
  variantId: string
  amount?: number | null
  currency: string
  compareAtAmount?: number | null
  countryIso?: string | null
}

export interface SyncPricesResponse {
  added: number
  updated: number
  removed: number
}
