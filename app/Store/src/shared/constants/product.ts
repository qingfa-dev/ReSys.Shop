// Boundary: Product querying constants — mirrors backend AllowedSortFields
export const PRODUCT_SORT_FIELDS = [
  'Name',
  'CreatedAtUtc',
  'ModifiedAtUtc',
  'AvailableOn',
  'Price',
] as const

export const PRODUCT_SEARCH_FIELDS = [
  'Name',
  'Description',
  'Slug',
  'StyleCode',
  'SeasonName',
  'Department',
  'GenderTarget',
] as const

export const PRODUCT_FILTER_FIELDS = [
  'Status',
  'IsDeleted',
  'CreatedAtUtc',
  'AvailableOn',
  'StyleCode',
  'SeasonName',
  'Department',
] as const
