export interface Country {
  id: string
  name: string
  isoCode: string
  callingCode: string
  isActive: boolean
  statesRequired?: boolean
  zipcodeRequired?: boolean
  createdAtUtc?: string
  modifiedAtUtc?: string
}

export interface State {
  id: string
  name: string
  abbreviation: string
  countryId: string
  countryName?: string
  isActive: boolean
}
