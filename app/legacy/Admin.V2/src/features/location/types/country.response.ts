export interface CountryResponse {
  id: string
  name: string
  isoCode: string
  iso3Code?: string | null
  numericCode?: string | null
  phoneCode?: string | null
  isActive: boolean
  statesCount?: number
  createdAt: string
  updatedAt: string
}
