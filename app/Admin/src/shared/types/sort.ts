export type SortDirection = 'asc' | 'desc'

export interface Sort<TField extends string = string> {
  field: TField
  direction: SortDirection
}
