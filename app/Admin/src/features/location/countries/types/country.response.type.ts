export interface Country {
  id: string
  name: string
  isoCode: string
  callingCode: string | null
  isActive: boolean
  statesRequired?: boolean
  createdAtUtc?: string
  modifiedAtUtc?: string
}
