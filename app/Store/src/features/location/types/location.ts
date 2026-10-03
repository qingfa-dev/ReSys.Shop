// Types mirror the storefront location DTOs exactly (camelCase JSON).
// Contracts pinned from Module.Location.Features.Storefront (service/Api):
// - GetStorefrontCountryPagedOrAll.Response — CountryListItemResponse (PagedResult envelope)
// - GetStorefrontStatePagedOrAll.Response — StateListResponse (PagedResult envelope)

// Contract: GET api/storefront/location/countries — CountryListItemResponse.
export interface Country {
  id: string
  name: string
  isoCode: string
  callingCode: string | null
  statesRequired: boolean
  isActive: boolean
}

// Contract: GET api/storefront/location/states — StateListResponse.
// countryId is the foreign key used by useLocationCascade to filter states for a selected country.
export interface State {
  id: string
  name: string
  abbreviation: string
  countryId: string
  isActive: boolean
  countryName: string | null
}
