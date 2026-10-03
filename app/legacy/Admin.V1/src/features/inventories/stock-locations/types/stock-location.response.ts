export interface StockLocation {
  id: string; name: string; code: string
  address: string; city: string; stateProvince: string
  postalCode: string; country: string
  isActive: boolean; isDefault: boolean
}

export interface StockLocationDetail extends StockLocation {
  createdAtUtc: string; modifiedAtUtc: string | null
}
