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
