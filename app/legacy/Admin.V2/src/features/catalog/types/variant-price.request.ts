export interface VariantPriceRequest {
  amount?: number | null
  currency: string
  compareAtAmount?: number | null
  countryIso?: string | null
}

export interface SyncPriceItem {
  amount?: number | null
  currency: string
  compareAtAmount?: number | null
  countryIso?: string | null
}

export interface SyncPricesRequest {
  prices: SyncPriceItem[]
}
