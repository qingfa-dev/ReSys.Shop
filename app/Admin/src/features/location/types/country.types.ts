export interface Country {
  id: string
  name: string
  isoCode2: string
  isoCode3: string
  numericCode: string
  phoneCode: string
  isActive: boolean
}

export interface CountryCreateRequest {
  name: string
  isoCode2: string
  isoCode3: string
  numericCode: string
  phoneCode: string
  isActive: boolean
}

export type CountryUpdateRequest = CountryCreateRequest
