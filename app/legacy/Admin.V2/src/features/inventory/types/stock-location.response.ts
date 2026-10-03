export interface StockLocationResponse {
  id: string
  name: string
  code: string
  address1?: string | null
  address2?: string | null
  city?: string | null
  state?: string | null
  postalCode?: string | null
  country?: string | null
  phone?: string | null
  isDefault: boolean
  isActive: boolean
  createdAt: string
  updatedAt: string
}
