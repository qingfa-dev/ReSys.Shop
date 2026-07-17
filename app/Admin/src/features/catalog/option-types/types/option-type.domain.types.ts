export interface OptionTypeListItem {
  id: string
  name: string
  presentation: string
  position: number
  filterable: boolean
  optionValuesCount: number
  productsCount: number
  createdAtUtc: string
  modifiedAtUtc: string
}

export interface OptionTypeDetail extends OptionTypeListItem {}
