export const LOCATIONS_ENDPOINTS = {
  COUNTRIES: '/api/storefront/locations/countries',
  COUNTRY: (id: string) => `/api/storefront/locations/countries/${id}`,
  COUNTRY_BY_ISO: (isoCode: string) => `/api/storefront/locations/countries/by-iso/${isoCode}`,
  STATES: '/api/storefront/locations/states',
  STATE: (id: string) => `/api/storefront/locations/states/${id}`,
  STATE_BY_ISO: (isoCode: string) => `/api/storefront/locations/states/by-iso/${isoCode}`,
} as const

export const ADDRESS_ENDPOINTS = {
  ADDRESSES: '/api/storefront/profiles/addresses',
  ADDRESS: (id: string) => `/api/storefront/profiles/addresses/${id}`,
} as const
