export const LOCATIONS_ENDPOINTS = {
  COUNTRIES: '/api/store/locations/countries',
  COUNTRY: (id: string) => `/api/store/locations/countries/${id}`,
  COUNTRY_BY_ISO: (isoCode: string) => `/api/store/locations/countries/by-iso/${isoCode}`,
  STATES: '/api/store/locations/states',
  STATE: (id: string) => `/api/store/locations/states/${id}`,
  STATE_BY_ISO: (isoCode: string) => `/api/store/locations/states/by-iso/${isoCode}`,
} as const

export const ADDRESS_ENDPOINTS = {
  ADDRESSES: '/api/store/profiles/addresses',
  ADDRESS: (id: string) => `/api/store/profiles/addresses/${id}`,
} as const
