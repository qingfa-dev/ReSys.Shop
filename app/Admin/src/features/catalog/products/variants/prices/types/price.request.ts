export interface SetVariantPriceRequest {
  amount: number
  currency: string
}

export interface SyncVariantPricesRequest {
  prices: SetVariantPriceRequest[]
}
